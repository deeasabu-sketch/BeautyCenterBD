namespace CustomAuthentication.Migrations
{
    using CustomAuthentication.Helpers;
    using CustomAuthentication.Models;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CustomAuthentication.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        // =========================================================
        // SAMPLE / TEST ACCOUNTS
        // =========================================================
        //
        // Runs automatically on every app start (see Global.asax ->
        // Database.SetInitializer). AddOrUpdate matches by Email, so
        // re-running this (e.g. after a fresh database, or every time
        // the app starts) never creates duplicates and never requires
        // manually re-seeding by hand.
        //
        // Password for every sample account below is the same
        // placeholder - change it after logging in for the first time:
        //
        //      Password: Test@123
        //
        // Replace the sample Gmail addresses with real ones before
        // going to production.
        //
        protected override void Seed(CustomAuthentication.Data.AppDbContext context)
        {
            // Keep EF6 migration seeding available as well.
            CustomAuthentication.Data.AppDbContext.SeedSampleUsers(context);
        }
    }
}
