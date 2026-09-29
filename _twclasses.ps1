$ErrorActionPreference = 'Stop'
$root = 'f:\.net\FanHubPlus'
$defined = [System.Collections.Generic.HashSet[string]]::new()
foreach ($f in @(
    "$root\wwwroot\css\bootstrap-fhp-theme.css",
    "$root\wwwroot\css\site.css",
    "$root\wwwroot\css\app.css",
    "$root\Areas\Admin\Views\Shared\_Layout.cshtml"
)) {
    if (Test-Path $f) { [regex]::Matches((Get-Content $f -Raw), '(\.|-\.)([A-Za-z][\w-]*)') | ForEach-Object { [void]$defined.Add($_.Groups[2].Value) } }
}
$skip = @('tailwind.css', 'fhp-utilities.css', 'tw.css', 'tw.check.css', 'tailwind.src.css')
foreach ($f in (Get-ChildItem "$root\wwwroot\css" -Filter *.css -Recurse -EA SilentlyContinue)) {
    if ($skip -contains $f.Name) { continue }
    [regex]::Matches((Get-Content $f.FullName -Raw), '(\.|-\.)([A-Za-z][\w-]*)') | ForEach-Object { [void]$defined.Add($_.Groups[2].Value) }
}
$bm = Get-ChildItem "$root\wwwroot\lib\bootstrap" -Recurse -Filter bootstrap*.css -EA SilentlyContinue | Sort-Object Length -Descending
foreach ($f in $bm) {
    [regex]::Matches((Get-Content $f.FullName -Raw), '(\.|-\.)([A-Za-z][\w-]*)') | ForEach-Object { [void]$defined.Add($_.Groups[2].Value) }
}

$scan = @(
    "$root\Areas\Admin\Views\Shared\_Layout.cshtml",
    "$root\Views\Shared\_Layout.cshtml"
)
$scan += (Get-ChildItem "$root\Views" -Recurse -Filter *.cshtml).FullName
$scan += (Get-ChildItem "$root\Areas" -Recurse -Filter *.cshtml).FullName
$scan = $scan | Sort-Object -Unique

$counts = [System.Collections.Generic.Dictionary[string,int]]::new()
foreach ($f in $scan) {
    $m = [regex]::Matches((Get-Content $f -Raw), 'class="([^"]*)"')
    foreach ($g in $m) {
        foreach ($t in ($g.Groups[1].Value -split '\s+')) {
            if (-not $t) { continue }
            if ($t -match '[@?()*\\$]') { continue }
            if ($t -match '^https?:' -or $t -match '::') { continue }
            $tok = ($t -replace '^(sm|md|lg|xl|2xl):', '')
            if ($defined.Contains($tok)) { continue }
            if ($counts.ContainsKey($t)) { $counts[$t]++ } else { $counts[$t] = 1 }
        }
    }
}
$out = $counts.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { "{0}`t{1}" -f $_.Value, $_.Key }
Set-Content "$root\_twclasses.txt" $out -Encoding UTF8
"distinct missing: " + $counts.Count
"total occurrences: " + (($counts.Values | Measure-Object -Sum).Sum)
