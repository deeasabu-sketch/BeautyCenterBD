using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Schema;

namespace CustomAuthentication.Controllers
{
    // Public storefront. Anyone (logged in or not) can browse products
    // and build a cart. Login is only required at Checkout/PlaceOrder.
    public class ShopController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // =========================
        // CART (session-based, no login required)
        // =========================
        public ActionResult Cart()
        {
            var cart = CartHelper.GetCart(HttpContext);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Home");
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Error"] = $"Only {product.StockQuantity} unit(s) of {product.ProductName} available.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            decimal effectivePrice = product.UnitPrice;

            if (product.DiscountPercent > 0)
            {
                effectivePrice = product.UnitPrice - (product.UnitPrice * product.DiscountPercent / 100m);
            }

            CartHelper.AddItem(HttpContext, new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ImagePath = product.ImagePath,
                UnitPrice = effectivePrice,
                Quantity = quantity
            });

            TempData["Success"] = product.ProductName + " added to cart.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCartItem(int productId, int quantity)
        {
            CartHelper.UpdateQuantity(HttpContext, productId, quantity);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int productId)
        {
            CartHelper.RemoveItem(HttpContext, productId);
            return RedirectToAction("Cart");
        }


        [HttpPost]
        public JsonResult ApplyPromo(string code)
        {
            var cart = CartHelper.GetCart(HttpContext);

            if (cart == null || cart.Count == 0)
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Please enter a promo code." });
            }

            decimal subTotal = cart.Sum(i => i.LineTotal);

            var promo = db.PromoCodes.FirstOrDefault(p => p.Code == code.Trim().ToUpper() && p.IsActive);

            if (promo == null)
            {
                return Json(new { success = false, message = "Invalid or inactive promo code." });
            }

            if (promo.ExpiryDate.HasValue && promo.ExpiryDate.Value.Date < DateTime.Now.Date)
            {
                return Json(new { success = false, message = "This promo code has expired." });
            }

            if (promo.MinOrderAmount.HasValue && subTotal < promo.MinOrderAmount.Value)
            {
                return Json(new { success = false, message = "Minimum order of Taka " + promo.MinOrderAmount.Value.ToString("N0") + " required for this code." });
            }

            decimal discountAmount = Math.Round(subTotal * promo.DiscountPercent / 100m, 2);

