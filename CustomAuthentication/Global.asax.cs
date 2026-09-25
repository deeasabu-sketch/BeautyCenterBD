
using System;
using System.Data.Entity;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CustomAuthentication
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Applies any pending EF Migrations automatically and runs
            // Migrations/Configuration.cs Seed() on every app start.
            // Seed() uses AddOrUpdate, so it's safe to run repeatedly -
            // no need to manually run Update-Database or re-seed by hand.
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<Data.AppDbContext, Migrations.Configuration>());

            // Apply pending migrations (if any), then ensure the sample
            // accounts exist. This means you do NOT need to run
            // Update-Database every time just to get the test users.
            using (var db = new Data.AppDbContext())
            {
                db.Database.Initialize(false);
                Data.AppDbContext.SeedSampleUsers(db);
            }

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(
                GlobalFilters.Filters
            );

            RouteConfig.RegisterRoutes(
                RouteTable.Routes
            );

            BundleConfig.RegisterBundles(
                BundleTable.Bundles
            );
        }


        // =====================================================
        // AUTHENTICATION + ROLE
        // =====================================================

        protected void Application_PostAuthenticateRequest(
            object sender,
            EventArgs e)
        {
            HttpCookie authCookie =
                Request.Cookies[
                    FormsAuthentication.FormsCookieName
                ];

            if (authCookie == null)
            {
                return;
            }


            FormsAuthenticationTicket ticket;

            try
            {
                ticket = FormsAuthentication.Decrypt(
                    authCookie.Value
                );
            }
            catch
            {
                return;
            }


            if (ticket == null)
            {
                return;
            }


            string username = ticket.Name;
            string role = ticket.UserData;


            // Create Identity
            FormsIdentity identity =
                new FormsIdentity(ticket);


            // Create Principal with Role
            GenericPrincipal principal =
                new GenericPrincipal(
                    identity,
                    new[] { role }
                );


            // Set Current User
            HttpContext.Current.User = principal;
        }
    }
}

