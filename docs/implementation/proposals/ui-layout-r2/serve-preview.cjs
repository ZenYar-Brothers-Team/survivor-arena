// Read-only, loopback-only server. No project mutations and no Unity connection.
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../../..');
const port = Number(process.env.UI_PREVIEW_PORT || 4179);
const mime = {'.html':'text/html; charset=utf-8','.css':'text/css; charset=utf-8','.js':'text/javascript; charset=utf-8','.json':'application/json','.png':'image/png','.ttf':'font/ttf'};
const allowed = ['docs/implementation/proposals/ui-layout-r2/', 'Assets/Resources/Art/', 'Assets/Resources/Content/ActiveSkills/', 'Assets/Resources/Content/Passives/', 'Assets/Resources/Content/Sets/', 'Assets/Game/UI/Fonts/'];
http.createServer((req,res)=>{
  if(req.method!=='GET'&&req.method!=='HEAD'){res.writeHead(405);return res.end();}
  let relative;
  try {relative=decodeURIComponent(new URL(req.url,'http://localhost').pathname).replace(/^\/+/, '');}catch{res.writeHead(400);return res.end();}
  if(!relative)relative='docs/implementation/proposals/ui-layout-r2/index.html';
  const full=path.resolve(root,relative), safeRelative=path.relative(root,full).replaceAll('\\','/');
  if(!safeRelative||safeRelative.startsWith('..')||!allowed.some(prefix=>safeRelative.startsWith(prefix))||!mime[path.extname(full)]){res.writeHead(403);return res.end();}
  fs.stat(full,(error,stat)=>{
    if(error||!stat.isFile()){res.writeHead(404);return res.end();}
    res.writeHead(200,{'Content-Type':mime[path.extname(full)],'Cache-Control':'no-store','X-Content-Type-Options':'nosniff'});
    if(req.method==='HEAD')return res.end();
    fs.createReadStream(full).on('error',()=>res.destroy()).pipe(res);
  });
}).listen(port,'127.0.0.1',()=>console.log(`UI preview: http://127.0.0.1:${port}/docs/implementation/proposals/ui-layout-r2/index.html`));
