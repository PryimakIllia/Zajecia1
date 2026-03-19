using OrderFlow.Data;
using OrderFlow.Services;
using OrderFlow.Models;

var validator = new OrderValidator();

// ===== Zadanie 2 — Delegaty i walidacja zamówień =====
// Sprawdzamy wszystkie zamówienia i pokazujemy, które są poprawne, a które nie
Console.WriteLine("=== ALL ORDERS VALIDATION ===\n");

foreach (var order in SampleData.Orders)
{
    Console.WriteLine($"Order {order.Id}");
    Console.WriteLine($"Customer: {order.Customer.FullName}");
    Console.WriteLine($"Status: {order.Status}");
    Console.WriteLine($"Total: {order.TotalAmount} PLN");

    var errors = validator.ValidateAll(order);

    if (errors.Count == 0)
        Console.WriteLine("✔ Valid");
    else
    {
        Console.WriteLine("❌ Validation errors:");
        foreach (var e in errors)
            Console.WriteLine($"   - {e}");
    }

    Console.WriteLine("----------------------");
}

// test "złe" zamówienie — puste pozycje, status cancelled
var badOrder = new Order
{
    Id = 99,
    Customer = SampleData.Customers[0],
    Status = OrderStatus.Cancelled,
    Items = new List<OrderItem>() // puste
};

Console.WriteLine("\n=== BAD ORDER TEST ===");
var badErrors = validator.ValidateAll(badOrder);

if (badErrors.Count == 0)
    Console.WriteLine("✔ Valid");
else
{
    Console.WriteLine("❌ Validation errors:");
    foreach (var e in badErrors)
        Console.WriteLine($"   - {e}");
}

// ===== Zadanie 3 — Action, Func, Predicate =====
var processor = new OrderProcessor(SampleData.Orders);

// Predicate
Predicate<Order> highValue = o => o.TotalAmount > 1000;
Predicate<Order> vipCustomer = o => o.Customer.IsVip;
Predicate<Order> completedOrders = o => o.Status == OrderStatus.Completed;

Console.WriteLine("\n=== High value orders (Predicate) ===");
var filtered = processor.Filter(highValue);
foreach (var o in filtered)
    Console.WriteLine($"Order {o.Id} — {o.TotalAmount} PLN");

// Action
Action<Order> printSummary = o => Console.WriteLine($"Order {o.Id} ({o.Customer.FullName}) — {o.TotalAmount} PLN");
Action<Order> markValidated = o => o.Status = OrderStatus.Validated;

Console.WriteLine("\n=== Applying printSummary Action ===");
processor.Apply(printSummary);

Console.WriteLine("\n=== Marking all orders as Validated ===");
processor.Apply(markValidated);

// Func
var summary = processor.Map(o => new { o.Id, Customer = o.Customer.FullName, o.TotalAmount });
Console.WriteLine("\n=== Order summary (Func) ===");
foreach (var s in summary)
    Console.WriteLine($"Order {s.Id} — {s.Customer} — {s.TotalAmount} PLN");

// Agregacja
decimal totalSum = processor.Aggregate(orders => orders.Sum(o => o.TotalAmount));
decimal avg = processor.Aggregate(orders => orders.Average(o => o.TotalAmount));
decimal max = processor.Aggregate(orders => orders.Max(o => o.TotalAmount));
Console.WriteLine($"\n=== Aggregates: Sum = {totalSum}, Avg = {avg:F2}, Max = {max} ===");

// Łańcuch operacji: filtruj; sortuj; top N; wypisz
Console.WriteLine("\n=== Top 3 high value orders (chain) ===");
processor.ProcessTopOrders(
    filter: o => o.TotalAmount > 500,
    keySelector: o => o.TotalAmount,
    topN: 3,
    action: printSummary
);

// ===== Zadanie 4 — LINQ =====
Console.WriteLine("\n=== Zadanie 4 — LINQ Queries ===\n");

// 1. query syntax
var ordersByCustomer = from o in SampleData.Orders
                       join c in SampleData.Customers on o.Customer.Id equals c.Id
                       group o by c.FullName into g
                       select new { Customer = g.Key, Orders = g.ToList() };

Console.WriteLine("=== Orders by Customer (query syntax) ===");
foreach (var group in ordersByCustomer)
    Console.WriteLine($"{group.Customer}: {group.Orders.Count} orders");

// 2. SelectMany 
var allProducts = SampleData.Orders
    .SelectMany(o => o.Items, (order, item) => item.Product)
    .Distinct()
    .ToList();

Console.WriteLine("\n=== All Products (SelectMany) ===");
foreach (var p in allProducts)
    Console.WriteLine(p.Name);

// 3. GroupBy z agregacją 
var topCustomers = SampleData.Orders
    .GroupBy(o => o.Customer.FullName)
    .Select(g => new { Customer = g.Key, Total = g.Sum(o => o.TotalAmount) })
    .OrderByDescending(x => x.Total)
    .ToList();

Console.WriteLine("\n=== Top Customers by TotalAmount ===");
foreach (var c in topCustomers)
    Console.WriteLine($"{c.Customer} — {c.Total} PLN");

// 4. GroupJoin
var customersWithOrders = SampleData.Customers
    .GroupJoin(SampleData.Orders,
               c => c.Id,
               o => o.Customer.Id,
               (c, orders) => new { Customer = c.FullName, Orders = orders })
    .ToList();

Console.WriteLine("\n=== Customers with Orders (GroupJoin) ===");
foreach (var c in customersWithOrders)
    Console.WriteLine($"{c.Customer} — {c.Orders.Count()} orders");

// 5. Mixed syntax 
var favoriteCategoryReport = (from o in SampleData.Orders
                              from item in o.Items
                              group item by o.Customer.FullName into g
                              select new
                              {
                                  Customer = g.Key,
                                  FavoriteCategory = g.GroupBy(i => i.Product.Category)
                                                      .OrderByDescending(gr => gr.Count())
                                                      .First().Key
                              })
                             .ToList();

Console.WriteLine("\n=== Favorite Category per Customer (Mixed syntax) ===");
foreach (var r in favoriteCategoryReport)
    Console.WriteLine($"{r.Customer} — {r.FavoriteCategory}");

// 6. Produkty w zamówieniach o wartości > 1000 PLN
var expensiveProducts = SampleData.Orders
    .Where(o => o.TotalAmount > 1000)
    .SelectMany(o => o.Items.Select(i => i.Product))
    .Distinct()
    .ToList();

Console.WriteLine("\n=== Expensive Products (TotalAmount > 1000) ===");
foreach (var p in expensiveProducts)
    Console.WriteLine(p.Name);