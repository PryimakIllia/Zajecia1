using OrderFlow.Models;

namespace OrderFlow.Services;

// wlasny delegat
public delegate bool ValidationRule(Order order, out string errorMessage);

public class OrderValidator
{
    private readonly List<ValidationRule> _rules = new();

    private readonly List<Func<Order, bool>> _funcRules = new();

    public OrderValidator()
    {
        // dodajemy named methods
        _rules.Add(HasItems);
        _rules.Add(ValidQuantity);
        _rules.Add(MaxAmount);

        // dodajemy labmda rules
        _funcRules.Add(o => o.Status != OrderStatus.Cancelled);
        _funcRules.Add(o => o.TotalAmount < 10000);
    }

    // glowny method
    public List<string> ValidateAll(Order order)
    {
        List<string> errors = new();

        // delegaty
        foreach (var rule in _rules)
        {
            if (!rule(order, out string error))
                errors.Add(error);
        }

        // func rules
        foreach (var rule in _funcRules)
        {
            if (!rule(order))
                errors.Add("Lambda rule failed");
        }

        return errors;
    }

    // =====================
    // NAMED METHODS
    // =====================

    private bool HasItems(Order order, out string error)
    {
        if (order.Items.Count == 0)
        {
            error = "Order has no items";
            return false;
        }

        error = "";
        return true;
    }

    private bool ValidQuantity(Order order, out string error)
    {
        if (order.Items.Any(i => i.Quantity <= 0))
        {
            error = "Invalid quantity (<=0)";
            return false;
        }

        error = "";
        return true;
    }

    private bool MaxAmount(Order order, out string error)
    {
        if (order.TotalAmount > 5000)
        {
            error = "Order exceeds max amount (5000)";
            return false;
        }

        error = "";
        return true;
    }
}