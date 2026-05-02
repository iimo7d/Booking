using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels
{
    public class LoginVM
    {
        public string LoginIdentifier { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
