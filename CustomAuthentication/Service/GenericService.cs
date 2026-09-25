using CustomAuthentication.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Service
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        protected readonly IGenericRepository<T> repository;

        public GenericService(IGenericRepository<T> repo)
        {
            repository = repo;
        }

        public List<T> GetAll()
        {
            return repository.GetAll();
        }

        public T GetById(int id)
        {
            return repository.GetById(id);
        }

        public void Add(T entity)
        {
            repository.Add(entity);
            repository.Save();
        }

        public void Update(T entity)
        {
            repository.Update(entity);
            repository.Save();
        }

        public void Delete(int id)
        {
            var entity = repository.GetById(id);
            if (entity != null)
            {
                repository.Delete(entity);
                repository.Save();
            }
        }
    

    }
}