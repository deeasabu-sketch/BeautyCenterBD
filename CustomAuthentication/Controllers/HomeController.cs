using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // The site's landing page. Per the shop requirement, EVERYONE
    // (logged in or not) lands on the product listing first - they can
    // browse and add to cart before ever being asked to log in. Product
    // views live here (Home) as requested; the actual add-to-cart /
    // checkout actions still live on ShopController, which these views
    // post to.
    public class HomeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Number of products loaded per page / per infinite-scroll batch.
        private const int PageSize = 12;

        public ActionResult Index(int? categoryId, string q, int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            try
            {
                var query = BuildProductQuery(categoryId, q);

                var totalCount = query.Count();

                var products = query
                    .OrderByDescending(p => p.ProductId)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                var cart = CartHelper.GetCart(HttpContext);

                var model = new ShopIndexViewModel
                {
                    Products = products,
                    Categories = GetActiveCategories(),
                    SelectedCategoryId = categoryId,
                    SearchTerm = q,
                    CartViewModel = cart,
                    CartItems = cart,
                    CartTotal = cart.Total,
                    CurrentPage = page,
                    PageSize = PageSize,
                    TotalProductCount = totalCount,
                    HasMoreProducts = (page * PageSize) < totalCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Trace.TraceError("HomeController.Index failed: " + ex);

                // Degrade gracefully: show the shop shell with an empty
                // product list and a friendly notice rather than a 500 page.
                var cart = SafeGetCart();

                var model = new ShopIndexViewModel
                {
                    Products = new System.Collections.Generic.List<Product>(),
                    Categories = new System.Collections.Generic.List<Category>(),
                    SelectedCategoryId = categoryId,
                    SearchTerm = q,
                    CartViewModel = cart,
                    CartItems = cart,
                    CartTotal = cart?.Total ?? 0,
                    CurrentPage = 1,
                    PageSize = PageSize,
                    TotalProductCount = 0,
                    HasMoreProducts = false,
                    LoadError = "We couldn't load products right now. Please try again shortly."
                };

                return View(model);
            }
        }

        // Called by infinite-scroll on the shop page to fetch the next
        // batch of products as the user scrolls down.
        [HttpGet]
        public ActionResult LoadMoreProducts(int? categoryId, string q, int page = 2)
        {
            if (page < 1)
            {
                page = 1;
            }

            try
            {
                var query = BuildProductQuery(categoryId, q);

                var totalCount = query.Count();

                var products = query
                    .OrderByDescending(p => p.ProductId)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                var hasMore = (page * PageSize) < totalCount;

                return Json(new
                {
                    success = true,
                    html = RenderPartialToString("_ProductGrid", products),
                    hasMore = hasMore,
                    nextPage = page + 1
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Trace.TraceError("HomeController.LoadMoreProducts failed: " + ex);

                Response.StatusCode = 500;
                return Json(new
                {
                    success = false,
                    message = "Could not load more products. Please try again."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetActivePromoCodes()
        {
            try
            {
                var today = DateTime.Now.Date;

                var activePromos = db.PromoCodes
                    .Where(p => p.IsActive && (!p.ExpiryDate.HasValue || p.ExpiryDate.Value >= today))
                    .OrderByDescending(p => p.PromoCodeId)
                    .Select(p => new
                    {
                        Code = p.Code,
                        Description = p.Description
                    })
                    .ToList();

                return Json(activePromos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Trace.TraceError("HomeController.GetActivePromoCodes failed: " + ex);

                // Empty list -> the ticker script simply stays hidden.
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Details(int id)
        {
            try
            {
                var product = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Unit)
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.ProductId == id && p.IsActive);

                if (product == null)
                {
                    return HttpNotFound();
                }

                var related = db.Products
                    .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.ProductId != id)
                    .OrderByDescending(p => p.ProductId)
                    .Take(4)
                    .ToList();

                ViewBag.RelatedProducts = related;

                return View(product);
            }
            catch (Exception ex)
            {
                Trace.TraceError("HomeController.Details failed for id=" + id + ": " + ex);
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        // ---- helpers ----------------------------------------------------

        private IQueryable<Product> BuildProductQuery(int? categoryId, string q)
        {
            var query = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                query = query.Where(p =>
                    p.ProductName.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }

            return query;
        }

        private System.Collections.Generic.List<Category> GetActiveCategories()
        {
            try
            {
                return db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                Trace.TraceError("HomeController.GetActiveCategories failed: " + ex);
                return new System.Collections.Generic.List<Category>();
            }
        }

        private Cart SafeGetCart()
        {
            try
            {
                return CartHelper.GetCart(HttpContext);
            }
            catch (Exception ex)
            {
                Trace.TraceError("HomeController.SafeGetCart failed: " + ex);
                return new Cart();
            }
        }

        // Renders a partial view to a string so infinite-scroll requests
        // can return ready-to-insert HTML alongside JSON paging info.
        private string RenderPartialToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);

                if (viewResult.View == null)
                {
                    throw new InvalidOperationException("Partial view '" + viewName + "' was not found.");
                }

                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }

        [AllowAnonymous]
        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
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
