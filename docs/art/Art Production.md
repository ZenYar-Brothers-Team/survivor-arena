\# Art Production

\#\# Назначение

Этот документ — production inventory для визуальной части Survival Arena.

Он отвечает не на вопрос \*\*«как должна выглядеть игра»\*\* — это задаёт [Art Direction](ART_DIRECTION.md), — а на вопрос:

\> \*\*какие конкретно визуальные ассеты и procedural-эффекты ещё нужны, каким способом их делать и в каком они состоянии.\*\*

Источники:
\- [Content Design](../Content_design.md) — список и механика игровых сущностей;
\- [Art Direction](ART_DIRECTION.md) — общий визуальный язык и правила production;
\- [Asset Pipeline](ASSET_PIPELINE.md) — подготовка, provenance, импорт, binding и approval gates;
\- этот документ — visual inventory и operational checklist.

Цель — не забыть ни отдельные картинки, ни VFX, ни UI, ни эффекты, которые должны делаться непосредственно в Unity.

\---

\#\# Статусы

\- \`NOT STARTED\` — работа ещё не начиналась.  
\- \`GENERATED\` — есть первая сгенерированная версия.  
\- \`REVIEW\` — идёт визуальный review / нужны правки.  
\- \`APPROVED\` — визуал принят.  
\- \`PREPARED\` — утверждённый runtime-ассет и его provenance подготовлены, но production owner ещё не использует его.
\- \`BOUND\` / \`INTEGRATED\` — ассет связан с production ID; итоговый gameplay-scale review ещё может быть открыт.
\- \`IN GAME\` — ассет импортирован и реально используется в Unity.
\- \`DEFERRED\` — отдельный ассет сейчас не требуется; возвращаться к нему только при появлении указанного UI/gameplay need.

\`APPROVED\` и \`IN GAME\` — разные состояния.

Статусы в этом документе описывают только visual inventory. Execution order, готовность IP и открытые gates определяет только [Implementation Status](../implementation/STATUS.md). Подготовленный или подключённый арт не закрывает production binding, gameplay-scale review или ручной прогон автоматически.

### Текущий срез — 2026-09-27

- Обязательный raster-каталог для уже реализованных gameplay owners подготовлен: тела, world-art, pickups, UI-иконки, thumbnails, ground textures и obstacle props имеют source/master/provenance/runtime records по своим пакетам.
- Основной незакрытый слой — production bindings для ещё не собранных owners и ручная проверка на реальном масштабе/скорости. Это не backlog повторной генерации изображений.
- FIELD-004…010 уже имеют утверждённые thumbnails, ground textures и по шесть obstacle props. Их production definitions, layouts и bindings принадлежат IP-23/IP-24.
- Отдельные Character Select portraits, meta currency art, meta-upgrade icons и pickup UI icons не входят в обязательный backlog, пока существующий body/world sprite либо обычный UI достаточно хорошо выполняет роль.
- Store/marketing art остаётся отдельным поздним слоем и не считается недостающим gameplay art.
- UI visual language «Полевой фолиант» утверждён в DECISION-0081. Его первый
  production pass начинается с композиции и информационной иерархии по UI/UX,
  затем собирается темой UI Toolkit, типографикой, формами и уже
  подготовленными иконками; он не открывает автоматический backlog raster-рамок,
  portraits или декоративных menu backgrounds. Целевой review slice —
  HUD → Draft → Pause / Build.

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

Длинные generation prompts в этом документе не хранятся. Здесь остаются только brief, production method и status.

\---

\# 1\. Playable Characters

Минимум для каждого персонажа:  
\- body sprite;  
\- для Character Select сначала использовать crop/variant body sprite; отдельный portrait генерировать только если это выглядит недостаточно хорошо.

| ID | Character | Asset | Method | Status | Notes |  
|---|---|---|---|---|---|  
| CHAR-001 | Клёпка | Body sprite | Generate via GPT | IN GAME — v002 | Концепт связан с fixture goblin; production CHAR-001 binding поставлен в F1-03, текущий вид принят 2026-09-24 |
| CHAR-001…010 | Character Select image | Reuse body sprite first | Reuse/crop | DEFERRED | Использовать crop/variant соответствующего body sprite; отдельный portrait создавать только по результату target-scale UI review |
| CHAR-002 | Бугор | Body sprite | Generate via GPT | APPROVED | Утверждён 2026-09-26; master и runtime подготовлены, production binding и gameplay-scale review ожидают IP-22 |
| CHAR-003 | Шепотка | Body sprite | Generate via GPT | APPROVED | Утверждён 2026-09-26; master и runtime подготовлены, production binding и gameplay-scale review ожидают IP-22 |
| CHAR-004 | Тётка Шмыга | Body sprite | Generate via GPT | APPROVED | Утверждён 2026-09-26; master и runtime подготовлены, production binding и gameplay-scale review ожидают IP-22 |
| CHAR-005 | Бабка Искра | Body sprite | Generate via GPT | APPROVED | Утверждён 2026-09-26; master и runtime подготовлены, production binding и gameplay-scale review ожидают IP-22 |
| CHAR-006 | Гром | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Огр; отдельный крупный силуэт, 210 PPU; production binding и gameplay-scale review ожидают IP-22 |
| CHAR-007 | Дед Вертун | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Худой пожилой бегун с короткой палкой поперёк корпуса; production binding и gameplay-scale review ожидают IP-22 |
| CHAR-008 | Тётушка Светляк | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Два компактных фонаря; production binding и gameplay-scale review ожидают IP-22 |
| CHAR-009 | Иголка | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Подростковый силуэт с разведёнными руками; production binding и gameplay-scale review ожидают IP-22 |
| CHAR-010 | Старшой Ночка | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Ветеран в разномастной броне; production binding и gameplay-scale review ожидают IP-22 |

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
| ENEMY-001 | Селянин с вилами | Body sprite | Generate via GPT + procedural motion | IN GAME — v002, production binding и текущий вид приняты 2026-09-24. [Provenance](../../Art/Source/Enemies/enemy-001/asset-record.json) |
| ENEMY-002 | Деревенский гонец | Body sprite | Generate via GPT + procedural motion | IN GAME — v002, production binding и текущий вид приняты 2026-09-24; v001 отклонён как испуганный и слишком похожий на playable goblin. [Provenance](../../Art/Source/Enemies/enemy-002/asset-record.json) |
| ENEMY-003 | Дровосек | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Enemies/enemy-003/body/asset-record.json) |
| ENEMY-004 | Пращник | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Enemies/enemy-004/body/asset-record.json) |
| ENEMY-005 | Королевский лучник | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Enemies/enemy-005/body/asset-record.json) |
| ENEMY-006 | Арбалетчик | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-26; снаряд переиспользует ENEMY-005. [Provenance](../../Art/Source/Enemies/enemy-006/body/asset-record.json) |
| ENEMY-007 | Охотничья гончая | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Enemies/enemy-007/body/asset-record.json) |
| ENEMY-008 | Конный разведчик | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-26. [Provenance](../../Art/Source/Enemies/enemy-008/body/asset-record.json) |
| ENEMY-009 | Щитоносец ополчения | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-26. [Provenance](../../Art/Source/Enemies/enemy-009/body/asset-record.json) |
| ENEMY-010 | Гвардейский стрелок | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-010/body/asset-record.json) |
| ENEMY-011 | Боевой капеллан | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-011/body/asset-record.json) |
| ENEMY-012 | Королевский копейщик | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-012/body/asset-record.json) |
| ENEMY-013 | Охотник на чудовищ | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-013/body/asset-record.json) |
| ENEMY-014 | Осадный маг | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-014/body/asset-record.json) |
| ENEMY-015 | Инквизитор | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-015/body/asset-record.json) |
| ENEMY-016 | Латный рыцарь | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-016/body/asset-record.json) |
| ENEMY-017 | Рыцарь-дуэлянт | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-017/body/asset-record.json) |
| ENEMY-018 | Придворный чародей | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-018/body/asset-record.json) |
| ENEMY-019 | Серафим-страж | Body sprite | Generate via GPT | BOUND — v001, принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-019/body/asset-record.json) |
| ENEMY-020 | Ангел-каратель | Body sprite | Generate via GPT | BOUND — v001 (candidate-05), принят 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Enemies/enemy-020/body/asset-record.json) |

\#\#\# Enemy attack visuals  
Отдельные projectile/effect assets добавляются только для реально используемых ranged patterns. Максимально переиспользовать общие семейства:  
\- arrow;  
\- crossbow bolt;  
\- sling stone;  
\- magic bolt;  
\- holy/light projectile;  
\- spear/thrust accent where needed.

Не создавать уникальный projectile для каждого врага, если визуально допустим reuse.

Пакет поздних ranged enemies утверждён и подключён 2026-09-27: `ENEMY-010/011/012/014/015/018/019-VISUAL-PROJECTILE`, все v001. Отдельные силуэты покрывают arrow, holy orb, javelin, explosive orb, crossfire bolt, spiral token и seraph fan shot; gameplay-scale review открыт. [Evidence](../implementation/evidence/2026-09-27-enemy-projectile-art.md).

**Правило читаемости вражеских снарядов** ([DECISION-0055](../decisions/0055-playtest-2026-09-24-fixes.md), плейтест 2026-09-24 OBS-06): каждый hostile projectile (обычные враги, боссы, Путники) обязан читаться на поле с первого взгляда и отличаться от снарядов игрока минимум двумя признаками (Art Direction §12):

- под спрайтом — приглушённый пульсирующий coral-red ореол (`projectile.threatHalo` в `FixtureSprites.json`), диаметр не меньше ≈2.5× collision diameter; sprite остаётся выше ореола по sorting order;
- тёмный или мелкий снаряд без ореола не допускается; если снаряд теряется даже с ореолом, увеличить `visualScale` (так камень пращи 1.9 → 2.4);
- при добавлении нового hostile projectile проверить его в игре на траве FIELD-001 среди skill-эффектов; тест `HostileProjectiles_AllHaveThreatHalo` требует ореол у всех назначенных врагам visual.

\---

\# 3\. Bosses

| ID | Boss | Asset | Method | Status |  
|---|---|---|---|---|  
| BOSS-001 | Староста-герой | Body sprite | Generate via GPT | IN GAME — body/projectile v001, приняты 2026-09-24. [Body provenance](../../Art/Source/Bosses/boss-001/body/asset-record.json), [projectile provenance](../../Art/Source/Bosses/boss-001/projectile/asset-record.json) |
| BOSS-002 | Капитан королевской стражи | Body sprite | Generate via GPT | IN GAME — body v001 утверждён и подключён 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-002/body/asset-record.json) |
| BOSS-003 | Главный королевский ловчий | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-003/body/asset-record.json) |
| BOSS-004 | Рыцарь знамени | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-004/body/asset-record.json) |
| BOSS-005 | Великий инквизитор | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-005/body/asset-record.json) |
| BOSS-006 | Королевский архимаг | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-006/body/asset-record.json) |
| BOSS-007 | Верховный паладин | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-007/body/asset-record.json) |
| BOSS-008 | Чемпион короны | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-008/body/asset-record.json) |
| BOSS-009 | Серафим-полководец | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-009/body/asset-record.json) |
| BOSS-010 | Архангел Спасения | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/boss-010/body/asset-record.json) |

Boss attack VFX/projectiles создаются по конкретным attack patterns из Content Design и могут переиспользовать generic VFX families. Девять shared projectile families FIELD-002…010 утверждены и подключены 2026-09-27: final boss владеет visual ID, соответствующий midboss поля переиспользует его; MIDBOSS-004 projectile не использует. Provenance: [002](../../Art/Source/Bosses/boss-002/projectile/asset-record.json), [003](../../Art/Source/Bosses/boss-003/projectile/asset-record.json), [004](../../Art/Source/Bosses/boss-004/projectile/asset-record.json), [005](../../Art/Source/Bosses/boss-005/projectile/asset-record.json), [006](../../Art/Source/Bosses/boss-006/projectile/asset-record.json), [007](../../Art/Source/Bosses/boss-007/projectile/asset-record.json), [008](../../Art/Source/Bosses/boss-008/projectile/asset-record.json), [009](../../Art/Source/Bosses/boss-009/projectile/asset-record.json), [010](../../Art/Source/Bosses/boss-010/projectile/asset-record.json). Зоны и лучи остаются процедурными; gameplay-scale review на owning fields открыт.

\---

\# 4\. Mid-Bosses

| ID | Mid-boss | Asset | Method | Status |  
|---|---|---|---|---|  
| MIDBOSS-001 | Старший загонщик | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Bosses/midboss-001/body/asset-record.json) |
| MIDBOSS-002 | Сержант стражи | Body sprite | Generate via GPT | IN GAME — body v001 утверждён и подключён 2026-09-26; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-002/body/asset-record.json) |
| MIDBOSS-003 | Королевский следопыт | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-003/body/asset-record.json) |
| MIDBOSS-004 | Рыцарь-преследователь | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-004/body/asset-record.json) |
| MIDBOSS-005 | Капитан городской стражи | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-005/body/asset-record.json) |
| MIDBOSS-006 | Маг-наблюдатель | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-006/body/asset-record.json) |
| MIDBOSS-007 | Паладин авангарда | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-007/body/asset-record.json) |
| MIDBOSS-008 | Рыцарь короны | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-008/body/asset-record.json) |
| MIDBOSS-009 | Вестник небес | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-009/body/asset-record.json) |
| MIDBOSS-010 | Ангел-предвестник | Body sprite | Generate via GPT | INTEGRATED — body v001 утверждён и подключён 2026-09-27; gameplay-scale review открыт. [Provenance](../../Art/Source/Bosses/midboss-010/body/asset-record.json) |

\---

\# 5\. Travelers / Путники

| ID | Traveler | Asset | Method | Status |  
|---|---|---|---|---|  
| TRAVELER-001 | Дорожный громила | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Travelers/traveler-001/body/asset-record.json) |
| TRAVELER-002 | Бродячий стрелок | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Travelers/traveler-002/body/asset-record.json) |
| TRAVELER-003 | Странствующий копейщик | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Компактное копьё поперёк корпуса; production binding и gameplay-scale review ожидают IP-30 |
| TRAVELER-004 | Наёмный дуэлянт | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Production binding и gameplay-scale review ожидают IP-30 |
| TRAVELER-005 | Паломник со щитом | Body sprite | Generate via GPT | IN GAME — v001, принят 2026-09-24. [Provenance](../../Art/Source/Travelers/traveler-005/body/asset-record.json) |
| TRAVELER-006 | Дорожный маг | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Production binding и gameplay-scale review ожидают IP-30 |
| TRAVELER-007 | Путевой инквизитор | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Production binding и gameplay-scale review ожидают IP-30 |
| TRAVELER-008 | Рыцарь-странник | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Production binding и gameplay-scale review ожидают IP-30 |
| TRAVELER-009 | Небесный паломник | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Production binding и gameplay-scale review ожидают IP-30 |
| TRAVELER-010 | Ангел-скиталец | Body sprite | Generate via GPT | APPROVED — v001; master/runtime подготовлены 2026-09-27 | Сложенная пара крыльев; production binding и gameplay-scale review ожидают IP-30 |

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
| SKILL-001 | Бросок камня | Stone projectile; shared procedural impact | Generate via GPT \+ Hybrid | IN GAME — projectile v001, production binding, rotation и общий impact подключены. [Provenance](../../Art/Source/Skills/skill-001/asset-record.json) |
| SKILL-002 | Веер игл | Needle projectile | Derived from approved icon \+ Unity fan pattern | IN GAME — v001 выделен из approved icon и подключён к production fan pattern (DECISION-0054) |
| SKILL-003 | Орбитальные клинки | Blade sprite | Generate via GPT \+ Unity orbit | IN GAME — v001 и visual-only orbit подключены |
| SKILL-004 | Импульсная волна | Expanding pulse/ring | Procedural in Unity / Hybrid texture | IN GAME — процедурное кольцо подключено через `SkillWorldEffects.json` |
| SKILL-005 | Ветряное копьё | Wind spear projectile | Derived from approved icon \+ Unity motion | IN GAME — v001 из approved icon подключён к production projectile |
| SKILL-006 | Бумеранг | Boomerang projectile | Generate via GPT \+ Unity return path | IN GAME — v001 и return presentation подключены |
| SKILL-007 | Цепная молния | Lightning chain \+ hit flash | Procedural in Unity / Hybrid | IN GAME — процедурные сегменты цепи подключены |
| SKILL-008 | Рикошетный диск | Disk projectile | Generate via GPT \+ Unity ricochet | IN GAME — v001 и ricochet presentation подключены |
| SKILL-009 | Магматическая мина | Mine sprite \+ explosion base | Hybrid | IN GAME — v001 и общий explosion presenter подключены; ручная проверка на реальной скорости открыта |
| SKILL-010 | Небесный удар | Telegraph marker \+ strike/impact | Hybrid | IN GAME — telegraph disc, предварительный столб света и impact подключены по [DECISION-0058](../decisions/0058-on-screen-targeting-and-strike-visual.md) |
| SKILL-011 | Спираль осколков | Shard projectile | Generate via GPT \+ Unity spiral pattern | IN GAME — v001 подключён; ручная проверка на реальной скорости открыта |
| SKILL-012 | Пульсирующий луч | Beam base visual | Procedural in Unity | IN GAME — процедурная полоса подключена через `SkillWorldEffects.json`; ручная проверка читаемости открыта |
| SKILL-013 | Ледяные осколки | Ice shard projectile \+ optional ice impact | Hybrid | IN GAME — v001 из approved icon и общий impact подключены |
| SKILL-014 | Взрывные сферы | Sphere projectile \+ explosion base | Hybrid | IN GAME — v001 и общий explosion presenter подключены |
| SKILL-015 | Крест клинков | Blade/wave visual | Hybrid; cross pattern in Unity | IN GAME — v001 подключён; ручная проверка на реальной скорости открыта |
| SKILL-016 | Разбрасыватель мусора | Small trash projectile set | Generate via GPT \+ Unity motion | IN GAME — v001 подключён; ручная проверка на реальной скорости открыта |

\#\#\# Skill UI icons  
Нужно 16 icons:  
\`SKILL-001 ... SKILL-016\`

Method: \`Generate via GPT\`    
Status: \`IN GAME — v001; 16 masters/runtime imports и production bindings подключены; ручная проверка конкретных world visuals перечислена в IP-17\`

\---

\# 7\. Passive Items

World sprite для passive item по умолчанию не нужен.

Нужно 14 UI icons:

| ID | Passive | Asset | Method | Status |  
|---|---|---|---|---|  
| PASSIVE-001 | Крепкое сердце | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-002 | Собиратель | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-003 | Лёгкие сапоги | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-004 | Точильный камень | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-005 | Метроном | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-006 | Эхо памяти | Icon | Generate via GPT | IN GAME — v001; target-scale slot review открыт |
| PASSIVE-007 | Магнит опыта | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-008 | Закалённая кожа | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-009 | Лечебная настойка | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-010 | Талисман ученика | Icon | Generate via GPT | IN GAME — v001; target-scale slot review открыт |
| PASSIVE-011 | Тяжёлый пояс | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-012 | Широкий замах | Icon | Generate via GPT | IN GAME — v001 |
| PASSIVE-013 | Длинные руки | Icon | Generate via GPT | IN GAME — v001; target-scale slot review открыт |
| PASSIVE-014 | Упрямство | Icon | Generate via GPT | IN GAME — v001; target-scale slot review открыт |

Постоянные world-aura для пассивок не создавать без отдельной gameplay/readability причины.

\---

\# 8\. Sets

Для каждого set нужен UI icon.

Нужно 20 icons:  
\`SET-001 ... SET-020\`

Дополнительный low-tier пакет: 15 отдельных иконок `SET-021…035`, утверждённых пользователем 2026-10-02;
[review v001](../implementation/proposals/2026-10-02-low-tier-set-icons/review.md),
[арт-пакет](../../Art/Packets/low-tier-set-icons-2026-10-02.json).
Для импорта применяется тот же icon contract; execution status — только [STATUS](../implementation/STATUS.md).

Method: \`Generate via GPT\`    
Status: \`IN GAME — v001; 20 masters/runtime imports и production bindings подключены; ручной обзор set-progress и сочетания 3–4 сетов остаётся\`

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
| SET-008 Утилизатор | Heavy trash replacement projectile \+ explosion; отдельный projectile v001 подключён | Hybrid |
| SET-009 Линия пробоя | Existing linear attacks amplified | Procedural / Hybrid |  
| SET-010 Холодная орбита | Cold orbit/slow accent | Hybrid |  
| SET-011 Кинетический арсенал | Speed/trail/impact amplification | Procedural / Hybrid |  
| SET-012 Неподвижная крепость | Defensive/impact accent | Procedural / Hybrid |  
| SET-013 Перегрузка сети | Distinct electrical network burst | Hybrid |  
| SET-014 Танец клинков | Existing blades/shards amplified | Procedural / Hybrid |  
| SET-015 Алхимия хаоса | Potion-triggered explosion | Reuse/variant explosion VFX |  
| SET-016 Выстрел великана | Huge bolt projectile v001 подключён | Generate via GPT \+ Unity motion |
| SET-017 Падающая звезда | Telegraph \+ large strike | Hybrid |  
| SET-018 Сфера разрушения | Large slow sphere v001 подключена; explosion использует общий presenter | Hybrid |
| SET-019 Ледяное копьё | Huge ice shard v001 подключён | Generate via GPT \+ Hybrid |
| SET-020 Каменное ядро | Massive boulder v001 подключён | Generate via GPT \+ Hybrid |
| SET-021 Камешек в сапоге | Approved SKILL-001 stone projectile; размер от set collider, общий spin/impact | Reuse |
| SET-022 Хлопушка | Только расходящаяся дуга, без прямых сторон; authoritative radius/angle/direction, pool/travel/fade | Procedural in Unity |

Generic rule: set effects должны быть вторичным визуальным слоем и не забивать основные active skills.

\---

\# 9\. Pickups / Drops

| ID / Entity | Asset | Method | Status | Notes |  
|---|---|---|---|---|  
| XP pickup | World sprite | Generate via GPT + procedural bob/pulse | IN GAME — v001 | Один cyan crystal; production pickup presentation подключён |
| PICKUP-001 Healing Potion | World sprite | Generate via GPT + procedural bob/pulse | IN GAME — v001, текущий вид принят 2026-09-24 | Зелёное зелье; production binding подключён |
| Traveler Book | World sprite | Generate via GPT + procedural bob/pulse | IN GAME — v001, текущий вид принят 2026-09-24 | Закрытая охристо-бордовая книга; production card/ID/параметры полного IP-30 остаются отдельным content gate |
| Meta currency | UI icon / optional world art | Generate via GPT | DEFERRED | Использовать обычный UI до утверждения необходимости отдельного изображения; точный presentation зависит от meta UI |

\---

\# 10\. Generic VFX

Эти эффекты могут переиспользоваться между многими сущностями.

| VFX | Method | Status | Notes |  
|---|---|---|---|  
| Basic hit flash | Procedural in Unity | IN GAME (fixture player) | Existing player Health.Damaged → animator; generic adapter tested separately, not all production owners |
| Basic impact spark | Procedural in Unity | IN GAME (fixture projectiles) | Общий короткий flash + material-colored particles; отдельный raster пока не нужен |
| Slash impact | Hybrid | DEFERRED | Blade/wave world visuals уже подключены; отдельный generic impact добавлять только если gameplay review покажет нехватку feedback |
| Generic explosion | Procedural in Unity | IN GAME | Общий `ExplosionBurstRuntime` переиспользуется sphere/mine/set projectiles; отдельный raster не нужен |
| Lightning impact | Procedural in Unity | IN GAME / PER-OWNER | SKILL-007 использует процедурную цепь и hit presentation; отдельный generic raster не нужен |
| Ice impact | Hybrid | DEFERRED | SKILL-013 использует общий impact; отдельный ice raster только по результату gameplay review |
| Heal effect | Hybrid / Procedural | QUESTIONABLE | Может не понадобиться; делать только после отдельного gameplay review |
| Level-up effect | Hybrid / Procedural | QUESTIONABLE | Может не понадобиться; делать только после отдельного gameplay review |
| Set activation effect | Hybrid | PER-SET / REVIEW | Все 20 set effects реализованы; общий дополнительный accent не обязателен, нужна ручная проверка сочетания 3–4 сетов |
| Enemy death effect | Procedural in Unity | IN GAME | Общий squash/fade + земляная пыль для ordinary/boss/Traveler |
| Slow feedback | Procedural / Hybrid | OPEN POLISH | Gameplay slow работает; отдельный tint/overlay не заявлен как обязательный raster |
| Projectile trail | Procedural / Hybrid | IN GAME / PER-OWNER | Enemy projectile trail и reset реализованы; дополнительные player/set trails добавляются только по необходимости читаемости |
| Beam base | Procedural in Unity | IN GAME | SKILL-012 и boss beams используют procedural presentation; отдельная texture не требуется |
| Aura base | Hybrid | OPEN POLISH | Защитные mechanics работают; дополнительная pulse/rotation presentation требует отдельного owning need |
| Telegraph marker | Procedural in Unity | IN GAME | Skill strikes, enemy attacks/dashes, boss hazards и teleport используют procedural telegraphs |
| Screen shake | Procedural in Unity | IN GAME | Runtime и пользовательская настройка подключены; отдельное изображение не требуется |
| Damage numbers | Procedural UI/Text | OPEN POLISH | Отдельное изображение не требуется; добавлять только отдельным UI/presentation packet |

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
| FIELD-001 | Деревенская окраина | Ground tile + плетень + пень + бочка + переиспользованный камень FIELD-002 + куст/трава; production geometry и thumbnail | Generate via GPT / Hybrid | Thumbnail v001 IN GAME, принят 2026-09-24; плотность и бочка подготовлены по [DECISION-0069](../decisions/0069-field001-feedback-tuning.md), gameplay-scale review открыт. [Provenance thumbnail](../../Art/Source/Fields/field-001/background/asset-record.json), [бочка](../../Art/Source/Fields/field-001/barrel/asset-record.json) |
| FIELD-002 | Пограничные руины (до DECISION-0136 — Королевский тракт) | Ground/background \+ decor pack \+ obstacle pack | Generate via GPT / Hybrid | Ground, boulder, колонна, святилище и thumbnail v001 подключены 2026-09-26; DECISION-0136: слот FIELD-002 теперь показывает руины — `FIELD-002-VISUAL-BACKGROUND` указывает на исходник Art/Source/Fields/field-003/background, фон земли — `FIELD-003-VISUAL-GROUND`; gameplay-scale review открыт. [Evidence](../implementation/evidence/2026-09-26-field002-art.md) |
| FIELD-003 | Королевский тракт (до DECISION-0136 — Пограничные руины) | Целевой набор DECISION-0137: основная дорога + разбитая дорога тупиков + reuse травы FIELD-001 + читаемая непроходимая кромка; decor/thumbnail по игровому review | Generate via GPT / Hybrid / Reuse | Принята схема дорог v4 2026-10-02. Первый road runtime показывает цветные mesh основной дороги/тупиков поверх `FIELD-001-VISUAL-GROUND`; это blockout для проверки движения. Новый игровой арт готовится после геометрии по [R03-06](../implementation/milestones/FIELD-003-road-network.md#r03-06--покрытия-кромка-и-окружение). Thumbnail `FIELD-003-VISUAL-BACKGROUND` пока сохраняет исходник Art/Source/Fields/field-002/background. Историческое [evidence](../implementation/evidence/2026-09-27-field003-art-and-projectile-halo.md) не подтверждает новый scope. |
| FIELD-004 | Рыцарский лагерь | Ground/background \+ camp decor/obstacles | Generate via GPT / Hybrid | PREPARED — thumbnail, ground и 6 obstacle props v001 утверждены; production definition/layout/binding и target-scale review открыты |
| FIELD-005 | Королевская столица | Ground/background \+ city decor/obstacles | Generate via GPT / Hybrid | PREPARED — thumbnail, ground и 6 obstacle props v001 утверждены; production definition/layout/binding и target-scale review открыты |
| FIELD-006 | Академия магов | Ground/background \+ magical decor/obstacles | Generate via GPT / Hybrid | PREPARED — thumbnail, ground и 6 obstacle props v001 утверждены; production definition/layout/binding и target-scale review открыты |
| FIELD-007 | Монастырские сады | Ground/background \+ garden/religious decor | Generate via GPT / Hybrid | PREPARED — thumbnail, ground и 6 obstacle props v001 утверждены; production definition/layout/binding и target-scale review открыты |
| FIELD-008 | Цитадель короны | Ground/background \+ fortress decor/obstacles | Generate via GPT / Hybrid | PREPARED — thumbnail, ground и 6 obstacle props v001 утверждены; production definition/layout/binding и target-scale review открыты |
| FIELD-009 | Небесные врата | Ground/background \+ celestial decor/obstacles | Generate via GPT / Hybrid | PREPARED — thumbnail, ground и 6 obstacle props v001 утверждены в cool white-blue palette; production definition/layout/binding и target-scale review открыты |
| FIELD-010 | Чертог Спасения | Ground/background \+ final celestial/interior kit | Generate via GPT / Hybrid | PREPARED — sunset thumbnail, отдельный sunset ground и 6 obstacle props v001 утверждены; production definition/layout/binding и target-scale review открыты |

Общие evidence подготовленных поздних полей: [thumbnails](../implementation/evidence/2026-09-27-field004-010-thumbnails.md), [obstacle props](../implementation/evidence/2026-09-27-field-obstacle-art.md), [ground textures](../implementation/evidence/2026-09-27-field004-010-ground-textures.md).

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
| Menu E background + adult Shepotka foreground | Approved generated layers, UI parallax | IN GAME | `Art/Packets/ui-entry-r1.json`; two full-canvas sprites, procedural light/dust; approval in menu-shepotka-review. Runtime visual acceptance tracked only in STATUS. No CHAR-003 redesign. |
| Folio panel material | Generated matte fold texture | IN GAME — v001 | `UI-FOLIO-SURFACE-VISUAL-BACKGROUND`; one stretched layer on large windows, including Pause inner columns, at 0.48 opacity; no repeated tile or baked UI. Integrated visual review remains in STATUS. |
| 16 skill icons | Generate via GPT | IN GAME — v001 | 16 masters/runtime imports и production bindings; draft и Pause / Build используют полный каталог |
| 14 passive icons | Generate via GPT | IN GAME — v001 | 14 masters/runtime imports и production bindings; для PASSIVE-006/010/013/014 открыт target-scale slot review |
| 20 set icons | Generate via GPT | IN GAME — v001 | 20 masters/runtime imports и production bindings; открыт set-progress review и совместный обзор 3–4 сетов |
| Character selection image | Reuse body sprite first | DEFERRED | Использовать crop/variant существующих 10 body sprites; отдельный portrait только если target-scale review выявит проблему |
| Field thumbnails | Generate / derive from field art | FIELD-001…003 IN GAME; FIELD-004…010 PREPARED — v001 | Все 10 thumbnails имеют runtime PNG и registered visual ID. FIELD-004…010 будут связаны с Field Select при реализации production definitions; target-scale UI review остаётся. [Evidence](../implementation/evidence/2026-09-27-field004-010-thumbnails.md) |
| Meta-upgrade icons | Generate via GPT | META-003…014, утверждённый пакет ui-meta-stat-icons-r1 | 12 простых значков перед названиями; исполнение и проверки — STATUS |
| Pickup icons if UI needs separate icon | Reuse world sprite / Generate if needed | DEFERRED | Сначала переиспользовать world sprite; отдельный asset не создавать без доказанной UI-проблемы |

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
\- Неполученный playable body показывается одноцветным силуэтом в каталоге и крупном просмотре; отдельный PNG не нужен. Цветной body раскрывается после получения, не просто выполнения условия покупки (DECISION-0087).
\- Character Select показывает только значимые stat modifiers; отдельные art assets под полный stat table не нужны.  
\- Pause / Build показывает все meta-открытые достижимые recipes, включая `0/N`, по UI §10/DECISION-0086; acquired/missed отдельно. Новый full-set-compendium UI/art не нужен.
\- Traveler HP bar является обязательным procedural UI для всех Travelers.  
\- Retry — обычная кнопка; отдельного confirmation screen/art нет. Нажатие сразу запускает новый run с теми же character и field.  
\- Reroll/Banish могут быть обычными текстовыми кнопками с простым icon только при необходимости; отдельные generated icons не обязательны.  
\- Set recipe readability строится прежде всего на существующих skill/passive/set icons \+ text/checkmarks, а не на дополнительных уникальных картинках.

По [DECISION-0083](../decisions/0083-player-ui-layout-and-dev-boundary.md) style
approval не утверждает временный layout. Чистовой review использует production
иконки, настоящий текст и игровое поле; отдельный fixture long-text capture
не заменяет review. Player HP расположен возле персонажа, speed controls
исключены из player-facing композиции. Планы работ — в IP-10A/IP-26, статус —
только в STATUS.

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
6\. Минимальные generic VFX: hit и death; heal и level-up остаются под вопросом и не производятся без отдельного gameplay review.
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

[Manifest](../../Art/asset-manifest.json) фиксирует owner+role, method, source/runtime, stage и evidence. CHAR-001 concept identity подтверждена пользователем: это master fixture goblin v002; runtime остаётся FIXTURE-CHARACTER-AGILE, production binding отдельно IP-22. UI body reuse в диагностическом слоте принят пользователем 2026-09-21; production Character Select binding проверяется отдельно. Шесть procedural diagnostic roles (body/projectile/pickup/telegraph/shadow/impact) приняты пользователем в Presentation Fixture Review («всё хорошо», 2026-09-21); они не заменяют production строки выше. Gameplay player/enemy shadow теперь использует общий procedural runtime по [ASSET_PIPELINE §24](ASSET_PIPELINE.md#24-единая-процедурная-ground-shadow). Полное исполнение IP учитывается только в STATUS.

## Body contact authoring

Для новых и заменяемых character/enemy body сначала проверить silhouette compatibility с одним кругом: основная масса не должна быть крайне вытянутой, а конечности, оружие и аксессуары — чрезмерно далеко выступать от корпуса. Это не требование делать персонажей круглыми; отклоняются только крайности, при которых круг покрывает малую часть фигуры или возникает слишком большое визуальное пересечение до контакта. После runtime import выполнить [ASSET_PIPELINE §22](ASSET_PIPELINE.md#22-подгонка-круга-контакта-для-world-body): максимальный вписанный круг по заполненному внешнему обводу, сохранение профиля, wiring, проверки и review. Принятый эталон — текущие goblin/villager; внутренние дырки не уменьшают круг. Production content gates сохраняются.

## Enemy death presentation

Единый death algorithm выполняется процедурно в Unity для всех врагов: squash, shrink, darken/fade и небольшой dust burst. Специальные варианты по enemy ID и отдельные death sprites сейчас не производятся. Death не добавляет толчок. Параметры и обязательные проверки описаны в [ASSET_PIPELINE §23](ASSET_PIPELINE.md#23-единая-процедурная-смерть-врагов).

## Gameplay ground shadow

Гоблин, обычные враги, боссы и Travelers используют одну мягкую процедурную ellipse shadow без отдельных PNG. Маска создаётся один раз и переиспользуется всеми renderer; размер, цвет и смещение задаёт общий JSON-профиль. Правила и проверки описаны в [ASSET_PIPELINE §24](ASSET_PIPELINE.md#24-единая-процедурная-ground-shadow).

## Gameplay projectile presentation

У текущего `FIXTURE-ENEMY-FAN` используется утверждённый компактный снаряд-письмо: cream parchment, тёмный шнур и burgundy seal. Это fixture-only ranged presentation и не меняет approved melee card ENEMY-002. Письмо ориентируется по направлению полёта без spin; попадание использует тот же дешёвый impact algorithm с parchment-colored particles. [Provenance](../../Art/Source/Enemies/fixture-enemy-fan-projectile/asset-record.json). Камень SKILL-001 временно подключён к `FIXTURE-SKILL-BOLT`; его production binding остаётся IP-17. Общие правила описаны в [ASSET_PIPELINE §25](ASSET_PIPELINE.md#25-projectile-sprite-вращение-и-дешёвый-impact).

## Стартовый состав — DECISION-0050

Утверждено 2026-09-22: [состав и условия](../Content_design.md#starting-content-0050),
[основание](../decisions/0050-starting-content-and-unlocks.md).
Стартовый playable — CHAR-001; Путники FIELD-001 — TRAVELER-001 (боевой),
TRAVELER-002 (неагрессивный), TRAVELER-005 (защитник). Использовать существующие
ID и роли inventory; не создавать три новых ID или отдельные версии для каждого
поля. Остальные семь Путников и полный roster остаются для дальнейшего контента.
Начальные skills/passives/sets определены таблицей CD; поздние открытия доступны
и на FIELD-001. Уже одобренные изображения полного каталога сохраняются.
Этот выбор не утверждает ещё отсутствующие Traveler assets. Scope стартового
этапа — [FIELD-001 initial slice](../implementation/milestones/FIELD-001-start.md);
порядок производства и готовность — только в STATUS.
Art Direction, Asset Pipeline и принятые visual gates сохраняются.

### Ordinary enemies стартового поля — DECISION-0052

По [DECISION-0052](../decisions/0052-field001-six-ordinary-enemies.md) стартовый
art scope включает шесть обычных врагов: ENEMY-001…005 и ENEMY-007.
К существующему набору добавлены body Королевского лучника и Охотничьей гончей,
нужный projectile/impact Лучника и читаемая подача рывка Гончей. Переиспользовать
подходящие общие роли; отдельный raster для каждого эффекта не обязателен.
Существующие inventory IDs и per-image gates сохраняются; новые изображения
этим решением не объявляются созданными или approved.

## FIELD-001…010 obstacle props — 2026-09-27

Утверждён и подготовлен единый пакет из 52 obstacle props: недостающие роли
FIELD-001…003 и полные наборы 3 small / 1 medium / 2 large для FIELD-004…010.
Все варианты имеют отдельные source/master/provenance/runtime records и роль
`SpriteRole.Prop`; small/medium/large используют 160/128/96 PPU при runtime 256×256.
FIELD-001…003 применяют approved additions через optional per-piece visual override,
не выводя collider из sprite. FIELD-004…010 entries зарегистрированы без ложных
production bindings до появления данных полей. Подробности и проверки:
[evidence](../implementation/evidence/2026-09-27-field-obstacle-art.md).

## FIELD-004…010 ground textures — 2026-09-27

Семь утверждённых top-down ground textures подготовлены как `SpriteRole.Tile`:
земля рыцарского лагеря, мостовая столицы, холодный камень академии, мшистый
монастырский сад, тёмные плиты цитадели, светлый небесный камень и отдельный
закатный мрамор FIELD-010. Каждый tile использует 64 PPU и 512 max import size.
Visual IDs зарегистрированы без production bindings до появления presentation data
FIELD-004…010. Подробности и проверки:
[evidence](../implementation/evidence/2026-09-27-field004-010-ground-textures.md).

## Slow-status ice texture — 2026-09-30

Утверждён один общий прозрачный ice mask `SLOW-STATUS-VISUAL-MASK` для ordinary, boss и
Traveler bodies. Master и prompt: `Art/Source/VFX/slow-status/ice/`; runtime:
`Assets/Resources/Art/VFX/slow-status-ice-mask.png` (`SpriteRole.Mask`, 512×512).
Шейдер ограничивает рисунок альфой body sprite, поэтому отдельные файлы для персонажей не
нужны. Полоска времени остаётся процедурной. Техническая подготовка и проверки:
[evidence](../implementation/evidence/2026-09-30-slow-ice-runtime.md).
После просмотра в игре пользователь выбрал первый, более плотный вариант текстуры; он хранится
как `v002/concept-01.png` и заменяет v001 по тому же runtime path и visual ID. Лёд повторяет
размер body sprite, а полоска размещается ниже его нижней границы.
