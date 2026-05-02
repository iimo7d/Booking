namespace Booking.Models
{
    public class ListingRoomClass
    {
        public int ListingId { get; set; }
        public Listing Listing { get; set; }

        public int RoomClassId { get; set; }
        public RoomClass RoomClass { get; set; }

    }
}
