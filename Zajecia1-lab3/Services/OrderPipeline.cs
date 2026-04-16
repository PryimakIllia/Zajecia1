using OrderFlow.Models;

namespace OrderFlow.Services
{
    public class OrderPipeline
    {
        public event EventHandler<OrderStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<OrderValidationEventArgs>? ValidationCompleted;

        public async Task ProcessOrderAsync(Order order)
        {
            await Task.Run(() => ProcessOrder(order));
        }

        public void ProcessOrder(Order order)
        {
            var errors = new List<string>();

            if (order.Items == null || !order.Items.Any())
                errors.Add("Order has no items");

            bool isValid = !errors.Any();

            ValidationCompleted?.Invoke(this, new OrderValidationEventArgs
            {
                Order = order,
                IsValid = isValid,
                Errors = errors
            });

            if (!isValid)
                return;

            ChangeStatus(order, OrderStatus.Validated);
            ChangeStatus(order, OrderStatus.Processing);
            ChangeStatus(order, OrderStatus.Completed);
        }

        private void ChangeStatus(Order order, OrderStatus newStatus)
        {
            var oldStatus = order.Status;
            order.Status = newStatus;

            StatusChanged?.Invoke(this, new OrderStatusChangedEventArgs
            {
                Order = order,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                Timestamp = DateTime.Now
            });
        }
    }
}