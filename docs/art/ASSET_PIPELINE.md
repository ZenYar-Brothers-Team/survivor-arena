# Survival Arena — Asset Pipeline

Status: Approved
Owner: IP-12A — Visual Presentation Foundation
Depends on: [`ART_DIRECTION.md`](ART_DIRECTION.md)
Last updated: 2026-09-16
Approved by: user, 2026-09-16
Related architecture: [DECISION-0013](../decisions/0013-procedural-sprite-presentation.md)

## 1. Назначение

Этот документ определяет воспроизводимый путь растрового ассета от генерации и пользовательского review до стабильного Unity runtime-файла. Он является source of truth для структуры каталогов, naming, версий, provenance, технической подготовки PNG, import settings и безопасной замены изображений.

Art Direction отвечает на вопрос «как ассет должен выглядеть». Asset Pipeline отвечает на вопрос «как он создаётся, утверждается, хранится, импортируется и заменяется».

Pipeline применяется к:

- gameplay character/enemy sprites;
- projectiles, pickups и VFX textures;
- portraits и gameplay icons;
- растровым элементам окружения и UI;
- AI-generated и вручную нарисованным raster assets.

SVG, шрифты, аудио, 3D-модели и кодовые UI-примитивы требуют отдельных правил и не входят в этот документ.

## 2. Основные инварианты

1. Preview не является production asset.
2. В Unity попадает только пользовательски утверждённый вариант.
3. Source/master и runtime derivative — разные файлы с разным назначением.
4. Runtime path и filename стабильны после первой интеграции; история хранится в Git, а не в суффиксах `final-v7`.
5. Unity `.meta` является частью ассета, коммитится и сохраняет GUID при обновлении изображения.
6. Content ID, visual content ID, filename и resource path связаны однозначной конвенцией.
7. Изображение не определяет collider или gameplay size.
8. Все AI-generated assets имеют provenance record с финальным prompt и approval.
9. Rejected previews по умолчанию не добавляются в репозиторий.
10. Замена уже подключённого runtime asset выполняется только после явного approval замены.

## 3. Структура каталогов

### 3.1. Authoring/source вне Unity import

```text
Art/
  Source/
    Characters/
      fixture-character-agile/
        asset-record.json
        selected-master.png
        v001/
          concept-01.png
        v002/
          concept-01.png
    Enemies/
    Skills/
    Pickups/
    Environment/
    UI/
```

`Art/Source` находится вне `Assets`, поэтому Unity не импортирует большие masters, рабочие варианты и provenance metadata в Player build.

Правила:

- создавать entity folder только при появлении реального ассета;
- не добавлять пустые каталоги и `.gitkeep` без необходимости;
- хранить только выбранный master и варианты, которые имеют самостоятельную review-ценность;
- одноразовые rejected previews оставлять во внешнем image-generation storage и не коммитить;
- editable source (`.psd`, `.aseprite`) допустим рядом с master, если он действительно использовался;
- generated PNG остаётся исходником даже при отсутствии PSD.

### 3.2. Runtime raster assets

```text
Assets/
  Resources/
    Art/
      Sprites/
        Characters/
          fixture-character-agile/
            fixture-character-agile-body.png
            fixture-character-agile-shadow.png
            fixture-character-agile-portrait.png
        Enemies/
        Skills/
        Pickups/
        Environment/
      UI/
        Icons/
        Portraits/
      VFX/
```

Почему используется `Assets/Resources/Art`:

- текущий проект уже загружает runtime content и UI через `Resources.Load`;
- JSON/content catalog может хранить extensionless resource path без прямой зависимости domain model от Unity GUID;
- переход на Addressables позже изолируется внутри presentation catalog и не должен менять content IDs или gameplay model.

Нельзя помещать master, concept variants или provenance JSON под `Assets/Resources`.

### 3.3. Код, материалы и сцены

```text
Assets/Game/Presentation/
  Model/
  Runtime/
  Profiles/
  Editor/
  Tests/

Assets/Game/Art/
  Materials/
  Shaders/
  Atlases/

Assets/Scenes/
  Gameplay.unity  # includes the development-only presentation showcase
```

- `Game.Presentation` содержит код и presentation contracts.
- Materials/shaders/atlas definitions не смешиваются с source PNG.
- Assets, которые не должны загружаться по resource path, не помещаются в `Resources`.
- Первый тестовый стенд встроен в development-панель `Gameplay.unity`, поэтому проверка идёт на реальном camera scale, через тот же `SpriteDefinition`, rig и runtime, что игра. Отдельная showcase-сцена создаётся только если будущим типам ассетов станет тесно в этом стенде; она не должна содержать копии runtime assets.

## 4. Content ID и naming convention

### 4.1. Content IDs

Gameplay owner сохраняет существующий ID:

```text
FIXTURE-CHARACTER-AGILE
CHAR-001
ENEMY-001
SKILL-001
```

Visual definition ID строится так:

```text
<OWNER-ID>-VISUAL-<ROLE>
```

Примеры:

```text
FIXTURE-CHARACTER-AGILE-VISUAL-BODY
FIXTURE-CHARACTER-AGILE-VISUAL-SHADOW
CHAR-001-VISUAL-PORTRAIT
ENEMY-001-VISUAL-BODY
SKILL-001-VISUAL-PROJECTILE
```

Если у gameplay definition есть ровно один legacy visual ID без role suffix, он может сохраняться ради обратной совместимости. Новые multi-asset definitions используют явную роль.

### 4.2. Folder и filename

- Только lowercase ASCII kebab-case.
- Entity folder равен lowercase owner ID.
- Runtime filename равен `<owner-id-lowercase>-<role>.<ext>`.
- В runtime filename запрещены пробелы, кириллица, даты, initials, `new`, `final`, `approved`, `copy` и version suffix.
- Расширение runtime raster asset — `.png`.

Пример:

```text
Content ID:     FIXTURE-CHARACTER-AGILE
Visual ID:      FIXTURE-CHARACTER-AGILE-VISUAL-BODY
Folder:         fixture-character-agile/
Filename:       fixture-character-agile-body.png
Resource path:  Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body
```

`Resource path` всегда:

- относительно `Assets/Resources`;
- без расширения;
- с `/`, независимо от ОС;
- с точным регистром фактического пути.

### 4.3. Допустимые role suffixes

Базовый словарь:

```text
body
shadow
portrait
icon
weapon
projectile
pickup
telegraph
impact
mask
background
tile
prop
```

Новый role suffix добавляется только когда существующие роли семантически неверны. Не использовать близкие дубликаты вроде `avatar`, `face`, `profile-picture` вместо согласованного `portrait`.

## 5. Version lifecycle

### 5.1. Preview

- Генерируется встроенным image-generation workflow.
- Показывается пользователю inline.
- Может оставаться во внешнем generated-images storage.
- Не получает runtime path и не подключается к Unity.
- Несколько разных ассетов генерируются отдельными запросами; варианты одного ассета получают один и тот же brief с одной целевой разницей за итерацию.

### 5.2. Candidate

После пользовательского выбора candidate копируется в:

```text
Art/Source/<Category>/<entity-id>/vNNN/
```

Версии имеют нулевое дополнение:

```text
v001
v002
v003
```

Внутри версии допустимы:

