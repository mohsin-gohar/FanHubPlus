// Minimal WebSocket client (no npm deps) - enough to drive Chrome DevTools.
const net = require('net');
const crypto = require('crypto');

class WS {
  constructor(url) {
    const u = new URL(url);
    this.key = crypto.randomBytes(16).toString('base64');
    this.handlers = [];
    this.buf = Buffer.alloc(0);
    this.frag = [];
    this.onOpen = null;
    this.connect(u);
  }
  connect(u) {
    this.sock = net.connect(Number(u.port) || 80, u.hostname, () => {
      this.sock.write(
        `GET ${u.pathname}${u.search} HTTP/1.1\r\n` +
        `Host: ${u.host}\r\nUpgrade: websocket\r\nConnection: Upgrade\r\n` +
        `Sec-WebSocket-Key: ${this.key}\r\nSec-WebSocket-Version: 13\r\n\r\n`
      );
    });
    let handshook = false;
    this.sock.on('data', (d) => {
      if (!handshook) {
        const s = d.toString('latin1');
        const end = s.indexOf('\r\n\r\n');
        if (end === -1) return;
        handshook = true;
        if (this.onOpen) this.onOpen();
        d = d.subarray(end + 4);
        if (!d.length) return;
      }
      this.buf = Buffer.concat([this.buf, d]);
      this.drain();
    });
    this.sock.on('error', (e) => console.error('ws error', e.message));
  }
  open() { return new Promise((r) => { this.onOpen = r; }); }
  on(ev, fn) { this.handlers.push([ev, fn]); }
  off(ev, fn) { this.handlers = this.handlers.filter(([e, f]) => !(e === ev && f === fn)); }
  drain() {
    while (this.buf.length >= 2) {
      const b1 = this.buf[1];
      let len = b1 & 127, off = 2;
      if (len === 126) { if (this.buf.length < 4) return; len = this.buf.readUInt16BE(2); off = 4; }
      else if (len === 127) { if (this.buf.length < 10) return; len = Number(this.buf.readBigUInt64BE(2)); off = 10; }
      if (this.buf.length < off + len) return;
      const payload = this.buf.subarray(off, off + len);
      this.buf = this.buf.subarray(off + len);
      if ((b1 & 0x80) === 0) {
        this.frag.push(payload);
        const full = Buffer.concat(this.frag).toString('utf8');
        this.frag = [];
        for (const [ev, fn] of this.handlers) { try { fn(full); } catch (_) {} }
      }
    }
  }
  send(obj) {
    const data = Buffer.from(JSON.stringify(obj));
    const mask = crypto.randomBytes(4);
    const head = [];
    head.push(0x81);
    if (data.length < 126) head.push(0x80 | data.length);
    else if (data.length < 65536) { head.push(0x80 | 126); const b = Buffer.alloc(2); b.writeUInt16BE(data.length); head.push(...b); }
    else { head.push(0x80 | 127); const b = Buffer.alloc(8); b.writeBigUInt64BE(BigInt(data.length)); head.push(...b); }
    const masked = Buffer.from(data.map((v, i) => v ^ mask[i % 4]));
    this.sock.write(Buffer.concat([Buffer.from(head), mask, masked]));
  }
  close() { try { this.sock.destroy(); } catch (_) {} }
}
module.exports = WS;
