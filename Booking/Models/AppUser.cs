using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class AppUser : IdentityUser
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateOnly? DOB { get; set; }

        [DataType(DataType.ImageUrl)]
        public string AvatarUrl { get; set; } = "/assets/images/avatar/PlaceHolder.png";

     
        public int? CityId { get; set; }
        public City City { get; set; } 
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Bookings> Bookings { get; set; } = new List<Bookings>();
        public List<Listing> Listings { get; set; } = new List<Listing>();
        public List<JoinUs> JoinUsRequests { get; set; } = new List<JoinUs>();
    }
}
