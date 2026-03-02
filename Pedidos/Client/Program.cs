using System.Net.Http.Json;
using System.Linq;

namespace WebApi.Client;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Cliente iniciando...");

        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:5001";
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

        Console.WriteLine("GET /api/products");
        var productsResp = await http.GetFromJsonAsync<ApiResponse<ProductListResponse>>("/api/products");
        if (productsResp is null)
        {
            Console.WriteLine("Error al obtener productos");
            return 1;
        }
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(productsResp));

        var first = productsResp.items?.FirstOrDefault();
        if (first == null)
        {
            Console.WriteLine("No hay productos disponibles para crear la orden");
            return 1;
        }

        Console.WriteLine("POST /api/orders");
        var orderReq = new
        {
            customerName = "Acme S.A.",
            items = new[] { new { productId = first.id, quantity = 1 } }
        };
        var postResp = await http.PostAsJsonAsync("/api/orders", orderReq);
        if (!postResp.IsSuccessStatusCode)
        {
            Console.WriteLine($"La creación falló: {postResp.StatusCode}");
            var txt = await postResp.Content.ReadAsStringAsync();
            Console.WriteLine(txt);
            return 1;
        }
        var created = await postResp.Content.ReadFromJsonAsync<OrderResponse>();
        Console.WriteLine("Orden creada:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(created));

        if (created is null)
        {
            Console.WriteLine("No se pudo obtener la orden");
            return 1;
        }
        var id = created.id;
        var getOrder = await http.GetFromJsonAsync<OrderResponse>($"/api/orders/{id}");
        Console.WriteLine("orden:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(getOrder));

        return 0;
    }

    record ApiResponse<T>(int total, int page, int pageSize, List<T>? items);
    record ProductListResponse(int id, string name, decimal price, int stock, DateTime createdAt);
    record OrderResponse(int id, string customerName, DateTime createdAt, List<OrderItemResponse> items);
    record OrderItemResponse(int id, int productId, int quantity, decimal unitPrice, int orderId);
}