```text
concept-01.png
concept-02.png
edit-mask.png
notes.md
```

### 5.3. Approved master

После финального визуального approval выбранный файл сохраняется как:

```text
Art/Source/<Category>/<entity-id>/selected-master.png
```

`selected-master.png` — текущий источник runtime derivative. Предыдущая версия остаётся доступной через Git; отдельный архивный `selected-master-old.png` не создаётся.

### 5.4. Runtime derivative

Runtime PNG создаётся из approved master путём только задокументированных операций:

- alpha cleanup;
- edge decontamination/dilation;
- canvas normalization;
- crop/padding normalization;
- color-profile normalization to sRGB;
- high-quality downscale;
- при явно утверждённой необходимости — разделение shadow/body или других presentation layers.

Нельзя на runtime-этапе незаметно менять дизайн, выражение, пропорции, палитру или добавлять детали. Такое изменение возвращает ассет в visual review.

## 6. Provenance record

Каждая entity folder под `Art/Source` содержит `asset-record.json`. Для нового ассета копируется [`Art/Templates/asset-record.template.json`](../../Art/Templates/asset-record.template.json); placeholders заполняются фактическими значениями до Gate C.

Минимальная форма:

```json
{
  "schemaVersion": 1,
  "contentId": "FIXTURE-CHARACTER-AGILE",
  "status": "approved",
  "artDirectionRevision": "2026-09-16",
  "sourceKind": "ai-generated",
  "generator": "OpenAI built-in image generation",
  "selectedVersion": "v002/concept-01.png",
  "approvedAt": "2026-09-16",
  "approvedBy": "user",
  "prompt": "<final prompt verbatim>",
  "constraints": [
    "transparent background",
    "no baked shadow",
    "animation-ready neutral pose"
  ],
  "runtimeOutputs": [
    "Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png"
  ]
}
```

Правила:

- `prompt` хранится дословно для выбранной генерации/правки;
- `generator` описывает фактически использованный путь, неизвестная model version не выдумывается;
- reference images перечисляются с ролью и происхождением, если использовались;
- не сохранять API keys, account IDs, локальные абсолютные пути и персональные данные;
- для hand-drawn asset использовать `sourceKind: "hand-authored"` и указать исходный editable file;
- изменение только runtime compression/import settings не создаёт новую visual version;
- содержательное изменение изображения создаёт новую `vNNN` и требует нового approval.

## 7. Raster technical contract

### 7.1. Формат

- Runtime: PNG, RGBA8, sRGB, straight/unassociated alpha.
- Master: PNG RGBA8 или lossless editable source плюс PNG preview.
- JPEG запрещён для sprites, masks, icons и изображений с прозрачностью.
- Не использовать premultiplied-alpha export как исходный runtime PNG.
- Не хранить ICC profile, который заметно изменяет цвет вне sRGB workflow.

### 7.2. Alpha и края

- Фон должен быть реально прозрачным, а не белым/шахматным изображением.
- Внешняя рамка минимум `2 px` runtime texture полностью прозрачна.
- На полупрозрачных краях не должно быть белого, чёрного или цветного matte halo.
- RGB transparent-edge pixels должны быть color-decontaminated/dilated от ближайшего foreground, чтобы bilinear filtering не создавал кайму.
- Случайные полупрозрачные пиксели вне силуэта удаляются.
- Body, shadow и glow не смешиваются в один PNG, если требуют разных transform/material behavior.

### 7.3. Canvas и padding

Master следует Art Direction. Runtime canvas нормализуется к power-of-two target текущей категории.

Для character body `512×512`:

- видимый силуэт ориентировочно `70–78%` высоты;
- минимум `12%` canvas слева/справа;
- около `14%` сверху;
- `8–10%` снизу;
- feet/ground-contact line горизонтальна;
- визуальный центр не смещается случайным прозрачным пространством.

Padding нужен для procedural stretch/rotation и не обрезается tight crop до исчезновения safety envelope.

### 7.4. Категорийные размеры

- Player/character body: master `1024–2048` square; runtime `512×512`; первый vertical slice остаётся uncompressed.
- Character portrait: master `1024` square; runtime `512×512`; композиция проходит отдельный review.
- Normal enemy body: master `1024` square; runtime `256×256` или `512×512`; `512` используется только когда этого требует силуэт.
- Boss body: master `2048` square; runtime `512×512` или `1024×1024`; требует memory check.
- Gameplay icon: master `512` square; runtime `256×256`; обязан читаться при фактическом размере UI slot.
- Projectile/pickup: master `512` square; runtime `128×128` или `256×256`; сохраняется достаточный effect padding.
- Ground shadow: master `512` square; runtime `128×128` или `256×256`; предпочтительна greyscale/alpha-friendly структура.
- Impact/telegraph texture: master `512–1024` square; runtime `256×256` или `512×512`; material может задавать runtime tint.

Power-of-two target — pipeline default, not artistic requirement. Не увеличивать малый source до большего runtime target.

### 7.5. Memory reference

До platform compression приблизительная GPU memory для RGBA32 без mipmaps:

- `128×128`: 64 KiB;
- `256×256`: 256 KiB;
- `512×512`: 1 MiB;
- `1024×1024`: 4 MiB.

Disk PNG size не равен runtime memory. Решение увеличить texture принимается по видимому качеству и memory budget, а не по малому размеру PNG на диске.

## 8. Unity import contract

### 8.1. Gameplay world sprite default

Для body/enemy/projectile/pickup/shadow:

```text
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Color Space: sRGB on
Alpha Source: Input Texture Alpha
Alpha Is Transparency: on
Read/Write: off
Generate Mip Maps: off
Wrap Mode: Clamp
Filter Mode: Bilinear
Mesh Type: Full Rect
Extrude Edges: 1
Pixels Per Unit: 320
```

Почему `Full Rect`: прозрачный safety padding и стабильная geometry важнее экономии нескольких vertices; procedural transform и pivot не должны меняться от tight-mesh contour.

Почему `320 PPU`: при body `512×512` и видимом силуэте около `70–78%` это даёт примерно `1.12–1.25` world units высоты, что соответствует текущему player collider `0.8×0.8` и camera orthographic size `5`, оставляя визуалу выразительную высоту без изменения gameplay geometry.

PPU является presentation default. Отклонение допускается только с записью причины в provenance/asset specification. Разные физические размеры существ достигаются размером силуэта на согласованном canvas или явным presentation scale profile, а не случайным PPU.

### 8.2. Pivot

- Character/enemy body: custom pivot в центре stance по X и на ground-contact line по Y.
- Pivot ставится по фактической позиции стоп, а не автоматически в `Center` или `Bottom` canvas.
- Ground shadow: center.
- Projectile/pickup/impact: center, если поведение не требует явно описанной точки запуска.
- Portrait/icon: center.
- Weapon/attachment: pivot в точке хвата/крепления и проверяется в rig.

Pivot является частью runtime contract. Его изменение после интеграции требует scene/prefab visual regression check.

### 8.3. Max size и compression

Первый `FIXTURE-CHARACTER-AGILE` vertical slice:

```text
Max Size: 512
Compression: None / Uncompressed
Crunch Compression: off
```

Это позволяет оценить alpha, outline и procedural deformation без compression artifacts. Перед массовым production content вводится platform profile:

