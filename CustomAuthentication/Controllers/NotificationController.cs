using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: Notification
        public ActionResult Index()
        {
            var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);

            if (user == null)
                return HttpNotFound();

            var notifications = db.Notifications
                .Where(x => x.UserId == user.UserId)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return View(notifications);
        }

        // Latest notifications for navbar
        [HttpGet]
        public JsonResult Latest()
        {
            var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    unreadCount = 0,
                    notifications = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

            var notifications = db.Notifications
                .Where(x => x.UserId == user.UserId)
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .Select(x => new
                {
                    id = x.NotificationId,
                    title = x.Title,
                    message = x.Message,
                    type = x.Type,
                    url = x.Url,
                    isRead = x.IsRead,
                    createdDate = x.CreatedDate
                })
                .ToList();

            int unreadCount = db.Notifications
                .Count(x => x.UserId == user.UserId && !x.IsRead);

            return Json(new
            {
                success = true,
                unreadCount = unreadCount,
                notifications = notifications
            }, JsonRequestBehavior.AllowGet);
        }

        // Mark notification as read
        [HttpPost]
        public JsonResult MarkAsRead(int id)
        {
            var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);

            if (user == null)
                return Json(new { success = false });

            var notification = db.Notifications
                .FirstOrDefault(x =>
                    x.NotificationId == id &&
                    x.UserId == user.UserId);

            if (notification == null)
                return Json(new { success = false });

            notification.IsRead = true;

            db.SaveChanges();

            return Json(new { success = true });
        }

        // Mark all notifications as read
        [HttpPost]
        public JsonResult MarkAllAsRead()
        {
            var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);

            if (user == null)
                return Json(new { success = false });

            var notifications = db.Notifications
                .Where(x => x.UserId == user.UserId && !x.IsRead)
                .ToList();

            foreach (var item in notifications)
            {
                item.IsRead = true;
            }

            db.SaveChanges();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}