using Booking.Data;
using Booking.Models;
using Booking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Text;
namespace Booking.Controllers
{
    public class AccountController : Controller
    {
        private readonly Context db;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IWebHostEnvironment env;
        private readonly IEmailSender emailSender;

        public AccountController(Context db, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IWebHostEnvironment env, IEmailSender emailSender)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
            this.emailSender = emailSender;
        }



        #region Register 
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM register, string returnUrl = null)
        {
            AppUser identityUser = new AppUser
            {
                UserName = register.Email,
                Email = register.Email,
                FirstName = register.FirstName,
                LastName = register.LastName
            };
            var identityResult = await userManager.CreateAsync(identityUser, register.Password);

            if (identityResult.Succeeded)
            {
                await userManager.AddToRoleAsync(identityUser, "User");
                await signInManager.SignInAsync(identityUser, false);
                string htmlMessage = @"
<html>
  <head>
    <meta charset='UTF-8'>
    <title>Registration Successful</title>
    <style>
      body { font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }
      .container { max-width: 600px; background-color: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }
      .header { background-color: #28a745; padding: 10px; color: white; text-align: center; border-radius: 8px 8px 0 0; }
      .content { padding: 20px; font-size: 16px; color: #333; }
      .button { display: inline-block; padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='header'>
        <h1>Registration Successful</h1>
      </div>
      <div class='content'>
        <p>Dear User,</p>
        <p>Congratulations! You have successfully registered on our platform. To get the best experience, please complete your profile details.</p>
        <p>Click the button below to update your profile:</p>
        <a href='https://localhost:7118/Account/EditProfile' class='button'>Complete Your Profile</a>
        <p>If you have any questions, feel free to contact our support team.</p>
        <p>Best regards,</p>
        <p><strong>Your Platform Team</strong></p>
      </div>
    </div>
  </body>
</html>";


                await emailSender.SendEmailAsync(identityUser.Email, "Login Successful", htmlMessage);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var err in identityResult.Errors)
                {
                    ModelState.AddModelError(err.Code, err.Description);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(register);
        }
        #endregion

        #region Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.LoginIdentifier)
                           ?? await userManager.FindByNameAsync(model.LoginIdentifier);

                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {

                        await emailSender.SendEmailAsync(model.LoginIdentifier, "Login Successful", @"
    <html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; }
            .container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }
            .header { background-color: #007bff; color: white; padding: 10px; text-align: center; border-radius: 8px 8px 0 0; }
            .content { padding: 20px; }
            .footer { margin-top: 20px; font-size: 12px; color: #666; text-align: center; }
            .btn { display: inline-block; background-color: #28a745; color: white; padding: 10px 15px; text-decoration: none; border-radius: 5px; }
            .btn:hover { background-color: #218838; }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h2>Login Successful</h2>
            </div>
            <div class='content'>
                <p>Hello,</p>
                <p>You have successfully logged into your account on <strong>{DateTime.Now:F}</strong>.</p>
                <p>If this wasn't you, please reset your password immediately by clicking the button below:</p>
                <p><a href='' class='btn'>Reset Password</a></p>
            </div>
            <div class='footer'>
                <p>If you did not attempt to log in, please contact our support team.</p>
                <p>&copy;  YourCompanyName. All Rights Reserved.</p>
            </div>
        </div>
    </body>
    </html>
");



                        return LocalRedirect(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        ViewData["ErrorMessage"] = "Your account is locked due to multiple failed login attempts. Please try again later.";
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No user found with this username/email.");
                }
            }


   

            

            return View(model);
        }

        #endregion


        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }




        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileVM
            {
                Profile = new EditProfile
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DOB = user.DOB,
                    AvatarUrl = user.AvatarUrl,
                    CityId = user.CityId
                },
                 


                Password = new PasswordVM()
            };

            var selectedCity = user.CityId.HasValue
                ? await db.Cities.FirstOrDefaultAsync(c => c.Id == user.CityId.Value)
                : null;
            model.Profile.CountryId = selectedCity?.CountryId ?? 0;
            await PopulateProfileSelectionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                await PopulateProfileSelectionsAsync(model);
                return View(model);
            }

            // Update the user's profile data.
            user.FirstName = model.Profile.FirstName;
            user.LastName = model.Profile.LastName;
            user.DOB = model.Profile.DOB;
            user.Email = model.Profile.Email;
            user.UserName = model.Profile.Email; 
            user.CityId = model.Profile.CityId;

            // Handle the avatar image upload.
            if (model.Profile.AvatarImage != null && model.Profile.AvatarImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(model.Profile.AvatarImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension) || model.Profile.AvatarImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Profile.AvatarImage", "Avatar must be a JPG, JPEG, or PNG image up to 5MB.");
                    await PopulateProfileSelectionsAsync(model);
                    return View(model);
                }

                // Define the folder to save uploaded images.
                string uploadsFolder = Path.Combine(env.WebRootPath, "assets", "images", "avatar");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name.
                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file.
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Profile.AvatarImage.CopyToAsync(fileStream);
                }

                // Update the user's avatar URL (relative path).
                user.AvatarUrl = $"/assets/images/avatar/{uniqueFileName}";
            }

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await emailSender.SendEmailAsync(user.Email, "Profile Updated", "Your profile has been updated successfully.");
                TempData["Success"] = "Your profile has been updated successfully.";
                return RedirectToAction("EditProfile");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                await PopulateProfileSelectionsAsync(model);
                return View(model);
            }

        }




        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(PasswordVM model)
        {

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await emailSender.SendEmailAsync(user.Email, "Password Changed", "Your password has been changed successfully.");
                TempData["PasswordSuccess"] = "Your password has been changed successfully.";
            }
            else
            {
                TempData["PasswordError"] = "Failed to change password: " + string.Join(" ", result.Errors);
            }

            return RedirectToAction("EditProfile");
        }





        private async Task PopulateProfileSelectionsAsync(ProfileVM model)
        {
            var countries = await db.Countries.ToListAsync();
            ViewBag.Countries = new SelectList(countries, "Id", "Name", model.Profile.CountryId);

            var cities = model.Profile.CountryId > 0
                ? await db.Cities.Where(c => c.CountryId == model.Profile.CountryId).ToListAsync()
                : new List<City>();

            ViewBag.Cities = new SelectList(cities, "Id", "Name", model.Profile.CityId);
        }

        [HttpGet]
        public JsonResult GetCities(int countryId)
        {
            var cities = db.Cities.Where(c => c.CountryId == countryId)
                                  .Select(c => new { c.Id, c.Name })
                                  .ToList();
            return Json(cities);
        }

    }
}
