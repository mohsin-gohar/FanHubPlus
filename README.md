# FanHubPlus

FanHubPlus is an ASP.NET Core 8 MVC fandom community portal: a browsable catalogue
(series, movies, games, manga, cosplay...), news articles, events, character profiles, a
fan-art gallery with moderated submissions, a merchandise shop, a support/help desk,
bookmarks and a chatbot assistant - plus a role-protected admin area for all of it.

* **Framework**: ASP.NET Core 8 MVC (Razor views, Areas, dependency injection)
* **CSS**: Bootstrap 5.3 (base framework + dark theme) with the FanHubPlus utility sheet
* **Database**: Microsoft SQL Server via EF Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`, code-first migrations)
* **Identity**: ASP.NET Core Identity (hashed passwords, e-mail confirmation, lockout, roles `Admin` / `User`)
* **Architecture**: Controller -> Service -> generic Repository (`IRepository<>` / `Repository<>`) -> EF Core
* **Security**: global anti-forgery validation, per-IP rate limiting, security headers, hardened auth cookie, magic-byte validated uploads, opt-in seeding only

## Repository layout

| Path | Purpose |
|---|---|
| `FanHubPlus.sln` | Solution file - open it in Visual Studio/Rider, or drive it with the .NET CLI |
| `FanHubPlus.csproj` | Web application project (repository root) |
| `Areas/Admin/` | Admin area, `[Area("Admin")]` + `[Authorize(Roles = "Admin")]` |
| `Controllers/`, `Views/` | Public MVC controllers and Razor views |
| `Data/` | `ApplicationDbContext`, design-time factory, seeder (`DbSeeder`, `DbSeederDemoData`) |
| `Migrations/` | EF Core migrations (`InitialCreate`, `AddFanSubmissionImageUrl`) |
| `Models/` | Entities, enums, view models |
| `Repositories/`, `Services/` | Data access and application services |
| `wwwroot/` | Static assets: the FanHubPlus theme (`assets/`), `css/site.css`, `js/site.js`, Bootstrap + jQuery libraries, runtime uploads |
| `tests/FanHubPlus.Tests/` | xUnit test project (unit, security and SQL Server integration tests) |
| `AUDIT_REPORT.md` | Audit trail and deployment runbook for this submission |

## Prerequisites

* .NET SDK 8.0 (`dotnet --version` reports `8.0.x`)
* SQL Server 2019+ / SQL Server Express / LocalDB
* Optional, only to run migrations from the CLI:
  `dotnet tool install --global dotnet-ef --version 8.0.11`

## Build and test

```powershell
dotnet build FanHubPlus.sln          # 0 warnings, 0 errors
dotnet test  FanHubPlus.sln          # database-backed tests skip automatically
```

Build output (`bin/`, `obj/`) is ignored by `.gitignore` and is never part of the hand-in archive.

## UI theme

The interface is the **FanHubPlus** template (EnvyTheme) - the same look as the original hand-in:

* `wwwroot/lib/bootstrap/` - Bootstrap 5.3.3 CSS + JS bundle (base framework)
* `wwwroot/css/bootstrap-fhp-theme.css` - Bootstrap 5.3 theme maps the FanHubPlus palette to CSS custom properties
* `wwwroot/css/fhp-utilities.css` - utility classes used by the Razor views (replaces the previous compiled Tailwind sheet)
* `wwwroot/css/{site,fanhubplus-app}.css` - the FanHubPlus component + widget layer (dark mode, cards, chat widget, modals, admin shell)
* `wwwroot/assets/css/{flaticon_fanhubplus,remixicon,swiper-bundle.min,scrollCue}.css` - icon fonts and slider styles
* `wwwroot/assets/js/` - GSAP + ScrollTrigger, Swiper, Lenis smooth scroll, lightbox, parallax, scroll cue and
  `fhp-custom.js` (template wiring adapted to this app)
* `wwwroot/assets/images/` - photography, shapes, posters and the `movie.mp4` hero video
* `wwwroot/css/site.css` - project stylesheet layered on top: `--fhp-*` design tokens, buttons, panels, cards,
  sidebars, page banners and the admin shell
* `wwwroot/js/site.js` - theme/loader helpers, the anti-forgery token helper used by the chatbot and bookmark
  AJAX calls, and shared component behaviour
* `Views/Shared/` - `_Layout`, `_PageBanner`, `_JoinNow`, `_Pager`, `_Alerts`, `_ChatWidget` and the card
  partials (`_ContentCard`, `_ArticleCard`, `_ArtworkCard`, `_CharacterCard`, `_MerchCard`)

Swapping the theme only touches `Views/`, `Areas/Admin/Views/`, `wwwroot/assets`, `wwwroot/css/site.css` and
`wwwroot/js/site.js`; controllers, services, repositories, migrations, seeding and tests are independent of it.

### Theme provenance

The design system under `wwwroot/assets/` is the EnvyTheme streaming/OTT template whose site is
recorded in `AUDIT_REPORT.md` -> "UI theme re-applied". It was imported unchanged and then rebranded
to FanHubPlus: the icon stylesheet and its font files were renamed to `flaticon_fanhubplus*`, the
composed wordmark artwork (`logo.svg`, `logo-big.svg`) was redrawn, and every occurrence of the
template's brand name in markup, scripts, styles and comments was replaced.

Class names, utility class strings, colours, type scale, behaviour scripts and all other media are
untouched, so the rendering matches the template exactly. `THEME_MIGRATION.md` records the full
rename map, the template-page-to-view mapping, the intentional deviations and the verification
commands.

## Configuration

Every setting is plain configuration, so it can come from `appsettings.json`, environment
variables (`:` is written `__` on the command line) or user-secrets.

| Key | Environment variable | Notes |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | SQL Server connection string (required) |
| `Database:Provider` | `Database__Provider` | only `SqlServer` is supported |
| `Database:StartupMode` | `Database__StartupMode` | `ApplyMigrations` or `None` (defaults to `None` outside Development) |
| `Database:SeedDemoData` | `Database__SeedDemoData` | demo catalogue: contents, articles, events, merch, characters, tags |
| `Database:SeedDemoAccounts` | `Database__SeedDemoAccounts` | opt-in switch for demo logins (see below) |
| `Identity:RequireConfirmedEmail` | `Identity__RequireConfirmedEmail` | `true` in production |

Local development (`appsettings.Development.json` already carries these defaults):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=FanHubPlus;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$env:Database__StartupMode  = "ApplyMigrations"
$env:Database__SeedDemoData = "true"
dotnet run --project FanHubPlus.csproj
```

