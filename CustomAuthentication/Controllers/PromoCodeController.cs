using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class PromoCodeController : InventoryBaseController
    {
        [RequirePermission("PromoCodes", PermissionAction.View)]
        public ActionResult Index()
        {
            var codes = db.PromoCodes
                .OrderByDescending(p => p.PromoCodeId)
                .ToList();

            return View(codes);
        }

        [RequirePermission("PromoCodes", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new PromoCodeViewModel { IsActive = true });
        }

        [RequirePermission("PromoCodes", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PromoCodeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Promo code is required.");
            }
            else
            {
                model.Code = model.Code.Trim().ToUpper();

                if (db.PromoCodes.Any(p => p.Code == model.Code))
                {
                    ModelState.AddModelError("Code", "This code already exists.");
                }
            }

            if (model.DiscountPercent <= 0 || model.DiscountPercent > 100)
            {
                ModelState.AddModelError("DiscountPercent", "Enter a discount percent between 1 and 100.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var promo = new PromoCode
            {
                Code = model.Code,
                Description = model.Description,
                DiscountPercent = model.DiscountPercent,
                MinOrderAmount = model.MinOrderAmount,
                ExpiryDate = model.ExpiryDate,
                IsActive = model.IsActive,
                CreateDate = DateTime.Now,
                CreatedByUserId = CurrentUser?.UserId ?? 0
            };

            db.PromoCodes.Add(promo);
            db.SaveChanges();

            TempData["Success"] = "Promo code created.";
            return RedirectToAction("Index");
        }

        [RequirePermission("PromoCodes", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var promo = db.PromoCodes.Find(id);

            if (promo == null)
            {
                return HttpNotFound();
            }

            var model = new PromoCodeViewModel
            {
                PromoCodeId = promo.PromoCodeId,
                Code = promo.Code,
                Description = promo.Description,
                DiscountPercent = promo.DiscountPercent,
                MinOrderAmount = promo.MinOrderAmount,
                ExpiryDate = promo.ExpiryDate,
                IsActive = promo.IsActive
            };

            return View(model);
        }

        [RequirePermission("PromoCodes", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PromoCodeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Promo code is required.");
            }
            else
            {
                model.Code = model.Code.Trim().ToUpper();

                if (db.PromoCodes.Any(p => p.Code == model.Code && p.PromoCodeId != model.PromoCodeId))
                {
                    ModelState.AddModelError("Code", "This code already exists.");
                }
            }

            if (model.DiscountPercent <= 0 || model.DiscountPercent > 100)
            {
                ModelState.AddModelError("DiscountPercent", "Enter a discount percent between 1 and 100.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var promo = db.PromoCodes.Find(model.PromoCodeId);

            if (promo == null)
            {
                return HttpNotFound();
            }

            promo.Code = model.Code;
            promo.Description = model.Description;
            promo.DiscountPercent = model.DiscountPercent;
            promo.MinOrderAmount = model.MinOrderAmount;
            promo.ExpiryDate = model.ExpiryDate;
            promo.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Promo code updated.";
            return RedirectToAction("Index");
        }

        [RequirePermission("PromoCodes", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var promo = db.PromoCodes.Find(id);

            if (promo != null)
            {
                db.PromoCodes.Remove(promo);
                db.SaveChanges();
                TempData["Success"] = "Promo code deleted.";
            }

            return RedirectToAction("Index");
        }
    }
}