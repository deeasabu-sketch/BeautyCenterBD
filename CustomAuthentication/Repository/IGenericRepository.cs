using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CustomAuthentication.Repository
{
    public interface IGenericRepository<T> where T : class
    {
       
        
            IQueryable<T> Query(); // for callers that need .Include()/.Where() chains

            List<T> GetAll();

            T GetById(int id);

            List<T> Find(Expression<Func<T, bool>> predicate);

            void Add(T entity);

            void Update(T entity);

            void Delete(T entity);

            void Save();
        
    }
}
