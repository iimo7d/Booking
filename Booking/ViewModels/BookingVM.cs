using Booking.Enums;
using Booking.Models;

namespace Booking.ViewModels
{
    public class BookingVM
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

        public List<Models.Room> Rooms { get; set; } = new List<Models.Room>();

        public AppUser? AppUser { get; set; }
    }
}
