using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "Listing")]
    public class Listing : ViewComponent
    {
        private readonly Context db;

        public Listing(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var TotalListing = await db.Listings.Where(x => x.IsActive == true).CountAsync(); 

            return View(TotalListing); 
        }

    }
}
