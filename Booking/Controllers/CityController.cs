using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Booking.Controllers
{
    public class CityController : Controller
    {
        private readonly Context _db;
        private readonly IWebHostEnvironment _env;

        public CityController(Context db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var cities = await _db.Cities
                .Include(c => c.Country)
                .ToListAsync();

            var countries = await _db.Countries
                .Select(c => new { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            ViewBag.Countries = new SelectList(countries, "Value", "Text");

            return View(cities);
        }

        // POST: /City/Create (AJAX submission)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(City city, IFormFile imageFile)
        {
          
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Define the upload folder (wwwroot/uploads/cities)
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique filename and save the file
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    // Save the relative path
                    city.ImageUrl = Path.Combine("uploads", "cities", uniqueFileName).Replace("\\", "/");
                }

                _db.Cities.Add(city);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "City created successfully." });
            
        }

        // POST: /City/Edit (AJAX submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, City model, IFormFile imageFile)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var city = await _db.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            
                // Update fields
                city.Name = model.Name;
                city.CountryId = model.CountryId;

                // If a new image is uploaded, delete the old file and upload the new one.
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(city.ImageUrl))
                    {
                        var existingPath = Path.Combine(_env.WebRootPath, city.ImageUrl);
                        if (System.IO.File.Exists(existingPath))
                        {
                            System.IO.File.Delete(existingPath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    city.ImageUrl = Path.Combine("uploads", "cities", uniqueFileName).Replace("\\", "/");
                }

                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "City updated successfully." });
          
        }

        // POST: /City/Delete (AJAX submission)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var city = await _db.Cities.FindAsync(id);
            if (city == null)
            {
                return Json(new { success = false, message = "City not found." });
            }

            // Delete the image file if it exists.
            if (!string.IsNullOrEmpty(city.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, city.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _db.Cities.Remove(city);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "City deleted successfully." });
        }
    }
}
