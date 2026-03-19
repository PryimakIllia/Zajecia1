using OrderFlow.Models;

namespace OrderFlow.Models;

public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; }

    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}