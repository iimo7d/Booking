using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booking.ViewModels.Room
{
    public class MultiRoomAssignmentVM
    {
        public int ListingId { get; set; }

        // A collection of rooms to be added or edited.
        public List<RoomVM> Rooms { get; set; } = new List<RoomVM>();

        // Dropdown list of room classes associated with the listing.
        public IEnumerable<SelectListItem> RoomClassList { get; set; }
    }
}
