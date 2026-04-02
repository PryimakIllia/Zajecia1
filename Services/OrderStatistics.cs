using OrderFlow.Models;
using System.Collections.Generic;
namespace OrderFlow.Services{
public class OrderStatistics
{
    public int TotalProcessed;
    public decimal TotalRevenue;
    public Dictionary<OrderStatus, int> OrdersPerStatus = new();
    public List<string> ProcessingErrors = new();
}
}