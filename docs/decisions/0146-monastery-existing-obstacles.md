# DECISION-0146 — Готовые препятствия монастырского поля

Дата: 2026-10-02. Approved по прямому поручению пользователя расставить ранее сгенерированные препятствия FIELD-007 с плотностью первой карты и закоммитить эту часть отдельно от фонового просмотра.

## Расстановка

Используются только шесть approved Prop из `Art/Packets/field-obstacles-2026-09-27.json`: cistern, footbridge, hedge block, herb planter, root mass и root stump. Новый raster не создаётся. Каждый получает обычный player-only collision rectangle; декоративно проходящий мост не вводится.

Плотность повторяет FIELD-001: квадратные ячейки 16 wu, два одиночных паттерна на ячейку. При размере 120×120 и edge margin 4 wu это 7×7 ячеек, целевые 98 предметов. Число меняется вместе с размером поля; start circle 6 wu свободен, внутри ячеек минимум 3 wu между паттернами, margin 2 wu оставляет проходы. Каждый запуск получает новый seed, reference seed 7002 воспроизводим. Шесть паттернов равновероятны, поворот 0 сохраняет нарисованную перспективу.

Последующее прямое уточнение пользователя: сначала расставить все алтари, затем препятствия. GameplayCompositionRoot заранее генерирует altar placements без obstacle outlines и сохраняет их; obstacle generator отклоняет кандидатов вокруг этих центров. Изменение obstacle seed не меняет уже выбранные алтари; запрет по скользящему screen сохраняется. Другие поля сохраняют прежний порядок. Если отдельный паттерн не помещается за placementAttempts, он пропускается существующим generator contract; число алтарей не сокращается и они не перегенерируются ради препятствий.

Основания алтарей не пересекают предметы: `altarObstacleRadius` 0.75 wu + `obstacleClearance` 2 wu = 2.75 wu от ближайшего outline. Область действия алтаря может накрывать предметы; это не физический контур алтаря. Optional override применяется только к IsAltar; отсутствие сохраняет прежнее правило полного effect radius у всех других полей. Межалтарное расстояние, counts, radii и screen cap не меняются.

## Владение и проверка

Authoring: `docs/balance/field007-altars-v1.json`; generator `scripts/content/fields.py`; runtime использует существующий FieldObstacleLayoutGenerator и FieldEnvironmentArtRuntime. Конкретные sprite overrides сохраняют approved арты вместо legacy пней первой карты. IP-23 — владелец подключения, DECISION-0145 — алтарный просмотр. Проверка ProductionMonasteryObstacleTests: matching density, все typed Prop refs, восемь пар obstacle/altar seeds, наличие всех 36 алтарей и foundation clearance. Текущий статус только в STATUS.

Фоновая техническая проверка и её import profile не входят в этот коммит.
