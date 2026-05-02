using Booking.Models;
using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels.Listing
{
    public class EditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hotel name is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Street is required.")]
        public string Street { get; set; }

        [Display(Name = "Total Rooms")]
        public int TotalRoom { get; set; }

        [Display(Name = "Total Floors")]
        public int TotalFloor { get; set; }

        [Display(Name = "Average Room Size")]
        public int AvgRoomSize { get; set; }

        [Display(Name = "Starter Price")]
        public decimal StarterPrice { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Listing type is required.")]
        public int ListingTypeId { get; set; }

        // You may use this property if you need to track the agent for the listing.
        public int AgentId { get; set; }

        // For cascading dropdown (if you need a Country selection)
        public int CountryId { get; set; }

        // For multi-select controls.
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        public List<int> SelectedRoomClassIds { get; set; } = new List<int>();

        // For uploading new images via standard file upload.
        public List<IFormFile> ListingImages { get; set; } = new List<IFormFile>();

        // For capturing uploaded image paths from Dropzone or similar AJAX uploads.
        public List<string> UploadedImagePaths { get; set; } = new List<string>();

        // Collection of existing images to display on the edit page.
        public List<ListingImage> Images { get; set; } = new List<ListingImage>();

    }
}
