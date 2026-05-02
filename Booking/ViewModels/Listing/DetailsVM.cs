using Booking.Models;

namespace Booking.ViewModels.Listing
{
    public class DetailsVM
    {
        public Booking.Models.Listing Listing { get; set; }
        public IEnumerable<Review> Reviews
        {
            get => Listing?.Reviews ?? new List<Review>();
        }


        public List<Booking.Models.Room> Rooms { get; set; }

        public bool IsFavorite { get; set; } =false;
        public int TotalReviews => Reviews.Count();
        public double AverageRating => TotalReviews > 0 ? Reviews.Average(r => r.Rate) : 0;

        public int FiveStarCount => Reviews.Count(r => r.Rate == 5);
        public int FourStarCount => Reviews.Count(r => r.Rate == 4);
        public int ThreeStarCount => Reviews.Count(r => r.Rate == 3);
        public int TwoStarCount => Reviews.Count(r => r.Rate == 2);
        public int OneStarCount => Reviews.Count(r => r.Rate == 1);

        public double FiveStarPercent => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
        public double FourStarPercent => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
        public double ThreeStarPercent => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
        public double TwoStarPercent => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
        public double OneStarPercent => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;

        public int PageSize { get; set; } = 5;
    }
}

