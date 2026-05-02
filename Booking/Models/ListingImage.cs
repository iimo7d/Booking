using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Booking.Models
{
    public class ListingImage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public int ListingId { get; set; }
        public Listing Listing { get; set; }
    }
}
