// Local read-only review server. Never exposes profiles, .git or the whole repo.
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../../..');
const port = Number(process.env.UI_ENTRY_PREVIEW_PORT || 4180);
const preview = 'docs/implementation/proposals/ui-entry-r1/';
const allowed = [preview, 'Assets/Resources/Art/', 'Assets/Game/UI/Fonts/'];
const previewImages = ['TestResults/ui-entry-r1/menu-escape-v001.png', 'TestResults/ui-entry-r1/menu-escape-v002.png', 'TestResults/ui-entry-r1/menu-escape-v003.png', 'TestResults/ui-entry-r1/menu-depth-background-v001.png', 'TestResults/ui-entry-r1/menu-depth-foreground-v001.png', 'TestResults/ui-entry-r1/menu-depth-foreground-shepotka-v001.png', 'TestResults/ui-entry-r1/menu-depth-foreground-shepotka-v002.png'];
const mime = {'.html':'text/html; charset=utf-8','.css':'text/css; charset=utf-8','.js':'text/javascript; charset=utf-8','.png':'image/png','.ttf':'font/ttf'};
http.createServer((req,res)=>{
  if(req.method!=='GET'&&req.method!=='HEAD'){res.writeHead(405);return res.end();}
  let relative;
  try {relative=decodeURIComponent(new URL(req.url,'http://localhost').pathname).replace(/^\/+/, '');}catch{res.writeHead(400);return res.end();}
  if(!relative)relative=preview+'index.html';
  const full=path.resolve(root,relative), safe=path.relative(root,full).replaceAll('\\','/');
  if(!safe||safe.startsWith('..')||(!allowed.some(prefix=>safe.startsWith(prefix))&&!previewImages.includes(safe))||!mime[path.extname(full)]){res.writeHead(403);return res.end();}
  fs.stat(full,(error,stat)=>{
    if(error||!stat.isFile()){res.writeHead(404);return res.end();}
    res.writeHead(200,{'Content-Type':mime[path.extname(full)],'Cache-Control':'no-store','X-Content-Type-Options':'nosniff'});
    if(req.method==='HEAD')return res.end();
    fs.createReadStream(full).on('error',()=>res.destroy()).pipe(res);
  });
}).listen(port,'127.0.0.1',()=>console.log(`UI entry preview: http://127.0.0.1:${port}/`));
