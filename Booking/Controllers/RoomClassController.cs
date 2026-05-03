using Booking.Data;
using Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using Booking.ViewModels.Listing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Booking.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Booking.ViewComponents;

namespace Booking.Controllers
{
    [Authorize]
    public class RoomClassController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailSender emailSender;

        public RoomClassController(Context db, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            this.db = db;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var roomClasses = await db.RoomClasses
                .Include(rc => rc.Rooms)
                .ToListAsync();
            return View(roomClasses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RoomClass roomClass)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid room class data." });
            }

            await db.RoomClasses.AddAsync(roomClass);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Room class created successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, RoomClass model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid room class data." });
            }

            var roomClass = await db.RoomClasses.FindAsync(id);
            if (roomClass == null)
            {
                return NotFound();
            }


            roomClass.Name = model.Name;
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Room class updated successfully." });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomClass = await db.RoomClasses.FindAsync(id);
            if (roomClass == null || id == 0)
            {
                return Json(new { success = false, message = "Room class not found." });
            }
            db.RoomClasses.Remove(roomClass);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Room class deleted successfully." });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Agent,User")]
        public async Task<IActionResult> MultiAssignAmenity(int listingId)
        {
            var listing = await db.Listings.FindAsync(listingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null || listing.AgentId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            var roomClasses = await db.RoomClasses
                .Where(rc => db.ListingRoomClasses.Any(lrc => lrc.RoomClassId == rc.Id && lrc.ListingId == listingId))
                .ToListAsync();

            var amenities = await db.Amenities.ToListAsync();

            var viewModel = new MultiRoomClassAmenityAssignmentVM
            {
                ListingId = listingId,
                AmenityList = amenities.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }),
                RoomClassAssignments = roomClasses.Select(rc => new RoomClassAmenityAssignmentVM
                {
                    RoomClassId = rc.Id,
                    RoomClassName = rc.Name,
                    SelectedAmenityIds = db.RoomClassAmenities
                                           .Where(r => r.RoomClassId == rc.Id && r.ListingId == listingId)
                                           .Select(r => r.AmenityId)
                                           .ToList()
                }).ToList()
            };

            ViewBag.ListingName = listing.Name;

            return View(viewModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Agent,User")]
        public async Task<IActionResult> MultiAssignAmenity(MultiRoomClassAmenityAssignmentVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var listing = await db.Listings.FindAsync(model.ListingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && listing.AgentId != user.Id)
            {
                return Forbid();
            }

            var validRoomClassIds = await db.ListingRoomClasses
                .Where(lrc => lrc.ListingId == model.ListingId)
                .Select(lrc => lrc.RoomClassId)
                .ToListAsync();

            foreach (var assignment in model.RoomClassAssignments.Where(a => validRoomClassIds.Contains(a.RoomClassId)))
            {
                var distinctAmenityIds = (assignment.SelectedAmenityIds ?? new List<int>()).Distinct().ToList();

                var currentAssignments = await db.RoomClassAmenities
                    .Where(r => r.RoomClassId == assignment.RoomClassId && r.ListingId == model.ListingId)
                    .ToListAsync();

                foreach (var existing in currentAssignments.ToList())
                {
                    if (!distinctAmenityIds.Contains(existing.AmenityId))
                    {
                        db.RoomClassAmenities.Remove(existing);
                        currentAssignments.Remove(existing);
                    }
                }

                foreach (var amenityId in distinctAmenityIds)
                {
                    if (!currentAssignments.Any(r => r.AmenityId == amenityId))
                    {
                        var newAssignment = new RoomClassAmenity
                        {
                            RoomClassId = assignment.RoomClassId,
                            AmenityId = amenityId,
                            ListingId = model.ListingId
                        };
                        db.RoomClassAmenities.Add(newAssignment);
                        currentAssignments.Add(newAssignment);
                    }
                }
            }

            await db.SaveChangesAsync();

            if (await userManager.IsInRoleAsync(user, "Agent"))
            {
                return RedirectToAction("Details", "Listing", new { id = model.ListingId });
            }
            else if (await userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("AddRooms", "Room", new { listingId = model.ListingId });
            }
            else
            {
                string emailBody = $@"
<html>
  <head>
    <meta charset='UTF-8'>
    <title>Request Sent</title>
    <style>
      body {{
        font-family: Arial, sans-serif;
        background-color: #f9f9f9;
        margin: 0;
        padding: 20px;
      }}
      .container {{
        max-width: 600px;
        margin: auto;
        background-color: #fff;
        padding: 20px;
        border: 1px solid #ddd;
      }}
      .header {{
        text-align: center;
        font-size: 24px;
        font-weight: bold;
        margin-bottom: 20px;
      }}
      .content {{
        font-size: 16px;
        line-height: 1.5;
      }}
      .footer {{
        font-size: 14px;
        color: #777;
        text-align: center;
        margin-top: 20px;
        border-top: 1px solid #ddd;
        padding-top: 10px;
      }}
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='header'>Request Sent</div>
      <div class='content'>
        <p>Dear {user.FirstName},</p>
        <p>Your request has been successfully submitted! ✅</p>
        <p>We have received your request and our team will review it shortly. You will be notified once we process your request.</p>
        <p><strong>📅 Submission Date:</strong> {DateTime.Now:dddd, MMMM dd, yyyy}</p>
        <p>If you have any questions, feel free to contact our support team.</p>
        <p>Best regards,<br>
           Booking<br>
           📞 Contact: +1 (234) 567-890<br>
           📧 support@yourplatform.com</p>
      </div>
      <div class='footer'>
        &copy; {DateTime.Now.Year} Booking. All rights reserved.
      </div>
    </div>
  </body>
</html>";

                await emailSender.SendEmailAsync(user.Email, "Request Sent", emailBody);
                TempData["Message"] = "Amenity assignments updated successfully!";
                return RedirectToAction("RequestSent", "JoinUs");
            }
        }

    }
}
