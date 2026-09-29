# _convlib.ps1 — shared conversion functions for the Tailwind -> Bootstrap converter
$convRoot = 'f:\.net\FanHubPlus'

function Load-Tsv($path) {
    $h = @{}
    if (Test-Path $path) {
        foreach ($line in [System.IO.File]::ReadAllLines($path)) {
            if (-not $line.Trim() -or $line.StartsWith('#')) { continue }
            $i = $line.IndexOf("`t")
            if ($i -lt 1) { continue }
            $h[$line.Substring(0, $i)] = $line.Substring($i + 1)
        }
    }
    return $h
}

function Get-ConvMaps {
    $map = Load-Tsv "$convRoot\_map.tsv"
    foreach ($f in @('_map_add.ps1', '_map_add2.ps1', '_map_add3.ps1')) {
        if (Test-Path "$convRoot\$f") {
            $extra = & "$convRoot\$f"
            foreach ($k in $extra.Keys) { $map[$k] = $extra[$k] }
        }
    }
    return @{ map = $map; pfx = Load-Tsv "$convRoot\_mappfx.tsv" }
}

function Split-Prefix([string]$t) {
    $acc = @()
    while ($t -match '^(2xl|xl|lg|md|sm|hover|focus|group-hover|last|first|rtl|ltr):(.+)$') {
        $acc += $Matches[1]
        $t = $Matches[2]
    }
    return @{ Pre = $acc; Core = [string]$t }
}

function Convert-Token([string]$t, $map, $pfx) {
    if (-not $t) { return $t }
    if ($t -match '[@?*\\$]') { return $t }
    if ($t -match '::') { return $t }
    if ($map.ContainsKey($t)) { return $map[$t] }

    $sp   = Split-Prefix $t
    $pre  = $sp.Pre
    $core = $sp.Core
    $core_orig = $core

    # ---- exact lookup first (covers "sm:", "md:mb-0", ...) ----
    if ($pfx.ContainsKey($t)) { return $pfx[$t] }

    # ---- arbitrary-value core: mb-[15px] -> fhp-mb-15 ----
    $ac = Convert-Exact $core $map
    if ($null -ne $ac) { $core = $ac }

    if ($pre.Count -eq 0) { return $core }

    # outer prefixes (hover/last/rtl/...) separated from a breakpoint
    $outer = @()
    $bp = $null
    foreach ($p in $pre) {
        if ($p -in @('sm','md','lg','xl','2xl') -and $null -eq $bp) { $bp = $p } else { $outer += $p }
    }
    $lead = if ($outer.Count) { ($outer -join ':') + ':' } else { '' }

    # ---- breakpoint-scoped Bootstrap families keep their native form ----
    $bpx = @{ sm='sm'; md='md'; lg='lg'; xl='xl'; '2xl'='xxl' }
    if ($bp -and $bpx.ContainsKey($bp) -and $outer.Count -eq 0) {
        $b = $bpx[$bp]
        if ($core -like 'd-*' -or $core -like 'align-items-*' -or $core -like 'justify-content-*' -or
            $core -like 'align-self-*' -or $core -like 'rounded-*' -or $core -like 'shadow*' -or
            $core -like 'overflow-*' -or $core -like 'gap-*' -or $core -like 'order-*' -or
            $core -like 'opacity-*' -or $core -eq 'text-start' -or $core -eq 'text-center' -or
            $core -eq 'text-end' -or $core -eq 'text-truncate' -or $core -eq 'bg-transparent' -or
            $core -match '^[mp][tbsxy]?-' -or $core -match '^[wh]-') {
            return "$b-$core"
        }
        if ($core.StartsWith('fhp-')) {
            # fhp-py-80 at md -> fhp-md-py-80
            return 'fhp-' + $b + '-' + $core.Substring(4)
        }
    }

    # ---- state prefixes: hover:text-secondary -> fhp-hover-text-secondary ----
    # Idempotent: after one pass the markup already holds fhp-hover-* tokens, so
    # a re-run must normalise them instead of double-wrapping.
    $clean = $t
    $lead  = ''
    $hasState = $false
    foreach ($p in @('group-hover', 'hover', 'focus', 'last', 'first', 'rtl', 'ltr')) {
        $pat = '(?:^|:)' + [regex]::Escape($p) + ':'
        if ($clean -match $pat) { $hasState = $true; $pState = $p; break }
    }
    if ($hasState) {
        $pState = $null
        foreach ($p in @('group-hover', 'hover', 'focus', 'last', 'first', 'rtl', 'ltr')) {
            if ($clean -match '(?:^|:)' + [regex]::Escape($p) + ':') { $pState = $p; break }
        }
        # unwrap the existing layers down to the bare Tailwind core
        $inner = $clean
        for ($i = 0; $i -lt 6; $i++) {
            $before = $inner
            $inner = $inner -replace '(?:^|:)fhp-(?:group-hover|hover|focus|last|first|rtl|ltr)-', ':'
            $inner = $inner -replace '^(?:group-hover|hover|focus|last|first|rtl|ltr):', ''
            if ($inner -eq $before) { break }
        }
        $inner = $inner -replace '(?:^|:)fhp-', ':'
        $inner = $inner -replace '^(?:group-hover|hover|focus|last|first|rtl|ltr):', ''
        $inner = $inner -replace '^(?:group-hover|hover|focus|last|first|rtl|ltr)-', ''
        $inner = $inner.TrimStart(':')
        if (-not $inner) { return $t }
        $conv = Convert-Exact $inner $map
        if ($null -eq $conv) { $conv = $inner }
        if ($conv.StartsWith('fhp-')) { $conv = $conv.Substring(4) }
        if ($conv.StartsWith($pState + '-')) { $conv = $conv.Substring($pState.Length + 1) }
        if (-not $conv) { return $t }
        return 'fhp-' + $pState + '-' + $conv
    }

    if ($bp -and $core.StartsWith('fhp-')) { return 'fhp-' + $bpx[$bp] + '-' + $core.Substring(4) }
    if ($core -ne $core_orig) { return $lead + $core }
    return $t
}

