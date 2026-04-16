using OrderFlow.Models;

namespace OrderFlow.Services
{
    public static class OrderSubscribers
    {
        // 1. Logger
        public static void LogStatus(object? sender, OrderStatusChangedEventArgs e)
        {
            Console.WriteLine($"[LOG] Order {e.Order.Id}: {e.OldStatus} → {e.NewStatus} at {e.Timestamp}");
        }

        // 2. Email simulation
        public static void SendEmail(object? sender, OrderStatusChangedEventArgs e)
        {
            Console.WriteLine($"[EMAIL] Customer {e.Order.Customer.FullName} notified about status: {e.NewStatus}");
        }

        // 3. Validation logger
        public static void ValidationLogger(object? sender, OrderValidationEventArgs e)
        {
            if (e.IsValid)
            {
                Console.WriteLine($"[VALIDATION] Order {e.Order.Id} is valid");
            }
            else
            {
                Console.WriteLine($"[VALIDATION] Order {e.Order.Id} has errors:");
                foreach (var err in e.Errors)
                    Console.WriteLine($"   - {err}");
            }
        }
    }
}