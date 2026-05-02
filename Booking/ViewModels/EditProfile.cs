using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels
{
    public class EditProfile
    {
        public int CountryId { get; set; }

        public string Email { get; set; }
        [Required, MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateOnly? DOB { get; set; }

        [Display(Name = "Avatar URL")]
        public string AvatarUrl { get; set; }

        [Display(Name = "Upload Avatar")]
        public IFormFile AvatarImage { get; set; }

        [Display(Name = "City")]
        public int? CityId { get; set; }
    }
}
