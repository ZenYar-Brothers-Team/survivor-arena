# Enemy projectile art — 2026-09-27

## Scope

- Пользователь утвердил семь projectile-кандидатов: `ENEMY-010`, `ENEMY-011`, `ENEMY-012`, `ENEMY-014`, `ENEMY-015`, `ENEMY-018`, `ENEMY-019`.
- `art_pipeline.py` импортировал v001 sources, selected masters, provenance, runtime PNG, import profiles, manifest и `FixtureSprites.json` definitions.
- `scripts/content/actors.py` и сгенерированный `ProductionEnemies.json` назначают каждому ranged enemy собственный `*-VISUAL-PROJECTILE`; melee IDs не получили искусственных projectile bindings.
- Все новые hostile projectile profiles используют прежний размер ореола `2.6` и pulse `0.35/0.18`, но приглушённые RGB `0.70/0.18/0.16` и alpha `0.32`.
- Runtime сохраняет sprite выше ореола: один sorting layer, `threatHalo.sortingOrder = projectile.sortingOrder - 1`.

## Verification

- `python scripts/art_pipeline.py TestResults/art-previews/enemy-projectiles-2026-09-27/approved-art-packet.json --apply` — APPLIED; повторный запуск идемпотентен, `changedFiles: 0`.
- `python scripts/content/generate.py --check` — UP TO DATE.
- `python scripts/check_project.py --scope art` — PASS, EditMode 50/50, manifest/provenance 188/188; `TestResults/checks/20260927T135702-456795Z/summary.json`.
- `python scripts/check_project.py --scope full` — PASS, EditMode 857/857 и PlayMode 30/30, generation/audio integrity и manifest 188/188; `TestResults/checks/20260927T135734-053265Z/summary.json`.

## Remaining gate

- Ручной gameplay-scale review в плотном бою: силуэт, направление, относительный масштаб и читаемость приглушённого ореола. Автоматические проверки подтверждают binding, schema, sorting и минимальный размер ореола, но не заменяют визуальную приёмку.
