# DECISION-0013 — Изолированный procedural sprite presentation

Status: Approved

Date: 2026-09-16

Related IP: IP-12A; groundwork for IP-17, IP-20, IP-21, IP-22, IP-26

Related content IDs: `FIXTURE-CHARACTER-AGILE` vertical slice; production IDs не утверждаются этим решением

## Context

Gameplay-сцена пока рисует Player через `SpriteRenderer` на том же объекте, где находятся `Rigidbody2D`, `BoxCollider2D`, `PlayerMover` и gameplay runtime. `PlayerMover` требует root-level `SpriteRenderer`, сам подставляет `PlaceholderSprite` и красит его cyan. Аналогичный root-level pattern существует у fixture enemies. Если применить squash/stretch, tilt или hit recoil к этому root, визуальный эффект начнёт менять authoritative transform/collider hierarchy и может повлиять на movement, collision и camera follow.

Несколько независимых визуальных реакций также не могут безопасно писать в один `Transform`: locomotion, hit reaction, spawn и будущий attack recoil будут перезаписывать scale/rotation друг друга в зависимости от порядка `Update()`.

Текущий `Health.HealthChanged` недостаточен как hit signal: он вызывается не только при damage, но и при healing и пересчёте current health после изменения Max HP. Выводить damage reaction из отрицательной разницы health означало бы смешать gameplay profile changes и реальный удар. Pause в проекте также не меняет `Time.timeScale`; системы обязаны проверять `RunState` явно.

Пользователь утвердил Art Direction и Asset Pipeline с engine-driven animation, stable content-referenced visuals и первым vertical slice на `FIXTURE-CHARACTER-AGILE`. Нужна единая архитектура, пригодная затем для pooled enemies и других sprite entities.

## Decision

### 1. Gameplay root не анимируется presentation-кодом

Entity hierarchy для первого vertical slice:

```text
PlayerRoot                         authoritative gameplay transform
├── Rigidbody2D
├── Collider2D
├── PlayerMover / gameplay runtime
└── VisualRoot                    presentation-only local space
    ├── ShadowRenderer            не получает body squash/tilt
    └── BodyRoot                  единственная pose target
        ├── BodyRenderer
        ├── WeaponAnchor
        └── EffectAnchor
```

- `PlayerRoot` position/rotation/scale и collider geometry принадлежат gameplay.
- `VisualRoot` отделяет всё presentation-поддерево от gameplay components.
- `BodyRoot` получает bob, squash/stretch, tilt и recoil.
- Shadow является sibling `BodyRoot`, поэтому body deformation не растягивает землю/тень.
- Attachments наследуют body pose осознанно; world-space effects могут жить вне `BodyRoot`.
- Camera следует gameplay root, а не деформируемому body.

Фраза «анимация применяется к VisualRoot» означает только presentation subtree. Конкретная body pose применяется к `BodyRoot`, чтобы shadow и независимые effects не наследовали неподходящую деформацию.

### 2. Один writer для результирующей pose

Только `ProceduralSpriteAnimator` имеет право изменять local position/rotation/scale `BodyRoot`, `SpriteRenderer.flipX` и transient renderer feedback body.

Другие системы не пишут в эти свойства напрямую. Они передают animator входные данные или вызывают семантические реакции:

```text
SetMotion(velocity, referenceSpeed)
PlayHit(appliedDamage)
PlaySpawn()
ResetPresentation()
```

Conceptual motion channels:

- base pose;
- locomotion bob;
- locomotion squash/stretch;
- velocity tilt;
- facing;
- hit impulse;
- spawn impulse;
- flash/tint feedback.

Не требуется создавать отдельный класс на каждый channel в первой реализации. Но итог вычисляется одним compositor и применяется одним writer.

### 3. Pose composition

Каждый tick строит результирующую presentation pose из базовой pose и активных channels:

- position offsets складываются;
- rotation offsets складываются и затем ограничиваются profile bounds;
- scale multipliers перемножаются относительно captured base scale;
- facing изменяет `flipX`, а не отрицательный scale transform;
- flash/tint вычисляется отдельно от transform pose;
- итог ограничивается утверждённым Art Direction safety envelope.

