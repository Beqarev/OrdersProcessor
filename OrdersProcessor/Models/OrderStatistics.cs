namespace OrdersProcessor.Models;

public sealed record OrderStatistics(
    int CompletedOrderCount,
    int SkippedCancelledCount,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    IReadOnlyList<string> TopProducts,
    int TopProductQuantity);
