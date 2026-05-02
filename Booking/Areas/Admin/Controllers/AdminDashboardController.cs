using Booking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly Context db;

        public AdminDashboardController(Context db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {

            #region rooms
            var bookedRooms = db.Rooms.Where(r => r.IsBooked == false).Count();
            var totalRooms = db.Rooms.Count();
            var freeRooms = totalRooms - bookedRooms;

            ViewBag.BookedRooms = bookedRooms;
            ViewBag.FreeRooms = freeRooms;
            #endregion

            #region Activity
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-6);

            int totalDays = (endDate - startDate).Days + 1;

            // Prepare arrays to store daily counts
            var checkInCounts = new int[totalDays];
            var checkOutCounts = new int[totalDays];
            var labels = new string[totalDays];

            for (int i = 0; i < totalDays; i++)
            {
                var day = startDate.AddDays(i);

                checkInCounts[i] = db.Bookings
                    .Count(b => b.CheckInDate.Date == day.Date);

                checkOutCounts[i] = db.Bookings
                    .Count(b => b.CheckOutDate.Date == day.Date);

                labels[i] = day.ToString("ddd");
            }

            ViewBag.LabelsJson = JsonConvert.SerializeObject(labels);
            ViewBag.CheckInJson = JsonConvert.SerializeObject(checkInCounts);
            ViewBag.CheckOutJson = JsonConvert.SerializeObject(checkOutCounts);
            #endregion

            return View();
        }
      
    }
}
