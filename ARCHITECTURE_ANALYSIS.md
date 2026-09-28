# FanHubPlus Architecture Analysis & Restructuring Plan

## Current State Analysis

### ✅ Strengths
- **Clean MVC Structure**: Controllers, Views, ViewModels, Models, Services, Repositories
- **Dependency Injection**: Proper DI container usage with scoped services
- **Repository Pattern**: Generic `IRepository<T>` with Unit of Work via shared DbContext
- **Identity Integration**: Full ASP.NET Core Identity with roles, lockout, email confirmation
- **Security Basics**: Anti-forgery tokens, rate limiting, secure cookies, security headers
- **Database Design**: Well-normalized schema with proper FK relationships, indexes, enums as strings
- **YouTube Integration**: `MediaUrl` service handles YouTube/Vimeo embed normalization
- **Modular Services**: Separate services for Content, Chatbot, Bookmarks, Stats, Email, etc.

### ⚠️ Gaps vs Industry Standards

| Area | Current | Industry Standard | Gap |
|------|---------|-------------------|-----|
| **Caching** | None | Multi-layer (Memory, Redis, Response, Output) | Critical |
| **Query Optimization** | Basic Include/AsSplitQuery | Projection, compiled queries, batching | High |
| **API Layer** | None (server-rendered only) | RESTful API + GraphQL option | High |
| **Testing** | Empty test project | Unit, Integration, E2E tests | High |
| **Observability** | Basic logging | Structured logging, metrics, health checks, tracing | High |
| **Asset Pipeline** | Raw libs in wwwroot | Bundling, minification, CDN, cache busting | High |
| **Game Integration** | ContentType.Game exists only | Playable games module | High |
| **Security Headers** | Basic 4 headers | Full CSP, Permissions-Policy, COOP/COEP | Medium |
| **Error Handling** | Basic exception page | Global error handling, problem details | Medium |
| **Validation** | DataAnnotations only | FluentValidation, client + server | Medium |

---

## Restructuring Plan

### Phase 1: Architecture & Foundation (Week 1)

#### 1.1 Project Structure Reorganization
```
FanHubPlus/
├── src/
│   ├── FanHubPlus.Web/           # ASP.NET Core MVC App (Controllers, Views, wwwroot)
│   ├── FanHubPlus.Core/          # Domain models, interfaces, exceptions
│   ├── FanHubPlus.Application/   # Services, DTOs, Validators, Use Cases
│   ├── FanHubPlus.Infrastructure/# EF Core, Repositories, External Services
│   └── FanHubPlus.Api/           # REST API Controllers, OpenAPI/Swagger
├── tests/
│   ├── FanHubPlus.UnitTests/
│   ├── FanHubPlus.IntegrationTests/
│   └── FanHubPlus.E2ETests/
└── build/                        # CI/CD, Docker, scripts
```

#### 1.2 Core Infrastructure Improvements
- **Caching Abstraction**: `ICacheService` with MemoryCache + Redis implementations
- **Configuration**: Strongly-typed options with validation
- **Health Checks**: Database, Redis, external APIs
- **Structured Logging**: Serilog with Seq/Elasticsearch sinks
- **OpenAPI/Swagger**: API documentation with Scalar UI

### Phase 2: Performance Optimization (Week 1-2)

#### 2.1 Database Query Optimization
```csharp
// BEFORE: Loading entire entities
var contents = await _contents.Query()
    .Include(c => c.Category)
    .Include(c => c.MediaItems)
    .ToListAsync();

// AFTER: Projection to DTOs
var contents = await _contents.Query()
    .Where(...)
    .Select(c => new ContentDto {
        Id = c.ContentId,
        Title = c.Title,
        ThumbnailUrl = c.ThumbnailUrl,
        CategoryName = c.Category.Name,
        TrailerUrl = c.MediaItems
            .Where(m => m.MediaType == MediaType.Trailer)
            .Select(m => m.EmbedUrl)
            .FirstOrDefault()
    })
    .AsNoTracking()
    .ToListAsync();
```

