using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels.Listing
{
    public class ReviewVM
    {
        public int Id { get; set; }

        [Required]
        public int ListingId { get; set; }

        [Range(1, 5)]
        public int Rate { get; set; }

        [Required]
        public string Content { get; set; }

        public IFormFile? Image { get; set; }
    }
}
