using Booking.Models;

namespace Booking.Areas.Agent.Models
{
    public class DashboardViewModel
    {
        public int TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }

        public List<Booking.Models.Bookings> RecentBookings { get; set; } = new List<Booking.Models.Bookings>();
    }
}
