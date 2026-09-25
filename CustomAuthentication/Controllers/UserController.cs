using CustomAuthentication.Filters;
using CustomAuthentication.Helpers;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System.Data.Entity;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // Admin/HR-only staff account management (Admin, HR, CentralManager,
    // BranchManager, StoreManager, SalesPerson, WarehouseManager).
    // Distinct from Customer accounts, who self-register through
    // /Account/Register and are managed via the Shop/Order flow instead.
    // This controller's Index view is exposed at "/User" so it's what
    // an admin sees after logging in and opening "Users" - as requested.
    public class UserController : InventoryBaseController
    {
        [RequirePermission("Users", PermissionAction.View)]
        public ActionResult Index()
        {
            SendExpiredInvitationReminders();

            var users = db.Users
                .Include(u => u.Warehouse)
                .Where(u => u.RoleName != UserRoles.Customer)
                .OrderBy(u => u.FullName)
                .ToList();

            return View(users);
        }

        [RequirePermission("Users", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new UserManagementViewModel { IsActive = true };
            LoadDropdowns(model);
            return View(model);
        }

        [RequirePermission("Users", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserManagementViewModel model)
        {
            // The invited user creates their password from the secure email link.
            ModelState.Remove("Password");

            if (!string.IsNullOrWhiteSpace(model.Email) && db.Users.Any(u => u.Email == model.Email.Trim()))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            bool needsWarehouse = UserRoles.WarehouseScopedRoles.Contains(model.RoleName);

            if (needsWarehouse && model.WarehouseId == null)
            {
                ModelState.AddModelError("WarehouseId", "Please select a warehouse for this role.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            var adminEmail = User.Identity.Name;
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                TempData["Error"] = "Could not determine the logged-in administrator email.";
                return RedirectToAction("Index");
            }

            var token = InvitationHelper.CreateToken();
            var expiresAt = DateTime.Now.AddHours(InvitationHelper.ExpiryHours);

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Phone = model.Phone.Trim(),
                Email = model.Email.Trim(),
                // Random initial hash is intentionally unusable because the user must set a password first.
                PasswordHash = PasswordHelper.HashPassword(Guid.NewGuid().ToString("N") + "!InvitationOnly"),
                RoleName = model.RoleName,
                WarehouseId = needsWarehouse ? model.WarehouseId : null,
                IsActive = model.IsActive,
                CreateDate = DateTime.Now,
                PasswordSetupTokenHash = InvitationHelper.HashToken(token),
                PasswordSetupExpiresAt = expiresAt,
                PasswordSetupCompletedAt = null,
                InvitationSentAt = null,
                InvitationReminderSentAt = null
            };

            db.Users.Add(user);
            db.SaveChanges();

            try
            {
                var setupUrl = InvitationHelper.BuildSetupUrl(Url, token);
                EmailService.Send(
                    user.Email,
                    "Set your BeautyCenterBD account password",
                    InvitationHelper.BuildSetupEmail(user.FullName, user.RoleName, setupUrl, expiresAt));

                user.InvitationSentAt = DateTime.Now;
                db.SaveChanges();

                TempData["Success"] = "User created. A password setup email has been sent to " + user.Email + ". The link is valid for " + InvitationHelper.ExpiryHours + " hours.";
            }
            catch (Exception ex)
            {
                // Keep the user so the admin can use Resend Invite after SMTP is configured/fixed.
                TempData["Error"] = "User was created, but the invitation email could not be sent. Please check SMTP settings and use Resend Invite. Details: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [RequirePermission("Users", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResendInvitation(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            if (user.PasswordSetupCompletedAt.HasValue)
            {
                TempData["Error"] = "This user has already set a password. A new invitation is not required.";
                return RedirectToAction("Index");
            }

            var token = InvitationHelper.CreateToken();
            user.PasswordSetupTokenHash = InvitationHelper.HashToken(token);
            user.PasswordSetupExpiresAt = DateTime.Now.AddHours(InvitationHelper.ExpiryHours);
            user.InvitationReminderSentAt = null;

            try
            {
                var setupUrl = InvitationHelper.BuildSetupUrl(Url, token);
                EmailService.Send(
                    user.Email,
                    "Your BeautyCenterBD password setup link",
                    InvitationHelper.BuildSetupEmail(user.FullName, user.RoleName, setupUrl, user.PasswordSetupExpiresAt.Value));

                user.InvitationSentAt = DateTime.Now;
                db.SaveChanges();
                TempData["Success"] = "A fresh password setup link has been sent to " + user.Email + ".";
            }
            catch (Exception ex)
            {
                db.SaveChanges();
                TempData["Error"] = "Could not send the invitation email. Please check SMTP settings. Details: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private void SendExpiredInvitationReminders()
        {
            var adminEmail = User.Identity.Name;
            if (string.IsNullOrWhiteSpace(adminEmail)) return;

            var expired = db.Users
                .Where(u => u.PasswordSetupExpiresAt.HasValue
                    && u.PasswordSetupExpiresAt.Value < DateTime.Now
                    && !u.PasswordSetupCompletedAt.HasValue
                    && !u.InvitationReminderSentAt.HasValue)
                .ToList();

            foreach (var user in expired)
            {
                try
                {
                    EmailService.Send(
                        adminEmail,
                        "BeautyCenterBD invitation expired: " + user.FullName,
                        InvitationHelper.BuildAdminExpiryEmail(user.FullName, user.Email, user.RoleName, user.PasswordSetupExpiresAt.Value));

                    user.InvitationReminderSentAt = DateTime.Now;
                    db.SaveChanges();
                }
                catch
                {
                    // Do not mark the reminder as sent when SMTP fails; it can retry on the next admin visit.
                }
            }
        }

        [RequirePermission("Users", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id = 0)
        {
            if (id <= 0)
            {
                TempData["Error"] = "No user selected to edit.";
                return RedirectToAction("Index");
            }

            var user = db.Users.Find(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            var model = new UserManagementViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                RoleName = user.RoleName,
                WarehouseId = user.WarehouseId,
                IsActive = user.IsActive,

                CanViewDashboard = user.CanViewDashboard,

                CanViewProducts = user.CanViewProducts,
                CanAddProducts = user.CanAddProducts,
                CanEditProducts = user.CanEditProducts,
                CanDeleteProducts = user.CanDeleteProducts,

                CanViewOrders = user.CanViewOrders,
                CanAddOrders = user.CanAddOrders,
                CanEditOrders = user.CanEditOrders,
                CanDeleteOrders = user.CanDeleteOrders,

                CanViewReports = user.CanViewReports,

                CanManageUsers = user.CanManageUsers,

                CanViewNotebook = user.CanViewNotebook,
                CanAddNotebook = user.CanAddNotebook,
                CanEditNotebook = user.CanEditNotebook,
                CanDeleteNotebook = user.CanDeleteNotebook,
                CanManagePromoCodes = user.CanManagePromoCodes
            };

            LoadDropdowns(model);
            return View(model);
        }

        [RequirePermission("Users", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserManagementViewModel model)
        {
            if (db.Users.Any(u => u.Email == model.Email.Trim() && u.UserId != model.UserId))
            {
                ModelState.AddModelError("Email", "This email is already registered to another user.");
            }

            bool needsWarehouse = UserRoles.WarehouseScopedRoles.Contains(model.RoleName);

            if (needsWarehouse && model.WarehouseId == null)
            {
                ModelState.AddModelError("WarehouseId", "Please select a warehouse for this role.");
            }

            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            var user = db.Users.Find(model.UserId);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.FullName = model.FullName.Trim();
            user.Phone = model.Phone.Trim();
            user.Email = model.Email.Trim();
            user.RoleName = model.RoleName;
            user.WarehouseId = needsWarehouse ? model.WarehouseId : null;
            user.IsActive = model.IsActive;

            user.CanViewDashboard = model.CanViewDashboard;

            user.CanViewProducts = model.CanViewProducts;
            user.CanAddProducts = model.CanAddProducts;
            user.CanEditProducts = model.CanEditProducts;
            user.CanDeleteProducts = model.CanDeleteProducts;

            user.CanViewOrders = model.CanViewOrders;
            user.CanAddOrders = model.CanAddOrders;
            user.CanEditOrders = model.CanEditOrders;
            user.CanDeleteOrders = model.CanDeleteOrders;

            user.CanViewReports = model.CanViewReports;

            user.CanManageUsers = model.CanManageUsers;

            user.CanViewNotebook = model.CanViewNotebook;
            user.CanAddNotebook = model.CanAddNotebook;
            user.CanEditNotebook = model.CanEditNotebook;
            user.CanDeleteNotebook = model.CanDeleteNotebook;
            user.CanManagePromoCodes = model.CanManagePromoCodes;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = PasswordHelper.HashPassword(model.Password);
            }

            db.SaveChanges();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Users", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.Email == User.Identity.Name)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            db.SaveChanges();

            TempData["Success"] = user.IsActive ? "User activated." : "User deactivated.";
            return RedirectToAction("Index");
        }

        private void LoadDropdowns(UserManagementViewModel model)
        {
            model.RoleOptions = UserRoles.AllInventoryRoles
                .Select(r => new SelectListItem { Value = r, Text = r })
                .ToList();

            model.WarehouseOptions = db.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.WarehouseId.ToString(), Text = w.Name })
                .ToList();
        }
    }
}
