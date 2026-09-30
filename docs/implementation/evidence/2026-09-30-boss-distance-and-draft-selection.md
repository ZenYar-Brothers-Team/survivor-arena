# Дистанция прыжка босса и выбор улучшения одним нажатием — 2026-09-30

## Изменения

- [DECISION-0111](../../decisions/0111-boss-teleport-trigger-distance.md): `farDistance`
  BOSS-001…010 изменён с 6.25 на 7.1875 units (+15%) в действующих авторских
  пакетах FIELD-001, FIELD-002 v2 и bosses-v1. `ProductionBosses.json`
  регенерирован. Таймер 2 s и параметры удара сохранены.
- [DECISION-0112](../../decisions/0112-draft-one-click-selection.md): в Draft/Book
  нажатие карточки выбирает улучшение/сет, а в режиме Banish исключает вариант.
  Наведение и клавиатурный фокус показывают связанные сеты; справка остаётся при
  уходе курсора. Отдельные кнопки подтверждения удалены. Проверка revision в
  presenter сохранена.
- Тестовые ожидания и UI/UX синхронизированы. В старом тесте материала
  замедления заменено точное сравнение цветов на допуск `1e-5`: значения
  отличаются после передачи в shader, хотя вывод округляется одинаково.
  Валидатор bosses-v1 приведён к утверждённой формуле урона DECISION-0080.

## Проверки

- `python scripts/content/generate.py --check` — UP TO DATE.
- `python -X utf8 docs/balance/validate_bosses_v1.py` — PASS, 16 encounters.
- Unity 6000.6.0f1, `check_project.py --scope full`: EditMode **1085/1085 PASS**,
  0 failed/skipped; результат `TestResults/checks/20260930T083021-578673Z/EditMode.xml`.
- PlayMode **NOT RUN / INCOMPLETE**: batch Editor дважды упал в
  `RenderPipelineManager.DoRenderLoop_Internal` без result XML, сначала при полном
  прогоне (`TestResults/checks/20260930T083021-578673Z/PlayMode.log`), затем при
  запуске только UI smoke (`TestResults/checks/20260930T083315-001887Z/PlayMode.log`).
  Эти запуски не подтверждают поведение интерфейса в игровой сцене.

## Открытая проверка

Последующий full graphics smoke на текущей рабочей копии 2026-09-30 прошёл:
1085/1085 EditMode + 59/59 PlayMode, 0 failed/skipped
(`TestResults/checks/20260930T091916-526918Z/summary.json`). Это включает
production UI PlayMode smoke и закрывает автоматическую проверку после двух
предыдущих crash. Остаётся ручной просмотр выбора кликом, hover/focus, ухода
курсора, Reroll/Banish и Book, а также новой дистанции прыжка босса.
