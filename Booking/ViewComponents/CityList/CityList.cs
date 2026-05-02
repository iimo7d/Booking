using Booking.Data;
using Microsoft.AspNetCore.Mvc;

namespace Booking.ViewComponents.CityList
{
    public class CityList : ViewComponent
    {
        private readonly Context db;

        public CityList(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync() 
        {
            var Cities = db.Cities.OrderByDescending(v => v.VisitCount).Take(12).ToList();

            return View(Cities);
        }

    }
}
