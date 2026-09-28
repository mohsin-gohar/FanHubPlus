# Misao -> FanHubPlus theme migration

Record of importing the **Misao** video-streaming template (EnvyTheme,
https://templates.envytheme.com/misao/index.html) into this ASP.NET Core 8 MVC application and
rebranding it to **FanHubPlus** with a global find-and-replace. Written so the whole migration can be
repeated, audited or rolled back from the two source trees.

> Outcome: the running site renders the template's design system unchanged - same stylesheet, same
> class vocabulary, same behaviour scripts, same media - and contains no occurrence of the template's
> brand name. The only naming change is `Misao` -> `FanHubPlus` (see the rename map in section 4).

## 1. What was analysed

### 1.1 Template (`F:\.net\My Web Sites\fanhub\templates.envytheme.com\misao`)

| Item | Detail |
|---|---|
| Pages | 42 hand-written `.html` files (`index.html`, `index-2..5`, `movies*`, `blog-*`, `shop`, `cart`, `checkout`, `pricing`, `faq`, `careers`, `sign-in`, `sign-up`, ...) |
| Stylesheets | `assets/css/style.css` (97 KB - a **compiled Tailwind v4.1.18** build, vendored as `wwwroot/css/fhp-utilities.css`), plus `flaticon_fanhubplus.css`, `remixicon.css`, `swiper-bundle.min.css`, `scrollCue.css`; Bootstrap 5.3.3 CSS added as the base framework |
| Scripts | `swiper-bundle`, `scrollCue`, `parallax`, `fslightbox`, `gsap` + `ScrollTrigger` + `SplitText`, `ukiyo`, `lenis`, and the template's own `custom.js` / `gsap-custom.js` |
| Fonts | `flaticon_misao14a1.*` (icon font, 5 formats), `remixicon6f74.*` |
| Media | 33 image folders (posters, photography, shapes, partner/channel logos) and a 15 MB `movie.mp4` |
| Design tokens | dark theme; primary `#f41b3b`, warm accent `#ff962e`, secondary `#ffea00`; Play + Montserrat + Montez type scale |

Consequence for the port: `style.css` is *compiled* Tailwind (there is no Tailwind source in the
delivery), so every class string written in markup must be copied **verbatim** - a renamed or
re-ordered utility silently loses its rule. The stylesheet was treated as a vendored
artefact and was never edited except for the brand rename.

> **Update (Bootstrap 5.3 migration):** the compiled Tailwind sheet has been copied to
> `wwwroot/css/fhp-utilities.css` (attribution stripped, supplemental classes added); Bootstrap
> 5.3.3 CSS is now loaded as the base framework in `_Layout.cshtml`, with a dedicated
> `wwwroot/css/bootstrap-fhp-theme.css` mapping the FanHubPlus palette to Bootstrap's CSS custom
> properties. The views' utility class strings are unchanged and continue to resolve via
> `fhp-utilities.css`.

### 1.2 Application (`f:\.net\FanHubPlus`)

ASP.NET Core 8 MVC + Razor views, ASP.NET Core Identity, EF Core / SQL Server. The presentation layer
is `Views/**`, `Areas/Admin/Views/**` and `wwwroot/**`; controllers, services, repositories,
migrations and seeding are independent of it, so the theme can be swapped without touching
application logic (see `README.md` -> "UI theme").

## 2. Migration shape (and why)

The template is static HTML with placeholder links (`movies.html`, `cart.html`, ...); this application
is data-driven Razor. The migration therefore keeps the template's **presentation** and re-targets its
**content**:

1. the template's stylesheets, icon fonts, images, video and behaviour scripts are copied in
   unchanged into `wwwroot/assets/`;
2. the template's structural markup is reproduced in the Razor layouts, partials and views with the
   same element nesting and the same class strings - `href="..."` becomes `asp-controller` /
   `asp-action` tag helpers and repeated card blocks become `@foreach` loops over view-model data;
3. the brand name is replaced everywhere (`Misao` -> `FanHubPlus`);
4. the project keeps one bridge stylesheet (`wwwroot/css/site.css`) for what the template has no rule
   for (Bootstrap coexistence, project components, the admin shell), written against the template's
   own palette and type scale.

| Template page family | Where it landed | Notes |
|---|---|---|
| `index.html` (hero + content rows) | `Views/Home/Index.cshtml` | `swiper bannerSwiper` hero on `banners/banner-bg{1..6}.jpg`, rows served from the database |
| `movies`, `movies-tv-shows`, `categories`, `channel-list`, `coverage-*`, `videos` | `Views/Explore/Index.cshtml`, `Views/Explore/Details.cshtml` | catalogue grid, filters, detail layout |
| `movie-details`, `video-details`, `movie-tv-show-details` | `Views/Explore/Details.cshtml` + `Views/Shared/_MediaPlayer.cshtml`, `_VideoModal.cshtml` | player, cast, related rows |
| `blog-grid`, `blog-left-sidebar`, `blog-right-sidebar`, `blog-details` | `Views/Blog/Index.cshtml`, `Views/Blog/Details.cshtml`, `_BlogCard.cshtml`, `_ArticleCard.cshtml` | |
| `faq`, `contact` | `Views/Support/Index.cshtml` (`#accordion` + `fhp-accordion__*`), `Views/Contact/Index.cshtml` | |
| `sign-in`, `sign-up`, `forgot-password`, `my-account` | `Views/Account/{Login,Register,ResetPassword,Profile}.cshtml` | |
| `shop`, `product-details`, `cart`, `checkout` | `Views/Merch/{Index,Details}.cshtml` + `_MerchCard.cshtml` | |
| `error` | `Views/Shared/Error.cshtml` | |
| (no template equivalent) | `Views/{Characters,Events,FanArt,Bookmarks,Chatbot,Sitemap}`, `Areas/Admin/**` | built from the template's own building blocks |

## 3. Step-by-step procedure (as executed)

### Step 0 - baseline

```powershell
cd f:\.net\FanHubPlus
dotnet build FanHubPlus.sln --nologo        # expect: 0 warnings, 0 errors BEFORE the swap
```

### Step 1 - asset parity check (template vs application)

Normalised content comparison (line endings ignored, so CRLF conversions do not show up as changes):

```powershell
$t='F:\...\misao\assets'; $a='f:\.net\FanHubPlus\wwwroot\assets'
Get-ChildItem $t -Recurse -File | ForEach-Object {
  $rel=$_.FullName.Substring($t.Length); $p=Join-Path $a $rel
  if(!(Test-Path $p)) { "MISSING_IN_APP: $rel" }
  else {
    $x=[IO.File]::ReadAllText($_.FullName) -replace "`r`n","`n"
    $y=[IO.File]::ReadAllText($p)             -replace "`r`n","`n"
    if($x -ne $y) { "REAL_DIFF: $rel" }
  }
}
```

Result: no `MISSING_IN_APP`, exactly one `REAL_DIFF` - `assets/js/gsap-custom.js`, an intentional
project improvement (see the deviations ledger in section 5). Everything else is byte-identical
modulo line endings, so no re-import was necessary.

### Step 2 - rename the icon font (stylesheet + font files)

```powershell
git mv wwwroot/assets/css/flaticon_misao.css wwwroot/assets/css/flaticon_fanhubplus.css
foreach($e in 'eot','svg','ttf','woff','woff2') {
  git mv "wwwroot/assets/fonts/flaticon_misao14a1.$e" "wwwroot/assets/fonts/flaticon_fanhubplus14a1.$e"
}
```

Then update the references, in this order:

1. `assets/css/flaticon_fanhubplus.css` - the `@font-face` family, the five `url()` targets and the
   `#...` SVG fragment, plus the `font-family: ... !important` rule on the `[class^="flaticon-"]`
   icon base class.
