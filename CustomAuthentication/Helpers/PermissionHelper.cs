using CustomAuthentication.Data;
using System.Linq;
using System.Security.Principal;

namespace CustomAuthentication.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(
            AppDbContext db,
            IPrincipal principal,
            string permission)
        {
            if (principal == null ||
                principal.Identity == null ||
                !principal.Identity.IsAuthenticated)
            {
                return false;
            }

            // Admin always has full permission
            if (principal.IsInRole("Admin"))
            {
                return true;
            }

            var user = db.Users.FirstOrDefault(
                x => x.Email == principal.Identity.Name
            );

            if (user == null || !user.IsActive)
            {
                return false;
            }

            switch (permission)
            {
                case "CanViewDashboard":
                    return user.CanViewDashboard;

                case "CanViewProducts":
                    return user.CanViewProducts;

                case "CanAddProducts":
                    return user.CanAddProducts;

                case "CanEditProducts":
                    return user.CanEditProducts;

                case "CanDeleteProducts":
                    return user.CanDeleteProducts;

                case "CanViewOrders":
                    return user.CanViewOrders;

                case "CanAddOrders":
                    return user.CanAddOrders;

                case "CanEditOrders":
                    return user.CanEditOrders;

                case "CanDeleteOrders":
                    return user.CanDeleteOrders;

                case "CanViewReports":
                    return user.CanViewReports;

                case "CanManageUsers":
                    return user.CanManageUsers;

                case "CanViewNotebook":
                    return user.CanViewNotebook;

                case "CanAddNotebook":
                    return user.CanAddNotebook;

                case "CanEditNotebook":
                    return user.CanEditNotebook;

                case "CanDeleteNotebook":
                    return user.CanDeleteNotebook;

                case "CanManagePromoCodes":
                    return user.CanManagePromoCodes;

                default:
                    return false;
            }
        }
    }
}