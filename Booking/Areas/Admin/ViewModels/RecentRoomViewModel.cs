namespace Booking.Areas.Admin.ViewModels
{
    public class RecentRoomViewModel
    {
        public int RoomId { get; set; }
        public string? Country { get; set; }
        public string? RoomName { get; set; }
        public string? City { get; set; }
        public string? HotelName { get; set; }
        public string? ImagePath { get; set; }
        public string? BookingStatus { get; set; }
        public DateTime? BookingDate { get; set; }
    }
}
