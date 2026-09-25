using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // In-store / POS sale (as opposed to the public web Shop, which
    // creates Orders instead). Decreases stock immediately.
    public class SaleController : InventoryBaseController
    {
        [RequirePermission("Sales", PermissionAction.View)]
        public ActionResult Index()
        {
            var query = db.Sales
                .Include(s => s.Customer)
                .Include(s => s.Warehouse)
                .AsQueryable();

            query = ScopeToWarehouse(query, s => s.WarehouseId);

            var sales = query
                .OrderByDescending(s => s.SaleId)
                .ToList();

            return View(sales);
        }

        [RequirePermission("Sales", PermissionAction.View)]
        public ActionResult Details(int id)
        {
            var sale = db.Sales
                .Include(s => s.Customer)
                .Include(s => s.Warehouse)
                .Include(s => s.SaleDetails.Select(d => d.Product))
                .FirstOrDefault(s => s.SaleId == id);

            if (sale == null)
            {
                return HttpNotFound();
            }

            return View(sale);
        }

        [RequirePermission("Sales", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new SaleViewModel
            {
                SaleDate = DateTime.Now,
                Status = "Completed",
                WarehouseId = CurrentWarehouseId ?? 0
            };

            LoadDropdowns(model);

            return View(model);
        }

        [RequirePermission("Sales", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SaleViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "Add at least one product line before saving.");
            }

            // Overselling guard - check stock is sufficient for every line.
            if (model.Items != null)
            {
                foreach (var item in model.Items)
                {
                    var product = db.Products.Find(item.ProductId);

                    if (product == null)
                    {
                        ModelState.AddModelError("", "One of the selected products no longer exists.");
                        continue;
                    }

                    if (item.Quantity > product.StockQuantity)
                    {
                        ModelState.AddModelError(
                            "",
                            $"Not enough stock for {product.ProductName}. Available: {product.StockQuantity}, requested: {item.Quantity}.");
                    }
                }
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
                    decimal subTotal = model.Items.Sum(i => i.Quantity * i.UnitPrice);
                    decimal total = subTotal - model.Discount + model.Tax;

                    var sale = new Sale
                    {
                        SaleNumber = "SALE-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        CustomerId = model.CustomerId,
                        WarehouseId = model.WarehouseId,
                        SaleDate = model.SaleDate,
                        SubTotal = subTotal,
                        Discount = model.Discount,
                        Tax = model.Tax,
                        TotalAmount = total,
                        Status = "Completed",
                        CreatedByUserId = CurrentUser?.UserId ?? 0,
                        IsActive = true
                    };

                    db.Sales.Add(sale);
                    db.SaveChanges();

                    foreach (var item in model.Items)
                    {
                        var product = db.Products.Find(item.ProductId);

                        if (product == null)
                        {
                            continue;
                        }

                        db.SaleDetails.Add(new SaleDetail
                        {
                            SaleId = sale.SaleId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            LineTotal = item.Quantity * item.UnitPrice
                        });

                        product.StockQuantity -= item.Quantity;

                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = model.WarehouseId,
                            TransactionType = "Sale",
                            QuantityChange = -item.Quantity,
                            ReferenceId = sale.SaleId,
                            ReferenceType = "Sale",
                            TransactionDate = DateTime.Now,
                            CreatedByUserId = CurrentUser?.UserId ?? 0
                        });
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Sale recorded and stock updated successfully.";
                    return RedirectToAction("Details", new { id = sale.SaleId });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Something went wrong while saving the sale. Please try again.");
                    LoadDropdowns(model);
                    return View(model);
                }
            }
        }

        [RequirePermission("Sales", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var sale = db.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefault(s => s.SaleId == id);

            if (sale == null)
            {
                return HttpNotFound();
            }

            if (sale.Status == "Cancelled")
            {
                TempData["Error"] = "This sale is already cancelled.";
                return RedirectToAction("Details", new { id });
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var detail in sale.SaleDetails)
                    {
                        var product = db.Products.Find(detail.ProductId);

                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }

                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = sale.WarehouseId,
                            TransactionType = "Adjustment",
                            QuantityChange = detail.Quantity,
                            ReferenceId = sale.SaleId,
                            ReferenceType = "Sale",
                            TransactionDate = DateTime.Now,
                            CreatedByUserId = CurrentUser?.UserId ?? 0
                        });
                    }

                    sale.Status = "Cancelled";
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Sale cancelled and stock restored.";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Could not cancel this sale. Please try again.";
                }
            }

            return RedirectToAction("Details", new { id });
        }

        private void LoadDropdowns(SaleViewModel model)
        {
            model.CustomerOptions = db.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CustomerId.ToString(), Text = c.Name })
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
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductName + " (Stock: " + p.StockQuantity + ")"
                })
                .ToList();
        }
    }
}
