using Booking.Models;

namespace Booking.Areas.Agent.Models
{
    public class AgentRoomVM
    {
        public int? HotelId { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms{ get; set; }
        public int BookedRooms { get; set; }
        public List<string> FirstImagePaths { get; set; }
        public List<Room> Rooms { get; set; } = new List<Room>();

    }
}
