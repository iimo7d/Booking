using Booking.Enums;
using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels
{
    public class ConfirmBookingVM
    {
        public int BookingId { get; set; }
        public int ListingId { get; set; }
        public int TotalRooms { get; set; }
        public int TotalNight { get; set; }
        public int TotalPrice { get; set; }



        public DateTime BookingDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public string UserName { get; set; } = string.Empty;
        public PaymentMethod paymentMethod { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Image { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirmation number is required")]
        public string ConfirmationNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<Models.Room> Rooms { get; set; } = new List<Models.Room>();
    }
}
