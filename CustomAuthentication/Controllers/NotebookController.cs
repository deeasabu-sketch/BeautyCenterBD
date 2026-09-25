using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize]
    public class NotebookController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private int? CurrentUserId()
        {
            var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            return user?.UserId;
        }

        // GET: Notebook/List
        [HttpGet]
        public JsonResult List()
        {
            var userId = CurrentUserId();

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." }, JsonRequestBehavior.AllowGet);
            }

            var notes = db.NotebookEntries
     .Where(x => x.UserId == userId.Value)
     .OrderBy(x => x.CreateDate)
     .Select(x => new
     {
         x.NoteId,
         x.NoteText,
         x.CreateDate
     })
     .ToList()
     .Select(x => new
     {
         x.NoteId,
         x.NoteText,
         CreateDate = x.CreateDate.ToString("dd MMM, hh:mm tt")
     })
     .ToList();

            return Json(new { success = true, notes = notes }, JsonRequestBehavior.AllowGet);
        }

        // POST: Notebook/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Json(new { success = false, message = "Note text is required." });
            }

            var userId = CurrentUserId();

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var note = new NotebookEntry
            {
                UserId = userId.Value,
                NoteText = text.Trim(),
                CreateDate = DateTime.Now
            };

            db.NotebookEntries.Add(note);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                note = new
                {
                    note.NoteId,
                    note.NoteText,
                    CreateDate = note.CreateDate.ToString("dd MMM, hh:mm tt")
                }
            });
        }

        // POST: Notebook/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            var userId = CurrentUserId();

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var note = db.NotebookEntries.FirstOrDefault(x => x.NoteId == id && x.UserId == userId.Value);

            if (note == null)
            {
                return Json(new { success = false, message = "Note not found." });
            }

            db.NotebookEntries.Remove(note);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