# --- converts a single (prefix-free) class when it is map-able ---
function Convert-Exact([string]$core, $map) {
    if ($map.ContainsKey($core)) { return $map[$core] }
    $a = Convert-Arbitrary $core
    if ($null -ne $a) { return $a }
    return $null
}

# --- generic Tailwind arbitrary value -> deterministic fhp-* name ---
function Convert-Arbitrary([string]$c) {
    if ($null -eq $c) { return $null }
    $c = $c -replace '^!', ''            # "!px-[100px]" -> rely on cascade order
    if ($c -notmatch '\[' -and $c -notmatch '/') { return $null }

    # px value: 80px -> 80 ; "-10px" -> "n10"
    $px = '(?<n>-?\d+(?:\.\d+)?)px'
    if ($c -match '^([mp])([tbsxy]?)-\[' + $px + '\]$') {
        $s = if ($Matches.n.StartsWith('-')) { 'n' + $Matches.n.Substring(1) } else { $Matches.n }
        return "fhp-$($Matches[1])$($Matches[2])-$s"
    }
    if ($c -match '^gap-\[' + $px + '\]$')            { return 'fhp-gap-' + $Matches.n }
    if ($c -match '^w-\[' + $px + '\]$')              { return 'fhp-w-'   + $Matches.n }
    if ($c -match '^h-\[' + $px + '\]$')              { return 'fhp-h-'   + $Matches.n }
    if ($c -match '^min-w-\[' + $px + '\]$')          { return 'fhp-minw-' + $Matches.n }
    if ($c -match '^min-h-\[' + $px + '\]$')          { return 'fhp-minh-' + $Matches.n }
    if ($c -match '^max-w-\[' + $px + '\]$')          { return 'fhp-maxw-' + $Matches.n }
    if ($c -match '^max-h-\[' + $px + '\]$')          { return 'fhp-maxh-' + $Matches.n }
    # percentage sizes: h-[50%] -> fhp-h-50pct
    if ($c -match '^(?<p>w|h|min-w|min-h|max-w|max-h)-\[(?<v>-?\d+(?:\.\d+)?%)\]$') {
        return 'fhp-' + $Matches.p + '-' + $Matches.v.TrimEnd('%') + 'pct'
    }
    # negative translate: -translate-y-1/2 -> fhp-ty-n50
    if ($c -match '^-translate-(?<a>[xy])-(?<n>\d+)/(?<d>\d+)$') {
        $pct = [int](([double]$Matches.n / [double]$Matches.d) * 100)
        return 'fhp-t' + $Matches.a + '-n' + $pct
    }
    if ($c -match '^-translate-(?<a>[xy])-(?<v>.+)$') {
        return 'fhp-t' + $Matches.a + '-n' + (Convert-Slug $Matches.v)
    }
    # negative arbitrary (z/opacity/...)
    if ($c -match '^-?(?<p>[a-z-]+)-\[(?<v>[^\]]+)\]$' -and $c.StartsWith('-')) {
        return 'fhp-' + (Convert-Slug $Matches.p) + '-n' + (Convert-Slug $Matches.v)
    }
    # negative non-arbitrary: -tracking-[0.05em]
    if ($c -match '^-(?<p>tracking|leading|rotate|scale|top|left|right|bottom|mt|mb|ml|mr)-(?<v>.+)$') {
        $inner = if ($Matches.v.StartsWith('[') -and $Matches.v.EndsWith(']')) {
            $Matches.v.Substring(1, $Matches.v.Length - 2)
        } else { $Matches.v }
        return 'fhp-n' + (Convert-Slug $Matches.p) + '-' + (Convert-Slug $inner)
    }
    # corner radii: rounded-b-[5px]
    if ($c -match '^rounded-(?<s>[tb][lr]?)-\[' + $px + '\]$') {
        return 'fhp-r' + $Matches.s + '-' + $Matches.n
    }
    # background image url
    if ($c -match "^bg-\[url\((?<u>.+)\)\]$") { return 'fhp-bg-url' }
    if ($c -match '^text-\[' + $px + '\]$') { return 'fhp-fs-' + $Matches.n }
    if ($c -match '^text-\[(?<v>\d+(?:\.\d+)?)px\]$') { return 'fhp-fs-'   + $Matches.v }
    if ($c -match '^border-\[(?<v>\d+(?:\.\d+)?)px\]$') { return 'fhp-bw-' + $Matches.v }
    if ($c -match '^rounded-\[(?<v>\d+(?:\.\d+)?)px\]$') { return 'fhp-r-' + $Matches.v }
    if ($c -match '^(?:min-)?(?:max-)?grid-cols-\[(?<v>[^\]]+)\]$') { return 'fhp-gcol-' + (Convert-Slug $Matches.v) }
    if ($c -match '^grid-cols-\[(?<v>[^\]]+)\]$')      { return 'fhp-gcol-' + (Convert-Slug $Matches.v) }
    if ($c -match '^(?<p>tracking|leading|basis|translate-x|translate-y|rotate|scale|scale-x|scale-y|top|left|right|bottom|inset-x|inset-y|z|opacity|duration|delay|font-size|line-height|backdrop-blur|blur|ring|ring-offset|divide|space-x|space-y|scroll-m|scroll-p|aspect|object-position|text-indent|outline-offset)-\[(?<v>[^\]]+)\]$') {
        return 'fhp-' + (Convert-Slug $Matches.p) + '-' + (Convert-Slug $Matches.v)
    }
    # arbitrary hex colours: bg-[#18181f]
    if ($c -match '^(?<p>bg|text|border|from|via|to|fill|stroke|ring|outline|decoration)-\[#(?<v>[0-9a-fA-F]+)\]$') {
        return 'fhp-' + $Matches.p + '-' + $Matches.v.ToLower()
    }
    # opacity colours: text-white/60, border-white/10, bg-primary/20
    if ($c -match '^(?<p>text|bg|border|from|ring|divide|placeholder|decoration)-(?<col>[a-z]+|\#[0-9a-fA-F]+)/(?<a>\d+)$') {
        return 'fhp-' + $Matches.p + '-' + (Convert-Slug $Matches.col) + '-' + $Matches.a
    }
    # col-span with arbitrary value
    if ($c -match '^col-(?<s>\d+)\[') { return $null }
    return $null
}

