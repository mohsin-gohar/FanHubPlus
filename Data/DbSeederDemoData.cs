using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Data;

// Demo catalogue data for the competition demo / judge walkthrough.
// Split from DbSeeder.cs (partial class). Every block is guarded by
// "only insert when the table is EMPTY", so re-runs are safe and
// admin-created rows are never overwritten.
public partial class DbSeeder
{
    private async Task SeedDemoDataAsync()
    {
        var cat = _db.Categories.ToDictionary(c => c.Name, c => c.CategoryId);
        // The catalogue may be enabled independently of demo accounts. Use the first
        // available configured account only as an author, never as a login shortcut.
        var configuredAdminEmail = _configuration["Database:SeedAdminEmail"]?.Trim();
        var configuredUserEmail = _configuration["Database:SeedUserEmail"]?.Trim();
        var admin = configuredAdminEmail is null ? null : await _userManager.FindByEmailAsync(configuredAdminEmail);
        var demo = configuredUserEmail is null ? null : await _userManager.FindByEmailAsync(configuredUserEmail);
        var authorId = admin?.Id ?? demo?.Id;
        var now = DateTime.UtcNow;

        // ---------- Tags ----------
        if (!_db.Tags.Any())
        {
            _db.Tags.AddRange(
                new Tag { Name = "Action" }, new Tag { Name = "Adventure" },
                new Tag { Name = "Fantasy" }, new Tag { Name = "Shonen" },
                new Tag { Name = "RPG" }, new Tag { Name = "Superhero" },
                new Tag { Name = "Sci-Fi" }, new Tag { Name = "Esports" },
                new Tag { Name = "Classic" }, new Tag { Name = "Romance" });
            await _db.SaveChangesAsync();
        }

        // ---------- Contents (Explorer catalogue) ----------
        // The list lives outside the empty-table guard below so the backfill
        // after it can reuse it. Both paths are title-keyed and never touch a
        // row that already exists.
        var contents = new List<Content>
        {
                new() { CategoryId = cat["Anime"], Title = "Attack on Titan", Type = ContentType.Series,
                        Genre = "Action, Dark Fantasy", PopularityScore = 980, ViewCount = 15400,
                        ReleaseDate = new DateTime(2013, 4, 7),
                        Description = "Humanity fights for survival behind giant walls against man-eating Titans. Eren Yeager's quest for freedom becomes one of anime's most acclaimed epics." },
                new() { CategoryId = cat["Anime"], Title = "Spirited Away", Type = ContentType.Movie,
                        Genre = "Fantasy, Adventure", PopularityScore = 940, ViewCount = 9800,
                        ReleaseDate = new DateTime(2001, 7, 20),
                        Description = "Studio Ghibli's Oscar-winning masterpiece. Chihiro must work in a bathhouse for spirits to free her parents." },
                new() { CategoryId = cat["Manga"], Title = "One Piece", Type = ContentType.Series,
                        Genre = "Adventure, Shonen", PopularityScore = 990, ViewCount = 16200,
                        ReleaseDate = new DateTime(1997, 7, 22),
                        Description = "Monkey D. Luffy and the Straw Hat crew sail the Grand Line in search of the legendary One Piece treasure." },
                new() { CategoryId = cat["Gaming"], Title = "Elden Ring", Type = ContentType.Game,
                        Genre = "Action RPG", PopularityScore = 960, ViewCount = 13100,
                        ReleaseDate = new DateTime(2022, 2, 25),
                        Description = "FromSoftware's open-world masterpiece written with George R.R. Martin. Rise, Tarnished, and claim the Elden Ring." }
                ,
                new() { CategoryId = cat["Gaming"], Title = "The Legend of Zelda: Tears of the Kingdom", Type = ContentType.Game,
                        Genre = "Action-Adventure", PopularityScore = 900, ViewCount = 8400,
                        ReleaseDate = new DateTime(2023, 5, 12),
                        Description = "Link returns to a Hyrule floating in the sky. Build, fuse and explore in the sequel to Breath of the Wild." },
                new() { CategoryId = cat["Movies"], Title = "Avengers: Endgame", Type = ContentType.Movie,
                        Genre = "Superhero, Action", PopularityScore = 950, ViewCount = 14800,
                        ReleaseDate = new DateTime(2019, 4, 26),
                        Description = "The Infinity Saga finale. The surviving Avengers assemble for one last stand against Thanos." },
                new() { CategoryId = cat["Movies"], Title = "Dune: Part Two", Type = ContentType.Movie,
                        Genre = "Sci-Fi, Epic", PopularityScore = 910, ViewCount = 7600,
                        ReleaseDate = new DateTime(2024, 3, 1),
                        Description = "Paul Atreides unites with the Fremen and wages war against House Harkonnen on Arrakis." },
                new() { CategoryId = cat["TV Shows"], Title = "Stranger Things", Type = ContentType.Series,
                        Genre = "Sci-Fi, Horror", PopularityScore = 920, ViewCount = 11200,
                        ReleaseDate = new DateTime(2016, 7, 15),
                        Description = "A group of kids in Hawkins confront the Upside Down. Netflix's flagship 80s-nostalgia phenomenon." },
                new() { CategoryId = cat["TV Shows"], Title = "The Last of Us", Type = ContentType.Series,
                        Genre = "Action, Drama", PopularityScore = 905, ViewCount = 8900,
                        ReleaseDate = new DateTime(2023, 1, 15),
                        Description = "Joel and Ellie cross a post-apocalyptic America in HBO's acclaimed adaptation of the game." },
                new() { CategoryId = cat["K-Pop"], Title = "BTS: Yet to Come in Cinemas", Type = ContentType.Special,
                        Genre = "Concert Film", PopularityScore = 870, ViewCount = 6200,
                        ReleaseDate = new DateTime(2023, 2, 1),
                        Description = "BTS's record-breaking Busan concert, remastered for the big screen." },
                new() { CategoryId = cat["K-Pop"], Title = "BLACKPINK: The Show", Type = ContentType.Special,
                        Genre = "Concert Film", PopularityScore = 850, ViewCount = 5400,
                        ReleaseDate = new DateTime(2021, 8, 8),
                        Description = "BLACKPINK's livestream concert spectacle with solo stages from all four members." },
                new() { CategoryId = cat["Comics"], Title = "Batman: The Long Halloween", Type = ContentType.Series,
                        Genre = "Superhero, Noir", PopularityScore = 830, ViewCount = 4800,
                        ReleaseDate = new DateTime(1996, 12, 1),
                        Description = "Jeph Loeb and Tim Sale's noir classic: a year-long hunt for the Holiday killer in Gotham." },
                new() { CategoryId = cat["Cosplay"], Title = "Cosplay Culture: Behind the Mask", Type = ContentType.Documentary,
                        Genre = "Documentary", PopularityScore = 700, ViewCount = 2100,
                        ReleaseDate = new DateTime(2022, 9, 10),
                        Description = "A look inside the craftsmanship, competitions and community of global cosplay culture." },
                new() { CategoryId = cat["Gaming"], Title = "The Esports Phenomenon", Type = ContentType.Documentary,
                        Genre = "Documentary, Esports", PopularityScore = 720, ViewCount = 2600,
                        ReleaseDate = new DateTime(2023, 11, 5),
                        Description = "From basement LAN parties to sold-out arenas: how competitive gaming became a billion-dollar industry." },
                new() { CategoryId = cat["Anime"], Title = "Unravel (Tokyo Ghoul OP)", Type = ContentType.Song,
                        Genre = "Anime OST, Rock", Artist = "TK from Ling Tosite Sigure", AlbumName = "Tokyo Ghoul Original Soundtrack",
                        PopularityScore = 970, ViewCount = 18900, ReleaseDate = new DateTime(2014, 7, 23),
                        Description = "The iconic opening theme song for Tokyo Ghoul performed by TK from Ling Tosite Sigure." },
                new() { CategoryId = cat["Anime"], Title = "Gurenge (Demon Slayer OP)", Type = ContentType.Song,
                        Genre = "Anime OST, J-Pop", Artist = "LiSA", AlbumName = "LEO-NiNE / Demon Slayer",
                        PopularityScore = 960, ViewCount = 17500, ReleaseDate = new DateTime(2019, 4, 22),
                        Description = "LiSA's record-smashing opening theme song for Demon Slayer: Kimetsu no Yaiba." },
                new() { CategoryId = cat["K-Pop"], Title = "Dynamite", Type = ContentType.Song,
                        Genre = "K-Pop, Disco Pop", Artist = "BTS", AlbumName = "BE",
                        PopularityScore = 995, ViewCount = 28400, ReleaseDate = new DateTime(2020, 8, 21),
                        Description = "BTS's Grammy-nominated disco-pop single that topped the Billboard Hot 100." },
                new() { CategoryId = cat["Gaming"], Title = "Elden Ring Main Theme", Type = ContentType.Song,
                        Genre = "Orchestral, Game Soundtrack", Artist = "Yuka Kitamura", AlbumName = "Elden Ring Official Soundtrack",
                        PopularityScore = 930, ViewCount = 9200, ReleaseDate = new DateTime(2022, 2, 25),
                        Description = "The epic orchestral title screen music from Elden Ring." },
                new() { CategoryId = cat["Gaming"], Title = "2048 Web Classic", Type = ContentType.PlayableGame,
                        Genre = "Puzzle, Casual", PopularityScore = 880, ViewCount = 12300,
                        ReleaseDate = new DateTime(2014, 3, 9), PlayableGameUrl = "https://play2048.co/",
                        OfficialWebsiteUrl = "https://play2048.co/",
                        Description = "The famous tile-matching puzzle game. Join the numbers to get to the 2048 tile!" },
                new() { CategoryId = cat["Gaming"], Title = "Pac-Man Arcade Classic", Type = ContentType.PlayableGame,
                        Genre = "Arcade, Retro", PopularityScore = 910, ViewCount = 14500,
                        ReleaseDate = new DateTime(1980, 5, 22), PlayableGameUrl = "https://freepacman.org/",
                        OfficialWebsiteUrl = "https://freepacman.org/",
                        Description = "Guide Pac-Man through the maze, eat dots, and avoid Blinky, Pinky, Inky, and Clyde!" },
                new() { CategoryId = cat["Movies"], Title = "Big Buck Bunny", Type = ContentType.Movie,
                        Genre = "Animation, Comedy", PopularityScore = 875, ViewCount = 6400,
                        ThumbnailUrl = "/assets/images/movies/movie1.jpg",
                        ReleaseDate = new DateTime(2008, 4, 10),
                        Description = "Open movie from the Blender Foundation. Watch the full short film here on FanHubPlus." },
                new() { CategoryId = cat["Movies"], Title = "Sintel", Type = ContentType.Movie,
                        Genre = "Animation, Fantasy", PopularityScore = 865, ViewCount = 5100,
                        ThumbnailUrl = "/assets/images/movies/movie2.jpg",
                        ReleaseDate = new DateTime(2010, 9, 27),
                        Description = "Blender Foundation's open fantasy short. Stream the complete film in the Movies hub." },
                new() { CategoryId = cat["Movies"], Title = "Tears of Steel", Type = ContentType.Movie,
                        Genre = "Sci-Fi, Live Action", PopularityScore = 850, ViewCount = 4300,
                        ThumbnailUrl = "/assets/images/movies/movie3.jpg",
                        ReleaseDate = new DateTime(2012, 9, 26),
                        Description = "Open live-action / CGI short from the Blender Institute. Play it in the browser." },
                new() { CategoryId = cat["Movies"], Title = "Elephants Dream", Type = ContentType.Movie,
                        Genre = "Animation, Experimental", PopularityScore = 820, ViewCount = 3900,
                        ThumbnailUrl = "/assets/images/movies/movie4.jpg",
                        ReleaseDate = new DateTime(2006, 3, 24),
                        Description = "The first open movie from the Orange Open Movie Project. Full video on-site." },
                new() { CategoryId = cat.GetValueOrDefault("Music", cat["K-Pop"]), Title = "SoundHelix Session 1", Type = ContentType.Song,
                        Genre = "Electronic, Royalty-free", Artist = "SoundHelix", AlbumName = "Studio Samples",
                        PopularityScore = 780, ViewCount = 2100, ReleaseDate = new DateTime(2010, 1, 1),
                        ThumbnailUrl = "/assets/images/categories/category1.jpg",
                        Description = "Royalty-free instrumental you can play instantly from the Music hub." },
                new() { CategoryId = cat.GetValueOrDefault("Music", cat["K-Pop"]), Title = "SoundHelix Session 2", Type = ContentType.Song,
                        Genre = "Electronic, Royalty-free", Artist = "SoundHelix", AlbumName = "Studio Samples",
                        PopularityScore = 770, ViewCount = 1800, ReleaseDate = new DateTime(2010, 1, 1),
                        ThumbnailUrl = "/assets/images/categories/category5.jpg",
                        Description = "A second royalty-free instrumental for in-browser listening." },
                new() { CategoryId = cat.GetValueOrDefault("Music", cat["K-Pop"]), Title = "Moonlight Sonata", Type = ContentType.Song,
                        Genre = "Classical, Piano", Artist = "Ludwig van Beethoven", AlbumName = "Public Domain Classics",
                        PopularityScore = 800, ViewCount = 3200, ReleaseDate = new DateTime(1801, 1, 1),
                        ThumbnailUrl = "/assets/images/categories/category6.jpg",
                        Description = "Public-domain piano recording. Listen on-site without leaving FanHubPlus." },
                new() { CategoryId = cat["Gaming"], Title = "Hextris", Type = ContentType.PlayableGame,
                        Genre = "Puzzle, Arcade", PopularityScore = 890, ViewCount = 9800,
                        ReleaseDate = new DateTime(2014, 1, 1), PlayableGameUrl = "https://hextris.io/",
                        OfficialWebsiteUrl = "https://hextris.io/",
                        ThumbnailUrl = "/assets/images/categories/category2.jpg",
                        Description = "Open-source hexagon puzzle. Play it instantly in your browser." },
                new() { CategoryId = cat["Gaming"], Title = "Clumsy Bird", Type = ContentType.PlayableGame,
                        Genre = "Arcade, Casual", PopularityScore = 840, ViewCount = 7600,
                        ReleaseDate = new DateTime(2014, 1, 1), PlayableGameUrl = "https://ellisonleao.github.io/clumsy-bird/",
                        OfficialWebsiteUrl = "https://ellisonleao.github.io/clumsy-bird/",
                        ThumbnailUrl = "/assets/images/categories/category4.jpg",
                        Description = "Open-source Flappy Bird-style game. Tap to fly, play in the browser." },
                new() { CategoryId = cat["Gaming"], Title = "Astray", Type = ContentType.PlayableGame,
                        Genre = "Puzzle, Maze", PopularityScore = 830, ViewCount = 5400,
                        ReleaseDate = new DateTime(2015, 1, 1), PlayableGameUrl = "https://wwwtyro.github.io/Astray/",
                        OfficialWebsiteUrl = "https://wwwtyro.github.io/Astray/",
                        ThumbnailUrl = "/assets/images/categories/category7.jpg",
                        Description = "WebGL maze explorer. WASD to move, play full-screen on FanHubPlus." }
        };
        if (!_db.Contents.Any())
        {
            _db.Contents.AddRange(contents);
            await _db.SaveChangesAsync();

            // Tag links + trailer embeds for a few flagship items
            var tagId = _db.Tags.ToDictionary(t => t.Name, t => t.TagId);
            int CId(string title) => contents.First(c => c.Title == title).ContentId;

            _db.ContentTags.AddRange(
                new ContentTag { ContentId = CId("Attack on Titan"), TagId = tagId["Action"] },
                new ContentTag { ContentId = CId("Attack on Titan"), TagId = tagId["Shonen"] },
                new ContentTag { ContentId = CId("One Piece"), TagId = tagId["Adventure"] },
                new ContentTag { ContentId = CId("One Piece"), TagId = tagId["Shonen"] },
                new ContentTag { ContentId = CId("Elden Ring"), TagId = tagId["RPG"] },
                new ContentTag { ContentId = CId("Elden Ring"), TagId = tagId["Fantasy"] },
                new ContentTag { ContentId = CId("The Legend of Zelda: Tears of the Kingdom"), TagId = tagId["Adventure"] },
                new ContentTag { ContentId = CId("The Legend of Zelda: Tears of the Kingdom"), TagId = tagId["RPG"] },
                new ContentTag { ContentId = CId("Avengers: Endgame"), TagId = tagId["Superhero"] },
                new ContentTag { ContentId = CId("Avengers: Endgame"), TagId = tagId["Action"] },
                new ContentTag { ContentId = CId("Dune: Part Two"), TagId = tagId["Sci-Fi"] },
                new ContentTag { ContentId = CId("Stranger Things"), TagId = tagId["Sci-Fi"] },
                new ContentTag { ContentId = CId("Spirited Away"), TagId = tagId["Classic"] });

            _db.MediaItems.AddRange(
                new MediaItem { ContentId = CId("Attack on Titan"), MediaType = MediaType.Trailer,
                                EmbedUrl = "https://www.youtube.com/embed/MGRm4IzK1SQ", Tag = "Official Trailer" },
                new MediaItem { ContentId = CId("Elden Ring"), MediaType = MediaType.Trailer,
                                EmbedUrl = "https://www.youtube.com/embed/E3Huy2cdih0", Tag = "Launch Trailer" },
                new MediaItem { ContentId = CId("Dune: Part Two"), MediaType = MediaType.Trailer,
                                EmbedUrl = "https://www.youtube.com/embed/Way9Dexny3w", Tag = "Official Trailer" },
                new MediaItem { ContentId = CId("Unravel (Tokyo Ghoul OP)"), MediaType = MediaType.Audio,
                                EmbedUrl = "https://www.youtube.com/embed/7aMOurgDB-U", Tag = "Official Theme" },
                new MediaItem { ContentId = CId("Dynamite"), MediaType = MediaType.Audio,
                                EmbedUrl = "https://www.youtube.com/embed/gdZLi9oWNZg", Tag = "Official Video" },
                new MediaItem { ContentId = CId("Gurenge (Demon Slayer OP)"), MediaType = MediaType.Audio,
                                EmbedUrl = "https://www.youtube.com/embed/CwkzK-F0Hs0", Tag = "Official Track" });

            await _db.SaveChangesAsync();
        }

        // ---------- Catalogue backfill (title-keyed, idempotent) ----------
        // A database seeded by an older demo script predates rows the catalogue
        // gained later (the four music tracks and the two playable games), and
        // the empty-table guard above can never add them. Insert only demo
        // titles the table is still missing, then wire up the audio embeds —
        // keyed by EmbedUrl so a re-run is a no-op. Existing rows are never
        // updated or deleted, so admin edits stay untouched.
        var seededTitles = _db.Contents.Select(c => c.Title).ToList();
        var missingContents = contents.Where(c => !seededTitles.Contains(c.Title)).ToList();
        if (missingContents.Count > 0)
        {
            _db.Contents.AddRange(missingContents);
            await _db.SaveChangesAsync();
        }

        var mediaEmbeds = new (string Title, MediaType Type, string Url, string Tag)[]
        {
            ("Unravel (Tokyo Ghoul OP)", MediaType.Audio, "https://www.youtube.com/embed/7aMOurgDB-U", "Official Theme"),
            ("Dynamite", MediaType.Audio, "https://www.youtube.com/embed/gdZLi9oWNZg", "Official Video"),
            ("Gurenge (Demon Slayer OP)", MediaType.Audio, "https://www.youtube.com/embed/CwkzK-F0Hs0", "Official Track"),
            ("Elden Ring Main Theme", MediaType.Audio, "https://www.youtube.com/embed/E3Huy2cdih0", "Official Theme"),
            ("Attack on Titan", MediaType.Trailer, "https://www.youtube.com/embed/MGRm4IzK1SQ", "Official Trailer"),
            ("Elden Ring", MediaType.Trailer, "https://www.youtube.com/embed/E3Huy2cdih0", "Launch Trailer"),
            ("Dune: Part Two", MediaType.Trailer, "https://www.youtube.com/embed/Way9Dexny3w", "Official Trailer"),
            ("Avengers: Endgame", MediaType.Trailer, "https://www.youtube.com/embed/TcMBFSGVi1c", "Official Trailer"),
            ("Stranger Things", MediaType.Trailer, "https://www.youtube.com/embed/b9EkMcBpZfg", "Official Trailer"),
            ("Spirited Away", MediaType.Trailer, "https://www.youtube.com/embed/ByXuk9QqQkk", "Official Trailer"),
            ("Big Buck Bunny", MediaType.Video, "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4", "Full Film"),
            ("Sintel", MediaType.Video, "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/Sintel.mp4", "Full Film"),
            ("Tears of Steel", MediaType.Video, "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4", "Full Film"),
            ("Elephants Dream", MediaType.Video, "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4", "Full Film"),
            ("SoundHelix Session 1", MediaType.Audio, "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3", "Full Track"),
            ("SoundHelix Session 2", MediaType.Audio, "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3", "Full Track"),
            ("Moonlight Sonata", MediaType.Audio, "https://upload.wikimedia.org/wikipedia/commons/4/47/Beethoven_Moonlight_1st_movement.ogg", "Public Domain"),
        };
        var embeddedUrls = _db.MediaItems.Select(m => m.EmbedUrl).ToList();
        var mediaToAdd = mediaEmbeds.Where(a => !embeddedUrls.Contains(a.Url)).ToList();
        if (mediaToAdd.Count > 0)
        {
            var mediaTitles = mediaToAdd.Select(a => a.Title).ToList();
            var mediaIds = _db.Contents
                .Where(c => mediaTitles.Contains(c.Title))
                .ToDictionary(c => c.Title, c => c.ContentId);
            foreach (var (title, type, url, tag) in mediaToAdd)
            {
                if (mediaIds.TryGetValue(title, out var id))
                    _db.MediaItems.Add(new MediaItem
                    {
                        ContentId = id,
                        MediaType = type,
                        EmbedUrl = url,
                        Tag = tag,
                    });
            }
            await _db.SaveChangesAsync();
        }

        // ---------- Articles (news + timeline stories) ----------
        if (!_db.Articles.Any())
        {
            _db.Articles.AddRange(
                new Article { CategoryId = cat["Anime"], AuthorId = authorId, PublishedAt = now.AddDays(-2),
                    Title = "Attack on Titan finale breaks global streaming records",
                    Body = "<p>The final episode of <strong>Attack on Titan</strong> drew record viewership across streaming platforms worldwide.</p><p>Fans praised the emotional conclusion to Eren Yeager's decade-long journey, cementing the series as a modern classic.</p>" },
                new Article { CategoryId = cat["Gaming"], AuthorId = authorId, PublishedAt = now.AddDays(-4),
                    Title = "GTA VI trailer shatters YouTube records in 24 hours",
                    Body = "<p>Rockstar's first <strong>GTA VI</strong> trailer broke YouTube records within a day of release, overtaking every previous game reveal.</p><p>The Vice City return is already the most anticipated game launch in history.</p>" },
                new Article { CategoryId = cat["Movies"], AuthorId = authorId, PublishedAt = now.AddDays(-6),
                    Title = "Marvel announces its next saga slate",
                    Body = "<p>Marvel Studios revealed the next chapter of the MCU, promising a return to grounded, character-driven storytelling.</p><p>Fan reactions at the announcement panel were electric.</p>" },
                new Article { CategoryId = cat["K-Pop"], AuthorId = authorId, PublishedAt = now.AddDays(-1),
                    Title = "BTS announces long-awaited reunion world tour",
                    Body = "<p>Following the completion of military service, <strong>BTS</strong> confirmed a global reunion tour.</p><p>Tickets are expected to sell out within minutes in every city.</p>" },
                new Article { CategoryId = cat["Movies"], AuthorId = authorId, PublishedAt = new DateTime(2008, 5, 2),
                    Title = "Iron Man launches the Marvel Cinematic Universe",
                    IsTimeline = true,
                    Body = "<p>In 2008, <strong>Iron Man</strong> hit theatres and quietly began the most successful shared universe in film history.</p><p>Its post-credits scene introduced the Avengers Initiative.</p>" },
                new Article { CategoryId = cat["Gaming"], AuthorId = authorId, PublishedAt = new DateTime(1986, 2, 21),
                    Title = "The Legend of Zelda debuts on the Famicom",
                    IsTimeline = true,
                    Body = "<p>Nintendo's 1986 release of <strong>The Legend of Zelda</strong> pioneered open exploration and battery-backed saves.</p><p>Nearly four decades later it remains one of gaming's most beloved franchises.</p>" });
            await _db.SaveChangesAsync();
        }
        // ---------- Events (map pins + calendar) ----------
        if (!_db.Events.Any())
        {
            _db.Events.AddRange(
                new EventItem { Title = "Anime Expo 2026", City = "Los Angeles", Type = "Convention",
                    Latitude = 34.0407, Longitude = -118.2695, EventDate = now.AddDays(30),
                    TicketUrl = "https://www.anime-expo.org",
                    Story = "North America's largest anime convention returns to the LA Convention Center with premieres, panels and cosplay gatherings." },
                new EventItem { Title = "Comic-Con International", City = "San Diego", Type = "Convention",
                    Latitude = 32.7157, Longitude = -117.1611, EventDate = now.AddDays(45),
                    TicketUrl = "https://www.comic-con.org",
                    Story = "The legendary pop-culture mega-event: studio Hall H reveals, artist alley and the world's best cosplay." },
                new EventItem { Title = "League of Legends Worlds Final", City = "Seoul", Type = "Esports",
                    Latitude = 37.5665, Longitude = 126.9780, EventDate = now.AddDays(60),
                    TicketUrl = "https://lolesports.com",
                    Story = "The Summoner's Cup is decided live in Seoul — the biggest night in esports." },
                new EventItem { Title = "Tokyo Game Show", City = "Tokyo", Type = "Convention",
                    Latitude = 35.6762, Longitude = 139.6503, EventDate = now.AddDays(21),
                    TicketUrl = "https://tgs.cesa.or.jp",
                    Story = "Japan's premier games expo at Makuhari Messe with playable demos of next year's biggest titles." },
                new EventItem { Title = "K-Pop Fan Meet: Gangnam", City = "Seoul", Type = "Fan Meet",
                    Latitude = 37.4979, Longitude = 127.0276, EventDate = now.AddDays(14),
                    Story = "A community-run fan meet with dance covers, photocard trading and lightstick ocean photos." },
                new EventItem { Title = "Online Cosplay Contest", City = "Online", Type = "Online",
                    Latitude = 0, Longitude = 0, EventDate = now.AddDays(7),
                    Story = "Submit your best cosplay photos online — community voting decides the winners." },
                new EventItem { Title = "Gamescom (last edition)", City = "Cologne", Type = "Convention",
                    Latitude = 50.9375, Longitude = 6.9603, EventDate = now.AddDays(-20),
                    Story = "Last year's edition drew 320,000 visitors — shown here so the 'past events' filter has data." });
            await _db.SaveChangesAsync();
        }
        // ---------- Merchandise (display-only showcase) ----------
        if (!_db.MerchandiseItems.Any())
        {
            _db.MerchandiseItems.AddRange(
                new MerchandiseItem { CategoryId = cat["Anime"], Name = "Attack on Titan Scout Regiment Hoodie",
                    Tag = MerchTag.LimitedEdition, ViewCount = 1200 },
                new MerchandiseItem { CategoryId = cat["Gaming"], Name = "Elden Ring Malenia Collectible Figure",
                    Tag = MerchTag.Collectible, ViewCount = 980 },
                new MerchandiseItem { CategoryId = cat["Gaming"], Name = "Zelda Master Sword Replica",
                    Tag = MerchTag.Collectible, ViewCount = 870 },
                new MerchandiseItem { CategoryId = cat["Movies"], Name = "Avengers Infinity Gauntlet Replica",
                    Tag = MerchTag.LimitedEdition, ViewCount = 760 },
                new MerchandiseItem { CategoryId = cat["K-Pop"], Name = "BTS Official Light Stick SE",
                    Tag = MerchTag.PreOrder, IsUpcoming = true, ReleaseDate = now.AddDays(30), ViewCount = 640 },
                new MerchandiseItem { CategoryId = cat["TV Shows"], Name = "Stranger Things Hellfire Club Tee",
                    Tag = MerchTag.None, ViewCount = 520 },
                new MerchandiseItem { CategoryId = cat["Comics"], Name = "Batman: The Long Halloween Hardcover",
                    Tag = MerchTag.PreOrder, IsUpcoming = true, ReleaseDate = now.AddDays(45), ViewCount = 310 },
                new MerchandiseItem { CategoryId = cat["Manga"], Name = "One Piece Going Merry Model Kit",
                    Tag = MerchTag.None, ViewCount = 480 });
            await _db.SaveChangesAsync();
        }

        // ---------- Character profiles ----------
        if (!_db.CharacterProfiles.Any())
        {
            _db.CharacterProfiles.AddRange(
                new CharacterProfile { CategoryId = cat["Anime"], Name = "Eren Yeager", Fandom = "Attack on Titan",
                    Bio = "The series' complex protagonist, driven by an unyielding desire for freedom beyond the walls." },
                new CharacterProfile { CategoryId = cat["Anime"], Name = "Levi Ackerman", Fandom = "Attack on Titan",
                    Bio = "Humanity's strongest soldier. Captain of the Survey Corps' Special Operations Squad." },
                new CharacterProfile { CategoryId = cat["Manga"], Name = "Monkey D. Luffy", Fandom = "One Piece",
                    Bio = "Captain of the Straw Hat Pirates. A rubber-bodied dreamer destined to become the Pirate King." },
                new CharacterProfile { CategoryId = cat["Gaming"], Name = "Link", Fandom = "The Legend of Zelda",
                    Bio = "The silent Hero of Hyrule, reincarnated across eras to defend the kingdom against Ganon." },
                new CharacterProfile { CategoryId = cat["Gaming"], Name = "Malenia, Blade of Miquella", Fandom = "Elden Ring",
                    Bio = "The most feared demigod of the Lands Between - undefeated in battle, bearer of the Scarlet Rot." },
                new CharacterProfile { CategoryId = cat["Comics"], Name = "Iron Man", Fandom = "Marvel",
                    Bio = "Genius, billionaire, playboy, philanthropist. Tony Stark's armour launched a cinematic universe." },
                new CharacterProfile { CategoryId = cat["TV Shows"], Name = "Eleven", Fandom = "Stranger Things",
                    Bio = "A girl with psychokinetic powers who escaped Hawkins Lab - Eggo enthusiast and party leader." },
                new CharacterProfile { CategoryId = cat["K-Pop"], Name = "Jungkook", Fandom = "BTS",
                    Bio = "BTS's golden maknae: main vocalist, centre and record-breaking solo artist." });
            await _db.SaveChangesAsync();
        }
        // ---------- Chatbot knowledge base (FAQ keyword matching) ----------
        if (!_db.ChatFaqs.Any())
        {
            _db.ChatFaqs.AddRange(
                new ChatFaq { Question = "How do ratings work?", Keywords = "rate,rating,stars,score",
                    Answer = "Open any content page and click the stars to rate it 1-5. You can change your rating any time; the average updates instantly." },
                new ChatFaq { Question = "How do I bookmark content?", Keywords = "bookmark,save,favourite,favorite,watchlist",
                    Answer = "Tap the Bookmark button on any content, character, article or merch page. Find everything under My Bookmarks in your account menu." },
                new ChatFaq { Question = "How do I enable dark mode?", Keywords = "dark,theme,light,display,colour,color",
                    Answer = "Use the theme toggle in the navigation bar, or set a permanent preference on your Profile page. Your choice is remembered across visits." },
                new ChatFaq { Question = "How do I create an account?", Keywords = "register,sign up,signup,account,login,log in",
                    Answer = "Click Register in the top-right menu. You only need a display name, an email and a password of 8+ characters." },
                new ChatFaq { Question = "Where can I see upcoming events?", Keywords = "event,events,convention,meetup,map,calendar",
                    Answer = "The Events page shows every upcoming fan event on an interactive map with dates, cities and ticket links." },
                new ChatFaq { Question = "Can I buy merchandise here?", Keywords = "buy,merch,merchandise,price,purchase,shop,order",
                    Answer = "No - Fan Hub Plus is a showcase only. You can browse limited editions and pre-orders, but no payments are handled on this portal." },
                new ChatFaq { Question = "How do I submit fan art?", Keywords = "fan art,fanart,submit,upload,gallery,story",
                    Answer = "Log in, open the Fan Art page and use the submission form. An admin reviews every piece before it appears in the public gallery." },
                new ChatFaq { Question = "How do I contact support?", Keywords = "contact,support,help,feedback,bug",
                    Answer = "Use the Support & FAQ page to send feedback (bugs, suggestions or questions). Admins track every message on their dashboard." });
            await _db.SaveChangesAsync();
        }
        // ---------- Sample ratings (so averages are non-zero in the demo) ----------
        if (!_db.Ratings.Any() && admin != null && demo != null)
        {
            var rated = _db.Contents.ToList();
            int RId(string title) => rated.First(c => c.Title == title).ContentId;

            _db.Ratings.AddRange(
                new Rating { UserId = admin.Id, ContentId = RId("Attack on Titan"), Stars = 5 },
                new Rating { UserId = demo.Id, ContentId = RId("Attack on Titan"), Stars = 5 },
                new Rating { UserId = admin.Id, ContentId = RId("Elden Ring"), Stars = 5 },
                new Rating { UserId = demo.Id, ContentId = RId("Elden Ring"), Stars = 4 },
                new Rating { UserId = admin.Id, ContentId = RId("Avengers: Endgame"), Stars = 4 },
                new Rating { UserId = demo.Id, ContentId = RId("One Piece"), Stars = 5 },
                new Rating { UserId = demo.Id, ContentId = RId("Dune: Part Two"), Stars = 4 });
            await _db.SaveChangesAsync();
        }

        // ---------- Demo fan submissions (one pending -> admin moderation demo) ----------
        if (!_db.FanSubmissions.Any() && demo != null)
        {
            _db.FanSubmissions.AddRange(
                new FanSubmission { UserId = demo.Id, Status = SubmissionStatus.Approved,
                    Title = "Why One Piece is the greatest adventure ever told",
                    Body = "After 1000+ chapters, Oda's world-building still surprises me.\n\nThe foreshadowing planted in volume 1 pays off decades later - no other series rewards long-term readers like this.", CreatedAt = now.AddDays(-5) },
                new FanSubmission { UserId = demo.Id, Status = SubmissionStatus.Pending,
                    Title = "My Levi Ackerman fan art process",
                    Body = "Sharing my step-by-step process for this Levi piece: sketch, lineart, cel shading and the blood-splatter effect.\n\nHope the community likes it!", CreatedAt = now.AddDays(-1) });
            await _db.SaveChangesAsync();
        }

        // ---------- Demo feedback rows (one open -> admin workflow demo) ----------
        if (!_db.Feedbacks.Any())
        {
            _db.Feedbacks.AddRange(
                new Feedback { UserId = demo?.Id, Type = FeedbackType.Suggestion, Status = FeedbackStatus.Open,
                    Message = "Could we get a 'recently viewed' section on the home page?", CreatedAt = now.AddDays(-2) },
                new Feedback { UserId = null, Type = FeedbackType.Bug, Status = FeedbackStatus.Resolved,
                    Message = "The events map did not load on my phone yesterday - seems fixed now.", CreatedAt = now.AddDays(-6) });
            await _db.SaveChangesAsync();
        }
    }
}
