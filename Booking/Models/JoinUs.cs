using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Booking.Enums;

namespace Booking.Models
{
    public class JoinUs
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public Status Status { get; set; }
        public DateTime DateOfSubmission { get; set; } = DateTime.Now;
        public string OwnerShipProof { get; set; }
        public string GovermentCertificate { get; set; }
        public string IdentityProof { get; set; }
        public string Reason { get; set; }
        public AppUser Agent { get; set; }

        public Listing Listing { get; set; } 
    }
}
 