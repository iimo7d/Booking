using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class Room
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string RoomNo { get; set; }

        public int AdultsCapacity { get; set; }
        public int ChildrenCapacity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int BookedCount { get; set; } = 0;

        public bool IsBooked { get; set; } = false;

        public int RoomSize { get; set; }

        public int ListingId { get; set; }

        public int RoomClassId { get; set; }
        public RoomClass RoomClass { get; set; }
        public Listing Listing { get; set; }    
        public List<RoomImage> Images { get; set; } = new List<RoomImage>();
        public List<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
        public List<Invoice> Invoices { get; set; }
    }
}
