using Booking.Enums;
using Booking.Models;

namespace Booking.ViewModels
{
    public class BookingVM
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

        public List<Models.Room> Rooms { get; set; }

        public AppUser? AppUser { get; set; }
    }
}
