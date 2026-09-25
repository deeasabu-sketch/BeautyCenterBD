using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // Workflow: Requested (by BranchManager) -> Approved/Rejected (by
    // source-side StoreManager) -> Sent (source WarehouseManager, stock
    // leaves FromWarehouse) -> Received (destination WarehouseManager,
    // stock arrives at ToWarehouse).
    public class StockTransferController : InventoryBaseController
    {
        [RequirePermission("StockTransfers", PermissionAction.View)]
        public ActionResult Index()
        {
            var query = db.StockTransfers
                .Include(t => t.FromWarehouse)
                .Include(t => t.ToWarehouse)
                .AsQueryable();

            // A warehouse-scoped user should see transfers touching
            // their warehouse on either side (incoming or outgoing).
            if (!IsUnrestricted && CurrentWarehouseId != null)
            {
                int wid = CurrentWarehouseId.Value;
                query = query.Where(t => t.FromWarehouseId == wid || t.ToWarehouseId == wid);
            }

            var transfers = query
                .OrderByDescending(t => t.StockTransferId)
                .ToList();

            return View(transfers);
        }

        [RequirePermission("StockTransfers", PermissionAction.View)]
        public ActionResult Details(int id)
        {
            var transfer = db.StockTransfers
                .Include(t => t.FromWarehouse)
                .Include(t => t.ToWarehouse)
                .Include(t => t.StockTransferDetails.Select(d => d.Product))
                .FirstOrDefault(t => t.StockTransferId == id);

            if (transfer == null)
            {
                return HttpNotFound();
            }

            return View(transfer);
        }

        // =========================
        // STEP 1: REQUEST
        // =========================
        [RequirePermission("StockTransfers", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new StockTransferCreateViewModel();

            if (CurrentWarehouseId != null)
            {
                model.FromWarehouseId = CurrentWarehouseId.Value;
            }

            LoadDropdowns(model);

            return View(model);
        }

        [RequirePermission("StockTransfers", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StockTransferCreateViewModel model)
        {
            if (model.FromWarehouseId == model.ToWarehouseId)
            {
                ModelState.AddModelError("", "Source and destination warehouse must be different.");
            }

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "Add at least one product line before saving.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            var transfer = new StockTransfer
            {
                TransferNumber = "TRF-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                FromWarehouseId = model.FromWarehouseId,
                ToWarehouseId = model.ToWarehouseId,
                TransferDate = DateTime.Now,
                Status = "Requested",
                RequestedByUserId = CurrentUser?.UserId ?? 0,
                RequestedDate = DateTime.Now,
                IsActive = true
            };

            db.StockTransfers.Add(transfer);
            db.SaveChanges();

            foreach (var item in model.Items)
            {
                db.StockTransferDetails.Add(new StockTransferDetail
                {
                    StockTransferId = transfer.StockTransferId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            db.SaveChanges();

            TempData["Success"] = "Transfer request submitted. Waiting for approval.";
            return RedirectToAction("Details", new { id = transfer.StockTransferId });
        }

        // =========================
        // STEP 2: APPROVE / REJECT
        // =========================
        [RequirePermission("StockTransfers", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var transfer = db.StockTransfers.Find(id);

            if (transfer == null)
            {
                return HttpNotFound();
            }

            if (transfer.Status != "Requested")
            {
                TempData["Error"] = "Only a pending request can be approved.";
                return RedirectToAction("Details", new { id });
            }

            transfer.Status = "Approved";
            transfer.ApprovedByUserId = CurrentUser?.UserId ?? 0;
            transfer.ApprovedDate = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Transfer approved. It can now be sent.";
            return RedirectToAction("Details", new { id });
        }

        [RequirePermission("StockTransfers", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id, string reason)
        {
            var transfer = db.StockTransfers.Find(id);

            if (transfer == null)
            {
                return HttpNotFound();
            }

            if (transfer.Status != "Requested")
            {
                TempData["Error"] = "Only a pending request can be rejected.";
                return RedirectToAction("Details", new { id });
            }

            transfer.Status = "Rejected";
            transfer.ApprovedByUserId = CurrentUser?.UserId ?? 0;
            transfer.ApprovedDate = DateTime.Now;
            transfer.RejectionReason = reason;

            db.SaveChanges();

            TempData["Success"] = "Transfer request rejected.";
            return RedirectToAction("Details", new { id });
        }

        // =========================
        // STEP 3: SEND (stock leaves FromWarehouse)
        // =========================
        [RequirePermission("StockTransfers", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send(int id)
        {
            var transfer = db.StockTransfers
                .Include(t => t.StockTransferDetails)
                .FirstOrDefault(t => t.StockTransferId == id);

            if (transfer == null)
            {
                return HttpNotFound();
            }

            if (transfer.Status != "Approved")
            {
                TempData["Error"] = "Only an approved transfer can be sent.";
                return RedirectToAction("Details", new { id });
            }

            foreach (var detail in transfer.StockTransferDetails)
            {
                var product = db.Products.Find(detail.ProductId);

                if (product != null && detail.Quantity > product.StockQuantity)
                {
                    TempData["Error"] = $"Not enough stock of {product.ProductName} at the source warehouse to send this transfer.";
                    return RedirectToAction("Details", new { id });
                }
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var detail in transfer.StockTransferDetails)
                    {
                        var product = db.Products.Find(detail.ProductId);

                        if (product != null)
                        {
                            product.StockQuantity -= detail.Quantity;
                        }

                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = transfer.FromWarehouseId,
                            TransactionType = "TransferOut",
                            QuantityChange = -detail.Quantity,
                            ReferenceId = transfer.StockTransferId,
                            ReferenceType = "StockTransfer",
                            TransactionDate = DateTime.Now,
                            CreatedByUserId = CurrentUser?.UserId ?? 0
                        });
                    }

                    transfer.Status = "Sent";
                    transfer.SentByUserId = CurrentUser?.UserId ?? 0;
                    transfer.SentDate = DateTime.Now;

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Transfer marked as sent. Stock deducted from the source warehouse.";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Could not mark this transfer as sent. Please try again.";
                }
            }

            return RedirectToAction("Details", new { id });
        }

        // =========================
        // STEP 4: RECEIVE (stock arrives at ToWarehouse)
        // =========================
        [RequirePermission("StockTransfers", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Receive(int id)
        {
            var transfer = db.StockTransfers
                .Include(t => t.StockTransferDetails)
                .FirstOrDefault(t => t.StockTransferId == id);

            if (transfer == null)
            {
                return HttpNotFound();
            }

            if (transfer.Status != "Sent")
            {
                TempData["Error"] = "Only a sent transfer can be received.";
                return RedirectToAction("Details", new { id });
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var detail in transfer.StockTransferDetails)
                    {
                        var product = db.Products.Find(detail.ProductId);

                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }

                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = transfer.ToWarehouseId,
                            TransactionType = "TransferIn",
                            QuantityChange = detail.Quantity,
                            ReferenceId = transfer.StockTransferId,
                            ReferenceType = "StockTransfer",
                            TransactionDate = DateTime.Now,
                            CreatedByUserId = CurrentUser?.UserId ?? 0
                        });
                    }

                    transfer.Status = "Received";
                    transfer.ReceivedByUserId = CurrentUser?.UserId ?? 0;
                    transfer.ReceivedDate = DateTime.Now;

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Transfer received. Stock added to the destination warehouse.";
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Could not mark this transfer as received. Please try again.";
                }
            }

            return RedirectToAction("Details", new { id });
        }

        // =========================
        // CANCEL (only before it's Sent)
        // =========================
        [RequirePermission("StockTransfers", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var transfer = db.StockTransfers.Find(id);

            if (transfer == null)
            {
                return HttpNotFound();
            }

            if (transfer.Status == "Sent" || transfer.Status == "Received")
            {
                TempData["Error"] = "A transfer that has already been sent cannot be cancelled.";
                return RedirectToAction("Details", new { id });
            }

            transfer.Status = "Cancelled";
            db.SaveChanges();

            TempData["Success"] = "Transfer request cancelled.";
            return RedirectToAction("Details", new { id });
        }

        private void LoadDropdowns(StockTransferCreateViewModel model)
        {
            model.WarehouseOptions = db.Warehouses
                .Where(w => w.IsActive)
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
