using Booking.Data;
using Booking.Models;
using Booking.ViewModels.Listing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Booking.Controllers
{
    public class ListingController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment env;
        private readonly IEmailSender EmailService;

        public ListingController(Context db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env, IEmailSender emailService)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.env = env;
            EmailService = emailService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(
    string? hotelName,
    decimal? minPrice,
    decimal? maxPrice,
    double[]? customerRatings,
    int[]? starRatings,
    int? listingTypeId,
    int[]? roomClassIds,
    int? cityId,
    int page = 1
)
        {
            int pageSize = 6;

            var query = db.Listings
                .Include(l => l.Reviews)
                .Include(l => l.Services)
                .Include(l => l.ListingType)
                .Include(l => l.ListingRoomClasses)
                    .ThenInclude(lrc => lrc.RoomClass)
                    .ThenInclude(rc => rc.Rooms)
                .Include(l => l.Images)
                .Include(l => l.City)
                .Where(l => l.IsActive == true &&
                            l.ListingRoomClasses.FirstOrDefault().RoomClass.Rooms.Count()>0)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(hotelName))
            {
                query = query.Where(l => l.Name.Contains(hotelName));
            }

            if (cityId.HasValue)
            {
                query = query.Where(l => l.CityId == cityId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(l => l.StarterPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(l => l.StarterPrice <= maxPrice.Value);
            }

            if (customerRatings != null && customerRatings.Any())
            {
                double minCustomerRating = customerRatings.Min();
                query = query.Where(l => l.Reviews.Any() && l.Reviews.Average(r => r.Rate) >= minCustomerRating);
            }

            if (starRatings != null && starRatings.Any())
            {
                query = query.Where(l => starRatings.Contains(l.Rating));
            }

            if (listingTypeId.HasValue)
            {
                query = query.Where(l => l.ListingTypeId == listingTypeId.Value);
            }

            // Apply filter if one or more room classes are selected.
            if (roomClassIds != null && roomClassIds.Any())
            {
                query = query.Where(l => l.ListingRoomClasses.Any(lrc => roomClassIds.Contains(lrc.RoomClassId)));
            }

            // Get total count for pagination.
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination.
            var results = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass pagination info to the view.
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Populate select lists for Listing Types and Room Classes.
            ViewBag.ListingTypes = new SelectList(
                await db.ListingTypes.OrderBy(lt => lt.Name).ToListAsync(), "Id", "Name"
            );
            ViewBag.RoomClasses = new MultiSelectList(
                await db.RoomClasses.OrderBy(rc => rc.Name).ToListAsync(), "Id", "Name"
            );

            return View(results);
        }




        [AllowAnonymous]

        public async Task<IActionResult> Details(int id)
        {
            var listing = await db.Listings
                .Include(l => l.City)
                    .ThenInclude(c => c.Country)
                .Include(l => l.Images)
                .Include(l => l.ListingRoomClasses)
                    .ThenInclude(lrc => lrc.RoomClass)
                        .ThenInclude(rc => rc.RoomClassAmenities)
                            .ThenInclude(amenity => amenity.Amenity)
                .Include(l => l.ListingServices)
                    .ThenInclude(ls => ls.Service)
                .Include(l => l.ListingRoomClasses)
                    .ThenInclude(x => x.RoomClass)
                    .ThenInclude(r => r.Rooms)
                .Include(l => l.Reviews)
                    .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            var cheapestRoom = await db.Rooms
                .Where(r => r.ListingId == id)
                .OrderBy(r => r.PricePerNight)
                .FirstOrDefaultAsync();

            if (cheapestRoom != null)
            {
                ViewBag.StarterPrice = cheapestRoom.PricePerNight;
            }
            else
            {
                ViewBag.StarterPrice = "N/A";
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isFavorite = false;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isFavorite = await db.Favorites.AnyAsync(f => f.ListingId == id && f.UserId == currentUserId);
            }

            var rooms = await db.Rooms
                .Include(i => i.Images)
                .Include(r => r.RoomClass)
                .ThenInclude(r => r.RoomClassAmenities)
                .ThenInclude(r => r.Amenity)
                .Where(r => r.ListingId == id).ToListAsync();

            var vm = new DetailsVM
            {
                Listing = listing,
                IsFavorite = isFavorite,
                Rooms = rooms  
            };

            return View(vm);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ListingVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return View(model);
            }

            // Only Admins can create a listing.
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                return View(model);
            }

            PopulateViewData();

            var listing = new Listing
            {
                Name = model.Name,
                Description = model.Description,
                CityId = model.CityId,
                Street = model.Street,
                TotalRoom = model.TotalRoom,
                TotalFloor = model.TotalFloor,
                PhoneNo = model.PhoneNo,
                Rating = model.Rating,
                ListingTypeId = model.ListingTypeId,
                AgentId = user.Id,
                StarterPrice = model.StarterPrice,
                AvgRoomSize = model.AvgRoomSize,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Listings.Add(listing);
            await db.SaveChangesAsync();

            // Process files submitted via the standard form (if any)
            if (model.ListingImages != null && model.ListingImages.Any())
            {
                foreach (var file in model.ListingImages)
                {
                    var filePath = await UploadFile(file, "ListingImages");
                    db.ListingImages.Add(new ListingImage
                    {
                        ListingId = listing.Id,
                        ImagePath = filePath
                    });
                }
            }

            // Process uploaded image paths from Dropzone hidden inputs
            if (model.UploadedImagePaths != null && model.UploadedImagePaths.Any())
            {
                foreach (var filePath in model.UploadedImagePaths)
                {
                    db.ListingImages.Add(new ListingImage
                    {
                        ListingId = listing.Id,
                        ImagePath = filePath
                    });
                }
            }





            // Save selected services.
            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    db.ListingServices.Add(new ListingService
                    {
                        ListingId = listing.Id,
                        ServiceId = serviceId
                    });
                }
            }

            // Save selected room classes.
            if (model.SelectedRoomClassIds != null && model.SelectedRoomClassIds.Any())
            {
                foreach (var roomClassId in model.SelectedRoomClassIds)
                {
                    db.ListingRoomClasses.Add(new ListingRoomClass
                    {
                        ListingId = listing.Id,
                        RoomClassId = roomClassId
                    });
                }
            }

            await db.SaveChangesAsync();

            // Redirect to the Assign Amenity page, passing the created listing's Id.
            return RedirectToAction("MultiAssignAmenity", "RoomClass", new { listingId = listing.Id });
        }

        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Edit(int id)
        {
            var listing = await db.Listings
                 .Include(l => l.ListingServices)
                 .Include(l => l.ListingRoomClasses)
                 .Include(l => l.Images)
                 .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            // Agents may only edit their own listing
            if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null || listing.AgentId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            // Map Listing properties to your Edit view model.
            var model = new EditVM
            {
                Id = listing.Id,
                Name = listing.Name,
                Description = listing.Description,
                CityId = listing.CityId,
                Street = listing.Street,
                TotalRoom = listing.TotalRoom,
                TotalFloor = listing.TotalFloor,
                AvgRoomSize = listing.AvgRoomSize,
                StarterPrice = listing.StarterPrice,
                PhoneNo = listing.PhoneNo,
                Rating = listing.Rating,
                ListingTypeId = listing.ListingTypeId,
                SelectedServiceIds = listing.ListingServices.Select(ls => ls.ServiceId).ToList(),
                SelectedRoomClassIds = listing.ListingRoomClasses.Select(lrc => lrc.RoomClassId).ToList(),
                Images = listing.Images.ToList()
            };

            PopulateViewData();

            // Return different views based on role.
            if (User.IsInRole("Admin") && User.IsInRole("Agent"))
            {
                return View("EditAdmin", model);
            }
            else if (User.IsInRole("Agent"))
            {
                return View("Edit", model);
            }
            else
            {
                return View(model);
            }
        }


        // POST: Listing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Edit(int id, EditVM model)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                PopulateViewData();
                return View(model);
            }

            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            bool isAgent = await userManager.IsInRoleAsync(user, "Agent");

            var listing = await db.Listings
                .Include(l => l.ListingServices)
                .Include(l => l.ListingRoomClasses)
                .Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            // Agents may only edit their own listing; non-Admin/Agent cannot edit
            if (!isAdmin)
            {
                if (!isAgent || listing.AgentId != user.Id)
                {
                    return Forbid();
                }
            }

            // Update scalar properties.
            listing.Name = model.Name;
            listing.Description = model.Description;
            listing.CityId = model.CityId;
            listing.Street = model.Street;
            listing.TotalRoom = model.TotalRoom;
            listing.TotalFloor = model.TotalFloor;
            listing.AvgRoomSize = model.AvgRoomSize;
            listing.StarterPrice = model.StarterPrice;
            listing.PhoneNo = model.PhoneNo;
            listing.Rating = model.Rating;
            listing.ListingTypeId = model.ListingTypeId;
            listing.UpdatedAt = DateTime.Now;

            // Process standard file uploads.
            if (model.ListingImages != null && model.ListingImages.Any())
            {
                // Remove old images if you wish to replace them.
                db.ListingImages.RemoveRange(listing.Images);

                foreach (var file in model.ListingImages)
                {
                    var filePath = await UploadFile(file, "ListingImages"); // Implement UploadFile as needed.
                    db.ListingImages.Add(new ListingImage
                    {
                        ListingId = listing.Id,
                        ImagePath = filePath
                    });
                }
            }

            if (model.UploadedImagePaths != null && model.UploadedImagePaths.Any())
            {
                foreach (var filePath in model.UploadedImagePaths)
                {
                    db.ListingImages.Add(new ListingImage
                    {
                        ListingId = listing.Id,
                        ImagePath = filePath
                    });
                }
            }

            db.ListingServices.RemoveRange(listing.ListingServices);
            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    db.ListingServices.Add(new ListingService
                    {
                        ListingId = listing.Id,
                        ServiceId = serviceId
                    });
                }
            }

            db.ListingRoomClasses.RemoveRange(listing.ListingRoomClasses);
            if (model.SelectedRoomClassIds != null && model.SelectedRoomClassIds.Any())
            {
                foreach (var roomClassId in model.SelectedRoomClassIds)
                {
                    db.ListingRoomClasses.Add(new ListingRoomClass
                    {
                        ListingId = listing.Id,
                        RoomClassId = roomClassId
                    });
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Details", new { id=listing.Id });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Listing/DeleteImage")]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            // Find the image by its ID (include listing for ownership check).
            var image = await db.ListingImages
                .Include(img => img.Listing)
                .FirstOrDefaultAsync(img => img.Id == imageId);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Image not found." });
            }

            // Agents may only delete images from their own listing
            if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null || image.Listing?.AgentId != currentUser.Id)
                {
                    return Forbid();
                }
            }


            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            db.ListingImages.Remove(image);
            await db.SaveChangesAsync();

            return Json(new { success = true, imageId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var listing = await db.Listings
                .Include(x => x.City)
                .Include(a => a.Agent)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null)
            {
                return Json(new { success = false, message = "Listing not found." });
            }

            // Toggle the IsActive status
            listing.IsActive = !listing.IsActive;
            listing.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();

            string statusMessage = listing.IsActive ? "Listing is now Active." : "Listing is now Inactive.";
            return Json(new { success = true, isActive = listing.IsActive, message = statusMessage });
        }


        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var listings = await db.Listings
                .Include(l => l.City)
                .Include(l => l.Agent)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(listings);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var filePath = await UploadFile(file, "ListingImages");
                // Return the file path as JSON.
                return Json(new { filePath });
            }
            catch (Exception ex)
            {
                // Log the error as needed.
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #region Helper Methods

        [HttpGet]
        public JsonResult GetCities(int countryId)
        {
            var cities = db.Cities
                           .Where(c => c.CountryId == countryId)
                           .Select(c => new { id = c.Id, name = c.Name })
                           .ToList();
            return Json(cities);
        }

        private async Task<string> UploadFile(IFormFile file, string folderName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Invalid file extension '{fileExtension}'. Allowed extensions: {string.Join(", ", allowedExtensions)}.");
            }

            // Maximum file size: 5MB.
            long maxSizeInBytes = 5 * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
            {
                throw new InvalidOperationException("File size is too large. Maximum allowed size is 5MB.");
            }

            var folder = Path.Combine(env.WebRootPath, "Files", folderName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative path (for use on the client side).
            return Path.Combine("Files", folderName, fileName).Replace("\\", "/");
        }

        private void PopulateViewData()
        {
            ViewBag.Countries = new SelectList(db.Countries, "Id", "Name");
            ViewBag.ListingTypes = new SelectList(db.ListingTypes, "Id", "Name");
            ViewBag.Services = new MultiSelectList(db.Services, "Id", "Description");
            ViewBag.RoomClasses = new MultiSelectList(db.RoomClasses, "Id", "Name");
        }

        #endregion
    }
}
