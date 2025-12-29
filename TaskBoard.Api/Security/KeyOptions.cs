using System;

namespace TaskBoard.Api.Security;

public sealed class KeyOptions
{

    public string ApiKey {get; init; } = "";

    public const string SectionName = "Security";

}
