using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "Invoice")]
    public class Invoice : ViewComponent
    {
        private readonly Context db;

        public Invoice(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync() 
        {
            var totalRevenue = await db.Bookings.SumAsync(x=> ((int)x.TotalPrice));
            return View(totalRevenue);
               
        }

    }
}
