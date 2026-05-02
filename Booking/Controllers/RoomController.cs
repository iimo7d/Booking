using Booking.Data;
using Booking.Models;
using Booking.ViewComponents;
using Booking.ViewModels.Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Controllers
{
    public class RoomController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;
        private readonly IWebHostEnvironment env;

        public RoomController(Context db, UserManager<AppUser> userManager, IWebHostEnvironment env)
        {
            this.db = db;
            this.userManager = userManager;
            this.env = env;
        }










        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RoomDetail(int roomId)
        {
            var room = await db.Rooms
                .Include(r => r.Images)
                .Include(r => r.RoomClass)
                    .ThenInclude(rc => rc.RoomClassAmenities)
                        .ThenInclude(rca => rca.Amenity)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound();
            }
            return PartialView("_RoomDetailPartial", room);
        }


        #region Edit one room

        [HttpGet]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> EditRoom(int roomId)
        {
            var room = await db.Rooms
                .Include(r => r.Images)
                .Include(r => r.RoomClass)
                .Include(x => x.Listing)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound();
            }

            // Agents may only edit rooms belonging to their own listing
            if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null || room.Listing?.AgentId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            var roomClasses = await db.RoomClasses
                .Where(rc => db.ListingRoomClasses.Any(lrc => lrc.RoomClassId == rc.Id && lrc.ListingId == room.Listing.Id))
                .ToListAsync();

            ViewBag.RoomClasses = new SelectList(roomClasses, "Id", "Name");
            return PartialView("_RoomEditPartial", room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> EditRoom(Room updatedRoom, IFormFileCollection RoomImages)
        {
            var room = await db.Rooms.Include(r => r.Images)
                .Include(x => x.Listing)
                .FirstOrDefaultAsync(r => r.Id == updatedRoom.Id);
            if (room == null)
            {
                return NotFound();
            }

            // Agents may only edit rooms belonging to their own listing
            if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null || room.Listing?.AgentId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            


            room.RoomNo = updatedRoom.RoomNo;
            room.AdultsCapacity = updatedRoom.AdultsCapacity;
            room.ChildrenCapacity = updatedRoom.ChildrenCapacity;
            room.PricePerNight = updatedRoom.PricePerNight;
            room.RoomSize = updatedRoom.RoomSize;
            room.RoomClassId = updatedRoom.RoomClassId;

            if (RoomImages != null && RoomImages.Count > 0)
            {
                foreach (var image in RoomImages)
                {
                    var filePath = await UploadFile(image, "RoomImages");
                    db.RoomImages.Add(new RoomImage
                    {
                        RoomId = room.Id,
                        ImagePath = filePath
                    });
                }
            }



            var roomClasses = await db.RoomClasses
                .Where(rc => db.ListingRoomClasses.Any(lrc => lrc.RoomClassId == rc.Id && lrc.ListingId == room.Listing.Id))
                .ToListAsync();
            ViewBag.RoomClasses = new SelectList(roomClasses, "Id", "Name");
            db.SaveChanges();
            return Json(new { success = true, message = "Room updated successfully." });
        }


        #endregion


        #region Add Rooms
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> AddRooms(int listingId)
        {
            try
            {
                var listing = await db.Listings.FindAsync(listingId);
                if (listing == null)
                    return NotFound("Hotel not found.");

                if (!listing.IsActive)
                    return BadRequest("Cannot add rooms to an inactive listing.");

                // Agents may only add rooms to their own listing
                if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
                {
                    var currentUser = await userManager.GetUserAsync(User);
                    if (currentUser == null || listing.AgentId != currentUser.Id)
                        return Forbid();
                }

                var roomClasses = await db.RoomClasses
                    .Where(rc => db.ListingRoomClasses.Any(lrc => lrc.RoomClassId == rc.Id && lrc.ListingId == listingId))
                    .ToListAsync();

                var vm = new MultiRoomAssignmentVM
                {
                    ListingId = listingId,
                    Rooms = new List<RoomVM> { new RoomVM() },
                    RoomClassList = roomClasses.Select(rc => new SelectListItem
                    {
                        Value = rc.Id.ToString(),
                        Text = rc.Name
                    })
                };

                ViewBag.ListingName = listing?.Name;
                return View(vm);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while loading the add rooms page.");
            }
        }

        // POST: Room/AddRooms
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> AddRooms(MultiRoomAssignmentVM model)
        {
            try
            {
                var listing = await db.Listings.FindAsync(model.ListingId);
                if (listing == null)
                    return NotFound("Hotel not found.");

                if (!listing.IsActive)
                    return BadRequest("Cannot add rooms to an inactive Hotel.");

                // Agents may only add rooms to their own listing
                if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
                {
                    var currentUser = await userManager.GetUserAsync(User);
                    if (currentUser == null || listing.AgentId != currentUser.Id)
                        return Forbid();
                }

            
                foreach (var roomVm in model.Rooms)
                {
                    var room = new Room
                    {
                        RoomNo = roomVm.RoomNo,
                        AdultsCapacity = roomVm.AdultsCapacity,
                        ChildrenCapacity = roomVm.ChildrenCapacity,
                        PricePerNight = roomVm.PricePerNight,
                        RoomSize = roomVm.RoomSize,
                        RoomClassId = roomVm.SelectedRoomClassId,
                        ListingId = model.ListingId,
                        CreatedAt = DateTime.Now
                    };

                    db.Rooms.Add(room);
                    await db.SaveChangesAsync(); // Save to get room.Id

                    if (roomVm.RoomImages != null && roomVm.RoomImages.Count > 0)
                    {
                        foreach (var image in roomVm.RoomImages)
                        {
                            var filePath = await UploadFile(image, "RoomImages");
                            db.RoomImages.Add(new RoomImage
                            {
                                RoomId = room.Id,
                                ImagePath = filePath
                            });
                        }
                    }
                }

                await db.SaveChangesAsync();
                TempData["Message"] = "Rooms added successfully!";
                return RedirectToAction("Index", "Listing");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding rooms.");
            }
        }

        #endregion

        #region Edit Rooms

        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> EditRooms(int listingId)
        {
            try
            {
                var listing = await db.Listings.FindAsync(listingId);
                if (listing == null)
                    return NotFound("Hotel not found.");

                // Agents may only edit rooms for their own listing
                if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
                {
                    var currentUser = await userManager.GetUserAsync(User);
                    if (currentUser == null || listing.AgentId != currentUser.Id)
                        return Forbid();
                }

                var rooms = await db.Rooms
                    .Where(r => r.ListingId == listingId)
                    .ToListAsync();

                var roomClasses = await db.RoomClasses
                    .Where(rc => db.ListingRoomClasses.Any(lrc => lrc.RoomClassId == rc.Id && lrc.ListingId == listingId))
                    .ToListAsync();

                var vm = new MultiRoomAssignmentVM
                {
                    ListingId = listingId,
                    Rooms = rooms.Select(r => new RoomVM
                    {
                        Id = r.Id,
                        RoomNo = r.RoomNo,
                        AdultsCapacity = r.AdultsCapacity,
                        ChildrenCapacity = r.ChildrenCapacity,
                        PricePerNight = r.PricePerNight,
                        RoomSize = r.RoomSize,
                        SelectedRoomClassId = r.RoomClassId,
                        ExistingImages = db.RoomImages
                            .Where(img => img.RoomId == r.Id)
                            .Select(img => new RoomImageVM { Id = img.Id, ImagePath = img.ImagePath })
                            .ToList()
                    }).ToList(),
                    RoomClassList = roomClasses.Select(rc => new SelectListItem
                    {
                        Value = rc.Id.ToString(),
                        Text = rc.Name
                    })
                };

                ViewBag.ListingName = listing?.Name;
                return View(vm);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while loading the edit rooms page.");
            }
        }

        // POST: Room/EditRooms
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> EditRooms(MultiRoomAssignmentVM model)
        {
            try
            {
                var listing = await db.Listings.FindAsync(model.ListingId);
                if (listing == null)
                    return NotFound("Hotel not found.");

                if (!listing.IsActive)
                    return BadRequest("Cannot edit rooms for an inactive Hotel.");

                var user = await userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                // Agents may only edit rooms for their own listing; Admins bypass this check
                if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
                {
                    if (user.Id != listing.AgentId)
                        return Unauthorized("You are not authorized to edit rooms for this Hotel.");
                }

                foreach (var roomVm in model.Rooms)
                {
                    var room = await db.Rooms.FindAsync(roomVm.Id);
                    if (room == null)
                        continue;

                    room.RoomNo = roomVm.RoomNo;
                    room.AdultsCapacity = roomVm.AdultsCapacity;
                    room.ChildrenCapacity = roomVm.ChildrenCapacity;
                    room.PricePerNight = roomVm.PricePerNight;
                    room.RoomSize = roomVm.RoomSize;
                    room.RoomClassId = roomVm.SelectedRoomClassId;

                    db.Rooms.Update(room);

                    if (roomVm.RoomImages != null && roomVm.RoomImages.Count > 0)
                    {
                        foreach (var image in roomVm.RoomImages)
                        {
                            var filePath = await UploadFile(image, "RoomImages");
                            db.RoomImages.Add(new RoomImage
                            {
                                RoomId = room.Id,
                                ImagePath = filePath
                            });
                        }
                    }
                }

                await db.SaveChangesAsync();
                TempData["Message"] = "Rooms updated successfully!";
                return RedirectToAction("Index", "Listing");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while editing rooms.");
            }
        }

        #endregion

        #region Delete Room & Delete Room Image

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var room = await db.Rooms.Include(r => r.Listing).FirstOrDefaultAsync(r => r.Id == id);
                if (room == null)
                    return NotFound();

                // Agents may only delete rooms belonging to their own listing
                if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
                {
                    var currentUser = await userManager.GetUserAsync(User);
                    if (currentUser == null || room.Listing?.AgentId != currentUser.Id)
                        return Forbid();
                }

                // Retrieve associated images.
                var images = await db.RoomImages.Where(img => img.RoomId == id).ToListAsync();
                foreach (var image in images)
                {
                    var relativePath = image.ImagePath.TrimStart('/');
                    var filePath = Path.Combine(env.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception)
                        {
                            // Continue even if one file fails deletion.
                        }
                    }
                }

                db.RoomImages.RemoveRange(images);
                db.Rooms.Remove(room);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the room.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> DeleteRoomImage(int imageId)
        {
            try
            {
                var image = await db.RoomImages
                    .Include(img => img.Room).ThenInclude(r => r.Listing)
                    .FirstOrDefaultAsync(img => img.Id == imageId);
                if (image == null)
                    return NotFound();

                // Agents may only delete images from their own listing's rooms
                if (User.IsInRole("Agent") && !User.IsInRole("Admin"))
                {
                    var currentUser = await userManager.GetUserAsync(User);
                    if (currentUser == null || image.Room?.Listing?.AgentId != currentUser.Id)
                        return Forbid();
                }

                var relativePath = image.ImagePath.TrimStart('/');
                var filePath = Path.Combine(env.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        // Continue even if file deletion fails.
                    }
                }

                db.RoomImages.Remove(image);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the image.");
            }
        }

        #endregion

        #region Helper Method

        private async Task<string> UploadFile(IFormFile file, string folderName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException($"Invalid file extension '{fileExtension}'. Allowed extensions are: {string.Join(", ", allowedExtensions)}.");

            long maxSizeInBytes = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSizeInBytes)
                throw new InvalidOperationException("File size is too large. The maximum allowed size is 5MB.");

            var uploadsFolder = Path.Combine(env.WebRootPath, "Files", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // Return relative path
            return Path.Combine("Files", folderName, fileName).Replace("\\", "/");
        }

        #endregion
    }
}
