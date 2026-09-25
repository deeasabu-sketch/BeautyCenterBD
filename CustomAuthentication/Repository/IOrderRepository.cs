using CustomAuthentication.Models;
using System;
using System.Collections.Generic;

namespace CustomAuthentication.Repository
{
    public interface IOrderRepository
    {
        List<Order> GetAll();

        Order GetById(int id);

        List<Order> GetByDate(DateTime startDate, DateTime endDate);

        int GetTotalOrders(DateTime startDate, DateTime endDate);

        decimal GetTotalAmount(DateTime startDate, DateTime endDate);

        List<MonthlySales> GetLast12MonthsSales();

        void Add(Order order);

        void Update(Order order);

        void Delete(int id);
    }
}