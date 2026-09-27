# DECISION-0072 — Навигация, генерация и ownership звука

Status: Approved (пользователь поручил реализовать предложенный рефакторинг)
Date: 2026-09-27
Related IP: IP-00, IP-26, IP-33; REPO-01
Related content IDs: существующие ID сохраняются

## Context

STATUS смешивал актуальную очередь и длинную историю поручений. README/Cursor
содержали устаревший выбор по номеру IP. Звук находился в Bootstrap/Settings,
включая общие клипы под FIELD-001; генератор с именем FIELD-001 обслуживал несколько
полей и каталоги. Общий check runner не запускал audio integrity/generation checks
и не включал их внешние authoring inputs в fingerprint.

## Decision

- STATUS сохраняет единственное право задавать execution order/status/границы.
  История выносится в датированный evidence; действующие gates и разрешения остаются
  в STATUS. PROJECT_MAP хранит только ссылки на owning code, данные и checks.
- `Game.Audio` владеет каталогом, DTO, проигрыванием, routing/preview implementation,
  импортёром и своими EditMode tests. Settings хранит предпочтения/IAudioPreview;
  Bootstrap создаёт зависимости. Обратных зависимостей gameplay/UI на Audio нет.
- Общие аудиоклипы находятся в Music/Sfx, атмосфера — Ambience/FIELD-001.
  Каталог использует `fieldAmbiences: [{fieldId, clip, gain}]` вместо единственных
  `fieldAmbience`/`ambienceGain` и bool в composition. Существующая единственная
  запись FIELD-001 сохраняет clip/gain; прочие поля по-прежнему без атмосферы.
  Все прежние `.meta`, hashes клипов, cue IDs, gain/cooldown, pause и voice policies сохраняются.
- `RuntimeContentCatalog.CreateFixture/CreateProduction` явно называют общий
  composition catalog и выбор данных; переименование не меняет content IDs/сохранения.
- `scripts/content/` делит генератор по категориям. `SOURCE_PATHS` и `TARGETS`
  определяют входы/выходы; старый CLI остаётся оболочкой. Generated JSON должны
  остаться побайтово прежними. `read_card.py` читает полную карточку из канона.
- `full` включает generation/audio validators; `audio` проверяет integrity и
  Audio EditMode; `content` — только generation без Unity. Scoped code/docs
  checks добавляют validators затронутых authoring/audio inputs. Fingerprint
  включает SOURCE_PATHS и SOURCES.json; reuse сохраняет прежнюю дату evidence.

## Consequences

Обновляются AGENTS, scoped rules, навигация, scripts/README, IP-33 и STATUS.
Текущие implementation-фразы удаляются из спецификаций в пользу ссылки на STATUS.
Gameplay rules, численные baseline, растры и канонические карточки не меняются.
Приёмка структуры требует Python regression tests, generation/audio integrity,
проверки ссылок/GUID и безопасного полного Unity smoke. Ручные gates прежних IP
этими проверками не закрываются.

## Approval

Пользователь 2026-09-27 ответил «Давай реализуй» на шесть предложенных направлений
рефакторинга. Это основание технических переносов и их проверок; разрешение
не распространяется на новые gameplay правила или следующий IP.