- player/portrait/icon: uncompressed или high-quality platform format после visual comparison;
- normal enemies/environment: normal/high-quality platform compression;
- masks: формат выбирается по реально используемым channels;
- Crunch не используется для runtime-critical frequently loaded sprites без отдельного load-time measurement.

Нельзя менять compression всего каталога без before/after screenshots на target scale.

### 8.4. UI sprite default

```text
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Mesh Type: Full Rect
Pivot: Center
sRGB: on
Alpha Is Transparency: on
Mip Maps: off
Filter Mode: Bilinear
Wrap Mode: Clamp
Read/Write: off
Max Size: exact category runtime target
```

UI layout отвечает за экранный размер; PPU не используется как способ визуально масштабировать UI.

## 9. Unity `.meta` и GUID policy

- Каждый runtime asset обязан иметь `.meta`, созданный Unity Editor.
- Новый PNG добавляется, затем Unity выполняет import и создаёт `.meta`; оба файла коммитятся вместе.
- Не писать GUID вручную.
- При approved replacement заменяются bytes PNG по существующему path; существующий `.meta` сохраняется.
- При перемещении/переименовании внутри Unity перемещается и `.meta`.
- Запрещено удалить ассет и создать «тот же» заново, если на его GUID уже существуют ссылки.
- Source assets вне `Assets` не имеют Unity `.meta`.

## 10. Git и binary policy

Репозиторий уже направляет `.png`, `.psd`, `.jpg`, `.tga`, `.tif` в Git LFS через `.gitattributes`.

- Перед первым binary commit на машине должен быть выполнен `git lfs install`.
- Проверить, что staged PNG представлен LFS pointer, а не большим inline blob.
- Не добавлять generated caches, Unity `Library`, временные masks или экспортные дубликаты.
- Не использовать Git LFS как оправдание для хранения всех rejected generations.
- Runtime PNG, approved master и значимые editable sources являются versioned project assets.

## 11. Sprite atlases и loading lifecycle

Первый vertical slice не требует atlas. Atlas добавляется только после появления группы assets и measurement build/runtime cost.

Правила будущей упаковки:

- группировать по совместному lifecycle загрузки, а не по тому, что все файлы являются «артом»;
- не создавать один глобальный atlas всех characters/enemies/fields;
- common gameplay, конкретное поле, character-selection UI и VFX могут иметь разные atlases;
- sprite resource path/content ID не должен зависеть от atlas placement;
- atlas migration не меняет domain definition;
- padding/extrusion atlas проверяются на отсутствие alpha bleeding.

Addressables не вводятся в IP-12A. Если они понадобятся, `SpriteDefinition` catalog становится границей миграции с `Resources`, сохраняя content IDs.

## 12. Review и approval workflow

### Gate A — Brief ready

- content ID и role определены;
- Art Direction sections выбраны;
- размер, перспектива, pose и запрещённые элементы перечислены;
- известна схема layers/attachments.

### Gate B — Visual candidate approved

- варианты показаны пользователю inline;
- выбран ровно один candidate или явно запрошена следующая правка;
- правка меняет одну целевую группу свойств и повторяет invariants;
- выбранный вариант проходит Art Direction review checklist.

### Gate C — Master prepared

- master сохранён в `Art/Source`;
- alpha и края проверены;
- provenance record заполнен;
- runtime derivative создан без дизайнерских изменений.

### Gate D — Unity import verified

- PNG и `.meta` находятся по canonical path;
- import settings соответствуют категории;
- pivot проверен;
- resource path разрешается catalog;
- target-scale screenshot не показывает blur/halo/crop.

### Gate E — Gameplay integration approved

- sprite проверен в Showcase и Gameplay;
- collider/root не изменены ради картинки;
- procedural animation остаётся в safety envelope;
- player/danger/pickup hierarchy сохраняется в плотной сцене;
- пользователь принимает визуал и движение либо возвращает конкретную правку.

Ни один gate не подразумевает автоматическое одобрение следующего.

### Ручной showcase-прогон

В Editor запустить `Assets/Scenes/Gameplay.unity` при reference resolution `1920×1080`, нажать компактную кнопку `DEV` в левом нижнем углу и выбрать вкладку `Presentation`. Drawer свёрнут по умолчанию, чтобы не закрывать gameplay-scale review. Вкладка предоставляет:

- `Live` — движение визуала снова читается из настоящего `Rigidbody2D` игрока;
- `Idle` — фиксированный нулевой presentation input с data-driven bob, breathing squash/stretch и лёгким sway;
- `Left` / `Right` — фиксированная скорость профиля в нужную сторону без изменения `Rigidbody2D`, collider или gameplay position;
- `Reset` — сброс phase/reactions/facing/spawn и возврат в `Live`.

Проверка одного body sprite выполняется в таком порядке:

1. `Idle`: дыхание и лёгкое тревожное покачивание заметны, но не ломают силуэт; pivot и ground contact стабильны, spawn не обрезается.
2. `Left`, затем `Right`: flip читается корректно; асимметрия света и аксессуаров не выглядит ошибкой.
3. `-10 HP`: видны squash/tilt и hit flash, gameplay root не сдвигается.
4. Во время `Left` или `Right` нажать `Pause`: поза и reaction timers замирают; после Resume продолжаются.
5. `Reset`: facing возвращается вправо, реакции очищаются, затем управление возвращается настоящему движению.
6. `Live`: пройти по игровому полю и оценить sprite рядом с врагами, pickups, VFX и UI.

Для калибровки первого fixture дополнительно проверить в `Live`:

- движение по диагонали: bob/stretch сохраняют полную интенсивность, а наклон не выглядит чрезмерным;
- при остановке locomotion без рывка уступает место idle;
- камера не наследует bob, sway, squash/stretch или hit reaction дочернего `BodyRoot`;
- collider и gameplay position продолжают соответствовать прежнему кубику;
- после pause, victory или defeat visual pose не продолжает проигрываться.

Автоматическая часть calibration gate для `FIXTURE-CHARACTER-AGILE` проверяет эквивалентность pose при симуляции 30 и 120 FPS, диагональное движение, допустимые пределы offset/scale/rotation/flash, неизменность gameplay root/collider и остановку presentation в `Paused`, `Won` и `Lost`. Эти проверки не заменяют ручную оценку читаемости и ощущения движения на игровом масштабе.

Эти элементы доступны только в Editor/Development Build. Они не являются player-facing управлением и не меняют authoritative movement state.

## 13. Replacement policy

Для изменения подключённого ассета:

1. Не трогать текущий runtime PNG.
2. Создать следующую `vNNN` в `Art/Source`.
3. Показать candidate и получить approval на замену.
4. Обновить `selected-master.png` и `asset-record.json`.
5. Перезаписать runtime PNG по тому же стабильному path.
6. Сохранить существующий `.meta`/GUID.
7. Повторить import и gameplay visual checks.

Если новый дизайн несовместим с прежним pivot, bounds или layer breakdown, это migration, а не простая замена. Она требует проверки prefab/scene references и animation profile.

## 14. Automated validation targets

Editor validation для runtime raster tree должен уметь сообщать как минимум:

- filename/folder не соответствует lowercase kebab-case;
- отсутствует ожидаемая role suffix;
- texture не PNG;
- dimensions превышают category max или неожиданно не power-of-two;
- Texture Type не Sprite;
- Sprite Mode не Single;
- mipmaps включены;
- wrap mode не Clamp;
- filter mode не Bilinear;
- alpha transparency отключена для asset, которому она нужна;
- Read/Write включён без явной причины;
- PPU отличается от default без override record;
- Mesh Type не Full Rect;
- runtime asset находится без `.meta`;
- content visual path не разрешается;
- source/master случайно попал под `Assets/Resources`.

Visual качества — silhouette, выражение, halo и художественная совместимость — не объявляются автоматически проверенными только потому, что technical validator прошёл.

## 15. Первый asset path

Для IP-12A используется следующий контракт:

```text
Owner content ID:
  FIXTURE-CHARACTER-AGILE

Body visual ID:
  FIXTURE-CHARACTER-AGILE-VISUAL-BODY

Source folder:
  Art/Source/Characters/fixture-character-agile/

Approved master:
  Art/Source/Characters/fixture-character-agile/selected-master.png

Runtime body:
  Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png

Runtime resource path:
  Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body

Runtime shadow:
  Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-shadow.png
```

Первый body импортируется как `512×512`, `320 PPU`, `Full Rect`, custom ground-contact pivot, Bilinear, Clamp, no mipmaps, uncompressed. Shadow хранится отдельно и не включается в body generation/output.

## 16. Definition of Ready для интеграции

Ассет готов попасть в код только если:

- Art Direction approved;
- candidate явно утверждён пользователем;
- `selected-master.png` и `asset-record.json` существуют;
- runtime PNG соответствует raster contract;
- canonical visual ID и resource path определены;
- Unity import settings проверены;
- pivot и target world size известны;
- layer/attachment expectations перечислены;
- нет неразрешённых copyright/provenance вопросов.

## 17. Definition of Done для одного ассета

- Runtime PNG и `.meta` находятся по canonical path.
- Content registry валидирует visual ID.
- Showcase и Gameplay используют один `SpriteDefinition`.
- На target scale нет crop, halo, blur или потери ключевого силуэта.
- Procedural motion не выходит за canvas safety envelope.
- Replacement не требует изменения gameplay root/collider.
- Provenance record указывает финальный prompt, выбранную source version и approval.
- Tests и manual visual checks из owning IP выполнены и записаны в `STATUS.md`.

## 18. Утверждённые базовые решения

Пользователь утвердил следующие defaults 2026-09-16:

1. Masters и рабочие версии хранятся в `Art/Source`, вне Unity import.
2. Runtime raster assets хранятся в `Assets/Resources/Art` до отдельной миграции на Addressables.
3. Visual ID следует форме `<OWNER-ID>-VISUAL-<ROLE>`.
4. Stable runtime filenames не содержат version suffix; replacement сохраняет path и `.meta` GUID.
5. Gameplay sprites используют `320 PPU`, Bilinear, Full Rect, no mipmaps и Clamp, если asset specification не содержит обоснованный override.
6. Первый player body — `512×512`, uncompressed; master — минимум `1024×1024`.
7. Rejected previews не коммитятся; выбранный master, provenance и runtime derivative коммитятся через уже настроенный Git LFS.

Эти пункты являются обязательным asset contract IP-12A.

## 19. Рецепт добавления нового ассета

Этот порядок используется для каждого нового character, enemy, projectile, pickup, portrait или icon. Шаг нельзя объявлять пройденным только по наличию файла: применяются соответствующие approval gate и category checklist из следующего раздела.

