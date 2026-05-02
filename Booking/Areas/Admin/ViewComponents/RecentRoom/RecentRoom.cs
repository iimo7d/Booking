using Booking.Areas.Admin.ViewModels;
using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "RecentRoom")]
    public class RecentRoom : ViewComponent
    {
        private readonly Context db;

        public RecentRoom(Context db)
        {
            this.db = db;
        }


       
     

    public async Task<IViewComponentResult> InvokeAsync()
        {
            var latestRooms = await db.Rooms
                .Include(r => r.Listing) 
                .ThenInclude(l => l.City) 
                .ThenInclude(c => c.Country) 
                .Include(r => r.Images)
                .Include(r => r.BookingRooms) 
                .ThenInclude(br => br.Booking) 
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentRoomViewModel
                {
                    RoomId = r.Id,
                    RoomName = r.RoomNo, 
                    HotelName = r.Listing.Name, 
                    City = r.Listing.City.Name, 
                    Country = r.Listing.City.Country.Name, 
                    ImagePath = r.Images.OrderBy(img => img.Id).Select(img => img.ImagePath).FirstOrDefault(), 
                    BookingStatus = r.IsBooked ? "Booked" : "Available",
                    BookingDate = r.BookingRooms
                                   .OrderByDescending(br => br.Booking.BookingDate)
                                   .Select(br => (DateTime?)br.Booking.BookingDate)
                                   .FirstOrDefault() 
                })
                .ToListAsync();

            return View(latestRooms);
        }
    }
}
