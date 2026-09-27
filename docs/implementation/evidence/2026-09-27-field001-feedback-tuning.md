# FIELD-001: звук, бумеранг, взрыв, окружение и таймер — 2026-09-27

Основание: прямой отзыв пользователя 2026-09-27. Числа и связь слоёв зафиксированы в [DECISION-0069](../../decisions/0069-field001-feedback-tuning.md).

## Изменения

- `level.up` заменён на короткий Kenney CC0 pizzicato jingle, gain 0.40; файл получен и проверен без воспроизведения через компьютер. Источник, SHA-256 и обработка — в `docs/audio/SOURCES.json`.
- SKILL-006: меньше damage, дальность, targeting radius, радиус попадания и видимый масштаб. Production JSON и исходный baseline packet согласованы; генератор выдаёт тот же JSON.
- SKILL-014: уменьшены размер/непрозрачность и насыщенность оранжевой вспышки попадания и взрыва; gameplay area и damage не менялись.
- FIELD-001: 12×12 ячеек, два препятствия в каждой; пень, горизонтальный плетень, новая бочка и переиспользованный камень. Новый raster создан через `scripts/art_pipeline.py` с provenance и import profile. Player-only collider задаётся layout-габаритом.
- HUD: остаток времени `duration - elapsed`, округлённый вверх до секунды; в конце 00:00. Тайминг волн и условие победы прежние.

## Проверки

- `python scripts/audio/check_audio.py`: PASS, 28 файлов, 15 cue, hash/license/reference.
- `python scripts/generate_field001_content.py --check`: UP TO DATE.
- `python -X utf8 docs/balance/validate_field_layouts_v1.py`: PASS, FIELD-001 ровно 288 препятствий на каждом из 200 сидов, межъячеечные проходы ≥4.
- `python -X utf8 docs/balance/validate_field001_baseline.py`: PASS (исторические 64 authored rectangles остаются в review packet; runtime layout определён DECISION-0069).
- Unity safe runner: EditMode **855/855 PASS**, `TestResults/checks/20260927T095100-531054Z/EditMode.xml`; PlayMode **30/30 PASS**, `TestResults/checks/20260927T095810-431459Z/summary.json`. При PlayMode общий AudioListener и источники в batch режиме заглушены.
- Art check: импорт и каталоги 50/50 EditMode PASS, 151 provenance record PASS, `TestResults/checks/20260927T100005-535711Z/summary.json`.
- Docs check в смешанном рабочем дереве не применим: `--scope docs` принимает только Markdown-изменения. Текущие Markdown-файлы проверены вместе с полным runtime-срезом и `git diff --check`.

Остаётся пользовательский прогон для субъективной оценки тембра level-up, читаемости плотной карты и мягкости вспышки. Это не автоматическая приёмка художественного результата.
