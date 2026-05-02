using System.Diagnostics.CodeAnalysis;

namespace Booking.Areas.Admin.ViewModels
{
    public class GuestViewModel
    {
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime JoinDate { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ContactNo { get; set; }
        public List<BookingHistoryViewModel> BookingHistory { get; set; } = new List<BookingHistoryViewModel>();

    }

    public class GuestListViewModel
    {

        public List<GuestViewModel>? Guests { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class BookingHistoryViewModel
    {
        public string? RoomName { get; set; }
        public decimal PricePerNight { get; set; }
        public string? RoomNumber { get; set; }
        public string? BookingStatus { get; set; }


        public DateTime BookDate { get; set; }
        public string? RoomImage { get; set; }
    }


}
