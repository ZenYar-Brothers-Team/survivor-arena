# Results R1 — Unity-перенос и Book upgrade gold

Дата: 2026-09-28. Основание: принятый макет Results R1 и DECISION-0090.
Execution status и remaining gates — только в STATUS.

## Реализация

- `RunResultsProjection`/`RunResultsViewState`/`ResultContentViewState` отделяют
  результат от текстового summary магазина. Имена/иконки разрешаются через
  текущий registry, названия ещё не доставленных definitions — через MetaCatalog;
  отсутствие изображения не подменяется чужим контентом.
- `RunResultsPanel`, UXML/USS подключены внутри MetaScreen; navigation/profile
  intents и gates сохраняются. Meta/Settings не проходят новый visual pass.
  Новые открытия (сеты, active/passive, персонажи, карты) выше собранных сетов,
  одна прокрутка коллекции, footer вне scroll. Компактный вариант 1280×720.
- Награда только из `LastReceipt` с совпавшим RunId, а не из кошелька. До
  подтверждения записи суммы не объявляются полученными, открытия скрыты;
  при ошибке есть Retry Save, переходы заблокированы.
- Production `bookUpgradeReward=20`, fixture=0. Начисление в успешном Book Select,
  не в UI и не за обычный XP draft. Пустая книга по-прежнему 50; cancelled/duplicate
  не добавляют 20. `DraftTotals.BookCurrency` и receipt сохраняют общий итог.
  Старые receipts/schema профиля не пересчитываются.
- Новых растровых assets/import/provenance нет; используются существующие icons.
  Unity создала `.meta` новых C#/UXML/USS при импорте.

## Проверки

Unity 6000.6.0f1, safe runner; интерактивный Editor не закрывался, batch разрешён
свежим process/lock preflight.

1. UI/Progression/Meta EditMode: 279/279, 0 failed/skipped;
   `TestResults/checks/20260928T204547-900994Z/summary.json`.
2. Production Results graphics PlayMode: 1/1, 0 failed/skipped;
   `TestResults/checks/20260928T204743-744826Z/summary.json`.
3. Первый full: 935/936 EditMode; один новый тест ошибочно вызывал Stop(Defeat),
   запрещённый model API. Исправлено на Kill(); runtime не менялся для обхода теста.
4. Финальный `python scripts/check_project.py --scope full --graphics`: **936/936
   EditMode + 36/36 PlayMode**, 0 failed/skipped, third-party 0; generation UP TO DATE,
   audio 28 files / 15 cues PASS, art 256 records PASS.
   Receipt: `TestResults/checks/20260928T205143-284843Z/summary.json`.

Новые проверки: Book reward once/empty/cancel, receipt/reload/save retry,
outcome copy, отсутствующие facts/receipt, новые unlock kinds, production Book→Quit
→Results→Retry с прежним character/field. UI проверяет 20 сетов и смешанные открытия,
pending error/disabled actions и geometry/scroll в 1080p/720p.
Существующий полный набор покрывает gameplay lifecycle, profile flows и Meta.

## Визуальный просмотр

Просмотрены Unity captures `TestResults/results-mixed-1280x720.png`,
`results-error-1280x720.png`, а также 1080p error предыдущего scoped pass.
Сохранены production/mixed/error snapshots обоих размеров. Смешанный stress
пример демонстрационный, не утверждение о достижимом билде/награде.
После отчёта о Unity-переносе и проверках пользователь ответил
«Отлично, идем дальше»: приёмка этого среза зафиксирована. Точные условия
пользовательского просмотра не домысливаются; новых прогонов при этой записи нет.
