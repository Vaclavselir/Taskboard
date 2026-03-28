using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskBoard.UI.Models.Admin;

namespace TaskBoard.UI.Services.Api;

public sealed class AdminClient
{

    private readonly HttpClient _http;
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions ApiJsonOptions = new()
    {

        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }

    };

    public AdminClient(HttpClient http, IConfiguration config)
    {

        _http = http;

        _apiKey = config["Security:ApiKey"] ?? throw new InvalidOperationException("Security:ApiKey není nakonfigurován v UI.");

    }

    public async Task<List<AdminTaskDto>> GetAllTasksAsync(CancellationToken ct = default)
    {

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/admin/tasks");
        request.Headers.Add("X-API-KEY", _apiKey);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<AdminTaskDto>>(ApiJsonOptions, ct) ?? [];

    }

}
