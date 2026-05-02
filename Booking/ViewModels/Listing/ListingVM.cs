using Booking.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels.Listing
{
    public class ListingVM
    {

        public int? Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int CityId { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public int TotalRoom { get; set; }

        [Required]
        public int TotalFloor { get; set; }
        [Required]
        public int AvgRoomSize { get; set; }

        [Required]
        public decimal StarterPrice { get; set; }

        [Phone]
        [Required]
        public string PhoneNo { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public int ListingTypeId { get; set; }

        [Required]
        public string AgentId { get; set; }
        [Required]

        public List<IFormFile> ListingImages { get; set; } = new List<IFormFile>();

        public List<string> UploadedImagePaths { get; set; } = new List<string>();

        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        public List<int> SelectedRoomClassIds { get; set; } = new List<int>();
    }
}

