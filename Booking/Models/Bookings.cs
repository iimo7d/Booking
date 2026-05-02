using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Booking.Enums;

namespace Booking.Models
{
    public class Bookings
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public BookingStatus Status { get; set; } 
        public PaymentMethod PaymentMethod { get; set; }
        [Required, MaxLength(50)]
        public string ConfirmationNo { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int ListingId { get; set; }
        public Listing Listing { get; set; }
        public AppUser User { get; set; } 
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    }
}

