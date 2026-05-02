using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.ViewComponents.CityList
{
    [ViewComponent(Name = "Room")]
    public class Room : ViewComponent
    {
        private readonly Context db;

        public Room(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync() 
        {
            var TotalRooms =await db.Rooms.CountAsync();

            return View(TotalRooms);
        }

    }
}
