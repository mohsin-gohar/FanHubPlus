$root = 'F:\.net\FanHubPlus'

# --- 1. classes defined in the NON-Tailwind stylesheets (Bootstrap + theme + site + app + assets) ---
$cssFiles = @(
  "$root\wwwroot\lib\bootstrap\dist\css\bootstrap.min.css",
  "$root\wwwroot\css\bootstrap-fhp-theme.css",
  "$root\wwwroot\css\site.css",
  "$root\wwwroot\css\fanhubplus-app.css"
) + (Get-ChildItem "$root\wwwroot\assets\css" -Filter *.css -File -ErrorAction SilentlyContinue).FullName

$defined = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($f in $cssFiles) {
  if (-not (Test-Path $f)) { continue }
  $css = Get-Content $f -Raw
  foreach ($m in [regex]::Matches($css, '\.([A-Za-z_][A-Za-z0-9_-]*)')) {
    [void]$defined.Add($m.Groups[1].Value)
  }
}

# --- 2. classes actually used in the LIVE views (root Views + Areas only) ---
$usage = @{}
$files = @(Get-ChildItem "$root\Views" -Recurse -Filter *.cshtml -File) +
         @(Get-ChildItem "$root\Areas"  -Recurse -Filter *.cshtml -File)

foreach ($f in $files) {
  $raw = Get-Content $f.FullName -Raw
  foreach ($cm in [regex]::Matches($raw, 'class\s*=\s*"([^"]*)"')) {
    $val = $cm.Groups[1].Value
    # drop Razor expressions but keep the literal text around them
    $val = $val -replace '@\([^)]*\)', ' ' -replace '@\{[^}]*\}', ' '
    foreach ($t in ($val -split '\s+')) {
      if ([string]::IsNullOrWhiteSpace($t)) { continue }
      if ($usage.ContainsKey($t)) { $usage[$t]++ } else { $usage[$t] = 1 }
    }
  }
}

# --- 3. which used classes are NOT provided by any non-Tailwind stylesheet ---
$missing = @{}
foreach ($k in $usage.Keys) {
  $base = ($k -split ':')[-1]            # strip responsive/state variant prefix
  $base = ($base -split '/')[-1]         # strip opacity suffix
  $base = $base.TrimStart('!')
  if (-not $defined.Contains($base)) { $missing[$k] = $usage[$k] }
}

"DEFINED_CLASS_NAMES = " + $defined.Count
"USED_CLASS_TOKENS    = " + $usage.Count
"MISSING_CLASS_TOKENS = " + $missing.Count
"MISSING_OCCURRENCES  = " + (($missing.Values | Measure-Object -Sum).Sum)
""
"---- top 80 missing (tailwind-only) classes ----"
$missing.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 80 |
  ForEach-Object { "{0,5}  {1}" -f $_.Value, $_.Key }
""
"---- all missing classes, alphabetical ----"
($missing.Keys | Sort-Object) -join ' '

""
"---- PER-FILE missing (tailwind-only) occurrence counts ----"
$perFile = @()
foreach ($f in $files) {
  $raw = Get-Content $f.FullName -Raw
  $n = 0
  foreach ($cm in [regex]::Matches($raw, 'class\s*=\s*"([^"]*)"')) {
    $val = $cm.Groups[1].Value -replace '@\([^)]*\)', ' ' -replace '@\{[^}]*\}', ' '
    foreach ($t in ($val -split '\s+')) {
      if ([string]::IsNullOrWhiteSpace($t)) { continue }
      if ($t -match '^[@?=&!]|[\(\)]$') { continue }   # skip razor/js artefacts
      $base = (($t -split ':')[-1] -split '/')[-1].TrimStart('!')
      if (-not $defined.Contains($base)) { $n++ }
    }
  }
  $perFile += [PSCustomObject]@{ Missing = $n; File = $f.FullName.Replace("$root\", '') }
}
$perFile | Sort-Object Missing -Descending | Where-Object { $_.Missing -gt 0 } | Format-Table -AutoSize
"FILES_WITH_WORK = " + @($perFile | Where-Object { $_.Missing -gt 0 }).Count
"TOTAL_MISSING_OCCURRENCES = " + (($perFile | Measure-Object Missing -Sum).Sum)
