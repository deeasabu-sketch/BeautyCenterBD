using CustomAuthentication.Models;
using System.Collections.Generic;

namespace CustomAuthentication.Repository
{
    public interface IProductRepository
    {
        List<Product> GetAll();

        List<Product> GetAllActive();

        Product GetById(int id);

        Product GetByIdWithDetails(int id);

        void Add(Product product);

        void Update(Product product);

        void Delete(int id);
    }
}
