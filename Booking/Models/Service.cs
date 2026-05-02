using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Booking.Enums;

namespace Booking.Models
{
    public class Service
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Description { get; set; }

        public ServiceType Type { get; set; }

        public List<ListingService> ListingServices { get; set; } = new List<ListingService>();
    }
}
