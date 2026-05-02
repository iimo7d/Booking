using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Booking.Enums;

namespace Booking.Models
{
    public class AmenityRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }

        public int RoomClassId { get; set; }
        public RoomClass RoomClass { get; set; }

        public Status RequsetStatus { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }

}

