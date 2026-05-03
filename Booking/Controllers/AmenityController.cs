using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Booking.Controllers
{
    public class AmenityController : Controller
    {
        private readonly Context _db;
        private readonly UserManager<AppUser> _userManager;

        public AmenityController(Context db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var amenities = await _db.Amenities
                .Include(a => a.RoomClassAmenities)
                    .ThenInclude(rca => rca.RoomClass)
                .ToListAsync();
            return View(amenities);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Amenity amenity)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid amenity data." });
            }
             
                _db.Amenities.Add(amenity);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Amenity created successfully." });
            
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var amenity = await _db.Amenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }
            return View(amenity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Amenity model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid amenity data." });
            }

            var amenity = await _db.Amenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound();
            }

                amenity.Name = model.Name;
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Amenity updated successfully." });
            
        }

        // POST: /Amenity/Delete (AJAX submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var amenity = await _db.Amenities.FindAsync(id);
            if (amenity == null || id == 0)
            {
                return NotFound();
            }

            _db.Amenities.Remove(amenity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Amenity deleted successfully." });
        }


        [HttpGet]
        public async Task<IActionResult> GetRoomClasses()
        {
            var roomClasses = await _db.RoomClasses
                .OrderBy(rc => rc.Name)
                .Select(rc => new { rc.Id, rc.Name })
                .ToListAsync();
            return Json(roomClasses);
        }
    }
}
