const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../../..');
const prefix = 'docs/implementation/proposals/ui-results-r1/';
const allowed = [prefix, 'Assets/Resources/Art/UI/Icons/Sets/', 'Assets/Resources/Art/UI/Icons/Skills/', 'Assets/Game/UI/Fonts/'];
const allowedFiles = ['Assets/Resources/Art/Sprites/Characters/char-002/char-002-body.png', 'Assets/Resources/Art/Sprites/Fields/field-002/field-002-background.png'];
const mime = {'.html':'text/html; charset=utf-8','.png':'image/png','.ttf':'font/ttf'};
http.createServer((req,res)=>{
  if(!['GET','HEAD'].includes(req.method)){res.writeHead(405);return res.end();}
  let relative;
  try{relative=decodeURIComponent(new URL(req.url,'http://localhost').pathname).replace(/^\/+/, '')||prefix+'index.html';}catch{res.writeHead(400);return res.end();}
  const full=path.resolve(root,relative), safe=path.relative(root,full).replaceAll('\\','/');
  if(safe.startsWith('..')||(!allowed.some(p=>safe.startsWith(p))&&!allowedFiles.includes(safe))||!mime[path.extname(full)]){res.writeHead(403);return res.end();}
  fs.stat(full,(error,stat)=>{if(error||!stat.isFile()){res.writeHead(404);return res.end();}res.writeHead(200,{'Content-Type':mime[path.extname(full)],'Cache-Control':'no-store','X-Content-Type-Options':'nosniff'});if(req.method==='HEAD')return res.end();fs.createReadStream(full).on('error',()=>res.destroy()).pipe(res);});
}).listen(4182,'127.0.0.1',()=>console.log('Results preview: http://127.0.0.1:4182/'));
