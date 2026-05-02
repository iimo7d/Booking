using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Booking.Controllers
{
    public class CountryController : Controller
    {
        private readonly Context _db;

        public CountryController(Context db)
        {
            _db = db;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var countries = await _db.Countries
                .Include(c => c.Cities)
                .ToListAsync();
            return View(countries);
        }

        // POST: /Country/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Country country)
        {

                _db.Countries.Add(country);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Country created successfully." });
            
        }

        // POST: /Country/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Country model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var country = await _db.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            
                country.Name = model.Name;
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Country updated successfully." });
            
        }

        // POST: /Country/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var country = await _db.Countries.FindAsync(id);
            if (country == null || id == 0)
                return NotFound();

            _db.Countries.Remove(country);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Country deleted successfully." });
        }
    }
}
