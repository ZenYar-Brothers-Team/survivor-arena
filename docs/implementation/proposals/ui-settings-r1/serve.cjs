const http = require('node:http'), fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../../..');
const prefix = 'docs/implementation/proposals/ui-settings-r1/';
http.createServer((req, res) => {
  if (!['GET', 'HEAD'].includes(req.method)) { res.writeHead(405); return res.end(); }
  let relative;
  try { relative = decodeURIComponent(new URL(req.url, 'http://localhost').pathname).replace(/^\/+/, '') || prefix + 'index.html'; }
  catch { res.writeHead(400); return res.end(); }
  const full = path.resolve(root, relative), safe = path.relative(root, full).replaceAll('\\', '/');
  const mime = { '.html': 'text/html; charset=utf-8', '.png': 'image/png', '.ttf': 'font/ttf' }[path.extname(full)];
  if (safe.startsWith('..') || !(safe.startsWith(prefix) || safe.startsWith('Assets/Game/UI/Fonts/')) || !mime) { res.writeHead(403); return res.end(); }
  fs.stat(full, (error, stat) => {
    if (error || !stat.isFile()) { res.writeHead(404); return res.end(); }
    res.writeHead(200, { 'Content-Type': mime, 'Cache-Control': 'no-store' });
    if (req.method === 'HEAD') return res.end();
    fs.createReadStream(full).pipe(res);
  });
}).listen(4185, '127.0.0.1', () => console.log('Settings preview http://127.0.0.1:4185'));
