using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Booking.Models;
using Booking.Data; 

public class EditButtonViewComponent : ViewComponent
{
    private readonly Context db;
    private readonly UserManager<AppUser> userManager;

    public EditButtonViewComponent(Context db, UserManager<AppUser> userManager)
    {
        this.userManager = userManager;
        this.db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return View(null);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return View(null);
        }

        var listing = await db.Listings.FirstOrDefaultAsync(x => x.AgentId == user.Id);
        return View(listing);
    }

}

