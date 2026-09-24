using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Portfolio.Application.Contracts;

public static partial class Slug
{
    /// <summary>"Ciência da Computação" → "ciencia-da-computacao"</summary>
    public static string From(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        return NonAlphanumeric().Replace(sb.ToString(), "-").Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
