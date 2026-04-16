using OrderFlow.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

public class OrderRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task SaveToJsonAsync(IEnumerable<Order> orders, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using FileStream fs = new(path, FileMode.Create);
        await JsonSerializer.SerializeAsync(fs, orders, _jsonOptions);
    }

    public async Task<List<Order>> LoadFromJsonAsync(string path)
    {
        if (!File.Exists(path))
            return new List<Order>();

        await using FileStream fs = new(path, FileMode.Open);
        var data = await JsonSerializer.DeserializeAsync<List<Order>>(fs, _jsonOptions);

        return data ?? new List<Order>();
    }

    public async Task SaveToXmlAsync(IEnumerable<Order> orders, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var serializer = new XmlSerializer(typeof(List<Order>));

        await using FileStream fs = new(path, FileMode.Create);
        serializer.Serialize(fs, orders.ToList());
    }

    public async Task<List<Order>> LoadFromXmlAsync(string path)
    {
        if (!File.Exists(path))
            return new List<Order>();

        var serializer = new XmlSerializer(typeof(List<Order>));

        await using FileStream fs = new(path, FileMode.Open);
        var data = serializer.Deserialize(fs) as List<Order>;

        return data ?? new List<Order>();
    }
}