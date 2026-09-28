using System.Globalization;
using System.Text;
using FanHubPlus.Models.Entities;

namespace FanHubPlus.Services;

/// <summary>
/// URL-friendly slugs for articles: "Attack on Titan finale!" -> "attack-on-titan-finale".
/// Used when saving an article (admin), when generating links (views) and when
/// resolving /Blog/Details/{slug} (ArticleService).
/// </summary>
public static class SlugHelper
{
    public static string Slugify(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;

        // Decompose accents so "Pokémon" -> "pokemon" instead of "pok-mon"
        var normalized = title.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        var lastWasDash = false;

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
                lastWasDash = false;
            }
            else if (!lastWasDash && sb.Length > 0)
            {
                sb.Append('-');
                lastWasDash = true;
            }
        }

        var result = sb.ToString().Trim('-');
        return result.Length > 200 ? result[..200].TrimEnd('-') : result;
    }

    /// <summary>Preferred link slug: stored value first, computed from the title as fallback (legacy/seeded rows).</summary>
    public static string Resolve(Article article)
        => !string.IsNullOrWhiteSpace(article.Slug) ? article.Slug : Slugify(article.Title);
}