function Convert-Slug([string]$s) {
    if ($null -eq $s) { return '' }
    $s = $s -replace '^!', ''
    $s = $s -replace '#', ''
    $s = $s -replace '[\[\]\(\)\./\s]+', '-'
    $s = $s -replace '-{2,}', '-'
    return $s.Trim('-')
}

function Get-Files {
    $root = 'f:\.net\FanHubPlus'
    $f = @()
    $f += (Get-ChildItem "$root\Views" -Recurse -Filter *.cshtml).FullName
    $f += (Get-ChildItem "$root\Areas" -Recurse -Filter *.cshtml).FullName
    return $f
}

function Convert-Text([string]$text, $map, $pfx, [ref]$hits) {
    $rx = [regex]'(?<pre>class\s*=\s*)(?<q>["''])(?<body>(?:(?!\k<q>).)*)\k<q>'
    $sb = New-Object System.Text.StringBuilder
    $last = 0
    foreach ($m in $rx.Matches($text)) {
        [void]$sb.Append($text.Substring($last, $m.Index - $last))
        $body = $m.Groups['body'].Value
        $q = $m.Groups['q'].Value
        $joined = Split-Razor $body $map $pfx $hits
        [void]$sb.Append($m.Groups['pre'].Value)
        [void]$sb.Append($q + ($joined.Replace($q, '&quot;')) + $q)
        $last = $m.Index + $m.Length
    }
    [void]$sb.Append($text.Substring($last))
    return $sb.ToString()
}

