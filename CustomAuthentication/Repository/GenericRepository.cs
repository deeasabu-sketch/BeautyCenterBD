using CustomAuthentication.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace CustomAuthentication.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext db;
        protected readonly DbSet<T> dbSet;

        public GenericRepository(AppDbContext context)
        {
            db = context;
            dbSet = db.Set<T>();
        }

        public IQueryable<T> Query()
        {
            return dbSet.AsQueryable();
        }

        public List<T> GetAll()
        {
            return dbSet.ToList();
        }

        public T GetById(int id)
        {
            return dbSet.Find(id);
        }

        public List<T> Find(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Where(predicate).ToList();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            db.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public void Save()
        {
            db.SaveChanges();
        }
    }
}