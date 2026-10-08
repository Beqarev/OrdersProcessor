using OrdersProcessor.Services;

namespace OrdersProcessor.Tests;

public class OrderLoaderTests
{
    [Fact]
    public void ProvidedOrdersFile_ProducesExpectedStatistics()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "orders.json");
        var service = new OrderService(OrderLoader.LoadFromFile(path));

        var stats = service.GetStatistics();

        Assert.Equal(4, service.GetAll().Count);
        Assert.Equal(3, stats.CompletedOrderCount);
        Assert.Equal(1, stats.SkippedCancelledCount);
        Assert.Equal(425m, stats.TotalRevenue);
        Assert.Equal(141.67m, Math.Round(stats.AverageOrderValue, 2));
        Assert.Equal(["Keyboard", "Mouse"], stats.TopProducts);
        Assert.Equal(3, stats.TopProductQuantity);
    }

    [Fact]
    public void Parse_TreatsNullItemsAsEmpty()
    {
        var orders = OrderLoader.Parse("""[{ "orderId": 1, "customer": "Nino", "status": "completed", "items": null }]""");

        Assert.Empty(Assert.Single(orders).Items);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("""[{ "orderId": 1, "status": "completed", "items": [{ "product": "Mouse", "quantity": -1, "price": 25 }] }]""")]
    [InlineData("""[{ "orderId": 1, "status": "completed", "items": [{ "product": "Mouse", "quantity": 1, "price": -5 }] }]""")]
    [InlineData("""[{ "orderId": 1, "status": "completed", "items": [{ "product": "", "quantity": 1, "price": 5 }] }]""")]
    public void Parse_RejectsInvalidData(string json)
    {
        Assert.Throws<OrderDataException>(() => OrderLoader.Parse(json));
    }

    [Fact]
    public void LoadFromFile_MissingFile_Throws()
    {
        Assert.Throws<OrderDataException>(() => OrderLoader.LoadFromFile("does-not-exist.json"));
    }
}
