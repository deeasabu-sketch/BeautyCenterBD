using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomAuthentication.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext db;

        public OrderRepository()
        {
            db = new AppDbContext();
        }

        public List<Order> GetAll()
        {
            return db.Orders
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public Order GetById(int id)
        {
            return db.Orders
                .FirstOrDefault(x => x.OrderId == id);
        }

        public List<Order> GetByDate(DateTime startDate, DateTime endDate)
        {
            return db.Orders
                .Where(x => x.OrderDate >= startDate &&
                            x.OrderDate < endDate)
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public int GetTotalOrders(DateTime startDate, DateTime endDate)
        {
            return db.Orders
                .Count(x => x.OrderDate >= startDate &&
                            x.OrderDate < endDate);
        }

        public decimal GetTotalAmount(DateTime startDate, DateTime endDate)
        {
            return db.Orders
                .Where(x => x.OrderDate >= startDate &&
                            x.OrderDate < endDate)
                .Select(x => (decimal?)x.TotalAmount)
                .Sum() ?? 0;
        }

        public List<MonthlySales> GetLast12MonthsSales()
        {
            DateTime currentMonth =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1
                );

            DateTime startDate =
                currentMonth.AddMonths(-11);

            DateTime endDate =
                currentMonth.AddMonths(1);

            // Only Completed Orders
            var sales = db.Orders
                .Where(x =>
                    x.OrderDate >= startDate &&
                    x.OrderDate < endDate &&
                    x.OrderStatus == "Completed"
                )
                .GroupBy(x => new
                {
                    x.OrderDate.Year,
                    x.OrderDate.Month
                })
                .Select(g => new MonthlySales
                {
                    Year = g.Key.Year,

                    Month = g.Key.Month,

                    TotalAmount =
                        g.Sum(x => x.TotalAmount),

                    TotalOrders =
                        g.Count()
                })
                .ToList();

            // Create all 12 months
            var result = new List<MonthlySales>();

            for (int i = 0; i < 12; i++)
            {
                DateTime month =
                    startDate.AddMonths(i);

                var existing =
                    sales.FirstOrDefault(x =>
                        x.Year == month.Year &&
                        x.Month == month.Month
                    );

                result.Add(new MonthlySales
                {
                    Year = month.Year,

                    Month = month.Month,

                    MonthName =
                        month.ToString("MMM yyyy"),

                    TotalAmount =
                        existing != null
                            ? existing.TotalAmount
                            : 0,

                    TotalOrders =
                        existing != null
                            ? existing.TotalOrders
                            : 0
                });
            }

            return result;
        }
        
        public void Add(Order order)
        {
            db.Orders.Add(order);
            db.SaveChanges();
        }

        public void Update(Order order)
        {
            db.Entry(order).State =
                System.Data.Entity.EntityState.Modified;

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            Order order = GetById(id);

            if (order != null)
            {
                db.Orders.Remove(order);
                db.SaveChanges();
            }
        }
    }
}