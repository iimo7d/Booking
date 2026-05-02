using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class ListingService
    {
        public int ListingId { get; set; }

        public int ServiceId { get; set; }

        public Listing Listing { get; set; }
        public Service Service { get; set; }
    }
}
