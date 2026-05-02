namespace Booking.Areas.Admin.ViewModels
{
    public class RecentUserViewModel
    {
        public string?  UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime JoinDate { get; set; }
        public string? City { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string>? Roles { get; set; } 
    }
}
