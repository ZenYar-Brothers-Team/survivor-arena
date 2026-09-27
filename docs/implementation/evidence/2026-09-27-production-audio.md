# IP-33 — первый проход production audio, 2026-09-27

## Поручение и граница

Пользователь поручил AI начать интеграцию звуков, минимизировать звуковую мешанину в bullet hell и не делать отдельную музыку каждому боссу: достаточно одной-двух общих тем. Работа ведётся по [звуковой ведомости](../../audio/AUDIO_PLAN.md) как отдельный user-directed packet; F1-09 и поздние production content gates не объявляются выполненными.

## Источники и обработка

Поставлены 28 файлов OGG: 22 коротких SFX из Kenney Interface Sounds, RPG Audio, Impact Sounds и Digital Audio, а также 6 музыкальных/атмосферных файлов OpenGameArt. Для каждого [SOURCES.json](../../audio/SOURCES.json) фиксирует исходный файл, URL, автора, CC0 1.0, SHA-256 исходника и результата, длительность, частоту, каналы и обработку. Короткие файлы обрезаны по тишине, получили пяти-миллисекундные fade-in/out и peak target 0.70. Длинные дорожки перекодированы в OGG. `python scripts/audio/check_audio.py`: PASS, 28 файлов, 15 cue families, хеши/лицензии/ссылки совпали. Неиспользуемые кандидаты в проект не включены.

Импорт Unity: музыка и атмосфера `Streaming`, короткие SFX `DecompressOnLoad`, Vorbis quality 0.75 через `ProductionAudioImportPostprocessor`; `.meta` после повторного импорта подтверждают `loadType: 2` для меню и `loadType: 0` для UI cue.

## Runtime

`ProductionAudio.json` связывает события с клипами, gain и cooldown. `RunAudioRuntime` имеет 8 рядовых, 2 важных и 2 UI голоса плюс тихую атмосферу. Отдельный global cooldown для рядового потока — 0.12 секунды реального времени; XP 0.22 секунды; частые удары и смерти 0.33/0.40 секунды. Важные сигналы не занимают рядовые голоса. На отдельные снаряды нет звуковых обработчиков. Смена музыки идёт через существующий `SettingsAudioRuntime` и его Master/Music; SFX через Master/SFX. Босс использует один общий трек для всех IDs, результат — отдельные темы по RunOutcome.

## Проверки и открытая приёмка

- `TestResults/checks/20260927T090150-450684Z/summary.json`: Bootstrap EditMode 33/33 PASS после первого импорта.
- `TestResults/checks/20260927T090402-423705Z`: общий EditMode 848/848; PlayMode 29/30, failure `ProductionField003SmokeTests.Field003_StartsWithItsRuins_VerticalWallsKeepTheirRectangles_AndSpawnsOnlyItsPool`: ожидается 107, наблюдается 110 коллайдеров. Аудиослой коллайдеры не создаёт; параллельно менялись FIELD-003 layout/арт.
- `TestResults/checks/20260927T090754-161680Z/summary.json`: после политики density фокусный EditMode 1/1, PlayMode 3/3 PASS (catalog, production run, settings presentation).
- `TestResults/checks/20260927T091046-867189Z`: после import postprocessor EditMode 1/1, но runner не выдал общий PASS из-за изменения входных файлов во время проверки.
- `TestResults/checks/20260927T091252-154375Z/summary.json`: после полного повторного импорта и теста настроек импорта фокусный EditMode 2/2, PlayMode 3/3, zero skipped, PASS.
- `TestResults/checks/20260927T091607-038557Z/summary.json`: повтор после rollback-cleanup, фокусный EditMode 2/2, PlayMode 3/3, zero skipped, PASS.
- По сообщению пользователя PlayMode-тесты действительно выводили музыку на системные динамики вне обычного запуска игры. Исправлено: `SettingsAudioRuntime` и `RunAudioRuntime` создают muted `AudioSource` в batch mode; общая `[SetUpFixture]` для Bootstrap PlayMode тестов ставит `AudioListener.volume = 0` и восстанавливает прежнее значение после suite. `ProductionRunSmokeTests` проверяет нулевую громкость listener в тесте. `TestResults/checks/20260927T091805-548769Z/summary.json`: production run PlayMode 1/1 PASS после исправления.
- `TestResults/checks/20260927T091845-825005Z/summary.json`: полный фокусный повтор после подавления звука — EditMode 2/2, PlayMode 3/3, zero skipped, PASS.

Звуки пока не прослушаны агентом в реальном миксе; автоматические проверки не подтверждают тембр, субъективную громкость, гладкость музыкальных петель и различимость критических сигналов на плотной волне. Нужен игровой прослушивательный review на 1× и 5×, затем возможна настройка `ProductionAudio.json` без нового подбора всего набора.
