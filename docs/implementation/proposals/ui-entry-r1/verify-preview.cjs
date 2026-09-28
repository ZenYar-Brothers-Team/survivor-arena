// Browser-only checks of the proposal. Not Unity/runtime acceptance.
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const output = path.resolve(__dirname,'../../../../TestResults/ui-entry-r1');
(async()=>{
  fs.mkdirSync(output,{recursive:true});
  const browser = await chromium.launch({headless:true,channel:'chrome'});
  const page = await browser.newPage({viewport:{width:1952,height:1400},deviceScaleFactor:1});
  const errors=[], captures=[];
  page.on('pageerror', e=>errors.push(e.message));
  page.on('response', r=>{if(r.status()>=400)errors.push(`${r.status()} ${r.url()}`);});
  const open = screen=>page.locator(`[data-screen="${screen}"]`).click();
  const actionsLayout = ()=>page.locator('.menu .action').evaluateAll(elements=>elements.map(e=>{
    const r=e.getBoundingClientRect(), root=document.querySelector('#viewport').getBoundingClientRect();
    return {text:e.textContent,x:r.x-root.x,y:r.y-root.y,width:r.width,height:r.height};
  }));
  async function verifyMotion(width, actions) {
    const control=page.locator('#menu-motion');
    assert.equal(await control.isVisible(),true);
    assert.equal(await control.isChecked(),true);
    const transforms=[];
    for(const phase of [0,6000,12000,18000,24000]) {
      const geometry=await page.evaluate(phase=>{
        const root=document.querySelector('#viewport'), art=root.querySelector('.menu-art');
        const animations=root.getAnimations({subtree:true});
        animations.forEach(a=>{a.pause();a.currentTime=phase;});
        const image=art.getBoundingClientRect(), bounds=root.getBoundingClientRect();
        return {count:animations.length,covered:image.left<=bounds.left+.5 && image.right>=bounds.right-.5 && image.top<=bounds.top+.5 && image.bottom>=bounds.bottom-.5,
          transform:getComputedStyle(art).transform,pointerEvents:getComputedStyle(root.querySelector('.menu-atmosphere')).pointerEvents};
      },phase);
      assert.equal(geometry.count,8,'One image transform and seven bounded dust animations');
      assert.equal(geometry.covered,true,'Motion must never reveal an image edge');
      assert.equal(geometry.pointerEvents,'none');
      transforms.push(geometry.transform);
      assert.deepEqual(await actionsLayout(),actions,'Moving artwork must not move controls');
      if(phase===12000)await capture(`menu-D-animated-${width}`);
    }
    assert.ok(new Set(transforms).size>1,'Image transform changes over time');
    await page.locator('.menu-art').evaluate(e=>{const a=e.getAnimations()[0];a.currentTime=0;a.play();});
    await page.waitForFunction(()=>document.querySelector('.menu-art').getAnimations()[0].currentTime>80);
    await control.uncheck();
    assert.equal(await page.locator('#viewport').evaluate(e=>e.getAnimations({subtree:true}).length),0);
    assert.equal(await page.locator('.menu-art').evaluate(e=>getComputedStyle(e).transform),'none');
    await capture(`menu-D-static-${width}`);
    await control.check();
    await page.emulateMedia({reducedMotion:'reduce'});
    await page.waitForFunction(()=>document.querySelector('#menu-motion').disabled);
    assert.equal(await control.isChecked(),false);
    assert.equal(await page.locator('#viewport').evaluate(e=>e.getAnimations({subtree:true}).length),0,'Reduced motion disables image and particles');
    await page.emulateMedia({reducedMotion:'no-preference'});
    await page.waitForFunction(()=>!document.querySelector('#menu-motion').disabled);
    assert.equal(await control.isChecked(),true);
  }
  async function verifyDepth(width,actions) {
    const root=page.locator('#viewport'), bounds=await root.boundingBox();
    assert.equal(await page.locator('.depth-light').count(),0,'No blanket brightening overlay');
    await page.waitForFunction(()=>document.querySelector('.sun-ray').getAnimations()[0].currentTime>100);
    assert.equal(await root.evaluate(e=>e.style.getPropertyValue('--look-x')),'0','Atmosphere runs before pointer movement');
    await page.mouse.move(bounds.x+bounds.width*.9,bounds.y+bounds.height*.25);
    await page.waitForFunction(()=>Number(document.querySelector('#viewport').style.getPropertyValue('--look-x'))>.6);
    assert.match(await page.locator('.depth-front img').getAttribute('src'),/foreground-shepotka-v002\.png$/);
    const matrices=[], rays=[], motes=[], rayAngles=[], dustCoverage=[];
    for(const phase of [0,5000,10000,15000,20000]) {
      const sample=await root.evaluate((root,phase)=>{
        const animations=root.getAnimations({subtree:true}); animations.forEach(a=>{a.pause();a.currentTime=phase;});
        const back=root.querySelector('.depth-back'), front=root.querySelector('.depth-front'), light=root.querySelector('.depth-rays');
        const r=root.getBoundingClientRect(), b=back.getBoundingClientRect();
        const img=front.querySelector('img'), imageBounds=img.getBoundingClientRect();
        const canvas=document.createElement('canvas');canvas.width=img.naturalWidth;canvas.height=img.naturalHeight;
        const ctx=canvas.getContext('2d');ctx.drawImage(img,0,0);
        const alpha=ctx.getImageData(0,0,canvas.width,canvas.height).data;
        const coveredSides=new Set();
        for(const mote of root.querySelectorAll('.menu-mote')) {
          if(Number(getComputedStyle(mote).opacity)<=.1)continue;
          const m=mote.getBoundingClientRect(), x=m.x+m.width/2, y=m.y+m.height/2;
          const px=Math.floor((x-imageBounds.x)/imageBounds.width*canvas.width), py=Math.floor((y-imageBounds.y)/imageBounds.height*canvas.height);
          if(px>=0 && py>=0 && px<canvas.width && py<canvas.height && alpha[(py*canvas.width+px)*4+3]>200)
            coveredSides.add(x<r.x+r.width*.73?'left':'right');
        }
        const rayMatrix=new DOMMatrixReadOnly(getComputedStyle(root.querySelector('.sun-ray')).transform);
        return {count:animations.length,back:getComputedStyle(back).transform,front:getComputedStyle(front).transform,
          covered:b.left<=r.left && b.top<=r.top && b.right>=r.right && b.bottom>=r.bottom,
          pointerEvents:getComputedStyle(root.querySelector('.menu-depth')).pointerEvents,
          raysBehind:Boolean(light.compareDocumentPosition(front)&Node.DOCUMENT_POSITION_FOLLOWING),
          rayEdgeFade:getComputedStyle(light).maskImage.startsWith('linear-gradient('),
          rayAngle:Math.atan2(rayMatrix.b,rayMatrix.a)*180/Math.PI,
          dustCoverage:[...coveredSides],
          dustInFront:Number(getComputedStyle(root.querySelector('.menu-atmosphere')).zIndex)>Number(getComputedStyle(root.querySelector('.menu-depth')).zIndex),
          frontFilter:getComputedStyle(front.querySelector('img')).filter,frontOpacity:getComputedStyle(front).opacity,
          ray:getComputedStyle(root.querySelector('.sun-ray')).transform,mote:getComputedStyle(root.querySelector('.menu-mote')).transform,
          visibleMotes:[...root.querySelectorAll('.menu-mote')].filter(e=>Number(getComputedStyle(e).opacity)>.1).length,
          softMotes:[...root.querySelectorAll('.menu-mote')].every(e=>{
            const s=getComputedStyle(e), size=parseFloat(s.width);
            return size>=7.49 && size<=16.81 && s.backgroundImage.startsWith('radial-gradient(') && Number(s.opacity)<=.461;
          }),
          light:Number(getComputedStyle(root.querySelector('.sun-ray')).opacity)};
      },phase);
      assert.equal(sample.count,23); assert.equal(sample.covered,true); assert.equal(sample.pointerEvents,'none');
      assert.notEqual(sample.back,sample.front,'Depth planes move independently');
      assert.ok(sample.light>=.599 && sample.light<=.851,'Pronounced ray opacity stays bounded');
      assert.equal(sample.raysBehind,true,'Light is behind opaque goblins, not a tint on their faces');
      assert.equal(sample.rayEdgeFade,true,'Stronger rays fade toward the menu without a hard clipping seam');
      assert.equal(sample.dustInFront,true,'Dust is explicitly composited above both goblins');
      assert.equal(sample.frontFilter,'none'); assert.equal(sample.frontOpacity,'1');
      assert.ok(sample.visibleMotes>=5,'Staggered particles keep the atmosphere present');
      assert.equal(sample.softMotes,true,'Intermediate dust size retains soft edges and partial transparency');
      rays.push(sample.ray);motes.push(sample.mote);
      rayAngles.push(sample.rayAngle);dustCoverage.push(...sample.dustCoverage);
      matrices.push(sample.front);
      assert.deepEqual(await actionsLayout(),actions,'Depth does not move controls');
      if(phase===10000)await capture(`menu-E-depth-${width}`);
    }
    assert.ok(new Set(matrices).size>1);
    assert.ok(new Set(rays).size>1 && new Set(motes).size>1,'Rays and dust move on autonomous timelines');
    assert.ok(Math.max(...rayAngles)-Math.min(...rayAngles)>5.9,'Rays visibly sweep through six degrees');
    assert.ok(dustCoverage.includes('left') && dustCoverage.includes('right'),'Visible dust crosses opaque pixels of both characters during the cycle');
    const transparency=await page.locator('.depth-front img').evaluate(img=>{
      const canvas=document.createElement('canvas');canvas.width=img.naturalWidth;canvas.height=img.naturalHeight;
      const ctx=canvas.getContext('2d');ctx.drawImage(img,0,0);const pixels=ctx.getImageData(0,0,canvas.width,canvas.height).data;
      let clear=0,opaque=0; for(let i=3;i<pixels.length;i+=4){if(pixels[i]===0)clear++;if(pixels[i]>250)opaque++;}
      return {clear:clear/(pixels.length/4),opaque:opaque/(pixels.length/4)};
    });
    assert.ok(transparency.clear>.6 && transparency.opaque>.1,'Foreground has real alpha, not a painted black background');
    await page.locator('#menu-motion').uncheck();
    assert.equal(await root.evaluate(e=>e.getAnimations({subtree:true}).length),0);
    assert.equal(await root.evaluate(e=>e.style.getPropertyValue('--look-x')),'0');
    await capture(`menu-E-static-${width}`);
    await page.locator('#menu-motion').check();
    await page.emulateMedia({reducedMotion:'reduce'});
    await page.waitForFunction(()=>document.querySelector('#menu-motion').disabled);
    assert.equal(await root.evaluate(e=>e.getAnimations({subtree:true}).length),0);
    await page.emulateMedia({reducedMotion:'no-preference'});
    await page.waitForFunction(()=>!document.querySelector('#menu-motion').disabled);
    await page.locator('[data-go="characters"]').click();
    assert.equal(await root.evaluate(e=>e.getAnimations({subtree:true}).length),0);
    assert.equal(await root.evaluate(e=>e.style.getPropertyValue('--look-x')),'0');
    await page.keyboard.press('Escape');
    assert.equal(await page.locator('.menu-variant-E').count(),1);
  }
  async function verifyFieldGrid() {
    assert.equal(await page.locator('.field-tile').count(),10);
    assert.equal(await page.locator('.field-detail').count(),0);
    assert.equal(await page.locator('.fields-page').innerText().then(s=>/плетн|огород|бочк|Обломки стен/.test(s)),false);
    const geometry=await page.locator('.field-grid').evaluate(grid=>{
      const r=grid.getBoundingClientRect();
      return {scroll:grid.scrollHeight-grid.clientHeight,
        clipped:[...grid.children].filter(e=>{const b=e.getBoundingClientRect();return b.top<r.top||b.bottom>r.bottom||e.scrollHeight>e.clientHeight+1;}).length};
    });
    assert.ok(geometry.scroll<=1,JSON.stringify(geometry)); assert.equal(geometry.clipped,0);
  }
  async function capture(label) {
    assert.deepEqual(errors,[],label+' browser errors');
    await page.mouse.move(0,0);
    await page.evaluate(()=>Promise.all([document.fonts.ready,...Array.from(document.images,i=>i.decode())]));
    const geometry = await page.evaluate(()=>{
      const root=document.querySelector('#viewport'), bounds=root.getBoundingClientRect();
      const outside=Array.from(root.querySelectorAll('.action,.detail,.catalog,.page-header,.page-footer,.hero-body .lock-note')).filter(e=>{
        const r=e.getBoundingClientRect();return r.left<bounds.left-1||r.right>bounds.right+1||r.top<bounds.top-1||r.bottom>bounds.bottom+1;
      }).map(e=>e.className);
      const overflow=Array.from(root.querySelectorAll('.choice,.field-copy,.field-title-row,.character-copy,.page-footer,.hero-body')).filter(e=>e.scrollWidth>e.clientWidth+1).map(e=>e.className);
      return {outside,overflow,images:Array.from(document.images).filter(i=>!i.naturalWidth).map(i=>i.src),width:bounds.width,height:bounds.height};
    });
    assert.ok(geometry.width>1200 && geometry.height>=720,JSON.stringify(geometry));
    assert.deepEqual({...geometry,width:0,height:0},{outside:[],overflow:[],images:[],width:0,height:0},label);
    await page.locator('#viewport').screenshot({path:path.join(output,label+'.png')}); captures.push(label);
  }
  try {
    await page.emulateMedia({reducedMotion:'no-preference'});
    await page.goto(`http://127.0.0.1:${process.env.UI_ENTRY_PREVIEW_PORT||4180}/`);
    for(const width of [1280,1920]) {
      await page.selectOption('#resolution',String(width));
      await page.selectOption('#profile','new'); await page.locator('#dense').uncheck();
      await open('menu');
      let initialActions;
      for (const variant of ['A','B','C','D','E']) {
        await page.locator(`[data-menu-variant="${variant}"]`).click();
        assert.equal(await page.locator('.menu-art').count(),variant==='C'?0:1);
        assert.equal(await page.locator('.menu-character').count(),['A','C'].includes(variant)?1:0);
        const actions=await actionsLayout();
        if(!initialActions)initialActions=actions; else assert.deepEqual(actions,initialActions,'Variants preserve menu actions and geometry');
        await capture(`menu-${variant}-${width}`);
        if(variant==='D')await verifyMotion(width,actions);
        else if(variant==='E')await verifyDepth(width,actions);
        else assert.equal(await page.locator('#viewport').evaluate(e=>e.getAnimations({subtree:true}).length),0,'Other variants remain static');
      }
      await page.locator('[data-menu-variant="A"]').click();
      await page.locator('[data-go="characters"]').click(); await capture(`character-open-${width}`);
      assert.equal(await page.locator('#viewport').evaluate(e=>e.getAnimations({subtree:true}).length),0,'No animation remains on another screen');
      assert.equal(await page.locator('#character-confirm').isEnabled(),true);
      await page.locator('[data-character="1"]').click();
      assert.equal(await page.locator('.hero-body > img').evaluate(e=>getComputedStyle(e).filter),'brightness(0)');
      assert.equal(await page.locator('[data-character="1"] img').evaluate(e=>getComputedStyle(e).filter),'brightness(0)');
      assert.equal(await page.locator('#character-confirm').isDisabled(),true);
      assert.match(await page.locator('.lock-note').innerText(),/Пройдите.*Деревенскую окраину/);
      await capture(`character-locked-${width}`);
      await page.selectOption('#profile','earned');
      assert.equal(await page.locator('.hero-body > img').evaluate(e=>getComputedStyle(e).filter),'brightness(0)','Unpurchased remains silhouette');
      assert.equal(await page.locator('#character-confirm').isDisabled(),true);
      assert.match(await page.locator('.lock-note').innerText(),/покупки · 100/);
      await capture(`character-purchase-${width}`);
      await page.selectOption('#profile','owned');
      assert.equal(await page.locator('.hero-body > img').evaluate(e=>getComputedStyle(e).filter),'none','Owned body reveals original art');
      assert.equal(await page.locator('[data-character="1"] img').evaluate(e=>getComputedStyle(e).filter),'none');
      assert.equal(await page.locator('#character-confirm').isEnabled(),true);
      await capture(`character-owned-${width}`);
      await page.locator('#character-confirm').focus(); await page.keyboard.press('Enter');
      assert.equal(await page.locator('h1').innerText(),'Выбери поле');
      assert.match(await page.locator('.selected-character').innerText(),/Бугор/);
      await capture(`field-open-${width}`);
      await verifyFieldGrid();
      await page.locator('[data-field="2"]').focus(); await page.keyboard.press('Space');
      assert.equal(await page.locator('#field-confirm').isDisabled(),true);
      assert.match(await page.locator('.field-selection-note .lock-note').innerText(),/Королевский тракт/);
      await capture(`field-locked-${width}`);
      await page.locator('[data-field="1"]').click();
      assert.equal(await page.locator('#field-confirm').isEnabled(),true);
      await page.locator('#field-confirm').click();
      assert.match(await page.locator('#review-message').innerText(),/Бугор → Королевский тракт/);
      await page.keyboard.press('Escape');
      assert.equal(await page.locator('[data-character="1"]').getAttribute('aria-pressed'),'true');
      assert.equal(await page.locator('#menu-review').isVisible(),false);
      await page.locator('#dense').check();
      for(const screen of ['characters','fields']) {
        await open(screen);
        assert.equal(await page.locator('.choice').count(),screen==='fields'?15:10);
        const footer=await page.locator('.page-footer').boundingBox();
        await page.locator('.catalog-scroll').evaluate(e=>e.scrollTop=e.scrollHeight);
        assert.ok(await page.locator('.catalog-scroll').evaluate(e=>e.scrollTop>0));
        await page.locator('.choice').last().click();
        assert.ok(await page.locator('.catalog-scroll').evaluate(e=>e.scrollTop>0),'Inspection preserves list scroll');
        assert.deepEqual(await page.locator('.page-footer').boundingBox(),footer);
        assert.equal(await page.locator('.page-footer .primary').isDisabled(),true);
        await capture(`dense-${screen}-${width}`);
      }
    }
    for (const variant of ['C','D','E']) {
      await page.goto(`http://127.0.0.1:${process.env.UI_ENTRY_PREVIEW_PORT||4180}/?menu=${variant}`);
      assert.equal(await page.locator(`.menu-variant-${variant}`).count(),1,'Direct link restores requested variant');
      await page.locator('[data-go="characters"]').click();
      await page.keyboard.press('Escape');
      assert.equal(await page.locator(`.menu-variant-${variant}`).count(),1,'Back retains menu variant');
    }
    await page.goto(`http://127.0.0.1:${process.env.UI_ENTRY_PREVIEW_PORT||4180}/?menu=D&motion=off`);
    assert.equal(await page.locator('#menu-motion').isChecked(),false,'Static direct link');
    await page.locator('[data-go="characters"]').click();
    await page.keyboard.press('Escape');
    assert.equal(await page.locator('#menu-motion').isChecked(),false,'Back preserves static preference');
    assert.deepEqual(errors,[]);
    const summary={verdict:'PASS',scope:'HTML proposal only',browser:'headless Chrome, temporary profile',resolutions:['1280x720','1920x1080'],captures,errors,
      checks:['A/B/C/D/E main menu variants preserve actions, geometry and Back','D motion timeline, image coverage and stationary controls','E independent depth planes, alpha, pointer response and light bounds','motion toggle, reduced motion and screen cleanup','variant and static direct links','assets and horizontal overflow','viewport and fixed footer bounds','unowned silhouettes / purchased original body','ten fields fit without scroll or environment descriptions','locked and unpurchased confirm guards','click/keyboard inspection without navigation','separate confirm','Back retains character','10-character / 15-field overflow and scroll retention']};
    fs.writeFileSync(path.join(output,'summary.json'),JSON.stringify(summary,null,2));
    console.log(JSON.stringify(summary));
  } finally { await browser.close(); }
})().catch(error=>{console.error(error);process.exitCode=1;});
