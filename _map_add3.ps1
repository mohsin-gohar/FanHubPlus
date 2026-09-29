# ---------------------------------------------------------------------------
# _map_add3.ps1 — Tailwind leftovers that survived the first two passes.
# Each entry was confirmed still present in the markup by the residual scan.
# ---------------------------------------------------------------------------
@{
    # ---- grid helpers not reachable from _map.tsv ----
    'grid-cols-1'         = 'fhp-gcol-1'
    'grid-cols-2'         = 'fhp-gcol-2'
    'grid-cols-3'         = 'fhp-gcol-3'
    'grid-cols-4'         = 'fhp-gcol-4'
    'grid-cols-5'         = 'fhp-gcol-5'
    'grid-cols-6'         = 'fhp-gcol-6'
    'grid-cols-7'         = 'fhp-gcol-7'
    'grid-cols-8'         = 'fhp-gcol-8'
    'grid-cols-9'         = 'fhp-gcol-9'
    'grid-cols-10'        = 'fhp-gcol-10'
    'grid-cols-11'        = 'fhp-gcol-11'
    'grid-cols-12'        = 'fhp-gcol-12'
    'col-span-2'          = 'fhp-gspan-2'
    'col-span-3'          = 'fhp-gspan-3'
    'col-span-4'          = 'fhp-gspan-4'
    'col-span-full'       = 'fhp-gspan-full'

    # ---- responsive variants Bootstrap spells differently ----
    'md-h-100'                  = 'fhp-md-h-100'
    'md-justify-content-start'  = 'fhp-md-justify-start'
    'sm-justify-content-start'  = 'fhp-sm-justify-start'
    'md-mb-0'                   = 'fhp-md-mb-0'
    'md-mt-0'                   = 'fhp-md-mt-0'
    'sm-mt-0'                   = 'fhp-sm-mt-0'
    'md-mx-0'                   = 'fhp-md-mx-0'
    'md-mx-auto'                = 'fhp-md-mx-auto'
    'xl-d-block'                = 'fhp-xl-d-block'
    'md-max-w-7xl'              = 'fhp-md-maxw-1200'

    # ---- site-specific effects kept verbatim from the Tailwind build ----
    # NOTE: bare "is" / "active" are polymorphic JS state hooks, NOT utilities.
    # They must never be mapped, or Razor expressions like @(x == "is") break.
    'scrollbar'          = 'fhp-scrollbar'
    'backdrop'           = 'fhp-backdrop'
    'with-border'        = 'fhp-with-border'
    'rotateme'           = 'fhp-rotateme'
    'text_animation'     = 'fhp-text-animation'
    'activeCategory'     = 'is-active-cat'
}