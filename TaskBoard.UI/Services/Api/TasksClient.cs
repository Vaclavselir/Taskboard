using System.Net;
using System.Text;
using TaskBoard.UI.Models.Common;
using TaskBoard.UI.Models.Tasks;

namespace TaskBoard.UI.Services.Api;

public class TasksClient
{

    private readonly HttpClient _httpClient;

    public TasksClient(HttpClient httpClient)
    {

        _httpClient = httpClient;
        
    }

    public async Task<PagedResult<TaskDto>?> GetAsync(
        GetTasksQueryModel query,
        CancellationToken cancellationToken = default)
    {
        var url = BuildTasksUrl(query);

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>(
            cancellationToken: cancellationToken);
    }

    public async Task<TaskDetailResult?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/tasks/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TaskDto>(
            cancellationToken: cancellationToken);

        return new TaskDetailResult
        {
            Task = dto,
            ETag = response.Headers.ETag?.Tag
        };
    }

    public async Task<TaskDto?> CreateAsync(
        CreateTaskModel request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/tasks",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<TaskDto>(
            cancellationToken: cancellationToken);
    }

    public async Task<PatchTaskResult> PatchAsync(
        Guid id,
        UpdateTaskModel request,
        string? eTag = null,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"api/tasks/{id}")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(eTag))
        {
            httpRequest.Headers.TryAddWithoutValidation("If-Match", eTag);
        }

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new PatchTaskResult
            {
                Success = true
            };
        }

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
        {
            return new PatchTaskResult
            {
                Success = false,
                ErrorMessage = "Task nebyl nalezen."
            };
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return new PatchTaskResult
        {
            Success = false,
            ErrorMessage = string.IsNullOrWhiteSpace(body)
                ? $"Chyba při PATCH: {(int)response.StatusCode} {response.ReasonPhrase}"
                : body
        };
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/tasks/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static string BuildTasksUrl(GetTasksQueryModel query)
    {
        var sb = new StringBuilder("api/tasks?");
        var hasAny = false;

        void Add(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (hasAny)
                sb.Append('&');

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
        {
            Add(nameof(query.Tags), tag);
        }

        return sb.ToString();
    }

}