2. `assets/css/style.css` - the one compiled-Tailwind rule that names the family
   (`#navbar .navbar-nav .nav-item .nav-link.dropdown-toggle:before`).
3. `assets/fonts/flaticon_fanhubplus14a1.svg` - the internal `<font id="...">` / `font-family="..."`
   attributes.
4. `Views/Shared/_Layout.cshtml` - the `<link rel="stylesheet" href="~/assets/css/flaticon_fanhubplus.css" />`.

Icon **class** names (`flaticon-up-arrow`, `flaticon-close`, ...) do not contain the brand and are
unchanged, so no markup had to be touched by this step.

### Step 3 - global find-and-replace of the brand name

Case-sensitive `Misao` -> `FanHubPlus` across every text file of the presentation layer and the docs
(comments, `aria-label`/`alt` text, the footer credit, README/AUDIT prose), plus the two upper-case
section banners in `wwwroot/css/site.css`:

```
Views/Shared/_Layout.cshtml                    6 occurrences
Views/Shared/_Pager.cshtml                     1
Views/Shared/_PageBanner.cshtml                1
Areas/Admin/Views/Shared/_Layout.cshtml         3
wwwroot/css/site.css                          18 (+2 upper-case banners)
wwwroot/js/site.js                             1
wwwroot/assets/js/custom.js                     1 (+1 DOM selector)
wwwroot/assets/js/fhp-custom.js                 2
README.md / AUDIT_REPORT.md                     3
```

