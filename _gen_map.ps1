# _gen_map.ps1 — append value-accurate spacing/gap mappings to _map.tsv
$ErrorActionPreference = 'Stop'
$root = 'f:\.net\FanHubPlus'
$path = "$root\_map.tsv"

# Tailwind spacing (rem) -> px
$tw = @{
    '0.5' = 2; '1' = 4; '1.5' = 6; '2' = 8; '2.5' = 10; '3' = 12; '3.5' = 14
    '4' = 16; '5' = 20; '6' = 24; '7' = 28; '8' = 32; '9' = 36; '10' = 40
    '11' = 44; '12' = 48; '14' = 56; '16' = 64; '20' = 80; '24' = 96
    '28' = 112; '32' = 128; '36' = 144; '40' = 160; '48' = 192; '56' = 224; '64' = 256
}
# Bootstrap spacing (rem) -> px
$bs = @{ '0' = 0; '1' = 4; '2' = 8; '3' = 16; '4' = 24; '5' = 48 }
$px2bs = @{}
foreach ($k in $bs.Keys) { $px2bs[$bs[$k]] = $k }

$props = @('m', 'p')
$suf   = @('', 't', 'b', 'l', 'r', 'x', 'y', 's', 'e')
$lines = New-Object System.Collections.Generic.List[string]

foreach ($k in ($tw.Keys | Sort-Object { [double]$_ })) {
    $px = $tw[$k]
    $target = if ($px2bs.ContainsKey($px)) { $px2bs[$px] } else { $null }

    foreach ($pr in $props) {
        foreach ($s in $suf) {
            $old = "$pr$s-$k"
            # never override an explicit entry already in _map.tsv
            $new = if ($null -ne $target) {
                $b = "$pr$s-$target"
                if ($pr -eq 'm') { $b = $b -replace '^ml-', 'ms-' -replace '^mr-', 'me-' }
                $b
            } else {
                "fhp-$pr$s-$([int]$px)"
            }
            $lines.Add("$old`t$new")
        }
    }
    # gap
    $g = if ($null -ne $target) { "gap-$target" } else { "fhp-gap-$([int]$px)" }
    $lines.Add("gap-$k`t$g")
}

$existing = @{}
foreach ($l in [System.IO.File]::ReadAllLines($path)) {
    if ($l.StartsWith('#') -or -not $l.Trim()) { continue }
    $i = $l.IndexOf("`t"); if ($i -lt 1) { continue }
    $existing[$l.Substring(0, $i)] = $true
}
$added = 0
foreach ($l in $lines) {
    $k = $l.Substring(0, $l.IndexOf("`t"))
    if ($existing.ContainsKey($k)) { continue }
    $existing[$k] = $true
    [System.IO.File]::AppendAllText($path, $l + "`r`n", [System.Text.UTF8Encoding]::new($false))
    $added++
}
"appended: $added"
