using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.ViewComponents.CityList
{
    [ViewComponent(Name = "BookingRoom")]
    public class BookingRoom : ViewComponent
    {
        private readonly Context db;

        public BookingRoom(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync() 
        {
            var BookingRooms =await  db.BookingRooms.CountAsync();

            return View(BookingRooms);
        }

    }
}
