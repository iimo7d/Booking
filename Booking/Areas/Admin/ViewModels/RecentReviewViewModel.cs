namespace Booking.Areas.Admin.ViewModels
{
    public class RecentReviewViewModel
    {
        public int ReviewId { get; set; }
        public string? HotelName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ImagePath { get; set; }
        public string? ReviewContent { get; set; }
        public int ReviewRating { get; set; }
        public DateTime? ReviewDate { get; set; }
    }
}
