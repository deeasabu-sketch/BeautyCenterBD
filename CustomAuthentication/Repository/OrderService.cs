using CustomAuthentication.Models;
using CustomAuthentication.Repository;
using System;
using System.Collections.Generic;

namespace CustomAuthentication.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository repository;

        public OrderService()
        {
            repository = new OrderRepository();
        }

        public List<Order> GetAllOrders()
        {
            return repository.GetAll();
        }

        public Order GetOrderById(int id)
        {
            return repository.GetById(id);
        }

        public List<Order> GetOrdersByDate(
            DateTime startDate,
            DateTime endDate)
        {
            return repository.GetByDate(startDate, endDate);
        }

        public int GetTotalOrders(
            DateTime startDate,
            DateTime endDate)
        {
            return repository.GetTotalOrders(
                startDate,
                endDate);
        }

        public decimal GetTotalAmount(
            DateTime startDate,
            DateTime endDate)
        {
            return repository.GetTotalAmount(
                startDate,
                endDate);
        }

        public void AddOrder(Order order)
        {
            repository.Add(order);
        }

        public void UpdateOrder(Order order)
        {
            repository.Update(order);
        }

        public void DeleteOrder(int id)
        {
            repository.Delete(id);
        }
        public List<MonthlySales> GetLast12MonthsSales()
        {
            return repository.GetLast12MonthsSales();
        }
    }
}