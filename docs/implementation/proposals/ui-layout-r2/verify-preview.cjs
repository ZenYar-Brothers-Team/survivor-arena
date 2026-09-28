// Requires Playwright. Checks only this HTML review, never launches Unity.
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const output = path.resolve(__dirname,'../../../../TestResults/ui-layout-r2');
const url = 'http://127.0.0.1:4179/docs/implementation/proposals/ui-layout-r2/index.html';
(async()=>{
  fs.mkdirSync(output,{recursive:true});
  const browser=await chromium.launch({headless:true,channel:process.env.UI_PREVIEW_BROWSER || 'msedge'});
  const page=await browser.newPage({viewport:{width:1952,height:1300},deviceScaleFactor:1});
  const errors=[];const checks=[];
  page.on('pageerror',error=>errors.push(error.message));
  page.on('response',r=>{if(r.status()>=400)errors.push(`${r.status()} ${r.url()}`);});
  try{
    await page.goto(url);await page.waitForSelector('body[data-ready=true]');
    for(const width of [1280,1920]){
      await page.selectOption('#resolution',String(width));
      for(const screen of ['hud','draft','book','pause','settings']){
        await page.locator(`button[data-screen=${screen}]`).click();
        await page.mouse.move(0,0);
        await page.evaluate(()=>Promise.all([...document.images].map(i=>i.decode().catch(()=>{}))));
        const geometry=await page.evaluate(()=>{
          const stage=document.querySelector('#stage').getBoundingClientRect();
          const visible=e=>!!(e.offsetWidth||e.offsetHeight)&&!e.closest('[hidden]');
          const clip=[...document.querySelectorAll('#overlay .draft-card,#overlay .mini-grid,#overlay .pane,#overlay footer button')].filter(visible).filter(e=>{
            const r=e.getBoundingClientRect();return r.left<stage.left||r.right>stage.right+1||r.top<stage.top||r.bottom>stage.bottom+1;
          }).map(e=>e.className||e.textContent);
          const textOverflow=[...document.querySelectorAll('.card-main,.mini-slot,.build-pane,.character-pane')].filter(visible).filter(e=>e.scrollHeight>e.clientHeight+2||e.scrollWidth>e.clientWidth+2).map(e=>e.className+': '+e.textContent);
          return {clip,textOverflow,broken:[...document.images].filter(visible).filter(i=>!i.naturalWidth).map(i=>i.src),stage:[stage.width,stage.height]};
        });
        await page.locator('#stage').screenshot({path:path.join(output,`${screen}-${width}.png`)});
        assert.deepEqual(geometry.clip,[],`${screen}/${width} clipping`);
        assert.deepEqual(geometry.textOverflow,[],`${screen}/${width} text overflow`);
        assert.deepEqual(geometry.broken,[],`${screen}/${width} missing images`);
        assert.equal(Math.round(geometry.stage[0]),width);
        if(screen==='book'){
          const cardText=await page.locator('.draft-grid').innerText();
          assert.ok(cardText.includes('+50% урона'),'Damage upgrade is shown as percent');
          assert.ok(!/9[,.]33|11[,.]2|Базовый урон/.test(cardText),'No base damage numbers on cards');
        }
        if(screen==='draft'||screen==='book'){
          const before=await page.locator('.draft-grid').boundingBox();
          await page.locator('[data-details="1"]').click();
          const after=await page.locator('.draft-grid').boundingBox();
          assert.deepEqual(after,before,'Details must not move cards');
          const detail=await page.locator('#details').boundingBox(),actions=await page.locator('.draft-actions').boundingBox();
          assert.ok(detail.y+detail.height<actions.y,'Details must not cover actions');
          await page.locator('#stage').screenshot({path:path.join(output,`${screen}-details-${width}.png`)});
        }
        checks.push(`${screen}/${width}: geometry, assets, capture`);
      }
      await page.locator('button[data-screen=hud]').click();
      await page.selectOption('#loadout','start');
      await page.locator('#stage').screenshot({path:path.join(output,`hud-start-${width}.png`)});
      await page.locator('button[data-screen=pause]').click();
      await page.locator('#stage').screenshot({path:path.join(output,`pause-start-${width}.png`)});
      await page.selectOption('#loadout','full');
      await page.locator('button[data-screen=hud]').click();
      await page.locator('#density').click();
      await page.locator('#stage').screenshot({path:path.join(output,`hud-dense-${width}.png`)});
      await page.locator('#density').click();
    }
    await page.locator('button[data-screen=settings]').click();
    assert.equal(await page.locator('#mouse-mode').isChecked(),false,'Keyboard is default');
    await page.locator('#mouse-mode').focus();await page.keyboard.press('Space');
    assert.equal(await page.locator('#mouse-mode').isChecked(),true);
    assert.equal(await page.locator('#stage').getAttribute('data-screen'),'settings','No pause+checkbox double action');
    await page.locator('button[data-screen=draft]').click();
    await page.locator('[data-action=banish]').click();assert.equal(await page.locator('[data-action=reroll]').isDisabled(),true);
    await page.locator('[data-action=banish]').click();assert.equal(await page.locator('[data-action=reroll]').isEnabled(),true);
    await page.locator('.card-main').first().focus();await page.keyboard.press('Space');
    assert.equal(await page.locator('#stage').getAttribute('data-screen'),'draft','Space must not unpause draft');
    assert.equal(await page.locator('.toast').count(),1,'One selection feedback only');
    await page.locator('button[data-screen=hud]').click();await page.locator('#world').click({position:{x:20,y:200},force:true});await page.keyboard.press('Space');
    assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause');
    await page.locator('#stage').click({button:'right',position:{x:20,y:200}});
    assert.equal(await page.locator('#stage').getAttribute('data-screen'),'hud');
    checks.push('Keyboard default; checkbox Space; no Draft pause leakage; single selection; banish disabled state; Space/RMB pause; percent damage, no base damage');
    assert.deepEqual(errors,[],'Browser errors');
    const result={result:'PASS',scope:'HTML composition only, not Unity verification',checks,output};
    fs.writeFileSync(path.join(output,'summary.json'),JSON.stringify(result,null,2));
    console.log(JSON.stringify(result,null,2));
  }finally{await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
