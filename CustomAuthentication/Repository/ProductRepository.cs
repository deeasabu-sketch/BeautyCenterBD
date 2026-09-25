using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CustomAuthentication.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext db;

        public ProductRepository()
        {
            db = new AppDbContext();
        }

        public List<Product> GetAll()
        {
            return db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .OrderByDescending(x => x.ProductId)
                .ToList();
        }

        public List<Product> GetAllActive()
        {
            return db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ProductId)
                .ToList();
        }

        public Product GetById(int id)
        {
            return db.Products
                .FirstOrDefault(x => x.ProductId == id);
        }

        public Product GetByIdWithDetails(int id)
        {
            return db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Include(p => p.ProductImages)
                .FirstOrDefault(x => x.ProductId == id);
        }

        public void Add(Product product)
        {
            db.Products.Add(product);
            db.SaveChanges();
        }

        public void Update(Product product)
        {
            var existingProduct = db.Products
                .FirstOrDefault(x => x.ProductId == product.ProductId);

            if (existingProduct == null)
            {
                return;
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.SKU = product.SKU;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.UnitId = product.UnitId;
            existingProduct.PurchasePrice = product.PurchasePrice;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.DiscountPercent = product.DiscountPercent;
            existingProduct.ReorderLevel = product.ReorderLevel;
            existingProduct.Description = product.Description;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.ImagePath = product.ImagePath;
            existingProduct.IsActive = product.IsActive;
            existingProduct.CreateDate = product.CreateDate;

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            Product product = GetById(id);

            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
        }
    }
}
