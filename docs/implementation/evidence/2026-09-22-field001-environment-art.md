# FIELD-001 environment art — 2026-09-22

## Scope

По поручению пользователя подготовлен минимальный набор «Деревенская окраина»:

- спокойная земля/трава для tiled background;
- плетень для границы арены;
- пень для существующего центрального препятствия;
- куст и пучок травы для декоративного слоя.

Пять generated masters сохранены неизменными в `Art/Source/Fields/field-001/<role>/selected-master.png` и `v001/concept-01.png`. Runtime derivatives находятся в `Assets/Resources/Art/Sprites/Fields/field-001/`; provenance, prompts, SHA-256 и preparation записаны в отдельных asset records и manifest. Изображения имеют статус Review до gameplay-scale оценки.

## Runtime presentation

`FixtureFieldEnvironmentPresentation.json` связывает fixture environment с пятью typed visual IDs, параметрами масштаба и seeded-расстановки. Ground создаётся одним tiled `SpriteRenderer`; mirrored-edge derivative уменьшает заметность швов и при 64 PPU повторяется каждые 8 world units. Четыре boundary renderer используют плетень. Пень визуально накрывает `Obstacle_Fixture`.

Кусты и трава размещаются по редкой сетке с детерминированными jitter, scale, flip и небольшим rotation. Они не имеют collider. Spawn point и центральное препятствие окружены свободным радиусом. Существующие scene colliders и player-only collision layers не изменяются. Shutdown удаляет `FieldEnvironmentArt` и восстанавливает placeholder renderer.

Основание: [DECISION-0044](../../decisions/0044-field-environment-art-is-presentation-only.md).

## Density revision after gameplay review

Первый проход оказался практически пустым: decoration spacing 10 / chance 0.45 давали меньше одного заметного prop на обычный экран; плетень находился только на границе ±100 world units; новым пнём был лишь visual существующего `Obstacle_Fixture`.

По прямому поручению пользователя spacing уменьшен до 6, chance увеличен до 0.75. Добавлены 64 внутренних gameplay obstacles, 16 из них — в радиусе 22 world units от старта. Плетни имеют axis-aligned box collider, пни — circle collider; `excludeLayers` оставляет столкновение только с Player. Pickup и Traveler placement получают их bounds. Основание: [DECISION-0045](../../decisions/0045-field-density-and-200-enemy-cap.md).

Следующая визуальная корректировка оставила все внутренние плетни горизонтальными (`rotation = 0°`); PlayMode smoke проверяет ориентацию каждого созданного `FenceObstacle`. Основание: [DECISION-0046](../../decisions/0046-stone-range-and-horizontal-fences.md).

Одновременно Final Rush regular cap увеличен `24 → 200`. Burst, bosses и Travelers сохраняют отдельные правила; 200 не является общим лимитом всех runtime объектов.

## Verification

- `python scripts/validate-art-manifest.py`: PASS, 25 owner/role records.
- Targeted content/import EditMode: **1/1**.
- Full EditMode: **651/651**, 0 skipped.
- Full PlayMode: **25/25**, 0 skipped.
- 200-enemy spawn/pool benchmark, 10 cycles: cold **20.182 ms**, warm max **2.717 ms**, 200 unique pooled instances.
- Horizontal-fence revision: **652/652 EditMode**, **25/25 PlayMode**, 0 skipped.
- Unity `6000.6.0f1`, 2026-09-22.

Gameplay smoke подтверждает создание `FieldEnvironmentArt`, ground/stump renderers и 64 внутренних obstacle colliders через реальную composition. Existing movement/collision, content registry, restart, enemy, pickup, draft и UI regressions проходят. Обязательный пользовательский review: контраст фона, заметность повторения tile, масштаб пня, читаемость границы, плотность декора и удобство проходов между препятствиями.
