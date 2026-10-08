using System.Text.Json;
using OrdersProcessor.Models;

namespace OrdersProcessor.Services;

public sealed class OrderDataException(string message, Exception? inner = null) : Exception(message, inner);

public static class OrderLoader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static List<Order> LoadFromFile(string path)
    {
        if (!File.Exists(path))
            throw new OrderDataException($"ფაილი ვერ მოიძებნა: {path}");

        return Parse(File.ReadAllText(path));
    }

    public static List<Order> Parse(string json)
    {
        List<Order?>? orders;
        try
        {
            orders = JsonSerializer.Deserialize<List<Order?>>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new OrderDataException($"JSON-ის ფორმატი არასწორია: {ex.Message}", ex);
        }

        if (orders is null)
            throw new OrderDataException("JSON ფაილი ცარიელია ან შეკვეთების მასივს არ შეიცავს.");

        var result = new List<Order>();
        foreach (var order in orders)
        {
            if (order is null)
                throw new OrderDataException("JSON-ში შეკვეთის ნაცვლად null მნიშვნელობაა.");

            order.Items ??= [];
            order.Customer ??= string.Empty;
            order.Status ??= string.Empty;
            Validate(order);
            result.Add(order);
        }

        return result;
    }

    private static void Validate(Order order)
    {
        foreach (var item in order.Items)
        {
            if (item is null)
                throw new OrderDataException($"შეკვეთა #{order.OrderId}: პროდუქტის ჩანაწერი null-ია.");
            if (string.IsNullOrWhiteSpace(item.Product))
                throw new OrderDataException($"შეკვეთა #{order.OrderId}: პროდუქტის სახელი ცარიელია.");
            if (item.Quantity < 0)
                throw new OrderDataException($"შეკვეთა #{order.OrderId}: '{item.Product}'-ის რაოდენობა უარყოფითია.");
            if (item.Price < 0)
                throw new OrderDataException($"შეკვეთა #{order.OrderId}: '{item.Product}'-ის ფასი უარყოფითია.");
        }
    }
}
