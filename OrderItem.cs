namespace OrderFlow.Models;

public class OrderItem
{
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }

    public decimal TotalPrice => Product.Price * Quantity;
}