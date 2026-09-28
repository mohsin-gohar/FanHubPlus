// Measures the real rendered layout of key pages via headless Chrome (CDP):
// confirms the Tailwind utilities actually resolve in the browser.
const { spawn } = require('child_process');
const http = require('http');
const fs = require('fs');
const path = require('path');
const WS = require('./ws-shim');

const CHROME = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const PORT = 9222;
const PROFILE = path.join(__dirname, '.chrome-verify');
const OUT = path.join(__dirname, 'shots');
fs.mkdirSync(OUT, { recursive: true });

const PAGES = [
  { name: 'gaming-details', url: 'http://localhost:5124/Gaming/Details/5' },
  { name: 'home', url: 'http://localhost:5124/' },
  { name: 'gaming-index', url: 'http://localhost:5124/Gaming' },
  { name: 'explore', url: 'http://localhost:5124/Explore' },
  { name: 'merch', url: 'http://localhost:5124/Merch' },
];

const chrome = spawn(CHROME, [
  '--headless=new', `--remote-debugging-port=${PORT}`, `--user-data-dir=${PROFILE}`,
  '--no-first-run', '--no-default-browser-check', '--disable-gpu',
  '--hide-scrollbars', '--window-size=1440,900', 'about:blank',
], { stdio: 'ignore' });

const getJson = (url) => new Promise((res, rej) => {
  http.get(url, (r) => { let d = ''; r.on('data', (c) => (d += c)); r.on('end', () => res(JSON.parse(d))); }).on('error', rej);
});
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

// The probe runs in the page: how many elements each selector matched and the
// rendered size of the first few, plus any horizontal overflow.
const PROBE = `(() => {
  const size = (sel) => {
    const els = [...document.querySelectorAll(sel)];
    return { n: els.length, sizes: els.slice(0, 3).map((e) => {
      const r = e.getBoundingClientRect();
      return Math.round(r.width) + 'x' + Math.round(r.height);
    }) };
  };
  return {
    iframe: size('iframe'),
    video: size('video'),
    coverImg: size('img.object-cover'),
    aspectBox: size('[class*="aspect-"]'),
    hFull: size('.h-full'),
    truncate: size('.truncate'),
    rounded2xl: size('.rounded-2xl'),
    objectFit: (() => {
      const e = document.querySelector('img.object-cover');
      return e ? getComputedStyle(e).objectFit : 'n/a';
    })(),
    hFullHeight: (() => {
      const e = document.querySelector('.h-full');
      return e ? getComputedStyle(e).height : 'n/a';
    })(),
    overflowX: Math.max(0, document.documentElement.scrollWidth - document.documentElement.clientWidth),
    tailwindLoaded: [...document.styleSheets].some((s) => (s.href || '').includes('tailwind.css')),
  };
})()`;

(async () => {
  for (let i = 0; i < 60; i++) {
    try { await getJson(`http://localhost:${PORT}/json/version`); break; } catch { await sleep(500); }
  }

  for (const p of PAGES) {
    const tab = await getJson(`http://localhost:${PORT}/json/new?about:blank`);
    const ws = new WS(tab.webSocketDebuggerUrl);
    await ws.open();

    let id = 0;
    const pending = new Map();
    ws.on('message', (raw) => {
      const m = JSON.parse(raw);
      if (m.id && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); }
    });
    const send = (method, params) => new Promise((res) => {
      const mid = ++id;
      pending.set(mid, (m) => res(m.result));
      ws.send({ id: mid, method, params });
    });
    const once = (ev) => new Promise((res) => {
      const h = (raw) => { const m = JSON.parse(raw); if (m.method === ev) { ws.off('message', h); res(m.params); } };
      ws.on('message', h);
    });

    await send('Page.enable', {});
    await send('Runtime.enable', {});
    await send('Emulation.setDeviceMetricsOverride', { width: 1440, height: 900, deviceScaleFactor: 1, mobile: false });
    const loaded = once('Page.loadEventFired');
    await send('Page.navigate', { url: p.url });
    await loaded;
    await sleep(3000);

    const r = await send('Runtime.evaluate', { expression: PROBE, returnByValue: true });
    console.log('=== ' + p.name + '  ' + p.url + ' ===');
    console.log(JSON.stringify(r.result.value, null, 2));

    const shot = await send('Page.captureScreenshot', { format: 'png' });
    fs.writeFileSync(path.join(OUT, p.name + '.png'), Buffer.from(shot.data, 'base64'));
    ws.close();
  }
  chrome.kill();
  process.exit(0);
})().catch((e) => { console.error('FAIL', e); chrome.kill(); process.exit(1); });
