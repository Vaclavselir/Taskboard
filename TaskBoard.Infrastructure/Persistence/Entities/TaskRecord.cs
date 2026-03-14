using System;

namespace TaskBoard.Infrastructure.Persistence;

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TaskBoard.Domain;


public sealed class TaskRecord
{

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

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
