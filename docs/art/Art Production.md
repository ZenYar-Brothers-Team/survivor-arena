\# Art Production

\#\# Назначение

Этот таб — production inventory для визуальной части Survival Arena.

Он отвечает не на вопрос \*\*«как должна выглядеть игра»\*\* — это задаёт \`Art Direction v2\`, — а на вопрос:

\> \*\*какие конкретно визуальные ассеты и procedural-эффекты ещё нужны, каким способом их делать и в каком они состоянии.\*\*

Источники:  
\- \`Content Design v2\` — список и механика игровых сущностей;  
\- \`Art Direction v2\` — общий визуальный язык и правила production;  
\- этот таб — operational checklist.

Цель — не забыть ни отдельные картинки, ни VFX, ни UI, ни эффекты, которые должны делаться непосредственно в Unity.

\---

\#\# Статусы

\- \`NOT STARTED\` — работа ещё не начиналась.  
\- \`GENERATED\` — есть первая сгенерированная версия.  
\- \`REVIEW\` — идёт визуальный review / нужны правки.  
\- \`APPROVED\` — визуал принят.  
\- \`IN GAME\` — ассет импортирован и реально используется в Unity.

\`APPROVED\` и \`IN GAME\` — разные состояния.

\---

\#\# Способ производства

\#\#\# Generate via GPT  
Нужен отдельный изображаемый ассет, который разумно генерировать через GPT:  
\- персонаж;  
\- враг;  
\- projectile;  
\- pickup;  
\- icon;  
\- obstacle;  
\- decoration;  
\- texture/effect base.

\#\#\# Procedural in Unity  
Новая картинка обычно не нужна. Эффект делается движком:  
\- движение;  
\- squash/stretch;  
\- shake;  
\- rotation;  
\- scale pulse;  
\- fade;  
\- hit flash;  
\- tint;  
\- screen shake;  
\- simple ring;  
\- simple line/beam;  
\- UI bars/shapes.

\#\#\# Hybrid  
GPT генерирует базовую визуальную часть, Unity делает поведение:  
\- aura texture \+ pulse/rotation;  
\- beam texture \+ stretching;  
\- impact sprite \+ fade/scale;  
\- projectile sprite \+ trail/movement;  
\- explosion art \+ timing/scale;  
\- set proc art \+ procedural emphasis.

\---

\#\# Общий production rule

По умолчанию \*\*не генерировать покадровую анимацию\*\*, если эффект можно получить через Unity transforms/materials/VFX.

Базовый pipeline:

\`\`\`text  
Content Design  
\+ Art Direction  
\+ Art Production entry  
        ↓  
Generate base asset if needed  
        ↓  
Visual review  
        ↓  
APPROVED  
        ↓  
Import to Unity  
        ↓  
Procedural motion / material / VFX behavior  
        ↓  
IN GAME  
\`\`\`

Длинные generation prompts в этом табе не хранятся. Здесь остаются только brief, production method и status.

\---

\# 1\. Playable Characters

Минимум для каждого персонажа:  
\- body sprite;  
\- для Character Select сначала использовать crop/variant body sprite; отдельный portrait генерировать только если это выглядит недостаточно хорошо.

| ID | Character | Asset | Method | Status | Notes |  
|---|---|---|---|---|---|  
| CHAR-001 | Клёпка | Body sprite | Generate via GPT | APPROVED | Концепт = fixture goblin v002: связь подтверждена пользователем 2026-09-21 в asset-record. Runtime интегрирован как FIXTURE-CHARACTER-AGILE; production binding CHAR-001 относится к IP-22 |
| CHAR-001 | Клёпка | Character Select image | Reuse body sprite first | NOT STARTED | Crop/variant existing body; отдельный portrait только если понадобится |  
| CHAR-002 | Бугор | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-003 | Шепотка | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-004 | Тётка Шмыга | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-005 | Бабка Искра | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-006 | Гром | Body sprite | Generate via GPT | NOT STARTED | Огр; отдельный крупный силуэт |  
| CHAR-007 | Дед Вертун | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-008 | Тётушка Светляк | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-009 | Иголка | Body sprite | Generate via GPT | NOT STARTED | |  
| CHAR-010 | Старшой Ночка | Body sprite | Generate via GPT | NOT STARTED | |

\#\#\# Procedural character presentation  
Для всех playable-персонажей по умолчанию:  
\- idle bob — Procedural in Unity;  
\- movement lean — Procedural in Unity;  
\- squash/stretch — Procedural in Unity;  
\- horizontal flip — Procedural in Unity;  
\- hit flash — Procedural in Unity;  
\- hit shake — Procedural in Unity;  
\- death fade/pop — Procedural in Unity / Hybrid.

\---

\# 2\. Regular Enemies

Для каждого обычного врага минимум нужен один body sprite. Покадровая walk-анимация по умолчанию не требуется.

| ID | Enemy | Asset | Method | Status |  
|---|---|---|---|---|  
| ENEMY-001 | Селянин с вилами | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-002 | Деревенский гонец | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-003 | Дровосек | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-004 | Пращник | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-005 | Королевский лучник | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-006 | Арбалетчик | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-007 | Охотничья гончая | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-008 | Конный разведчик | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-009 | Щитоносец ополчения | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-010 | Гвардейский стрелок | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-011 | Боевой капеллан | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-012 | Королевский копейщик | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-013 | Охотник на чудовищ | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-014 | Осадный маг | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-015 | Инквизитор | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-016 | Латный рыцарь | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-017 | Рыцарь-дуэлянт | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-018 | Придворный чародей | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-019 | Серафим-страж | Body sprite | Generate via GPT | NOT STARTED |  
| ENEMY-020 | Ангел-каратель | Body sprite | Generate via GPT | NOT STARTED |

\#\#\# Enemy attack visuals  
Отдельные projectile/effect assets добавляются только для реально используемых ranged patterns. Максимально переиспользовать общие семейства:  
\- arrow;  
\- crossbow bolt;  
\- sling stone;  
\- magic bolt;  
\- holy/light projectile;  
\- spear/thrust accent where needed.

Не создавать уникальный projectile для каждого врага, если визуально допустим reuse.

\---

\# 3\. Bosses

| ID | Boss | Asset | Method | Status |  
|---|---|---|---|---|  
| BOSS-001 | Староста-герой | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-002 | Капитан королевской стражи | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-003 | Главный королевский ловчий | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-004 | Рыцарь знамени | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-005 | Великий инквизитор | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-006 | Королевский архимаг | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-007 | Верховный паладин | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-008 | Чемпион короны | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-009 | Серафим-полководец | Body sprite | Generate via GPT | NOT STARTED |  
| BOSS-010 | Архангел Спасения | Body sprite | Generate via GPT | NOT STARTED |

Boss attack VFX/projectiles создаются по конкретным attack patterns из Content Design и могут переиспользовать generic VFX families.

\---

\# 4\. Mid-Bosses

| ID | Mid-boss | Asset | Method | Status |  
|---|---|---|---|---|  
| MIDBOSS-001 | Старший загонщик | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-002 | Сержант стражи | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-003 | Королевский следопыт | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-004 | Рыцарь-преследователь | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-005 | Капитан городской стражи | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-006 | Маг-наблюдатель | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-007 | Паладин авангарда | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-008 | Рыцарь короны | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-009 | Вестник небес | Body sprite | Generate via GPT | NOT STARTED |  
| MIDBOSS-010 | Ангел-предвестник | Body sprite | Generate via GPT | NOT STARTED |

\---

\# 5\. Travelers / Путники

| ID | Traveler | Asset | Method | Status |  
|---|---|---|---|---|  
| TRAVELER-001 | Дорожный громила | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-002 | Бродячий стрелок | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-003 | Странствующий копейщик | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-004 | Наёмный дуэлянт | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-005 | Паломник со щитом | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-006 | Дорожный маг | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-007 | Путевой инквизитор | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-008 | Рыцарь-странник | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-009 | Небесный паломник | Body sprite | Generate via GPT | NOT STARTED |  
| TRAVELER-010 | Ангел-скиталец | Body sprite | Generate via GPT | NOT STARTED |

Дополнительно:  
\- off-screen direction arrow — Procedural/simple UI;  
\- HP bar над каждым Traveler — Procedural/simple UI;  
\- shield/support aura для защитников — Hybrid / Procedural in Unity;  
\- отдельная картинка для arrow или HP bar не нужна.

\---

\# 6\. Active Skills

Для каждого skill нужен UI icon. World-art зависит от механики.

| ID | Skill | World visual assets | Method | Status |  
|---|---|---|---|---|  
| SKILL-001 | Бросок камня | Stone projectile; optional impact | Generate via GPT \+ Hybrid | NOT STARTED |  
| SKILL-002 | Веер игл | Needle projectile | Generate via GPT \+ Unity fan pattern | NOT STARTED |  
| SKILL-003 | Орбитальные клинки | Blade sprite | Generate via GPT \+ Unity orbit | NOT STARTED |  
| SKILL-004 | Импульсная волна | Expanding pulse/ring | Procedural in Unity / Hybrid texture | NOT STARTED |  
| SKILL-005 | Ветряное копьё | Wind spear projectile | Generate via GPT \+ Unity motion | NOT STARTED |  
| SKILL-006 | Бумеранг | Boomerang projectile | Generate via GPT \+ Unity return path | NOT STARTED |  
| SKILL-007 | Цепная молния | Lightning chain \+ hit flash | Procedural in Unity / Hybrid | NOT STARTED |  
| SKILL-008 | Рикошетный диск | Disk projectile | Generate via GPT \+ Unity ricochet | NOT STARTED |  
| SKILL-009 | Магматическая мина | Mine sprite \+ explosion base | Hybrid | NOT STARTED |  
| SKILL-010 | Небесный удар | Telegraph marker \+ strike/impact | Hybrid | NOT STARTED |  
| SKILL-011 | Спираль осколков | Shard projectile | Generate via GPT \+ Unity spiral pattern | NOT STARTED |  
| SKILL-012 | Пульсирующий луч | Beam base visual | Hybrid; stretch/aim in Unity | NOT STARTED |  
| SKILL-013 | Ледяные осколки | Ice shard projectile \+ optional ice impact | Hybrid | NOT STARTED |  
| SKILL-014 | Взрывные сферы | Sphere projectile \+ explosion base | Hybrid | NOT STARTED |  
| SKILL-015 | Крест клинков | Blade/wave visual | Hybrid; cross pattern in Unity | NOT STARTED |  
| SKILL-016 | Разбрасыватель мусора | Small trash projectile set | Generate via GPT \+ Unity motion | NOT STARTED |

\#\#\# Skill UI icons  
Нужно 16 icons:  
\`SKILL-001 ... SKILL-016\`

Method: \`Generate via GPT\`    
Status: \`NOT STARTED\`

\---

\# 7\. Passive Items

World sprite для passive item по умолчанию не нужен.

Нужно 14 UI icons:

| ID | Passive | Asset | Method | Status |  
|---|---|---|---|---|  
| PASSIVE-001 | Крепкое сердце | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-002 | Собиратель | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-003 | Лёгкие сапоги | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-004 | Точильный камень | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-005 | Метроном | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-006 | Эхо памяти | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-007 | Магнит опыта | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-008 | Закалённая кожа | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-009 | Лечебная настойка | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-010 | Талисман ученика | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-011 | Тяжёлый пояс | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-012 | Широкий замах | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-013 | Длинные руки | Icon | Generate via GPT | NOT STARTED |  
| PASSIVE-014 | Упрямство | Icon | Generate via GPT | NOT STARTED |

Постоянные world-aura для пассивок не создавать без отдельной gameplay/readability причины.

\---

\# 8\. Sets

Для каждого set нужен UI icon.

Нужно 20 icons:  
\`SET-001 ... SET-020\`

Method: \`Generate via GPT\`    
Status: \`NOT STARTED\`

\#\# Set world effects

Большинство сетов усиливают/трансформируют существующие skills и \*\*не требуют нового самостоятельного projectile\*\*.

| Set | Additional visual work | Method |  
|---|---|---|  
| SET-001 Тяжёлый боезапас | Более крупный/тяжёлый existing rock \+ trail accent | Procedural / Hybrid |  
| SET-002 Возвратный ритм | Distinct return trail/accent | Procedural / Hybrid |  
| SET-003 Грозовой проводник | Усиленная existing lightning presentation | Procedural / Hybrid |  
| SET-004 Ледяной таран | Ice/knockback accent | Hybrid |  
| SET-005 Жадность к знаниям | XP/progression accent if needed | Procedural / Hybrid |  
| SET-006 Полевой медик | Healing accent | Reuse generic VFX |  
| SET-007 Векторный шторм | Existing skill size/width emphasis | Procedural |  
| SET-008 Утилизатор | Heavy trash replacement projectile \+ explosion | Hybrid |  
| SET-009 Линия пробоя | Existing linear attacks amplified | Procedural / Hybrid |  
| SET-010 Холодная орбита | Cold orbit/slow accent | Hybrid |  
| SET-011 Кинетический арсенал | Speed/trail/impact amplification | Procedural / Hybrid |  
| SET-012 Неподвижная крепость | Defensive/impact accent | Procedural / Hybrid |  
| SET-013 Перегрузка сети | Distinct electrical network burst | Hybrid |  
| SET-014 Танец клинков | Existing blades/shards amplified | Procedural / Hybrid |  
| SET-015 Алхимия хаоса | Potion-triggered explosion | Reuse/variant explosion VFX |  
| SET-016 Выстрел великана | Huge bolt projectile | Generate via GPT \+ Unity motion |  
| SET-017 Падающая звезда | Telegraph \+ large strike | Hybrid |  
| SET-018 Сфера разрушения | Large slow sphere \+ explosion | Hybrid |  
| SET-019 Ледяное копьё | Huge ice shard | Generate via GPT \+ Hybrid |  
| SET-020 Каменное ядро | Massive boulder | Generate via GPT \+ Hybrid |

Generic rule: set effects должны быть вторичным визуальным слоем и не забивать основные active skills.

\---

\# 9\. Pickups / Drops

| ID / Entity | Asset | Method | Status | Notes |  
|---|---|---|---|---|  
| XP pickup | World sprite | Generate via GPT | NOT STARTED | Можно иметь 1–3 visual variants if useful |  
| PICKUP-001 Healing Potion | World sprite | Generate via GPT | NOT STARTED | |  
| Traveler Book | World sprite | Generate via GPT | NOT STARTED | Открывает extra draft |  
| Meta currency | UI icon / optional world art | Generate via GPT | NOT STARTED | Точный presentation зависит от meta UI |

\---

\# 10\. Generic VFX

Эти эффекты могут переиспользоваться между многими сущностями.

| VFX | Method | Status | Notes |  
|---|---|---|---|  
| Basic hit flash | Procedural in Unity | IN GAME (fixture player) | Existing player Health.Damaged → animator; generic adapter tested separately, not all production owners |
| Basic impact spark | Hybrid | NOT STARTED | Можно генерировать одну базовую вспышку |  
| Slash impact | Hybrid | NOT STARTED | Для blade-type attacks |  
| Generic explosion | Hybrid | NOT STARTED | Mine/sphere/set reuse |  
| Lightning impact | Hybrid / Procedural | NOT STARTED | |  
| Ice impact | Hybrid | NOT STARTED | |  
| Heal effect | Hybrid | NOT STARTED | |  
| Level-up effect | Hybrid | NOT STARTED | |  
| Set activation effect | Hybrid | NOT STARTED | Общий accent |  
| Enemy death effect | Hybrid / Procedural | NOT STARTED | Переиспользуемый |  
| Slow feedback | Procedural / Hybrid | NOT STARTED | Tint \+ optional overlay |  
| Projectile trail | Procedural / Hybrid | NOT STARTED | Trail Renderer \+ optional texture |  
| Beam base | Hybrid | NOT STARTED | Texture/style \+ Unity stretch |  
| Aura base | Hybrid | NOT STARTED | Texture/style \+ Unity pulse/rotation |  
| Telegraph marker | Hybrid / Procedural | NOT STARTED | Для strikes/dashes if needed |  
| Screen shake | Procedural in Unity | NOT STARTED | No image |  
| Damage numbers | Procedural UI/Text | NOT STARTED | No image |

\---

\# 11\. Fields / Environment

Для каждого field минимум определить:  
\- ground/background treatment;  
\- decoration pack;  
\- obstacle pack, если препятствия предусмотрены;  
\- уникальные крупные landmarks только если реально нужны.

Не строить заранее сложный tileset, если поле можно собрать из повторяемого ground \+ нескольких props.

| ID | Field | Needed art | Method | Status |  
|---|---|---|---|---|  
| FIELD-001 | Деревенская окраина | Ground/background \+ decor pack \+ obstacle pack | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-002 | Королевский тракт | Ground/background \+ decor pack \+ obstacle pack | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-003 | Пограничные руины | Ground/background \+ ruins/walls/bridge-style props | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-004 | Рыцарский лагерь | Ground/background \+ camp decor/obstacles | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-005 | Королевская столица | Ground/background \+ city decor/obstacles | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-006 | Академия магов | Ground/background \+ magical decor/obstacles | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-007 | Монастырские сады | Ground/background \+ garden/religious decor | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-008 | Цитадель короны | Ground/background \+ fortress decor/obstacles | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-009 | Небесные врата | Ground/background \+ celestial decor/obstacles | Generate via GPT / Hybrid | NOT STARTED |  
| FIELD-010 | Чертог Спасения | Ground/background \+ final celestial/interior kit | Generate via GPT / Hybrid | NOT STARTED |

\---

\# 12\. Obstacles

Obstacle — gameplay object, который влияет на пространство/перемещение.

По умолчанию:  
\- one sprite per obstacle type;  
\- collider/interaction — Unity;  
\- movement/rotation if any — Unity.

Примеры категорий, которые могут собираться пакетами по field:  
\- rocks;  
\- fences;  
\- crates/barrels;  
\- carts;  
\- broken walls;  
\- columns;  
\- statues;  
\- altars;  
\- roots;  
\- camp objects;  
\- magical structures.

Method: \`Generate via GPT\` для base sprite, \`Procedural in Unity\` для gameplay behavior.

Не нужно заранее делать 5 obstacles на каждое поле: создавать только то, что реально присутствует в утверждённой geometry/content конкретного field.

\---

\# 13\. Decorations

Decoration не влияет на gameplay и может использоваться для разнообразия среды.

Примеры:  
\- grass/flowers;  
\- bushes;  
\- stones;  
\- bones;  
\- flags;  
\- candles;  
\- rubble;  
\- banners;  
\- books;  
\- magical props;  
\- religious props;  
\- celestial ornaments.

Method: \`Generate via GPT\`.  
\# 14\. UI Art

\#\# Image assets required by approved UI

| UI asset | Method | Status | Notes |  
|---|---|---|---|  
| 16 skill icons | Generate via GPT | NOT STARTED | Обязательны для HUD, draft и Pause / Build |  
| 14 passive icons | Generate via GPT | NOT STARTED | Обязательны для draft и Pause / Build |  
| 20 set icons | Generate via GPT | NOT STARTED | Обязательны для draft, acquired sets и set progress |  
| Character selection image | Reuse body sprite first | NOT STARTED | Сначала использовать crop/variant existing body sprite; отдельный portrait генерировать только если выглядит плохо |  
| Field thumbnails | Generate / derive from field art | NOT STARTED | По одному на FIELD-001…010 для Field Select; отдельная уникальная картинка не нужна, если подходит crop/композиция existing field art |  
| Meta-upgrade icons | Generate via GPT as content is defined | NOT STARTED | Только для реально реализованных permanent upgrades |  
| Pickup icons if UI needs separate icon | Reuse world sprite / Generate if needed | NOT STARTED | Не создавать отдельный asset без необходимости |

\#\# Procedural / simple Unity UI required by approved UI

Для следующих элементов отдельная generated image \*\*не нужна\*\*:

\- player HP bar;  
\- player XP bar;  
\- boss HP bar \+ boss name;  
\- HP bar над \*\*каждым Traveler\*\*;  
\- run timer;  
\- current level text;  
\- active/passive/set icon slots;  
\- DraftCard background/layout;  
\- Active / Passive / Set labels;  
\- set progress text вида \`3/4\`;  
\- recipe component checkmarks / incomplete markers;  
\- \`COMPLETES RECIPE\` / \`SET PROGRESS\` accents;  
\- tooltip / detail panel для полного set recipe;  
\- reroll button \+ remaining count;  
\- banish button \+ remaining count;  
\- Traveler off-screen direction arrow;  
\- Pause / Build panels;  
\- set-progress list in Pause / Build;  
\- boss incoming / traveler / unlock notifications;  
\- Victory / Defeat text;  
\- Run Results layout;  
\- Retry / Main Menu buttons;  
\- Main Menu buttons;  
\- Character / Field card containers;  
\- locked overlay;  
\- hover / pressed / disabled / selected states;  
\- simple difficulty indicator;  
\- Meta Progression cards, currency counter and Buy button;  
\- settings sliders/toggles;  
\- generic panels, borders and progress indicators.

Все перечисленные элементы сначала реализуются обычными Unity UI shapes/text/components. Отдельный art добавляется позже только если это реально улучшает визуал.

\#\# UI-specific production rules

\- Character portraits \*\*не входят в обязательный generation backlog\*\*: сначала использовать crop/variant body sprite.  
\- Character Select показывает только значимые stat modifiers; отдельные art assets под полный stat table не нужны.  
\- Pause / Build показывает set recipes только с текущим progress; отдельный full-set-compendium UI/art для MVP не нужен.  
\- Traveler HP bar является обязательным procedural UI для всех Travelers.  
\- Retry — обычная кнопка; отдельного confirmation screen/art нет. Нажатие сразу запускает новый run с теми же character и field.  
\- Reroll/Banish могут быть обычными текстовыми кнопками с простым icon только при необходимости; отдельные generated icons не обязательны.  
\- Set recipe readability строится прежде всего на существующих skill/passive/set icons \+ text/checkmarks, а не на дополнительных уникальных картинках.

\---

\# 15\. Procedural-only presentation

Эти задачи должны присутствовать в production checklist, хотя новых изображений для них не требуется:

\- idle bob;  
\- movement lean;  
\- squash/stretch;  
\- hit shake;  
\- hit flash;  
\- projectile rotation;  
\- projectile travel;  
\- boomerang return;  
\- orbit motion;  
\- wave expansion;  
\- beam stretch;  
\- aura pulse;  
\- aura rotation;  
\- telegraph timing;  
\- fade in/out;  
\- spawn pop;  
\- death fade/pop;  
\- knockback motion;  
\- slow tint;  
\- set proc emphasis;  
\- screen shake.

Default для ещё не подключённых owners: \`Procedural in Unity\`, \`NOT STARTED\`. Existing fixture player уже использует idle/bob/lean/squash/flip/hit/spawn; это не означает готовность всех production owners. Gameplay knockback/travel/orbit остаются authoritative motion, не sprite-анимацией.

\---

\# 16\. Marketing / Store Art — отдельный поздний слой

Не требуется для первого playable build, но не забыть перед release:  
\- Steam capsule assets;  
\- library/header images;  
\- screenshots;  
\- key art;  
\- logo/title treatment;  
\- optional trailer visuals.

Эти ассеты не смешивать с gameplay art production.

\---

\# 17\. Рекомендуемый порядок производства

Не генерировать сразу весь каталог.

\#\# Phase A — первый визуально собранный playable build  
1\. CHAR-001 body sprite — уже есть concept.  
2\. Только те enemies, которые реально реализованы в текущем playable scope.  
3\. World assets только для реализованных skills.  
4\. XP pickup \+ potion \+ Traveler Book, если соответствующие systems уже работают.  
5\. Один field kit для текущего field.  
6\. Минимальные generic VFX: hit, death, heal, level-up.  
7\. Минимальные icons для реально доступного в build контента.

\#\# Phase B — расширение текущего implementation scope  
После появления новой сущности в playable build:  
1\. добавить/актуализировать строку в этом inventory;  
2\. сгенерировать base art;  
3\. review;  
4\. импортировать;  
5\. сделать procedural behavior;  
6\. перевести статус в \`IN GAME\`.

\#\# Phase C — full content  
Только после стабилизации gameplay постепенно закрывать весь remaining Content Design.

\---

\# 18\. Generation workflow with GPT

Для конкретного asset generation request использовать вместе:  
1\. \`Content Design v2\` — что это за сущность;  
2\. \`Art Direction v2\` — как она должна выглядеть;  
3\. \`Art Production\` — какой именно файл/эффект сейчас нужен;  
4\. уже approved assets — как visual reference для консистентности.

Для каждого нового ассета сначала определить:  
\- entity ID;  
\- asset role;  
\- Generate / Procedural / Hybrid;  
\- transparent background нужен или нет;  
\- intended Unity use;  
\- required silhouette/readability;  
\- relationship to existing assets.

После генерации:  
\`GENERATED → REVIEW → APPROVED → IN GAME\`.

\---

\# 19\. Практический принцип экономии времени

Если визуальный результат можно убедительно получить:  
\- одним base sprite;  
\- Unity movement;  
\- scale/rotation;  
\- material tint;  
\- simple trail;  
\- reusable VFX;

не создавать дополнительную картинку только ради того, чтобы «ассет был».

Цель — минимальное число production assets, которое даёт читаемую и цельную игру.  


## Проверяемый fixture inventory — 2026-09-21

[Manifest](../../Art/asset-manifest.json) фиксирует owner+role, method, source/runtime, stage и evidence. CHAR-001 concept identity подтверждена пользователем: это master fixture goblin v002; runtime остаётся FIXTURE-CHARACTER-AGILE, production binding отдельно IP-22. UI body reuse в диагностическом слоте принят пользователем 2026-09-21; production Character Select binding проверяется отдельно. Шесть новых procedural diagnostic roles (body/projectile/pickup/telegraph/shadow/impact) приняты пользователем в Presentation Fixture Review («всё хорошо», 2026-09-21); они не заменяют production строки выше. Player ShadowRenderer пока не имеет отдельного shadow asset. Полное исполнение IP учитывается только в STATUS.
