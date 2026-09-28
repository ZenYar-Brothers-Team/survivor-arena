/* Composition-only review. No Unity connection, persistence, or gameplay simulation. */
'use strict';
const root = '../../../../Assets/Resources/';
const stage = document.querySelector('#stage');
const overlay = document.querySelector('#overlay');
const art = path => root + 'Art/' + path;
const icon = id => art(`UI/Icons/${id.startsWith('SKILL') ? 'Skills' : id.startsWith('PASSIVE') ? 'Passives' : 'Sets'}/${id.toLowerCase()}-icon.png`);
const state = { screen: 'hud', width: 1280, full: true, dense: false, mouse: false, banish: false };
const active = [['SKILL-001',3],['SKILL-002',3],['SKILL-003',3],['SKILL-004',3],['SKILL-006',2],['SKILL-007',2]];
const passive = [['PASSIVE-001',2],['PASSIVE-002',3],['PASSIVE-004',2],['PASSIVE-008',3],['PASSIVE-009',2],['PASSIVE-011',2]];
const levels = Object.fromEntries([...active,...passive]);
const escapeHtml = value => String(value).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
let catalog = new Map();
let sets = new Map();
let detailTimer;
const notes = {
  hud: ['Поле остаётся главным', 'Здоровье рядом с Клёпкой. Таймер сверху, компактный билд и XP снизу; нет общей тяжёлой подложки и кнопок скорости. Сцена собрана из настоящего арта FIELD-001, но не является кадром работающей игры.'],
  draft: ['Сначала — эффект выбора', 'Три равные карточки. Изменения и смысл выбора видны сразу; рецепты раскрываются по запросу без перемещения карточек. Здесь показан отдельный пример до получения Тяжёлого боезапаса. Завершение рецепта не означает получение сета.'],
  book: ['Та же структура, другой источник', 'Книга меняет заголовок и небольшой акцент. Это отдельный ранний пример с новым Бумерангом и свободными слотами, не продолжение заполненного билда. Базовый урон скрыт; усиление урона показано в процентах.'],
  pause: ['Билд виден целиком', 'Персонаж слева, все 6 + 6 слотов в центре, рецепты справа. Прокручивается только список рецептов; нижние действия остаются на месте. Полученный сет отделён от доступных и упущенных рецептов.'],
  settings: ['Управление не меняет навигацию', 'Показан только фрагмент будущего экрана настроек. Клавиатура выбрана по умолчанию; галочка включает мышь. Настоящее поведение и сохранение реализует параллельная сессия. Этот макет не управляет персонажем.']
};
function name(id) { return catalog.get(id)?.displayName || id; }
function img(id, cls='') { return `<img class="${cls}" src="${icon(id)}" alt="">`; }
function fit() {
  const viewport = document.querySelector('#viewport');
  const scale = Math.min(1, (document.documentElement.clientWidth - 32) / state.width);
  viewport.style.width = `${state.width * scale}px`;
  viewport.style.height = `${state.width * 9 / 16 * scale}px`;
  stage.style.transform = `scale(${scale})`;
}
function world() {
  const sprites = [
    ['Fields/field-001/field-001-stump-prop.png',18,35,130],
    ['Fields/field-001/field-001-barrel-prop.png',82,60,115],
    ['Fields/field-001/field-001-fence-prop.png',72,19,210],
    ['Fields/field-001/field-001-bush-prop.png',9,14,100],
    ['Fields/field-001/field-001-grass-prop.png',36,70,78],
    ['Fields/field-001/field-001-grass-prop.png',60,28,65],
    ['Fields/field-001/field-001-grass-prop.png',92,82,76],
    ['Fields/field-001/field-001-grass-prop.png',31,19,60]
  ];
  const enemyCount = state.dense ? 48 : 9;
  for (let i=0;i<enemyCount;i++) {
    const angle=i*2.399963, radius=state.dense ? 14+(i%7)*4.6 : 25+(i%3)*9;
    sprites.push([`Enemies/enemy-00${i%3===0 ? 2 : 1}/enemy-00${i%3===0 ? 2 : 1}-body.png`,50+Math.cos(angle)*radius,50+Math.sin(angle)*radius*.78, i%3===0?118:112]);
  }
  for(let i=0;i<11;i++) sprites.push(['Pickups/xp-pickup/xp-pickup.png',33+(i*7)%39,39+(i*11)%28,22]);
  document.querySelector('#world').innerHTML=sprites.map(([path,x,y,w])=>`<img class="world-prop" src="${art('Sprites/'+path)}" alt="" style="left:${x}%;top:${y}%;width:${w}px;height:${w}px">`).join('');
}
function slot([id,level]=[]) { return id ? `<span class="icon-slot">${img(id)}<b>${level}</b></span>` : '<span class="icon-slot empty"></span>'; }
function renderHud() {
  const skills=state.full?active:[['SKILL-001',1]], items=state.full?passive:[];
  document.querySelector('#hud').innerHTML=`<div class="timer"><strong>${state.full?'07:42':'14:56'}</strong><small>${state.full?'ВОЛНА 9 / 16':'ВОЛНА 1 / 16'}</small></div>
    <button class="pause-button" data-action="pause">Пауза <small>Esc · Пробел · ПКМ</small></button>
    <div class="mode-hint">${state.mouse?'Управление мышью':'WASD / стрелки'}<br>Настройки — в паузе</div>
    <div class="build-hud"><div class="hud-row"><span class="hud-row-label">УМЕНИЯ</span>${Array.from({length:6},(_,i)=>slot(skills[i])).join('')}</div><div class="hud-row"><span class="hud-row-label">ПРЕДМЕТЫ</span>${Array.from({length:6},(_,i)=>slot(items[i])).join('')}</div></div>
    ${state.full?`<div class="sets-hud"><small>ПОЛУЧЕНО</small><span class="icon-slot">${img('SET-001')}</span></div>`:''}
    <div class="xp-hud"><span>Ур. ${state.full?'28':'1'}</span><div class="xp-line"><i style="width:${state.full?62:18}%"></i></div></div>`;
  const hp = document.querySelector('.hero-hp');
  hp.setAttribute('aria-valuemax',state.full?'116':'100');
  hp.setAttribute('aria-valuenow',state.full?'84':'100');
  hp.firstElementChild.style.width=state.full?'72.4%':'100%';
}
function componentMarkup(setId, projections={}) {
  return sets.get(setId).recipe.map(({id,minimumLevel})=>{
    const actual=levels[id]||0, next=projections[id]??actual;
    return `<span class="${next>=minimumLevel?'fulfilled':'pending'}">${next>=minimumLevel?'✓':'○'} ${escapeHtml(name(id))} ${next===actual?actual:`${actual} → ${next}`} / ${minimumLevel}</span>`;
  }).join('');
}
function damageIncreasePercent(id, from, to) {
  const skill=catalog.get(id), before=skill.levels[from-1].baseDamage, after=skill.levels[to-1].baseDamage;
  if(!(before>0)||!Number.isFinite(after))throw Error('Invalid damage comparison in mock: '+id);
  return Math.round((after/before-1)*100);
}
function options(book) {
  return book ? [
    {id:'SKILL-006',type:'Умение',level:'Новое',effect:'Летит к врагу и возвращается.<br>Срабатывает <strong>каждые 2,8 с</strong>',recipe:'Есть свободный слот',detail:'Базовая дальность 1,6. Повторное попадание в ту же цель — не чаще раза в 1 с. Отбрасывание 0,30; радиус попадания 0,16. На новом профиле нет открытых связанных рецептов.'},
    {id:'SKILL-001',type:'Умение',level:'1 → 2',effect:`<strong>+${damageIncreasePercent('SKILL-001',1,2)}% урона</strong><br>Отбрасывание: <strong>+20%</strong>`,recipe:'Тяжёлый боезапас · 0 / 3',detail:'Прирост урона относительно предыдущего уровня. Камень ещё ниже требуемого уровня 3; Точильный камень и Тяжёлый пояс в этом раннем примере не получены.'},
    {id:'PASSIVE-001',type:'Предмет',level:'Новое',effect:'Максимальное здоровье<br><strong>+8%</strong>',recipe:'Полевой медик · 0 / 3',detail:'Уровень 1 ниже порога 3. Для Полевого медика также нужны Собиратель ур. 3 и Лечебная настойка ур. 2. Новый предмет не завершает рецепт.'}
  ] : [
    {id:'SKILL-001',type:'Умение',level:'3 → 4',effect:'<strong>+1 рикошет</strong> каждого камня.<br>Рикошет сохраняет <strong>80%</strong><br>урона и отбрасывания.',recipe:'Тяжёлый боезапас · <em>3 / 3</em>',detail:'Рецепт уже выполнен; улучшение не выдаёт сет. Сам сет нужно выбрать отдельной карточкой.',set:'SET-001'},
    {id:'PASSIVE-001',type:'Предмет',level:'2 → 3',effect:'Максимальное здоровье<br><strong>+16% → +24%</strong>',recipe:'<em>Завершает рецепт</em><br>Полевой медик · 2 / 3 → 3 / 3',detail:'Рецепт будет выполнен. Полевой медик не выдаётся сразу: нужно дождаться и выбрать его карточку.',set:'SET-006',projection:{'PASSIVE-001':3}},
    {id:'SET-001',type:'Сет',level:'Не занимает слот',effect:'Бросок камня:<br><strong>+60% урона · +25% размера<br>+35% отбрасывания</strong>',recipe:'Камень 3 · Точильный камень 2<br>Тяжёлый пояс 2 · <em>готово</em>',detail:'Получение усиливает только Бросок камня. Не занимает слот умения или предмета.',set:'SET-001'}
  ];
}
function renderDraft(book) {
  overlay.innerHTML=`<header class="screen-heading"><div class="eyebrow ${book?'book-title':''}">${book?'Книга странника':'Новый уровень'}</div><h1>${book?'Ещё одна возможность':'Выбери улучшение'}</h1><p>${book?'Выбор 1 из 2':'Игра приостановлена'}</p></header>
    <div class="draft-grid ${state.banish?'banish':''}">${options(book).map((o,i)=>`<article class="draft-card ${o.type==='Сет'?'set-card':o.type==='Предмет'?'passive-card':''}"><button class="card-main" data-choice="${i}"><span class="card-top">${img(o.id,'card-icon')}<span><span class="type-label">${o.type.toUpperCase()}</span><span class="level-tag">${o.level}</span></span></span><h2>${escapeHtml(name(o.id))}</h2><span class="card-effect">${o.effect}</span><span class="recipe-summary">${o.recipe}</span></button><button class="recipe-button" data-details="${i}" aria-expanded="false">${o.set?'Рецепт и компоненты':'Подробности'} <span aria-hidden="true">↗</span></button></article>`).join('')}</div>
    <section class="details-popover" id="details" aria-label="Подробности варианта" hidden></section>
    <footer class="draft-actions"><button data-action="reroll" ${state.banish?'disabled':''}>Перебросить <span class="count">3</span></button><button data-action="banish" class="${state.banish?'danger':''}">${state.banish?'Отменить исключение':'Исключить <span class="count">2</span>'}</button><span class="hint">${state.banish?'Выбери, что исключить':'ЛКМ / Enter — выбор'}</span></footer>`;
}
function miniSlot([id,level]=[]) { return id?`<button class="mini-slot" data-item="${id}" title="${escapeHtml(name(id))}, уровень ${level}">${img(id)}<span><span class="entry-name">${escapeHtml(name(id))}</span><small>Уровень ${level}</small></span></button>`:'<div class="mini-slot empty">Свободно</div>'; }
function recipeEntry(id,missed=false) {
  const def=sets.get(id), fulfilled=def.recipe.filter(x=>(levels[x.id]||0)>=x.minimumLevel).length;
  return `<div class="recipe-entry"><h3>${escapeHtml(def.displayName)}<small>${fulfilled}/${def.recipe.length}</small></h3><ul>${def.recipe.map(({id,minimumLevel})=>`<li class="${(levels[id]||0)>=minimumLevel?'fulfilled':'pending'}">${(levels[id]||0)>=minimumLevel?'✓':'○'} ${escapeHtml(name(id))} ${levels[id]||0} / ${minimumLevel}</li>`).join('')}</ul>${missed?'<p class="pending" style="font-size:13px;margin-top:6px">Недостающий компонент не поместится</p>':''}</div>`;
}
function renderPause() {
  const skills=state.full?active:[['SKILL-001',1]],items=state.full?passive:[];
  overlay.innerHTML=`<header class="screen-heading"><div class="eyebrow">Забег приостановлен · ${state.full?'07:42':'14:56'}</div><h1>Передышка</h1></header>
    <div class="pause-layout"><section class="pane character-pane"><div class="section-label">ПЕРСОНАЖ</div><img class="character-portrait" src="${art('Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png')}" alt="Клёпка"><h2 class="character-name">Клёпка</h2><p class="character-subtitle">Уровень ${state.full?'28':'1'}</p><div class="stat-row"><span>Здоровье</span><strong>${state.full?'84 / 116':'100 / 100'}</strong></div><div class="hp-stat"><i style="width:${state.full?72.4:100}%"></i></div><div class="stat-row"><span>Скорость</span><b>3,0</b></div><div class="stat-row"><span>Регенерация</span><b>${state.full?'0,42':'0'} / с</b></div><div class="stat-row"><span>Входящий урон</span><b>${state.full?'−13%':'100%'}</b></div></section>
    <section class="pane build-pane"><div class="section-label">УМЕНИЯ · ${skills.length} / 6</div><div class="mini-grid">${Array.from({length:6},(_,i)=>miniSlot(skills[i])).join('')}</div><div class="section-label">ПРЕДМЕТЫ · ${items.length} / 6</div><div class="mini-grid">${Array.from({length:6},(_,i)=>miniSlot(items[i])).join('')}</div>${state.full?`<div class="acquired">${img('SET-001')}<div><div class="eyebrow">Полученный сет</div><h3>${escapeHtml(name('SET-001'))}</h3><p>Камень: +60% урона · +25% размера<br>+35% отбрасывания</p></div></div>`:'<p class="pending" style="font-size:15px">Полученные сеты появятся здесь.</p>'}</section>
    <section class="pane recipe-pane"><div class="section-label">ПРОГРЕСС РЕЦЕПТОВ</div>${state.full?recipeEntry('SET-006')+'<div class="missed-label">УПУЩЕННЫЕ · СЛОТЫ ЗАПОЛНЕНЫ</div>'+recipeEntry('SET-004',true)+recipeEntry('SET-010',true):`<div class="recipe-entry"><h3>Тяжёлый боезапас <small>0/3</small></h3><ul><li>○ Бросок камня 1 / 3</li><li>○ Точильный камень 0 / 2</li><li>○ Тяжёлый пояс 0 / 2</li></ul></div>`}</section></div>
    <div class="pause-description" hidden></div><footer class="pause-footer"><button class="primary" data-action="resume">Продолжить</button><button data-action="settings">Настройки</button><button class="danger" data-action="quit">Завершить забег</button></footer>`;
}
function renderSettings() {
  overlay.innerHTML=`<header class="screen-heading"><div class="eyebrow">Настройки · фрагмент макета</div><h1>Управление</h1></header><section class="pane settings-panel"><div class="section-label">ПЕРЕМЕЩЕНИЕ</div><label class="setting-toggle"><input id="mouse-mode" type="checkbox" ${state.mouse?'checked':''}>Управление мышью</label><p>${state.mouse?'Выбран режим мыши. Точное поведение курсора задаёт система управления.':'По умолчанию — клавиатура: WASD или стрелки.'}</p><dl><dt>Пауза</dt><dd>Esc · Пробел · ПКМ</dd><dt>Выбор в меню</dt><dd>ЛКМ · Enter</dd></dl><p class="note">Переключатель меняет только этот макет. Звук, видео и остальные настройки здесь намеренно не дублируются.</p></section><footer class="pause-footer"><button class="primary" data-action="back">Назад к паузе</button></footer>`;
}
function render(screen=state.screen) {
  clearTimeout(detailTimer);
  state.screen=screen;stage.dataset.screen=screen;stage.dataset.resolution=state.width;
  document.querySelectorAll('[data-screen]').forEach(el=>{if(el.tagName==='BUTTON')el.setAttribute('aria-pressed',el.dataset.screen===screen)});
  overlay.hidden=screen==='hud';
  renderHud();
  if(screen==='draft'||screen==='book')renderDraft(screen==='book');
  else if(screen==='pause')renderPause();else if(screen==='settings')renderSettings();else overlay.innerHTML='';
  document.querySelector('#review-heading').textContent=notes[screen][0];
  document.querySelector('#review-description').textContent=notes[screen][1];
  fit();
}
function showDetails(index) {
  const o=options(state.screen==='book')[index],el=document.querySelector('#details');
  if(!el)return;
  el.innerHTML=`<button class="detail-close" data-action="close-details" aria-label="Закрыть подробности">×</button><h3>${escapeHtml(o.set?name(o.set):name(o.id))}</h3><p>${o.detail}</p>${o.set?`<div class="component-list">${componentMarkup(o.set,o.projection)}</div>`:''}`;
  el.hidden=false;
  document.querySelectorAll('[data-details]').forEach(b=>b.setAttribute('aria-expanded',Number(b.dataset.details)===index));
}
function closeDetails(){const el=document.querySelector('#details');if(el)el.hidden=true;document.querySelectorAll('[data-details]').forEach(b=>b.setAttribute('aria-expanded','false'));}
function toast(text){document.querySelector('.toast')?.remove();const el=document.createElement('div');el.className='toast';el.setAttribute('role','status');el.textContent=text;stage.append(el);setTimeout(()=>el.remove(),3200);}
document.addEventListener('click',e=>{
  const b=e.target.closest('button');if(!b)return;
  if(b.dataset.screen){state.banish=false;render(b.dataset.screen);return;}
  if(b.dataset.details!==undefined){showDetails(Number(b.dataset.details));return;}
  if(b.dataset.choice!==undefined){const o=options(state.screen==='book')[Number(b.dataset.choice)];toast(`Макет: ${state.banish?'исключение':'выбор'} «${name(o.id)}». Данные игры не меняются.`);return;}
  if(b.dataset.item){const box=document.querySelector('.pause-description');box.hidden=false;box.textContent=`${name(b.dataset.item)} · уровень ${levels[b.dataset.item]||1}. Детальная карточка предмета — следующий presentation state.`;return;}
  switch(b.dataset.action){
    case 'pause':render('pause');break;case 'resume':render('hud');break;case 'settings':render('settings');break;case 'back':render('pause');break;
    case 'close-details':closeDetails();break;case 'banish':state.banish=!state.banish;render();break;
    case 'reroll':toast('Макет: переброс предложений. Случайный пул здесь не моделируется.');break;
    case 'quit':toast('В игре это действие ведёт к результатам. Макет не завершает настоящий забег.');break;
  }
});
overlay.addEventListener('pointerover',e=>{const b=e.target.closest('.card-main');if(b){clearTimeout(detailTimer);detailTimer=setTimeout(()=>showDetails(Number(b.dataset.choice)),450);}});
overlay.addEventListener('pointerout',e=>{if(e.target.closest('.card-main'))clearTimeout(detailTimer);});
overlay.addEventListener('focusin',e=>{if(e.target.matches('.card-main'))showDetails(Number(e.target.dataset.choice));});
document.addEventListener('keydown',e=>{
  if(!['Escape','Space'].includes(e.code)||e.repeat)return;
  // Never run pause bindings on a focused control: avoid pause + checkbox/button submit.
  if(e.target.closest('button,input,select'))return;
  if(state.screen==='hud'){e.preventDefault();render('pause');}
  else if(state.screen==='pause'){e.preventDefault();render('hud');}
  else if(e.code==='Escape')closeDetails();
});
stage.addEventListener('contextmenu',e=>{e.preventDefault();if(state.screen==='hud')render('pause');else if(state.screen==='pause')render('hud');});
document.querySelector('#resolution').addEventListener('change',e=>{state.width=Number(e.target.value);render();});
document.querySelector('#loadout').addEventListener('change',e=>{state.full=e.target.value==='full';render();});
document.querySelector('#density').addEventListener('click',e=>{state.dense=!state.dense;e.currentTarget.setAttribute('aria-pressed',state.dense);world();});
overlay.addEventListener('change',e=>{if(e.target.id==='mouse-mode'){state.mouse=e.target.checked;renderSettings();document.querySelector('#mouse-mode').focus();}});
window.addEventListener('resize',fit);
async function boot(){
  const paths=['ActiveSkills/ProductionActiveSkills.json','Passives/ProductionPassives.json','Sets/ProductionSets.json'];
  const data=await Promise.all(paths.map(async p=>{const r=await fetch(root+'Content/'+p);if(!r.ok)throw Error(p+': '+r.status);return r.json();}));
  catalog=new Map(data.flat().map(x=>[x.id,x]));sets=new Map(data[2].map(x=>[x.id,x]));
  world();render();await document.fonts.ready;document.body.dataset.ready='true';
}
boot().catch(error=>{document.querySelector('#review-description').textContent='Не удалось загрузить ресурсы. Запустите serve-preview.cjs по README. '+error.message;console.error(error);});
