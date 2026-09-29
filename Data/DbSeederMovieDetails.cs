using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Data;

/// <summary>
/// Movie &amp; TV show detail-page data. The detail page shows the template's
/// sidebar metadata (director, runtime, age rating, production country, cast) and
/// a Reviews block, so a fresh demo database is filled with those values plus a
/// fistful of written reviews. Only gaps are filled - nothing is overwritten, and
/// reviews are only written when a demo account exists to own them.
/// </summary>
public partial class DbSeeder
{
    // Deterministic, obviously-demo values so every install looks the same.
    private static readonly (string Director, int Runtime, string Age, string Country, string Cast)[] MovieFacts =
    {
        ("Rian Johnson", 145, "PG-13", "United States",
            "Ana Cruz:Lila Moreau, Ben Osei:Marcus Hale, Chloe Kim:Detective Ono, Diego Marsh:Captain Reyes"),
        ("Hana Suzuki", 132, "PG", "Japan",
            "Yuki Tanaka:Rei, Sora Ito:Kenji, Mina Sato:Aiko, Ryo Nakamura:Old Master"),
        ("Marcus Bell", 118, "PG-13", "United Kingdom",
            "Oliver Grant:Dr. Vale, Priya Nair:Inspector Rao, Sam Ford:Alex, Leah Dunn:June"),
        ("Sofia Marchetti", 96, "TV-14", "Italy",
            "Giulia Rossi:Bea, Tom Hollis:Nico, Elena Fabbri:Marta, Luca Conti:Father Paul"),
    };

    private static readonly (int Stars, string Text)[] DemoReviews =
    {
        (5, "A heartwarming story that hits all the right notes - the score alone is worth the watch."),
        (4, "While the movie had a slow start, it picked up beautifully in the second half. The director tied every loose end together and the climax was very satisfying."),
        (4, "Great cast chemistry and gorgeous cinematography. The middle act drags a little, but the ending lands."),
        (3, "Solid performances, though the script plays it safe. Still worth an evening."),
    };

    private async Task SeedMovieDetailsAsync()
    {
        // Only the streaming hubs' content types carry these columns.
        var movies = await _db.Contents
            .Where(c => c.Type == ContentType.Movie
                     || c.Type == ContentType.Series
                     || c.Type == ContentType.Documentary
                     || c.Type == ContentType.Special)
            .OrderBy(c => c.ContentId)
            .ToListAsync();

        for (var i = 0; i < movies.Count; i++)
        {
            var movie = movies[i];
            var facts = MovieFacts[i % MovieFacts.Length];

            if (string.IsNullOrWhiteSpace(movie.Director)) movie.Director = facts.Director;
            if (movie.RuntimeMinutes is null) movie.RuntimeMinutes = facts.Runtime;
            if (string.IsNullOrWhiteSpace(movie.AgeRating)) movie.AgeRating = facts.Age;
            if (string.IsNullOrWhiteSpace(movie.ProductionCountry)) movie.ProductionCountry = facts.Country;
            if (string.IsNullOrWhiteSpace(movie.Cast)) movie.Cast = facts.Cast;
        }

        if (movies.Count > 0)
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation("Movie detail metadata is present on {Count} titles.", movies.Count);
        }

        // ---- Reviews: owned by the opt-in demo accounts. -------------------
        // Without an account to attribute them to, the page shows its own
        // (working) empty state instead of fake rows.
        var authors = await _db.Users.OrderBy(u => u.Id).Take(2).ToListAsync();
        if (authors.Count == 0) return;

        if (await _db.Ratings.AnyAsync(r => r.Review != null && r.Review != "")) return;

        var targets = movies.Take(3).ToList();
        for (var i = 0; i < targets.Count; i++)
        {
            var (stars, text) = DemoReviews[i % DemoReviews.Length];
            _db.Ratings.Add(new Rating
            {
                UserId    = authors[i % authors.Count].Id,
                ContentId = targets[i].ContentId,
                Stars     = stars,
                Review    = text,
                CreatedAt = DateTime.UtcNow.AddDays(-(i + 1))
            });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo reviews for the streaming detail pages.", targets.Count);
    }
}
