# ---------------------------------------------------------------------------
# _t.ps1 — quick harness: proves Convert-Text leaves Razor expressions alone
#           while still converting the static class tokens around them.
# ---------------------------------------------------------------------------
. 'f:\.net\FanHubPlus\_convlib.ps1'
$m = Get-ConvMaps

$cases = @(
    'fhp-chip @(activeCategory is null ? "is-active" : "") w-full mb-[15px]'
    'w-full @(cond) group-hover:scale-105 lg:grid-cols-3'
    'px-[10px] pt-[10px] relative'
    'text-white/60 border-white/10 hover:bg-white/10'
    'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-[25px]'
    'md:mb-[15px] md:text-lg xl:!max-w-[1920px]'
    'fhp-chip@(Model.X is null ? "a" : "b") rounded-[5px]'
)

$fail = 0
foreach ($c in $cases) {
    $full = 'class="' + $c + '"'
    $h = [ref]0
    $out = Convert-Text $full $m.map $m.pfx $h
    $inner = $out.Substring(7, $out.Length - 8)
    $same = ($inner -eq $c)
    if (-not $same) { $fail++ }
    "IN : $c"
    "OUT: $inner"
    if ($same) { '  [unchanged - expected for already-converted input]' } else { '  [converted]' }
    ''
}
"unchanged: $($cases.Count - $fail) / $($cases.Count)"