Files are written back with the same encoding (UTF-8, BOM preserved when present) and the same line
endings, so the diffs stay one-line-per-change. The DOM selector
`.misaoOriginalsSwiper` -> `.fanhubplusOriginalsSwiper` in `assets/js/custom.js` is the only
identifier change: no markup references that class in either tree, so behaviour is identical.

### Step 4 - brand artwork

`assets/images/logo.svg` (170x43) and `logo-big.svg` (897x147) are the old name **drawn as vector
paths**, which a text find-and-replace can never reach. They are replaced with FanHubPlus wordmarks in
the same viewBoxes and sizes - play badge in the theme's red/orange gradient, `FanHub` white,
`Plus` `#FFEA00` - so any markup pointing at them keeps its layout. Neither file is referenced by the
Razor views (the header and footer render the brand as live text), so this is asset hygiene rather
than a visible change.

### Step 5 - verification

See section 6. Re-run `dotnet build`, the brand search and the runtime smoke test after any further
theme work.

## 4. Rename map

| Kind | Before | After |
|---|---|---|
| Icon-font stylesheet | `assets/css/flaticon_misao.css` | `assets/css/flaticon_fanhubplus.css` |
| Icon-font files | `assets/fonts/flaticon_misao14a1.{eot,svg,ttf,woff,woff2}` | `assets/fonts/flaticon_fanhubplus14a1.{eot,svg,ttf,woff,woff2}` |
| `@font-face` family, `url()` targets, SVG fragment | `flaticon_misao` | `flaticon_fanhubplus` |
| Compiled Tailwind rule (dropdown caret) | `font-family:flaticon_misao` | `font-family:flaticon_fanhubplus` |
| Swiper selector (no markup used it) | `.misaoOriginalsSwiper` | `.fanhubplusOriginalsSwiper` |
| Wordmark artwork | `logo.svg`, `logo-big.svg` (drawn old name) | redrawn FanHubPlus wordmarks, same viewBoxes |
| Layout/partial/comment/prose text | `Misao` | `FanHubPlus` |
| `site.css` section banners | `MISAO (ENVYTHEME) BRIDGE`, `MISAO "VIDEO DETAILS" DESIGN SYSTEM` | `FANHUBPLUS ...` |
| `<link>` in `Views/Shared/_Layout.cshtml` | `~/assets/css/flaticon_misao.css` | `~/assets/css/flaticon_fanhubplus.css` |

Deliberately **not** renamed:

* the upstream URL in `AUDIT_REPORT.md` (`https://templates.envytheme.com/misao/index.html`) - it is
  a real address; rewriting it would break the link;
* the strings inside the binary icon-font files (`flaticon_fanhubplus14a1.{eot,ttf,woff,woff2}`) -
  editing a font's internal name table corrupts it, and the `@font-face` alias is what the browser
  resolves, so the family name used by CSS is already the new one.

## 5. Deviations ledger

Everything below is intentionally **not** a byte-for-byte copy of the template, and why:

| File / area | Deviation | Reason it is kept |
|---|---|---|
| `assets/js/gsap-custom.js` | Lenis smooth scroll is wrapped in guards: it skips initialisation when the library is absent or the visitor prefers reduced motion, calls `ScrollTrigger.update()` on scroll, and exposes `window.fhpLenis` | `wwwroot/js/site.js` (back-to-top) and `assets/js/fhp-custom.js` (anchor links) call `window.fhpLenis.scrollTo(...)`. Restoring the template's version would silently drop those features. This was the single `REAL_DIFF` found in step 1. |
| `wwwroot/css/site.css` | Project bridge layer: `--fhp-*` tokens, buttons, panels, cards, sidebars, page banners, Bootstrap coexistence rules, admin shell | The template ships no rule for the application's own components and does not coexist with Bootstrap. All of it is written against the template palette, type scale and spacing. |
| `wwwroot/js/site.js`, `assets/js/fhp-custom.js` | Sticky navbar, mobile sidebar, accordions, marquee, hero parallax wiring, anti-forgery helper, error-state fallbacks | Behaviour the Razor app needs; `fhp-custom.js` is the template's `custom.js` adapted (both are loaded - the template's original file is still present as `assets/js/custom.js`). |
| `Views/**`, `Areas/Admin/Views/**` | Markup re-targeted to Razor: `asp-controller`/`asp-action` instead of static `href`s, `@foreach` over view models instead of duplicated cards, server-rendered active states instead of hard-coded `active` classes | The template's placeholder links and hard-coded cards cannot drive a database-backed application. Class strings, nesting and section order follow the template. |
| `assets/images/logo.svg`, `logo-big.svg` | Redrawn (step 4) | Drawn lettering cannot be found-and-replaced. |
| `AUDIT_REPORT.md`, `README.md`, `THEME_MIGRATION.md` | Prose adapted to this project; the upstream URL is retained | Factual attribution/provenance for a third-party template. |

