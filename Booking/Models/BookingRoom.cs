using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class BookingRoom
    {
        public int BookingId { get; set; }

        public int RoomId { get; set; }    

        public Bookings Booking { get; set; }
        public Room Room { get; set; }
    }
}
