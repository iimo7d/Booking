using System.Diagnostics;
using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly Context db;

        public HomeController(ILogger<HomeController> logger, Context db)
        {
            _logger = logger;
            this.db = db;
        }

        public IActionResult Index()
        {
            ViewBag.Countries = new SelectList(db.Countries, "Id", "Name");
            ViewBag.Types = new SelectList(db.ListingTypes, "Id", "Name");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
