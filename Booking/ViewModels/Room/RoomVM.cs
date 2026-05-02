using System.ComponentModel.DataAnnotations;

namespace Booking.ViewModels.Room
{
    public class RoomVM
    {
        public int Id { get; set; }

        [Required]
        public string RoomNo { get; set; }

        [Required]
        [Range(1, 10)]
        public int AdultsCapacity { get; set; }

        [Range(0, 10)]
        public int ChildrenCapacity { get; set; }

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int RoomSize { get; set; }

        [Required(ErrorMessage = "Please select a room class")]
        public int SelectedRoomClassId { get; set; }

        public List<RoomImageVM> ExistingImages { get; set; } = new List<RoomImageVM>();

        public IFormFileCollection RoomImages { get; set; }
    }
}
