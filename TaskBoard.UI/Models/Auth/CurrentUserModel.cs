using System;

namespace TaskBoard.UI.Models;

public class CurrentUserModel
{

    public bool IsAuthenticated { get; set; }

    public string? Id { get; set; }

    public string? Email { get; set; }

    public bool IsAdmin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = [];

}
