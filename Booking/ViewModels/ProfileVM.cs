using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels
{
    public class ProfileVM
    {
        public EditProfile Profile { get; set; }
        public PasswordVM Password { get; set; }
    }
}
