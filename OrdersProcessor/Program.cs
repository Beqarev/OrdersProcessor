using System.Globalization;
using System.Text;
using OrdersProcessor.Models;
using OrdersProcessor.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var arguments = args.ToList();
var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "orders.json");

var fileFlag = arguments.IndexOf("--file");
if (fileFlag >= 0)
{
    if (fileFlag + 1 >= arguments.Count)
    {
        Console.Error.WriteLine("--file-ს უნდა მოჰყვეს ფაილის მისამართი.");
        return 1;
    }

    filePath = arguments[fileFlag + 1];
    arguments.RemoveRange(fileFlag, 2);
}

OrderService service;
try
{
    service = new OrderService(OrderLoader.LoadFromFile(filePath));
}
catch (OrderDataException ex)
{
    Console.Error.WriteLine($"შეცდომა: {ex.Message}");
    return 1;
}

if (arguments.Count == 0)
{
    RunMenu(service);
    return 0;
}

switch (arguments[0].ToLowerInvariant())
{
    case "list":
        PrintOrders(service.GetAll());
        return 0;
    case "customer" when arguments.Count >= 2:
        PrintCustomerOrders(service, string.Join(' ', arguments.Skip(1)));
        return 0;
    case "stats":
        PrintStatistics(service.GetStatistics());
        return 0;
    default:
        PrintUsage();
        return arguments[0] is "help" or "--help" or "-h" ? 0 : 1;
}

static void RunMenu(OrderService service)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== შეკვეთების დამუშავება ===");
        Console.WriteLine("1. ყველა შეკვეთის ჩვენება");
        Console.WriteLine("2. მომხმარებლის შეკვეთების ძებნა");
        Console.WriteLine("3. სტატისტიკა (დასრულებული შეკვეთები)");
        Console.WriteLine("0. გასვლა");
        Console.Write("აირჩიეთ: ");

        var choice = Console.ReadLine();
        if (choice is null)
            return;

        Console.WriteLine();
        switch (choice.Trim())
        {
            case "1":
                PrintOrders(service.GetAll());
                break;
            case "2":
                Console.Write("მომხმარებლის სახელი: ");
                PrintCustomerOrders(service, Console.ReadLine() ?? string.Empty);
                break;
            case "3":
                PrintStatistics(service.GetStatistics());
                break;
            case "0":
                return;
            default:
                Console.WriteLine("არასწორი არჩევანი.");
                break;
        }
    }
}

static void PrintCustomerOrders(OrderService service, string customer)
{
    var orders = service.FindByCustomer(customer);
    if (orders.Count == 0)
    {
        Console.WriteLine($"მომხმარებელს '{customer.Trim()}' შეკვეთები არ აქვს.");
        return;
    }

    PrintOrders(orders);
}

static void PrintOrders(IReadOnlyList<Order> orders)
{
    if (orders.Count == 0)
    {
        Console.WriteLine("შეკვეთები არ მოიძებნა.");
        return;
    }

    foreach (var order in orders)
    {
        Console.WriteLine($"#{order.OrderId,-4} {order.Customer,-12} {order.Status,-10} ჯამი: {order.Total:0.00}");
        foreach (var item in order.Items)
            Console.WriteLine($"      - {item.Product} x{item.Quantity} @ {item.Price:0.00} = {item.Subtotal:0.00}");
    }
}

static void PrintStatistics(OrderStatistics stats)
{
    var top = stats.TopProducts.Count == 0
        ? "—"
        : $"{string.Join(", ", stats.TopProducts)} ({stats.TopProductQuantity} ცალი)";
    var tieNote = stats.TopProducts.Count > 1 ? " [თანაბარი რაოდენობა]" : string.Empty;

    Console.WriteLine("სტატისტიკა (მხოლოდ დასრულებული შეკვეთები):");
    PrintRow("დასრულებული შეკვეთები", stats.CompletedOrderCount.ToString());
    PrintRow("გამოტოვებული გაუქმებული", stats.SkippedCancelledCount.ToString());
    PrintRow("გაყიდვების ჯამური თანხა", $"{stats.TotalRevenue:0.00}");
    PrintRow("საშუალო შეკვეთის ღირებულება", $"{stats.AverageOrderValue:0.00}");
    PrintRow("ყველაზე პოპულარული პროდუქტი", top + tieNote);

    static void PrintRow(string label, string value) => Console.WriteLine($"  {label + ":",-30} {value}");
}

static void PrintUsage()
{
    Console.WriteLine("""
        გამოყენება:
          dotnet run --project OrdersProcessor                      ინტერაქტიული მენიუ
          dotnet run --project OrdersProcessor -- list              ყველა შეკვეთა
          dotnet run --project OrdersProcessor -- customer <სახელი>  მომხმარებლის შეკვეთები
          dotnet run --project OrdersProcessor -- stats             სტატისტიკა
          დამატებით: --file <path> სხვა JSON ფაილის მისათითებლად
        """);
}
