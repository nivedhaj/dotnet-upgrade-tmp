using System.Text;
using System.Text.RegularExpressions;
using Ct.Core.Domain;

namespace Ct.Modules.Admissions.Domain;

/// <summary>
/// Legacy `Person` (spec section 7). Backs both standalone parent/guardian
/// records and, via <see cref="Student"/>, enrolled students.
/// </summary>
public partial class Person : EntityBase
{
    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = string.Empty;

    public string? Title { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? VirtualId { get; set; }
    public string? Pan { get; set; }

    /// <summary>Free-form dynamic attributes. JSONB — genuinely dynamic data only (spec section 113).</summary>
    public Dictionary<string, object?> Attributes { get; set; } = new();

    public List<PersonContact> Contacts { get; private set; } = new();
    public List<PersonAddress> Addresses { get; private set; } = new();
    public List<PersonTag> Tags { get; private set; } = new();
    public List<PersonOccupation> Occupations { get; private set; } = new();

    /// <summary>
    /// Legacy `Person.Name` getter: FirstName + MiddleName + LastName, whitespace-normalized.
    /// Kept as a computed projection rather than a stored column (spec section 7).
    /// </summary>
    public string Name => NormalizeWhitespace(
        string.Join(' ', new[] { FirstName, MiddleName, LastName }.Where(p => !string.IsNullOrWhiteSpace(p))));

    /// <summary>
    /// Legacy `Person.Name` setter behavior, preserved exactly (spec section 7):
    /// 1. Trim input.
    /// 2. Replace non-alphabetic characters except apostrophe with spaces.
    /// 3. Collapse multiple spaces.
    /// 4. Split into words.
    /// 5. First word -> FirstName, last word -> LastName, middle words -> MiddleName.
    /// 6. Title-case every component.
    /// </summary>
    public void SetName(string? rawName)
    {
        var trimmed = (rawName ?? string.Empty).Trim();
        var cleaned = NonAlphaExceptApostrophe().Replace(trimmed, " ");
        var normalized = NormalizeWhitespace(cleaned);

        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            FirstName = string.Empty;
            MiddleName = null;
            LastName = string.Empty;
            return;
        }

        var titleCased = words.Select(ToTitleCase).ToArray();

        FirstName = titleCased[0];
        LastName = titleCased[^1];
        MiddleName = titleCased.Length > 2
            ? string.Join(' ', titleCased[1..^1])
            : null;

        // A single-word name has no distinct last name in the legacy model;
        // FirstName == LastName in that case is intentional (word[0] == word[^1]).
    }

    /// <summary>
    /// Legacy derivation used only when no explicit virtual id was ever set
    /// (spec section 7): firstname.lastname.<first 4 chars of id>@campustrack.net
    /// </summary>
    public string DeriveVirtualId()
    {
        var idFragment = Id.ToString("N")[..4];
        var first = FirstName.ToLowerInvariant();
        var last = LastName.ToLowerInvariant();
        return $"{first}.{last}.{idFragment}@campustrack.net";
    }

    public void EnsureVirtualId()
    {
        if (string.IsNullOrWhiteSpace(VirtualId))
        {
            VirtualId = DeriveVirtualId();
        }
    }

    private static string NormalizeWhitespace(string value) =>
        WhitespaceRun().Replace(value, " ").Trim();

    private static string ToTitleCase(string word)
    {
        if (word.Length == 0) return word;
        var sb = new StringBuilder(word.Length);
        var capitalizeNext = true;
        foreach (var c in word)
        {
            if (c == '\'')
            {
                sb.Append(c);
                capitalizeNext = true; // O'Brien-style capitalization after apostrophe
                continue;
            }

            sb.Append(capitalizeNext ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
            capitalizeNext = false;
        }
        return sb.ToString();
    }

    [GeneratedRegex(@"[^a-zA-Z']+")]
    private static partial Regex NonAlphaExceptApostrophe();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
