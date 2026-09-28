'use strict';
// Read-only review fixture: no game/profile connection, purchases or persistence.
const body = '/Assets/Resources/Art/Sprites/Characters/';
const art = '/Assets/Resources/Art/Sprites/Fields/';
const skill = '/Assets/Resources/Art/UI/Icons/Skills/';
const characters = [
  { id:'CHAR-001', name:'Клёпка', image:body+'fixture-character-agile/fixture-character-agile-body.png',
    role:'Универсальный беглец с сильным броском камня.', skill:'Бросок камня', icon:skill+'skill-001-icon.png',
    boosts:['Урон камня +60%', 'Скорость использования +25%'], stats:[] },
  { id:'CHAR-002', name:'Бугор', image:body+'char-002/char-002-body.png',
    role:'Крепкий боец. Держит преследователей в ближней зоне.', skill:'Орбитальные клинки', icon:skill+'skill-003-icon.png',
    boosts:['Урон клинков +60%', 'Размер эффекта +25%'], stats:[['Здоровье','+20%'],['Скорость движения','−8%'],['Урон умений','+5%']] }
];
const fields = [
  { id:'FIELD-001', name:'Деревенская окраина', image:art+'field-001/field-001-background.png', difficulty:1, unlock:'' },
  { id:'FIELD-002', name:'Королевский тракт', image:art+'field-002/field-002-background.png', difficulty:1,
    unlock:'Пройдите «Деревенскую окраину»' },
  { id:'FIELD-003', name:'Пограничные руины', image:art+'field-003/field-003-background.png', difficulty:2,
    unlock:'Пройдите «Королевский тракт»' },
  { id:'FIELD-004', name:'Рыцарский лагерь', image:art+'field-004/field-004-background.png', difficulty:2, unlock:'Пройдите «Пограничные руины»' },
  { id:'FIELD-005', name:'Королевская столица', image:art+'field-005/field-005-background.png', difficulty:3, unlock:'Пройдите «Рыцарский лагерь»' },
  { id:'FIELD-006', name:'Академия магов', image:art+'field-006/field-006-background.png', difficulty:3, unlock:'Пройдите «Королевскую столицу»' },
  { id:'FIELD-007', name:'Монастырские сады', image:art+'field-007/field-007-background.png', difficulty:4, unlock:'Пройдите «Академию магов»' },
  { id:'FIELD-008', name:'Цитадель короны', image:art+'field-008/field-008-background.png', difficulty:4, unlock:'Пройдите «Монастырские сады»' },
  { id:'FIELD-009', name:'Небесные врата', image:art+'field-009/field-009-background.png', difficulty:5, unlock:'Пройдите «Цитадель короны»' },
  { id:'FIELD-010', name:'Чертог Спасения', image:art+'field-010/field-010-background.png', difficulty:5, unlock:'Пройдите «Небесные врата»' }
];
const requestedVariant = new URLSearchParams(location.search).get('menu');
const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
const state = { screen:'menu', width:1920, profile:'new', dense:false, character:0, field:0, characterInspection:0, fieldInspection:0,
  motion:new URLSearchParams(location.search).get('motion')!=='off',
  menuVariant:['A','B','C','D','E'].includes(requestedVariant)?requestedVariant:'A' };
