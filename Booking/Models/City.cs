using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Booking.Models
{
    public class City
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int CountryId { get; set; }
        public Country Country { get; set; }
        public int VisitCount { get; set; } = 0;
        public string ImageUrl { get; set; }

        public List<Listing> Listings { get; set; } = new List<Listing>();
    }
}
