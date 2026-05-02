using Booking.Data;
using Booking.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Booking.ViewComponents
{
    [ViewComponent(Name = "RecentUser")]
    public class RecentUser : ViewComponent
    {
        private readonly Context db;

        public RecentUser(Context db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentUsers = await db.Users
       .OrderByDescending(u => u.JoinDate)
       .Take(6)
       .Select(u => new RecentUserViewModel
       {
           UserId = u.Id,
           FullName = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
           Email = u.Email,
           JoinDate = u.JoinDate,
           AvatarUrl = u.AvatarUrl,
           Roles = db.UserRoles
               .Where(ur => ur.UserId == u.Id)
               .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
               .ToList()
       })
       .ToListAsync();



            return View(recentUsers);
        }
    }
}