#### 2.2 Caching Strategy
| Data Type | Cache Layer | TTL | Invalidation |
|-----------|-------------|-----|--------------|
| Categories | Memory (L1) + Redis (L2) | 1 hour | On write |
| Trending Content | Redis | 15 min | Scheduled refresh |
| User Profile | Memory | 5 min | On update |
| Static Assets | CDN + Response Cache | 1 year | Versioned URLs |
| API Responses | Response Caching | 30-300s | ETag/Last-Modified |

#### 2.3 Asset Optimization
- **Bundling**: WebOptimizer for CSS/JS minification
- **Critical CSS**: Inline above-the-fold styles
- **Lazy Loading**: Images, video players, non-critical JS
- **Preloading**: Key resources (fonts, hero images)
- **Service Worker**: Offline support, cache-first for static assets

### Phase 3: Gaming Module (Week 2)

#### 3.1 Domain Model Extensions
```csharp
public class Game : Content
{
    public GamePlatform Platform { get; set; } // Browser, Steam, Epic, etc.
    public GameGenre Genre { get; set; }       // Action, RPG, Strategy, etc.
    public string? PlayUrl { get; set; }       // Direct play URL (iframe)
    public string? EmbedHtml { get; set; }     // Pre-built embed code
    public GameControls Controls { get; set; } // Keyboard, Gamepad, Touch
    public bool SupportsMultiplayer { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string? SaveDataKey { get; set; }   // For browser game saves
}

public enum GamePlatform { Browser, Steam, Epic, GOG, Itch, Custom }
public enum GameGenre { Action, Adventure, RPG, Strategy, Puzzle, Sports, Racing, Simulation, Multiplayer }
```

#### 3.2 Game Integration Patterns
1. **HTML5 Games**: Direct iframe embedding (itch.io, custom)
2. **Cloud Gaming**: GeForce Now, Xbox Cloud Gaming deep links
3. **Native Launchers**: Steam/Epic protocol handlers (`steam://run/12345`)
4. **WebAssembly**: WASM games running in-browser
5. **Retro/Emulator**: JS-based emulators (RetroArch web)

#### 3.3 New Controllers & Views
- `GamesController` - Browse, search, filter games
- `GamePlayController` - Play page with fullscreen iframe
- `GameSavesController` - Cloud save sync for browser games
- `GameLeaderboardController` - Scores, achievements

### Phase 4: Security Hardening (Week 2)

#### 4.1 Content Security Policy
```csharp
// Strict CSP with nonces for inline scripts
app.Use(async (ctx, next) => {
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    ctx.Items["CspNonce"] = nonce;
    ctx.Response.Headers["Content-Security-Policy"] = 
        $"default-src 'self'; " +
        $"script-src 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net; " +
        $"style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        $"img-src 'self' data: https:; " +
        $"font-src 'self' https://fonts.gstatic.com; " +
        $"frame-src https://www.youtube.com https://player.vimeo.com https://itch.io; " +
        $"connect-src 'self' https://api.fanhubplus.com; " +
        $"frame-ancestors 'none'; " +
        $"base-uri 'self'; " +
        $"form-action 'self';";
    await next();
});
```

#### 4.2 Additional Security
- **Rate Limiting**: Per-user + per-IP with sliding window
- **XSS Prevention**: HTML sanitization for user content (HtmlSanitizer)
- **CSRF**: Already implemented with antiforgery tokens
- **SQL Injection**: Parameterized queries via EF Core (already safe)
- **Secrets**: Azure Key Vault / User Secrets / Environment variables only

### Phase 5: Quality Assurance (Week 2-3)

