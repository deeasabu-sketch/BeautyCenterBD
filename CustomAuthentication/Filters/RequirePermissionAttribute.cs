using CustomAuthentication.Data;
using CustomAuthentication.Service;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Filters
{
    public enum PermissionAction { View, Create, Edit, Delete }

    // Usage: [RequirePermission("Products", PermissionAction.Edit)]
    // Checks the RolePermission table (via PermissionService) for the
    // current user's role instead of a hardcoded role list. Redirects
    // to Login if not authenticated, or AccessDenied if the check fails.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        private readonly string moduleName;
        private readonly PermissionAction action;

        public RequirePermissionAttribute(string moduleName, PermissionAction action = PermissionAction.View)
        {
            this.moduleName = moduleName;
            this.action = action;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.User;

            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            using (var db = new AppDbContext())
            {
                var currentUser = db.Users.FirstOrDefault(u => u.Email == user.Identity.Name);

                if (currentUser == null || !currentUser.IsActive)
                {
                    filterContext.Result = new RedirectResult("~/Account/Login");
                    return;
                }

                var permissionService = new PermissionService(db);
                bool allowed;

                switch (action)
                {
                    case PermissionAction.Create:
                        allowed = permissionService.CanCreate(currentUser, moduleName);
                        break;

                    case PermissionAction.Edit:
                        allowed = permissionService.CanEdit(currentUser, moduleName);
                        break;

                    case PermissionAction.Delete:
                        allowed = permissionService.CanDelete(currentUser, moduleName);
                        break;

                    default:
                        allowed = permissionService.CanView(currentUser, moduleName);
                        break;
                }

                if (!allowed)
                {
                    filterContext.Result = new RedirectResult("~/Account/AccessDenied");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