const variantNotes = {
  A:'Сохранённый исходный вариант: игровой спрайт на иллюстрации поля.',
  B:'Одна иллюстрация без наложенного персонажа: акцент на мире.',
  C:'Персонаж на матовой подложке: акцент на герое, без смешения с пейзажем.',
  D:'Цельная сцена: сосредоточенный побег, без улыбок и чайника. Версия 3, preview — ещё не игровой ассет.',
  E:'Шепотка справа; пыль среднего размера перед обоими героями, усиленное движение лучей. Мышь добавляет параллакс. Preview, не Unity.'
};
const root = document.querySelector('#viewport');
let depthFrame=0, depthX=0, depthY=0, targetDepthX=0, targetDepthY=0;
const message = text => { document.querySelector('#review-message').textContent = text; };
const accessibleCharacter = index => index === 0 || (index === 1 && state.profile === 'owned');
const accessibleField = index => index === 0 || (index === 1 && state.profile !== 'new');
const charStatus = index => index === 0 ? 'Доступен' : state.profile === 'owned' ? 'Доступен' : state.profile === 'earned' ? 'Покупка: 100' : 'Закрыт';
function header(title, step) {
  return `<header class="page-header"><h1>${title}</h1><div class="steps" aria-label="Шаг выбора"><span class="${step===1?'current':''}">1 · Персонаж</span><span>—</span><span class="${step===2?'current':''}">2 · Поле</span></div></header>`;
}
function show(screen) {
  state.screen = screen; render();
  root.querySelector('h1')?.focus({preventScroll:true});
}
function samples(data) {
  return state.dense ? Array.from({length:data===fields?15:10}, (_,i) => ({ ...data[i%data.length], source:i%data.length,
    name:i<data.length?data[i].name:`Тестовая карточка ${i+1}`, sample:i>=data.length })) : data.map((item,i)=>({...item,source:i}));
}
function menu() {
  return `<section class="menu menu-variant-${state.menuVariant}">
    ${state.menuVariant==='E'?`<div class="menu-depth" aria-hidden="true"><div class="depth-plane depth-back"><img class="menu-art" src="/docs/implementation/proposals/ui-entry-r1/assets/menu-depth-background-v001.png" alt=""></div><div class="depth-rays"><span class="sun-ray" style="--ray-left:44%;--ray-width:23%;--ray-angle:24deg;--duration:10s;--delay:-5s"></span><span class="sun-ray" style="--ray-left:68%;--ray-width:11%;--ray-angle:27deg;--duration:13s;--delay:-11s"></span><span class="sun-ray" style="--ray-left:88%;--ray-width:17%;--ray-angle:22deg;--duration:12s;--delay:-3s"></span></div><div class="depth-plane depth-front"><img src="/docs/implementation/proposals/ui-entry-r1/assets/menu-depth-foreground-shepotka-v002.png" alt=""></div></div>`:state.menuVariant!=='C'?`<img class="menu-art" src="${state.menuVariant==='D'?'/TestResults/ui-entry-r1/menu-escape-v003.png':fields[0].image}" alt="">`:'<div class="menu-portrait-surface"></div>'}<div class="menu-scrim"></div>
    ${['A','C'].includes(state.menuVariant)?`<img class="menu-character" src="${characters[0].image}" alt="">`:''}
    ${['D','E'].includes(state.menuVariant)?`<div class="menu-atmosphere" aria-hidden="true">${Array.from({length:state.menuVariant==='E'?18:7},(_,i)=>`<span class="menu-mote" style="left:${state.menuVariant==='E'?14+(i%2===0?(i*7)%38:42+(i*11)%38):8+i*13}%;top:${state.menuVariant==='E'?42+(i*17)%49:45+(i*17)%42}%;--size:${7.5+i%4*3.1}px;--drift:${-45-i%5*14}px;--duration:${11+i%4*2}s;--delay:${-i*3.7}s"></span>`).join('')}</div>`:''}
    <div class="menu-body"><h1 tabindex="-1">SURVIVOR<br>ARENA</h1>
      <button class="action primary" data-go="characters">Играть</button>
      <button class="action" data-placeholder="Развитие">Развитие</button>
      <button class="action" data-placeholder="Настройки">Настройки</button>
      <button class="action quiet" data-placeholder="Выход">Выйти</button>
    </div>
  </section>`;
}
function characterPage() {
  const inspected = samples(characters)[state.characterInspection], index = inspected.source;
  const locked = inspected.sample || !accessibleCharacter(index);
  const lockText = inspected.sample ? 'Тестовая запись для проверки длинного списка' : state.profile === 'new' ? 'Пройдите «Деревенскую окраину»' : 'Доступен для покупки · 100';
  const grid = samples(characters).map((c,i)=>`<button class="choice character-choice ${i===state.characterInspection?'selected':''} ${c.sample||!accessibleCharacter(c.source)?'locked':''}" data-character="${i}" aria-pressed="${i===state.characterInspection}">
    <img src="${c.image}" alt=""><span class="choice-name">${c.name}</span><span class="choice-state">${c.sample?'Тестовый размер списка':charStatus(c.source)}</span></button>`).join('');
  return `<section class="page">${header('Выбери персонажа',1)}<div class="workspace">
    <aside class="catalog"><div class="catalog-caption">Беглецы</div><div class="catalog-scroll"><div class="character-grid">${grid}</div></div></aside>
    <article class="detail character-detail"><div class="hero-body ${locked?'is-locked':''}"><img src="${inspected.image}" alt="${locked?'Силуэт: ':''}${inspected.name}"><div class="body-caption">${locked?'Персонаж не получен':'Выбранный персонаж'}</div>${locked?`<div class="lock-note">${lockText}<small>${inspected.sample?'Не новая игровая сущность.':state.profile==='new'?'Затем покупка за 100 в разделе «Развитие».':'Покупка — в разделе «Развитие» главного меню.'}</small></div>`:''}</div>
    <div class="character-copy"><div class="eyebrow">${locked?'ЗАКРЫТ':'ГОТОВ К ПОБЕГУ'}</div><h2>${inspected.name}</h2><p class="role">${inspected.role}</p>
      <div class="skill-block"><div class="section-label">Стартовое умение</div><div class="skill-heading"><img src="${inspected.icon}" alt=""><h3>${inspected.skill}</h3></div>${inspected.boosts.map(b=>`<p class="boost">${b}</p>`).join('')}</div>
      ${inspected.stats.length?`<div class="stat-list"><div class="section-label">Отличия от базового персонажа</div>${inspected.stats.map(([label,value])=>`<div class="stat"><span>${label}</span><span class="value ${value.startsWith('−')?'negative':''}">${value}</span></div>`).join('')}</div>`:''}
    </div></article></div>
    <footer class="page-footer"><button class="action" data-go="menu">Назад</button><span class="footer-detail">${locked?'Этот персонаж пока недоступен':inspected.name}</span><button class="action primary" id="character-confirm" ${locked?'disabled':''}>Выбрать поле →</button></footer></section>`;
}
function fieldPage() {
  const inspected = samples(fields)[state.fieldInspection], locked = inspected.sample || !accessibleField(inspected.source);
  const list = samples(fields).map((f,i)=>`<button class="choice field-tile ${i===state.fieldInspection?'selected':''} ${f.sample||!accessibleField(f.source)?'locked':''}" data-field="${i}" aria-pressed="${i===state.fieldInspection}"><img src="${f.image}" alt=""><span class="field-tile-copy"><span class="choice-name">${f.name}</span><span class="field-tile-difficulty">Сложность ${f.difficulty}/5</span><span class="choice-state">${f.sample?'Тестовая карточка':accessibleField(f.source)?'Доступно':'Закрыто'}</span></span></button>`).join('');
  return `<section class="page fields-page">${header('Выбери поле',2)}<div class="field-grid catalog-scroll ${state.dense?'field-grid-overflow':''}">${list}</div>
    <div class="field-selection-note" role="status"><strong>${inspected.name}</strong><span class="${locked?'lock-note':''}">${locked?(inspected.sample?'Тестовая запись, не игровое поле':inspected.unlock):'Доступно'}</span></div>
    <footer class="page-footer"><button class="action" data-go="characters">Назад</button><span class="footer-detail selected-character"><img src="${characters[state.character].image}" alt="">${characters[state.character].name}</span><button class="action primary" id="field-confirm" ${locked?'disabled':''}>Начать забег</button></footer></section>`;
}
function render(preserveScroll = false) {
  resetDepth();
  const previousScroll = preserveScroll ? root.querySelector('.catalog-scroll')?.scrollTop || 0 : 0;
  root.classList.toggle('compact', state.width===1280);
  root.innerHTML = state.screen==='menu'?menu():state.screen==='characters'?characterPage():fieldPage();
  if (preserveScroll && root.querySelector('.catalog-scroll')) root.querySelector('.catalog-scroll').scrollTop = previousScroll;
  root.querySelector('h1')?.setAttribute('tabindex','-1');
  document.querySelectorAll('[data-screen]').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.screen===state.screen)));
  document.querySelector('#menu-review').hidden = state.screen!=='menu';
  document.querySelectorAll('[data-menu-variant]').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.menuVariant===state.menuVariant)));
  document.querySelector('#variant-note').textContent = variantNotes[state.menuVariant];
  updateMotion();
  fit();
}
function updateMotion() {
  const available=state.screen==='menu' && ['D','E'].includes(state.menuVariant);
  document.querySelector('#motion-control').hidden=!available;
  const control=document.querySelector('#menu-motion');
  control.disabled=reducedMotion.matches;
  control.checked=state.motion && !reducedMotion.matches;
  control.title=reducedMotion.matches?'Отключено системной настройкой уменьшения движения':'';
  root.classList.toggle('motion-enabled',available && control.checked);
  document.documentElement.classList.toggle('preview-inactive',document.hidden);
  if(!depthActive())resetDepth();
}
function depthActive() { return state.screen==='menu' && state.menuVariant==='E' && state.motion && !reducedMotion.matches && !document.hidden; }
function resetDepth() {
  cancelAnimationFrame(depthFrame); depthFrame=0;
  depthX=depthY=targetDepthX=targetDepthY=0;
  root.style.setProperty('--look-x','0'); root.style.setProperty('--look-y','0');
}
function smoothDepth() {
  depthFrame=0;
  if(!depthActive())return;
  depthX+=(targetDepthX-depthX)*.12; depthY+=(targetDepthY-depthY)*.12;
  root.style.setProperty('--look-x',depthX.toFixed(4)); root.style.setProperty('--look-y',depthY.toFixed(4));
  if(Math.abs(targetDepthX-depthX)+Math.abs(targetDepthY-depthY)>.001)depthFrame=requestAnimationFrame(smoothDepth);
}
function targetDepth(x,y) {
  targetDepthX=x; targetDepthY=y;
  if(!depthFrame)depthFrame=requestAnimationFrame(smoothDepth);
}
root.addEventListener('pointermove',e=>{
  if(!depthActive() || e.pointerType==='touch')return;
  const r=root.getBoundingClientRect();
  targetDepth(Math.max(-1,Math.min(1,(e.clientX-r.left)/r.width*2-1)),Math.max(-1,Math.min(1,(e.clientY-r.top)/r.height*2-1)));
});
root.addEventListener('pointerleave',()=>{if(depthActive())targetDepth(0,0);});
function fit() {
  const height = state.width*9/16, scale = Math.min(1, (window.innerWidth-24)/state.width);
  root.style.width = `${state.width}px`; root.style.height = `${height}px`; root.style.transform = `scale(${scale})`;
  const stage = document.querySelector('#stage'); stage.style.width=`${state.width*scale}px`; stage.style.height=`${height*scale}px`;
}
document.addEventListener('click', event => {
  const button = event.target.closest('button'); if (!button || button.disabled) return;
  if (button.dataset.menuVariant) {
    state.menuVariant=button.dataset.menuVariant;
    const url=new URL(location.href); url.searchParams.set('menu',state.menuVariant); history.replaceState(null,'',url);
    render(); return;
  }
  if (button.dataset.screen || button.dataset.go) { show(button.dataset.screen||button.dataset.go); return; }
  if (button.dataset.placeholder) { message(`«${button.dataset.placeholder}»: существующее действие. Экран не входит в этот макет; игра и профиль не изменены.`); return; }
  if (button.dataset.character!==undefined) {
    state.characterInspection=Number(button.dataset.character); render(true); root.querySelector(`[data-character="${state.characterInspection}"]`).focus({preventScroll:true}); return;
  }
  if (button.dataset.field!==undefined) {
    state.fieldInspection=Number(button.dataset.field); render(true); root.querySelector(`[data-field="${state.fieldInspection}"]`).focus({preventScroll:true}); return;
  }
  if (button.id==='character-confirm') {
    state.character=samples(characters)[state.characterInspection].source; show('fields'); return;
  }
  if (button.id==='field-confirm') {
    state.field=samples(fields)[state.fieldInspection].source;
    message(`Макет: запуск ${characters[state.character].name} → ${fields[state.field].name}. Реальный забег не запускался.`);
  }
});
document.querySelector('#resolution').addEventListener('change',e=>{ state.width=Number(e.target.value); render(); });
document.querySelector('#menu-motion').addEventListener('change',e=>{
  state.motion=e.target.checked;
  const url=new URL(location.href); url.searchParams.set('motion',state.motion?'on':'off'); history.replaceState(null,'',url);
  updateMotion();
});
reducedMotion.addEventListener('change',updateMotion);
document.addEventListener('visibilitychange',updateMotion);
document.querySelector('#profile').addEventListener('change',e=>{ state.profile=e.target.value; if(!accessibleCharacter(state.character))state.character=0; render(); });
document.querySelector('#dense').addEventListener('change',e=>{ state.dense=e.target.checked; state.characterInspection=0; state.fieldInspection=0; render(); message(state.dense?'Стресс: 10 героев / 15 полей; дополнительные записи тестовые, не новый игровой контент.':'Обычный пример: два героя и десять approved полей. Runtime-каталог не изменён.'); });
document.addEventListener('keydown',e=>{
  if(e.key==='Escape' && state.screen!=='menu') { e.preventDefault(); show(state.screen==='fields'?'characters':'menu'); }
});
window.addEventListener('resize',fit);
render();