`SpritePose` является value representation результата. Он не хранит gameplay state и может тестироваться без scene physics.

Последний ненулевой horizontal direction сохраняется как facing. Вертикальное движение в IP-12A не вводит отдельные directional sprites.

### 4. Motion profile является content/config data

Числовые параметры procedural motion не задаются literals или `[SerializeField]` defaults на runtime MonoBehaviour. Они загружаются из JSON под общей content tree:

```text
Assets/Resources/Content/Presentation/FixtureSpriteMotionProfiles.json
```

Profile получает stable content ID, например:

```text
FIXTURE-MOTION-GOBLIN-AGILE
```

Profile содержит только presentation values: отдельные idle bob/breath/sway, locomotion bob frequency/amplitude, stretch/squash, tilt, hit duration/strength, flash duration, spawn impulse и clamps. Idle проигрывается при нулевой скорости, плавно ослабевает с ростом скорости и полностью уступает место locomotion на reference speed. Domain constructor валидирует finite/range constraints через `Game.Content.NumericValidation`; JSON остаётся authoring layer.

`CharacterDefinition` ссылается на visual и motion profile через typed `ContentRef`. Missing или wrong-type references обнаруживаются при `ContentRegistry.Build()`, а не при первом кадре.

Presentation profile не может менять movement speed, collider, invulnerability, damage timing или другие gameplay values.

### 5. Авторитетные runtime signals

Motion:

- presentation читает фактическую `Rigidbody2D.linearVelocity`, а не raw input;
- поэтому knockback, scripted movement и будущие AI-controlled entities используют тот же pipeline;
- velocity нормализуется относительно явного reference speed/profile input, без изменения gameplay velocity.

Damage:

- `Game.Combat.Health` получает отдельное событие `Damaged`, вызываемое только когда `TakeDamage` реально применил положительный damage;
- payload содержит applied damage;
- healing, regeneration и Max HP rescale не вызывают `Damaged`;
- presentation подписывается на это событие и не пытается вывести удар из `HealthChanged`.

Pause/lifecycle:

- `RunState.Running` разрешает продвижение presentation time;
- `Paused`, `NotStarted`, `Won` и `Lost` не продвигают locomotion/hit/spawn timers;
- pause замораживает текущую pose, а не сбрасывает её;
- resume продолжает с замороженного состояния;
- `Shutdown()` и pool return сбрасывают baseline pose, flip, flash и все impulses.

Death animation не входит в IP-12A. Отдельный death presentation later может получить явно выбранную time policy; он не должен случайно обходить current run-state contract.

### 6. Runtime ownership и dependency direction

`Game.Presentation` владеет:

- `SpriteDefinition` и motion-profile definition;
- rig validation;
- pose representation/composition;
- procedural animator;
- generic presentation lifecycle/runtime adapter;
- fixture presentation catalogs/loaders.

Допустимые зависимости `Game.Presentation`: `Game.Content`, `Game.Content.Json`, `Game.Combat`, `Game.Run`, UnityEngine и временно `Game.Movement` до отдельного переноса legacy `PlaceholderSprite`. `Game.Presentation` не зависит от `Game.Character`, `Game.Enemy`, `Game.Progression` или `Game.Bootstrap`.

`Game.Progression.CharacterDefinition` может ссылаться на presentation definitions. `Game.Bootstrap` разрешает выбранные references, затем инициализирует player presentation после `PlayerCharacterRuntime` и включает его в reverse-order rollback через публичный `Shutdown()`.

Entity-specific gameplay modules могут посылать semantic presentation signals через узкие public APIs, но presentation не вызывает gameplay mutations.

### 7. Initialize/Shutdown и pooling

Presentation runtime следует repository lifecycle rule:

