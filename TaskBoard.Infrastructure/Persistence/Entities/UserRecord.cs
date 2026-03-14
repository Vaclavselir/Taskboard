namespace TaskBoard.Infrastructure.Persistence.Entities;
using System.Text.Json.Serialization;
public record class UserRecord
{

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("isAdmin")]
    public bool IsAdmin { get; set; }

}
