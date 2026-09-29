# ---------------------------------------------------------------------------
# _map_add.ps1 — extra Tailwind -> Bootstrap / fhp-* mappings (part 1).
# Kept as a PowerShell hashtable so no tab/escaping issues.
# ---------------------------------------------------------------------------
@{
    # ---- type scale (Tailwind rem steps -> px based fhp-fs-*) ----
    'text-lg'  = 'fhp-fs-18'
    'text-xl'  = 'fhp-fs-20'
    'text-2xl' = 'fhp-fs-24'
    'text-3xl' = 'fhp-fs-30'
    'text-6xl' = 'fhp-fs-60'
    'text-7xl' = 'fhp-fs-72'
    'text-9xl' = 'fhp-fs-128'

    # ---- tracking ----
    'tracking-tight'   = 'fhp-ls-tight'
    'tracking-tighter' = 'fhp-ls-tighter'
    'tracking-wide'    = 'fhp-ls-wide'
    'tracking-wider'   = 'fhp-ls-wider'
    'tracking-normal'  = 'fhp-ls-normal'

    # ---- background ----
    'bg-center'    = 'fhp-bg-center'
    'bg-top'       = 'fhp-bg-top'
    'bg-bottom'    = 'fhp-bg-bottom'
    'bg-left'      = 'fhp-bg-left'
    'bg-right'     = 'fhp-bg-right'
    'bg-cover'     = 'fhp-bg-cover'
    'bg-contain'   = 'fhp-bg-contain'
    'bg-no-repeat' = 'fhp-bg-norepeat'
    'bg-fixed'     = 'fhp-bg-fixed'
    'bg-local'     = 'fhp-bg-local'

    # ---- aspect ratio ----
    'aspect-square' = 'fhp-aspect-square'
    'aspect-video'  = 'fhp-aspect-video'
    'aspect-auto'   = 'fhp-aspect-auto'

    # ---- sizing (Tailwind 0.25rem steps -> px) ----
    'min-w-0'    = 'fhp-minw-0'
    'max-w-none' = 'fhp-maxw-none'
    'max-w-2xl'  = 'fhp-maxw-672'
    'max-w-3xl'  = 'fhp-maxw-768'
    'max-w-4xl'  = 'fhp-maxw-896'
    'max-w-5xl'  = 'fhp-maxw-1024'
    'max-w-6xl'  = 'fhp-maxw-1152'
    'max-w-7xl'  = 'fhp-maxw-1280'
    'w-10' = 'fhp-w-40';  'w-12' = 'fhp-w-48';  'w-14' = 'fhp-w-56';  'w-16' = 'fhp-w-64'
    'w-20' = 'fhp-w-80';  'w-24' = 'fhp-w-96';  'w-28' = 'fhp-w-112'; 'w-32' = 'fhp-w-128'
    'w-40' = 'fhp-w-160'; 'w-44' = 'fhp-w-176'; 'w-48' = 'fhp-w-192'; 'w-56' = 'fhp-w-224'
    'w-64' = 'fhp-w-256'; 'w-72' = 'fhp-w-288'; 'w-80' = 'fhp-w-320'; 'w-96' = 'fhp-w-384'
    'h-10' = 'fhp-h-40';  'h-12' = 'fhp-h-48';  'h-14' = 'fhp-h-56';  'h-16' = 'fhp-h-64'
    'h-20' = 'fhp-h-80';  'h-24' = 'fhp-h-96';  'h-28' = 'fhp-h-112'; 'h-32' = 'fhp-h-128'
    'h-40' = 'fhp-h-160'; 'h-44' = 'fhp-h-176'; 'h-48' = 'fhp-h-192'; 'h-56' = 'fhp-h-224'
    'h-64' = 'fhp-h-256'; 'h-72' = 'fhp-h-288'; 'h-80' = 'fhp-h-320'; 'h-96' = 'fhp-h-384'
    'h-screen' = 'fhp-h-screen'

    # ---- offsets ----
    'top-1'    = 'fhp-top-4';    'top-2'    = 'fhp-top-8';    'top-3'    = 'fhp-top-12'
    'top-4'    = 'fhp-top-16';   'top-5'    = 'fhp-top-20';   'top-px'   = 'fhp-top-1'
    'bottom-1' = 'fhp-bottom-4'; 'bottom-2' = 'fhp-bottom-8'; 'bottom-3' = 'fhp-bottom-12'
    'bottom-4' = 'fhp-bottom-16';'bottom-5' = 'fhp-bottom-20';'bottom-px'= 'fhp-bottom-1'
    'left-1'   = 'fhp-left-4';   'left-2'   = 'fhp-left-8';   'left-3'   = 'fhp-left-12'
    'left-4'   = 'fhp-left-16';  'left-5'   = 'fhp-left-20';  'left-px'  = 'fhp-left-1'
    'right-1'  = 'fhp-right-4';  'right-2'  = 'fhp-right-8';  'right-3'  = 'fhp-right-12'
    'right-4'  = 'fhp-right-16'; 'right-5'  = 'fhp-right-20'; 'right-px' = 'fhp-right-1'
}