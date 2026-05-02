using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class Review
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Content { get; set; }
        [Range(1, 5)]
        public int Rate { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime? LastUpdate { get; set; }

        [Required]
        public string UserId { get; set; }
        public int ListingId { get; set; }
        public string? ImagePath { get; set; }

        public Listing Listing { get; set; }
        public AppUser User { get; set; }

        
        public string? AgentReply { get; set; }
        public DateTime? AgentReplyDate { get; set; }
    }

}
