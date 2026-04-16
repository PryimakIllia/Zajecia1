using OrderFlow.Models;

namespace OrderFlow.Models
{
    public class OrderValidationEventArgs : EventArgs
    {
        public Order Order { get; set; } = null!;
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}