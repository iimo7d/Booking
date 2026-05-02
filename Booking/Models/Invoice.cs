using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class Invoice
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Bookings Booking { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price{ get; set; }

    }
}
