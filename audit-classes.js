// Screenshots key pages via headless Chrome (CDP) so the rendered UI can be
// reviewed. Newer Chrome requires PUT for /json/new (the old audit-classes.js
// used GET and now fails), which this script does.
const { spawn } = require('child_process');
const http = require('http');
const fs = require('fs');
const path = require('path');
const WS = require('./ws-shim');

const CHROME = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const PORT = 9223;
const PROFILE = path.join(__dirname, '.chrome-cdp');
const OUT = path.join(__dirname, 'shots');
fs.mkdirSync(OUT, { recursive: true });

const PAGES = [
  { name: 'home', url: 'http://localhost:5124/' },
  { name: 'gaming-details', url: 'http://localhost:5124/Gaming/Details/5' },
  { name: 'gaming-index', url: 'http://localhost:5124/Gaming' },
  { name: 'streaming-details', url: 'http://localhost:5124/Streaming/Details/1' },
  { name: 'streaming-index', url: 'http://localhost:5124/Streaming' },
  { name: 'explore', url: 'http://localhost:5124/Explore' },
  { name: 'explore-details', url: 'http://localhost:5124/Explore/Details/1' },
  { name: 'merch', url: 'http://localhost:5124/Merch' },
  { name: 'music', url: 'http://localhost:5124/Music' },
  { name: 'news', url: 'http://localhost:5124/News' },
];

const chrome = spawn(CHROME, [
  '--headless=new', `--remote-debugging-port=${PORT}`, `--user-data-dir=${PROFILE}`,
  '--no-first-run', '--no-default-browser-check', '--disable-gpu',
  '--hide-scrollbars', '--window-size=1440,3000', 'about:blank',
], { stdio: 'ignore' });

const getJson = (url, method = 'GET') => new Promise((res, rej) => {
  const req = http.request(url, { method }, (r) => {
    let d = ''; r.on('data', (c) => (d += c)); r.on('end', () => { try { res(JSON.parse(d)); } catch (e) { rej(e); } });
  });
  req.on('error', rej); req.end();
});
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

(async () => {
  for (let i = 0; i < 60; i++) {
    try { await getJson(`http://localhost:${PORT}/json/version`); break; } catch { await sleep(500); }
  }

  for (const p of PAGES) {
    const tab = await getJson(`http://localhost:${PORT}/json/new?about:blank`, 'PUT');
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
    await send('Emulation.setDeviceMetricsOverride', { width: 1440, height: 3000, deviceScaleFactor: 1, mobile: false });
    const loaded = once('Page.loadEventFired');
    await send('Page.navigate', { url: p.url });
    await loaded;
    await sleep(3500);

    // Measure layout problems: horizontal overflow, broken images, player size.
    const probe = `(() => {
      const imgs = [...document.images];
      return {
        overflowX: Math.max(0, document.documentElement.scrollWidth - document.documentElement.clientWidth),
        brokenImgs: imgs.filter(i => i.complete && i.naturalWidth === 0).map(i => i.getAttribute('src')).slice(0, 8),
        video: [...document.querySelectorAll('video')].map(v => { const r = v.getBoundingClientRect(); return Math.round(r.width) + 'x' + Math.round(r.height); }),
        iframe: [...document.querySelectorAll('iframe')].map(v => { const r = v.getBoundingClientRect(); return Math.round(r.width) + 'x' + Math.round(r.height); }),
        playerBox: [...document.querySelectorAll('.fhp-media__player')].map(v => { const r = v.getBoundingClientRect(); return Math.round(r.width) + 'x' + Math.round(r.height); }),
      };
    })()`;
    const r = await send('Runtime.evaluate', { expression: probe, returnByValue: true });
    console.log('=== ' + p.name + ' ===');
    console.log(JSON.stringify(r.result.value));

    const shot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: true });
    fs.writeFileSync(path.join(OUT, p.name + '.png'), Buffer.from(shot.data, 'base64'));
    ws.close();
  }
  chrome.kill();
  process.exit(0);
})().catch((e) => { console.error('FAIL', e); chrome.kill(); process.exit(1); });

