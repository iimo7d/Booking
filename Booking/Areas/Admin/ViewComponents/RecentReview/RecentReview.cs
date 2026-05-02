using Booking.Areas.Admin.ViewModels;
using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "RecentReview")]
    public class RecentReviewViewComponent : ViewComponent
    {
        private readonly Context db;

        public RecentReviewViewComponent(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var latestReviews = await db.Reviews
                .Include(r => r.Listing)
                .ThenInclude(l => l.City)
                .ThenInclude(c => c.Country)
                .OrderByDescending(r => r.CreationTime)
                .Take(5)
                .Select(r => new RecentReviewViewModel
                {
                    ReviewId = r.Id,
                    HotelName = r.Listing.Name,
                    City = r.Listing.City.Name,
                    Country = r.Listing.City.Country.Name,
                    ImagePath = r.ImagePath ?? "/default-review.jpg",
                    ReviewContent = r.Content,
                    ReviewRating = r.Rate,
                    ReviewDate = r.CreationTime
                })
                .ToListAsync();

            return View(latestReviews);
        }
    }
}
