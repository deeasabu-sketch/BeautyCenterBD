using CustomAuthentication.Models;
using CustomAuthentication.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace CustomAuthentication.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("defaultconnection")

        { }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<NotebookEntry> NotebookEntries { get; set; }


        // Master data
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Product> Products { get; set; }

        // Purchase
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseDetail> PurchaseDetails { get; set; }

        // Sale
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }

        // Stock movement
        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<StockTransferDetail> StockTransferDetails { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        // HR
        public DbSet<Salary> Salaries { get; set; }

        // Permission system
        public DbSet<Module> Modules { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Product gallery
        public DbSet<ProductImage> ProductImages { get; set; }

        //PromoCode
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Creates/updates the sample accounts used for first-run testing.
        /// AddOrUpdate by email makes this safe to run repeatedly.
        /// It does NOT require a new EF migration.
        /// </summary>
        public static void SeedSampleUsers(AppDbContext context)
        {
            const string samplePassword = "Test@123";
            string passwordHash = PasswordHelper.HashPassword(samplePassword);

            var mainWarehouse = new Warehouse
            {
                Name = "Main Warehouse", Code = "WH-MAIN",
                Address = "Dhaka, Bangladesh", Phone = "01700000000", IsActive = true
            };

            context.Warehouses.AddOrUpdate(w => w.Code, mainWarehouse);
            context.SaveChanges();
            int mainWarehouseId = context.Warehouses.First(w => w.Code == "WH-MAIN").WarehouseId;

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Admin User", Phone="01580932171", Email="deeasabu@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.Admin, IsActive=true, WarehouseId=null,
              CanViewDashboard=true, CanViewProducts=true, CanAddProducts=true, CanEditProducts=true, CanDeleteProducts=true, CanViewOrders=true, CanAddOrders=true, CanEditOrders=true, CanDeleteOrders=true, CanViewReports=true, CanManageUsers=true, CanViewNotebook=true, CanAddNotebook=true, CanEditNotebook=true, CanDeleteNotebook=true, CanManagePromoCodes=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="HR Manager", Phone="01700000002", Email="hr.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.HR, IsActive=true, WarehouseId=null, CanViewDashboard=true, CanManageUsers=true, CanViewReports=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Central Manager", Phone="01700000003", Email="centralmanager.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.CentralManager, IsActive=true, WarehouseId=null, CanViewDashboard=true, CanViewProducts=true, CanEditProducts=true, CanViewOrders=true, CanEditOrders=true, CanViewReports=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Branch Manager", Phone="01700000004", Email="branchmanager.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.BranchManager, IsActive=true, WarehouseId=mainWarehouseId, CanViewDashboard=true, CanViewProducts=true, CanEditProducts=true, CanViewOrders=true, CanEditOrders=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Sales Person", Phone="01700000005", Email="salesperson.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.SalesPerson, IsActive=true, WarehouseId=mainWarehouseId, CanViewDashboard=true, CanViewOrders=true, CanAddOrders=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Warehouse Manager", Phone="01700000006", Email="warehousemanager.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.WarehouseManager, IsActive=true, WarehouseId=mainWarehouseId, CanViewDashboard=true, CanViewProducts=true, CanEditProducts=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Accountant User", Phone="01700000007", Email="accountant.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.Accountant, IsActive=true, WarehouseId=null, CanViewDashboard=true, CanViewOrders=true, CanViewReports=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Employee User", Phone="01700000008", Email="employee.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.Employee, IsActive=true, WarehouseId=mainWarehouseId, CanViewDashboard=true, CanViewProducts=true, CanViewOrders=true });

            context.Users.AddOrUpdate(u => u.Email, new User
            { FullName="Sample Customer", Phone="01700000009", Address="Mirpur, Dhaka", Email="customer.sample@gmail.com", PasswordHash=passwordHash, RoleName=UserRoles.Customer, IsActive=true, WarehouseId=null, CanViewDashboard=true });

            context.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Category Relationship (Required)
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);

            // Brand Relationship (Optional)
            modelBuilder.Entity<Product>()
                .HasOptional(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .WillCascadeOnDelete(false);

            // Prevent EF's default cascade-delete conventions from creating
            // multiple cascade paths between Warehouse <-> Purchase/Sale/etc,
            // which SQL Server would reject at DB-creation time.
            modelBuilder.Entity<Purchase>()
                .HasRequired(p => p.Warehouse)
                .WithMany(w => w.Purchases)
                .HasForeignKey(p => p.WarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Purchase>()
                .HasRequired(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Sale>()
                .HasRequired(s => s.Warehouse)
                .WithMany(w => w.Sales)
                .HasForeignKey(s => s.WarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Sale>()
                .HasOptional(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockTransaction>()
                .HasRequired(t => t.Warehouse)
                .WithMany(w => w.StockTransactions)
                .HasForeignKey(t => t.WarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockTransfer>()
                .HasRequired(t => t.FromWarehouse)
                .WithMany()
                .HasForeignKey(t => t.FromWarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockTransfer>()
                .HasRequired(t => t.ToWarehouse)
                .WithMany()
                .HasForeignKey(t => t.ToWarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasOptional(u => u.Warehouse)
                .WithMany(w => w.Users)
                .HasForeignKey(u => u.WarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Salary>()
                .HasRequired(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .WillCascadeOnDelete(false);

            // Module (1) -> RolePermission (many)
            modelBuilder.Entity<RolePermission>()
                .HasRequired(rp => rp.Module)
                .WithMany(m => m.RolePermissions)
                .HasForeignKey(rp => rp.ModuleId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PurchaseDetail>()
                .HasRequired(pd => pd.Purchase)
                .WithMany(p => p.PurchaseDetails)
                .HasForeignKey(pd => pd.PurchaseId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<SaleDetail>()
                .HasRequired(sd => sd.Sale)
                .WithMany(s => s.SaleDetails)
                .HasForeignKey(sd => sd.SaleId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<StockTransferDetail>()
                .HasRequired(td => td.StockTransfer)
                .WithMany(t => t.StockTransferDetails)
                .HasForeignKey(td => td.StockTransferId)
                .WillCascadeOnDelete(true);

            // ---- Public shop: Order / OrderItem / ProductImage ----
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Warehouse)
                .WithMany()
                .HasForeignKey(o => o.WarehouseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasOptional(o => o.Sale)
                .WithMany()
                .HasForeignKey(o => o.SaleId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProductImage>()
                .HasRequired(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .WillCascadeOnDelete(true);


            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .WillCascadeOnDelete(false);


            // ================================
            // MESSAGE - RECEIVER
            // ================================

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .WillCascadeOnDelete(false);


            // ================================
            // NOTIFICATION - USER
            // ================================

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(false);


            base.OnModelCreating(modelBuilder);

        }
    }
   
}