
using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CustomAuthentication.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();


        // =====================================================
        // REGISTER - GET
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check Email Already Exists
            bool exists = db.Users.Any(x => x.Email == model.Email);

            if (exists)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email Already Registered"
                );

                return View(model);
            }

            // Create New User
            User user = new User
            {
                FullName = model.FullName,
                Phone = model.Phone,
                Email = model.Email,

                // Password Hash
                PasswordHash =
                    PasswordHelper.HashPassword(model.Password),

                // New Registration = Customer
                RoleName = UserRoles.Customer,

                // Account Active
                IsActive = true,

                // Created Date
                CreateDate = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login", "Account");
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Find Active User
            User u = db.Users.FirstOrDefault(x =>
    x.Email == vm.Email &&
    x.IsActive
);

            // Invited staff must complete password setup before their first login.
            if (u != null && !u.PasswordSetupCompletedAt.HasValue && u.PasswordSetupExpiresAt.HasValue)
            {
                ModelState.AddModelError("", "Please set your password using the invitation email before logging in.");
                return View(vm);
            }

            // User Not Found / Password Wrong
            if (u == null ||
                !PasswordHelper.VerifyPassword(
                    vm.Password,
                    u.PasswordHash))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password."
                );

                return View(vm);
            }


            // =================================================
            // CREATE AUTHENTICATION TICKET
            // =================================================

            FormsAuthenticationTicket ticket =
                new FormsAuthenticationTicket(
                    1,
                    u.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    vm.RemeberMe,
                    u.RoleName
                );


            // Encrypt Ticket
            string encryptedTicket =
                FormsAuthentication.Encrypt(ticket);


            // Authentication Cookie
            HttpCookie cookie =
                new HttpCookie(
                    FormsAuthentication.FormsCookieName,
                    encryptedTicket
                );

            cookie.HttpOnly = true;

            Response.Cookies.Add(cookie);


            // =================================================
            // ROLE BASED REDIRECT
            // =================================================

            if (u.RoleName == UserRoles.Admin)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }

            if (u.RoleName == UserRoles.Employee)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Employee"
                );
            }

            if (u.RoleName == UserRoles.Accountant)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Accountant"
                );
            }

            // All inventory/staff roles use the permission-aware admin dashboard.
            if (UserRoles.AllInventoryRoles.Contains(u.RoleName))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }

            // Customer
            return RedirectToAction(
                "Profile",
                "Account"
            );
        }


        // =====================================================
        // PASSWORD SETUP FROM ADMIN INVITATION
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public ActionResult SetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.InvitationError = "This password setup link is invalid.";
                return View();
            }

            string tokenHash = InvitationHelper.HashToken(token);
            var user = db.Users.FirstOrDefault(u => u.PasswordSetupTokenHash == tokenHash);

            if (user == null || user.PasswordSetupCompletedAt.HasValue)
            {
                ViewBag.InvitationError = "This password setup link is invalid or has already been used.";
                return View();
            }

            if (!user.PasswordSetupExpiresAt.HasValue || user.PasswordSetupExpiresAt.Value < DateTime.Now)
            {
                ViewBag.InvitationError = "This password setup link has expired. Please ask the administrator to resend the invitation.";
                ViewBag.ExpiredEmail = user.Email;
                return View();
            }

            ViewBag.Token = token;
            ViewBag.FullName = user.FullName;
            ViewBag.ExpiresAt = user.PasswordSetupExpiresAt.Value;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPassword(string token, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.InvitationError = "This password setup link is invalid.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ViewBag.InvitationError = "Password must be at least 6 characters.";
                ViewBag.Token = token;
                return View();
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ViewBag.InvitationError = "Passwords do not match.";
                ViewBag.Token = token;
                return View();
            }

            string tokenHash = InvitationHelper.HashToken(token);
            var user = db.Users.FirstOrDefault(u => u.PasswordSetupTokenHash == tokenHash);

            if (user == null || user.PasswordSetupCompletedAt.HasValue)
            {
                ViewBag.InvitationError = "This password setup link is invalid or has already been used.";
                return View();
            }

            if (!user.PasswordSetupExpiresAt.HasValue || user.PasswordSetupExpiresAt.Value < DateTime.Now)
            {
                ViewBag.InvitationError = "This password setup link has expired. Please ask the administrator to resend the invitation.";
                return View();
            }

            user.PasswordHash = PasswordHelper.HashPassword(password);
            user.PasswordSetupCompletedAt = DateTime.Now;
            user.PasswordSetupTokenHash = null;
            user.PasswordSetupExpiresAt = null;
            user.InvitationReminderSentAt = null;
            db.SaveChanges();

            TempData["Success"] = "Your password has been set successfully. You can now log in.";
            return RedirectToAction("Login", "Account");
        }

        // =====================================================
        // PROFILE
        // =====================================================

        [Authorize]
        [HttpGet]
        public ActionResult Profile()
        {
            string email = User.Identity.Name;

            User user = db.Users.FirstOrDefault(
                x => x.Email == email
            );

            if (user == null)
            {
                return HttpNotFound();
            }

            ProfileViewModel model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.RoleName,
                IsActive = user.IsActive,
                ImagePath = user.ImagePath
            };

            return View(model);
        }


        // =====================================================
        // UPLOAD PROFILE PICTURE
        // =====================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadProfilePicture(HttpPostedFileBase profileImage)
        {
            string email = User.Identity.Name;

            User user = db.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return HttpNotFound();
            }

            if (profileImage != null && profileImage.ContentLength > 0)
            {
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                string extension = Path.GetExtension(profileImage.FileName)?.ToLower();

                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only JPG, JPEG, PNG, GIF and WEBP images are allowed.";
                    return RedirectToAction("Profile");
                }

                if (profileImage.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "Image size must be less than 5 MB.";
                    return RedirectToAction("Profile");
                }

                string folderPath = Server.MapPath("~/Content/Profiles");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Delete old image
                if (!string.IsNullOrEmpty(user.ImagePath))
                {
                    string oldImagePath = Server.MapPath(user.ImagePath);

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                string fileName = Guid.NewGuid().ToString("N") + extension;
                string filePath = Path.Combine(folderPath, fileName);

                profileImage.SaveAs(filePath);

                user.ImagePath = "~/Content/Profiles/" + fileName;

                db.SaveChanges();

                TempData["Success"] = "Profile picture updated successfully.";
            }

            return RedirectToAction("Profile");
        }


        // =====================================================
        // ACCESS DENIED
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public ActionResult AccessDenied()
        {
            return View();
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [Authorize]
        [HttpGet]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();

            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Login",
                "Account"
            );
        }

    }
 }

