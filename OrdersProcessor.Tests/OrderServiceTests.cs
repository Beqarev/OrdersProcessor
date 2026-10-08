using OrdersProcessor.Models;
using OrdersProcessor.Services;

namespace OrdersProcessor.Tests;

public class OrderServiceTests
{
    private static Order MakeOrder(int id, string customer, string status, params (string Product, int Qty, decimal Price)[] items) =>
        new()
        {
            OrderId = id,
            Customer = customer,
            Status = status,
            Items = items.Select(i => new OrderItem { Product = i.Product, Quantity = i.Qty, Price = i.Price }).ToList()
        };

    [Fact]
    public void Statistics_IgnoreCancelledOrders()
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", "completed", ("Mouse", 1, 25m)),
            MakeOrder(2, "Giorgi", "cancelled", ("Keyboard", 10, 50m))
        ]);

        var stats = service.GetStatistics();

        Assert.Equal(1, stats.CompletedOrderCount);
        Assert.Equal(1, stats.SkippedCancelledCount);
        Assert.Equal(25m, stats.TotalRevenue);
        Assert.Equal(["Mouse"], stats.TopProducts);
    }

    [Fact]
    public void Statistics_CalculateRevenueAndAveragePerOrder()
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", "completed", ("Keyboard", 2, 50m), ("Mouse", 1, 25m)),
            MakeOrder(2, "Ana", "completed", ("Monitor", 1, 200m))
        ]);

        var stats = service.GetStatistics();

        Assert.Equal(2, stats.CompletedOrderCount);
        Assert.Equal(325m, stats.TotalRevenue);
        Assert.Equal(162.5m, stats.AverageOrderValue);
    }

    [Fact]
    public void Statistics_TopProductIsByQuantityNotRevenue()
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", "completed", ("Monitor", 1, 200m), ("Mouse", 3, 25m))
        ]);

        var stats = service.GetStatistics();

        Assert.Equal(["Mouse"], stats.TopProducts);
        Assert.Equal(3, stats.TopProductQuantity);
    }

    [Fact]
    public void Statistics_ReturnAllProductsOnTie()
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", "completed", ("Mouse", 2, 25m), ("Keyboard", 2, 50m))
        ]);

        var stats = service.GetStatistics();

        Assert.Equal(["Keyboard", "Mouse"], stats.TopProducts);
        Assert.Equal(2, stats.TopProductQuantity);
    }

    [Fact]
    public void Statistics_WithNoCompletedOrders_AreZeroWithoutDivisionByZero()
    {
        var service = new OrderService([MakeOrder(1, "Giorgi", "cancelled", ("Keyboard", 1, 50m))]);

        var stats = service.GetStatistics();

        Assert.Equal(0, stats.CompletedOrderCount);
        Assert.Equal(0m, stats.TotalRevenue);
        Assert.Equal(0m, stats.AverageOrderValue);
        Assert.Empty(stats.TopProducts);
    }

    [Fact]
    public void Statistics_StatusComparisonIgnoresCaseAndWhitespace()
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", " Completed ", ("Mouse", 1, 25m)),
            MakeOrder(2, "Ana", "CANCELLED", ("Mouse", 1, 25m)),
            MakeOrder(3, "Ana", "pending", ("Mouse", 1, 25m))
        ]);

        var stats = service.GetStatistics();

        Assert.Equal(1, stats.CompletedOrderCount);
        Assert.Equal(1, stats.SkippedCancelledCount);
    }

    [Theory]
    [InlineData("Nino", 2)]
    [InlineData("nino", 2)]
    [InlineData("  NINO ", 2)]
    [InlineData("Giorgi", 1)]
    [InlineData("Nin", 0)]
    [InlineData("Unknown", 0)]
    [InlineData("", 0)]
    public void FindByCustomer_MatchesExactNameIgnoringCase(string query, int expectedCount)
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", "completed"),
            MakeOrder(2, "Giorgi", "cancelled"),
            MakeOrder(3, "Nino", "completed")
        ]);

        Assert.Equal(expectedCount, service.FindByCustomer(query).Count);
    }

    [Fact]
    public void GetAll_ReturnsEveryOrderIncludingCancelled()
    {
        var service = new OrderService([
            MakeOrder(1, "Nino", "completed"),
            MakeOrder(2, "Giorgi", "cancelled")
        ]);

        Assert.Equal([1, 2], service.GetAll().Select(o => o.OrderId));
    }
}
