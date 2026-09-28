/* Composition-only review. No Unity connection, persistence, or gameplay simulation. */
'use strict';
const root = '../../../../Assets/Resources/';
const stage = document.querySelector('#stage');
const overlay = document.querySelector('#overlay');
const art = path => root + 'Art/' + path;
const icon = id => art(`UI/Icons/${id.startsWith('SKILL') ? 'Skills' : id.startsWith('PASSIVE') ? 'Passives' : 'Sets'}/${id.toLowerCase()}-icon.png`);
const state = { screen:'hud', width:1280, loadout:'full', dense:false, mouse:false, banish:false, stress:false, inspected:0, recipe:null, confirmCount:0 };
const active = [['SKILL-001',3],['SKILL-002',3],['SKILL-003',3],['SKILL-004',3],['SKILL-006',2],['SKILL-007',2]];
const passive = [['PASSIVE-001',2],['PASSIVE-002',3],['PASSIVE-004',2],['PASSIVE-008',3],['PASSIVE-009',2],['PASSIVE-011',2]];
const midActive = [['SKILL-001',3],['SKILL-003',3],['SKILL-004',3],['SKILL-007',2],['SKILL-013',3]];
const midPassive = [['PASSIVE-001',2],['PASSIVE-002',3],['PASSIVE-009',2],['PASSIVE-011',2]];
const startupSetIds = ['SET-001','SET-004','SET-006','SET-010','SET-017'];
const escapeHtml = value => String(value).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
let catalog = new Map(), sets = new Map(), commonSpeed = 0, characterSpeed = 0;
let setInfoTrigger = null;
// Short UI copy from the full Approved cards; no new mechanics or raw damage values.
const setEffects = {
  'SET-001':'Бросок камня: +60% урона. Камни крупнее и сильнее отбрасывают.',
  'SET-004':'Замедленных врагов сильнее отбрасывает. Импульсная волна шире и отбрасывает сильнее.',
  'SET-006':'Больше здоровья, сильнее регенерация и лечение. Зелья лечения выпадают чаще.',
  'SET-010':'Орбита замедляет врагов. Клинки наносят на 40% больше урона замедленным целям.',
  'SET-017':'Периодически отмечает область и обрушивает на неё мощный удар.'
};
const notes = {
  hud:['Меньше интерфейса — больше поля','Убраны подсказки, номер волны и кнопка паузы. В 720p слоты и длина XP-полосы уменьшены на 40%; 1080p сохраняет прежний масштаб. Пауза — Esc, пробел или ПКМ.'],
  draft:['Сначала изучить, затем выбрать','Карточка только закрепляет вариант для просмотра. «Выбрать» — отдельное действие. Снизу прокручивается список только тех связанных сетов, которые ещё можно собрать. Справа — статус и компоненты выбранного рецепта.'],
  book:['Та же структура, другой источник','Отдельный ранний пример со свободными слотами. Урон — в процентах; вторичные коэффициенты заменены короткими формулировками. Первая карточка не участвует в открытых рецептах стартового профиля.'],
  pause:['Все сеты — в одном месте','Справа общий прокручиваемый список: компактные «Получены», рецепты в 3/2 колонки и компактные «Упущены». Нажмите на любой сет, чтобы посмотреть краткий эффект. Слева остаются персонаж и все 6+6 слотов.'],
  settings:['Управление не меняет навигацию','Фрагмент Settings: клавиатура по умолчанию, мышь включается галочкой. Настоящие movement, bindings и сохранение реализованы отдельно; макет их не меняет.']
};
function name(id) { return catalog.get(id)?.displayName || id; }
function img(id, cls='') { return `<img class="${cls}" src="${icon(id)}" alt="">`; }
function loadout(mode=state.loadout) {
  const skills=mode==='full'?active:mode==='mid'?midActive:[['SKILL-001',1]];
  const items=mode==='full'?passive:mode==='mid'?midPassive:[];
  return {skills,items,levels:Object.fromEntries([...skills,...items]),acquired:mode==='full'?['SET-001']:[]};
}
function movementPercent(currentSpeed) {
  if(!Number.isFinite(commonSpeed)||!(commonSpeed>0)||!Number.isFinite(currentSpeed)||currentSpeed<0)throw Error('Invalid baseline-relative movement');
  return Math.round(currentSpeed/commonSpeed*100);
}
function fit() {
  const viewport=document.querySelector('#viewport'), scale=Math.min(1,(document.documentElement.clientWidth-32)/state.width);
  viewport.style.width=`${state.width*scale}px`;viewport.style.height=`${state.width*9/16*scale}px`;
  stage.style.transform=`scale(${scale})`;
}
function world() {
  const sprites=[
    ['Fields/field-001/field-001-stump-prop.png',18,35,130],['Fields/field-001/field-001-barrel-prop.png',82,60,115],
    ['Fields/field-001/field-001-fence-prop.png',72,19,210],['Fields/field-001/field-001-bush-prop.png',9,14,100],
    ['Fields/field-001/field-001-grass-prop.png',36,70,78],['Fields/field-001/field-001-grass-prop.png',60,28,65],
    ['Fields/field-001/field-001-grass-prop.png',92,82,76],['Fields/field-001/field-001-grass-prop.png',31,19,60]
  ];
  for(let i=0;i<(state.dense?48:9);i++) {
    const angle=i*2.399963,radius=state.dense?14+(i%7)*4.6:25+(i%3)*9,enemy=i%3===0?2:1;
    sprites.push([`Enemies/enemy-00${enemy}/enemy-00${enemy}-body.png`,50+Math.cos(angle)*radius,50+Math.sin(angle)*radius*.78,enemy===2?118:112]);
  }
  for(let i=0;i<11;i++)sprites.push(['Pickups/xp-pickup/xp-pickup.png',33+(i*7)%39,39+(i*11)%28,22]);
  document.querySelector('#world').innerHTML=sprites.map(([p,x,y,w])=>`<img class="world-prop" src="${art('Sprites/'+p)}" alt="" style="left:${x}%;top:${y}%;width:${w}px;height:${w}px">`).join('');
}
function slot([id,level]=[]) { return id?`<span class="icon-slot">${img(id)}<b>${level}</b></span>`:'<span class="icon-slot empty"></span>'; }
function renderHud() {
  const {skills,items,acquired}=loadout(),start=state.loadout==='start';
  document.querySelector('#hud').innerHTML=`<div class="timer"><strong>${start?'14:56':'07:42'}</strong></div>
    <div class="build-hud"><div class="hud-row"><span class="hud-row-label">УМЕНИЯ</span>${Array.from({length:6},(_,i)=>slot(skills[i])).join('')}</div><div class="hud-row"><span class="hud-row-label">ПРЕДМЕТЫ</span>${Array.from({length:6},(_,i)=>slot(items[i])).join('')}</div></div>
    ${acquired.length?`<div class="sets-hud"><small>ПОЛУЧЕНО</small>${acquired.map(id=>`<span class="icon-slot">${img(id)}</span>`).join('')}</div>`:''}
    <div class="xp-hud"><span>Ур. ${start?'1':state.loadout==='mid'?'22':'28'}</span><div class="xp-line"><i style="width:${start?18:62}%"></i></div></div>`;
  const hp=document.querySelector('.hero-hp');hp.setAttribute('aria-valuemax',start?'100':'116');hp.setAttribute('aria-valuenow',start?'100':'84');hp.firstElementChild.style.width=start?'100%':'72.4%';
}
function damageIncreasePercent(id,from,to) {
  const skill=catalog.get(id),before=skill.levels[from-1].baseDamage,after=skill.levels[to-1].baseDamage;
  if(!(before>0)||!Number.isFinite(after))throw Error('Invalid damage comparison in mock: '+id);
  return Math.round((after/before-1)*100);
}
function options(book=state.screen==='book') {
  return book?[
    {id:'SKILL-006',type:'Умение',level:'Новое',next:1,effect:'Летит к врагу и возвращается.<br>Срабатывает <strong>каждые 2,8 с</strong>'},
    {id:'SKILL-001',type:'Умение',level:'1 → 2',next:2,effect:`<strong>+${damageIncreasePercent('SKILL-001',1,2)}% урона</strong><br>Сильнее отбрасывает.`},
    {id:'PASSIVE-001',type:'Предмет',level:'Новое',next:1,effect:'Максимальное здоровье<br><strong>+8%</strong>'}
  ]:[
    {id:'SKILL-001',type:'Умение',level:'3 → 4',next:4,effect:'<strong>+1 рикошет.</strong><br>Повторный удар слабее.'},
    {id:'PASSIVE-001',type:'Предмет',level:'2 → 3',next:3,effect:'Максимальное здоровье<br><strong>+16% → +24%</strong>'},
    {id:'SET-001',type:'Сет',level:'Не занимает слот',effect:'<strong>+60% урона камня.</strong><br>Камни крупнее и сильнее<br>отбрасывают.'}
  ];
}
// Layout fixtures only: no claim that one production skill belongs to all these recipes.
function fixtureRecipes(count,sourceIds=startupSetIds) {
  if(!sourceIds.length)return [];
  return Array.from({length:count},(_,i)=>({...sets.get(sourceIds[i%sourceIds.length]),
    id:`FIXTURE-UI-RECIPE-${String(i+1).padStart(2,'0')}`,iconId:sourceIds[i%sourceIds.length],
    displayName:`Тестовый рецепт ${i+1}${i===8?' с длинным названием':''}`,fixture:true}));
}
// Draft is a separate example immediately before obtaining SET-001.
function draftSnapshot() { return {...loadout(state.screen==='book'?'start':'full'),acquired:[]}; }
function draftLevels() { return draftSnapshot().levels; }
function recipeProgress(def,current,projection={},acquired=[]) {
  const before=def.recipe.filter(x=>(current[x.id]||0)>=x.minimumLevel).length;
  const after=def.recipe.filter(x=>(projection[x.id]??current[x.id]??0)>=x.minimumLevel).length;
  const total=def.recipe.length;
  const status=acquired.includes(def.id)?'Получен':before===total?'Рецепт готов':after===total?'Завершит рецепт':'В процессе';
  return {before,after,total,status,rank:status==='Завершит рецепт'?0:status==='Рецепт готов'?1:2,
    text:before===after?`${before}/${total}`:`${before}/${total} → ${after}/${total}`};
}
function relatedRecipes(option,snapshot=draftSnapshot()) {
  const current=snapshot.levels,projection=option.next?{[option.id]:option.next}:{};
  const candidates=(snapshot.setPool||startupSetIds).map(id=>sets.get(id)).filter(def=>def&&
    !snapshot.acquired.includes(def.id)&&!(snapshot.unavailable||[]).includes(def.id)&&!isMissed(def,snapshot)&&
    (option.type==='Сет'?def.id===option.id:def.recipe.some(c=>c.id===option.id)));
  // Density fixtures clone only reachable candidates, so they do not bypass this filter.
  const definitions=state.stress?fixtureRecipes(10,candidates.map(def=>def.id)):candidates;
  return definitions.map(def=>({def,progress:recipeProgress(def,current,projection)}))
    .sort((a,b)=>a.progress.rank-b.progress.rank||b.progress.after/b.progress.total-a.progress.after/a.progress.total||a.def.id.localeCompare(b.def.id));
}
function components(def,current,projection={}) {
  return def.recipe.map(({id,minimumLevel})=>{
    const before=current[id]||0,after=projection[id]??before;
    return `<div class="component ${after>=minimumLevel?'fulfilled':'pending'} ${after!==before?'projected':''}"><span>${after>=minimumLevel?'✓':'○'} ${escapeHtml(name(id))}</span><b>${after===before?before:`${before} → ${after}`} / ${minimumLevel}</b></div>`;
  }).join('');
}
function renderDraft(book) {
  const opts=options(book);state.inspected=Math.min(state.inspected,opts.length-1);
  overlay.innerHTML=`<header class="screen-heading"><div class="eyebrow ${book?'book-title':''}">${book?'Книга странника':'Новый уровень'}</div><h1>${book?'Ещё одна возможность':'Выбери улучшение'}</h1><p>${book?'Выбор 1 из 2':'Игра приостановлена'}</p></header>
    <div class="draft-grid ${state.banish?'banish':''}">${opts.map((o,i)=>`<article class="draft-card ${o.type==='Сет'?'set-card':o.type==='Предмет'?'passive-card':''}" data-card="${i}">
      <button class="card-main" data-inspect="${i}" aria-controls="recipe-inspector" aria-pressed="false"><span class="card-top">${img(o.id,'card-icon')}<span><span class="type-label">${o.type.toUpperCase()}</span><span class="level-tag">${o.level}</span></span></span><h2>${escapeHtml(name(o.id))}</h2><span class="card-effect">${o.effect}</span><span class="recipe-count">${o.type==='Сет'?'Рецепт: 3/3':`Сеты: ${relatedRecipes(o).length}`}</span></button>
      <button class="choose-card" data-confirm="${i}" disabled>${state.banish?'Исключить':'Выбрать'}</button></article>`).join('')}</div>
    <section id="recipe-inspector" class="recipe-inspector" aria-label="Связанные рецепты" hidden></section>
    <footer class="draft-actions"><button data-action="reroll" ${state.banish?'disabled':''}>Перебросить <span class="count">3</span></button><button data-action="banish" class="${state.banish?'danger':''}">${state.banish?'Отменить исключение':'Исключить <span class="count">2</span>'}</button></footer>`;
  inspectCard(state.inspected);
}
function inspectCard(index) {
  const changed=state.inspected!==index;state.inspected=index;if(changed)state.recipe=null;
  document.querySelectorAll('[data-card]').forEach(el=>el.classList.toggle('inspected',Number(el.dataset.card)===index));
  document.querySelectorAll('[data-inspect]').forEach(el=>el.setAttribute('aria-pressed',Number(el.dataset.inspect)===index));
  document.querySelectorAll('[data-confirm]').forEach(el=>{const selected=Number(el.dataset.confirm)===index;el.disabled=!selected;el.classList.toggle('primary',selected&&!state.banish);});
  renderInspector();
}
function renderInspector() {
  const panel=document.querySelector('#recipe-inspector'),option=options()[state.inspected],list=relatedRecipes(option);
  panel.hidden=list.length===0;if(!list.length){panel.innerHTML='';return;}
  if(!list.some(x=>x.def.id===state.recipe))state.recipe=list[0].def.id;
  panel.innerHTML=`<section class="recipe-picker"><div class="section-label">СВЯЗАННЫЕ СЕТЫ · ${list.length}</div><div class="recipe-picker-scroll" role="group" aria-label="Выбор рецепта">${list.map(({def,progress})=>`<button class="recipe-pick" data-recipe="${def.id}" aria-pressed="${def.id===state.recipe}">${img(def.iconId||def.id)}<span>${escapeHtml(def.displayName)}</span><b>${progress.text}</b></button>`).join('')}</div></section><section class="recipe-inspection"></section>`;
  renderSelectedRecipe();
}
function renderSelectedRecipe() {
  const option=options()[state.inspected],selected=relatedRecipes(option).find(x=>x.def.id===state.recipe);
  if(!selected)return;
  const {def,progress}=selected;
  document.querySelectorAll('[data-recipe]').forEach(el=>el.setAttribute('aria-pressed',el.dataset.recipe===state.recipe));
  document.querySelector('.recipe-inspection').innerHTML=`<header><h2>${escapeHtml(def.displayName)}</h2><span class="recipe-status ${progress.rank<2?'ready':''}">${progress.status}</span></header><p class="recipe-effect">${escapeHtml(setEffects[def.iconId||def.id])}</p><div class="component-list">${components(def,draftLevels(),option.next?{[option.id]:option.next}:{})}</div>`;
}
function miniSlot([id,level]=[]) {
  return id?`<button class="mini-slot" data-item="${id}" title="${escapeHtml(name(id))}, уровень ${level}"><span class="mini-icon">${img(id)}<b>${level}</b></span><span class="entry-name">${escapeHtml(name(id))}</span></button>`:'<div class="mini-slot empty">Свободно</div>';
}
function isMissed(def,snapshot) {
  // DECISION-0073: owned components can still be upgraded. Unavailable contains
  // absent IDs excluded by banish/pool/zero character weight in a test snapshot.
  const missing=def.recipe.filter(c=>!snapshot.levels[c.id]);
  return missing.some(c=>(snapshot.unavailable||[]).includes(c.id))||
    missing.filter(c=>c.kind==='ActiveSkill').length>6-snapshot.skills.length||
    missing.filter(c=>c.kind!=='ActiveSkill').length>6-snapshot.items.length;
}
function pauseRecipes(snapshot) {
  if(state.stress){const all=fixtureRecipes(20);return {available:all.slice(0,12),acquired:all.slice(12,16),missed:all.slice(16)};}
  const relevant=startupSetIds.map(id=>sets.get(id)).filter(def=>!snapshot.acquired.includes(def.id));
  const available=relevant.filter(def=>!isMissed(def,snapshot)&&def.recipe.some(c=>snapshot.levels[c.id])).sort((a,b)=>{
    const pa=recipeProgress(a,snapshot.levels),pb=recipeProgress(b,snapshot.levels);
    return pb.after/pb.total-pa.after/pa.total;
  });
  return {available,acquired:snapshot.acquired.map(id=>sets.get(id)),missed:relevant.filter(def=>isMissed(def,snapshot))};
}
function setInfoAttributes(def,status) {
  return `data-set-info="${def.id}" data-set-source="${def.iconId||def.id}" data-set-title="${escapeHtml(def.displayName)}" data-set-status="${status}" aria-haspopup="dialog" aria-label="О сете: ${escapeHtml(def.displayName)}"`;
}
function compactSets(definitions,received) {
  if(!definitions.length)return '';
  const kind=received?'received':'missed',title=received?'Получены':'Упущены';
  return `<section class="set-group ${kind}-section" aria-label="${title}"><h3>${title}<span>${definitions.length}</span></h3><div class="compact-set-list">${definitions.map(def=>`<button class="set-chip ${kind}-entry" ${setInfoAttributes(def,received?'Получен':'Упущен')}>${img(def.iconId||def.id)}<span>${escapeHtml(def.displayName)}</span></button>`).join('')}</div></section>`;
}
function recipeEntry(def,snapshot) {
  const p=recipeProgress(def,snapshot.levels);
  return `<button class="recipe-entry" ${setInfoAttributes(def,p.status)}><header>${img(def.iconId||def.id)}<h3>${escapeHtml(def.displayName)}</h3><b>${p.text}</b></header><span class="recipe-status ${p.rank<2?'ready':''}">${p.status}</span><div class="component-list">${components(def,snapshot.levels)}</div></button>`;
}
function renderPause() {
  const snapshot=loadout(),{skills,items}=snapshot,{available,acquired,missed}=pauseRecipes(snapshot),start=state.loadout==='start';
  overlay.innerHTML=`<header class="screen-heading"><div class="eyebrow">Забег приостановлен · ${start?'14:56':'07:42'}</div><h1>Передышка</h1></header>
    <div class="pause-layout"><section class="pane build-pane"><div class="character-row"><img class="character-portrait" src="${art('Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png')}" alt="Клёпка"><div><h2>Клёпка <small>Ур. ${start?'1':state.loadout==='mid'?'22':'28'}</small></h2><div class="health-label">Здоровье <b>${start?'100 / 100':'84 / 116'}</b></div><div class="hp-stat"><i style="width:${start?100:72.4}%"></i></div></div></div>
      <div class="stats-strip"><div><span>Скорость</span><b class="speed-value">${movementPercent(characterSpeed)}%</b></div><div><span>Регенерация</span><b>${start?'0':'0,42'} / с</b></div><div><span>Входящий урон</span><b>${state.loadout==='full'?'−13%':'100%'}</b></div></div>
      <div class="section-label">УМЕНИЯ · ${skills.length} / 6</div><div class="mini-grid">${Array.from({length:6},(_,i)=>miniSlot(skills[i])).join('')}</div>
      <div class="section-label">ПРЕДМЕТЫ · ${items.length} / 6</div><div class="mini-grid">${Array.from({length:6},(_,i)=>miniSlot(items[i])).join('')}</div>
      </section>
    <section class="pane recipe-pane" aria-label="Сеты"><header class="recipe-pane-header"><h2>Сеты</h2><span>${available.length+acquired.length+missed.length}</span></header><div class="recipe-scroll" tabindex="0">${compactSets(acquired,true)}
      ${available.length?`<h3 class="recipe-list-heading">Рецепты <span>${available.length}</span></h3><div class="recipe-grid">${available.map(def=>recipeEntry(def,snapshot)).join('')}</div>`:''}
      ${compactSets(missed,false)}</div></section></div>
    <div class="pause-description" hidden></div><footer class="pause-footer"><button class="primary" data-action="resume">Продолжить</button><button data-action="settings">Настройки</button><button class="danger" data-action="quit">Завершить забег</button></footer>`;
  document.querySelector('.recipe-scroll').addEventListener('scroll',()=>closeSetInfo(false),{passive:true});
}
function closeSetInfo(restoreFocus=true) {
  document.querySelector('#set-info-popup')?.remove();
  const trigger=setInfoTrigger;setInfoTrigger=null;
  if(restoreFocus&&trigger?.isConnected)trigger.focus({preventScroll:true});
}
function openSetInfo(trigger) {
  closeSetInfo(false);setInfoTrigger=trigger;
  const popup=document.createElement('aside');popup.id='set-info-popup';popup.className='set-popover';
  popup.setAttribute('role','dialog');popup.setAttribute('aria-labelledby','set-info-title');popup.setAttribute('aria-describedby','set-info-effect');
  popup.innerHTML=`<button class="set-info-close" data-action="close-set-info" aria-label="Закрыть описание">×</button><header>${img(trigger.dataset.setSource)}<div><div class="eyebrow">${escapeHtml(trigger.dataset.setStatus)}</div><h2 id="set-info-title">${escapeHtml(trigger.dataset.setTitle)}</h2></div></header><p id="set-info-effect">${escapeHtml(setEffects[trigger.dataset.setSource])}</p>`;
  stage.append(popup);
  const frame=stage.getBoundingClientRect(),anchor=trigger.getBoundingClientRect(),scale=frame.width/stage.clientWidth;
  const pane=document.querySelector('.recipe-pane').getBoundingClientRect(),footer=document.querySelector('.pause-footer').getBoundingClientRect();
  const left=Math.max((pane.left-frame.left)/scale+8,Math.min((anchor.left-frame.left+anchor.width/2)/scale-popup.offsetWidth/2,stage.clientWidth-popup.offsetWidth-16));
  const bottom=(footer.top-frame.top)/scale-12;
  let top=(anchor.bottom-frame.top)/scale+8;
  if(top+popup.offsetHeight>bottom)top=(anchor.top-frame.top)/scale-popup.offsetHeight-8;
  top=Math.max(16,Math.min(top,bottom-popup.offsetHeight));
  popup.style.left=`${left}px`;popup.style.top=`${top}px`;popup.querySelector('button').focus({preventScroll:true});
}
function renderSettings() {
  overlay.innerHTML=`<header class="screen-heading"><div class="eyebrow">Настройки · фрагмент макета</div><h1>Управление</h1></header><section class="pane settings-panel"><div class="section-label">ПЕРЕМЕЩЕНИЕ</div><label class="setting-toggle"><input id="mouse-mode" type="checkbox" ${state.mouse?'checked':''}>Управление мышью</label><p>${state.mouse?'Двигай курсор в нужную сторону. Подведи его к персонажу, чтобы остановиться.':'По умолчанию — клавиатура: WASD или стрелки.'}</p><dl><dt>Пауза</dt><dd>Esc · Пробел · ПКМ</dd><dt>Выбор в меню</dt><dd>ЛКМ · Enter</dd></dl><p class="note">Переключатель меняет только этот макет. Звук, видео и остальные настройки здесь намеренно не дублируются.</p></section><footer class="pause-footer"><button class="primary" data-action="back">Назад к паузе</button></footer>`;
}
function render(screen=state.screen) {
  closeSetInfo(false);
  if(screen!==state.screen){state.inspected=0;state.recipe=null;}
  state.screen=screen;stage.dataset.screen=screen;stage.dataset.resolution=state.width;stage.dataset.confirmCount=state.confirmCount;
  document.querySelectorAll('button[data-screen]').forEach(el=>el.setAttribute('aria-pressed',el.dataset.screen===screen));
  overlay.hidden=screen==='hud';renderHud();
  if(screen==='draft'||screen==='book')renderDraft(screen==='book');else if(screen==='pause')renderPause();else if(screen==='settings')renderSettings();else overlay.innerHTML='';
  document.querySelector('.preview-stamp').textContent=state.stress?'МАКЕТ · ТЕСТ ПЛОТНОСТИ · синтетические рецепты · не игровой контент':'МАКЕТ · production-арт · постановочная сцена · DEV скрыт';
  document.querySelector('#review-heading').textContent=notes[screen][0];document.querySelector('#review-description').textContent=notes[screen][1];fit();
}
function toast(text) {
  document.querySelector('.toast')?.remove();const el=document.createElement('div');el.className='toast';el.setAttribute('role','status');el.textContent=text;stage.append(el);setTimeout(()=>el.remove(),3200);
}
document.addEventListener('click',e=>{
  const popup=document.querySelector('#set-info-popup');
  if(popup&&!popup.contains(e.target)&&!e.target.closest('[data-set-info]')){
    closeSetInfo();e.preventDefault();return; // Dismissal never also resumes/quits/confirms.
  }
  const b=e.target.closest('button');if(!b||b.disabled)return;
  if(b.dataset.setInfo){openSetInfo(b);return;}
  if(b.dataset.action==='close-set-info'){closeSetInfo();return;}
  if(b.dataset.screen){state.banish=false;render(b.dataset.screen);return;}
  if(b.dataset.inspect!==undefined){inspectCard(Number(b.dataset.inspect));return;}
  if(b.dataset.recipe){state.recipe=b.dataset.recipe;renderSelectedRecipe();return;}
  if(b.dataset.confirm!==undefined){
    if(Number(b.dataset.confirm)!==state.inspected)return;
    const o=options()[state.inspected];state.confirmCount++;stage.dataset.confirmCount=state.confirmCount;
    toast(`Макет: ${state.banish?'исключение':'выбор'} «${name(o.id)}». Данные игры не меняются.`);return;
  }
  if(b.dataset.item){const box=document.querySelector('.pause-description');box.hidden=false;box.textContent=`${name(b.dataset.item)} · уровень ${loadout().levels[b.dataset.item]||1}`;return;}
  switch(b.dataset.action){
    case 'resume':render('hud');break;case 'settings':render('settings');break;case 'back':render('pause');break;
    case 'banish':state.banish=!state.banish;render();break;
    case 'reroll':state.inspected=0;state.recipe=null;render();toast('Макет: переброс предложений. Случайный пул здесь не моделируется.');break;
    case 'quit':toast('В игре это действие ведёт к результатам. Макет не завершает настоящий забег.');break;
  }
});
// Focus/hover never confirms or changes the pinned inspector. Activate the inspect button first.
document.addEventListener('keydown',e=>{
  if(document.querySelector('#set-info-popup')){if(e.code==='Escape'&&!e.repeat){e.preventDefault();closeSetInfo();}return;}
  if(!['Escape','Space'].includes(e.code)||e.repeat||(e.code==='Space'&&e.target.closest('button,input,select')))return;
  if(state.screen==='hud'){e.preventDefault();render('pause');}else if(state.screen==='pause'){e.preventDefault();render('hud');}
});
stage.addEventListener('contextmenu',e=>{e.preventDefault();if(document.querySelector('#set-info-popup')){closeSetInfo();return;}if(state.screen==='hud')render('pause');else if(state.screen==='pause')render('hud');});
document.querySelector('#resolution').addEventListener('change',e=>{state.width=Number(e.target.value);render();});
document.querySelector('#loadout').addEventListener('change',e=>{state.loadout=e.target.value;render();});
document.querySelector('#recipe-density').addEventListener('change',e=>{state.stress=e.target.value==='stress';state.recipe=null;render();});
document.querySelector('#density').addEventListener('click',e=>{state.dense=!state.dense;e.currentTarget.setAttribute('aria-pressed',state.dense);world();});
overlay.addEventListener('change',e=>{if(e.target.id==='mouse-mode'){state.mouse=e.target.checked;renderSettings();document.querySelector('#mouse-mode').focus();}});
window.addEventListener('resize',()=>{closeSetInfo(false);fit();});
async function boot() {
  const paths=['ActiveSkills/ProductionActiveSkills.json','Passives/ProductionPassives.json','Sets/ProductionSets.json','Characters/ProductionCharacterBaseline.json','Characters/ProductionCharacters.json'];
  const data=await Promise.all(paths.map(async p=>{const r=await fetch(root+'Content/'+p);if(!r.ok)throw Error(p+': '+r.status);return r.json();}));
  catalog=new Map(data.slice(0,3).flat().map(x=>[x.id,x]));sets=new Map(data[2].map(x=>[x.id,x]));
  commonSpeed=data[3].baseStats.movementSpeed;characterSpeed=data[4].find(x=>x.id==='CHAR-001').baseStats.movementSpeed;
  movementPercent(characterSpeed);world();render();await document.fonts.ready;document.body.dataset.ready='true';
}
boot().catch(error=>{document.querySelector('#review-description').textContent='Не удалось загрузить ресурсы. Запустите serve-preview.cjs по README. '+error.message;console.error(error);});
