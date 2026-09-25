
using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // ==========================================
        // CURRENT USER
        // ==========================================
        private User GetCurrentUser()
        {
            return db.Users
                .FirstOrDefault(x => x.Email == User.Identity.Name);
        }


        // ==========================================
        // MESSENGER MAIN PAGE
        // ==========================================
        [HttpGet]
        public ActionResult Index()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return HttpNotFound();

            return View(currentUser);
        }


        // ==========================================
        // SEARCH USERS
        // ==========================================
        [HttpGet]
        public JsonResult SearchUsers(string term)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return Json(new
                {
                    success = false,
                    users = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

            term = (term ?? "").Trim();

            var users = db.Users
                .Where(x => x.UserId != currentUser.UserId)
                .Where(x =>
                    string.IsNullOrEmpty(term) ||
                    x.FullName.Contains(term) ||
                    x.Email.Contains(term))
                .OrderBy(x => x.FullName)
                .Take(20)
                .Select(x => new
                {
                    id = x.UserId,
                    name = x.FullName,
                    email = x.Email,
                    role = x.RoleName,
                    image = x.ImagePath
                })
                .ToList();

            return Json(new
            {
                success = true,
                users = users
            }, JsonRequestBehavior.AllowGet);
        }


        // ==========================================
        // ALL USERS
        // ==========================================
        [HttpGet]
        public JsonResult Users()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return Json(new
                {
                    success = false,
                    users = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

            var users = db.Users
                .Where(x => x.UserId != currentUser.UserId)
                .OrderBy(x => x.FullName)
                .Select(x => new
                {
                    id = x.UserId,
                    name = x.FullName,
                    email = x.Email,
                    role = x.RoleName,
                    image = x.ImagePath
                })
                .ToList();

            return Json(new
            {
                success = true,
                users = users
            }, JsonRequestBehavior.AllowGet);
        }


        // ==========================================
        // GET CONVERSATION
        // ==========================================
        [HttpGet]
        public JsonResult Conversation(int userId)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return Json(new
                {
                    success = false,
                    messages = new object[0]
                }, JsonRequestBehavior.AllowGet);
            }

            // Cannot message yourself
            if (currentUser.UserId == userId)
            {
                return Json(new
                {
                    success = false,
                    messages = new object[0],
                    message = "You cannot open a conversation with yourself."
                }, JsonRequestBehavior.AllowGet);
            }

            var otherUser = db.Users
                .FirstOrDefault(x => x.UserId == userId);

            if (otherUser == null)
            {
                return Json(new
                {
                    success = false,
                    messages = new object[0],
                    message = "User not found."
                }, JsonRequestBehavior.AllowGet);
            }


            // ==========================================
            // MARK INCOMING MESSAGES AS READ
            // ==========================================
            var unreadMessages = db.Messages
                .Where(x =>
                    x.SenderId == userId &&
                    x.ReceiverId == currentUser.UserId &&
                    !x.IsRead)
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Any())
            {
                db.SaveChanges();
            }


            // ==========================================
            // LOAD CONVERSATION AFTER READ UPDATE
            // ==========================================
            var messages = db.Messages
                .Where(x =>
                    (x.SenderId == currentUser.UserId &&
                     x.ReceiverId == userId)
                    ||
                    (x.SenderId == userId &&
                     x.ReceiverId == currentUser.UserId)
                )
                .OrderBy(x => x.CreatedDate)
                .Select(x => new
                {
                    id = x.MessageId,
                    senderId = x.SenderId,
                    receiverId = x.ReceiverId,
                    message = x.MessageText,
                    isRead = x.IsRead,
                    createdDate = x.CreatedDate
                })
                .ToList();


            return Json(new
            {
                success = true,

                currentUserId = currentUser.UserId,

                user = new
                {
                    id = otherUser.UserId,
                    name = otherUser.FullName,
                    email = otherUser.Email,
                    role = otherUser.RoleName,
                    image = otherUser.ImagePath
                },

                messages = messages
            }, JsonRequestBehavior.AllowGet);
        }


        // ==========================================
        // SEND MESSAGE
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Send(int receiverId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return Json(new
                {
                    success = false,
                    message = "Message cannot be empty."
                });
            }


            var sender = GetCurrentUser();

            if (sender == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sender not found."
                });
            }


            // Prevent sending message to yourself
            if (sender.UserId == receiverId)
            {
                return Json(new
                {
                    success = false,
                    message = "You cannot send a message to yourself."
                });
            }


            var receiver = db.Users
                .FirstOrDefault(x => x.UserId == receiverId);

            if (receiver == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Receiver not found."
                });
            }


            var text = messageText.Trim();

            // Optional maximum message length
            if (text.Length > 2000)
            {
                return Json(new
                {
                    success = false,
                    message = "Message cannot exceed 2000 characters."
                });
            }


            var newMessage = new Message
            {
                SenderId = sender.UserId,
                ReceiverId = receiver.UserId,
                MessageText = text,
                IsRead = false,
                CreatedDate = DateTime.Now
            };


            db.Messages.Add(newMessage);
            db.SaveChanges();


            return Json(new
            {
                success = true,
                message = "Message sent successfully.",

                data = new
                {
                    id = newMessage.MessageId,
                    senderId = newMessage.SenderId,
                    receiverId = newMessage.ReceiverId,
                    message = newMessage.MessageText,
                    isRead = newMessage.IsRead,
                    createdDate = newMessage.CreatedDate
                }
            });
        }


        // ==========================================
        // UNREAD MESSAGE COUNT
        // ==========================================
        [HttpGet]
        public JsonResult UnreadCount()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return Json(new
                {
                    success = false,
                    count = 0
                }, JsonRequestBehavior.AllowGet);
            }


            var count = db.Messages.Count(x =>
                x.ReceiverId == currentUser.UserId &&
                !x.IsRead);


            return Json(new
            {
                success = true,
                count = count
            }, JsonRequestBehavior.AllowGet);
        }


        // ==========================================
        // MARK ONE MESSAGE AS READ
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MarkAsRead(int id)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return Json(new { success = false });


            var message = db.Messages
                .FirstOrDefault(x =>
                    x.MessageId == id &&
                    x.ReceiverId == currentUser.UserId);


            if (message == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Message not found."
                });
            }


            if (!message.IsRead)
            {
                message.IsRead = true;
                db.SaveChanges();
            }


            return Json(new
            {
                success = true
            });
        }


        // ==========================================
        // MARK ALL MESSAGES AS READ
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MarkAllAsRead()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return Json(new { success = false });


            var messages = db.Messages
                .Where(x =>
                    x.ReceiverId == currentUser.UserId &&
                    !x.IsRead)
                .ToList();


            foreach (var message in messages)
            {
                message.IsRead = true;
            }


            if (messages.Any())
            {
                db.SaveChanges();
            }


            return Json(new
            {
                success = true,
                count = messages.Count
            });
        }


        // ==========================================
        // DISPOSE
        // ==========================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

