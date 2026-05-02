using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class Listing
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }

        public int CityId { get; set; }
        public City City { get; set; }
        [Required]
        public string Street { get; set; }
        public int TotalRoom { get; set; }
        public int TotalFloor { get; set; }

        public int AvgRoomSize { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal StarterPrice { get; set; }
        [Phone]
        public string PhoneNo { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public int ListingTypeId { get; set; }
        public ListingType ListingType { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string AgentId { get; set; }
        public AppUser Agent { get; set; }
        public JoinUs JoinUs { get; set; }
        public int? JoinUsId { get; set; }
        public List<Service> Services { get; set; } = new List<Service>();
        public List<ListingImage> Images { get; set; } = new List<ListingImage>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();
        public List<Bookings> Bookings { get; set; } = new List<Bookings>();
        public List<ListingRoomClass> ListingRoomClasses { get; set; } = new List<ListingRoomClass>();
        public List<ListingService> ListingServices { get; set; } = new List<ListingService>();
    }
}

