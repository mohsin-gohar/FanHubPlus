# audit-subpages.ps1 — finds CSS classes used in rendered pages but missing
# from every stylesheet the site loads (bootstrap-fhp-theme, site.css,
# fhp-utilities, fanhubplus-app, bootstrap, swiper, scrollCue, remixicon).
param(
    [string[]]$Pages = @('/', '/Explore', '/News', '/Merch', '/Support', '/Account/Login', '/Account/Register'),
    [string]$Base = 'http://localhost:5124'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$cssFiles = @(
    'F:\.net\FanHubPlus\wwwroot\css\bootstrap-fhp-theme.css',
    'F:\.net\FanHubPlus\wwwroot\css\site.css',
    'F:\.net\FanHubPlus\wwwroot\css\fhp-utilities.css',
    'F:\.net\FanHubPlus\wwwroot\css\fanhubplus-app.css',
    'F:\.net\FanHubPlus\wwwroot\lib\bootstrap\dist\css\bootstrap.min.css',
    'F:\.net\FanHubPlus\wwwroot\assets\css\swiper-bundle.min.css',
    'F:\.net\FanHubPlus\wwwroot\assets\css\scrollCue.css',
    'F:\.net\FanHubPlus\wwwroot\assets\css\remixicon.css',
    'F:\.net\FanHubPlus\wwwroot\assets\css\flaticon_fanhubplus.css'
)
$css = ($cssFiles | ForEach-Object { [System.IO.File]::ReadAllText($_) }) -join "`n"

function Escape-Class([string]$c) {
    # Build the CSS source form of a class: . -> \.  : -> \:  [ -> \[  etc.
    $special = '.:/[]#%!,()@'.ToCharArray()   # RHS must be an ARRAY ('a' -in 'str' is always false)
    $out = ''
    foreach ($ch in $c.ToCharArray()) {
        if ($ch -in $special) { $out += '\' }
        $out += $ch
    }
    $out
}

function Test-HasClass([string]$css, [string]$esc) {
    $needle = '.' + $esc
    $idx = $css.IndexOf($needle)
    while ($idx -ge 0) {
        $end = $idx + $needle.Length
        $after = if ($end -lt $css.Length) { $css[$end] } else { ' ' }
        if ("$after" -notmatch '[A-Za-z0-9_-]') { return $true }
        $idx = $css.IndexOf($needle, $idx + 1)
    }
    return $false
}

$allMissing = @{}
# Classes that are JavaScript hooks / validation states, never styled in CSS.
$jsHooks = 'Swiper(Thumbs)?$', '^feedback-button-', '^fhp-share$', '^(text_animation|accordion-panel|chat-input|chat-suggestions|navbar-burger-toggle|page-loader|loader|backdrop)$', '^field-validation-'
foreach ($p in $Pages) {
    $h = (Invoke-WebRequest -Uri ($Base + $p) -UseBasicParsing -TimeoutSec 30).Content
    $tokens = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($m in [regex]::Matches($h, 'class="([^"]*)"')) {
        foreach ($tok in ($m.Groups[1].Value -split '\s+')) {
            if ($tok -and $tok -match '^[A-Za-z_]') { [void]$tokens.Add($tok) }
        }
    }
    foreach ($tok in ($tokens | Sort-Object)) {
        $skip = $false
        foreach ($pat in $jsHooks) { if ($tok -match $pat) { $skip = $true; break } }
        if ($skip) { continue }
        if (-not (Test-HasClass $css (Escape-Class $tok))) {
            if (-not $allMissing.ContainsKey($tok)) { $allMissing[$tok] = @() }
            if ($allMissing[$tok] -notcontains $p) { $allMissing[$tok] += $p }
        }
    }
}

if ($allMissing.Count -eq 0) { 'AUDIT OK - every class has a CSS rule' }
else {
    "MISSING CLASSES ($($allMissing.Count)):"
    $allMissing.GetEnumerator() | Sort-Object Name | ForEach-Object { "  {0}   <- {1}" -f $_.Key, ($_.Value -join ', ') }
}