Повторяемые технические шаги 5–9 выполняются через `python scripts/art_pipeline.py <packet.json>` (plan) и `--apply` после уже полученного approval. Формат пакета и ограничения: [scripts/README](../../scripts/README.md#1-подготовка-утверждённого-арта). Команда не генерирует и не утверждает изображения, не создаёт `.meta`, не выводит body contacts из пикселей и не закрывает gates D/E. Новые записи имеют этап `Prepared`; проверки выполняет `scripts/check_project.py --scope art`. Для настройки существующих эффектов доступен ограниченный [visual-preview](../../scripts/README.md#2-быстрая-визуальная-итерация) без тестового прогона после каждого изменения числа; финальная проверка сохраняется.

1. **Проверить content gate.** Определить, является ли owner production-сущностью или явно названным `FIXTURE-*`. Draft ID нельзя превращать в production content без approval.
2. **Назначить идентификаторы.** Зафиксировать существующий owner content ID, visual ID `<OWNER-ID>-VISUAL-<ROLE>`, category, role, source folder, stable runtime filename и extensionless resource path.
3. **Составить brief.** Взять generation contract из `ART_DIRECTION.md`, добавить назначение ассета, gameplay scale, camera view, silhouette requirement, разрешённые слои и category-specific ограничения. Не смешивать разные ассеты в одном generation request.
4. **Создать preview-варианты.** Показать варианты пользователю до попадания в runtime tree. Не подключать preview и не выдавать его за approved master.
5. **Получить approval.** Сохранить выбранный candidate в следующую `Art/Source/<Category>/<owner-id>/vNNN/`; содержательные правки создают новую version. Зафиксировать утверждённый вариант явно.
6. **Подготовить source record.** Обновить `selected-master.png`; скопировать provenance template в `asset-record.json`; дословно записать финальный prompt, реальные references/generator, approval и ожидаемые runtime outputs.
7. **Создать runtime derivative.** Выполнить только alpha/edge cleanup, canvas и padding normalization, sRGB normalization и качественный downscale. Любое изменение дизайна возвращает процесс к preview и approval.
8. **Импортировать в Unity.** Поместить PNG по stable runtime path, дать Unity создать `.meta`, применить category import contract и сохранить GUID при будущей замене. Source/master не помещать под `Assets`.
9. **Зарегистрировать presentation content.** Добавить `SpriteDefinition` в fixture или production presentation catalog; добавить typed visual reference в owner definition; убедиться, что `ContentRegistry.Build()` обнаруживает missing/wrong-type references. Gameplay-код не загружает PNG напрямую в обход catalog boundary.
10. **Подключить runtime presentation.** Для деформируемого world body использовать дочерний `VisualRoot`/`BodyRoot`, отдельную shadow и motion profile. Projectile/pickup/UI используют только нужный им presentation adapter и не получают пустой character rig «для единообразия».
10a. **Подогнать круг контакта для character/enemy body.** Выполнить §22 после фиксации PNG, PPU и pivot; сохранить contact profile и применить его через scene/factory. Для остальных категорий этот шаг не применяется.
11. **Проверить автоматически.** Проверить path/ID resolution, import contract, alpha/dimensions, scene or factory wiring, reset/pooling semantics и отсутствие изменения gameplay root/collider от анимации. Для body проверить authored круг по §22. Запустить релевантные EditMode/PlayMode regressions.
12. **Проверить визуально.** Использовать Gameplay showcase или реальный owning screen на target scale. Проверить silhouette, halo/crop, pivot, facing/rotation, motion/effect padding, плотную сцену и category checklist.
13. **Записать evidence.** Обновить owning IP в `STATUS.md`: implementation, verification, deviations и documentation impact. Только после этого asset считается завершённым.

Короткая цепочка:

```text
owner content ID
→ visual ID и role
→ Art Direction brief
→ preview variants
→ user approval
→ versioned source + selected master + provenance
→ normalized runtime PNG
→ Unity import + .meta
→ SpriteDefinition + owner reference
→ подходящий rig/adapter + optional motion profile
→ для character/enemy body: максимальный вписанный круг (§22)
→ automated checks
→ gameplay/showcase review
→ STATUS evidence
```

## 20. Чек-листы по типам ассетов

### 20.1. Новый персонаж

- Production ID и образ прошли content gate; fixture явно помечен `FIXTURE-*`.
- Body использует нейтральную animation-ready stance, свободные конечности, чистый силуэт и горизонтальную ground-contact line.
- Body, shadow, weapon и portrait являются отдельными roles, если требуют разных transform/material/UI lifecycle.
- Canvas/PPU/pivot дают ожидаемый world size; статический body contact profile подготовлен по §22.
- Горизонтальный flip не ломает свет, аксессуары, хват или смысл силуэта.
- `CharacterDefinition` ссылается на зарегистрированные visual и motion-profile IDs.
- `VisualRoot` изолирует idle, locomotion, hit и spawn от physics; pause/end/reset проверены.
- Персонаж читается рядом с enemies, pickups, VFX и HUD при gameplay camera scale.

### 20.2. Новый враг

- Silhouette и цвет выражают gameplay threat/role, не полагаясь на мелкую детализацию.
- Runtime size соответствует категории normal enemy или boss; повышение до `512/1024` обосновано silhouette/memory check.
- Pivot находится на stance/ground-contact line; shadow отделена от деформируемого body.
- Contact profile подготовлен по §22. Анимация visual не меняет collider, seek/contact distance, damage timing или authoritative movement.
- Если enemy pooled, повторный spawn полностью сбрасывает flip, tint, scale, reactions и transient effects.
- Directional flip/rotation соответствует фактическому способу движения enemy.
- Проверена читаемость в ожидаемой максимальной плотности толпы, а не только один объект на пустом фоне.

### 20.3. Новый снаряд

- Силуэт и контраст читаются при реальной скорости и минимальном экранном размере.
- Pivot обычно `Center`; orientation/forward axis явно согласованы с runtime rotation.
- Canvas сохраняет padding для glow/trail/rotation, но не содержит baked motion blur или длинный пустой хвост.
- Visual size не определяет hitbox, pierce radius, damage area или скорость.
- Projectile и impact/telegraph используют разные roles и lifecycles, если ведут себя по-разному.
- Pool return сбрасывает rotation, scale, tint, trail/effect state и sprite override.
- Проверено движение во всех используемых направлениях на светлом и тёмном участке поля.

### 20.4. Новый pickup

- Pickup отличается от врага, опасности и фонового декора формой и контрастом.
- Pivot обычно `Center`; hover/bob применяется к visual child и не двигает trigger/collider.
- Размер читается на gameplay scale, но не вводит ложное представление о collection radius.
- Glow/outline имеют достаточный padding и не загрязняют alpha случайными пикселями.
- Visual, shadow и collect impact разделены, если их transform/lifetime различаются.
- Pool/reuse сбрасывает animation phase, scale, tint и effect state.
- Проверены обычный фон, плотная сцена, движение камеры, pickup и expiry.

### 20.5. Новый UI portrait или icon

- `portrait` и `icon` не являются взаимозаменяемыми roles: portrait передаёт персонажа, icon — функцию/предмет/умение.
- Композиция проверена в фактическом UI slot и при минимальном целевом размере, а не только на master canvas.
- Важная форма и expression не обрезаются mask/layout; transparent padding согласован с соседними элементами.
- Нет мелкого текста, baked frame или фона, если они принадлежат UI layout/theme.
- Import использует UI contract; экранный размер задаёт layout, а не случайный PPU.
- Проверены normal/hover/disabled/selected состояния, если они существуют; tint не уничтожает читаемость.
- Visual ID зарегистрирован и разрешается через presentation/content boundary; UI не содержит случайный прямой путь к source/master.

## 21. Category profiles и reusable adapters (IP-12A)

### Контактная геометрия body

Для character/enemy body применяется отдельный authoring-этап [§22](#22-подгонка-круга-контакта-для-world-body), утверждённый в DECISION-0039. Он задаёт статическую физическую геометрию после подготовки рисунка; анимация и обычный reimport её не меняют.

### Первый enemy body: approved art в существующем fixture

ENEMY-001 v002 имеет отдельный master/provenance и runtime derivative 256×256.
FIXTURE-ENEMY-SEEKER-VISUAL ссылается на этот PNG; gameplay owner остаётся
FIXTURE-ENEMY-SEEKER, его баланс и collision geometry не изменяются.
Production ENEMY-001-VISUAL-BODY зарезервирован в manifest, но не выдаётся за
зарегистрированную production definition.

Optional EnemyDefinition.MotionProfile / JSON motionProfileId — typed ref на
SpriteMotionProfile, проверяемый registry и сохраняемый WaveEnemyScaler.
Composition root разрешает body role и profile; spawner/factory передают resolved
sprite/profile в runtime. Тела с motion используют existing SpritePresentationRuntime
на дочерних VisualRoot/BodyRoot; VisualRoot компенсирует collision-size scale,
поэтому PPU определяет визуальный размер независимо от коллайдера.
Health.Damaged даёт hit reaction; pause/terminal замораживают pose; death,
reinitialize и pool return сбрасывают renderer/pose/subscription.
Общий death tail и ground shadow подключаются через единые presentation profiles;
они не содержат веток по конкретному enemy ID. Новая art integration не закрывает пользовательский gate E.

`Art/ImportProfiles.json` — editor-side technical settings. Category defaults: UI icon 256, portrait 512; projectile/pickup/shadow 256; impact/telegraph 512. Center pivot используется для UI/VFX. World body требует записи с полным asset path и фактической ground-contact точкой: неизвестный body не получает произвольный pivot. Exact-path record задаёт PPU, maxSize, pivot и причину override; повторный import применяет тот же record. После изменения профиля выполнить Reimport затронутых ассетов. Общие Sprite/Single/sRGB/alpha/FullRect/Bilinear/Clamp/no mipmaps/no ReadWrite и uncompressed contract сохраняются. PPU > 0; pivot в [0,1]; maxSize — power-of-two 32…8192, увеличение сверх category target требует прежней memory/readability проверки.

`SpriteDefinition.Role` и `RequireRole` отделяют Body/Portrait/Icon/Projectile/Pickup/Telegraph/Impact/Shadow. Legacy fixtures могут иметь Unspecified, но новый role adapter этого не принимает. Catalog не подставляет placeholder для production ID. `SpritePortraitCrop` переиспользует texture approved body, задаёт нормализованный прямоугольник внутри [0,1] с положительными сторонами и center pivot; владелец освобождает только созданный Sprite через Dispose. Это UI reuse без копирования master в Assets; crop всё равно проходит slot review.

`SpritePresentationAdapter` принимает `IPresentationSource` (running, velocity, semantic hit/proc/death/collect events), рисует только child без physics в subtree и возвращает captured baseline при Shutdown/reinitialize. `FixturePresentationFeedback.json`: fadeSeconds и procSeconds > 0 секунд; procScale в [0,0.1], относительный акцент масштаба. Значения synthetic; scale результата ограничивается safety envelope. Death/collect запускают одноразовый fade, но не задерживают gameplay despawn; owner вправе вернуть объект сразу. При необходимости пережить despawn owner предоставляет отдельный pooled visual, не удерживает gameplay entity ради анимации. Пауза/terminal останавливают часы и новые реакции; proc не накапливается, повтор обновляет envelope. Settings/camera consumer IP-26 получает `IScreenShakePreference` и `ScreenShakeRequestGate`; camera service/persistence здесь не реализуются.

Инвентарь небольшого проверяемого пакета: [Art/asset-manifest.json](../../Art/asset-manifest.json); read-only audit: `python scripts/validate-art-manifest.py`. Production remainder остаётся в Art Production. Generated, procedural и hybrid роли имеют отдельные evidence; пустой ShadowRenderer игрока не объявлен готовым shadow image.

Editor diagnostic: **Tools → Survivor Arena → Presentation Fixture Review**. Это отдельное opt-in окно с scroll, не overlay поверх игры и не часть Player build. Idle/Left/Right, Hit/Proc/Death/Collect, Pause/Resume, Reset и 4 copies управляют synthetic source через adapter. Шесть процедурных геометрических ролей — технические fixtures; они не выдаются за production enemy/projectile/XP art. Четыре копии проверяют совместную работу adapters, но не заменяют реальный gameplay run с 3–4 сетами. Capture Presentation Fixture Review сохраняет технический снимок в TestResults; визуальная приёмка и плотный gameplay review остаются отдельными gate E.

## 22. Подгонка круга контакта для world body

Утверждено пользователем 2026-09-22 после проверки гоблина и селянина; основание — [DECISION-0039](../decisions/0039-conservative-body-contact-circles.md). Этап выполняется для новых и изменённых character/enemy body с круговым контактом после подготовки runtime PNG и фиксации import settings. Projectile, pickup, UI и VFX сохраняют собственную геометрию.

### Правило

Один CircleCollider2D задаёт и физический упор, и contact damage. Вписываем максимально большой круг в заполненный внешний обвод персонажа, игнорируя внутренние дырки и промежутки между рукой и телом или ногами. Практическое определение обвода — выпуклая оболочка пикселей с alpha >=230/255; оружие и выступы участвуют в обводе, но не получают отдельных коллайдеров. Прозрачный padding и полупрозрачная тень не определяют размер. Визуальное пересечение до физического контакта допустимо; совпадение с каждым пикселем во время анимации не требуется.

Круг является ограничением authoring, но не шаблоном внешности. Персонажей не требуется рисовать круглыми. Основная масса body может быть высокой, широкой и асимметричной, однако крайне вытянутые узкие силуэты и очень длинные далеко торчащие конечности, оружие или аксессуары не проходят стандартный body review: с одним кругом они дают слишком большую область видимого тела вне контакта. Такой candidate сначала перерабатывается на уровне силуэта; fit не компенсируется несколькими коллайдерами, скрытым увеличением круга или индивидуальной подгонкой gameplay geometry.

Центр по X находится на вертикали sprite pivot: один и тот же круг помещается в исходный и зеркальный силуэты. Одновременно оптимизируются радиус и центр по Y. Для каждой грани оболочки выполняется `n·c + r <= -b`, где n — внешняя единичная нормаль, b — смещение грани, c — центр круга, r — радиус в пикселях; выбирается максимальный r. Например при грани x<=200 и center.x=140 допустимый радиус не больше 60 px. В JSON радиус и высота центра над foot pivot делятся на PPU и хранятся в world units. Дополнительного shrink factor нет; радиус округляется вниз до 0.000001 world units.

### Порядок работы

1. До runtime preparation проверить source candidate на target scale: основная масса не выглядит крайне вытянутой, а выступающие части не создают чрезмерную дистанцию от корпуса до визуального края. Круг не должен делать всех персонажей круглыми; review отклоняет только крайности, несовместимые с одним contact circle. Затем убедиться, что runtime PNG утверждён и импортирован, PPU и ground pivot записаны в `Art/ImportProfiles.json`. Масштаб рисунка проверяется рядом с уже готовыми персонажами.
2. Зарегистрировать Body в presentation catalog. Текущий authoring tool читает `FixtureSprites.json`: запись должна иметь парные `contactRadius` >0 и `contactCenterY` >=0, оба finite. Для новой записи допустимы временные стартовые значения, которые fit заменит до runtime integration. Production catalog требует соответствующего адаптера, fixture tool не вводит production ID автоматически.
3. Из корня репозитория выполнить `python scripts/fit-body-contacts.py --fit-outer` для предложения, затем `python scripts/fit-body-contacts.py --fit-outer --write` для сохранения. Требуются Pillow, NumPy, SciPy. Команда обрабатывает все записи с contact profile: просмотреть JSON diff и убедиться в нужном scope. Без аргументов скрипт только проверяет сохранённые круги. `--radius-scale` предназначен для отдельно согласованных экспериментов и не входит в стандартный fit.
4. Применить профиль через Unity API: для текущего Player — `Game.Presentation.Editor.FixtureContactBaker.BakePlayer`; enemy factory читает профиль при spawn/reuse. Сцену не править вручную в YAML. Gameplay root остаётся центром круга; body visual смещается вниз на contactCenterY. Root scaling компенсируется при задании radius. Анимация, flip, pause и pool reset не пересчитывают геометрию.
5. Сделать capture с наложенными кругами: отдельно проверить оба направления спрайта и касание пар с восьми сторон. Текущая команда — `Game.Presentation.Editor.PresentationReviewCapture.CaptureContacts`; результат — `TestResults/body-contact-review.png`. Для новых body расширить выборку capture. Осмотреть также реальное движение, hit pose и читаемость толпы: если круг покрывает лишь малую центральную часть фигуры или длинный выступ создаёт чрезмерное визуальное пересечение до контакта, вернуть body на silhouette revision.
6. Проверить сохранённый круг внутри оболочки и касание её границы с допуском до одного пикселя для дискретизации, отсутствие урона до физического контакта, урон после контакта, pause/end и смешанный pool reuse. Запускать релевантные Unity tests по smoke-check safety procedure. Если для нового силуэта выпуклый обвод даёт нежелательный результат, зафиксировать отклонение и отдельный review, не подменять правило скрытым коэффициентом.
7. Записать параметры, capture и проверки в evidence, обновить owning scope в STATUS и получить визуальную оценку. Замена PNG, PPU или pivot требует повторного fit и review; обычный reimport не перезаписывает contact profile.

### Runtime стоимость и текущий эталон

Оболочка и оптимизация рассчитываются только при подготовке ассета. В игре нет чтения alpha, поиска контура или дополнительных коллайдеров на конечностях: один фиксированный круг на актёра.

| Body | Radius | CenterY над foot pivot |
|---|---:|---:|
| Goblin | 0.401431 | 0.530976 |
| Villager | 0.330282 | 0.469539 |
| Courier v002 | 0.360855 | 0.453097 |

Текущая пара принята пользователем как образец пайплайна; полный gameplay/density gate остаётся отдельным. Проверки: [contact evidence](../implementation/evidence/2026-09-22-body-contact-circles.md#third-trial--maximum-inscribed-circles).

## 23. Единая процедурная смерть врагов

По [DECISION-0040](../decisions/0040-shared-enemy-death-presentation.md) ordinary enemy, boss и Traveler используют один presentation algorithm без content-ID веток и без отдельного death raster:

1. На `Died` gameplay немедленно отключает movement/attack/contact, collider, rigidbody simulation и telegraph, удаляет жизнь из target registry и выдаёт награду. Root остаётся в той же мировой позиции; impulse или направленный death push запрещены.
2. Presentation копирует текущий активный sprite, material, sorting, flip и transform в дочерний `DeathVisual`. Исходный renderer скрывается. Это одинаково работает для plain placeholder и `VisualRoot/BodyRoot`.
3. За authored squash interval тело расширяется по X и сжимается по Y. Затем уменьшается, темнеет и растворяется за fade interval. Одновременно один переиспользуемый ParticleSystem выпускает небольшой dust burst. Покадровые death sprites не генерируются.
4. Только после visual tail публикуется `Despawned` и объект возвращается в pool. Pause не продвигает эффект; run terminal/cleanup отменяет tail и освобождает объект сразу. Reinitialize очищает clone, particles, color, flip и scale.

Все параметры хранятся одним validated profile в `Content/Presentation/FixtureEnemyDeathPresentation.json`: durations >0; squash width 1…2; height/end scale 0.01…1 в пределах domain constraints; цвета RGBA 0…1; dust count 1…12; lifetime/speed/size >0. Текущий fixture: 0.10 s squash + 0.20 s fade, scale 1.12×0.72 → 0.15, пять dust particles размером 0.08 world units, приглушённого земляного цвета. Это один общий профиль, а не значения в individual enemy cards.

Acceptance: Died и reward происходят немедленно; collider/physics/target registry выключены в тот же кадр; root position до и после tail совпадает; пауза замораживает позу и delayed despawn; terminal cleanup не ждёт tail; Despawned/pool return происходят один раз; ordinary/boss/Traveler получают один profile; mixed pool reuse не сохраняет старую позу или частицы. Проверки и текущие результаты: [evidence](../implementation/evidence/2026-09-22-shared-enemy-death.md).

## 24. Единая процедурная ground shadow

Для playable body, ordinary enemy, boss и Traveler используется одна мягкая эллиптическая тень. Отдельный raster asset не производится: `GroundShadowSprite` один раз создаёт общую radial alpha mask 32×32, а каждый актёр имеет только `SpriteRenderer`, который растягивает эту маску до ellipse. Маска не создаётся на каждого врага и не участвует в physics.

Высота, цвет, прозрачность, вертикальное смещение, fallback-width/ground point и `contactWidthScale` задаются в `Content/Presentation/FixtureGroundShadowPresentation.json`. Для body с contact profile world-width тени равен `2 × contactRadius × contactWidthScale`; вычисление выполняется один раз при initialize/reuse и не читает sprite pixels. Тень находится у ground point body: `-contactCenterY + offsetY`; при отсутствии contact profile применяются общие fallback-значения. Для scaled enemy root local position и scale делятся на `collisionSize`, поэтому тень и body, чей `VisualRoot` компенсирует collision scale, остаются в одной визуальной системе координат. Sorting order равен body order минус один. Тень не наследует bob/tilt/squash дочернего `BodyRoot`.

Один профиль передаётся composition root игроку, ordinary spawner, boss encounter и Traveler encounter. Pool reuse повторно включает и перенастраивает тот же renderer; cleanup отключает его. Во время короткого death tail тень остаётся на исходной позиции до `Despawned`, затем выключается вместе с объектом.

Acceptance: player и все enemy categories получают один profile; два актора используют тот же `Sprite`; тень не добавляет collider; contact radius определяет ширину, contact center — ground alignment; collision-size compensation не меняет её world size; pool reuse не создаёт дополнительные renderers; raster manifest не содержит отдельного shadow PNG. Проверки и текущие результаты: [evidence](../implementation/evidence/2026-09-22-courier-and-ground-shadows.md).

## 25. Projectile sprite, вращение и дешёвый impact

Projectile raster проходит тот же approval → immutable source/master → 256×256 runtime derivative → provenance/manifest путь. Запись `SpriteRole.Projectile` обязана иметь `projectile` profile в `FixtureSprites.json`: относительный visual scale, spin в градусах в секунду и параметры общего impact burst. Gameplay radius остаётся authoritative и задаётся механикой; визуал нормализуется относительно `Sprite.bounds` и диаметра collider, поэтому импортный PPU и прозрачный padding не меняют попадание.

Physics root не вращается. Направленный sprite живёт в дочернем `ProjectileVisual`, при spawn ориентируется вправо вдоль velocity; `spinDegreesPerSecond` вращает только этот child. Ноль сохраняет стабильную ориентацию письма/стрелы, небольшое ненулевое значение подходит камню или диску. Вращение продвигается только в `RunState.Running` и сбрасывается при pool reuse. Текущий fixture-камень проходит 5 world units — половину эталонной высоты экрана 10 units — при скорости 10 и lifetime 0.5 s на каждом уровне.

Impact не требует отдельного raster. Один переиспользуемый `ParticleSystem` на pooled projectile выпускает один мягкий flash particle и 2–4 material-colored particles в world space. Для terminal hit collider и sprite выключаются сразу, damage уже применён, а возврат в pool задерживается только на короткую жизнь частиц; pause замораживает tail, terminal run state очищает его немедленно. Для pierce тот же emitter оставляет частицы в world space, пока projectile продолжает путь. Цвета различают материал и не кодируют кровь: камень даёт земляно-серую пыль, письмо — тёплые parchment flecks.

Acceptance: custom visual имеет роль Projectile и полный profile; круглый collider сохраняет authored radius; child совпадает с направлением; spin не вращает physics root; placeholder остаётся fallback для fixtures без visual; hit damage не ждёт tail; pause/terminal cleanup/pool reuse не оставляют старый sprite или particles; source, runtime и approval зафиксированы в manifest. Текущая реализация: [evidence](../implementation/evidence/2026-09-22-projectile-art.md).

## 26. Pickup sprites, bob/pulse и разброс drops

XP, Зелье и Traveler Book используют отдельные 256×256 runtime derivatives с ролью `SpriteRole.Pickup`, centered pivot и category import profile. Их цвет и крупная форма различимы на gameplay scale: XP — cyan crystal, лечение — зелёная круглая бутылка, Book — охристо-бордовый закрытый том. Collider/collection radius остаются authoritative и не выводятся из пикселей.

Один `PickupSpritePresentation` создаёт дочерний `VisualRoot` и применяет небольшой bob/pulse только в running-time. Root, trigger и authoritative position не двигаются и не масштабируются. Shutdown/pool return выключает renderer, очищает sprite/tint и возвращает transform baseline; отдельные raster frames, shadow и particle emitter не требуются.

Перед placement drop получает смещение, равномерное по площади диска радиуса `0.30` world units. XP и world pickups используют отдельные seeded RNG streams; scatter не расходует chance RNG. После смещения Зелье/Book проходят обычный reachable-point adapter. Основание: [DECISION-0043](../decisions/0043-seeded-drop-scatter.md).

Acceptance: три sprites зарегистрированы как Pickup и имеют source/provenance/runtime records; визуальная анимация замораживается на pause и не влияет на collider; последовательные drops из одной source point получают разные позиции внутри radius; pool reuse не сохраняет фазу/scale/tint; финальный gameplay-scale review остаётся пользовательским gate.

## 27. Минимальный environment kit и visual-only fixture binding

Первый FIELD-001 art pass состоит из одного ground tile, одного boundary prop, одного obstacle prop и двух лёгких decor props. Полный tileset, здания и landmarks не производятся до подтверждения базовой палитры и масштаба. Каждый raster имеет отдельные source/master/provenance/runtime records и роль `Tile` либо `Prop`.

Первый проход не менял geometry: один tiled renderer покрывал арену, плетень обозначал только четыре далёкие boundary colliders, а пень накрывал единственный `Obstacle_Fixture`. Gameplay review показал, что на 200×200 поле такой набор почти не встречается. По [DECISION-0045](../decisions/0045-field-density-and-200-enemy-cap.md) fixture теперь дополнительно создаёт data-driven внутренние пни и плетни со статическими player-only colliders. Внутренний плетень всегда расположен горизонтально: вертикальное применение этого raster плохо читается в текущей перспективе. Куст и трава остаются без collider; их более плотная seeded-расстановка не накрывает gameplay obstacles и оставляет свободную зону у spawn.

Ground derivative может использовать зеркальную сборку краёв для дешёвого бесшовного повторения. Tile PPU выбирается из целевого world repeat, а не из character default. Prop PPU и presentation scale фиксируются отдельно; они не выводят размер collider из пикселей. В одном environment создаётся один visual root, который полностью удаляется при shutdown/restart, а скрытые scene placeholders восстанавливаются.

Acceptance: typed references разрешаются через общий registry; import profiles соответствуют Tile/Prop; boundary совпадает с physics; каждый внутренний пень/плетень имеет ровно один player-only collider; pickup/Traveler placement учитывает его bounds; decor не имеет physics components; одинаковые seeds дают одинаковую раскладку; cleanup не оставляет второй ground, obstacles или decor root. Исходный visual-only проход описан [DECISION-0044](../decisions/0044-field-environment-art-is-presentation-only.md), действующая плотность и obstacle contract — [DECISION-0045](../decisions/0045-field-density-and-200-enemy-cap.md).

## 28. Пакет UI-иконок навыков

Skill icon производится отдельной ролью `icon`, даже если для того же навыка уже существует projectile или VFX raster. Для каждого `SKILL-XXX` сохраняются `icon/vNNN/concept-NN.png`, `icon/selected-master.png` и отдельный `icon/asset-record.json`; world-art record в корне skill folder не переиспользуется как provenance иконки.

Runtime-файл имеет стабильное имя `Assets/Resources/Art/UI/Icons/Skills/skill-XXX-icon.png`, visual ID `SKILL-XXX-VISUAL-ICON` и `SpriteRole.Icon`. Shared import profile ограничивает импорт до 256, ставит center pivot и не добавляет baked frame. Master остаётся lossless и неизменным; уменьшение выполняет Unity importer. Проверка проводится в реальном draft card и occupied build slot при минимальном размере UI.

До появления production definitions разрешено временно назначить production icon существующему fixture skill только при ясном механическом соответствии. Такое назначение фиксируется decision/evidence, не переименовывает fixture ID и не считается реализацией production content. Иконку без соответствующего fixture skill следует импортировать и зарегистрировать без ложного mapping. Текущий пакет и mapping: [DECISION-0047](../decisions/0047-skill-icon-fixture-mapping.md), [evidence](../implementation/evidence/2026-09-22-skill-icons.md).

## 29. Пакеты UI-иконок пассивок и сетов

Пассивки и сеты используют тот же immutable icon packet, import profile и approval gate, что навыки. Исходники хранятся в `Art/Source/Passives/passive-XXX/icon/` и `Art/Source/Sets/set-XXX/icon/`; runtime-файлы — в `Assets/Resources/Art/UI/Icons/Passives/` и `Assets/Resources/Art/UI/Icons/Sets/`. Стабильные visual ID имеют вид `PASSIVE-XXX-VISUAL-ICON` и `SET-XXX-VISUAL-ICON`, роль всегда `SpriteRole.Icon`.

Fixture mapping допустим только при ясном механическом соответствии. Отсутствие такого соответствия не блокирует импорт и регистрацию утверждённой иконки: она ожидает production definition IP-18 или IP-19. Presenter разрешает typed icon reference через общий registry; пассивка использует иконку в draft/build slot, приобретённый сет — в set row. Null reference остаётся допустимым для изолированных тестовых definitions.

Текущий approved пакет включает 14 пассивок и 20 сетов. Девять fixture-пассивок и четыре fixture-сета получили соответствующие ссылки; остальные зарегистрированы без ложной gameplay-привязки. Основание и проверка: [DECISION-0048](../decisions/0048-passive-and-set-icon-fixture-mapping.md), [evidence](../implementation/evidence/2026-09-22-passive-and-set-icons.md).

## 30. World-art для орбитального клинка, бумеранга, рикошетного диска и взрывной сферы

`SKILL-003`, `SKILL-006`, `SKILL-008` и `SKILL-014` имеют по одному утверждённому прозрачному projectile master и 256×256 runtime derivative. UI icon остаётся отдельной ролью и provenance-записью. Все четыре world-sprite используют `SpriteRole.Projectile`, centered pivot и общий projectile import profile; gameplay radius, орбита, return, ricochet и blast radius не выводятся из пикселей.

Один progression-level `visualId` наследуется всеми уровнями навыка, пока конкретный level не задаёт осознанный override. Fixture mapping используется только для визуального review механически соответствующего framework-паттерна и не регистрирует production definition.

Орбитальный клинок не создаёт projectile physics root. На первой damage-выборке активации создаётся один pooled `SpriteRenderer` на каждый клинок, каждый остаётся visual-only child владельца. Позиция и касательная ориентация вычисляются из уже утверждённых `bladeCount`, `radius`, `angularSpeedDegrees` и duration; pause не двигает визуал, terminal/clear возвращает renderers в pool. Damage sampling и `bladeHitboxRadius` не меняются.

Бумеранг, рикошетный диск и сфера используют существующий `ProjectileVisual`: collider и physics root не вращаются, child spin настраивается в profile и сбрасывается при reuse. Сфера дополнительно содержит optional explosion profile. Общий `ExplosionBurstRuntime` создаёт один мягкий core flash и небольшой радиальный particle burst в world space, масштабируя только presentation от authoritative blast radius. Damage выполняется до visual tail; pause замораживает burst, terminal cleanup очищает его немедленно, pool return не сохраняет частицы. Этот runtime не зависит от ID сферы и может быть повторно использован миной и set-effects через явный profile без нового raster.

Acceptance: четыре references разрешаются registry и имеют Projectile role/profile; все уровни соответствующих fixtures наследуют один world visual; orbit создаёт точное число visual-only blades и очищает их; projectile spin не вращает collider root; sphere impact/expiry запускают общий burst без задержки damage; pause/terminal/pool reset не оставляют sprite или particles; masters, prompts, approval, runtime files и manifest records синхронизированы. Текущий пакет: [evidence](../implementation/evidence/2026-09-22-skill-world-art.md).
