# FanHubPlus audit and deployment runbook

Last updated: 2026-09-24. Scope: current working tree (ASP.NET Core 8 MVC, Microsoft SQL Server / T-SQL provider via `Microsoft.EntityFrameworkCore.SqlServer`).

## Confirmed issues and implemented fixes

| Area | Exact location | Root cause | Implemented resolution |
|---|---|---|---|
| Database Provider | `FanHubPlus.csproj`, `Program.cs`, `ApplicationDbContext.cs`, `ApplicationDbContextFactory.cs` | Inconsistent or partial MySQL transition | Standardized on `Microsoft.EntityFrameworkCore.SqlServer` with connection resiliency and split-query support |
| Schema Evolution | `Migrations/20260924153243_AddFanSubmissionImageUrl.cs`, `DbSeeder.cs`, `FanArtController.cs` | `FanSubmissions` lacked `ImageUrl` column, causing mismatch in fan art submissions | Generated and applied SQL Server migration `AddFanSubmissionImageUrl` (`nvarchar(300)` / `nvarchar(600)` schema aligned) without data loss |
| Production migrations | `Program.cs` startup block | Startup defaulted to applying migrations everywhere | `Database:StartupMode` defaults to `None` outside Development |
| Demo accounts | `Data/DbSeeder.cs` | Passwords and emails were hard-coded; base config enabled them | Account/data seeding is opt-in; passwords must come from configuration; role/reference data remains idempotent |
| Registration | `Controllers/AccountController.cs` | Every registration was marked confirmed | Confirmation follows `Identity:RequireConfirmedEmail`; production defaults to required |
| Password reset | `AccountController.ForgotPassword`, `Views/Account/Login.cshtml` | Reset token link could display whenever SMTP was disabled | Preview is Development-only |
| Email logging | `Services/EmailService.cs` | Disabled-delivery logs included full HTML email/token | Logs recipient/subject only |
| Login disclosure | `Views/Account/Login.cshtml` | Demo passwords printed publicly | Removed |
| Cookie security | `Program.cs` | `SameAsRequest` permits cookies over plain HTTP in production | `Always` outside Development |
| Upload deletion | `Services/FileUploadService.cs` | Prefix check could confuse sibling folder | Requires path prefix plus directory separator |
| Upload validation | `Services/FileUploadService.cs` | Client MIME/extension alone is unsafe | Magic-byte, MIME, extension, size, dimensions, random filename and root checks (now covered by tests) |
| Test isolation & safety | `tests/FanHubPlus.Tests` | Missing SQL Server identity test coverage and disposable safety checks | Dedicated test suite (`FanHubPlus.Tests`) targeting SQL Server with strict safety checks (`_Test` DB name requirement) |


## SQL Server configuration

Set these as environment variables or through a secret store (do not commit them):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__DefaultConnection = "Server=YOUR_SQL_SERVER;Database=FanHubPlus;Trusted_Connection=True;TrustServerCertificate=False;MultipleActiveResultSets=true;Encrypt=True"
$env:Database__StartupMode = "None"             # production: apply reviewed migrations during deployment pipeline
$env:Identity__RequireConfirmedEmail = "true"
$env:FileUploads__MaxImageBytes = "5242880"
```

Configure SMTP using the existing `Smtp` settings through environment variables/user-secrets.

### Local development

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=FanHubPlus;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$env:Database__StartupMode = "ApplyMigrations"
$env:Database__SeedDemoData = "true"
dotnet run
```

To create local accounts, explicitly set `Database__SeedDemoAccounts=true` and provide `Database__SeedAdminEmail`, `Database__SeedAdminPassword`, `Database__SeedUserEmail`, and `Database__SeedUserPassword`. There are no committed default passwords.

## Actual verification

