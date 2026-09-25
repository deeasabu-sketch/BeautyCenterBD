using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // Note: this is the master-data "Customer" record used for in-store
    // Sales (POS / walk-in customers, optional on a Sale). It is separate
    // from the shop's own registered Users who place Orders online.
    public class CustomerController : InventoryBaseController
    {
        [RequirePermission("Customers", PermissionAction.View)]
        public ActionResult Index()
        {
            var customers = db.Customers
                .OrderBy(c => c.Name)
                .ToList();

            return View(customers);
        }

        [RequirePermission("Customers", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Customer { IsActive = true });
        }

        [RequirePermission("Customers", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Customer name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();
            db.Customers.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Customer added successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Customers", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var customer = db.Customers.Find(id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        [RequirePermission("Customers", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Customer name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customer = db.Customers.Find(model.CustomerId);

            if (customer == null)
            {
                return HttpNotFound();
            }

            customer.Name = model.Name.Trim();
            customer.Phone = model.Phone;
            customer.Email = model.Email;
            customer.Address = model.Address;
            customer.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Customers", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var customer = db.Customers.Find(id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            bool inUse = db.Sales.Any(s => s.CustomerId == id);

            if (inUse)
            {
                TempData["Error"] = "This customer has sale records and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            TempData["Success"] = "Customer deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
