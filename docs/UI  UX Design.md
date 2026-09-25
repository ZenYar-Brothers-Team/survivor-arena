\# UI / UX Design

\#\# Статус

Approved.

> Стартовый набор, unlocks и Путники FIELD-001 утверждены пользователем 2026-09-22: [DECISION-0050](decisions/0050-starting-content-and-unlocks.md). Численные TBD и production data/art gates сохраняются.


Этот таб задаёт функциональный дизайн интерфейсов Survival Arena. Цель — использовать простые, привычные и дешёвые в реализации UI-паттерны без сложных нестандартных экранов и анимаций.

Главный принцип:

\> Игрок должен быстро понимать текущее состояние забега, свой билд и пути к сетам, а интерфейс не должен отвлекать от движения и боя.

Визуальный стиль интерфейса определяется отдельно через \`Art Direction v2\` и \`Art Production\`. Здесь фиксируется прежде всего структура, информация и поведение.

\---

\# 1\. Общие принципы

\- Интерфейс должен быть стандартным и понятным без обучения.  
\- Во время обычного боя игрок не должен постоянно взаимодействовать с UI.  
\- Основное активное UI-взаимодействие во время run — level-up draft, Book draft и pause/build screen.  
\- HUD показывает только информацию, которая нужна постоянно.  
\- Подробности билда и рецептов сетов доступны на паузе и в draft.  
\- Не вводить отдельные сложные drag-and-drop, radial-menu или nested-menu механики.  
\- Для первой версии ориентироваться на PC: клавиатура \+ мышь.  
\- Управление персонажем остаётся только движением; ручного aim через UI нет.  
\- Controller support можно добавить позже без изменения базовой структуры экранов.

\---

\# 2\. Общий screen flow

\`\`\`text  
Launch  
  ↓  
Main Menu  
  ├─ Play  
  │    ↓  
  │  Character Select  
  │    ↓  
  │  Field Select  
  │    ↓  
  │  Run  
  │    ├─ Level-up Draft  
  │    ├─ Traveler Book Draft  
  │    ├─ Pause / Build  
  │    └─ Victory / Defeat  
  │          ↓  
  │       Run Results  
  │          ├─ Retry  
  │          └─ Main Menu  
  │  
  ├─ Meta Progression  
  ├─ Settings  
  └─ Exit  
\`\`\`

Для MVP этого flow достаточно.

\---

\# 3\. Main Menu

\#\# Обязательные элементы

\- \`Play\`  
\- \`Meta Progression\`  
\- \`Settings\`  
\- \`Exit\`

\#\# Поведение

\`Play\` открывает Character Select.

Не добавлять на первом этапе:  
\- news;  
\- daily rewards;  
\- complicated profile screen;  
\- online features;  
\- отдельный compendium, если он не нужен для текущего gameplay.

\---

\# 4\. Character Select

Экран показывает разблокированных и заблокированных playable characters.

\#\# Для выбранного персонажа показывать

\- для первой версии использовать crop/variant имеющегося body sprite; отдельный portrait генерировать только если такой вариант выглядит плохо;  
\- имя;  
\- starting active skill;  
\- основные отличия характеристик;  
\- короткое текстовое описание gameplay-роли.

Не нужно показывать весь внутренний набор параметров.

Показывать только значимые отклонения от baseline; полный набор точных внутренних характеристик на этом экране не нужен.

Baseline задаётся отдельными явными данными и не выбирается из первого или текущего персонажа. Данные персонажа содержат упорядоченный список отображаемых особенностей: автор контента явно выбирает, какие отличия показать, без автоматического процентного порога. Числовые отличия должны соответствовать характеристикам персонажа и baseline. Пустой список не дополняется автоматически. Это правило представления, оно не меняет gameplay stats. См. [DECISION-0026](decisions/0026-character-selection-baseline.md).

Например:

\`\`\`text  
HP: \+30%  
Move Speed: \-10%  
Starting Skill: Heavenly Strike  
\`\`\`

\#\# Locked character

Показывать:  
\- силуэт / затемнённую карточку;  
\- имя, если оно не является секретом;  
\- условие разблокировки.

\#\# Actions

\- выбрать персонажа;  
\- подтвердить и перейти к Field Select;  
\- Back.

\---

\# 5\. Field Select

Для каждого разблокированного поля показывать:

\- название;  
\- thumbnail или простое изображение;  
\- короткое описание;  
\- простой индикатор сложности;  
\- locked/unlocked state.

Для первой версии достаточно простой шкалы:

\`Difficulty: 1–5\`

или эквивалентных визуальных маркеров.

Не нужно заранее показывать полный список врагов, точные wave timings или boss stats.

\#\# Locked field

Показывать условие unlock.

\#\# Actions

\- выбрать field;  
\- Start Run;  
\- Back.

\---

\# 6\. Run HUD

HUD должен быть компактным и не перекрывать центр игрового поля.

\#\# Постоянно видимые элементы

\#\#\# Top center  
\- timer \`00:00 → 15:00\`.

\#\#\# Top / boss area  
\- boss HP bar только когда final boss активен.

\#\#\# Bottom / lower area  
\- player HP bar;  
\- XP bar;  
\- current level.
\- выбор скорости забега 1×, 2×, 3×, 5× рядом с кнопкой Pause; активная скорость выделена. Во время паузы и после завершения кнопки недоступны, после продолжения сохраняется прежний выбор.

\#\#\# Build icons  
Показывать компактно:  
\- до 6 active skill icons \+ level;  
\- до 6 passive icons \+ level;  
\- полученные set icons.

Иконки маленькие; подробное описание открывается только в Pause / Build screen.

\#\# Optional lightweight indicators

\- количество доступных reroll;  
\- количество доступных banish.

Их можно показывать только во время draft, а не постоянно в HUD.

\---

\# 7\. Level-up Draft

При level-up игра ставится на паузу.

Открывается стандартный экран с \*\*3 карточками выбора\*\*.

До показа неполного драфта свободные позиции дозаполняются доступными сетами с неудачной проверкой шанса — равновероятно и без повторов, по GDD. Неудачный бросок сам по себе не означает пустую позицию, пока такие сеты доступны. Это же правило отображения применяется к Book Draft.

\#\# Каждая карточка содержит

\- icon;  
\- название;  
\- тип: \`Active\`, \`Passive\` или \`Set\`;  
\- current level → new level, если это upgrade;  
\- краткое описание того, что изменится после выбора;  
\- set-related information, если вариант участвует в рецептах.

Не показывать длинное полное описание механики, если его можно заменить короткой конкретной формулировкой.

Пример:

\`\`\`text  
Rock Throw  
Lv. 3 → 4

\+1 ricochet  
Ricochet damage: 80%  
\`\`\`

\#\# Actions

\- выбрать одну из 3 карточек;  
\- Reroll;  
\- Banish.

\#\#\# Reroll  
Кнопка пересобирает текущие предложения по правилам GDD и заново выполняет set checks. Banish сохраняет результаты проверок остальных сетов текущего запроса (DECISION-0022).

Рядом показывать оставшееся количество.

\#\#\# Banish  
Простой вариант реализации:  
1\. нажать кнопку \`Banish\`;  
2\. выбрать одну из трёх карточек;  
3\. выбранный вариант исключается из pool до конца run;  
4\. draft обновляется.

В режиме Banish карточка выполняет исключение вместо получения; Cancel возвращает обычный выбор без расхода. Reroll в этом режиме неактивен. После успешного исключения, смены предложений или закрытия draft режим сбрасывается. Показывать остатки и причины недоступности controls.

Не вводить отдельный сложный banish screen.

\---

\# 8\. Set information in Draft

Это обязательная часть UI, потому что построение сетов является одной из центральных целей run.

Игрок не должен запоминать все 20 рецептов самостоятельно.

\#\# Для active/passive option

Если предмет входит хотя бы в один set, в нижней части карточки показывается компактная строка:

\`\`\`text  
SETS  
Cold Orbit  3/4  
Ice Ram     2/3  
\+2 more  
\`\`\`

Показывать в карточке прежде всего \*\*самые близкие к завершению рецепты\*\*.

Если связанных сетов много, не пытаться вместить все названия в карточку.

\#\# Detailed set info

При наведении мыши на set-info / option либо в небольшой боковой detail panel показывать полный список связанных сетов:

\`\`\`text  
Cold Orbit — 3/4

✓ Orbit Blades  
✓ Ice Shards  
✓ Light Boots  
○ Hardened Skin  
\`\`\`

Для текущего draft option нужный component визуально выделяется.

\#\# Important states

Если выбор текущей карточки:

\#\#\# Завершает рецепт  
Показывать яркое, но простое сообщение:

\`COMPLETES RECIPE: Cold Orbit\`

Это означает завершение recipe requirements. Сам set всё равно активируется только после последующего появления и выбора set-card по правилам GDD.

\#\#\# Продвигает рецепт  
Показывать:

\`SET PROGRESS: Cold Orbit 2/4 → 3/4\`

\#\#\# Уже не нужен для recipe progression  
Не акцентировать set information сильнее обычного.

\#\# Component levels

Если рецепт требует минимального уровня компонента, progress считается по фактически выполненному threshold.

То есть UI должен отличать:  
\- компонент имеется, но ещё недостаточного уровня;  
\- threshold выполнен.

Простой вариант визуализации:

\`◐ Rock Throw Lv.2 / required Lv.4\`

и после выполнения:

\`✓ Rock Throw Lv.4\`

\#\# Goal

Игрок должен прямо во время draft понимать:

\- какие sets доступны через этот вариант;  
\- насколько близок каждый set;  
\- завершит ли этот выбор recipe;  
\- что ещё нужно собрать.

\---

\# 9\. Set card in Draft

Когда completed recipe проходит \`setDraftChance\` и set попадает в draft, он показывается как отдельная карточка.

Set-card должна визуально отличаться от Active/Passive card, но использовать ту же общую структуру.

\#\# Показывать

\- set icon;  
\- set name;  
\- label \`SET\`;  
\- короткое описание эффекта;  
\- список компонентов в компактном виде;  
\- отметку, что set не занимает active/passive slot.

Пример:

\`\`\`text  
SET — Cold Orbit

Orbit effects apply Slow.  
Bonus damage against slowed enemies.

FREE SET SLOT  
\`\`\`

Никакого отдельного экрана подтверждения после выбора не требуется.

После клика set сразу получен, draft закрывается.

\---

\# 10\. Pause / Build Screen

Manual pause открывает один простой full-screen overlay.

Игра полностью остановлена.

\#\# Показывать

\#\#\# Character  
\- имя;  
\- current HP / max HP;  
\- несколько основных текущих stats.

\#\#\# Active Skills  
Сетка до 6 icons:  
\- icon;  
\- name;  
\- level.

\#\#\# Passives  
Сетка до 6 icons:  
\- icon;  
\- name;  
\- level.

\#\#\# Acquired Sets  
\- set icons;  
\- name;  
\- короткое описание.

\#\#\# Set Progress  
Отдельный простой список наиболее релевантных ещё не полученных sets.

Для каждого:

\`\`\`text  
Cold Orbit — 3/4  
✓ Orbit Blades  
✓ Ice Shards  
✓ Light Boots  
○ Hardened Skin  
\`\`\`

Не обязательно постоянно показывать все 20 рецептов. В первой версии достаточно:  
\- только sets, для которых уже есть текущий progress хотя бы по одному component/threshold.

Sets без текущего progress в Pause / Build screen не показываются.

При необходимости можно добавить кнопку \`All Sets\` позже.

\#\# Actions

\- Resume;  
\- Settings;  
\- Quit Run.

\---

\# 11\. Traveler UI

Пока Traveler существует:

\#\# On-screen  
Специальный Traveler должен визуально отличаться от обычных enemies самим sprite/visual treatment.

Отдельный большой UI panel не нужен.

Для всех Travelers показывать простой HP bar над сущностью, независимо от их роли.

\#\# Off-screen  
Используется уже заданная GDD стрелка у края экрана:  
\- показывает направление;  
\- исчезает, когда Traveler снова видим;  
\- исчезает после ухода/смерти Traveler.

\#\# Optional timer  
Для первой версии \*\*не показывать точный countdown до ухода\*\*, если gameplay не докажет, что он необходим.

Можно использовать простой visual warning ближе к моменту ухода позже.

\---

\# 12\. Traveler Book Draft

Подбор Book:  
\- ставит игру на паузу;  
\- открывает отдельный draft из 3 предложений;  
\- не повышает level.

UI максимально переиспользует обычный Level-up Draft.

Различия:  
\- заголовок \`Traveler Book\` / эквивалент;  
\- визуальный accent;  
\- нет level-up presentation.

Set-information на карточках работает так же, как в обычном draft. Книга использует обычный pool и общий остаток reroll/banish.

После дозаполнения сетами 1–2 варианта показываются с неактивными пустыми позициями. Если предложений нет уже при подборе Книги, пустой draft не открывается: игрок сразу видит прибавку валюты. Обычный пустой level-up валюты не даёт. Число и origin следующего запроса очереди видны рядом с текущим draft; terminal result закрывает draft и отменяет ожидающие выборы.

\---

\# 13\. Boss UI

Когда final boss появляется:

\- сверху появляется boss HP bar;  
\- показывается имя boss;  
\- обычный HUD остаётся видимым.

Не делать отдельную boss intro cutscene для MVP.

Допустимо короткое сообщение:

\`BOSS INCOMING\`

или имя boss при появлении.

Поскольку убийство final boss не требуется для победы, timer остаётся главным индикатором конца run.

\---

\# 14\. Notifications

Использовать короткие стандартные notifications.

Нужные события:

\- \`LEVEL UP\`;  
\- \`SET RECIPE COMPLETED\`;  
\- \`SET ACQUIRED\`;  
\- \`TRAVELER APPEARED\`;  
\- \`TRAVELER ESCAPED\`;  
\- \`BOSS INCOMING\`;  
\- \`NEW CHARACTER UNLOCKED\`;  
\- \`NEW FIELD UNLOCKED\`.

Правила:  
\- короткий текст;  
\- не блокирует gameplay, кроме draft;  
\- исчезает автоматически;  
\- несколько notifications не должны закрывать центр экрана одновременно.

\---

\# 15\. Victory / Defeat

При завершении run gameplay останавливается.

Показывается:

\`VICTORY\` если персонаж жив на 15:00.

Иначе:

\`DEFEAT\`.

Run Results открывается сразу после terminal snapshot, без дополнительной паузы или подтверждения (DECISION-0037).

\---

\# 16\. Run Results

Один простой summary screen. Quit Run также открывает Results без дополнительного
подтверждения, с подписью «Забег прерван»; обрабатываемая ошибка — «Забег остановлен».
Награда одинакова: 5 × достигнутый уровень, плюс уже начисленные 50 за каждую пустую
Книгу. Показывать отдельно награду за уровень, Книги и итог, не начислять повторно.
При ошибке записи показывать «Не удалось сохранить результат» и Retry Save;
Retry run, новый run и покупки недоступны до успешной записи pending результата.
Ошибки загрузки/backup recovery/reset — по [DECISION-0037](decisions/0037-meta-economy-and-persistence.md).

\#\# Показывать

\- Victory / Defeat;  
\- survival time;  
\- final level;  
\- kills;  
\- meta currency earned;  
\- acquired sets;  
\- unlocked content, если появился новый unlock.

Дополнительно, если данные уже доступны без дополнительной сложной реализации:  
\- top 3 skills by damage.

Не строить сложный analytics screen для MVP.

\#\# Actions

\- Retry;  
\- Main Menu.

Retry немедленно запускает новый run с теми же character и field без возврата к selection и без дополнительных подтверждений.

\---

\# 17\. Meta Progression

Первая версия должна быть простой.

\#\# Main screen

Показывать:  
\- текущую meta currency;  
\- доступные permanent upgrades;  
\- доступные unlock purchases.

Permanent upgrades отображаются списком карточек: глобальные здоровье/урон и
здоровье/урон выбранного открытого персонажа. По пять уровней; цены/эффекты берутся
из Content Design «Мета-экономика». Покупки только вне run. На пределе уровня,
при недостатке валюты или pending сохранении Buy отключён с явной причиной.
Ошибка покупки не списывает валюту; новые upgrades действуют со следующего run.

Каждая карточка:  
\- icon;  
\- name;  
\- current level;  
\- effect;  
\- price;  
\- Buy button.

\#\# Unlocks

Characters/fields/content, открываемые за currency, могут использовать тот же простой card pattern.

Achievement-based unlocks показывают условие вместо price.

Не строить сложное skill tree, если оно не требуется GDD.

\---

\# 18\. Settings

Минимальный набор:

\#\#\# Audio  
\- Master Volume;  
\- Music Volume;  
\- SFX Volume.

\#\#\# Video  
\- Resolution;  
\- Fullscreen / Windowed.

\#\#\# Gameplay  
\- Screen Shake: On / Off.

\#\#\# Controls  
\- показать current movement keys;  
\- возможность remap можно добавить позже, если implementation cost заметный.

\#\#\# Other  
\- Language — только когда реально появляется localization.

Уточнённый контракт — [DECISION-0038](decisions/0038-settings-and-field-difficulty.md):

- Defaults: Master 80%, Music 60%, SFX 80%, Screen Shake On; borderless fullscreen в desktop resolution.
- Громкость/Shake применяются сразу; Back сохраняет настройки отдельным файлом. При ошибке — сообщение и Retry Save, игра не блокируется. Повреждённый/неизвестный settings file сохраняется для диагностики, включаются defaults; прогресс не сбрасывается.
- Windowed предлагает поддерживаемые разрешения; в borderless показано desktop resolution. Video Apply → Keep/Revert, автоматический Revert через 10 секунд real time. Неподтверждённый режим не сохраняется; safe fallback — окно до 1280×720 в пределах рабочего экрана.
- Test Music / Test SFX проверяют соответствующий канал с учётом Master. Preview работает на паузе и останавливается при закрытии Settings. Production аудиобиблиотека не обязательна для этого этапа.
- Shake слабый и короткий, только от полученного игроком урона; Off/Pause/end немедленно сбрасывают его. Полные timing/bounds определены решением.
- Settings из Pause сохраняет паузу и возвращает в Pause; Main Menu возвращает в Main Menu. Controls отображает actual bindings, без remap.
- Field Select показывает явную сложность 1–5 из карточки; пары полей имеют оценки 1,1,2,2,3,3,4,4,5,5.

Settings доступны:  
\- из Main Menu;  
\- из Pause.

\---

\# 19\. Basic UI states

Для reusable button/card components предусмотреть только стандартные состояния:

\- Normal;  
\- Hover;  
\- Pressed;  
\- Disabled;  
\- Selected.

Для locked content:  
\- Locked.

Для draft option:  
\- Normal;  
\- Set-related;  
\- Recipe-completing;  
\- Set card.

Не создавать сложную систему rarity colors, если gameplay её не требует.

\---

\# 20\. UI implementation principles

Чтобы UI было просто реализовывать:

\- переиспользовать один \`DraftCard\` component для Active / Passive / Set / Book Draft;  
\- переиспользовать один простой \`ContentCard\` для Character / Field / Meta Unlocks, где возможно;  
\- использовать стандартные grid/list layouts;  
\- не делать draggable UI;  
\- не делать complex animated navigation;  
\- не делать уникальный screen transition для каждого экрана;  
\- использовать обычные buttons, panels, tooltips и progress bars;  
\- подробности показывать tooltip/detail panel вместо перегруженных основных карточек;  
\- текст и layout должны работать до финального UI art.

Сначала интерфейсы должны быть полностью функциональными на простых shapes/placeholders. Финальный art накладывается после.

\---

\# 21\. MVP UI scope

\#\# Required

\- Main Menu;  
\- Character Select;  
\- Field Select;  
\- Run HUD;  
\- Level-up Draft;  
\- set progress information inside Draft;  
\- Set card;  
\- Traveler direction arrow;  
\- Traveler Book Draft;  
\- Pause / Build;  
\- boss HP bar;  
\- Victory / Defeat;  
\- Run Results;  
\- Meta Progression;  
\- basic Settings.

\#\# Later / only if needed

\- full compendium;  
\- advanced statistics;  
\- separate encyclopedia of all set recipes;  
\- controller-specific navigation polish;  
\- achievements browser;  
\- elaborate transitions;  
\- animated menu backgrounds;  
\- detailed combat log;  
\- exact Traveler escape timer;  
\- sophisticated accessibility menu.

\---

\# 22\. UI dependencies on Art Production

После approval этого таба в \`Art Production\` должны быть учтены только реально необходимые image assets.

Основной обязательный UI-art слой:

\- 16 active skill icons;  
\- 14 passive icons;  
\- 20 set icons;  
\- character portraits/icons, если body crop недостаточен;  
\- field thumbnails;  
\- reroll / banish icons при необходимости;  
\- meta-upgrade icons по мере определения meta content.

Следующие элементы для MVP можно делать без отдельной generated art:  
\- buttons;  
\- panels;  
\- HP/XP bars;  
\- boss bar;  
\- draft card backgrounds;  
\- progress indicators;  
\- tooltips;  
\- locked overlay;  
\- selected/hover states;  
\- Traveler arrow.

\---

\# 23\. Resolved review decisions

Следующие решения подтверждены:

1\. Character Select сначала использует crop/variant существующего body sprite. Отдельные portraits генерируются только если результат выглядит недостаточно хорошо.  
2\. На Character Select показываются только явно выбранные в данных персонажа modifiers относительно отдельно заданного baseline, без автоматического порога значимости и полного списка точных внутренних stats (DECISION-0026).
3\. Pause / Build показывает только sets, по которым уже есть текущий progress.  
4\. HP bar показывается для всех Travelers.  
5\. Retry немедленно перезапускает run с теми же character и field без дополнительных кликов.

Других открытых решений в этой секции сейчас нет.


## Стартовая прогрессия — DECISION-0050

Утверждено пользователем 2026-09-22; [Content Design](Content_design.md#starting-content-0050)
и [DECISION-0050](decisions/0050-starting-content-and-unlocks.md) задают состав и условия.

- Character Select: на новом профиле доступна Клёпка; для Бугра показывать
  «Пройдите Деревенскую окраину», затем цену 100 и обычное состояние покупки.
  Прочие character unlocks берутся из карточек CD.
- Level-up, Book и reroll не показывают locked skills/passives/sets. В подсказках
  рецептов при draft и Pause Build показывать прогресс только meta-открытых сетов;
  не направлять игрока к недоступным компонентам. Banish и component thresholds
  по-прежнему участвуют в recipe eligibility.
- В существующем Meta Progression → Unlocks показывать закрытые active/passive/set
  карточки с названием и понятным условием «Пройдите …». Группировать по полю,
  открывающему пакет; не добавлять отдельный экран или skill tree. Выполнение
  условия бесплатно, Buy для этих карточек отсутствует.
- Results после успешного сохранения перечисляет только новые открытия текущего
  результата по категориям. Можно показать пакет компактно с раскрытием полного
  списка; новые открытия доступны со следующего забега, включая Retry FIELD-001.
  Ошибка сохранения сохраняет pending-состояние; повтор Retry Save не повторяет
  уведомление. Восстановление/миграция старого профиля не выдаёт повторный Results.
- Field Select не показывает постоянный лимит «10/10/5» у первого поля: состав
  принадлежит профилю и расширяется. Повторный заход использует текущие открытия.
- Число слотов HUD/Pause остаётся 6 active / 6 passive. Новый выбор состава
  не меняет размеры иконок, принятый card pattern или visual style.
