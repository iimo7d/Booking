using Booking.Data;
using Booking.Enums;
using Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Booking.Controllers
{
    public class ServiceController : Controller
    {
        private readonly Context _db;

        public ServiceController(Context db)
        {
            _db = db;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var services = await _db.Services
                .Include(s => s.ListingServices)
                .ToListAsync();
            ViewBag.ServiceTypes = new SelectList(Enum.GetValues(typeof(ServiceType)));
            return View(services);
        }

        // POST: /Service/Create (AJAX submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Service service)
        {
           
                await _db.Services.AddAsync(service);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Service created successfully." });
            ;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            ViewBag.ServiceTypes = new SelectList(Enum.GetValues(typeof(ServiceType)));
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Service model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var service = await _db.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            
                service.Description = model.Description;
                service.Type = model.Type;
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Service updated successfully." });
         
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null)
            {
                return Json(new { success = false, message = "Service not found." });
            }
            _db.Services.Remove(service);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Service deleted successfully." });
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