## 6. Verification (observed results)

| Check | Command | Result |
|---|---|---|
| Compile | `dotnet build FanHubPlus.sln --nologo` | Build succeeded - **0 warnings, 0 errors** |
| Brand search | case-insensitive `misao` across `Views/ Areas/ wwwroot/ Models/ Services/ Controllers/ Data/ Migrations/ Properties/` | **0 hits** (whole repo: only the upstream URL in `AUDIT_REPORT.md` and this migration record) |
| Asset references | every `(href\|src)="~/..."` in `Views/Shared/_Layout.cshtml` tested against `wwwroot/` | **0 missing** |
| Icon font | all five `url()` targets in `flaticon_fanhubplus.css` tested against `wwwroot/assets/fonts/` | **0 missing** |
| Artwork | the two SVGs parsed as XML | valid, `viewBox` preserved (`0 0 170 43`, `0 0 897 147`) |

Runtime smoke test (application started on `http://localhost:5199` from an isolated build output so a
running debug session is not disturbed):

| Request | Result |
|---|---|
| `GET /` | `200`, 84 009 chars of HTML; contains `flaticon_fanhubplus.css`; contains `FanHubPlus`; contains **no** occurrence of the old brand |
| `GET /assets/css/flaticon_fanhubplus.css` | `200` |
| `GET /assets/fonts/flaticon_fanhubplus14a1.woff2` | `200` |
| `GET /assets/css/style.css` | `200` |
| `GET /assets/js/fhp-custom.js` | `200` |
| `GET /assets/images/logo.svg`, `/assets/images/logo-big.svg` | `200` |
| `GET /assets/css/flaticon_misao.css` (old path) | `404` - expected, the file was renamed |

## 7. Known limitations

1. **Binary icon-font name table** - the internal family string inside the `.eot`/`.ttf`/`.woff`/`.woff2`
   files cannot be text-edited without corrupting the font. The `@font-face` alias in
   `flaticon_fanhubplus.css` is what the browser resolves, so rendering uses the new family name
   regardless. Re-generating the icon font from its source is the only way to change the internals.
2. **Raster artwork** - `assets/images/**` (posters, banners, `ribbon.png`, `favicon.ico`) is the
   template's stock imagery and is byte-identical to it. Any lettering baked into a photograph or
   poster is untouched; replacing that is a design task, not a rename.
3. **Stale copies inside the repository** still contain the old name and were deliberately left alone:
   the git worktrees under `.kilo/worktrees/` (`soft-rumba`, `east-gojirasaurus`), the nested legacy
   snapshot `FanHubPlus/` (excluded from the build by `FanHubPlus.csproj`), and the Chrome user
   profiles `cpA`-`cpE`. Tidy them with `git worktree remove` / `git worktree prune` plus a folder
   delete when convenient.
4. **Two class vocabularies coexist** - template sections use the compiled-Tailwind utilities,
   project components use `fhp-*` classes from `site.css`. New UI should reuse the template's class
   strings verbatim (they exist in `style.css`) or extend the bridge stylesheet; inventing Tailwind
   utilities that are not in the compiled sheet will silently have no effect.

## 8. Re-running or rolling back

* **Re-run**: the procedure in section 3 is ordered and idempotent - the renames are `git mv` and the
  text edits are literal replaces. Re-copying `assets/` from the template and repeating steps 2-4
  reproduces this state from any starting point.
* **Roll back**: `git restore --staged --worktree .` (or `git checkout -- .`) returns the tree to the
  previous commit; all six renames are recorded by git as `R`, so history and blame survive, and the
  template folder is untouched on disk.
* **Swapping the theme again**: only `Views/`, `Areas/Admin/Views/`, `wwwroot/assets`,
  `wwwroot/css/site.css` and `wwwroot/js/site.js` participate - controllers, services, repositories,
  migrations and tests are independent of the presentation layer.


