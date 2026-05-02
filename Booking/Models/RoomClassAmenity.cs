using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class RoomClassAmenity
    {
        [Column(Order = 0)]
        public int RoomClassId { get; set; }

        [ Column(Order = 1)]
        public int AmenityId { get; set; }

        public RoomClass RoomClass { get; set; }
        public Amenity Amenity { get; set; }

        public int ListingId { get; set; }
        public Listing Listing { get; set; }
    }
}
