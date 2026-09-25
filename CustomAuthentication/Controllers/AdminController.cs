using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using CustomAuthentication.Models;
using CustomAuthentication.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private readonly IOrderService orderService;
        private readonly IProductService productService;

        public AdminController()
        {
            orderService = new OrderService();
            productService = new ProductService();
        }

        // =========================================================
        // ORDER VIEW ACCESS
        // Admin can grant/revoke order VIEW permission
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult ToggleOrderAccess(int id)
        {
            try
            {
                var user = db.Users.Find(id);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index", "User");
                }

                if (user.RoleName == "Admin")
                {
                    TempData["Error"] =
                        "Admin already has full order access.";

                    return RedirectToAction("Index", "User");
                }

                user.CanViewOrders = !user.CanViewOrders;

                db.SaveChanges();

                TempData["Success"] = user.CanViewOrders
                    ? "Order view access granted to " + user.FullName + "."
                    : "Order view access revoked from " + user.FullName + ".";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Could not update order access: " + ex.Message;
            }

            return RedirectToAction("Index", "User");
        }


        // =========================================================
        // ORDER PERMISSION HELPER
        // =========================================================

        private bool CanViewOrders()
        {
            // Admin always has full access
            if (User.IsInRole("Admin"))
                return true;

            var user = db.Users.FirstOrDefault(
                x => x.Email == User.Identity.Name
            );

            return user != null && user.CanViewOrders;
        }


        // =========================================================
        // DASHBOARD
        // =========================================================

        [Authorize]
        public ActionResult Dashboard()
        {
            // =====================================================
            // PERMISSIONS FOR THIS VIEWER
            // =====================================================
            // Admin always sees everything (PermissionHelper short-
            // circuits to true for the Admin role). Every other staff
            // role only sees the sections their User.CanX flags allow.

            bool isAdmin = User.IsInRole(UserRoles.Admin);

            bool canManageUsers = PermissionHelper.HasPermission(db, User, "CanManageUsers");
            bool canViewOrders = PermissionHelper.HasPermission(db, User, "CanViewOrders");
            bool canViewProducts = PermissionHelper.HasPermission(db, User, "CanViewProducts");
            bool canViewReports = PermissionHelper.HasPermission(db, User, "CanViewReports");
            bool canViewNotebook = PermissionHelper.HasPermission(db, User, "CanViewNotebook");
            bool canManagePromoCodes = PermissionHelper.HasPermission(db, User, "CanManagePromoCodes");

            ViewBag.CanManageUsers = canManageUsers;
            ViewBag.CanViewOrders = canViewOrders;
            ViewBag.CanViewProducts = canViewProducts;
            ViewBag.CanViewReports = canViewReports;
            ViewBag.CanViewNotebook = canViewNotebook;
            ViewBag.CanManagePromoCodes = canManagePromoCodes;

            // Salary / Purchase / Supplier / Sale history and the
            // financial summary (spend, profit) are business-sensitive,
            // so they stay Admin + Reports-permission only.
            bool canViewFinancials = isAdmin || canViewReports;
            ViewBag.CanViewFinancials = canViewFinancials;

            // =====================================================
            // USER STATISTICS (Admin / user managers only)
            // =====================================================

            if (canManageUsers)
            {
                ViewBag.TotalUsers =
                    db.Users.Count();

                ViewBag.ActiveUsers =
                    db.Users.Count(x => x.IsActive);

                ViewBag.AdminUsers =
                    db.Users.Count(x => x.RoleName == "Admin");

                ViewBag.CustomerUsers =
                    db.Users.Count(x => x.RoleName == "Customer");
            }


            // =====================================================
            // PRODUCT STATISTICS
            // =====================================================

            if (canViewProducts)
            {
                ViewBag.TotalProducts =
                    db.Products.Count();

                ViewBag.ActiveProducts =
                    db.Products.Count(x => x.IsActive);

                ViewBag.InactiveProducts =
                    db.Products.Count(x => !x.IsActive);
            }


            // =====================================================
            // ORDER STATISTICS
            // =====================================================

            if (canViewOrders)
            {
                ViewBag.TotalOrders =
                    db.Orders.Count();

                ViewBag.TotalOrderAmount =
                    db.Orders
                        .Where(x => x.IsActive)
                        .Sum(x => (decimal?)x.TotalAmount) ?? 0m;
            }


            // =====================================================
            // FINANCIALS: SALARY / PURCHASE / SUPPLIER / SALE /
            // TOTAL SPENT / NET PROFIT
            // =====================================================
            // Business-sensitive numbers - Admin or explicit
            // "CanViewReports" permission only.

            if (canViewFinancials)
            {
                // Warehouse counts (shown on the Dashboard's Warehouse
                // Management card).
                ViewBag.TotalWarehouses =
                    db.Warehouses.Count();

                ViewBag.ActiveWarehouses =
                    db.Warehouses.Count(x => x.IsActive);

                // Only PAID and ACTIVE salaries are treated as actual
                // salary spending.
                ViewBag.TotalSalarySpent =
                    db.Salaries
                        .Where(x =>
                            x.IsActive &&
                            x.Status == "Paid")
                        .Sum(x => (decimal?)x.NetSalary) ?? 0m;

                // Only RECEIVED and ACTIVE purchases are counted.
                // Purchase.TotalAmount already contains SubTotal - Discount + Tax.
                ViewBag.TotalProductCostSpent =
                    db.Purchases
                        .Where(x =>
                            x.IsActive &&
                            x.Status == "Received")
                        .Sum(x => (decimal?)x.TotalAmount) ?? 0m;

                ViewBag.TotalPurchases =
                    db.Purchases.Count(x => x.IsActive);

                ViewBag.TotalSuppliers =
                    db.Suppliers.Count(x => x.IsActive);

                ViewBag.TotalSales =
                    db.Sales.Count(x =>
                        x.IsActive &&
                        x.Status == "Completed");

                ViewBag.TotalSaleAmount =
                    db.Sales
                        .Where(x =>
                            x.IsActive &&
                            x.Status == "Completed")
                        .Sum(x => (decimal?)x.TotalAmount) ?? 0m;

                // No separate Expense/OtherExpense model in the project yet,
                // so this intentionally stays 0 rather than guessing from NotebookEntry.
                ViewBag.OthersSpent = 0m;

                decimal totalSalarySpent = ViewBag.TotalSalarySpent;
                decimal totalProductCostSpent = ViewBag.TotalProductCostSpent;
                decimal othersSpent = ViewBag.OthersSpent;

                decimal totalSpent =
                    totalSalarySpent +
                    totalProductCostSpent +
                    othersSpent;

                ViewBag.TotalSpent = totalSpent;

                // Net Profit = Total Order Amount - Salary Spent - Product/Purchase Cost - Other Expenses
                decimal totalSales = canViewOrders ? (decimal)ViewBag.TotalOrderAmount : 0m;

                ViewBag.NetProfit = totalSales - totalSpent;
            }


            // =====================================================
            // RETURN
            // =====================================================

            return View();
        }


        // =========================================================
        // USERS
        // User management is now handled by UserController
        // =========================================================

        [Authorize(Roles = "Admin")]
        public ActionResult Users()
        {
            var users = db.Users
                .Include(u => u.Warehouse)
                .OrderBy(u => u.FullName)
                .Select(u => new ViewModel.AdminUserViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleName = u.RoleName,

                    WarehouseName =
                        u.Warehouse != null
                            ? u.Warehouse.Name
                            : null,

                    IsActive = u.IsActive,
                    CanViewOrders = u.CanViewOrders,
                    CreateDate = u.CreateDate
                })
                .ToList();

            return View(users);
        }


        // =========================================================
        // OLD MANAGE USERS URL
        // Redirect old /Admin/ManageUsers URL to /User/Index
        // =========================================================

        [Authorize(Roles = "Admin")]
        public ActionResult ManageUsers()
        {
            return RedirectToAction("Index", "User");
        }


        // =========================================================
        // OLD ADD USER URL
        // Redirect old /Admin/AddUser URL to /User/Create
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddUser()
        {
            return RedirectToAction("Create", "User");
        }


        // =========================================================
        // ORDERS
        // =========================================================

        [Authorize]
        public ActionResult Orders()
        {
            if (!CanViewOrders())
            {
                TempData["Error"] =
                    "You don't have permission to view orders.";

                return RedirectToAction("Index", "Home");
            }

            var orders = orderService.GetAllOrders();

            // Only Admin can manage orders
            ViewBag.CanManageOrders =
                User.IsInRole("Admin");

            return View(orders);
        }


        // =========================================================
        // ORDER DETAILS
        // =========================================================

        [Authorize]
        public ActionResult OrderDetails(int id)
        {
            if (!CanViewOrders())
            {
                TempData["Error"] =
                    "You don't have permission to view orders.";

                return RedirectToAction("Index", "Home");
            }

            var order = db.Orders
                .Include("OrderItems.Product")
                .FirstOrDefault(x => x.OrderId == id);

            if (order == null)
            {
                TempData["Error"] =
                    "Order not found.";

                return RedirectToAction("Orders");
            }

            ViewBag.CanManageOrders =
                User.IsInRole("Admin");

            return View(order);
        }


        // =========================================================
        // ORDER INVOICE
        // =========================================================

        [Authorize]
        public ActionResult Invoice(int id)
        {
            if (!CanViewOrders())
            {
                TempData["Error"] =
                    "You don't have permission to view orders.";

                return RedirectToAction("Index", "Home");
            }

            var order = db.Orders
                .Include("OrderItems.Product")
                .FirstOrDefault(x => x.OrderId == id);

            if (order == null)
            {
                TempData["Error"] =
                    "Order not found.";

                return RedirectToAction("Orders");
            }


            // -------------------------
            // Customer information
            // -------------------------

            var user = db.Users
                .FirstOrDefault(x =>
                    x.UserId == order.UserId
                );


            ViewBag.CustomerName =
                user != null
                    ? user.FullName
                    : "Customer";


            ViewBag.CustomerPhone =
                order.CustomerPhone
                ?? (user != null
                    ? user.Phone
                    : "");


            ViewBag.CustomerEmail =
                order.CustomerEmail
                ?? (user != null
                    ? user.Email
                    : "");


            ViewBag.CustomerAddress =
                order.CustomerAddress
                ?? (user != null
                    ? user.Address
                    : "");


            return View(order);
        }


        // =========================================================
        // ADD ORDER - GET
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddOrder()
        {
            var model = new Order
            {
                DeliveryArea =
                    Models.DeliveryAreas.Dhaka,

                DeliveryCharge =
                    Models.DeliveryAreas.DhakaCharge,

                OrderStatus = "Pending",

                IsActive = true
            };


            PopulateViewBags();


            return View(model);
        }


        // =========================================================
        // ADD ORDER - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult AddOrder(
            Order model,
            int[] ProductIds,
            int[] Quantities)
        {
            try
            {
                // =================================================
                // 1. BASIC VALIDATION
                // =================================================

                ModelState.Remove("OrderNumber");


                if (model.UserId <= 0)
                {
                    ModelState.AddModelError(
                        "UserId",
                        "Please select a customer."
                    );
                }


                // =================================================
                // 2. CUSTOMER
                // =================================================

                var customer = db.Users.FirstOrDefault(
                    x =>
                        x.UserId == model.UserId &&
                        x.IsActive
                );


                if (customer == null)
                {
                    ModelState.AddModelError(
                        "UserId",
                        "Selected customer was not found."
                    );
                }
                else
                {
                    // Customer snapshot

                    model.CustomerPhone =
                        customer.Phone;

                    model.CustomerEmail =
                        customer.Email;

                    model.CustomerAddress =
                        customer.Address;


                    // Shipping fallback

                    if (string.IsNullOrWhiteSpace(
                        model.ShippingName))
                    {
                        model.ShippingName =
                            customer.FullName;
                    }


                    if (string.IsNullOrWhiteSpace(
                        model.ShippingPhone))
                    {
                        model.ShippingPhone =
                            customer.Phone;
                    }


                    if (string.IsNullOrWhiteSpace(
                        model.ShippingAddress))
                    {
                        model.ShippingAddress =
                            customer.Address
                            ?? customer.FullName;
                    }
                }


                // =================================================
                // 3. ORDER STATUS
                // =================================================

                if (string.IsNullOrWhiteSpace(
                    model.OrderStatus))
                {
                    ModelState.AddModelError(
                        "OrderStatus",
                        "Please select order status."
                    );
                }


                // =================================================
                // 4. PRODUCTS
                // =================================================

                if (ProductIds == null ||
                    ProductIds.Length == 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Please add at least one product."
                    );
                }


                if (Quantities == null ||
                    Quantities.Length == 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Please enter product quantity."
                    );
                }


                if (ProductIds != null &&
                    Quantities != null &&
                    ProductIds.Length !=
                    Quantities.Length)
                {
                    ModelState.AddModelError(
                        "",
                        "Product and quantity information is invalid."
                    );
                }


                // Early validation

                if (!ModelState.IsValid)
                {
                    PopulateViewBags(model.UserId);

                    return View(model);
                }


                // =================================================
                // 5. DELIVERY
                // =================================================

                if (string.IsNullOrWhiteSpace(
                    model.DeliveryArea))
                {
                    model.DeliveryArea =
                        Models.DeliveryAreas.Dhaka;
                }


                model.DeliveryCharge =
                    Models.DeliveryAreas.GetCharge(
                        model.DeliveryArea
                    );


                // =================================================
                // 6. WAREHOUSE
                // =================================================

                var warehouse = db.Warehouses
                    .FirstOrDefault(w => w.IsActive);


                if (warehouse == null)
                {
                    ModelState.AddModelError(
                        "",
                        "No active warehouse found. Please create or activate a warehouse first."
                    );

                    PopulateViewBags(model.UserId);

                    return View(model);
                }


                model.WarehouseId =
                    warehouse.WarehouseId;


                // =================================================
                // 7. SUBTOTAL + STOCK CHECK
                // =================================================

                decimal subtotal = 0;


                for (int i = 0;
                     i < ProductIds.Length;
                     i++)
                {
                    int productId =
                        ProductIds[i];

                    int quantity =
                        Quantities[i];


                    if (quantity <= 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "Quantity must be greater than 0."
                        );

                        continue;
                    }


                    var product =
                        db.Products.FirstOrDefault(
                            x =>
                                x.ProductId == productId &&
                                x.IsActive
                        );


                    if (product == null)
                    {
                        ModelState.AddModelError(
                            "",
                            "Product not found."
                        );

                        continue;
                    }


                    if (quantity >
                        product.StockQuantity)
                    {
                        ModelState.AddModelError(
                            "",
                            product.ProductName +
                            " has only " +
                            product.StockQuantity +
                            " item(s) in stock."
                        );

                        continue;
                    }


                    subtotal +=
                        product.UnitPrice *
                        quantity;
                }


                if (!ModelState.IsValid)
                {
                    PopulateViewBags(model.UserId);

                    return View(model);
                }


                // =================================================
                // 8. DISCOUNT + TAX + TOTAL
                // =================================================

                decimal discount =
                    model.Discount < 0
                        ? 0
                        : model.Discount;


                decimal tax =
                    model.Tax < 0
                        ? 0
                        : model.Tax;


                if (discount > subtotal)
                {
                    ModelState.AddModelError(
                        "Discount",
                        "Discount cannot be greater than subtotal."
                    );

                    PopulateViewBags(model.UserId);

                    return View(model);
                }


                model.Discount =
                    discount;

                model.Tax =
                    tax;

                model.SubTotal =
                    subtotal;


                model.TotalAmount =
                    (subtotal - discount)
                    + tax
                    + model.DeliveryCharge;


                // =================================================
                // 9. SYSTEM META
                // =================================================

                model.OrderNumber =
                    "ORD-" +
                    DateTime.Now.ToString(
                        "yyyyMMddHHmmssfff"
                    );


                model.OrderDate =
                    DateTime.Now;


                model.IsActive =
                    true;


                // =================================================
                // 10. SAVE ORDER
                // =================================================

                db.Orders.Add(model);

                db.SaveChanges();


                // =================================================
                // 11. SAVE ORDER ITEMS + DEDUCT STOCK
                // =================================================

                for (int i = 0;
                     i < ProductIds.Length;
                     i++)
                {
                    int productId =
                        ProductIds[i];

                    int quantity =
                        Quantities[i];


                    var product =
                        db.Products.FirstOrDefault(
                            x =>
                                x.ProductId ==
                                productId &&
                                x.IsActive
                        );


                    if (product == null)
                        continue;


                    OrderItem item =
                        new OrderItem
                        {
                            OrderId =
                                model.OrderId,

                            ProductId =
                                product.ProductId,

                            Quantity =
                                quantity,

                            UnitPrice =
                                product.UnitPrice,

                            LineTotal =
                                product.UnitPrice * quantity
                        };


                    db.OrderItems.Add(item);


                    // Deduct stock

                    product.StockQuantity -=
                        quantity;
                }


                db.SaveChanges();


                TempData["Success"] =
                    "Order added successfully.";


                return RedirectToAction("Orders");
            }
            catch (Exception ex)
            {
                string errorMessage =
                    ex.Message;


                Exception inner =
                    ex.InnerException;


                while (inner != null)
                {
                    errorMessage +=
                        " | INNER: " +
                        inner.Message;

                    inner =
                        inner.InnerException;
                }


                ModelState.AddModelError(
                    "",
                    "Order could not be added. Error: " +
                    errorMessage
                );


                PopulateViewBags(model.UserId);


                return View(model);
            }
        }


        // =========================================================
        // POPULATE ADD ORDER VIEWBAGS
        // =========================================================

        private void PopulateViewBags(
            int selectedUserId = 0)
        {
            ViewBag.Users =
                new SelectList(
                    db.Users
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.FullName)
                        .ToList(),

                    "UserId",
                    "FullName",

                    selectedUserId > 0
                        ? (object)selectedUserId
                        : null
                );


            ViewBag.Products =
                db.Products
                    .Where(x =>
                        x.IsActive &&
                        x.StockQuantity > 0)
                    .OrderBy(x => x.ProductName)
                    .ToList();
        }


        // =========================================================
        // EDIT ORDER - GET
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditOrder(int id)
        {
            var order =
                db.Orders
                    .Include("OrderItems.Product")
                    .FirstOrDefault(
                        x => x.OrderId == id
                    );


            if (order == null)
            {
                TempData["Error"] =
                    "Order not found.";

                return RedirectToAction("Orders");
            }


            ViewBag.Customers =
                db.Users
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.FullName)
                    .ToList();


            return View(order);
        }


        // =========================================================
        // EDIT ORDER - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult EditOrder(Order model)
        {
            try
            {
                ModelState.Remove("OrderNumber");
                ModelState.Remove("OrderDate");


                // =================================================
                // VALIDATION
                // =================================================

                if (model.Discount < 0)
                {
                    ModelState.AddModelError(
                        "Discount",
                        "Discount cannot be negative."
                    );
                }


                if (model.Tax < 0)
                {
                    ModelState.AddModelError(
                        "Tax",
                        "Tax cannot be negative."
                    );
                }


                if (string.IsNullOrWhiteSpace(
                    model.OrderStatus))
                {
                    ModelState.AddModelError(
                        "OrderStatus",
                        "Please select order status."
                    );
                }


                // =================================================
                // CUSTOMER
                // =================================================

                var selectedCustomer =
                    db.Users.FirstOrDefault(
                        x =>
                            x.UserId ==
                            model.UserId &&

                            x.IsActive
                    );


                if (selectedCustomer == null)
                {
                    ModelState.AddModelError(
                        "UserId",
                        "Please select a valid customer."
                    );
                }


                if (!ModelState.IsValid)
                {
                    ViewBag.Customers =
                        db.Users
                            .Where(x => x.IsActive)
                            .OrderBy(x => x.FullName)
                            .ToList();

                    return View(model);
                }


                // =================================================
                // EXISTING ORDER
                // =================================================

                var existingOrder =
                    db.Orders
                        .Include(
                            "OrderItems.Product"
                        )
                        .FirstOrDefault(
                            x =>
                                x.OrderId ==
                                model.OrderId
                        );


                if (existingOrder == null)
                {
                    TempData["Error"] =
                        "Order not found.";

                    return RedirectToAction(
                        "Orders"
                    );
                }


                // =================================================
                // SUBTOTAL
                // =================================================

                decimal subtotal =
                    existingOrder.OrderItems != null
                        ? existingOrder.OrderItems.Sum(
                            x =>
                                x.Quantity *
                                x.UnitPrice)
                        : 0;


                if (model.Discount > subtotal)
                {
                    ModelState.AddModelError(
                        "Discount",
                        "Discount cannot be greater than subtotal."
                    );


                    ViewBag.Customers =
                        db.Users
                            .Where(x => x.IsActive)
                            .OrderBy(x => x.FullName)
                            .ToList();


                    return View(model);
                }


                // =================================================
                // CUSTOMER
                // =================================================

                existingOrder.UserId =
                    selectedCustomer.UserId;


                existingOrder.CustomerPhone =
                    selectedCustomer.Phone;


                existingOrder.CustomerEmail =
                    selectedCustomer.Email;


                existingOrder.CustomerAddress =
                    selectedCustomer.Address;


                // =================================================
                // SHIPPING SNAPSHOT
                // =================================================

                if (!string.IsNullOrWhiteSpace(
                    model.ShippingName))
                {
                    existingOrder.ShippingName =
                        model.ShippingName;
                }


                if (!string.IsNullOrWhiteSpace(
                    model.ShippingPhone))
                {
                    existingOrder.ShippingPhone =
                        model.ShippingPhone;
                }


                if (!string.IsNullOrWhiteSpace(
                    model.ShippingAddress))
                {
                    existingOrder.ShippingAddress =
                        model.ShippingAddress;
                }


                // =================================================
                // DELIVERY
                // =================================================

                if (!string.IsNullOrWhiteSpace(
                    model.DeliveryArea))
                {
                    existingOrder.DeliveryArea =
                        model.DeliveryArea;


                    existingOrder.DeliveryCharge =
                        Models.DeliveryAreas.GetCharge(
                            model.DeliveryArea
                        );
                }


                // =================================================
                // DISCOUNT + TAX
                // =================================================

                existingOrder.Discount =
                    model.Discount;


                existingOrder.Tax =
                    model.Tax;


                // =================================================
                // TOTAL
                // =================================================

                existingOrder.SubTotal =
                    subtotal;


                existingOrder.TotalAmount =
                    subtotal
                    - existingOrder.Discount
                    + existingOrder.Tax
                    + existingOrder.DeliveryCharge;


                // =================================================
                // STATUS
                // =================================================

                existingOrder.OrderStatus =
                    model.OrderStatus;


                existingOrder.IsActive =
                    model.IsActive;


                db.SaveChanges();


                TempData["Success"] =
                    "Order updated successfully.";


                return RedirectToAction(
                    "OrderDetails",
                    new
                    {
                        id =
                            existingOrder.OrderId
                    }
                );
            }
            catch (Exception ex)
            {
                string errorMessage =
                    ex.Message;


                Exception inner =
                    ex.InnerException;


                while (inner != null)
                {
                    errorMessage +=
                        " | INNER: " +
                        inner.Message;

                    inner =
                        inner.InnerException;
                }


                ModelState.AddModelError(
                    "",
                    "Order could not be updated. Error: " +
                    errorMessage
                );


                ViewBag.Customers =
                    db.Users
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.FullName)
                        .ToList();


                return View(model);
            }
        }


        // =========================================================
        // DELETE ORDER
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteOrder(int id)
        {
            try
            {
                var order =
                    db.Orders
                        .FirstOrDefault(
                            x => x.OrderId == id
                        );


                if (order == null)
                {
                    TempData["Error"] =
                        "Order not found.";

                    return RedirectToAction(
                        "Orders"
                    );
                }


                // -------------------------
                // Delete OrderItems first
                // -------------------------

                var orderItems =
                    db.OrderItems
                        .Where(
                            x =>
                                x.OrderId == id
                        )
                        .ToList();


                if (orderItems.Any())
                {
                    db.OrderItems.RemoveRange(
                        orderItems
                    );
                }


                // -------------------------
                // Delete Order
                // -------------------------

                db.Orders.Remove(order);


                db.SaveChanges();


                TempData["Success"] =
                    "Order deleted successfully.";
            }
            catch (Exception ex)
            {
                string errorMessage =
                    ex.Message;


                Exception inner =
                    ex.InnerException;


                while (inner != null)
                {
                    errorMessage +=
                        " | INNER: " +
                        inner.Message;

                    inner =
                        inner.InnerException;
                }


                TempData["Error"] =
                    "Order could not be deleted: " +
                    errorMessage;
            }


            return RedirectToAction(
                "Orders"
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            var order = db.Orders.FirstOrDefault(x => x.OrderId == id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            // Only allow valid statuses
            var allowedStatuses = new[]
            {
        "Pending",
        "Processing",
        "Completed",
        "Cancelled"
    };

            if (string.IsNullOrWhiteSpace(status) ||
                !allowedStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid order status.";
                return RedirectToAction("Orders");
            }

            // ==============================
            // ADMIN CAN CHANGE ANY ORDER
            // ==============================

            bool isAdmin = User.IsInRole(UserRoles.Admin);

            if (!isAdmin)
            {
                // ==============================
                // NORMAL USER
                // Can change ONLY own order
                // ==============================

                var currentUserEmail = User.Identity.Name;

                var currentUser = db.Users
                    .FirstOrDefault(x => x.Email == currentUserEmail);

                if (currentUser == null)
                {
                    TempData["Error"] = "User account not found.";
                    return RedirectToAction("Orders");
                }

                if (order.UserId != currentUser.UserId)
                {
                    TempData["Error"] = "You are not allowed to change this order.";
                    return RedirectToAction("Orders");
                }
            }

            order.OrderStatus = status;

            db.SaveChanges();

            TempData["Success"] =
                "Order " + order.OrderNumber +
                " status changed to " + status + ".";

            return RedirectToAction("Orders");
        }

        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }


            base.Dispose(disposing);
        }
    }
}