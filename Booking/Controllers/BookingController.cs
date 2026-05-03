using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Booking.Services;
using Booking.Enums;
using Booking.Models;
using Booking.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Booking.Controllers
{
    public class BookingController : Controller
    {
        private const string RoomCartSessionKey = "RoomCart";

        private readonly Context db;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailSender emailSender;

        public BookingController(Context db, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            this.db = db;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }


        #region CART Methods


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoomToCart(int roomId)
        {
            var room = await db.Rooms.Include(r => r.Listing).FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
            {
                return Json(new { success = false, message = "Room not found." });
            }

            // Retrieve cart from session
            var cartRoomIds = HttpContext.Session.GetObjectFromJson<List<int>>(RoomCartSessionKey)
                              ?? new List<int>();

            // Ensure all rooms in the cart are from the same listing
            if (cartRoomIds.Any())
            {
                var firstRoom = await db.Rooms.FindAsync(cartRoomIds[0]);
                if (firstRoom != null && firstRoom.ListingId != room.ListingId)
                {
                    return Json(new { success = false, message = "Rooms in one booking must be from the same listing." });
                }
            }

            // Add if not already in cart
            if (!cartRoomIds.Contains(roomId))
            {
                cartRoomIds.Add(roomId);
                HttpContext.Session.SetObjectAsJson(RoomCartSessionKey, cartRoomIds);
            }

            return Json(new { success = true, message = "Room added to cart." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveRoomFromCart(int roomId)
        {
            var cartRoomIds = HttpContext.Session.GetObjectFromJson<List<int>>(RoomCartSessionKey)
                              ?? new List<int>();

            if (cartRoomIds.Contains(roomId))
            {
                cartRoomIds.Remove(roomId);
                HttpContext.Session.SetObjectAsJson(RoomCartSessionKey, cartRoomIds);
            }

            return Json(new { success = true, message = "Room removed from cart." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(RoomCartSessionKey);
            return RedirectToAction("ShowCart");
        }

        #endregion

        #region Booking / Checkout

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var model = await BuildCheckoutViewModelAsync(new BookingVM());
            if (!model.Rooms.Any())
            {
                ModelState.AddModelError("", "Your cart is empty. Please select rooms before checking out.");
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(BookingVM model)
        {
            if (model.CheckInDate.Date <= DateTime.Today)
            {
                ModelState.AddModelError("", "Check-in date must be after today.");
            }

            var nights = (model.CheckOutDate - model.CheckInDate).TotalDays;
            if (nights < 1)
            {
                ModelState.AddModelError("", "Check-out date must be after check-in date.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool sameCheckInExists = await db.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.CheckInDate.Date == model.CheckInDate.Date &&
                b.Status == BookingStatus.Pending);

            if (sameCheckInExists)
            {
                ModelState.AddModelError("", "You already have a booking with that check-in date.");
            }

            var cartRoomIds = HttpContext.Session.GetObjectFromJson<List<int>>(RoomCartSessionKey)
                            ?? new List<int>();
            if (!cartRoomIds.Any())
            {
                ModelState.AddModelError("", "Your cart is empty. Please select rooms first.");
            }

            var rooms = await db.Rooms
                .Include(r => r.Listing)
                    .ThenInclude(l => l.City)
                .Where(r => cartRoomIds.Contains(r.Id))
                .ToListAsync();

            if (rooms.Select(r => r.ListingId).Distinct().Count() > 1)
            {
                ModelState.AddModelError("", "Rooms in one booking must be from the same listing.");
            }

            var listingId = rooms.Select(r => r.ListingId).FirstOrDefault();

            bool overlap = listingId != 0 && await db.Bookings.AnyAsync(b =>
               b.ListingId == listingId
               && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
               && b.CheckInDate < model.CheckOutDate
               && model.CheckInDate < b.CheckOutDate
               && b.BookingRooms.Any(br => cartRoomIds.Contains(br.RoomId))
           );

            if (overlap)
            {
                ModelState.AddModelError("", "Some rooms in your cart are already booked for those dates.");
            }

            if (!ModelState.IsValid)
            {
                model = await BuildCheckoutViewModelAsync(model);
                return View(model);
            }

            decimal totalPrice = 0m;
            foreach (var room in rooms)
            {
                totalPrice += room.PricePerNight * (decimal)nights;
            }

            var booking = new Bookings
            {
                UserId = userId,
                ListingId = listingId,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                PaymentMethod = model.PaymentMethod,
                ConfirmationNo = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                BookingDate = DateTime.Now,
                Status = BookingStatus.Pending,
                TotalPrice = totalPrice
            };

            db.Bookings.Add(booking);
            await db.SaveChangesAsync();

            foreach (var roomId in cartRoomIds)
            {
                booking.BookingRooms.Add(new BookingRoom { BookingId = booking.Id, RoomId = roomId });
            }
            await db.SaveChangesAsync();

            var appUser = await db.Users.FindAsync(userId);
            if (appUser != null && !string.IsNullOrWhiteSpace(appUser.Email))
            {
                string body = $@"
<html>
  <head>
    <meta charset='UTF-8'>
    <title>Your Booking Confirmation</title>
    <style>
      body {{ font-family: Arial, sans-serif; }}
      .confirmation {{ font-size: 16px; margin-bottom: 20px; }}
      .details p {{ margin: 5px 0; }}
    </style>
  </head>
  <body>
    <h1>Your Booking Confirmation</h1>
    <div class='confirmation'>
      <p>Your booking is created with confirmation no: <strong>{booking.ConfirmationNo}</strong></p>
      <div class='details'>
        <p>Check-in: <strong>{booking.CheckInDate:d}</strong> / Check-out: <strong>{booking.CheckOutDate:d}</strong></p>
        <p>Total Price: <strong>{booking.TotalPrice:C}</strong></p>
      </div>
    </div>
  </body>
</html>";

                await emailSender.SendEmailAsync(appUser.Email, "Your Booking Confirmation", body);
            }

            HttpContext.Session.Remove(RoomCartSessionKey);

            TempData["BookingCreated"] = "Booking created successfully. Please confirm with your confirmation number.";
            return RedirectToAction("ConfirmBooking", new { bookingId = booking.Id });
        }


        #endregion

        #region ConfirmBooking (Updated to Mark Rooms, Create Invoice, Increment VisitCount)

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmBooking(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = await BuildConfirmBookingViewModelAsync(bookingId, userId);
            if (vm == null)
            {
                return NotFound("Booking not found or does not belong to you.");
            }

            TempData["Id"] = vm.BookingId;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ConfirmBooking(ConfirmBookingVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await db.Bookings
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
                .Include(b => b.Listing).ThenInclude(l => l.City)
                .Include(b => b.User)
                .Where(b => b.Id == model.BookingId && b.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                ModelState.AddModelError("", "Booking not found or does not belong to you.");
                var missingVm = await BuildConfirmBookingViewModelAsync(model.BookingId, user.Id);
                return View(missingVm ?? model);
            }

            if (!string.Equals(model.ConfirmationNo, booking.ConfirmationNo, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Invalid confirmation number. Please check your email and try again.");
                var invalidVm = await BuildConfirmBookingViewModelAsync(booking.Id, user.Id);
                if (invalidVm != null)
                {
                    invalidVm.ConfirmationNo = model.ConfirmationNo;
                    return View(invalidVm);
                }
                return View(model);
            }

            if (booking.Status != BookingStatus.Pending)
            {
                ModelState.AddModelError("", "This booking cannot be confirmed because it is not in Pending status.");
                var statusVm = await BuildConfirmBookingViewModelAsync(booking.Id, user.Id);
                if (statusVm != null)
                {
                    statusVm.ConfirmationNo = model.ConfirmationNo;
                    return View(statusVm);
                }
                return View(model);
            }

            booking.Status = BookingStatus.Confirmed;
            db.Bookings.Update(booking);

            foreach (var br in booking.BookingRooms)
            {
                var room = br.Room;
                room.BookedCount += 1;
                db.Rooms.Update(room);
            }

            if (booking.Listing?.City != null)
            {
                booking.Listing.City.VisitCount += 1;
                db.Cities.Update(booking.Listing.City);
            }

            await db.SaveChangesAsync();

            string emailBody = $@"
<html>
  <head>
    <meta charset='UTF-8'>
    <title>Booking Confirmation</title>
    <style>
      body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
      .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; padding: 20px; border: 1px solid #dddddd; }}
      .header {{ background-color: #007bff; color: #ffffff; padding: 15px; text-align: center; }}
      .content {{ padding: 20px; }}
      .footer {{ font-size: 12px; color: #777777; text-align: center; padding: 10px; border-top: 1px solid #dddddd; margin-top: 20px; }}
      ul {{ list-style-type: none; padding: 0; }}
      li {{ margin-bottom: 8px; }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='header'>
        <h1>Booking Confirmed!</h1>
      </div>
      <div class='content'>
        <p>Dear {user.FirstName ?? "Valued Customer"},</p>
        <p>Your booking has been successfully confirmed with the following details:</p>
        <ul>
          <li><strong>Confirmation No:</strong> {booking.ConfirmationNo}</li>
          <li><strong>Check-in:</strong> {booking.CheckInDate:d}</li>
          <li><strong>Check-out:</strong> {booking.CheckOutDate:d}</li>
          <li><strong>Total Price:</strong> {booking.TotalPrice:C}</li>
        </ul>
        <p>Thank you for choosing our service. We look forward to hosting you.</p>
      </div>
      <div class='footer'>
        <p>&copy; {DateTime.Now.Year} Booking Services. All rights reserved.</p>
      </div>
    </div>
  </body>
</html>";

            await emailSender.SendEmailAsync(user.Email, "Your Booking Confirmation", emailBody);

            TempData["ConfirmationSuccess"] = "Booking confirmed successfully.";
            return RedirectToAction(nameof(MyBookings));
        }

        #endregion

        #region CancelBooking

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await db.Bookings
                .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
            {
                return NotFound("Booking not found or does not belong to you.");
            }

            var hoursToCheckIn = (booking.CheckInDate - DateTime.Now).TotalHours;
            if (hoursToCheckIn < 24)
            {
                return BadRequest("Cannot cancel within 24 hours of check-in.");
            }

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                return BadRequest("Booking is not eligible for cancellation.");
            }

            booking.Status = BookingStatus.Cancelled;
            db.Bookings.Update(booking);
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }

        #endregion



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Using the built-in GetUserId helper
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await db.Bookings
                .Include(b => b.Listing)
                    .ThenInclude(l => l.City)
                .Include(b => b.Listing)
                    .ThenInclude(l => l.Images)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var upcoming = bookings
                .Where(b => b.Status != BookingStatus.Cancelled &&
                            b.Status != BookingStatus.Completed &&
                            b.CheckInDate >= DateTime.Now)
                .ToList();

            var cancelled = bookings
                .Where(b => b.Status == BookingStatus.Cancelled)
                .ToList();

            var completed = bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .ToList();

            var viewModel = new MyBooking
            {
                UpcomingBookings = upcoming,
                CancelledBookings = cancelled,
                CompletedBookings = completed
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> ShowCart()
        {
            var cartRoomIds = HttpContext.Session.GetObjectFromJson<List<int>>(RoomCartSessionKey) ?? new List<int>();
            var rooms = await db.Rooms
                .Include(r => r.Listing)
                .Where(r => cartRoomIds.Contains(r.Id))
                .ToListAsync();

            return View(rooms);
        }

        private async Task<BookingVM> BuildCheckoutViewModelAsync(BookingVM model)
        {
            var cartRoomIds = HttpContext.Session.GetObjectFromJson<List<int>>(RoomCartSessionKey)
                              ?? new List<int>();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                model.AppUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }

            if (cartRoomIds.Any())
            {
                var firstRoom = await db.Rooms
                    .Include(r => r.Listing)
                        .ThenInclude(l => l.City)
                        .ThenInclude(c => c.Country)
                    .Include(r => r.Listing)
                        .ThenInclude(l => l.Images)
                    .FirstOrDefaultAsync(r => cartRoomIds.Contains(r.Id));

                if (firstRoom != null)
                {
                    ViewBag.ListingId = firstRoom.ListingId;
                    ViewBag.HotelName = firstRoom.Listing.Name;
                    ViewBag.Address = $"{firstRoom.Listing.Street},{firstRoom.Listing.City.Name} - {firstRoom.Listing.City.Country.Name}";
                    ViewBag.Rate = firstRoom.Listing.Rating;
                    ViewBag.ImageUrl = firstRoom.Listing.Images.FirstOrDefault()?.ImagePath;
                    ViewBag.RoomCount = cartRoomIds.Count;
                    ViewBag.Date = DateTime.Now.Date;
                    ViewBag.Desciprtion = firstRoom.Listing.Description;
                }

                model.Rooms = await db.Rooms
                    .Include(r => r.Images)
                    .Include(r => r.RoomClass)
                        .ThenInclude(rc => rc.RoomClassAmenities)
                            .ThenInclude(rca => rca.Amenity)
                    .Where(r => cartRoomIds.Contains(r.Id))
                    .ToListAsync();
            }
            else
            {
                model.Rooms = new List<Room>();
            }

            return model;
        }

        private async Task<ConfirmBookingVM?> BuildConfirmBookingViewModelAsync(int bookingId, string userId)
        {
            var booking = await db.Bookings
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .ThenInclude(r => r.RoomClass)
                .ThenInclude(rc => rc.RoomClassAmenities)
                .ThenInclude(rca => rca.Amenity)
                .Include(b => b.BookingRooms)
                .ThenInclude(r => r.Room)
                .ThenInclude(r => r.Images)
                .Include(b => b.Listing)
                .ThenInclude(l => l.City)
                .ThenInclude(c => c.Country)
                .Include(b => b.Listing)
                .ThenInclude(l => l.Images)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
            {
                return null;
            }

            var nights = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
            return new ConfirmBookingVM
            {
                BookingId = booking.Id,
                ListingId = booking.ListingId,
                TotalRooms = booking.BookingRooms.Count,
                TotalNight = (int)nights,
                TotalPrice = (int)booking.TotalPrice,
                BookingDate = booking.BookingDate,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                UserName = $"{booking.User.FirstName} {booking.User.LastName}",
                paymentMethod = booking.PaymentMethod,
                HotelName = booking.Listing.Name,
                Address = $"{booking.Listing.Street}, {booking.Listing.City.Name} - {booking.Listing.City.Country.Name}",
                Rating = booking.Listing.Rating,
                Image = booking.Listing.Images.FirstOrDefault()?.ImagePath ?? string.Empty,
                Rooms = booking.BookingRooms.Select(br => br.Room).ToList(),
                Email = booking.User.Email ?? string.Empty
            };
        }

        public async Task<IActionResult> GetCartDropdownPartial()
        {
            var cartRoomIds = HttpContext.Session.GetObjectFromJson<List<int>>("RoomCart") ?? new List<int>();
            var roomsInCart = await db.Rooms
                .Include(r => r.Listing)
                .Where(r => cartRoomIds.Contains(r.Id))
                .ToListAsync();
            return PartialView("_CartDropdownPartial", roomsInCart);
        }

    }
}
