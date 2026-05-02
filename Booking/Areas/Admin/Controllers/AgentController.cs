using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booking.Models;
using System.Security.Claims;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AgentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public AgentController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            
            var users = await _userManager.Users.ToListAsync();

          
            var agents = new List<AppUser>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Agent"))
                {
                    agents.Add(user);
                }
            }

            return View(agents);
        }
        public async Task<IActionResult> Details(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var agent = await _userManager.Users
                .Include(u => u.Listings) 
                    .ThenInclude(l => l.Images) 
                .Include(u => u.Listings)
                    .ThenInclude(l => l.City) 
                .FirstOrDefaultAsync(u => u.Id == id);

            if (agent == null)
            {
                return NotFound();
            }

            return View(agent);
        }



    }
}
