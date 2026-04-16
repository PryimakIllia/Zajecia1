using OrderFlow.Models;
using System.Diagnostics;

namespace OrderFlow.Services
{
    public class AsyncOrderProcessor
    {
        private readonly ExternalServiceSimulator _service = new();

        public async Task ProcessOrderAsync(Order order)
        {
            Console.WriteLine($"\nProcessing order {order.Id}...");

            var stopwatch = Stopwatch.StartNew();

            var inventoryTasks = order.Items
                .Select(i => _service.CheckInventoryAsync(i.Product));

            var paymentTask = _service.ValidatePaymentAsync(order);
            var shippingTask = _service.CalculateShippingAsync(order);

            var tasks = new List<Task>();

            tasks.AddRange(inventoryTasks);
            tasks.Add(paymentTask);
            tasks.Add(shippingTask);

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            Console.WriteLine($"Order {order.Id} processed in {stopwatch.ElapsedMilliseconds} ms");
        }

        public async Task ProcessMultipleOrdersAsync(List<Order> orders)
        {
            var semaphore = new SemaphoreSlim(3);
            int completed = 0;

            var tasks = orders.Select(async order =>
            {
                await semaphore.WaitAsync();

                try
                {
                    await ProcessOrderAsync(order);

                    Interlocked.Increment(ref completed);
                    Console.WriteLine($"Progress: {completed}/{orders.Count}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}