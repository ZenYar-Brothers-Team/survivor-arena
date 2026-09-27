# FIELD-003 art и читаемость hostile projectiles — 2026-09-27

## Scope

- Пользователь утвердил пять preview-кандидатов FIELD-003: ground, ruined wall, rubble, visual-only water и field-select thumbnail.
- `art_pipeline.py` сохранил immutable `v001` sources, selected masters, provenance, runtime PNG, import profiles, manifest и sprite definitions.
- Генератор content подключает `FIELD-003-VISUAL-GROUND/WALL/RUBBLE/WATER/BACKGROUND`; gameplay geometry, obstacle dimensions и layout не изменены.
- По дополнительному отзыву пользователя hostile projectile halo оставлен прежнего размера и pulse, но приглушён: RGB `0.95/0.30/0.25 → 0.70/0.18/0.16`, alpha `0.55 → 0.32` во всех 13 существующих hostile profiles.
- Runtime уже рисует halo под projectile: `EnemyProjectileRuntime.ConfigureHalo` использует тот же sorting layer и `sortingOrder = projectile sortingOrder - 1`.

## Verification

- `python scripts/content/generate.py --check` — UP TO DATE.
- `python scripts/check_project.py --scope art` — PASS, Unity 6000.6.0f1 EditMode 50/50, manifest/provenance 181/181; `TestResults/checks/20260927T133600-130840Z/summary.json`.
- `python scripts/check_project.py --scope code --platforms EditMode --filter '^Game\.Bootstrap\.Tests\.ProductionField003ContentTests$'` — PASS 4/4; `TestResults/checks/20260927T133629-894924Z/summary.json`.

## Remaining gates

- Target-scale gameplay review FIELD-003: ground repetition, rotated wall readability, rubble footprint, water density and thumbnail in Field Select.
- Reduced halo brightness requires visual confirmation in a dense hostile-projectile scene; automated checks confirm catalog validity and minimum halo scale, not artistic brightness.
- Последующий общий smoke после подключения enemy projectile packet также покрывает этот state: EditMode 857/857, PlayMode 30/30, manifest 188/188; `TestResults/checks/20260927T135734-053265Z/summary.json`.
