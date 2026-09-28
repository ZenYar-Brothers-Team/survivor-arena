// Browser-only geometry and interaction checks. Does not start or connect to Unity.
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const output = path.resolve(__dirname,'../../../../TestResults/ui-layout-r2-set-info');
const url = `http://127.0.0.1:${process.env.UI_PREVIEW_PORT || 4179}/docs/implementation/proposals/ui-layout-r2/index.html`;
(async()=>{
  fs.mkdirSync(output,{recursive:true});
  const browser=await chromium.launch({headless:true,channel:process.env.UI_PREVIEW_BROWSER || 'msedge'});
  const page=await browser.newPage({viewport:{width:1952,height:1400},deviceScaleFactor:1});
  const errors=[],checks=[],captures=[];
  page.on('pageerror',error=>errors.push(error.message));
  page.on('response',r=>{if(r.status()>=400)errors.push(`${r.status()} ${r.url()}`);});
  const open=screen=>page.locator(`button[data-screen=${screen}]`).click();
  const count=async()=>Number(await page.locator('#stage').getAttribute('data-confirm-count'));
  async function capture(label){
    await page.mouse.move(0,0);
    await page.evaluate(()=>Promise.all([...document.images].map(i=>i.decode().catch(()=>{}))));
    const geometry=await page.evaluate(()=>{
      const stage=document.querySelector('#stage').getBoundingClientRect();
      const visible=e=>!!(e.offsetWidth||e.offsetHeight)&&!e.closest('[hidden]')&&getComputedStyle(e).visibility!=='hidden';
      const clip=[...document.querySelectorAll('#overlay .draft-card,#overlay .mini-grid,#overlay .pane,#overlay footer button,#recipe-inspector,#set-info-popup')].filter(visible).filter(e=>{
        const r=e.getBoundingClientRect();return r.left<stage.left||r.right>stage.right+1||r.top<stage.top||r.bottom>stage.bottom+1;
      }).map(e=>e.className);
      const textOverflow=[...document.querySelectorAll('.card-main,.mini-slot,.build-pane,.recipe-inspection,.recipe-entry,.recipe-pick,.set-chip,#set-info-popup')].filter(visible).filter(e=>e.scrollHeight>e.clientHeight+2||e.scrollWidth>e.clientWidth+2).map(e=>e.className+': '+e.textContent);
      return {clip,textOverflow,broken:[...document.images].filter(visible).filter(i=>!i.naturalWidth).map(i=>i.src),stage:[stage.width,stage.height]};
    });
    await page.locator('#stage').screenshot({path:path.join(output,label+'.png')});captures.push(label);
    assert.deepEqual(geometry.clip,[],label+' clipping');assert.deepEqual(geometry.textOverflow,[],label+' text overflow');assert.deepEqual(geometry.broken,[],label+' assets');
  }
  try{
    await page.goto(url);await page.waitForSelector('body[data-ready=true]');
    for(const width of [1280,1920]){
      await page.selectOption('#resolution',String(width));
      await page.selectOption('#loadout','full');
      for(const screen of ['hud','draft','book','pause','settings']){
        await open(screen);await capture(`${screen}-${width}`);
        assert.equal(Math.round((await page.locator('#stage').boundingBox()).width),width);
        if(screen==='hud'){
          const dims=await page.evaluate(()=>({slot:document.querySelector('.icon-slot').offsetWidth,set:document.querySelector('.sets-hud .icon-slot').offsetWidth,xp:parseFloat(getComputedStyle(document.querySelector('.xp-hud')).width),timer:document.querySelector('.timer').offsetWidth}));
          assert.equal(dims.slot,width===1280?30:50);assert.equal(dims.set,dims.slot);assert.ok(Math.abs(dims.xp-(width===1280?208.2:347))<.1);assert.equal(dims.timer,width===1280?112:164);
          assert.ok(!/WASD|WSD|стрелки|Настройки|Пауза|волна/i.test(await page.locator('#hud').innerText()),'No HUD instructions/wave');
          assert.equal(await page.locator('#hud button').count(),0,'No Pause button or other HUD buttons');
        }
        if(screen==='draft'||screen==='book'){
          const cardText=await page.locator('.draft-grid').innerText();
          assert.ok(!/9[,.]33|11[,.]2|Базовый урон|80%/.test(cardText));
          if(screen==='book')assert.ok(cardText.includes('+50% урона'));
          if(screen==='draft')assert.ok(cardText.includes('+1 рикошет.'));
          const before=await page.locator('[data-confirm="1"]').boundingBox(),initial=await count();
          await page.locator('[data-inspect="1"]').click();
          assert.equal(await count(),initial,'Inspect must not choose');
          assert.equal(await page.locator('[data-confirm="0"]').isDisabled(),true);
          assert.equal(await page.locator('[data-confirm="1"]').isEnabled(),true);
          await page.locator('[data-inspect="0"]').hover();
          assert.equal(await page.locator('[data-inspect="1"]').getAttribute('aria-pressed'),'true','Hover does not change pinned card');
          assert.deepEqual(await page.locator('[data-confirm="1"]').boundingBox(),before,'Confirmation does not move');
          const detail=await page.locator('#recipe-inspector').boundingBox(),actions=await page.locator('.draft-actions').boundingBox(),cards=await page.locator('.draft-grid').boundingBox();
          assert.ok(cards.y+cards.height<detail.y&&detail.y+detail.height<actions.y,'Inspector cannot overlap cards/actions');
          await page.locator('[data-recipe]').last().click();assert.equal(await count(),initial,'Recipe inspection must not choose');
          assert.ok((await page.locator('.recipe-effect').innerText()).length>20,'Draft recipe has short set effect next to component levels');
          await page.locator('[data-inspect="2"]').focus();await page.keyboard.press('Space');
          assert.equal(await count(),initial,'Keyboard inspect must not choose');
          assert.equal(await page.locator('[data-inspect="2"]').getAttribute('aria-pressed'),'true');
          await page.locator('[data-confirm="2"]').focus();await page.keyboard.press('Enter');
          assert.equal(await count(),initial+1,'Exactly one explicit confirmation');
          await page.evaluate(()=>document.querySelector('.toast')?.remove());
          await page.locator('[data-inspect="1"]').click();await capture(`${screen}-inspect-${width}`);
        }
        if(screen==='pause'){
          assert.equal(await page.locator('.mini-slot').count(),12);
          assert.equal(await page.locator('.speed-value').innerText(),'100%');
          assert.equal(await page.locator('.missed-entry').count(),3,'All missed recipes, including those with no owned components');
          assert.equal(await page.locator('.missed-entry .component').count(),0);
          assert.ok(await page.locator('.missed-entry').evaluateAll(els=>els.every(e=>e.children.length===2&&e.firstElementChild.tagName==='IMG')));
          const grid=await page.locator('.recipe-grid').evaluate(e=>getComputedStyle(e).gridTemplateColumns.split(' ').length);assert.equal(grid,width===1920?3:2);
          const missed=await page.locator('.missed-entry').first().evaluate(e=>({font:parseFloat(getComputedStyle(e).fontSize),icon:e.querySelector('img').offsetWidth}));
          assert.deepEqual(missed,width===1920?{font:18,icon:34}:{font:16,icon:30},'Larger missed labels/icons');
          assert.equal(await page.locator('.build-pane [data-set-info]').count(),0);
          assert.equal(await page.locator('.recipe-scroll .received-entry').count(),1);
          assert.ok(await page.locator('.received-entry').evaluate(e=>e.children.length===2&&e.firstElementChild.tagName==='IMG'),'Received is compact icon/name too');
          const portrait=await page.locator('.character-portrait').evaluate(e=>[e.offsetWidth,e.offsetHeight]);
          assert.deepEqual(portrait,width===1920?[140,120]:[100,96]);
          const initial=await count(),layout=await page.locator('.pause-layout').boundingBox();
          for(const selector of ['.received-entry','.recipe-entry','.missed-entry']){
            const item=page.locator(selector).first(),title=await item.getAttribute('data-set-title');
            await item.click();assert.equal(await page.locator('#set-info-title').innerText(),title);
            assert.ok((await page.locator('#set-info-effect').innerText()).length>20);
            assert.equal(await count(),initial,'Set info never confirms a choice');
            const popup=await page.locator('#set-info-popup').boundingBox(),footer=await page.locator('.pause-footer').boundingBox();
            assert.ok(popup.y+popup.height<footer.y,'Popup must leave pause actions visible');
            assert.deepEqual(await page.locator('.pause-layout').boundingBox(),layout,'Popup does not reflow layout');
            await capture(`pause-info-${selector.slice(1)}-${width}`);
            await page.keyboard.press('Escape');assert.equal(await page.locator('#set-info-popup').count(),0);assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause');
            assert.ok(await item.evaluate(e=>e===document.activeElement),'Focus returns to inspected set');
          }
          await page.locator('.received-entry').first().focus();await page.keyboard.press('Enter');assert.equal(await page.locator('#set-info-popup').count(),1);
          await page.locator('[data-action=resume]').click();assert.equal(await page.locator('#set-info-popup').count(),0);assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause','Outside click closes without also resuming');
          await page.locator('.received-entry').first().focus();await page.keyboard.press('Space');assert.equal(await page.locator('#set-info-popup').count(),1);
          await page.locator('[data-action=close-set-info]').click();assert.equal(await page.locator('#set-info-popup').count(),0);
          await page.locator('.received-entry').first().click();await page.locator('.missed-entry').first().click();
          assert.equal(await page.locator('#set-info-title').innerText(),await page.locator('.missed-entry').first().getAttribute('data-set-title'));
          await page.locator('#stage').click({button:'right',position:{x:20,y:200}});assert.equal(await page.locator('#set-info-popup').count(),0);assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause');
        }
        checks.push(`${screen}/${width}: geometry, assets, interaction contract`);
      }
      await page.selectOption('#loadout','start');await open('hud');await capture(`hud-start-${width}`);await open('pause');assert.equal(await page.locator('.received-section').count(),0);await capture(`pause-start-${width}`);
      await page.selectOption('#loadout','mid');assert.equal(await page.locator('.recipe-entry').count(),4);await capture(`pause-mid-${width}`);
      await page.selectOption('#loadout','full');await open('hud');await page.locator('#density').click();await capture(`hud-dense-${width}`);await page.locator('#density').click();
      await page.selectOption('#recipe-density','stress');await open('draft');
      assert.equal(await page.locator('[data-recipe]').count(),10);
      assert.equal(await page.locator('.recipe-count').first().innerText(),'Сеты: 10');
      const initial=await count();await page.locator('[data-recipe]').last().click();
      const scroll=await page.locator('.recipe-picker-scroll').evaluate(e=>({top:e.scrollTop,overflow:e.scrollHeight>e.clientHeight}));
      assert.ok(scroll.overflow&&scroll.top>0,'Ten related sets use scroll');
      assert.equal(await count(),initial);assert.ok(await page.locator('.recipe-inspection h2').innerText());
      assert.equal(await page.locator('.recipe-picker-scroll').evaluate(e=>e.scrollTop),scroll.top,'Selecting recipe preserves list position');
      await capture(`draft-stress-${width}`);
      await page.locator('[data-recipe="FIXTURE-UI-RECIPE-09"]').click();await capture(`draft-stress-long-effect-${width}`);
      await open('pause');assert.equal(await page.locator('.recipe-entry').count(),12);assert.equal(await page.locator('.received-entry').count(),4);assert.equal(await page.locator('.missed-entry').count(),4);await capture(`pause-stress-${width}`);
      await page.locator('.received-entry').last().click();await page.locator('.recipe-scroll').evaluate(e=>{e.scrollTop=100;});await page.waitForFunction(()=>!document.querySelector('#set-info-popup'));
      const footer=await page.locator('.pause-footer').boundingBox();
      assert.ok(await page.locator('.recipe-scroll').evaluate(e=>e.scrollHeight>e.clientHeight));
      await page.locator('.recipe-scroll').evaluate(e=>{e.scrollTop=e.scrollHeight;});
      assert.deepEqual(await page.locator('.pause-footer').boundingBox(),footer,'Recipe scroll cannot move actions');
      await capture(`pause-stress-bottom-${width}`);
      await page.locator('.missed-entry').last().click();await capture(`pause-info-edge-${width}`);
      await page.keyboard.press('Escape');assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause');
      await page.keyboard.press('Escape');assert.equal(await page.locator('#stage').getAttribute('data-screen'),'hud','Second Escape can resume after description is dismissed');
      await page.selectOption('#recipe-density','normal');
      checks.push(`${width}: start/mid/full; dense HUD; 10-related/20-total labeled stress; compact missed; fixed footer`);
    }
    checks.push('Received icon/name group shares right scroll; 12 recipes + 4 received + 4 missed stress; all Pause set states open short popup; keyboard, focus return, outside/RMB/Escape close, no double action, edge bounds; enlarged character image');
    const eligibility=await page.evaluate(()=>{
      const rock={id:'SKILL-001',type:'Умение',next:4},orbit={id:'SKILL-003',type:'Умение',next:4},heart={id:'PASSIVE-001',type:'Предмет',next:1};
      const full=loadout('full'),start=loadout('start');
      const twoMissing={recipe:[{id:'FIXTURE-PASSIVE-A',kind:'Passive',minimumLevel:1},{id:'FIXTURE-PASSIVE-B',kind:'Passive',minimumLevel:1}]};
      const threeMissing={recipe:[...twoMissing.recipe,{id:'FIXTURE-PASSIVE-C',kind:'Passive',minimumLevel:1}]};
      const slotMatrix=[];
      for(const kind of ['ActiveSkill','Passive'])for(let free=0;free<=6;free++)for(let missing=0;missing<=6;missing++){
        const occupied=Array.from({length:6-free},(_,i)=>[`FIXTURE-OWNED-${i}`,1]);
        const def={recipe:Array.from({length:missing},(_,i)=>({id:`FIXTURE-MISSING-${i}`,kind,minimumLevel:1}))};
        const snapshot={skills:kind==='ActiveSkill'?occupied:[],items:kind==='Passive'?occupied:[],levels:{}};
        slotMatrix.push(isMissed(def,snapshot)===(missing>free));
      }
      return {
        readyVisible:relatedRecipes(rock,{...full,acquired:[]}).map(x=>x.def.id),
        acquiredHidden:relatedRecipes(rock,full).length===0,
        slotBlockedHidden:relatedRecipes(orbit,{...full,acquired:[]}).length===0,
        missingUnavailableHidden:relatedRecipes(heart,{...start,unavailable:['PASSIVE-002']}).length===0,
        closedSetHidden:relatedRecipes(rock,{...full,acquired:[],setPool:[]}).length===0,
        excludedSetHidden:relatedRecipes(rock,{...full,acquired:[],unavailable:['SET-001']}).length===0,
        ownedBelowThresholdReachable:!isMissed(sets.get('SET-006'),{...full,unavailable:['PASSIVE-001']}),
        twoMissingOneSlotBlocked:isMissed(twoMissing,{...full,items:full.items.slice(0,5)}),
        twoMissingTwoSlotsReachable:!isMissed(twoMissing,{...full,items:full.items.slice(0,4)}),
        threeMissingTwoSlotsBlocked:isMissed(threeMissing,{...full,items:full.items.slice(0,4)}),
        all98SlotComparisons:slotMatrix.length===98&&slotMatrix.every(Boolean)
      };
    });
    assert.deepEqual(eligibility.readyVisible,['SET-001']);
    assert.ok(Object.entries(eligibility).filter(([k])=>k!=='readyVisible').every(([,v])=>v),JSON.stringify(eligibility));
    // DOM-level empty result: one excluded absent component hides both count and panel.
    await open('book');
    const noRecipe=await page.evaluate(()=>{
      const original=draftSnapshot;
      try{
        draftSnapshot=()=>({...original(),unavailable:['PASSIVE-002']});render();inspectCard(2);
        return {count:document.querySelector('[data-inspect="2"] .recipe-count').textContent,hidden:document.querySelector('#recipe-inspector').hidden};
      }finally{draftSnapshot=original;render();}
    });
    assert.deepEqual(noRecipe,{count:'Сеты: 0',hidden:true});
    checks.push('Draft filters acquired, closed/excluded sets and unreachable recipes; missing components vs free slots; owned below threshold remains reachable; zero-count inspector hidden');
    const speed=await page.evaluate(()=>[movementPercent(commonSpeed),movementPercent(commonSpeed*1.2),movementPercent(0),movementPercent(commonSpeed*1.5)]);
    assert.deepEqual(speed,[100,120,0,150],'One shared baseline, not selected hero starting speed');
    const invalidSpeed=await page.evaluate(()=>{
      const original=commonSpeed,results=[];
      try{
        for(const bad of [0,-1,Infinity,NaN]){commonSpeed=bad;try{movementPercent(3);results.push(false);}catch{results.push(true);}}
        commonSpeed=original;
        for(const bad of [-1,Infinity,NaN]){try{movementPercent(bad);results.push(false);}catch{results.push(true);}}
      }finally{commonSpeed=original;}
      return results;
    });
    assert.ok(invalidSpeed.every(Boolean),'Invalid stats never silently use the selected hero as baseline');
    await open('settings');assert.equal(await page.locator('#mouse-mode').isChecked(),false);
    await page.locator('#mouse-mode').focus();await page.keyboard.press('Space');assert.equal(await page.locator('#mouse-mode').isChecked(),true);assert.equal(await page.locator('#stage').getAttribute('data-screen'),'settings');
    await open('draft');await page.locator('[data-action=banish]').click();assert.equal(await page.locator('[data-action=reroll]').isDisabled(),true);
    let initial=await count();await page.locator('[data-inspect="1"]').click();assert.equal(await count(),initial);assert.equal(await page.locator('[data-confirm="1"]').innerText(),'Исключить');await page.locator('[data-confirm="1"]').click();assert.equal(await count(),initial+1);
    await page.locator('[data-action=banish]').click();assert.equal(await page.locator('[data-action=reroll]').isEnabled(),true);
    await open('hud');await page.locator('#stage').click({position:{x:20,y:200}});await page.keyboard.press('Space');assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause');await page.locator('#stage').click({button:'right',position:{x:20,y:200}});assert.equal(await page.locator('#stage').getAttribute('data-screen'),'hud');
    await page.keyboard.press('Escape');assert.equal(await page.locator('#stage').getAttribute('data-screen'),'pause');await page.keyboard.press('Escape');assert.equal(await page.locator('#stage').getAttribute('data-screen'),'hud');
    checks.push('Shared baseline 100/120/150%; keyboard default, checkbox Space, Banish inspect/confirm, Space/RMB pause');
    assert.deepEqual(errors,[],'Browser errors');
    const result={result:'PASS',scope:'HTML composition only; not Unity verification',checks,captureCount:captures.length,captures,output};
    fs.writeFileSync(path.join(output,'summary.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));
  }finally{await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
