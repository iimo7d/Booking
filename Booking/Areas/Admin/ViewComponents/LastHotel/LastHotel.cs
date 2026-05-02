using Booking.Areas.Admin.ViewModels;
using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "LastHotel")]
    public class LatestHotelsViewComponent : ViewComponent
    {
        private readonly Context db;

        public LatestHotelsViewComponent(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var latestHotels = await db.Listings
                .Include(l => l.City)
                .ThenInclude(c => c.Country)
                .Include(l => l.Images) 
                .Include(l => l.ListingRoomClasses) 
                .ThenInclude(lrc => lrc.RoomClass)
                .ThenInclude(rc => rc.Rooms) 
                .ThenInclude(r => r.Images) 
                .OrderByDescending(l => l.CreatedAt)
                .Where(l => l.IsActive == true)
                .Take(4)
                .Select(l => new LatestHotelViewModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    Street = l.Street,
                    CountryName = l.City.Country.Name,
                    ImagePath = l.Images.Any()
                        ? l.Images.OrderBy(img => img.Id).Select(img => img.ImagePath).FirstOrDefault()
                        : l.ListingRoomClasses.SelectMany(lrc => lrc.RoomClass.Rooms)
                                              .SelectMany(r => r.Images)
                                              .OrderBy(img => img.Id)
                                              .Select(img => img.ImagePath)
                                              .FirstOrDefault()
                          ?? "https://via.placeholder.com/150" 
                })
                .ToListAsync();

            return View(latestHotels);
        }





    }
}
