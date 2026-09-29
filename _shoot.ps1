param(
    [string]$OutDir = 'F:\.net\FanHubPlus\shots\before',
    [string]$Base  = 'http://localhost:5124'
)
$ErrorActionPreference = 'SilentlyContinue'
$Chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
$Profile = 'F:\.net\FanHubPlus\.chrome-verify'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$Pages = @(
    @{ n = 'home';           u = '/' },
    @{ n = 'music';          u = '/Music' },
    @{ n = 'music-playlists';u = '/Music/Playlists' },
    @{ n = 'streaming';      u = '/Streaming' },
    @{ n = 'gaming';         u = '/Gaming' },
    @{ n = 'explore';        u = '/Explore' },
    @{ n = 'news';           u = '/News' },
    @{ n = 'merch';          u = '/Merch' },
    @{ n = 'blog';           u = '/Blog' },
    @{ n = 'support';        u = '/Support' },
    @{ n = 'events';         u = '/Events' },
    @{ n = 'fanart';         u = '/FanArt' },
    @{ n = 'login';          u = '/Account/Login' },
    @{ n = 'register';       u = '/Account/Register' },
    @{ n = 'contact';        u = '/Contact' },
    @{ n = 'characters';     u = '/Characters' }
)

foreach ($p in $Pages) {
    $url  = $Base + $p.u
    $out  = Join-Path $OutDir ($p.n + '.png')
    & $Chrome --headless=new --disable-gpu --no-sandbox --hide-scrollbars `
        --user-data-dir="$Profile" --window-size=1440,2600 `
        --virtual-time-budget=14000 --run-all-compositor-stages-before-draw `
        --screenshot="$out" "$url" 2>$null | Out-Null
    if (Test-Path $out) {
        $len = (Get-Item $out).Length
        "{0,-18} {1,8} bytes" -f $p.n, $len
    } else {
        "{0,-18} FAILED" -f $p.n
    }
}
