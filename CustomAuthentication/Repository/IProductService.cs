using CustomAuthentication.Models;
using System.Collections.Generic;

namespace CustomAuthentication.Services
{
    public interface IProductService
    {
        List<Product> GetAllProducts();

        List<Product> GetAllActiveProducts();

        Product GetProductById(int id);

        Product GetProductByIdWithDetails(int id);

        void AddProduct(Product product);

        void UpdateProduct(Product product);

        void DeleteProduct(int id);
    }
}
