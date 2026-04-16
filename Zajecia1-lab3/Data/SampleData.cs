using OrderFlow.Models;

namespace OrderFlow.Data;

public static class SampleData
{
    public static List<Product> Products = new()
    {
        new Product { Id = 1, Name = "Shure SM7B", Price = 1899m, Category = "Audio" },
        new Product { Id = 2, Name = "Kabel XLR", Price = 50m, Category = "Audio" },
        new Product { Id = 3, Name = "Monitor 27\"", Price = 1200m, Category = "Electronics" },
        new Product { Id = 4, Name = "Klawiatura mechaniczna", Price = 300m, Category = "Electronics" },
        new Product { Id = 5, Name = "Biurko", Price = 800m, Category = "Furniture" }
    };

    public static List<Customer> Customers = new()
    {
        new Customer { Id = 1, FullName = "Jan Kowalski", IsVip = false },
        new Customer { Id = 2, FullName = "Anna Nowak", IsVip = true },
        new Customer { Id = 3, FullName = "Piotr Zielinski", IsVip = false },
        new Customer { Id = 4, FullName = "Ola Wisniewska", IsVip = false }
    };

    public static List<Order> Orders = new()
    {
        new Order
        {
            Id = 1,
            Customer = Customers[0],
            Status = OrderStatus.New,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = Products[0], Quantity = 1 },
                new OrderItem { Product = Products[1], Quantity = 2 }
            }
        },

        new Order
        {
            Id = 2,
            Customer = Customers[1],
            Status = OrderStatus.Completed,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = Products[2], Quantity = 1 }
            }
        },

        new Order
        {
            Id = 3,
            Customer = Customers[2],
            Status = OrderStatus.Processing,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = Products[3], Quantity = 2 }
            }
        },

        new Order
        {
            Id = 4,
            Customer = Customers[1],
            Status = OrderStatus.Validated,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = Products[4], Quantity = 1 }
            }
        },

        new Order
        {
            Id = 5,
            Customer = Customers[3],
            Status = OrderStatus.Cancelled,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = Products[1], Quantity = 5 }
            }
        },

        new Order
        {
            Id = 6,
            Customer = Customers[0],
            Status = OrderStatus.Completed,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = Products[2], Quantity = 2 },
                new OrderItem { Product = Products[3], Quantity = 1 }
            }
        }
    };
}