#### 5.1 Testing Strategy
```
Unit Tests (xUnit + Moq):
├── Services: ContentService, ChatbotService, Rating logic
├── Validators: FluentValidation rules
├── Helpers: MediaUrl, SlugHelper, MediaPlayerViewModel
└── Domain: Entity behavior, enum conversions

Integration Tests (TestContainer SQL Server):
├── Repository CRUD operations
├── DbSeeder idempotency
├── Identity flows (register, login, password reset)
├── Controller actions with TestServer

E2E Tests (Playwright):
├── Critical user journeys: Register → Browse → Rate → Bookmark
├── Game play flow: Search → Play → Save progress
├── Admin workflows: Content CRUD, moderation
└── Accessibility: WCAG 2.1 AA compliance
```

#### 5.2 Code Quality Gates
- **Static Analysis**: SonarCloud / Roslyn analyzers
- **Style**: EditorConfig + dotnet format
- **Complexity**: Cyclomatic complexity < 10 per method
- **Coverage**: Minimum 80% line coverage for business logic

---

## Performance Targets

| Metric | Current | Target | Method |
|--------|---------|--------|--------|
| **Home Page TTFB** | ~800ms | <100ms | Response caching + Redis |
| **Explorer Page Load** | ~1.2s | <200ms | Projection + Redis + Pagination |
| **Detail Page Load** | ~900ms | <150ms | Cached content + lazy media |
| **API Response (p95)** | N/A | <50ms | Compiled queries + Memory cache |
| **Database CPU** | High | <30% | Query optimization + indexes |
| **Memory Usage** | Unknown | <500MB | Object pooling, proper disposal |
| **Lighthouse Score** | ~25 | >95 | All above + asset optimization |

---

## Gaming Module Specification

### Supported Game Sources
1. **Itch.io Embeds** - iframe with `https://itch.io/embed/12345`
2. **Custom HTML5/JS Games** - Self-hosted in `/games/{id}/index.html`
3. **WebAssembly Games** - `.wasm` + JS loader
4. **Cloud Gaming Links** - Deep links to GeForce Now, Xbox Cloud
5. **Steam/Epic Protocol** - `steam://run/{appId}` for installed games

### Game Play Page Features
- Fullscreen toggle (Fullscreen API)
- Gamepad support (Gamepad API)
- Keyboard remapping
- Save/Load state (IndexedDB + cloud sync)
- Screenshot capture (Canvas API)
- Share button (Web Share API)
- Related games sidebar
- Comments/ratings on game

### Admin Game Management
- Upload HTML5 game zip → auto-extract to wwwroot/games/{id}
- Configure iframe URL for external games
- Set age rating, genre, platform
- Featured games carousel
- Analytics: plays, avg session time, completion rate

---

## Migration Strategy

### Step 1: Branch & Baseline
```bash
git checkout -b restructure/v2
dotnet test --collect:"XPlat Code Coverage"
# Record baseline metrics
```

### Step 2: Incremental Refactoring
1. Extract Core/Application/Infrastructure projects
2. Add caching layer (feature flag gated)
3. Optimize queries one controller at a time
4. Add API controllers alongside MVC
5. Implement gaming module
6. Security headers + CSP
7. Tests for each changed component

### Step 3: Validation
- Load testing with k6/nbomber
- Security scan (OWASP ZAP)
- Accessibility audit (axe-core)
- Cross-browser testing

### Step 4: Deploy
- Blue-green deployment
- Feature flags for gradual rollout
- Monitoring dashboards (Grafana + Prometheus)
- Rollback plan

---

## Risk Mitigation

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking existing views | High | High | Keep MVC controllers, add API alongside |
| Database migration issues | Medium | High | Test migrations on staging clone first |
| Cache invalidation bugs | Medium | Medium | Comprehensive integration tests |
| Game iframe security | Medium | High | Sandbox attribute, CSP frame-src |
| Performance regression | Low | High | Continuous benchmarking in CI |

---

## Success Criteria

- [ ] Lighthouse Performance > 95
- [ ] API p95 latency < 50ms
- [ ] 0 critical/high security findings
- [ ] 80%+ test coverage on business logic
- [ ] All existing features work identically
- [ ] Gaming module plays 5+ demo games
- [ ] Zero-downtime deployment capability