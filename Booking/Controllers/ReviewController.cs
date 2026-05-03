using Booking.Data;
using Booking.Enums;
using Booking.Models;
using Booking.ViewModels.Listing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booking.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly Context db;
        private readonly IWebHostEnvironment env;

        public ReviewController(Context db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }


        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostReview(Review model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var hasConfirmedBooking = await db.Bookings
                .AnyAsync(b => b.UserId == userId
                    && b.Status == BookingStatus.Confirmed
                    && b.BookingRooms.Any(br => br.Room.ListingId == model.ListingId));

            if (!hasConfirmedBooking)
            {
                TempData["Error"] = "You can only review hotels with a confirmed booking.";
                return RedirectToAction("Details", "Listing", new { id = model.ListingId });
            }

            model.UserId = userId;

            var uploadedImagePaths = Request.Form["UploadedImagePaths"].FirstOrDefault();
            if (!string.IsNullOrEmpty(uploadedImagePaths))
            {
                model.ImagePath = uploadedImagePaths;
            }

            model.CreationTime = DateTime.Now;
            db.Reviews.Add(model);
            await db.SaveChangesAsync();
            TempData["Success"] = "Review posted successfully.";
            return RedirectToAction("Details", "Listing", new { id = model.ListingId });


        }


        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(Review review)
        {
            if (review == null || review.Id <= 0)
            {
                TempData["Error"] = "Invalid review.";
                return BadRequest();
            }

            var existingReview = await db.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == review.Id);
            if (existingReview == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Details", "Listing", new { id = review.ListingId });
            }

            // Ensure only the original creator can edit.
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingReview.UserId != currentUserId)
            {
                TempData["Error"] = "You are not authorized to edit this review.";
                return RedirectToAction("Details", "Listing", new { id = review.ListingId });
            }

            // Check for a new uploaded image via the hidden field.
            var uploadedImagePaths = Request.Form["UploadedImagePaths"].FirstOrDefault();
            if (!string.IsNullOrEmpty(uploadedImagePaths))
            {
                // Optionally delete the old image file.
                if (!string.IsNullOrEmpty(existingReview.ImagePath))
                {
                    var oldImagePath = Path.Combine(env.WebRootPath, existingReview.ImagePath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                review.ImagePath = uploadedImagePaths;
            }
            else
            {
                // Retain the old image if no new one is uploaded.
                review.ImagePath = existingReview.ImagePath;
            }

            // Ensure that UserId and CreationTime remain unchanged.
            review.UserId = existingReview.UserId;
            review.ListingId = existingReview.ListingId;
            review.CreationTime = existingReview.CreationTime;
            review.LastUpdate = DateTime.Now;


            try
            {
                db.Reviews.Update(review);
                await db.SaveChangesAsync();
                TempData["Success"] = "Review updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!db.Reviews.Any(r => r.Id == review.Id))
                {
                    TempData["Error"] = "Review not found.";
                    return RedirectToAction("Details", "Listing", new { id = review.ListingId });
                }
                else
                {
                    throw;
                }
            }


            return RedirectToAction("Details", "Listing", new { id = review.ListingId });
        }


        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await db.Reviews.FindAsync(id);
            if (review == null)
            {
                return Json(new { success = false, message = "Review not found." });
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != currentUserId)
            {
                return Json(new { success = false, message = "You are not authorized to delete this review." });
            }

            // Delete the associated image file if it exists.
            if (!string.IsNullOrEmpty(review.ImagePath))
            {
                var filePath = Path.Combine(env.WebRootPath, review.ImagePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            db.Reviews.Remove(review);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Review deleted successfully." });
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviews(int listingId, int page, int pageSize)
        {
            var reviewsQuery = db.Reviews
                .Include(r => r.User)
                .Where(r => r.ListingId == listingId)
                .OrderByDescending(r => r.CreationTime);

            var reviews = await reviewsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Build an anonymous object with matching property names:
            var reviewsData = reviews.Select(r => new
            {
                r.Id,

                // The client uses `review.AvatarPath`:
                AvatarPath = string.IsNullOrEmpty(r.User.AvatarUrl)
                    ? Url.Content("~/assets/images/avatar/default.jpg")
                    : Url.Content("~/" + r.User.AvatarUrl),

                // The client uses `review.UserName`:
                UserName = $"{r.User.FirstName} {r.User.LastName}",

                // The client uses `review.CreationTime`:
                CreationTime = r.CreationTime.ToString("dd MMM yyyy"),

                // The client uses `review.ReviewCount`:
                ReviewCount = db.Reviews.Count(x => x.UserId == r.UserId),

                // The client uses `review.Rate`, `review.Content`:
                r.Rate,
                r.Content,

                // The client uses `review.ImagePath`:
                ImagePath = string.IsNullOrEmpty(r.ImagePath)
                    ? null
                    : Url.Content("~/" + r.ImagePath),

                // The client uses `review.UserId` to check ownership:
                r.UserId
            });

            return Json(new { success = true, reviews = reviewsData });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension) || file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Invalid image file.");
                }

                var uploadsFolder = Path.Combine(env.WebRootPath, "images", "reviews");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the relative path (with forward slashes for web use).
                var relativePath = Path.Combine("images", "reviews", fileName).Replace("\\", "/");
                return Json(new { filePath = relativePath });
            }
            return Json(new { filePath = string.Empty });
        }
    }
}
