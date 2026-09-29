# ---------------------------------------------------------------------------
# _convert.ps1 — Tailwind -> Bootstrap 5.3 converter (FanHubPlus)
#   dry run : powershell -File _convert.ps1
#   apply   : powershell -File _convert.ps1 -Apply
# ---------------------------------------------------------------------------
param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$root = 'f:\.net\FanHubPlus'
. 'f:\.net\FanHubPlus\_convlib.ps1'

$maps   = Get-ConvMaps
$files  = Get-Files
$hits   = 0
$changed = @()
$script:dummy = 0

foreach ($f in $files) {
    $text = [System.IO.File]::ReadAllText($f)
    $new  = Convert-Text $text $maps.map $maps.pfx ([ref]$hits)
    if ($new -ne $text) {
        $changed += $f
        if ($Apply) {
            $b = [System.IO.File]::ReadAllBytes($f)
            $hasBom = ($b.Length -ge 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191)
            [System.IO.File]::WriteAllText($f, $new, [System.Text.UTF8Encoding]::new($hasBom))
        }
    }
}

# residual report — re-scan whatever is left after this run
$residual = @{}
# --- what is already valid CSS (everything except the Tailwind bundle) ---
$skipCss = @('tailwind.css', 'fhp-utilities.css', 'tw.css', 'tw.check.css', 'tailwind.src.css')
$defined = [System.Collections.Generic.HashSet[string]]::new()
function Add-ClassesFrom($path) {
    if (-not (Test-Path $path)) { return }
    foreach ($m in [regex]::Matches([System.IO.File]::ReadAllText($path), '(?:^|[^\w-])\.(-?[A-Za-z][\w-]*)')) {
        [void]$defined.Add($m.Groups[1].Value)
    }
}
Get-ChildItem "$root\wwwroot\css" -Filter *.css -Recurse -EA SilentlyContinue |
    Where-Object { $skipCss -notcontains $_.Name } | ForEach-Object { Add-ClassesFrom $_.FullName }
Get-ChildItem "$root\wwwroot\assets\css" -Filter *.css -Recurse -EA SilentlyContinue |
    ForEach-Object { Add-ClassesFrom $_.FullName }
Get-ChildItem "$root\wwwroot\lib\bootstrap" -Filter *.css -Recurse -EA SilentlyContinue |
    ForEach-Object { Add-ClassesFrom $_.FullName }
Add-ClassesFrom "$root\Areas\Admin\Views\Shared\_Layout.cshtml"
# inline <style> blocks in views also define real classes
foreach ($f in $files) {
    $t0 = [System.IO.File]::ReadAllText($f)
    foreach ($m in [regex]::Matches($t0, '(?s)<style[^>]*>(.*?)</style>')) {
        foreach ($mm in [regex]::Matches($m.Groups[1].Value, '(?:^|[^\w-])\.(-?[A-Za-z][\w-]*)')) {
            [void]$defined.Add($mm.Groups[1].Value)
        }
    }
}

# --- categorised residual report ---
$attrRx  = [regex]'(class\s*=\s*)("([^"]*)"|''([^'']*)'')'
$buckets = @{
    state = [System.Collections.Generic.Dictionary[string,int]]::new()
    resp  = [System.Collections.Generic.Dictionary[string,int]]::new()
    arb   = [System.Collections.Generic.Dictionary[string,int]]::new()
    other = [System.Collections.Generic.Dictionary[string,int]]::new()
}
function Bump($d, $k) { if ($d.ContainsKey($k)) { $d[$k]++ } else { $d[$k] = 1 } }

foreach ($f in $files) {
    $text = [System.IO.File]::ReadAllText($f)
    foreach ($m in $attrRx.Matches($text)) {
        $body = if ($m.Groups[2].Value.StartsWith('"')) { $m.Groups[3].Value } else { $m.Groups[4].Value }
        foreach ($t in ($body -split '\s+')) {
            if (-not $t) { continue }
            if ($t -match '[?*\\$=<>]|::|\(') { continue }
            if ($t -match '\.') { continue }                 # Razor / code fragments
            # probe conversion of this single token (Convert-Text needs a class attribute)
            $cv = Convert-Text ('class="' + $t + '"') $maps.map $maps.pfx ([ref]$script:dummy)
            $convTok = if ($cv -match '^class="([^"]*)"$') { $Matches[1] } else { $t }
            if ($convTok -ne $t) { continue }                # converted cleanly (incl. grids)
            if ($defined.Contains($t)) { continue }
            $sp = Split-Prefix $t
            if ($sp.Pre.Count -gt 0 -and ($sp.Pre -contains 'hover' -or $sp.Pre -contains 'focus' -or
                $sp.Pre -contains 'group-hover' -or $sp.Pre -contains 'last' -or $sp.Pre -contains 'first')) {
                Bump $buckets.state $t
            } elseif ($t -match '^(sm|md|lg|xl|2xl):') {
                Bump $buckets.resp $t
            } elseif ($t -match '\[' -or $t -match '/' -or $t -match '!') {
                Bump $buckets.arb $t
            } else {
                Bump $buckets.other $t
            }
        }
    }
}
$sb = New-Object System.Text.StringBuilder
foreach ($k in @('state', 'resp', 'arb', 'other')) {
    [void]$sb.AppendLine("=== $k (" + $buckets[$k].Count + " distinct / " +
        (($buckets[$k].Values | Measure-Object -Sum).Sum) + " hits) ===")
    foreach ($e in ($buckets[$k].GetEnumerator() | Sort-Object Value -Descending)) {
        [void]$sb.AppendLine("$($e.Value)`t$($e.Key)")
    }
    [void]$sb.AppendLine('')
}
[System.IO.File]::WriteAllText("$root\_residual.txt", $sb.ToString(), [System.Text.UTF8Encoding]::new($false))

"mode             : " + $(if ($Apply) { 'APPLY' } else { 'DRY-RUN' })
"files changed    : " + $changed.Count
"class edits      : " + $hits
"defined classes  : " + $defined.Count
foreach ($k in @('state', 'resp', 'arb', 'other')) {
    "residual {0,-6}: {1} distinct / {2} hits" -f $k, $buckets[$k].Count,
        (($buckets[$k].Values | Measure-Object -Sum).Sum)
}
