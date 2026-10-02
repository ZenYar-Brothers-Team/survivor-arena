# FIELD-007 — типы алтарей и святынь

[Открыть галерею](index.html): все 14 кандидатов крупно и уменьшенно,
а также схема общего плетёного контура. Это preview, не gameplay capture.
Все 14 изображений прямо утверждены пользователем: «Хорошо, подтверждаю».
Изображения созданы встроенным OpenAI image generation, отдельным запросом
на каждый Prop. Промпты и references: [generation-jobs.json](generation-jobs.json).
Конкретные bytes/hashes зафиксированы в [approved packet](../../Packets/field007-altar-types-v1.json).

| Тип | Позитивный | Негативный |
|---|---|---|
| Ветер | [Завиток](field-007-altar-haste.png) | [Завиток](field-007-altar-haste-cursed.png) |
| Исцеление | [Росток](field-007-altar-heal.png) | [Росток](field-007-altar-heal-cursed.png) |
| Защита | [Щит](field-007-altar-ward.png) | [Щит](field-007-altar-ward-cursed.png) |
| Сила | [Песочные часы](field-007-altar-power.png) | [Песочные часы](field-007-altar-power-cursed.png) |
| Удары | [Молния](field-007-altar-strike-holy.png) | [Молния](field-007-altar-strike-cursed.png) |

Святыни: [жизни](field-007-shrine-life.png), [опыта](field-007-shrine-experience.png),
[защиты](field-007-shrine-ward.png), [мощи](field-007-shrine-power.png).

Выбраны показанные различия внутри пар ветра/силы и прежний солнечный рельеф
трёх святынь; тип читается по реликвии. После approval содержательных правок нет.
Runtime derivatives проходят fit 512 / padding 24 / alphaNoiseCutoff 32.
Основания/ступени сохраняют узнаваемость прежних изображений.

[art-packet.pending.json](art-packet.pending.json) — первоначальный review artifact:
содержит hashes, будущие
visual IDs/paths, profile и дословные prompts, но имеет
`approvedBy: pending-user-review`: это явная защита от импорта невыбранных pixels.
`python scripts/art_pipeline.py Art/Candidates/field007-altar-types-2026-10-02/art-packet.pending.json`
не должен применять такой пакет. Для работы использовать approved packet,
а не переименовывать или менять этот исторический review artifact.
`prepare_review.py` собирает галерею без редактирования изображений.
Середина двух нитей схемы точно совпадает с actual radius; белый пунктир —
только пояснение в галерее, отдельного круглого renderer в игре нет.

Контракт: [DECISION-0150](../../../docs/decisions/0150-field007-altar-type-visuals.md).
Текущий ход исполнения и проверки — только [STATUS](../../../docs/implementation/STATUS.md).
