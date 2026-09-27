# FIELD-004…010 field thumbnails — 2026-09-27

## Approval and scope

- Пользователь просмотрел семь generated candidates FIELD-004…010.
- После замечания о сходстве двух последних изображений FIELD-010 был переведён в отдельную закатную палитру при сохранении композиции.
- Пользователь утвердил интеграцию полного набора: «Хорошо, можно встраивать.»
- Выбранные варианты: FIELD-004…009 candidate 01; FIELD-010 sunset candidate 02.

Пакет покрывает только field-select thumbnails. Он не создаёт отсутствующие geometry,
metadata, encounter schedules или environment kits FIELD-004…010 и не меняет их
канонические карточки.

## Prepared assets

Для каждого FIELD-004…010 подготовлены:

- immutable `Art/Source/Fields/field-NNN/background/v001/concept-01.png`;
- `selected-master.png` и `asset-record.json` с prompt/provenance/approval;
- runtime `Assets/Resources/Art/Sprites/Fields/field-NNN/field-NNN-background.png`;
- import profile: 320 PPU, max size 512, centered pivot;
- registered `FIELD-NNN-VISUAL-BACKGROUND` с ролью `Background`.

Generated RGB bytes сохранены в candidate packet. Для pipeline созданы технические
RGBA-нормализации с полностью непрозрачным alpha и без изменения видимых пикселей.
Пакет: `Art/Packets/field-thumbnails-004-010-2026-09-27.json`.

## Verification

- `python scripts/art_pipeline.py Art/Packets/field-thumbnails-004-010-2026-09-27.json` — PLAN, 31 path.
- Та же команда с `--apply` — APPLIED, 31 path.
- `python scripts/check_project.py --scope art` — PASS.
- Unity 6000.6.0f1, batch runner: Game EditMode **50/50**, 0 failed, 0 skipped; third-party 0.
- Manifest/provenance audit: **195/195** owner/role records PASS.
- Content generation check: UP TO DATE.
- Evidence receipt: `TestResults/checks/20260927T141555-066708Z/summary.json`.

## Remaining

- FIELD-004…010 production definitions do not yet exist, so this packet deliberately
  adds no `ProductionFields.json` bindings and invents no missing field data.
- Bind each `thumbnailVisualId` when its owning field packet creates that production definition.
- Review center-crop/readability in the real Field Select card at target scale after binding.
- Environment kits, geometry and gameplay-scale field review remain IP-23 work.