- `Initialize()` проверяет rig, захватывает baseline local transforms/color, принимает resolved sprite/profile/runtime sources и подписывается на signals;
- если type предназначен для pooled reuse, повторный `Initialize()` сначала выполняет cleanup предыдущей жизни;
- `Shutdown()` отписывается от `Health.Damaged`/других external events, сбрасывает transient state и возвращает rig в baseline;
- `OnDestroy()` использует тот же cleanup;
- скрытые static profiles, pools или cross-scene presentation state запрещены.

Хотя Player не pooled, первая реализация обязана иметь reset semantics, чтобы тот же компонент позже безопасно использовался врагами.

### 8. Renderer feedback

- Hit flash применяет `MaterialPropertyBlock` или эквивалентный shared-material-safe механизм.
- Runtime не создаёт уникальный material instance на каждый hit/entity.
- Base color/tint захватывается при `Initialize()` и восстанавливается при reset.
- Animator является единственным writer transient body tint/flash, чтобы несколько эффектов не оставляли renderer в промежуточном состоянии.
- Sprite replacement и profile replacement не должны терять исходный base tint.

### 9. Legacy root SpriteRenderer migration

Для Player root-level `SpriteRenderer` и presentation setup внутри `PlayerMover` удаляются как ownership violation:

- `PlayerMover` больше не требует/создаёт/красит visual;
- body renderer живёт только в presentation subtree;
- placeholder fallback принадлежит presentation catalog/runtime;
- movement tests проверяют движение независимо от renderer.

Enemy root renderer остаётся вне первого vertical slice и мигрирует по тому же pattern в IP-20 или отдельном approved extension. IP-12A не делает массовый enemy refactor «заодно».

### 10. Animator Controller и frame animation

Baseline IP-12A не использует Unity Animator Controller и не требует покадровой анимации. Procedural runtime является каноническим путём для idle/locomotion/facing/hit/spawn.

Будущий frame animation, sprite swap или 2D skeletal animation допустим как дополнительный source channel, но:

- не получает право писать gameplay root;
- не становится вторым writer `BodyRoot` pose;
- синхронизируется через тот же presentation runtime;
- не меняет collider/gameplay timing без отдельного gameplay decision.

## Consequences

- Squash/stretch и recoil не могут изменить authoritative movement/collision transform.
- Shadow остаётся стабильной при body deformation.
- Idle, locomotion, hit и spawn могут работать одновременно без last-writer-wins конфликтов.
- Damage reaction соответствует только реально применённому damage; healing/profile changes не дают ложных вспышек.
- Pause работает корректно без зависимости от `Time.timeScale`.
- Motion tuning становится data-driven и проверяемым, а не скрытым в scene defaults.
- Composition root получает ещё одну rollback-aware subsystem.
- Для реализации потребуется добавить `Health.Damaged`, character visual/motion references, JSON fixture profile, presentation rig/runtime, scene hierarchy и tests.
- Root-level renderer ownership PlayerMover будет удалён; это намеренная миграция ответственности без изменения movement behavior.
- Для массовых enemies архитектура готова, но их миграция остаётся вне IP-12A.

## Verification contract

Автоматически проверяется:

- pose composition при нескольких одновременно активных channels;
- safety clamps и finite validation;
- horizontal facing сохраняет последний ненулевой direction;
- `Health.Damaged` вызывается только для positive applied damage;
- healing/Max HP changes не запускают hit channel;
- pause не продвигает channel time;
- resume продолжает frozen state;
- reset/shutdown возвращают captured baseline;
- `PlayerRoot` transform и collider size не меняются при locomotion/hit/spawn;
- repeated initialize не сохраняет impulses/flash предыдущей жизни;
- missing/wrong visual or motion reference отклоняется content registry;
- Gameplay scene рисует approved fixture sprite через child body renderer.

Manual check подтверждает target-scale silhouette, amplitudes, pivot, flip и отсутствие shadow deformation/alpha artifacts.

## Approval

Пользователь утвердил Art Direction и Asset Pipeline 2026-09-16, затем явно запросил следующий шаг утверждённого IP-12A плана. В рамках предоставленного пользователем права самостоятельно определить техническую структуру это решение фиксирует согласованный procedural-animation подход без изменения Game Design или production content.
