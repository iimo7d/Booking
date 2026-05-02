using Booking.Data;
using Booking.Models;
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
        public async Task<IActionResult> Create(Blogs blog, IFormFile? image, IFormFile? video)
        {
            if (blog == null || string.IsNullOrEmpty(blog.Title) || string.IsNullOrEmpty(blog.Content))
                return Json(new { success = false, message = "Please fill all required fields." });

            // Process Image
            if (image != null)
            {
                var folder = "Uploads/Blogs/Images";
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var folderPath = Path.Combine(env.WebRootPath, folder);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }
                blog.ImageUrl = Path.Combine(folder, fileName).Replace("\\", "/");
            }

            // Process Video
            if (video != null)
            {
                var folder = "Uploads/Blogs/Videos";
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                var folderPath = Path.Combine(env.WebRootPath, folder);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await video.CopyToAsync(fs);
                }
                blog.VideoUrl = Path.Combine(folder, fileName).Replace("\\", "/");
            }

            blog.CreatedAt = DateTime.UtcNow;
            db.Blogs.Add(blog);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Blog created successfully." });
        }


        [HttpPost]
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
                var folder = "Uploads/Blogs/Images";
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var folderPath = Path.Combine(env.WebRootPath, folder);

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }

                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    var oldFile = Path.Combine(env.WebRootPath, blog.ImageUrl);
                    if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                }

                blog.ImageUrl = Path.Combine(folder, fileName).Replace("\\", "/");
            }

            // Process Video Update
            if (video != null)
            {
                var folder = "Uploads/Blogs/Videos";
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                var folderPath = Path.Combine(env.WebRootPath, folder);

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await video.CopyToAsync(fs);
                }

                if (!string.IsNullOrEmpty(blog.VideoUrl))
                {
                    var oldFile = Path.Combine(env.WebRootPath, blog.VideoUrl);
                    if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                }

                blog.VideoUrl = Path.Combine(folder, fileName).Replace("\\", "/");
            }

            db.Blogs.Update(blog);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Blog updated successfully." });
        }


        [HttpPost]
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