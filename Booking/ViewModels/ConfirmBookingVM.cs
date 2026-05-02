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

        public string UserName { get; set; }
        public PaymentMethod paymentMethod { get; set; }
        public string HotelName { get; set; }
        public string Address { get; set; }
        public int Rating { get; set; }
        public string Image { get; set; }


        [Required(ErrorMessage = "Confirmation number is required")]
        public string ConfirmationNo { get; set; }
        public string Email { get; set; }
        public List<Models.Room> Rooms { get; set; }
    }
}
