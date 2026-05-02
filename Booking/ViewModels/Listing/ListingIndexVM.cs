namespace Booking.ViewModels.Listing
{
    public class ListingIndexVM
    {
        // The list of listings to display.
        public IEnumerable<Booking.Models.Listing> Listings { get; set; }

        // Pagination info.
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Filter parameters.
        public string ListingTypeFilter { get; set; }
        public int? CityIdFilter { get; set; }
        public decimal? MinPriceFilter { get; set; }
        public decimal? MaxPriceFilter { get; set; }
        public int? RatingFilter { get; set; }
        public int? MinAvgRoomSizeFilter { get; set; }
        public int? MaxAvgRoomSizeFilter { get; set; }
        public int[] RoomClassIdsFilter { get; set; }
    }
}
