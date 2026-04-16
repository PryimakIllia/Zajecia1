using OrderFlow.Models;

namespace OrderFlow.Models
{
    public class OrderStatusChangedEventArgs : EventArgs
    {
        public Order Order { get; set; } = null!;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}