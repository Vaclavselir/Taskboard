using System.Text.RegularExpressions;

namespace TaskBoard.Domain;

public sealed record Tag
{

    private static readonly Regex _regex = new(@"^[a-z0-9_-]{1,20}$", RegexOptions.Compiled);
    
    public string Value { get; }

    public Tag(string value)
    {
        value = (value ?? string.Empty).Trim();

        if (!_regex.IsMatch(value))
            throw new ArgumentException("Invalid tag. Allowed: a-z, 0-9, '_' and '-', length 1-20.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;

}
