using System;

namespace TaskBoard.Infrastructure.Persistence;
using System.Text.Json.Serialization;
using TaskBoard.Domain;

public sealed class TaskRecord
{

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("priority")]
    public Priority Priority { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();


}