**No default credentials are committed by design** - there is no built-in `admin/admin` login
anywhere in the source, configuration or migrations. Creating a demo account is an explicit
opt-in and **both** the e-mail and the password must be supplied by whoever runs the app.

Set all of the following before the first `dotnet run` (PowerShell):

```powershell
# 1) point at your SQL Server instance
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=FanHubPlus;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# 2) create/upgrade the schema on startup and load the demo catalogue
$env:Database__StartupMode  = "ApplyMigrations"
$env:Database__SeedDemoData = "true"

# 3) switch on demo-account seeding and provide the credentials
$env:Database__SeedDemoAccounts  = "true"
$env:Database__SeedAdminEmail    = "admin@fanhubplus.local"
$env:Database__SeedAdminPassword = "Adm1n#Demo2026"      # use any policy-compliant password

# 4) optional: a second, non-admin account
$env:Database__SeedUserEmail     = "fan@fanhubplus.local"
$env:Database__SeedUserPassword  = "Fan#Demo2026"

dotnet run --project FanHubPlus.csproj
```

bash/zsh equivalent:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=FanHubPlus;User Id=sa;Password=<your-sa-password>;TrustServerCertificate=True;MultipleActiveResultSets=true"
export Database__StartupMode=ApplyMigrations
export Database__SeedDemoData=true
export Database__SeedDemoAccounts=true
export Database__SeedAdminEmail="admin@fanhubplus.local"
export Database__SeedAdminPassword="Adm1n#Demo2026"
export Database__SeedUserEmail="fan@fanhubplus.local"
export Database__SeedUserPassword="Fan#Demo2026"
dotnet run --project FanHubPlus.csproj
```

Behaviour worth knowing:

* `Database__SeedAdminEmail` and `Database__SeedAdminPassword` are **both** required; if either is empty the
  seeder only logs `Demo account seeding was requested but ... is missing; account skipped.` and creates nothing.
* The admin account is created with the `Admin` role and is stored as **e-mail confirmed**, so it can sign
  in even when `Identity__RequireConfirmedEmail=true`.
* The password must satisfy the Identity policy: 8+ characters with an uppercase letter, a lowercase
  letter, a digit and a non-alphanumeric character (`Adm1n#Demo2026` satisfies it).
