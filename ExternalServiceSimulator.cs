using OrderFlow.Models;

namespace OrderFlow.Services
{
    public class ExternalServiceSimulator
    {
        private static readonly Random _random = new();

        public async Task<bool> CheckInventoryAsync(Product product)
        {
            await Task.Delay(_random.Next(500, 1500));
            Console.WriteLine($"[Inventory] {product.Name} checked");
            return true;
        }

        public async Task<bool> ValidatePaymentAsync(Order order)
        {
            await Task.Delay(_random.Next(1000, 2000));
            Console.WriteLine($"[Payment] Order {order.Id} validated");
            return true;
        }

        public async Task<decimal> CalculateShippingAsync(Order order)
        {
            await Task.Delay(_random.Next(300, 800));
            var cost = _random.Next(20, 100);
            Console.WriteLine($"[Shipping] Order {order.Id}: {cost} PLN");
            return cost;
        }
    }
}