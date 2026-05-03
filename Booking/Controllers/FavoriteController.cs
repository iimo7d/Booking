using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;

        public FavoriteController(Context db, UserManager<AppUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFavorite(int listingId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Look up an existing favorite
            var favorite = await db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ListingId == listingId);

            // Only add if a favorite doesn't already exist
            if (favorite == null)
            {
                favorite = new Favorite
                {
                    UserId = user.Id,
                    ListingId = listingId
                };

                db.Favorites.Add(favorite);
                await db.SaveChangesAsync();
            }

            // Redirect back to the listing details page
            return RedirectToAction("Details", "Listing", new { id = listingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavorite(int listingId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var favorite = await db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ListingId == listingId);

            if (favorite != null)
            {
                db.Favorites.Remove(favorite);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Listing", new { id = listingId });
        }

        [HttpGet]
        public async Task<IActionResult> MyFavorites()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var favorites = await db.Favorites
                .Where(f => f.UserId == user.Id)
                .Include(f => f.Listing)
                .ThenInclude(c => c.City)
                .Include(f => f.Listing)
                .ThenInclude(i=> i.Images)
                .ToListAsync();

            return View(favorites);
        }
    }
}
