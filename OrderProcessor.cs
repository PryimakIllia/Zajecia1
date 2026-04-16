using OrderFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderFlow.Services
{
    public class OrderProcessor
    {
        private readonly List<Order> _orders;

        public OrderProcessor(List<Order> orders)
        {
            _orders = orders;
        }

        // =============================
        // Predicate — filtrowanie
        // =============================
        public List<Order> Filter(Predicate<Order> predicate)
        {
            return _orders.FindAll(predicate);
        }

        // =============================
        // Action — wykonanie akcji
        // =============================
        public void Apply(Action<Order> action)
        {
            foreach (var order in _orders)
                action(order);
        }

        // =============================
        // Func — projekcja
        // =============================
        public List<T> Map<T>(Func<Order, T> selector)
        {
            return _orders.Select(selector).ToList();
        }

        // =============================
        // Agregacja
        // =============================
        public decimal Aggregate(Func<IEnumerable<Order>, decimal> aggregator)
        {
            return aggregator(_orders);
        }

        // =============================
        // Łańcuch: filtruj; sortuj; weź top N; wypisz
        // =============================
        public void ProcessTopOrders(Predicate<Order> filter, Func<Order, decimal> keySelector, int topN, Action<Order> action)
        {
            _orders
                .FindAll(filter)                 // Predicate; filtrowanie
                .OrderByDescending(keySelector)  // Func; wybór klucza do sortowania
                .Take(topN)                      // top N
                .ToList()
                .ForEach(action);                // Action; wypisz / zmień status
        }
    }
}