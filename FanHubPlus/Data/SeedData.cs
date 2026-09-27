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

            // Fill in the detail-page metadata for every title without hand-writing it 25 times.
            var directors = new[] { "A. Rao", "L. Ferreira", "S. Nakamura", "M. Okafor", "R. Halvorsen" };
            var castPool = new[]
            {
                "I. Malhotra, D. Costa, R. Menon, P. Varghese",
                "T. Adeyemi, L. Bergström, K. Saito, N. Dsouza",
                "H. Lindqvist, M. Dsouza, A. Khan, C. Oyelaran",
                "Y. Tanaka, S. Ferreira, D. Malhotra, V. Iyer"
            };
            var taglines = new[]
            {
                "Some doors should never open.",
                "The sky is not the limit, it is the countdown.",
                "A city that never sleeps has one singer left to wake up.",
                "Every signal leaves a trace."
            };
            for (var i = 0; i < v.Count; i++)
            {
                var item = v[i];
                item.Director = directors[i % directors.Length];
                item.Cast = castPool[i % castPool.Length];
                item.Tagline = taglines[i % taglines.Length];
                item.IsSeries = item.Id == 3 || item.Id == 18 || item.Id == 20;
                item.TotalSeasons = item.Id == 20 ? 2 : item.IsSeries ? 1 : 0;
                item.TotalEpisodes = item.IsSeries ? 4 : 1;
                item.Quality = item.IsLive ? "1080p Live" : "4K UHD";
                item.AgeRating = item.Genre is "Horror" or "Action" ? "18+" : "16+";
                item.Languages = item.Id % 3 == 0 ? "English, Hindi, Tamil" : "English, Hindi";
                item.AudioTracks = item.Id % 2 == 0 ? "English, Hindi, Tamil, Telugu" : "English, Hindi";
                item.Subtitles = "English, Hindi, Arabic, French, Spanish";
            }

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

            db.Channels.AddRange(
                new Channel { Id = 1, Name = "FanHub Sports 1", Category = "Sports", Number = "101", Language = "English", Viewers = 48200, LogoTheme = "t1", NowPlaying = "Prime League: Matchweek 22", Description = "Live football, pre and post match analysis, and highlights within minutes of the final whistle." },
                new Channel { Id = 2, Name = "FanHub Cricket", Category = "Sports", Number = "102", Language = "English, Hindi", Viewers = 93500, LogoTheme = "t4", NowPlaying = "T20 Series: India vs Australia", Description = "Every match, every over, in ultra low latency with optional scorecard overlay." },
                new Channel { Id = 3, Name = "FanHub News 24", Category = "News", Number = "201", Language = "Hindi, English", Viewers = 12400, LogoTheme = "t5", NowPlaying = "The Evening Report", Description = "Rolling national and world news with a live ticker and regional feeds." },
                new Channel { Id = 4, Name = "FanHub Cinema One", Category = "Movies", Number = "301", Language = "English", Viewers = 8600, LogoTheme = "t2", NowPlaying = "Crimson Horizon", Description = "Award season premieres and restorations, uncut and commercial free." },
                new Channel { Id = 5, Name = "FanHub Kids", Category = "Kids", Number = "401", Language = "English, Hindi", Viewers = 5100, LogoTheme = "t3", NowPlaying = "The Paper Boat Club", Description = "Animation, documentaries and sing-along specials approved by teachers and parents." },
                new Channel { Id = 6, Name = "FanHub Music", Category = "Entertainment", Number = "501", Language = "English", Viewers = 21900, LogoTheme = "t6", NowPlaying = "Late Night Frequency", Description = "Concerts, live sets and music documentaries, plus a 24 hour music video channel." },
                new Channel { Id = 7, Name = "FanHub Anime", Category = "Entertainment", Number = "502", Language = "Japanese, English", Viewers = 17300, LogoTheme = "t8", NowPlaying = "Ronin Kitchen", Description = "Simulcast and dubbed anime seasons with next day subtitles in eight languages." },
                new Channel { Id = 8, Name = "FanHub Docu", Category = "Movies", Number = "302", Language = "English", Viewers = 3200, LogoTheme = "t7", NowPlaying = "King Jungle - Episode 3", Description = "Nature, science and history documentaries mastered in 4K HDR." }
            );

            db.Episodes.AddRange(
                new Episode { Id = 1, VideoId = 3, Season = 1, Number = 1, Title = "The Alley", Description = "Mira takes the last bus home and finds the whole street waiting for news she wanted to miss.", Duration = "48m", ImdbRating = 8.1, StillTheme = "t4", AiredOn = new DateTime(2025, 3, 7) },
                new Episode { Id = 2, VideoId = 3, Season = 1, Number = 2, Title = "Soundcheck", Description = "A stolen microphone leads Mira back to the venue where she first played.", Duration = "44m", ImdbRating = 8.4, StillTheme = "t2", AiredOn = new DateTime(2025, 3, 14) },
                new Episode { Id = 3, VideoId = 3, Season = 1, Number = 3, Title = "Static", Description = "The radio tower goes dark and the neighbourhood starts talking about why.", Duration = "51m", ImdbRating = 8.6, StillTheme = "t6", AiredOn = new DateTime(2025, 3, 21) },
                new Episode { Id = 4, VideoId = 3, Season = 1, Number = 4, Title = "Encore", Description = "The season finale puts every character on the same stage for one night.", Duration = "56m", ImdbRating = 9.0, StillTheme = "t5", AiredOn = new DateTime(2025, 3, 28) },
                new Episode { Id = 5, VideoId = 18, Season = 1, Number = 1, Title = "The Invitation", Description = "A stranger arrives with a card, a key and an offer nobody should accept.", Duration = "52m", ImdbRating = 8.4, StillTheme = "t2", IsFree = true, AiredOn = new DateTime(2025, 2, 4) },
                new Episode { Id = 6, VideoId = 18, Season = 1, Number = 2, Title = "The Long Sleep", Description = "Nobody in town has aged a day. Somebody is keeping score.", Duration = "49m", ImdbRating = 8.2, StillTheme = "t7", AiredOn = new DateTime(2025, 2, 11) },
                new Episode { Id = 7, VideoId = 20, Season = 2, Number = 1, Title = "Last Order", Description = "A new chef takes over the counter and immediately changes every regular's order.", Duration = "24m", ImdbRating = 8.8, StillTheme = "t3", IsFree = true, AiredOn = new DateTime(2025, 1, 18) },
                new Episode { Id = 8, VideoId = 20, Season = 2, Number = 2, Title = "Midnight Set", Description = "A regular orders something that is not on the menu, and explains why.", Duration = "23m", ImdbRating = 8.6, StillTheme = "t8", AiredOn = new DateTime(2025, 1, 25) }
            );

            db.Comments.AddRange(
                new Comment { Id = 1, VideoId = 1, Author = "Rohan Mehta", Body = "The sound design in the first ten minutes is unreal. Headphones strongly recommended.", Likes = 42, PostedOn = new DateTime(2025, 11, 18, 19, 32, 0) },
                new Comment { Id = 2, VideoId = 1, Author = "Sara Iqbal", Body = "Watched it twice. The ending reframes the whole middle section, brilliant writing.", Likes = 27, PostedOn = new DateTime(2025, 11, 17, 21, 5, 0) },
                new Comment { Id = 3, VideoId = 2, Author = "Daniel Fernandes", Body = "Ignore the trailers, the aerial sequences alone are worth the subscription.", Likes = 18, PostedOn = new DateTime(2025, 11, 16, 10, 12, 0) },
                new Comment { Id = 4, VideoId = 3, Author = "Priya Nair", ParentId = 2, Body = "Completely agree - the encore episode is one of the best things on TV this year.", Likes = 9, PostedOn = new DateTime(2025, 11, 17, 22, 40, 0) }
            );

            db.Products.AddRange(
                new StoreProduct { Id = 1, Name = "FanHub Plus Annual Pass", Category = "Bundles", Price = 119.99m, OldPrice = 149.99m, Rating = 4.9, Reviews = 842, Sold = 3120, Badge = "-20%", ArtTheme = "t1", ArtGlyph = "ri-ticket-2-line", Sku = "FHP-ANN-01", Description = "Twelve months of Premium 4K UHD, early access to premieres and one cinema ticket credit every quarter." },
                new StoreProduct { Id = 2, Name = "Neon Alley Limited Tee", Category = "Merch", Price = 29.00m, OldPrice = 39.00m, Rating = 4.7, Reviews = 316, Sold = 1890, Badge = "Best Seller", ArtTheme = "t6", ArtGlyph = "ri-shirt-line", Sku = "FHP-TEE-07", Description = "Heavyweight 220gsm cotton tee with a screen printed poster motif from the FanHub original Neon Alley." },
                new StoreProduct { Id = 3, Name = "Director's Edition Blu-Ray Box", Category = "Merch", Price = 74.50m, Rating = 4.9, Reviews = 128, Sold = 460, Badge = "New", ArtTheme = "t2", ArtGlyph = "ri-disc-line", Sku = "FHP-BOX-02", Description = "Collector's edition four disc set with a 120 page book, poster and two unused alternate endings." },
                new StoreProduct { Id = 4, Name = "Crimson Horizon OST Vinyl", Category = "Audio", Price = 34.00m, OldPrice = 42.00m, Rating = 4.6, Reviews = 204, Sold = 940, Badge = "-19%", ArtTheme = "t4", ArtGlyph = "ri-disc-2-line", Sku = "FHP-VIN-11", Description = "Double gatefold vinyl pressing of the original score recorded with a full 80 piece orchestra." },
                new StoreProduct { Id = 5, Name = "FanHub Streaming Device", Category = "Devices", Price = 59.00m, OldPrice = 79.00m, Rating = 4.5, Reviews = 671, Sold = 5210, Badge = "-25%", ArtTheme = "t5", ArtGlyph = "ri-tv-line", Sku = "FHP-BOX-04", Description = "4K streaming dongle with HDMI 2.1, voice remote and one year of Premium access included." },
                new StoreProduct { Id = 6, Name = "Kids Profile Pack - 6 Months", Category = "Bundles", Price = 0.00m, Rating = 4.8, Reviews = 511, Sold = 2740, Badge = "Free", ArtTheme = "t3", ArtGlyph = "ri-shield-check-line", Sku = "FHP-KID-06", Description = "Adds a fully filtered kids profile with age appropriate artwork and parental PIN controls." },
                new StoreProduct { Id = 7, Name = "Studio Headphones ANC", Category = "Devices", Price = 189.00m, OldPrice = 229.00m, Rating = 4.3, Reviews = 289, Sold = 720, Badge = "-17%", ArtTheme = "t7", ArtGlyph = "ri-headphone-line", Sku = "FHP-AUD-09", Description = "Closed back studio tuned headphones with active noise cancelling and 60 hour battery." },
                new StoreProduct { Id = 8, Name = "FanHub Originals Poster Set", Category = "Merch", Price = 24.00m, Rating = 4.6, Reviews = 143, Sold = 1120, Badge = "New", ArtTheme = "t8", ArtGlyph = "ri-image-2-line", Sku = "FHP-POS-03", Description = "A2 matte posters for four FanHub originals, printed on 200gsm uncoated stock." }
            );
            db.Jobs.AddRange(
                new JobOpening { Id = 1, Title = "Senior Backend Engineer (Streaming)", Department = "Engineering", Location = "Mumbai (Hybrid)", Type = "Full Time", Experience = "4-6 years", Salary = "INR 32-45 LPA", IsUrgent = true, PostedOn = new DateTime(2025, 11, 3),
                    Summary = "Own the playback, catalogue and subscription services behind millions of weekly streams.",
                    Responsibilities = "Design and ship services that stay up during premiere-week traffic spikes\nPartner with the player team to keep time-to-first-frame under two seconds\nMentor two mid-level engineers through design review",
                    Requirements = "Five or more years with C# / .NET or a comparable stack\nHands on with Redis, PostgreSQL and message queues\nExperience running production services on Kubernetes",
                    Benefits = "Hybrid work, twice a week in studio\nAnnual learning budget\nFamily premium plan on us" },
                new JobOpening { Id = 2, Title = "Content Curator - Originals", Department = "Content", Location = "Mumbai (On-site)", Type = "Full Time", Experience = "2-4 years", Salary = "INR 14-20 LPA", PostedOn = new DateTime(2025, 10, 22),
                    Summary = "Find the next ten originals worth a greenlight and keep the library coherent across markets.",
                    Responsibilities = "Build and defend the quarterly originals slate\nWork with the creative team on pitches and festival strategy\nTrack performance and retire what is not working",
                    Requirements = "Three or more years in film, TV or editorial curation\nSharp written English and confident presentation skills\nComfortable reading scripts and contracts",
                    Benefits = "Festival travel credits\nInternal screening access" },
                new JobOpening { Id = 3, Title = "Product Designer - Player Experience", Department = "Design", Location = "Remote (India)", Type = "Full Time", Experience = "3-5 years", Salary = "INR 22-30 LPA", PostedOn = new DateTime(2025, 10, 9),
                    Summary = "Simplify the player, the queue and the download experience across ten thousand screens a day.",
                    Responsibilities = "Own end to end design for playback surfaces on TV, mobile and web\nRun research sessions with real viewers every sprint\nKeep the design system consistent as the product grows",
                    Requirements = "A portfolio of shipped product work\nComfortable in Figma with real components, not static frames\nExperience designing for 10 foot interfaces",
                    Benefits = "Remote first, quarterly offsites\nHardware budget for testing on real devices" },
                new JobOpening { Id = 4, Title = "Viewer Support Specialist", Department = "Support", Location = "Mumbai (Hybrid)", Type = "Full Time", Experience = "1-2 years", Salary = "INR 6-9 LPA", PostedOn = new DateTime(2025, 9, 28),
                    Summary = "Be the human on the other side of the chat for billing, playback and account questions.",
                    Responsibilities = "Answer chat, mail and phone contacts within agreed response times\nEscalate genuine outages to engineering with clean reproduction steps\nSpot recurring issues and file them weekly",
                    Requirements = "Clear written communication in English and one Indian language\nPatience when a viewer is frustrated\nBasic troubleshooting of streaming playback",
                    Benefits = "Shift allowance\nCertification path to support lead" },
                new JobOpening { Id = 5, Title = "Data Analyst - Engagement", Department = "Engineering", Location = "Remote (India)", Type = "Contract", Experience = "2-3 years", Salary = "INR 12-16 LPA", PostedOn = new DateTime(2025, 9, 12),
                    Summary = "Turn watch data into decisions about what we commission, schedule and promote.",
                    Responsibilities = "Build and maintain the engagement dashboards\nModel retention cohorts and churn risk\nPresent findings to non technical stakeholders",
                    Requirements = "Strong SQL and one BI tool\nExperience with product analytics pipelines\nAble to explain a number and its caveats clearly",
                    Benefits = "Six month contract with extension option\nFlexible hours" },
                new JobOpening { Id = 6, Title = "Marketing Intern - Originals", Department = "Content", Location = "Mumbai (On-site)", Type = "Internship", Experience = "0-1 years", Salary = "INR 15-20k / month", PostedOn = new DateTime(2025, 9, 2),
                    Summary = "Support release campaigns for original premieres across social, OOH and partnerships.",
                    Responsibilities = "Draft social copy and calendar posts for release weeks\nMaintain the campaign asset library\nTrack performance of every post and report weekly",
                    Requirements = "Currently studying media, marketing or communications\nStrong writing and basic design sense\nAvailable for six months, four days a week",
                    Benefits = "Paid internship\nReal campaign ownership\nMentorship from the marketing lead" }
            );


            db.Faqs.AddRange(
                new FaqItem { Id = 1, Question = "Which devices can I watch FanHub Plus on?", Answer = "FanHub Plus plays on smart TVs (Android TV, tvOS, Roku and Fire TV), phones and tablets on iOS and Android, desktop browsers, and streaming dongles. You can be signed in on up to ten devices and watch on four screens at the same time on Premium 4K UHD.", Topic = "Devices", IsPopular = true },
                new FaqItem { Id = 2, Question = "Can I cancel my subscription at any time?", Answer = "Yes. Open My Account, choose Plan, and hit Cancel. Your plan stays active until the end of the period you already paid for, and there is no cancellation fee.", Topic = "Plans", IsPopular = true },
                new FaqItem { Id = 3, Question = "Does FanHub Plus show ads?", Answer = "No. Every plan is commercial free. The store and the free live channels are the only places you will ever see a promo from us, and never during a title.", Topic = "Plans" },
                new FaqItem { Id = 4, Question = "How do offline downloads work?", Answer = "Download any title on a phone, tablet or laptop and it stays available for up to thirty days. Downloads renew automatically while your plan is active, and smart storage quietly clears titles you have finished.", Topic = "Devices", IsPopular = true },
                new FaqItem { Id = 5, Question = "Do you have a free trial?", Answer = "Premium 4K UHD comes with seven days free. We only ask for payment details when the trial ends, and you can cancel in two clicks before that.", Topic = "Billing", IsPopular = true },
                new FaqItem { Id = 6, Question = "Can I create profiles for my children?", Answer = "Yes, up to five extra profiles per account. Kid profiles only show titles inside a certified age band, and you can lock purchases behind a parental PIN.", Topic = "Account" },
                new FaqItem { Id = 7, Question = "Why is a title not available in my region?", Answer = "Licensing windows differ by country. If a title is missing it is usually because local rights have not started yet - the details page shows the expected date where we know it.", Topic = "General" },
                new FaqItem { Id = 8, Question = "How do I change or reset my password?", Answer = "Use the Forgot Password link on the Sign In page and we will email a single use reset link that expires after thirty minutes. If the email does not arrive, check the spam folder or contact support.", Topic = "Account" },
                new FaqItem { Id = 9, Question = "What payment methods do you accept?", Answer = "All major cards, net banking, UPI and supported digital wallets. Invoices are available in My Account under Billing for every payment you make.", Topic = "Billing" },
                new FaqItem { Id = 10, Question = "Can I watch live sport with a delay?", Answer = "Live channels run with a five to eight second delay over the internet. Fixtures that carry broadcast rights in your region may be geo-blocked, and the channel page will say so before you press play.", Topic = "General" }
            );

            db.SaveChanges();
        }


    }
}
