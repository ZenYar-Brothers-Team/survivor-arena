# Пороги открытий за обычные убийства — 2026-10-01

## Изменение

По поручению пользователя все восемь порогов `ordinaryKills` в `MetaEconomy.json`
увеличены в шесть раз относительно DECISION-0125; точный mapping —
[DECISION-0131](../../decisions/0131-ordinary-kill-unlock-thresholds.md).
Карточки Content Design синхронизированы. `ProductionFields.json` регенерирован
из `scripts/content/fields.py`: подсказка FIELD-002 теперь содержит оба пути
открытия. Production Field Select получает текущий прогресс и порог из профиля,
Meta Unlocks — из каталога; тексты называют именно обычных врагов.
Существующие открытия в профиле не отзываются.

Тесты проверяют все восемь значений, накопление 500 + 2500 убийств в двух
поражениях, идемпотентность повторного применения результата, сохранение
профиля и тексты прогресса. Ожидание каталога в
`ProductionFieldContentTests` обновлено до четырёх уже подключённых полей;
эта рассинхронизация обнаружилась первым полным прогоном.

## Проверки

- `python scripts/content/generate.py --check` — UP TO DATE.
- `python scripts/check_project.py --scope content` — STATIC PASS.
- `python scripts/check_project.py --scope full` — статические generation/audio/art
  проверки PASS; EditMode 1169/1169, 0 failed/skipped (Unity 6000.6.0f1,
  `TestResults/checks/20260930T205926-888627Z/EditMode.xml`).
- Полный PlayMode этого прогона — NOT RUN / INCOMPLETE: Unity завершился с
  кодом 3221225477 в render loop во время `ProductionRunSmokeTests`, XML не
  создан (`TestResults/checks/20260930T205926-888627Z/PlayMode.log`).
- `python scripts/check_project.py --scope code --platforms PlayMode --filter '^Game\\.Bootstrap\\.PlayModeTests\\.(RunAchievementSmokeTests|ProductionField002SmokeTests)'`
  — 2/2 PASS, 0 failed/skipped (`TestResults/checks/20260930T210310-331048Z/summary.json`).

Полный PlayMode smoke остаётся непроверенным после этой дельты; целевые пути
достижений и запуска FIELD-002 проверены отдельно.
