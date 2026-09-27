# FIELD-001…010 obstacle art packet — 2026-09-27

## Approval and scope

- Пользователь запросил для каждой карты набор из трёх маленьких, одного среднего
  и двух умеренно крупных препятствий; самые маленькие должны читаться примерно как
  бочка или пень FIELD-001.
- Для уже готовых FIELD-001…003 пользователь отдельно попросил дополнить недостающие роли.
- Все кандидаты были показаны inline. Интеграция утверждена сообщением:
  «отлично, добавляй их в проект» (2026-09-27).

Пакет добавляет 52 новых prop asset: 2 для FIELD-001, по 4 для FIELD-002/003 и
по 6 для FIELD-004…010. Он не создаёт отсутствующие production definitions,
geometry или encounter schedules FIELD-004…010.

## Asset preparation

- Packet: `Art/Packets/field-obstacles-2026-09-27.json`.
- Candidate inventory and tier map:
  `Art/Candidates/field-obstacles-2026-09-27/PROMPTS.md`.
- Каждый asset получил immutable `v001/concept-01.png`, `selected-master.png`,
  provenance record, 256×256 runtime derivative, manifest entry, import profile
  и `SpriteRole.Prop` registry entry.
- Presentation tiers: small — 160 PPU, medium — 128 PPU, large — 96 PPU.
  Картинка не определяет collider; geometry остаётся в field presentation data.
- Pipeline применил `cropAlpha: true`, padding 8 и `alphaNoiseCutoff: 16`.

## Runtime binding

- FIELD-001: hay bales и village handcart добавлены как два дополнительных
  визуальных паттерна при сохранении прежних типов player-only geometry.
- FIELD-002: milestone, bench, broken wagon и road barricade назначены отдельным
  pieces существующих рядов; размеры и количество collider rectangles не менялись.
- FIELD-003: ruined arch, broken urns, fallen capstone и collapsed well назначены
  pieces существующих ruin clusters; размеры, число препятствий и clear start не менялись.
- Для этого `FieldObstaclePiece`/`FieldObstacleDefinition` получили optional typed
  visual override. Registry валидирует такие ссылки; отсутствие override сохраняет
  прежнее kind-based mapping.
- FIELD-004…010 assets зарегистрированы и готовы для будущих production layouts;
  преждевременные bindings не создавались.

## Verification

- `python scripts/art_pipeline.py Art/Packets/field-obstacles-2026-09-27.json` —
  PLAN, 211 changed paths.
- Та же команда с `--apply` — APPLIED, 211 changed paths.
- `python scripts/check_project.py --scope art` — PASS.
- Unity 6000.6.0f1, batch runner: Game EditMode **50/50**, 0 failed, 0 skipped;
  third-party 0.
- Manifest/provenance audit: **247/247** owner/role records PASS.
- Evidence receipt: `TestResults/checks/20260927T152700-955720Z/summary.json`.
- Полный smoke после runtime binding — PASS: Game EditMode **859/859**,
  PlayMode **30/30**, 0 failed, 0 skipped; third-party 0.
- Full evidence receipt:
  `TestResults/checks/20260927T153748-635944Z/summary.json`.

## Remaining gates

- FIELD-001…003 требуют gameplay-scale review разнообразия, размера и halo в живом забеге.
- FIELD-004…010 получат bindings только вместе с их approved production field data.
- Основные ground textures FIELD-004…010 остаются отдельным следующим art packet.
