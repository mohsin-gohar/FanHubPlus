using FanHubPlus.Models;
using System.Collections.Generic;
using System.Linq;


namespace FanHubPlus.Data
{
    public static class SeedData
    {
        public static void Initialize(FanHubDbContext db)
        {
            if (db.Categories.Any()) return;

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action",       Icon = "💥", VideoCount = 12400 },
                new Category { Id = 2, Name = "Drama",        Icon = "🎭", VideoCount = 7300 },
                new Category { Id = 3, Name = "Comedy",       Icon = "😂", VideoCount = 345 },
                new Category { Id = 4, Name = "Documentary",  Icon = "🌍", VideoCount = 678 },
                new Category { Id = 5, Name = "Anime",        Icon = "🗾", VideoCount = 346 },
                new Category { Id = 6, Name = "Horror",       Icon = "👻", VideoCount = 568 },
                new Category { Id = 7, Name = "Romance",      Icon = "❤️", VideoCount = 987 },
                new Category { Id = 8, Name = "Sports",       Icon = "🏆", VideoCount = 1520 }
            );

            var v = new List<VideoItem>
            {
                // Featured hero slides
                new VideoItem { Id = 1,  Title = "The Next Saga Unfolds", Genre = "Horror",  Year = 2025, Duration = "1h 55m", ImdbRating = 8.4, IsFeatured = true, PosterTheme = "t2", Description = "A chilling tale from the deep woods where five friends discover that some doors should never open. FanHub Original." },
                new VideoItem { Id = 2,  Title = "Crimson Horizon",       Genre = "Action",  Year = 2026, Duration = "1h 42m", ImdbRating = 8.9, IsFeatured = true, PosterTheme = "t1", Description = "An elite pilot must race against time to stop a global conspiracy in this high-octane FanHub Original blockbuster." },
                new VideoItem { Id = 3,  Title = "Neon Alley",            Genre = "Drama",   Year = 2025, Duration = "2h 05m", ImdbRating = 8.1, IsFeatured = true, PosterTheme = "t4", Description = "A street musician chasing a dream she is told she can no longer afford, set against a city that never sleeps." },

                // Trending
                new VideoItem { Id = 4,  Title = "Crimson Horizon",  Genre = "Adventure", Year = 2025, ImdbRating = 7.9, IsTrending = true, PosterTheme = "t1", Duration = "2h 10m", ReleaseDate = new DateTime(2025, 3, 12), Description = "Expedition teams race to claim a lost valley before a rival corporation buries its secret forever." },
                new VideoItem { Id = 5,  Title = "The Glass Empire", Genre = "Political", Year = 2025, ImdbRating = 8.2, IsTrending = true, PosterTheme = "t3", Duration = "1h 58m", ReleaseDate = new DateTime(2025, 3, 15), Description = "A political thriller about the family behind the country's largest media empire - and the truth that could break it." },
                new VideoItem { Id = 6,  Title = "Beyond The Neon",  Genre = "Action",    Year = 2025, ImdbRating = 8.6, IsTrending = true, PosterTheme = "t5", Duration = "2h 22m", ReleaseDate = new DateTime(2025, 3, 16), Description = "In a rain-soaked megacity, a retired detective takes one last case that leads her into the neon underworld." },
                new VideoItem { Id = 7,  Title = "Parallel Line",    Genre = "Action",    Year = 2025, ImdbRating = 7.7, IsTrending = true, PosterTheme = "t6", Duration = "1h 47m", ReleaseDate = new DateTime(2025, 3, 19), Description = "Two agents from parallel timelines are forced to work together when both of their worlds start to collapse." },
                new VideoItem { Id = 8,  Title = "The Silent Code",  Genre = "Drama",     Year = 2025, ImdbRating = 8.0, IsTrending = true, PosterTheme = "t7", Duration = "2h 01m", ReleaseDate = new DateTime(2025, 3, 22), Description = "A cryptographer uncovers a message hidden inside the world's most popular game - and becomes its target." },
                new VideoItem { Id = 9,  Title = "Dream Signal",     Genre = "Horror",    Year = 2025, ImdbRating = 7.5, IsTrending = true, PosterTheme = "t2", Duration = "1h 39m", ReleaseDate = new DateTime(2025, 3, 25), Description = "A sleep study goes wrong when volunteers begin receiving the same impossible broadcast in their dreams." },
                // Live now
                new VideoItem { Id = 10, Title = "The K-Pop Live",     Genre = "Music",         Year = 2026, ImdbRating = 0, IsLive = true, WatchingNow = 750,    PosterTheme = "t8", Duration = "LIVE",  Description = "Live concert broadcast - front row seats, anywhere in the world." },
                new VideoItem { Id = 11, Title = "Spotlight Live",     Genre = "Entertainment", Year = 2026, ImdbRating = 0, IsLive = true, WatchingNow = 20000, PosterTheme = "t5", Duration = "LIVE",  Description = "Red carpet live coverage with backstage interviews with FanHub Originals stars." },
                new VideoItem { Id = 12, Title = "Breaking Stage",     Genre = "Sports",        Year = 2026, ImdbRating = 0, IsLive = true, WatchingNow = 11000, PosterTheme = "t1", Duration = "LIVE",  Description = "Breakingdance world championship - live from the arena." },
                new VideoItem { Id = 13, Title = "The Streamline Cup", Genre = "Sports",        Year = 2026, ImdbRating = 0, IsLive = true, WatchingNow = 750,    PosterTheme = "t3", Duration = "LIVE",  Description = "Esports grand finals - every match, every angle, live in 4K." },
                new VideoItem { Id = 14, Title = "Midnight Frequency", Genre = "Music",         Year = 2026, ImdbRating = 0, IsLive = true, WatchingNow = 1921,   PosterTheme = "t4", Duration = "ENDED", Description = "Late-night radio visual show - replay available now." },
                new VideoItem { Id = 15, Title = "Sidewalk The Hero",  Genre = "Sports",        Year = 2026, ImdbRating = 0, IsLive = true, WatchingNow = 10000, PosterTheme = "t6", Duration = "LIVE",  Description = "Urban marathon through the city skyline - live multi-camera feed." },

                // Top rated
                new VideoItem { Id = 16, Title = "The Mountain",     Genre = "Adventure", Year = 2025, ImdbRating = 8.9, IsTopRated = true, PosterTheme = "t3", Duration = "2h 25m", Description = "A breathtaking climb, a brutal storm, and a choice no climber should have to make." },
                new VideoItem { Id = 17, Title = "After The Fall",   Genre = "Action",    Year = 2024, ImdbRating = 8.8, IsTopRated = true, PosterTheme = "t1", Duration = "2h 11m", Description = "The city rebuilt itself after the collapse. The people who caused it did not disappear." },
                new VideoItem { Id = 18, Title = "Tuck Everlasting", Genre = "Horror",    Year = 2025, ImdbRating = 8.2, IsTopRated = true, PosterTheme = "t2", Duration = "2h 32m", Description = "A small town where nobody dies - and the stranger who learns why that is a curse, not a gift." },
                new VideoItem { Id = 19, Title = "MMA Victory",      Genre = "Sports",    Year = 2023, ImdbRating = 8.5, IsTopRated = true, PosterTheme = "t6", Duration = "2h 12m", Description = "Documentary series following a champion fighter through the hardest comeback of her career." },
                new VideoItem { Id = 20, Title = "Midnight Diner",   Genre = "Comedy",    Year = 2025, ImdbRating = 8.8, IsTopRated = true, PosterTheme = "t7", Duration = "1h 55m", Description = "The cook listens, the regulars talk, and every episode ends with a meal you will wish you could order." },

                // Latest releases
                new VideoItem { Id = 21, Title = "Beyond The Neon",    Genre = "Noir",        Year = 2025, ImdbRating = 8.3, PosterTheme = "t5", Duration = "2h 03m", ReleaseDate = new DateTime(2025, 8, 16),  Description = "A neon-drenched noir about the last honest cop in a dishonest precinct." },
                new VideoItem { Id = 22, Title = "Unexpected Journey", Genre = "Adventure",   Year = 2025, ImdbRating = 7.8, PosterTheme = "t3", Duration = "1h 51m", ReleaseDate = new DateTime(2025, 11, 12), Description = "A cross-country road trip that was supposed to take two days and changes four lives." },
                new VideoItem { Id = 23, Title = "Miles In The Dark",  Genre = "Horror",      Year = 2025, ImdbRating = 7.4, PosterTheme = "t2", Duration = "1h 44m", ReleaseDate = new DateTime(2025, 6, 22),  Description = "A lighthouse keeper, a fog that never lifts, and footsteps on the gallery every night at three." },
                new VideoItem { Id = 24, Title = "Touch Story",        Genre = "Action",      Year = 2025, ImdbRating = 8.0, PosterTheme = "t1", Duration = "2h 18m", ReleaseDate = new DateTime(2025, 12, 21), Description = "A courier with perfect memory carries the only copy of a secret worth killing for." },
                new VideoItem { Id = 25, Title = "King Jungle",        Genre = "Documentary", Year = 2025, ImdbRating = 9.0, PosterTheme = "t4", Duration = "1h 29m", ReleaseDate = new DateTime(2025, 1, 11),  Description = "Three years inside the canopy, following a leopard family raising cubs in a changing forest." }
            };
            db.Videos.AddRange(v);
            db.BlogPosts.AddRange(
                new BlogPost { Id = 1, Title = "Top 10 FanHub Originals You Can't Miss This Month", Author = "Admin", PublishedOn = new DateTime(2025, 11, 14), PosterTheme = "t1",
                    Excerpt = "From neon-soaked noir to the documentary everyone is talking about - here is what to watch this month on FanHub Plus.",
                    Content = "Every month FanHub Plus adds fresh originals to the library, and this month is one of the strongest line-ups of the year. We counted down ten titles worth your weekend, starting with Crimson Horizon, the high-octane aviation thriller that has held the number one spot since release. If you prefer something slower, Midnight Diner is the comfort watch of the season: one episode per night is the only rule we recommend. Rounding out the list are the finale of The Glass Empire, the horror sleeper hit Dream Signal, and King Jungle, the documentary that critics are calling the best nature film in a decade. Whatever you pick, the whole list is available today in HD and 4K on every device." },
                new BlogPost { Id = 2, Title = "Behind The Scenes: Creating FanHub Originals", Author = "Admin", PublishedOn = new DateTime(2025, 11, 13), PosterTheme = "t4",
                    Excerpt = "We visited the sets of two of our biggest originals to see how the magic actually gets made.",
                    Content = "What does it take to build a FanHub Original from scratch? We spent a week on the sets of The Next Saga Unfolds and Crimson Horizon to find out. From practical effects teams working through the night to virtual-production stages that render entire cities around the actors, the scale surprised even our most seasoned crew members. In this piece we talk with directors, camera operators and post-production artists about how a script becomes the show you press play on - including the one scene in Crimson Horizon that took four hundred people and eleven days to shoot." },
                new BlogPost { Id = 3, Title = "Tips & Tricks To Maximize Your FanHub Viewing Experience", Author = "Admin", PublishedOn = new DateTime(2025, 11, 12), PosterTheme = "t7",
                    Excerpt = "Profiles, downloads, watch parties, picture settings - power-user tips most viewers never find.",
                    Content = "FanHub Plus has a lot of depth hiding under a simple interface. This guide walks through the features people miss first: creating kid-safe profiles with their own recommendations, queueing downloads over night so your data plan stays untouched, starting a watch party so you and friends get a synced play button no matter where you are, and switching to reference picture mode on supported TVs so a film looks the way the colorist graded it. Each tip takes under two minutes to set up." },
                new BlogPost { Id = 4, Title = "How Offline Downloads Changed The Way We Watch", Author = "Admin", PublishedOn = new DateTime(2025, 11, 8), PosterTheme = "t5",
                    Excerpt = "Flights, commutes, dead zones - one-tap downloads turned waiting time into watching time.",
                    Content = "When we launched one-tap downloads, we expected them to be a travel feature. What we found is that our viewers download everywhere: subway commutes, long drives, camping trips and even power cuts. This post looks at the numbers behind that behaviour, explains how smart storage optimization keeps your library fresh without filling your device, and previews the upcoming feature that lets you pre-download the next episode of anything you are watching before you even ask for it." }
            );

            db.Testimonials.AddRange(
                new Testimonial { Id = 1, Name = "Alex Morgan",   Role = "Filmmaker",        ReviewCount = 199, Quote = "I've used many streaming platforms, but FanHub stands out for how immersive it feels. The visual quality is crisp from the very first frame." },
                new Testimonial { Id = 2, Name = "Emily Carter",  Role = "Music Enthusiast", ReviewCount = 342, Quote = "Watching live concerts and events on FanHub has become my Friday ritual. It genuinely feels like being in the front row." },
                new Testimonial { Id = 3, Name = "Olivia Parker", Role = "Director",         ReviewCount = 212, Quote = "Easy to use and always spot on. It's a great way to keep up with the latest releases without juggling five different apps." }
            );

            db.SaveChanges();
        }
    }
}
