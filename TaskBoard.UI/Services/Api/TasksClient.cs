using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TaskBoard.UI.Models.Common;
using TaskBoard.UI.Models.Tasks;
using TaskBoard.UI.Services.Auth;
using System.Text.Json.Serialization; 
using System.Text.Json;

namespace TaskBoard.UI.Services.Api;

public class TasksClient
{

    private readonly HttpClient _httpClient;
    private readonly TokenStore _tokenStore;

    public TasksClient(HttpClient httpClient, TokenStore tokenStore)
    {

        _httpClient = httpClient;
        _tokenStore = tokenStore;

    }

    public async Task<PagedResult<TaskDto>?> GetAsync(GetTasksQueryModel query, CancellationToken cancellationToken = default)
    {

        var url = BuildTasksUrl(query);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AttachToken(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>(cancellationToken: cancellationToken);

    }

    public async Task<TaskDetailResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/tasks/{id}");
        AttachToken(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TaskDto>(cancellationToken: cancellationToken);

        return new TaskDetailResult
        {

            Task = dto,
            ETag = response.Headers.ETag?.Tag

        };

    }

    public async Task<TaskDto?> CreateAsync(CreateTaskModel model, CancellationToken cancellationToken = default)
    {

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/tasks")
        {
            Content = JsonContent.Create(model, options: ApiJsonOptions)
        };
        AttachToken(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<TaskDto>(cancellationToken: cancellationToken);

    }

    public async Task<PatchTaskResult> PatchAsync(Guid id, UpdateTaskModel model, string? eTag = null, CancellationToken cancellationToken = default)
    {

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/tasks/{id}")
        {
            Content = JsonContent.Create(model, options: ApiJsonOptions)
        };
        AttachToken(request);

        if (!string.IsNullOrWhiteSpace(eTag))
            request.Headers.TryAddWithoutValidation("If-Match", eTag);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return new PatchTaskResult { Success = true };

        if (response.StatusCode == HttpStatusCode.Conflict)
        {

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new PatchTaskResult
            {
                Success = false,
                Conflict = true,
                ErrorMessage = error
            };

        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new PatchTaskResult
            {
                Success = false,
                ErrorMessage = "Task nebyl nalezen."
            };

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new PatchTaskResult
        {
            Success = false,
            ErrorMessage = string.IsNullOrWhiteSpace(body)
                ? $"Chyba při PATCH: {(int)response.StatusCode} {response.ReasonPhrase}"
                : body
        };

    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/tasks/{id}");
        AttachToken(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        return response.IsSuccessStatusCode;

    }

    // Sets the Bearer token from the circuit-scoped TokenStore
    private void AttachToken(HttpRequestMessage request)
    {

        var token = _tokenStore.Token;

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    }

    private static string BuildTasksUrl(GetTasksQueryModel query)
    {

        var sb = new StringBuilder("api/tasks?");
        var hasAny = false;

        void Add(string key, string? value)
        {

            if (string.IsNullOrWhiteSpace(value)) return;

            if (hasAny) sb.Append('&');

            sb.Append(Uri.EscapeDataString(key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(value));
            hasAny = true;

        }

        Add(nameof(query.Priority), query.Priority);
        Add(nameof(query.Status), query.Status);
        Add(nameof(query.PageNumber), query.PageNumber.ToString());
        Add(nameof(query.PageSize), query.PageSize.ToString());

        foreach (var tag in query.Tags)
            Add(nameof(query.Tags), tag);

        return sb.ToString();

    }

    private static readonly JsonSerializerOptions ApiJsonOptions = new()
    {

        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }

    };

}
