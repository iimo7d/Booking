using Booking.Areas.Agent.Models;
using Booking.Data;
using Booking.Models;
using Booking.ViewModels.Listing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.Areas.Agent.Controllers
{
    [Area("Agent")]
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;

        public AgentController(Context context, UserManager<AppUser> userManager)
        {
            db = context;
            this.userManager = userManager;
        }


        public async Task<IActionResult> Dashboard()
        {
            var agent = await userManager.GetUserAsync(User);
            if (agent == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var listing = await db.Listings
                .Include(l => l.City)
                    .ThenInclude(c => c.Country)
                .Include(l => l.Images)
                .Include(l => l.ListingRoomClasses)
                    .ThenInclude(lrc => lrc.RoomClass)
                        .ThenInclude(rc => rc.RoomClassAmenities)
                            .ThenInclude(amenity => amenity.Amenity)
                .Include(l => l.ListingServices)
                    .ThenInclude(ls => ls.Service)
                .Include(l => l.ListingRoomClasses)
                    .ThenInclude(x => x.RoomClass)
                        .ThenInclude(r => r.Rooms)
                .Include(l => l.Reviews)
                    .ThenInclude(u => u.User)
                .Include(l => l.Agent)
                .FirstOrDefaultAsync(l => l.AgentId == agent.Id);

            if (listing == null)
            {
                return RedirectToAction("Home", "Error", new { area = "" });
            }

            var bookings = await db.Bookings
                .Include(b => b.User)
                .Include(l => l.Listing)
                .ThenInclude(a => a.Agent)
                .Where(b => b.ListingId == listing.Id && b.Listing.AgentId == agent.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            int totalRevenue = Convert.ToInt32(bookings.Sum(x => x.TotalPrice));
            int totalBookings = bookings.Count;
            int totalReviews = listing.Reviews.Count;
            int totalFavorites = await db.Favorites
                .Where(f => f.ListingId == listing.Id)
                .CountAsync();

            var model = new DashboardViewModel
            {
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                TotalReviews = totalReviews,
                TotalFavorites = totalFavorites,
                RecentBookings = bookings
            };

            ViewBag.Status = listing.IsActive;

            return View(model);
        }

    

        public async Task<IActionResult> Rooms()
        {
            var agent = await userManager.GetUserAsync(User);
            if (agent == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }



            var rooms = await db.Rooms
                .Include(r => r.Images)
                .Include(r => r.RoomClass)
                .Include(r => r.Listing)
                    .ThenInclude(l => l.Agent)
                .Where(r => r.Listing.AgentId == agent.Id)
                .ToListAsync();

            var listingId = await db.Listings
                .Where(l => l.AgentId == agent.Id)
                .Select(l => l.Id)
                .FirstOrDefaultAsync();

            var totalRooms = rooms.Count;
            var availableRooms = rooms.Count(r => !r.IsBooked);
            var bookedRooms = rooms.Count(r => r.IsBooked);


            var model = new AgentRoomVM
            {
                HotelId = listingId,
                FirstImagePaths = rooms
                    .Select(r => r.Images.FirstOrDefault()?.ImagePath)
                    .ToList(),
                TotalRooms = totalRooms,
                AvailableRooms = availableRooms,
                BookedRooms = bookedRooms,
                Rooms = rooms
            };

            return View(model);
        }

    }

}
