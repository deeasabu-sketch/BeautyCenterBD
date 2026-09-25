using CustomAuthentication.Models;
using CustomAuthentication.Repository;
using System.Collections.Generic;

namespace CustomAuthentication.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repository;

        public ProductService()
        {
            repository = new ProductRepository();
        }

        public List<Product> GetAllProducts()
        {
            return repository.GetAll();
        }

        public List<Product> GetAllActiveProducts()
        {
            return repository.GetAllActive();
        }

        public Product GetProductById(int id)
        {
            return repository.GetById(id);
        }

        public Product GetProductByIdWithDetails(int id)
        {
            return repository.GetByIdWithDetails(id);
        }

        public void AddProduct(Product product)
        {
            repository.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            repository.Update(product);
        }

        public void DeleteProduct(int id)
        {
            repository.Delete(id);
        }
    }
}