- `dotnet build --no-restore`: passed, 0 warnings / 0 errors.
- `dotnet test tests/FanHubPlus.Tests/FanHubPlus.Tests.csproj`: 21 passed, 0 failed, 0 skipped.
- Integration tests verify SQL Server schema creation, role creation, and user management on a disposable database (`FANHUBPLUS_TEST_SQLSERVER`).
- Live HTTP smoke test executed across all primary endpoints with HTTP 200 OK:
  - `GET /` (Home)
  - `GET /Explore` (Explore catalog & category filters)
  - `GET /News` (Articles & news feeds)
  - `GET /Events` (Conventions & gatherings)
  - `GET /Characters` (Lore & character roster)
  - `GET /Merch` (Merchandise catalog)
  - `GET /FanArt` (Community gallery & submissions)
  - `GET /Support` (FAQ & help desk)
  - `GET /Account/Login` (Identity authentication)
  - `GET /Sitemap` (SEO sitemap)
- Live database verification confirmed all tables and migration records are intact:
  - `Categories`: 8 rows
  - `Contents`: 14 rows
  - `Articles`: 6 rows
  - `Events`: 7 rows
  - `MerchandiseItems`: 8 rows
  - `CharacterProfiles`: 8 rows
  - Migration history: `20260924095153_InitialCreate`, `20260924153243_AddFanSubmissionImageUrl`.

## Safe migration and rollback procedure

1. Inventory the production database before changing it: tables, columns, keys, row counts, and users. Do not run `EnsureDeleted`, `Drop`, or a fresh initial migration against it.
2. Take a full database backup using SQLCMD or SSMS before applying changes:
   ```powershell
   sqlcmd -S localhost\SQLEXPRESS -E -C -b -Q "BACKUP DATABASE [FanHubPlus] TO DISK=N'F:\tools\fanhubplus-backup\sqlserver\FanHubPlus-before-migration.bak' WITH INIT, STATS=5;"
   ```
3. Test the migration script against a disposable database ending in `_Test`:
   ```powershell
   $env:FANHUBPLUS_TEST_SQLSERVER = "Server=localhost\SQLEXPRESS;Database=FanHubPlus_Identity_Test;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   dotnet test tests\FanHubPlus.Tests\FanHubPlus.Tests.csproj --filter "FullyQualifiedName~SqlServerIdentityIntegrationTests"
   ```
4. Generate and review an idempotent forward SQL script before execution:
   ```powershell
   dotnet ef migrations script --idempotent --output F:\tools\fanhubplus-backup\sqlserver\fanhubplus-forward.sql
   dotnet ef database update
   ```
5. Deploy and run staging registration/login/role checks.
6. Rollback: stop writes, then restore the pre-migration `.bak` into the database if the forward migration is incompatible. Do not automatically run EF `Down` after new application writes exist.

## Next steps

- Proceed with the luxury UI/UX redesign across Razor views, shared layout, navigation, and CSS/styling.
- Validate visual polish across responsive viewports.

## UI/UX luxury pass (2026-09-24)

- `wwwroot/css/site.css`: every hard-coded colour moved to design tokens; added the **Ivory** light theme (dark stays the default); fixed invisible select carets, switch knobs and close buttons on dark surfaces; refined badges, alerts, empty/error states, footer and admin chrome; WCAG AA contrast checked for both themes.
- `wwwroot/js/site.js`: the theme toggle now really switches themes (previously the `dark-mode` class had no CSS); the broken-image fallback also catches images that failed before scripts ran; password show/hide sits inside the field; quiet scroll-reveal (respects reduced motion).
- Layouts: skip link, theme toggle button, editorial footer, SVG icons in the admin sidebar, no-JS loader guard.
- Views: decorative emoji removed everywhere; empty states and error pages restyled; Explore cards are real grid children.
- Bug fixes: the Events map popup escapes event text (XSS) and no longer crashes `fitBounds` when an event has no coordinates; the undefined `--fhp-surface-2` variable is now defined.
- `ApplicationUser.DarkMode` now defaults to `true` for new accounts (no migration needed; existing users keep their saved choice).

