using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booking.ViewModels.Listing
{
    public class MultiRoomClassAmenityAssignmentVM
    {
        public int ListingId { get; set; }

        // One "assignment" object per room class
        public List<RoomClassAmenityAssignmentVM> RoomClassAssignments { get; set; }
            = new List<RoomClassAmenityAssignmentVM>();

        // A list of amenities to render as checkboxes
        public IEnumerable<SelectListItem> AmenityList { get; set; }
            = Enumerable.Empty<SelectListItem>();

    }
}
