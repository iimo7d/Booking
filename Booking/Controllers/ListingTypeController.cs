using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Controllers
{
    public class ListingTypeController : Controller
    {
        private readonly Context _db;

        public ListingTypeController(Context db)
        {
            _db = db;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var types = await _db.ListingTypes
                .Include(l => l.Listings)
                .ToListAsync();
            return View(types);
        }

        // POST: /ListingType/Create (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create(ListingType listingType)
        {
            
                await _db.ListingTypes.AddAsync(listingType);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Listing type created successfully." });
          
        }

        // POST: /ListingType/Edit (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id, ListingType model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var listingType = await _db.ListingTypes.FindAsync(id);
            if (listingType == null)
            {
                return NotFound();
            }

           listingType.Name = model.Name;
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Listing type updated successfully." });
           
        }

        // POST: /ListingType/Delete (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int id)
        {
            var listingType = await _db.ListingTypes.FindAsync(id);
            if (listingType == null)
            {
                return Json(new { success = false, message = "Listing type not found." });
            }
            _db.ListingTypes.Remove(listingType);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Listing type deleted successfully." });
        }
    }
}
