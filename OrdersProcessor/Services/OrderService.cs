using OrdersProcessor.Models;

namespace OrdersProcessor.Services;

public sealed class OrderService(IEnumerable<Order> orders)
{
    private readonly List<Order> _orders = orders.ToList();

    public IReadOnlyList<Order> GetAll() => _orders;

    public IReadOnlyList<Order> FindByCustomer(string customer)
    {
        var name = customer.Trim();
        if (name.Length == 0)
            return [];

        return _orders
            .Where(o => string.Equals(o.Customer.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public OrderStatistics GetStatistics()
    {
        var completed = _orders.Where(o => o.IsCompleted).ToList();
        var cancelledCount = _orders.Count(o => o.IsCancelled);

        var revenue = completed.Sum(o => o.Total);
        var average = completed.Count == 0 ? 0m : revenue / completed.Count;

        var quantities = completed
            .SelectMany(o => o.Items)
            .GroupBy(i => i.Product.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => (Product: g.Key, Quantity: g.Sum(i => i.Quantity)))
            .ToList();

        var topQuantity = quantities.Count == 0 ? 0 : quantities.Max(q => q.Quantity);
        var topProducts = topQuantity == 0
            ? []
            : quantities
                .Where(q => q.Quantity == topQuantity)
                .Select(q => q.Product)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToList();

        return new OrderStatistics(completed.Count, cancelledCount, revenue, average, topProducts, topQuantity);
    }
}
