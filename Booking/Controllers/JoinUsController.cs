using Booking.Data;
using Booking.Enums;
using Booking.Models;
using Booking.ViewModels.JoinUs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Controllers
{
   

    public class JoinUsController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment env;
        private readonly IEmailSender emailSender;

        public JoinUsController(Context db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env,IEmailSender emailSender)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.env = env;
            this.emailSender = emailSender;
        }

        [Authorize(Roles = "User")]    
        public IActionResult LandingPage() 
        {
            ViewBag.TotalHotels = db.Listings.Count();
            ViewBag.TotalUsers = db.AppUsers.Count();
            ViewBag.TotalBookings = db.Bookings.Count();




            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult RequestSent() 
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var requests = db.JoinUs
                             .Include(j => j.Agent)
                             .Include(j => j.Listing)
                             .Include(j => j.Agent)
                             .ToList();
            return View(requests);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var request = await db.JoinUs
                                  .Include(j => j.Agent)
                                  .Include(j => j.Listing)
                                    .ThenInclude(c => c.City)
                                        .ThenInclude(c => c.Country)
                                  .Include(j => j.Listing)
                                    .ThenInclude(l => l.ListingType)
                                  .Include(l=>l.Listing)
                                        .ThenInclude(i => i.Images)
                                  .Include(l => l.Listing)
                                     .ThenInclude(l => l.ListingRoomClasses)
                                        .ThenInclude(l => l.RoomClass)
                                            .ThenInclude(l=>l.RoomClassAmenities)
                                                .ThenInclude(a => a.Amenity)
                                  .Include(l => l.Listing)
                                    .ThenInclude(l => l.ListingServices)
                                        .ThenInclude(l => l.Service)
                                  .FirstOrDefaultAsync(j => j.Id == id);
            if (request == null)
                return NotFound();
            return View(request);
        }

        [Authorize(Roles = "User")]

        public IActionResult CreateRequest()
        {
            PopulateViewData();
            return View(new JoinUsRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]

        public async Task<IActionResult> CreateRequest(JoinUsRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PopulateViewData();
                    return View(model);
                }
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Message"] = "please sign in or create account";
                    return View(model);
                }
                var existingRequest = db.JoinUs.FirstOrDefault(j => j.UserId == user.Id &&
                    (j.Status == Status.Pending || j.Status == Status.Approved));
                if (existingRequest != null)
                {
                    ModelState.AddModelError(string.Empty, "You already have a pending or approved request.");
                    PopulateViewData();
                    return View(model);
                }

                // Create the JoinUs request record.
                var joinUs = new JoinUs
                {
                    UserId = user.Id,
                    Reason = model.Reason,
                    Status = Status.Pending,
                    DateOfSubmission = DateTime.Now
                };

                if (model.OwnershipProof != null)
                {
                    joinUs.OwnerShipProof = await UploadFile(model.OwnershipProof, "OwnershipProof");
                }
                if (model.GovernmentCertificate != null)
                {
                    joinUs.GovermentCertificate = await UploadFile(model.GovernmentCertificate, "GovernmentCertificate");
                }
                if (model.IdentityProof != null)
                {
                    joinUs.IdentityProof = await UploadFile(model.IdentityProof, "IdentityProof");
                }

                db.JoinUs.Add(joinUs);
                await db.SaveChangesAsync();

                // Create the Listing record.
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
                    IsActive = false,
                    JoinUsId = joinUs.Id,
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


                // Process Selected Services.
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
                    await db.SaveChangesAsync();
                }

                // Process Selected Room Classes.
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
                    await db.SaveChangesAsync();
                }

                return RedirectToAction("MultiAssignAmenity", "RoomClass" , new { listingId = listing.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                PopulateViewData();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var joinUs = await db.JoinUs
                .Include(j => j.Listing)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (joinUs == null)
            {
                return NotFound("JoinUs request not found.");
            }

            if (string.IsNullOrEmpty(joinUs.UserId))
            {
                return BadRequest("User ID is missing.");
            }

            var user = await userManager.FindByIdAsync(joinUs.UserId);
            if (user == null)
            {
                return NotFound("User not found in the database.");
            }

            if (joinUs.Listing != null)
            {
                joinUs.Listing.IsActive = true;
                db.Listings.Update(joinUs.Listing);
            }

            if (!await userManager.IsInRoleAsync(user, "Agent"))
            {
                await userManager.AddToRoleAsync(user, "Agent");
            }

            joinUs.Status = Status.Approved;
            db.JoinUs.Update(joinUs);

            string userFirstName = user.FirstName ?? "User";
            string body = $@"
<html>
  <head>
    <meta charset='UTF-8'>
    <title>Your Hotel Listing Request Has Been Approved!</title>
  </head>
  <body>
    <p>Dear {userFirstName},</p>
    <p>Your hotel listing request has been <strong>APPROVED!</strong></p>
  </body>
</html>";

            await emailSender.SendEmailAsync(user.Email, "Your Hotel Listing Request Has Been Approved!", body);
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var joinUs = await db.JoinUs.Include(j => j.Listing).FirstOrDefaultAsync(j => j.Id == id);
            if (joinUs == null)
            {
                return NotFound();
            }
            if (joinUs.Listing != null)
            {
                db.Listings.Remove(joinUs.Listing);
            }

            joinUs.Status = Status.Rejected;

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public JsonResult GetCities(int countryId)
        {
            var cities = db.Cities
                           .Where(c => c.CountryId == countryId)
                           .Select(c => new { id = c.Id, name = c.Name })
                           .ToList();
            return Json(cities);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
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

        private void PopulateViewData()
        {
            ViewBag.Countries = new SelectList(db.Countries, "Id", "Name");
            ViewBag.ListingTypes = new SelectList(db.ListingTypes, "Id", "Name");
            ViewBag.Services = new MultiSelectList(db.Services, "Id", "Description");
            ViewBag.RoomClasses = new MultiSelectList(db.RoomClasses, "Id", "Name");
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
    }
}
