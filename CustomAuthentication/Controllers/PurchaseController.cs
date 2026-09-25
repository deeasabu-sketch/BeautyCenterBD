using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class PurchaseController : InventoryBaseController
    {
        // =========================
        // LIST
        // =========================
        [RequirePermission("Purchases", PermissionAction.View)]
        public ActionResult Index()
        {
            var query = db.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .AsQueryable();

            query = ScopeToWarehouse(query, p => p.WarehouseId);

            var purchases = query
                .OrderByDescending(p => p.PurchaseId)
                .ToList();

            return View(purchases);
        }

        // =========================
        // DETAILS
        // =========================
        [RequirePermission("Purchases", PermissionAction.View)]
        public ActionResult Details(int id)
        {
            var purchase = db.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .Include(p => p.PurchaseDetails.Select(d => d.Product))
                .FirstOrDefault(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return HttpNotFound();
            }

            return View(purchase);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [RequirePermission("Purchases", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new PurchaseViewModel
            {
                PurchaseDate = DateTime.Now,
                Status = "Pending"
            };

            LoadDropdowns(model);

            return View(model);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [RequirePermission("Purchases", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "Add at least one product line before saving.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal subTotal = model.Items.Sum(i => i.Quantity * i.UnitCost);
                    decimal total = subTotal - model.Discount + model.Tax;

                    var purchase = new Purchase
                    {
                        PurchaseNumber = "PUR-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        SupplierId = model.SupplierId,
                        WarehouseId = model.WarehouseId,
                        PurchaseDate = model.PurchaseDate,
                        SubTotal = subTotal,
                        Discount = model.Discount,
                        Tax = model.Tax,
                        TotalAmount = total,
                        Status = "Received",
                        CreatedByUserId = CurrentUser?.UserId ?? 0,
                        IsActive = true
                    };

                    db.Purchases.Add(purchase);
                    db.SaveChanges();

                    foreach (var item in model.Items)
                    {
                        var product = db.Products.Find(item.ProductId);

                        if (product == null)
                        {
                            continue;
                        }

                        db.PurchaseDetails.Add(new PurchaseDetail
                        {
                            PurchaseId = purchase.PurchaseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitCost = item.UnitCost,
                            LineTotal = item.Quantity * item.UnitCost
                        });

                        // Receiving stock immediately (Status = "Received").
                        product.StockQuantity += item.Quantity;

                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = model.WarehouseId,
                            TransactionType = "Purchase",
                            QuantityChange = item.Quantity,
                            ReferenceId = purchase.PurchaseId,
                            ReferenceType = "Purchase",
                            TransactionDate = DateTime.Now,
                            CreatedByUserId = CurrentUser?.UserId ?? 0
                        });
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Purchase recorded and stock updated successfully.";
                    return RedirectToAction("Details", new { id = purchase.PurchaseId });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Something went wrong while saving the purchase. Please try again.");
                    LoadDropdowns(model);
                    return View(model);
                }
            }
        }

        // =========================
        // CANCEL (reverses stock if it was Received)
        // =========================
        [RequirePermission("Purchases", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var purchase = db.Purchases
                .Include(p => p.PurchaseDetails)
                .FirstOrDefault(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return HttpNotFound();
            }

            if (purchase.Status == "Cancelled")
            {
                TempData["Error"] = "This purchase is already cancelled.";
                return RedirectToAction("Details", new { id });
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (purchase.Status == "Received")
                    {
                        foreach (var detail in purchase.PurchaseDetails)
                        {
                            var product = db.Products.Find(detail.ProductId);

                            if (product != null)
                            {
                                product.StockQuantity -= detail.Quantity;

                                if (product.StockQuantity < 0)
                                {
                                    product.StockQuantity = 0;
                                }
                            }

                            db.StockTransactions.Add(new StockTransaction
                            {
                                ProductId = detail.ProductId,
                                WarehouseId = purchase.WarehouseId,
                                TransactionType = "Adjustment",
                                QuantityChange = -detail.Quantity,
                                ReferenceId = purchase.PurchaseId,
                                ReferenceType = "Purchase",
                                TransactionDate = DateTime.Now,
                                CreatedByUserId = CurrentUser?.UserId ?? 0
                            });
                        }
                    }

                    purchase.Status = "Cancelled";
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Purchase cancelled and stock reversed.";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Could not cancel this purchase. Please try again.";
                }
            }

            return RedirectToAction("Details", new { id });
        }

        private void LoadDropdowns(PurchaseViewModel model)
        {
            model.SupplierOptions = db.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.Name })
                .ToList();

            var warehouseQuery = db.Warehouses.Where(w => w.IsActive).AsQueryable();
            warehouseQuery = ScopeToWarehouse(warehouseQuery, w => w.WarehouseId);

            model.WarehouseOptions = warehouseQuery
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.WarehouseId.ToString(), Text = w.Name })
                .ToList();

            model.ProductOptions = db.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new SelectListItem { Value = p.ProductId.ToString(), Text = p.ProductName })
                .ToList();
        }
    }
}
