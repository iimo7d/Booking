using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.ViewComponents.RandomHotels
{
    public class RandomHotels : ViewComponent
    {
        private readonly Context db;

        public RandomHotels(Context db)
        {
            this.db = db;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var randomListings = await db.Listings
               .Include(l => l.Reviews)
                .Include(l => l.Services)
                .Include(l => l.ListingType)
                .Include(l => l.ListingRoomClasses)
                    .ThenInclude(lrc => lrc.RoomClass)
                    .ThenInclude(rc => rc.Rooms)
                .Include(l => l.Images)
                .Include(l => l.City)
                .Where(l => l.IsActive == true &&
                            l.ListingRoomClasses.Any(lrc => lrc.RoomClass.Rooms.Any()))
             .Take(4)
             .ToListAsync();

            return View(randomListings);
        }
    }
}
