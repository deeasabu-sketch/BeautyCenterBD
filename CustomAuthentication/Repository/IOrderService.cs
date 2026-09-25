using CustomAuthentication.Models;
using System;
using System.Collections.Generic;

namespace CustomAuthentication.Services
{
    public interface IOrderService
    {
        List<Order> GetAllOrders();

        Order GetOrderById(int id);

        List<Order> GetOrdersByDate(
            DateTime startDate,
            DateTime endDate);

        int GetTotalOrders(
            DateTime startDate,
            DateTime endDate);

        decimal GetTotalAmount(
            DateTime startDate,
            DateTime endDate);

        void AddOrder(Order order);

        void UpdateOrder(Order order);

        void DeleteOrder(int id);
        List<MonthlySales> GetLast12MonthsSales();
    }
}