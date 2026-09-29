namespace FanHubPlus.Models.ViewModels;

/// <summary>
/// The title card every admin page wears - the same one the dashboard opens
/// with. Keeping it in one view model means the nine list pages and the six
/// forms all say the same thing the same way instead of re-rolling a heading.
/// </summary>
public class AdminHeroViewModel
{
    public string Eyebrow { get; set; } = "FanHub Plus · Control Room";
    public string Title { get; set; } = string.Empty;

    /// <summary>Rendered after the title in the champagne/gold gradient.
    /// Nullable: the form pages pass <c>null</c> for "no accent word" when
    /// creating a record, which the hero treats as "omit it".</summary>
    public string? Accent { get; set; }

    public string Lede { get; set; } = string.Empty;

    public List<AdminHeroChip> Chips { get; set; } = new();

    /// <summary>The page's own action ("New content"), rendered top right.</summary>
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionIcon { get; set; }

    /// <summary>Optional escape hatch back to the list a form was opened from.</summary>
    public string? BackText { get; set; }
    public string? BackUrl { get; set; }
    public string? BackIcon { get; set; } = "ri-arrow-left-line";
}

/// <summary>One pill in the hero: a value in champagne plus its label.</summary>
public class AdminHeroChip
{
    public string Icon { get; set; } = "ri-information-line";
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    /// <summary>Pulses the dot in red - used for "waiting on you" counters.</summary>
    public bool Live { get; set; }

    public AdminHeroChip() { }

    public AdminHeroChip(string icon, string value, string label, bool live = false)
    {
        Icon = icon;
        Value = value;
        Label = label;
        Live = live;
    }
}