## UI theme re-applied (2026-09-27)

The **FanHubPlus** template (EnvyTheme, https://templates.envytheme.com/misao/index.html) is the visual
foundation again, matching the original hand-in:

| Layer | Location | State |
|---|---|---|
| Template design system | `wwwroot/assets/css/style.css` + `flaticon_fanhubplus.css`, `remixicon.css`, `swiper-bundle.min.css`, `scrollCue.css` | restored |
| Template behaviour | `wwwroot/assets/js/` - GSAP/ScrollTrigger, Swiper, Lenis, lightbox, parallax, scroll cue, `fhp-custom.js` | restored |
| Template media | `wwwroot/assets/images/` (photography, shapes, posters) and `movie.mp4` hero video | restored |
| Project design system | `wwwroot/css/site.css` (`--fhp-*` tokens, buttons, panels, cards, banners, admin shell) and `wwwroot/js/site.js` | re-paired with the template |
| Page furniture | `Views/Shared/_Layout`, `_PageBanner`, `_JoinNow`, `_Pager`, `_Alerts`, `_ChatWidget` and the `_ContentCard` / `_ArticleCard` / `_ArtworkCard` / `_CharacterCard` / `_MerchCard` partials | restored |
| Admin shell | `Areas/Admin/Views/Shared/_Layout.cshtml` | restored |

Only the presentation layer was replaced. Controllers, services, repositories, `Data/DbSeeder*`, migrations,
security middleware and the test suite are unchanged; the three detail pages whose newer view models no
longer carry a category list (`Explore/Details`, `News/Details`, `Merch/Details`) were adapted to the current
contracts instead of changing any controller. Verified after the swap: build 0 warnings/0 errors, 20 tests
pass, and every public, authenticated and admin route returns HTTP 200 with no unhandled exception.

## Theme rebrand pass (2026-09-28)

The imported theme was rebranded to FanHubPlus with a global find-and-replace. No visual change; only
names changed:

| Item | Change |
|---|---|
| Icon font | `assets/css/flaticon_misao.css` -> `flaticon_fanhubplus.css`, `assets/fonts/flaticon_misao14a1.*` -> `flaticon_fanhubplus14a1.*`; the `@font-face` family, all five `url()` targets and the `#...` fragment were renamed, as were the caret rule in `assets/css/style.css`, the `<font id>` inside the SVG font and the `<link>` in `Views/Shared/_Layout.cshtml` |
| Wordmark artwork | `assets/images/logo.svg` and `logo-big.svg` redrawn as FanHubPlus wordmarks in the same viewBoxes/sizes (neither file is referenced by the views - the header and footer render the brand as live text) |
| Text | Footer credit, code comments and documentation: `Misao` -> `FanHubPlus`; also `.misaoOriginalsSwiper` -> `.fanhubplusOriginalsSwiper` in `assets/js/custom.js` (a selector no markup referenced, so behaviour is unchanged) |
| Untouched | All class names, utility strings, colours, type scale, behaviour scripts, media, and the upstream URL quoted above |

Verified after the pass: `dotnet build FanHubPlus.sln` reports 0 warnings / 0 errors; a
case-insensitive search for the old brand across `Views/`, `Areas/`, `wwwroot/`, `Models/`,
`Services/`, `Controllers/`, `Data/`, `Migrations/` and `Properties/` returns 0 hits; every `~/`
asset reference in `Views/Shared/_Layout.cshtml` and all five `url()` targets in
`flaticon_fanhubplus.css` resolve on disk; and a runtime smoke test answers HTTP 200 for `/`, for the
renamed stylesheet, for the renamed `woff2` font and for both logo files (the old stylesheet path
correctly returns 404).

Full record, including the intentional deviations and how to re-run or roll back the migration:
`THEME_MIGRATION.md`.
