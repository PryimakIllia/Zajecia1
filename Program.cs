using OrderFlow.Data;
using OrderFlow.Services;
using OrderFlow.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;

// ===== Zadanie 1 — Zdarzenia (Events) =====
var pipeline = new OrderPipeline();

// subscribers
pipeline.StatusChanged += OrderSubscribers.LogStatus;
pipeline.StatusChanged += OrderSubscribers.SendEmail;
pipeline.ValidationCompleted += OrderSubscribers.ValidationLogger;

Console.WriteLine("=== ORDER PROCESSING WITH EVENTS ===\n");

// Проганяємо 2-3 замовлення
foreach (var order in SampleData.Orders.Take(3))
{
    Console.WriteLine($"\nProcessing Order {order.Id}");
    pipeline.ProcessOrder(order);
    Console.WriteLine("----------------------");
}

// ===== Zadanie 2 — Delegaty i walidacja zamówień =====
var validator = new OrderValidator();

Console.WriteLine("\n=== ALL ORDERS VALIDATION ===\n");

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

// test błędnego zamówienia
var badOrder = new Order
{
    Id = 99,
    Customer = SampleData.Customers[0],
    Status = OrderStatus.Cancelled,
    Items = new List<OrderItem>()
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

// Aggregates
decimal totalSum = processor.Aggregate(orders => orders.Sum(o => o.TotalAmount));
decimal avg = processor.Aggregate(orders => orders.Average(o => o.TotalAmount));
decimal max = processor.Aggregate(orders => orders.Max(o => o.TotalAmount));

Console.WriteLine($"\n=== Aggregates: Sum = {totalSum}, Avg = {avg:F2}, Max = {max} ===");

// Chain operations
Console.WriteLine("\n=== Top 3 high value orders (chain) ===");
processor.ProcessTopOrders(
    filter: o => o.TotalAmount > 500,
    keySelector: o => o.TotalAmount,
    topN: 3,
    action: printSummary
);

// ===== Zadanie 4 — LINQ =====
Console.WriteLine("\n=== Zadanie 4 — LINQ Queries ===\n");

var ordersByCustomer = from o in SampleData.Orders
                       join c in SampleData.Customers on o.Customer.Id equals c.Id
                       group o by c.FullName into g
                       select new { Customer = g.Key, Orders = g.ToList() };

Console.WriteLine("=== Orders by Customer ===");
foreach (var group in ordersByCustomer)
    Console.WriteLine($"{group.Customer}: {group.Orders.Count} orders");

var allProducts = SampleData.Orders
    .SelectMany(o => o.Items, (order, item) => item.Product)
    .Distinct()
    .ToList();

Console.WriteLine("\n=== All Products ===");
foreach (var p in allProducts)
    Console.WriteLine(p.Name);

var topCustomers = SampleData.Orders
    .GroupBy(o => o.Customer.FullName)
    .Select(g => new { Customer = g.Key, Total = g.Sum(o => o.TotalAmount) })
    .OrderByDescending(x => x.Total)
    .ToList();

Console.WriteLine("\n=== Top Customers ===");
foreach (var c in topCustomers)
    Console.WriteLine($"{c.Customer} — {c.Total} PLN");

var customersWithOrders = SampleData.Customers
    .GroupJoin(SampleData.Orders,
               c => c.Id,
               o => o.Customer.Id,
               (c, orders) => new { Customer = c.FullName, Orders = orders })
    .ToList();

Console.WriteLine("\n=== Customers with Orders ===");
foreach (var c in customersWithOrders)
    Console.WriteLine($"{c.Customer} — {c.Orders.Count()} orders");

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

Console.WriteLine("\n=== Favorite Category per Customer ===");
foreach (var r in favoriteCategoryReport)
    Console.WriteLine($"{r.Customer} — {r.FavoriteCategory}");

var expensiveProducts = SampleData.Orders
    .Where(o => o.TotalAmount > 1000)
    .SelectMany(o => o.Items.Select(i => i.Product))
    .Distinct()
    .ToList();

Console.WriteLine("\n=== Expensive Products ===");
foreach (var p in expensiveProducts)
    Console.WriteLine(p.Name);

// ===== Zadanie 2 — Async =====
var asyncProcessor = new AsyncOrderProcessor();

Console.WriteLine("\n=== SEQUENTIAL PROCESSING ===");
var sw1 = Stopwatch.StartNew();

foreach (var order in SampleData.Orders.Take(3))
{
    await asyncProcessor.ProcessOrderAsync(order);
}

sw1.Stop();
Console.WriteLine($"Sequential time: {sw1.ElapsedMilliseconds} ms");

Console.WriteLine("\n=== PARALLEL PROCESSING (max 3) ===");
var sw2 = Stopwatch.StartNew();

await asyncProcessor.ProcessMultipleOrdersAsync(SampleData.Orders.Take(6).ToList());

sw2.Stop();
Console.WriteLine($"Parallel time: {sw2.ElapsedMilliseconds} ms");

// ===== Zadanie 3 — Thread safety =====
Console.WriteLine("\n=== Zadanie 3: Thread safety demo ===");


// Demo bez synchro

/*
var stats = new OrderStatistics();

Parallel.ForEach(SampleData.Orders, order =>
{
    if (order.Items == null || !order.Items.Any())
        stats.ProcessingErrors.Add($"Order {order.Id} has no items");

    stats.TotalProcessed++;
    stats.TotalRevenue += order.TotalAmount;

    if (!stats.OrdersPerStatus.ContainsKey(order.Status))
        stats.OrdersPerStatus[order.Status] = 0;

    stats.OrdersPerStatus[order.Status]++;
});

Console.WriteLine("\n-- Without synchronization --");
Console.WriteLine($"Processed: {stats.TotalProcessed}, Revenue: {stats.TotalRevenue}");
foreach (var kv in stats.OrdersPerStatus)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
Console.WriteLine($"Errors: {string.Join(", ", stats.ProcessingErrors)}");
*/
// Demo з thread-safe
var safeStats = new ThreadSafeOrderStatistics();

Parallel.ForEach(SampleData.Orders, order =>
{
    safeStats.AddOrder(order);
});

Console.WriteLine("\n-- Thread-safe version --");
Console.WriteLine($"Processed: {safeStats.TotalProcessed}, Revenue: {safeStats.TotalRevenue}");
foreach (var kv in safeStats.OrdersPerStatus)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
Console.WriteLine($"Errors: {string.Join(", ", safeStats.ProcessingErrors)}");