            return Json(new
            {
                success = true,
                message = "'" + promo.Code + "' applied - " + promo.DiscountPercent.ToString("0.##") + "% off.",
                discountPercent = promo.DiscountPercent,
                discountAmount = discountAmount
            });
        }

        // =========================
        // CHECKOUT (login required from here on)
        // =========================
        [Authorize]
        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = CartHelper.GetCart(HttpContext);

            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Home");
            }

            var currentUser = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);

            var model = new CheckoutViewModel
            {
                Cart = cart,
                ShippingName = currentUser?.FullName,
                ShippingPhone = currentUser?.Phone,
                ShippingAddress = currentUser?.Address,
                DeliveryArea = DeliveryAreas.Dhaka,
                DeliveryCharge = DeliveryAreas.DhakaCharge,
                WarehouseId = db.Warehouses.Where(w => w.IsActive).Select(w => w.WarehouseId).FirstOrDefault(),
                WarehouseOptions = db.Warehouses
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.Name)
                    .Select(w => new SelectListItem { Value = w.WarehouseId.ToString(), Text = w.Name })
                    .ToList()
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            var cart = CartHelper.GetCart(HttpContext);

            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Home");
            }


            // ==========================================
            // 1. BASIC VALIDATION
            // ==========================================

            if (string.IsNullOrWhiteSpace(model.ShippingName))
            {
                ModelState.AddModelError(
                    "ShippingName",
                    "Full name is required."
                );
            }


            if (string.IsNullOrWhiteSpace(model.ShippingPhone))
            {
                ModelState.AddModelError(
                    "ShippingPhone",
                    "Phone number is required."
                );
            }


            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                ModelState.AddModelError(
                    "ShippingAddress",
                    "Delivery address is required."
                );
            }


            // ==========================================
            // 2. DELIVERY AREA VALIDATION
            // ==========================================

            if (model.DeliveryArea != DeliveryAreas.Dhaka &&
                model.DeliveryArea != DeliveryAreas.OutsideDhaka)
            {
                ModelState.AddModelError(
                    "DeliveryArea",
                    "Please select a valid delivery area."
                );
            }


            // ==========================================
            // 3. PAYMENT METHOD VALIDATION
            // ==========================================

            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Please select a payment method."
                );
            }
            else
            {
                var allowedPaymentMethods = new[]
                {
            "bKash",
            "Nagad",
            "Cash on Delivery"
        };

                if (!allowedPaymentMethods.Contains(model.PaymentMethod))
                {
                    ModelState.AddModelError(
                        "PaymentMethod",
                        "Invalid payment method."
                    );
                }
            }


            // ==========================================
            // 4. STOCK VALIDATION
            // ==========================================

            foreach (var item in cart)
            {
                var product =
                    db.Products.Find(item.ProductId);

                if (product == null || !product.IsActive)
                {
                    ModelState.AddModelError(
                        "",
                        $"{item.ProductName} is no longer available."
                    );
                }
                else if (item.Quantity > product.StockQuantity)
                {
                    ModelState.AddModelError(
                        "",
                        $"Only {product.StockQuantity} unit(s) of {product.ProductName} left in stock."
                    );
                }
            }


            // ==========================================
            // 5. IF VALIDATION FAILS
            // ==========================================

            if (!ModelState.IsValid)
            {
                model.Cart = cart;

                model.WarehouseOptions =
                    db.Warehouses
                      .Where(w => w.IsActive)
                      .OrderBy(w => w.Name)
                      .Select(w => new SelectListItem
                      {
                          Value = w.WarehouseId.ToString(),
                          Text = w.Name
                      })
                      .ToList();

                return View("Checkout", model);
            }


            // ==========================================
            // 6. CURRENT USER
            // ==========================================

            var userIdentity =
                User.Identity.Name;

            var currentUser =
                db.Users.FirstOrDefault(
                    u =>
                        u.Email == userIdentity ||
                        u.FullName == userIdentity
                );


            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }


            // ==========================================
            // 7. DATABASE TRANSACTION
            // ==========================================

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {

                    // ==========================================
                    // 8. SUBTOTAL
                    // ==========================================

                    decimal subTotal =
                        cart.Sum(i => i.LineTotal);


                    // ==========================================
                    // 9. DELIVERY CHARGE
                    // ==========================================

                    decimal deliveryCharge =
                        DeliveryAreas.GetCharge(
                            model.DeliveryArea
                        );


                    // ==========================================
                    // 10. PROMO DISCOUNT
                    // ==========================================

                    decimal discountPercent = 0;

                    decimal discountAmount = 0;

                    string appliedPromoCode = null;


                    if (!string.IsNullOrWhiteSpace(
                        model.PromoCode))
                    {

                        var promo =
                            db.PromoCodes.FirstOrDefault(
                                p =>
                                    p.Code ==
                                        model.PromoCode
                                            .Trim()
                                            .ToUpper()
                                    &&
                                    p.IsActive
                            );


                        if (promo != null
                            &&
                            (
                                !promo.ExpiryDate.HasValue
                                ||
                                promo.ExpiryDate.Value.Date
                                >= DateTime.Now.Date
                            )
                            &&
                            (
                                !promo.MinOrderAmount.HasValue
                                ||
                                subTotal
                                >= promo.MinOrderAmount.Value
                            )
                        )
                        {
                            discountPercent =
                                promo.DiscountPercent;


                            discountAmount =
                                Math.Round(
                                    subTotal
                                    * discountPercent
                                    / 100m,
                                    2
                                );


                            appliedPromoCode =
                                promo.Code;
                        }
                    }


                    // ==========================================
                    // 11. FINAL TOTAL
                    // ==========================================

                    decimal total =
                        subTotal
                        + deliveryCharge
                        - discountAmount;


                    if (total < 0)
                    {
                        total = 0;
                    }


                    // ==========================================
                    // 12. GENERATE ORDER NUMBER
                    // ==========================================

                    string generatedOrderNumber =
                        "ORD-"
                        + DateTime.Now.ToString(
                            "yyyyMMddHHmmss"
                        );


                    // ==========================================
                    // 13. CREATE ORDER
                    // ==========================================

                    var order = new Order
                    {
                        OrderNumber =
                            generatedOrderNumber,

                        UserId =
                            currentUser.UserId,

                        ShippingName =
                            model.ShippingName,

                        ShippingPhone =
                            model.ShippingPhone,

                        ShippingAddress =
                            model.ShippingAddress,

                        DeliveryArea =
                            model.DeliveryArea,

                        WarehouseId =
                            model.WarehouseId,

                        SubTotal =
                            subTotal,

                        DeliveryCharge =
                            deliveryCharge,

                        DiscountAmount =
                            discountAmount,

                        PromoCode =
                            appliedPromoCode,

                        TotalAmount =
                            total,

                        OrderDate =
                            DateTime.Now,

                        OrderStatus =
                            "Pending",

                        Status =
                            "Pending",

                        // =========================
                        // PAYMENT
                        // =========================

                        PaymentMethod =
                            model.PaymentMethod,

                        PaymentStatus =
                            "Pending"
                    };


                    // ==========================================
                    // 14. SAVE ORDER
                    // ==========================================

                    db.Orders.Add(order);

                    db.SaveChanges();


                    // ==========================================
                    // 15. ORDER ITEMS + STOCK
                    // ==========================================

                    foreach (var item in cart)
                    {
                        var product =
                            db.Products.Find(
                                item.ProductId
                            );


                        // --------------------------
                        // ORDER ITEM
                        // --------------------------

                        db.OrderItems.Add(
                            new OrderItem
                            {
                                OrderId =
                                    order.OrderId,

                                ProductId =
                                    item.ProductId,

                                Quantity =
                                    item.Quantity,

                                UnitPrice =
                                    item.UnitPrice
                            }
                        );


                        // --------------------------
                        // STOCK
                        // --------------------------

                        if (product != null)
                        {
                            product.StockQuantity -=
                                item.Quantity;


                            db.StockTransactions.Add(
                                new StockTransaction
                                {
                                    ProductId =
                                        item.ProductId,

                                    WarehouseId =
                                        order.WarehouseId,

                                    TransactionType =
                                        "Sale",

                                    QuantityChange =
                                        -item.Quantity,

                                    ReferenceId =
                                        order.OrderId,

                                    ReferenceType =
                                        "Order",

                                    TransactionDate =
                                        DateTime.Now,

                                    CreatedByUserId =
                                        currentUser.UserId
                                }
                            );
                        }
                    }


                    // ==========================================
                    // 16. SAVE EVERYTHING
                    // ==========================================

                    db.SaveChanges();


                    // ==========================================
                    // 17. COMMIT TRANSACTION
                    // ==========================================

                    transaction.Commit();


                    // ==========================================
                    // 18. CLEAR CART
                    // ==========================================

                    CartHelper.Clear(
                        HttpContext
                    );


                    // ==========================================
                    // 19. PAYMENT ROUTING
                    // ==========================================

                    if (model.PaymentMethod == "bKash")
                    {
                        return RedirectToAction(
                            "Bkash",
                            "Payment",
                            new
                            {
                                orderId =
                                    order.OrderId
                            }
                        );
                    }


                    if (model.PaymentMethod == "Nagad")
                    {
                        return RedirectToAction(
                            "Nagad",
                            "Payment",
                            new
                            {
                                orderId =
                                    order.OrderId
                            }
                        );
                    }


                    // ==========================================
                    // 20. CASH ON DELIVERY
                    // ==========================================

                    if (model.PaymentMethod ==
                        "Cash on Delivery")
                    {
                        TempData["Success"] =
                            "Your order has been placed successfully!";

                        return RedirectToAction(
                            "OrderConfirmation",
                            new
                            {
                                id =
                                    order.OrderId
                            }
                        );
                    }


                    // ==========================================
                    // FALLBACK
                    // ==========================================

                    return RedirectToAction(
                        "OrderConfirmation",
                        new
                        {
                            id =
                                order.OrderId
                        }
                    );
                }


                // ==========================================
                // ENTITY VALIDATION ERROR
                // ==========================================

                catch (
                    System.Data.Entity.Validation
                        .DbEntityValidationException dbEx)
                {
                    transaction.Rollback();


                    var errorMessages =
                        dbEx.EntityValidationErrors
                            .SelectMany(
                                x => x.ValidationErrors
                            )
                            .Select(
                                x => x.ErrorMessage
                            );


                    var fullErrorMessage =
                        string.Join(
                            "; ",
                            errorMessages
                        );


                    ModelState.AddModelError(
                        "",
                        "Validation Error: "
                        + fullErrorMessage
                    );
                }


                // ==========================================
                // GENERAL ERROR
                // ==========================================

                catch (Exception ex)
                {
                    transaction.Rollback();


                    string innerMsg =
                        ex.InnerException != null
                            ?
                            (
                                ex.InnerException
                                    .InnerException != null
                                    ?
                                    ex.InnerException
                                        .InnerException
                                        .Message
                                    :
                                    ex.InnerException
                                        .Message
                            )
                            :
                            ex.Message;


                    ModelState.AddModelError(
                        "",
                        "Database Error: "
                        + innerMsg
                    );
                }


                // ==========================================
                // RETURN CHECKOUT WITH ERRORS
                // ==========================================

                model.Cart = cart;


                model.WarehouseOptions =
                    db.Warehouses
                      .Where(w => w.IsActive)
                      .OrderBy(w => w.Name)
                      .Select(w => new SelectListItem
                      {
                          Value =
                              w.WarehouseId.ToString(),

                          Text =
                              w.Name
                      })
                      .ToList();


                return View(
                    "Checkout",
                    model
                );
            }
        }

        // =========================
        // ORDER CONFIRMATION / INVOICE (customer's own order only)
        // =========================
        [Authorize]
        public ActionResult OrderConfirmation(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderItems.Select(i => i.Product))
                .Include(o => o.Warehouse)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var currentUser = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);

            bool isOwner = currentUser != null && order.UserId == currentUser.UserId;
            bool isStaff = currentUser != null && currentUser.RoleName != UserRoles.Customer;

            if (!isOwner && !isStaff)
            {
                return new HttpStatusCodeResult(403);
            }

            return View(order);
        }

        // Full invoice page (used right after checkout and from "My Orders").
        [Authorize]
        public ActionResult Invoice(int id)
        {
            var order = GetOrderForCurrentUserOrStaff(id, out ActionResult errorResult);

            if (errorResult != null)
            {
                return errorResult;
            }

            return View(order);
        }

        // Shared lookup + ownership check used by both OrderConfirmation and Invoice.
        private Order GetOrderForCurrentUserOrStaff(int id, out ActionResult errorResult)
        {
            errorResult = null;

            try
            {
                var order = db.Orders
                    .Include(o => o.OrderItems.Select(i => i.Product))
                    .Include(o => o.Warehouse)
                    .FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                {
                    errorResult = HttpNotFound();
                    return null;
                }

                var currentUser = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                bool isOwner = currentUser != null && order.UserId == currentUser.UserId;
                bool isStaff = currentUser != null && currentUser.RoleName != UserRoles.Customer;

                if (!isOwner && !isStaff)
                {
                    errorResult = new HttpStatusCodeResult(403);
                    return null;
                }

                return order;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ShopController.GetOrderForCurrentUserOrStaff failed for id=" + id + ": " + ex);
                errorResult = View("Error");
                return null;
            }
        }


        // =========================
        // MY ORDERS (customer dashboard)
        // =========================
        [Authorize]
        public ActionResult MyOrders()
        {
            var currentUser = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == currentUser.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // =========================
        // BUY NOW (adds this one item to cart, goes straight to Checkout)
        // =========================
        [HttpGet]
        public ActionResult BuyNow(int productId, int quantity = 1)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Home");
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Error"] = $"Only {product.StockQuantity} unit(s) of {product.ProductName} available.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            decimal effectivePrice = product.UnitPrice;

            if (product.DiscountPercent > 0)
            {
                effectivePrice = product.UnitPrice - (product.UnitPrice * product.DiscountPercent / 100m);
            }

            CartHelper.AddItem(HttpContext, new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ImagePath = product.ImagePath,
                UnitPrice = effectivePrice,
                Quantity = quantity
            });

            return RedirectToAction("Checkout");
        }

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
