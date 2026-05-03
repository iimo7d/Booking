using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.Controllers
{
    public class BlogController : Controller
    {
        private readonly Context db;
        private readonly IWebHostEnvironment env;

        public BlogController(Context db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }


        public async Task<IActionResult> Index()
        {
            var blogs = await db.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(blogs);
        }


        public async Task<IActionResult> BlogContent()
        {
            var blogs = await db.Blogs

                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(blogs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await db.Blogs.FindAsync(id);
            if (blog == null) return Json(null);

            return Json(new
            {
                title = blog.Title,
                content = blog.Content,
                author = blog.Author,
                createdAt = blog.CreatedAt.ToString("dd MMM yyyy"),
                imageUrl = !string.IsNullOrEmpty(blog.ImageUrl) ? blog.ImageUrl : null,
                videoUrl = !string.IsNullOrEmpty(blog.VideoUrl) ? blog.VideoUrl : null
            });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Blogs blog, IFormFile? image, IFormFile? video)
        {
            if (blog == null || string.IsNullOrEmpty(blog.Title) || string.IsNullOrEmpty(blog.Content))
                return Json(new { success = false, message = "Please fill all required fields." });

            // Process Image
            if (image != null)
            {
                var imagePath = await SaveBlogFileAsync(image, "Uploads/Blogs/Images", new[] { ".jpg", ".jpeg", ".png" }, 5 * 1024 * 1024);
                if (imagePath == null)
                {
                    return Json(new { success = false, message = "Image must be a JPG, JPEG, or PNG file up to 5MB." });
                }
                blog.ImageUrl = imagePath;
            }

            // Process Video
            if (video != null)
            {
                var videoPath = await SaveBlogFileAsync(video, "Uploads/Blogs/Videos", new[] { ".mp4", ".webm", ".mov" }, 50 * 1024 * 1024);
                if (videoPath == null)
                {
                    return Json(new { success = false, message = "Video must be an MP4, WEBM, or MOV file up to 50MB." });
                }
                blog.VideoUrl = videoPath;
            }

            blog.CreatedAt = DateTime.UtcNow;
            db.Blogs.Add(blog);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Blog created successfully." });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Blogs model, IFormFile? image, IFormFile? video)
        {
            if (id != model.Id) return Json(new { success = false, message = "Invalid Blog ID." });

            var blog = await db.Blogs.FindAsync(id);
            if (blog == null) return Json(new { success = false, message = "Blog not found." });

            blog.Title = model.Title;
            blog.Content = model.Content;
            blog.Author = model.Author;
            blog.IsPublished = model.IsPublished;

            // Process Image Update
            if (image != null)
            {
                var imagePath = await SaveBlogFileAsync(image, "Uploads/Blogs/Images", new[] { ".jpg", ".jpeg", ".png" }, 5 * 1024 * 1024);
                if (imagePath == null)
                {
                    return Json(new { success = false, message = "Image must be a JPG, JPEG, or PNG file up to 5MB." });
                }

                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    var oldFile = Path.Combine(env.WebRootPath, blog.ImageUrl);
                    if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                }

                blog.ImageUrl = imagePath;
            }

            // Process Video Update
            if (video != null)
            {
                var videoPath = await SaveBlogFileAsync(video, "Uploads/Blogs/Videos", new[] { ".mp4", ".webm", ".mov" }, 50 * 1024 * 1024);
                if (videoPath == null)
                {
                    return Json(new { success = false, message = "Video must be an MP4, WEBM, or MOV file up to 50MB." });
                }

                if (!string.IsNullOrEmpty(blog.VideoUrl))
                {
                    var oldFile = Path.Combine(env.WebRootPath, blog.VideoUrl);
                    if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                }

                blog.VideoUrl = videoPath;
            }

            db.Blogs.Update(blog);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Blog updated successfully." });
        }


        private async Task<string?> SaveBlogFileAsync(IFormFile file, string folder, string[] allowedExtensions, long maxBytes)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension) || file.Length > maxBytes)
            {
                return null;
            }

            var folderPath = Path.Combine(env.WebRootPath, folder);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(folderPath, fileName);
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await db.Blogs.FindAsync(id);
            if (blog == null) return Json(new { success = false, message = "Blog not found." });

            // Delete Image File
            if (!string.IsNullOrEmpty(blog.ImageUrl))
            {
                var oldImage = Path.Combine(env.WebRootPath, blog.ImageUrl);
                if (System.IO.File.Exists(oldImage)) System.IO.File.Delete(oldImage);
            }

            // Delete Video File
            if (!string.IsNullOrEmpty(blog.VideoUrl))
            {
                var oldVideo = Path.Combine(env.WebRootPath, blog.VideoUrl);
                if (System.IO.File.Exists(oldVideo)) System.IO.File.Delete(oldVideo);
            }

            db.Blogs.Remove(blog);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Blog deleted successfully." });
        }

    }
}
