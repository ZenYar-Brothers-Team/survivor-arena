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

## Verification

- `python scripts/validate-art-manifest.py`: PASS, 25 owner/role records.
- Targeted content/import EditMode: **1/1**.
- Full EditMode: **651/651**, 0 skipped.
- Full PlayMode: **25/25**, 0 skipped.
- Unity `6000.6.0f1`, 2026-09-22.

Gameplay smoke подтверждает создание `FieldEnvironmentArt`, ground и stump renderers через реальную composition. Existing movement/collision, content registry, restart, enemy, pickup, draft и UI regressions проходят. Обязательный пользовательский review: контраст фона, заметность повторения tile, масштаб пня, читаемость границы и плотность декора.
