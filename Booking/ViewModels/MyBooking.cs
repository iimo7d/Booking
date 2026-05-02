using Booking.Models;

namespace Booking.ViewModels
{
    public class MyBooking
    {
        public List<Bookings> UpcomingBookings { get; set; } = new List<Bookings>();
        public List<Bookings> CancelledBookings { get; set; } = new List<Bookings>();
        public List<Bookings> CompletedBookings { get; set; } = new List<Bookings>();
    }
}
