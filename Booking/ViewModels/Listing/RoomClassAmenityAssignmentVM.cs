namespace Booking.ViewModels.Listing
{
    public class RoomClassAmenityAssignmentVM
    {

        public int RoomClassId { get; set; }
        public string RoomClassName { get; set; }

        // Amenity IDs that the user has checked for this room class
        public List<int> SelectedAmenityIds { get; set; } = new List<int>();
    }
}
