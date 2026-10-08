namespace OrdersProcessor.Models;

public sealed class Order
{
    public const string CompletedStatus = "completed";
    public const string CancelledStatus = "cancelled";

    public int OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];

    public decimal Total => Items.Sum(i => i.Subtotal);

    public bool HasStatus(string status) =>
        string.Equals(Status.Trim(), status, StringComparison.OrdinalIgnoreCase);

    public bool IsCompleted => HasStatus(CompletedStatus);
    public bool IsCancelled => HasStatus(CancelledStatus);
}
