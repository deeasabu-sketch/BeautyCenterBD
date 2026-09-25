using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // Admin-side product management (create/edit/delete the catalog).
    // The public-facing product browsing lives in HomeController/Shop.
    public class ProductController : InventoryBaseController
    {
        [RequirePermission("Products", PermissionAction.View)]
       
        public async Task<ActionResult> Index()
        {
            var products = await db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();

            return View(products);
        }

        [RequirePermission("Products", PermissionAction.View)]
        public ActionResult Details(int id)
        {
            // Removed StockQuantity and DiscountedPrice from .Include() 
            // since they are scalar/primitive properties, not navigation properties.
            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        [RequirePermission("Products", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new ProductViewModel { IsActive = true };
            LoadDropdowns(model);
            return View(model);
        }

        [RequirePermission("Products", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            var product = new Product
            {
                ProductName = model.ProductName.Trim(),
                SKU = model.SKU,
                CategoryId = model.CategoryId,
                BrandId = model.BrandId,
                UnitId = model.UnitId,
                PurchasePrice = model.PurchasePrice,
                UnitPrice = model.UnitPrice,
                DiscountPercent = model.DiscountPercent,
                ReorderLevel = model.ReorderLevel,
                Description = model.Description,
                StockQuantity = model.StockQuantity,
                IsActive = model.IsActive,
                CreateDate = DateTime.Now,
                ImagePath = SaveImage(model.CoverImage, "~/Content/Products")
            };

            db.Products.Add(product);
            db.SaveChanges();

            if (model.GalleryImages != null)
            {
                foreach (var file in model.GalleryImages.Where(f => f != null && f.ContentLength > 0))
                {
                    string path = SaveImage(file, "~/Content/Products");

                    if (path != null)
                    {
                        db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImagePath = path
                        });
                    }
                }

                db.SaveChanges();
            }

            TempData["Success"] = "Product added successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Products", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                UnitId = product.UnitId,
                StockQuantity=product.StockQuantity,
                PurchasePrice = product.PurchasePrice,
                UnitPrice = product.UnitPrice,
                DiscountPercent = product.DiscountPercent,
                ReorderLevel = product.ReorderLevel,
                Description = product.Description,
                IsActive = product.IsActive,
                ExistingImagePath = product.ImagePath,
                ExistingGalleryImages = product.ProductImages
                    .Select(pi => new ExistingGalleryImage { ProductImageId = pi.ProductImageId, ImagePath = pi.ImagePath })
                    .ToList()
            };

            LoadDropdowns(model);
            return View(model);
        }

        [RequirePermission("Products", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            var product = db.Products.Find(model.ProductId);

            if (product == null)
            {
                return HttpNotFound();
            }

            product.ProductName = model.ProductName.Trim();
            product.SKU = model.SKU;
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;
            product.UnitId = model.UnitId;
            product.StockQuantity = model.StockQuantity;
            product.PurchasePrice = model.PurchasePrice;
            product.UnitPrice = model.UnitPrice;
            product.DiscountPercent = model.DiscountPercent;
            product.ReorderLevel = model.ReorderLevel;
            product.Description = model.Description;
            product.IsActive = model.IsActive;

            if (model.CoverImage != null && model.CoverImage.ContentLength > 0)
            {
                string newPath = SaveImage(model.CoverImage, "~/Content/Products");

                if (newPath != null)
                {
                    DeletePhysicalFile(product.ImagePath);
                    product.ImagePath = newPath;
                }
            }

            db.SaveChanges();

            if (model.GalleryImages != null)
            {
                foreach (var file in model.GalleryImages.Where(f => f != null && f.ContentLength > 0))
                {
                    string path = SaveImage(file, "~/Content/Products");

                    if (path != null)
                    {
                        db.ProductImages.Add(new ProductImage { ProductId = product.ProductId, ImagePath = path });
                    }
                }

                db.SaveChanges();
            }

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Products", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveGalleryImage(int productImageId, int productId)
        {
            var image = db.ProductImages.Find(productImageId);

            if (image != null)
            {
                DeletePhysicalFile(image.ImagePath);
                db.ProductImages.Remove(image);
                db.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = productId });
        }

        [RequirePermission("Products", PermissionAction.Delete)]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        [RequirePermission("Products", PermissionAction.Delete)]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            bool hasOrders = db.OrderItems.Any(oi => oi.ProductId == id);
            bool hasSales = db.SaleDetails.Any(sd => sd.ProductId == id);

            if (hasOrders || hasSales)
            {
                TempData["Error"] = "This product has order/sale history and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            DeletePhysicalFile(product.ImagePath);

            foreach (var img in product.ProductImages)
            {
                DeletePhysicalFile(img.ImagePath);
            }

            db.Products.Remove(product);
            db.SaveChanges();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }

        private void LoadDropdowns(ProductViewModel model)
        {
            model.CategoryOptions = db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name }).ToList();

            model.BrandOptions = db.Brands.Where(b => b.IsActive).OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.BrandId.ToString(), Text = b.Name }).ToList();

            model.UnitOptions = db.Units.Where(u => u.IsActive).OrderBy(u => u.Name)
                .Select(u => new SelectListItem { Value = u.UnitId.ToString(), Text = u.Name + " (" + u.ShortCode + ")" }).ToList();
        }

        private string SaveImage(System.Web.HttpPostedFileBase file, string folder)
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

            string folderPath = Server.MapPath(folder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = Guid.NewGuid().ToString("N") + extension;
            file.SaveAs(Path.Combine(folderPath, fileName));

            return folder + "/" + fileName;
        }

        private void DeletePhysicalFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            string fullPath = Server.MapPath(relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
