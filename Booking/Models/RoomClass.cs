using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class RoomClass
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        // Many-to-many relationship with Amenity
        public List<RoomClassAmenity> RoomClassAmenities { get; set; } = new List<RoomClassAmenity>();
        public List<ListingRoomClass> ListingRoomClasses { get; set; } = new List<ListingRoomClass>();

        public List<Room> Rooms { get; set; } = new List<Room>();
    }
}