# Walk a class attribute, copying Razor expressions through verbatim and
# converting only the static class tokens in between.  A Razor expression is
# @( ... ) with balanced parentheses, or a bare @identifier, and may contain
# string literals - so this tracks depth and quote state rather than regexing.
function Split-Razor([string]$body, $map, $pfx, [ref]$hits) {
    $out = New-Object System.Text.StringBuilder
    $i = 0
    $segStart = 0
    $n = $body.Length
    while ($i -lt $n) {
        $c = $body[$i]
        if ($c -ne '@') { $i++; continue }

        # @@ is a literal escaped at-sign -> emit with the static part
        if (($i + 1) -lt $n -and $body[$i + 1] -eq '@') {
            [void]$out.Append((Convert-ClassList $body.Substring($segStart, $i - $segStart) $map $pfx $hits))
            [void]$out.Append('@@')
            $i += 2
            $segStart = $i
            continue
        }

        # @( ... )  -> copy the whole balanced expression verbatim
        if (($i + 1) -lt $n -and $body[$i + 1] -eq '(') {
            $end = Find-RazorEnd $body ($i + 1)
            [void]$out.Append((Convert-ClassList $body.Substring($segStart, $i - $segStart) $map $pfx $hits))
            [void]$out.Append($body.Substring($i, $end - $i + 1))
            $i = $end + 1
            $segStart = $i
            continue
        }

        # @identifier -> implicit expression, copy it verbatim
        $j = $i + 1
        while ($j -lt $n -and ($body[$j] -match '[\w\.]')) { $j++ }
        if ($j -eq $i + 1) { $i++; continue }   # lone @ (e.g. an email in text)
        [void]$out.Append((Convert-ClassList $body.Substring($segStart, $i - $segStart) $map $pfx $hits))
        [void]$out.Append($body.Substring($i, $j - $i))
        $i = $j
        $segStart = $i
    }
    [void]$out.Append((Convert-ClassList $body.Substring($segStart) $map $pfx $hits))
    return $out.ToString()
}

# Index of the ')' that closes the '(' at $open, skipping nested parens,
# string literals and character literals.
function Find-RazorEnd([string]$s, [int]$open) {
    $depth = 0
    $n = $s.Length
    $i = $open
    while ($i -lt $n) {
        $ch = $s[$i]
        if ($ch -eq '"' -or $ch -eq "'") {
            $q = $ch
            $i++
            while ($i -lt $n) {
                if ($s[$i] -eq '\\') { $i += 2; continue }
                if ($s[$i] -eq $q) { $i++; break }
                $i++
            }
            continue
        }
        if ($ch -eq '(') { $depth++ }
        elseif ($ch -eq ')') {
            $depth--
            if ($depth -eq 0) { return $i }
        }
        $i++
    }
    return $n - 1
}

# Convert the static class tokens of one attribute segment.  Leading/trailing
# whitespace is preserved so tokens around a Razor expression keep their
# separation (e.g. "fhp-chip @(x) w-full").
function Convert-ClassList([string]$seg, $map, $pfx, [ref]$hits) {
    if (-not $seg) { return '' }
    $lead = ''
    $trail = ''
    if ($seg -match '^\s*') { $lead = $Matches[0] }
    if ($seg -match '\s*$') { $trail = $Matches[0] }
    $core = $seg.Substring($lead.Length, $seg.Length - $lead.Length - $trail.Length)
    if (-not $core) { return $seg }
    $toks = foreach ($t in ($core -split '\s+')) {
        if (-not $t) { continue }
        $new = Convert-Token $t $map $pfx
        if ($new -ne $t) { $hits.Value++ }
        $new
    }
    return $lead + ($toks -join ' ') + $trail
}