* Seeding is idempotent - an account whose e-mail already exists is skipped, and no existing row is ever
  modified or deleted, so restarts are safe.
* Sign in at `/Account/Login`. The admin area starts at `/Admin/Dashboard` and also covers
  `/Admin/Contents`, `/Admin/Articles`, `/Admin/Categories`, `/Admin/Events`, `/Admin/Characters`,
  `/Admin/Merch`, `/Admin/Submissions`, `/Admin/Feedback` and `/Admin/Users`.
* Verified against a disposable SQL Server database while packaging this submission: with exactly the
  variables above, `admin@fanhubplus.local` is created with role `Admin`, the login POST returns the
  `FanHubPlus.Auth` cookie and `GET /Admin/Dashboard` answers `200`.

### Alternative: user-secrets (Development only)

`FanHubPlus.csproj` carries a `UserSecretsId`, so credentials can live in the user profile instead of the
shell environment:

```powershell
dotnet user-secrets set "Database:SeedDemoAccounts"  "true"                   --project FanHubPlus.csproj
dotnet user-secrets set "Database:SeedAdminEmail"    "admin@fanhubplus.local" --project FanHubPlus.csproj
dotnet user-secrets set "Database:SeedAdminPassword" "Adm1n#Demo2026"         --project FanHubPlus.csproj
```

## Migrations

```powershell
# check for model/migration drift (must report: No changes have been made to the model since the last migration.)
dotnet ef migrations has-pending-model-changes --project FanHubPlus.csproj

dotnet ef migrations list --project FanHubPlus.csproj
dotnet ef migrations script --idempotent --output fanhubplus-forward.sql --project FanHubPlus.csproj
dotnet ef database update --project FanHubPlus.csproj
```

`Database__StartupMode=None` (the default outside Development) keeps production schema changes in the
deployment pipeline instead of application startup; see `AUDIT_REPORT.md` for the backup/rollback runbook.

## Tests

```powershell
dotnet test FanHubPlus.sln
```

20 unit/security/contract tests run and pass out of the box; the SQL Server integration test skips itself
unless a disposable database is supplied:

```powershell
$env:FANHUBPLUS_TEST_SQLSERVER = "Server=localhost\SQLEXPRESS;Database=FanHubPlus_Identity_Test;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet test tests\FanHubPlus.Tests\FanHubPlus.Tests.csproj --filter "FullyQualifiedName~SqlServerIdentityIntegrationTests"
```

The integration test deliberately refuses to touch a database whose name does not end in `_Test`.

## Submission packaging

* `.gitignore` excludes build output (`bin/`, `obj/`), archives (`*.zip`), local secrets/overrides and
  runtime uploads, so build artifacts and packaged copies of the project can never be committed or zipped again.
* The hand-in archive contains only the project folder `FanHubPlus/` (which includes `tests/`): no `bin/`,
  no `obj/`, no nested/duplicate copies of the project, no `.bak` UI snapshots and no `.zip` files.
