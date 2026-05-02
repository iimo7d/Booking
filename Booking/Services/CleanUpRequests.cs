using Booking.Data;
using Booking.Enums;
using Booking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Booking.Services
{
    public class CleanUpRequests : IJob
    {

        private readonly Context _db;
        private readonly UserManager<AppUser> _userManager;

        public CleanUpRequests(Context db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cutoffDate = DateTime.Now.AddDays(-30);

            var pendingRequests = await _db.JoinUs.Include(u => u.Agent)
                 .Where(j => j.Status == Status.Pending && j.DateOfSubmission < cutoffDate)
                .ToListAsync();

            foreach (var request in pendingRequests)
            {
                var user = await _userManager.FindByIdAsync(request.Agent.Id);
                if (user != null)
                {
                    //sendmessage
                }

                if (request.Listing != null)
                {
                    _db.Listings.Remove(request.Listing);
                }

                _db.JoinUs.Remove(request);
            }

            await _db.SaveChangesAsync();
        }
    }
}
