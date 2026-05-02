using Booking.Data;
using Booking.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "CityListComponent")]
    public class CityListComponent : ViewComponent
    {
        private readonly Context db;
        public CityListComponent(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cityCountries = await db.Cities
                .Include(c => c.Country)
                .Select(c => new CityCountryViewModel
                {
                    CityId = c.Id,
                    CityName = c.Name,
                    CountryId = c.Country.Id,
                    CountryName = c.Country.Name,
                    ImageUrl = string.IsNullOrEmpty(c.ImageUrl) ? "/default-city.png" : c.ImageUrl 
                })
                .OrderBy(c => c.CountryName)
                .ThenBy(c => c.CityName)
                .Take(6) 
                .ToListAsync();

            return View(cityCountries);
        }
    }
}
