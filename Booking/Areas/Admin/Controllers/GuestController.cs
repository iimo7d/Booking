using Booking.Data;
using Booking.Areas.Admin.ViewModels; // تأكد من أن هذا الـ namespace موجود
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GuestController : Controller
    {
        private readonly Context db;

        public GuestController(Context db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            var totalGuests = await db.Users.CountAsync();
            var totalPages = (int)Math.Ceiling(totalGuests / (double)pageSize);

            var guests = await db.Users
                .Include(u => u.City)
                .OrderByDescending(u => u.JoinDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new GuestViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    AvatarUrl = string.IsNullOrEmpty(u.AvatarUrl) ? "/default-avatar.png" : u.AvatarUrl,
                    JoinDate = u.JoinDate,
                    City = u.City != null ? u.City.Name : "N/A",
                    Country = u.City != null ? u.City.Country.Name : "N/A"
                })
                .ToListAsync();

            var model = new GuestListViewModel
            {
                Guests = guests,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }
        public async Task<IActionResult> Detail(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var guest = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => new GuestViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl ?? "/default-avatar.png",
                    BookingHistory = db.Bookings
    .Where(b => b.UserId == u.Id)
    .SelectMany(b => b.BookingRooms.Select(br => new BookingHistoryViewModel
    {
        RoomName = br.Room.Listing.Name,
        PricePerNight = br.Room.PricePerNight,
        BookDate = b.BookingDate,
        RoomNumber=br.Room.RoomNo,
        BookingStatus = br.Room.IsBooked ? "Booked" : "Available",
        RoomImage = br.Room.Images
            .OrderBy(img => img.Id)
            .Select(img => img.ImagePath)
            .FirstOrDefault()
    }))
    .ToList()


                })
                .FirstOrDefaultAsync();

            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }
    }
}
