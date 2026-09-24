# FIELD-001 — открытые art gates стартового этапа

Исходный рабочий бриф для генерации изображений, которые нельзя было получить в
среде без генератора ([DECISION-0054 §3](../decisions/0054-field001-autonomous-execution.md#3-арт-без-генератора)).
Перечисленные ниже роли импортированы и подключены 2026-09-24; provenance,
результаты автоматических проверок и открытый gameplay-scale review —
[evidence](../implementation/evidence/field001-art-integration-2026-09-24.md).
Inventory и статусы изображений — [Art Production](Art%20Production.md); порядок
source → runtime → manifest → approval — [ASSET_PIPELINE](ASSET_PIPELINE.md) и
`scripts/README.md`. Execution status — только [STATUS](../implementation/STATUS.md#field001-execution).

Для будущих отсутствующих изображений runtime показывает явный placeholder без
подмены fixture-картинкой; gameplay и данные от этого не зависят.

## Как подключить готовое изображение

1. Положить выбранный master в `Art/Source/<Category>/<id>/selected-master.png`,
   заполнить `asset-record.json` по шаблону (финальный prompt дословно).
2. Описать art packet по формату `scripts/README.md` (локальный файл в
   `TestResults/`, он не коммитится) и выполнить `python scripts/art_pipeline.py <packet>` → `--apply`.
3. Добавить visual ID в definition через `docs/balance/field001-baseline-v1.json`
   и `python scripts/generate_field001_content.py` (не править production JSON руками).
4. Подогнать contact circle по [ASSET_PIPELINE §22](ASSET_PIPELINE.md#22-подгонка-круга-контакта-для-world-body),
   затем `python scripts/check_project.py --scope full` при закрытом Editor.

## Общий стиль (из принятых ENEMY-001/002)

Hand-painted 2D storybook cutout, тёмно-сливовый внешний контур, простые cel-shaded
массы, сдержанная бумажная текстура, мягкий тёплый свет сверху-спереди; full body,
3/4 top-down, лицом вправо, нейтральная animation-ready поза, обе ступни видны;
один персонаж по центру квадратного прозрачного холста с запасом. Без фона, пола,
запечённой тени, текста, свечения, эффектов движения. Читаемость на 128–160 px;
силуэт совместим с одним кругом контакта (Art Production, правило body).

## Запросы

Роль и размер — из карточек Content Design; конкретный костюм не канонизирован,
поэтому ниже только ограничения, вытекающие из карточек. Детали костюма —
творческий выбор автора изображения.

| ID | Имя (CD) | Что должно читаться | Размер (CD) | Runtime category |
|---|---|---|---|---|
| ENEMY-003 | Дровосек | медленный выносливый блокирующий враг; топор, массивный корпус | 1.15 | enemy body |
| ENEMY-004 | Пращник | базовый стрелок; праща в руке (камень пращи уже есть) | 0.8 | enemy body |
| ENEMY-005 | Королевский лучник | точный стрелок средней угрозы; лук, королевская служба отличает от селян | 0.75 | enemy body |
| ENEMY-007 | Охотничья гончая | быстрая собака с рывком; четвероногий силуэт в круге | 0.75 | enemy body |
| MIDBOSS-001 | Старший загонщик | командир облавы, крупнее рядовых, рывки | 1.6 | boss body |
| BOSS-001 | Староста-герой | финальный босс FIELD-001; самый крупный, «герой деревни» | 2.4 | boss body |
| TRAVELER-001 | Дорожный громила | боевой Путник, тяжёлый, медленный, contact-давление | 1.35 (baseline) | traveler body |
| TRAVELER-002 | Бродячий стрелок | неагрессивный блуждающий странник, стрелок по имени, не атакует | 1.0 (baseline) | traveler body |
| TRAVELER-005 | Паломник со щитом | защитник врагов; большой щит, паломник | 1.4 (baseline) | traveler body |
| BOSS-001-FAN / RING | снаряды веера и кольца | враждебная палитра (коралловый ободок, как у камня пращи) | — | projectile |
