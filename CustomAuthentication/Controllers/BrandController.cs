using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class BrandController : InventoryBaseController
    {
        [RequirePermission("Brands", PermissionAction.View)]
        public ActionResult Index()
        {
            var brands = db.Brands
                .OrderBy(b => b.Name)
                .ToList();

            return View(brands);
        }

        [RequirePermission("Brands", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Brand { IsActive = true });
        }

        [RequirePermission("Brands", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Brand model, HttpPostedFileBase brandImage)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Brand name is required.");
            }
            else if (db.Brands.Any(b => b.Name == model.Name.Trim()))
            {
                ModelState.AddModelError("Name", "A brand with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();
            model.ImagePath = SaveBrandImage(brandImage);

            db.Brands.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Brand added successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Brands", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var brand = db.Brands.Find(id);

            if (brand == null)
            {
                return HttpNotFound();
            }

            return View(brand);
        }

        [RequirePermission("Brands", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Brand model, HttpPostedFileBase brandImage)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Brand name is required.");
            }
            else if (db.Brands.Any(b => b.Name == model.Name.Trim() && b.BrandId != model.BrandId))
            {
                ModelState.AddModelError("Name", "A brand with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var brand = db.Brands.Find(model.BrandId);

            if (brand == null)
            {
                return HttpNotFound();
            }

            brand.Name = model.Name.Trim();
            brand.Description = model.Description;
            brand.IsActive = model.IsActive;

            if (brandImage != null && brandImage.ContentLength > 0)
            {
                brand.ImagePath = SaveBrandImage(brandImage);
            }

            db.SaveChanges();

            TempData["Success"] = "Brand updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Brands", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var brand = db.Brands.Find(id);

            if (brand == null)
            {
                return HttpNotFound();
            }

            bool inUse = db.Products.Any(p => p.BrandId == id);

            if (inUse)
            {
                TempData["Error"] = "This brand is used by one or more products and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            db.Brands.Remove(brand);
            db.SaveChanges();

            TempData["Success"] = "Brand deleted successfully.";
            return RedirectToAction("Index");
        }

        private string SaveBrandImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return null;
            }

            string[] allowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string extension = Path.GetExtension(file.FileName)?.ToLower();

            if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension))
            {
                return null;
            }

            string folderPath = Server.MapPath("~/Content/Brands");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = Guid.NewGuid().ToString("N") + extension;
            file.SaveAs(Path.Combine(folderPath, fileName));

            return "~/Content/Brands/" + fileName;
        }
    }
}
