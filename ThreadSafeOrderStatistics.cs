using OrderFlow.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ThreadSafeOrderStatistics
{
    public int TotalProcessed;
    public decimal TotalRevenue;
    public ConcurrentDictionary<OrderStatus, int> OrdersPerStatus = new();
    public List<string> ProcessingErrors = new();
    private readonly object _lock = new();

    public void AddOrder(Order order)
    {
        Interlocked.Increment(ref TotalProcessed);
        lock (_lock)
        {
            TotalRevenue += order.TotalAmount;
        }
        OrdersPerStatus.AddOrUpdate(order.Status, 1, (_, old) => old + 1);
        if (order.Items == null || !order.Items.Any())
        {
            lock (_lock)
            {
                ProcessingErrors.Add($"Order {order.Id} has no items");
            }
        }
    }
}