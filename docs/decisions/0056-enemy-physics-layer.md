# DECISION-0056 — Физический слой врагов для area-запросов

Status: Approved
Date: 2026-09-25
Related IP: IP-08 (active skill framework), IP-14/IP-20 (enemies), IP-16 (field physics)
Related content IDs: —

## Context

Живой плейтест 2026-09-25 (Editor.log): `EnemyDamageArea.Apply` стабильно 20–31 ms при PerfGuard-пороге
2 ms. `EnemyDamageArea.Apply`, `ApplyCircles`, `ExpandingAreaState`, `PersistentOrbitState` и аура SET-010
вызывают `Physics2D.OverlapCircle` с `ContactFilter2D.noFilter` и затем для каждого найденного коллайдера
вызывают `GetComponentInParent<IEnemyDamageReceiver>()`; не-враги отбрасываются уже после этой работы.

Проверка схемы слоёв (`ProjectSettings/TagManager.asset`, `Assets/Game/**`): из пользовательских слоёв
существует только `Player` (используется для player-only препятствий, DECISION-0045). Выделенного слоя
врагов нет: `EnemyRuntime` (единственный `IEnemyDamageReceiver`: обычные враги, боссы, Путники),
снаряды игрока и врагов, XP drops, pickups, стены и 64 препятствия поля — все на `Default`. Поэтому
ограничить запрос слоем сейчас нельзя без новой слоевой схемы, а вводить её молча в рамках perf-фикса
запрещено.

Сопутствующее наблюдение: trigger-коллайдер XP drop имеет радиус XP pickup radius (`ExperienceDropRuntime.
RefreshColliderRadius`); после DECISION-0055 на PASSIVE-007 L6 это 2.5 units. Каждый такой коллайдер
попадает в любой AoE-запрос рядом с ним, поэтому стоимость area damage растёт с числом лежащего опыта.

## Decision

1. Завести слой `Enemy` в TagManager; `EnemyFactory`/`EnemyRuntime` ставят его на root и коллайдеры
   врагов (включая боссов и Путников). Остальные объекты остаются на своих слоях.
2. Все area-запросы урона (`EnemyDamageArea.Apply/ApplyCircles`, `ExpandingAreaState`, `PersistentOrbitState`,
   `SetEffectHost` аура) используют `ContactFilter2D` c `layerMask = Enemy`, `useTriggers` как сейчас.
3. Проверить collision matrix: слой `Enemy` сохраняет текущие столкновения с `Default`/`Player`
   (поведение движения и контакта не меняется); player-only препятствия уже исключают всё, кроме `Player`.
4. Тест: area-запрос не вызывает `GetComponentInParent` для XP/снарядов (например, счётчик результатов
   запроса при 100 XP drops рядом) и поведение урона не меняется.

Альтернатива без слоя: фильтр `useTriggers = false` отсечёт XP, снаряды и pickups (все trigger), но
закрепит неявный контракт «получатели урона — только non-trigger коллайдеры»; это тоже изменение
поведения и требует того же одобрения.

## Consequences

Реализовано 2026-09-26:

- `ProjectSettings/TagManager.asset`: слой 7 `Enemy`. Collision matrix не менялась (все пары
  сталкиваются), поэтому движение/контакт с игроком, снаряды и стены работают как раньше;
  player-only препятствия по-прежнему исключают всё, кроме `Player` (`excludeLayers`).
- `EnemyPhysicsLayer` (Game.Enemy): индекс слоя (ошибка, если слоя нет) и `CreateQueryFilter()` —
  `noFilter` (triggers включены) с маской `Enemy`. `EnemyRuntime.Initialize` ставит слой на root —
  единственный объект с коллайдером врага; обычные враги, боссы и Путники создаются через `EnemyFactory`.
- Пять area-запросов используют этот фильтр. Альтернатива «фильтр по trigger» отклонена.
- Тесты: `EnemyPhysicsLayerTests` (слой врага; 100 trigger-коллайдеров рядом не попадают в запрос,
  урон прежний) и существующие area/orbit/set тесты без изменений.

Проверка: Unity 6000.6.0f1 full smoke 2026-09-26 — EditMode 757/757, PlayMode 27/27, 0 skipped
(`TestResults/checks/20260926T062226-949358Z/summary.json`).

Проблема 3 perf-аудита 2026-09-25 закрыта в коде. В игре пользователь 2026-09-26: «проверил, всё хорошо»;
замеры времени `EnemyDamageArea.Apply`/FPS не снимались. Новый код, которому нужны враги в физическом запросе, использует этот фильтр;
новые коллайдеры на объектах врага должны получать тот же слой.

## Approval

Утверждено пользователем 2026-09-26: «0056-enemy-physics-layer делаем отдельный слой» — выбран слой `Enemy`.
