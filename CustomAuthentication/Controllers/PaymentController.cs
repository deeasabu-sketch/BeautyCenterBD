using CustomAuthentication.Data;
using CustomAuthentication.Models;
using CustomAuthentication.Services;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext db =
            new AppDbContext();

        private readonly BkashPaymentService _bkash =
            new BkashPaymentService();


        // =====================================================
        // bKash Payment Start
        // =====================================================

        [HttpGet]
        public async Task<ActionResult> Bkash(int orderId)
        {
            var order =
                await db.Orders
                    .FirstOrDefaultAsync(
                        x => x.OrderId == orderId
                    );

            if (order == null)
            {
                return HttpNotFound();
            }


            // Security:
            // শুধু নিজের order-এর payment করা যাবে

            var currentUser =
                db.Users.FirstOrDefault(
                    u => u.Email == User.Identity.Name
                );


            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }


            if (order.UserId != currentUser.UserId)
            {
                return new HttpStatusCodeResult(403);
            }


            if (order.PaymentStatus == "Paid")
            {
                return RedirectToAction(
                    "OrderConfirmation",
                    "Shop",
                    new
                    {
                        id = order.OrderId
                    }
                );
            }


            // =====================================
            // DATABASE TOTAL
            // =====================================

            decimal amount =
                order.TotalAmount;


            string invoiceNumber =
                order.OrderNumber;


            string callbackUrl =
                Url.Action(
                    "BkashCallback",
                    "Payment",
                    null,
                    Request.Url.Scheme
                );


            // =====================================
            // CREATE BKASH PAYMENT
            // =====================================

            var payment =
                await _bkash.CreatePayment(
                    amount,
                    invoiceNumber,
                    callbackUrl
                );


            // =====================================
            // SAVE PAYMENT ID
            // =====================================

            order.PaymentMethod =
                "bKash";

            order.PaymentStatus =
                "Pending";

            order.PaymentID =
                payment.paymentID;


            await db.SaveChangesAsync();


            // =====================================
            // REDIRECT TO BKASH
            // =====================================

            string paymentUrl =
                payment.bkashURL;


            return Redirect(paymentUrl);
        }

        // =====================================================
        // bKash Callback
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> BkashCallback(
            string paymentID,
            string status)
        {
            // -----------------------------------------
            // Cancel
            // -----------------------------------------

            if (status == "cancel")
            {
                return RedirectToAction(
                    "PaymentFailed",
                    "Payment"
                );
            }


            // -----------------------------------------
            // Failure
            // -----------------------------------------

            if (status == "failure")
            {
                return RedirectToAction(
                    "PaymentFailed",
                    "Payment"
                );
            }


            // -----------------------------------------
            // Success
            // -----------------------------------------

            if (status == "success")
            {
                var result =
                    await _bkash.ExecutePayment(
                        paymentID
                    );


                if (result != null)
                {
                    string transactionStatus =
                        result.transactionStatus;


                    // Payment completed
                    if (transactionStatus == "Completed")
                    {
                        var order =
                            await db.Orders
                                .FirstOrDefaultAsync(
                                    x =>
                                    x.PaymentID == paymentID
                                );


                        if (order != null)
                        {
                            order.PaymentStatus =
                                "Paid";


                            order.TransactionId =
                                result.trxID;


                            order.PaymentMethod =
                                "bKash";


                            await db.SaveChangesAsync();


                            return RedirectToAction(
                                "OrderConfirmation",
                                "Shop",
                                new
                                {
                                    id = order.OrderId
                                }
                            );
                        }
                    }
                }
            }


            // Something went wrong
            return RedirectToAction(
                "PaymentFailed",
                "Payment"
            );
        }


        // =====================================================
        // COD
        // =====================================================

        [HttpGet]
        public async Task<ActionResult> COD(
            int orderId)
        {
            var order =
                await db.Orders
                    .FirstOrDefaultAsync(
                        x => x.OrderId == orderId
                    );


            if (order == null)
            {
                return HttpNotFound();
            }


            order.PaymentMethod =
                "Cash on Delivery";

            order.PaymentStatus =
                "Pending";


            await db.SaveChangesAsync();


            return RedirectToAction(
                "OrderConfirmation",
                "Shop",
                new
                {
                    id = order.OrderId
                }
            );
        }


        // =====================================================
        // Nagad
        // =====================================================

        [HttpGet]
        public async Task<ActionResult> Nagad(
            int orderId)
        {
            var order =
                await db.Orders
                    .FirstOrDefaultAsync(
                        x => x.OrderId == orderId
                    );


            if (order == null)
            {
                return HttpNotFound();
            }


            order.PaymentMethod =
                "Nagad";

            order.PaymentStatus =
                "Pending";


            await db.SaveChangesAsync();


            // Nagad API এখানে পরে বসাব


            return Content(
                "Nagad API integration will be added next."
            );
        }


        // =====================================================
        // Payment Failed
        // =====================================================

        [AllowAnonymous]
        public ActionResult PaymentFailed()
        {
            return View();
        }


        // =====================================================
        // Dispose
        // =====================================================

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