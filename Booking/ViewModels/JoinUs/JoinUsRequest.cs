using Booking.Enums;
using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels.JoinUs
{

    public class JoinUsRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

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
        public string PhoneNo { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public int ListingTypeId { get; set; }

        public IFormFile OwnershipProof { get; set; }
        public IFormFile GovernmentCertificate { get; set; }
        public IFormFile IdentityProof { get; set; }

        public List<IFormFile> ListingImages { get; set; } = new List<IFormFile>();

        public List<string> UploadedImagePaths { get; set; } = new List<string>();
        [Required]
        public string Reason { get; set; }

        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        public List<int> SelectedRoomClassIds { get; set; } = new List<int>();
    }
}



