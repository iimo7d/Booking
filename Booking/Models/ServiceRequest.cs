using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Booking.Enums;

namespace Booking.Models
{
    public class ServiceRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public ServiceType Type { get; set; }
        public Status RequsetStatus { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}
