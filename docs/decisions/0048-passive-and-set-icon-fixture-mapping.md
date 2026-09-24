# DECISION-0048 — Иконки production-пассивок и сетов в текущем fixture UI

Status: Approved
Date: 2026-09-22
Related IP: IP-12A, IP-18, IP-19, IP-26

## Основание

Пользователь поручил последовательно создать иконки пассивок и сетов, затем утвердил весь представленный комплект из 14 passive icons и 20 set icons. Production definitions IP-18 и IP-19 ещё не завершены, но доступный fixture UI позволяет заранее проверить часть изображений на реальном размере.

## Решение

Все 34 утверждённых master импортируются как `SpriteRole.Icon` под стабильными ID `PASSIVE-XXX-VISUAL-ICON` и `SET-XXX-VISUAL-ICON`. Девять существующих fixture-пассивок и четыре fixture-сета получают ссылки на механически соответствующие production-иконки. Это presentation mapping: fixture ID и механики не переименовываются, а наличие иконки не считается реализацией production content.

Остальные пять passive icons и шестнадцать set icons зарегистрированы без несоответствующей gameplay-привязки. `GameplayUiPresenter` разрешает иконку пассивки для draft/build slot и иконку приобретённого сета для set row через общий `ContentRegistry`.

## Следствия

- текущий fixture run показывает девять passive icons и четыре acquired-set icons для проверки масштаба и читаемости;
- все 34 стабильных visual ID готовы к будущей привязке production definitions в IP-18/IP-19;
- отсутствующая icon reference остаётся допустимой для изолированных тестовых definitions;
- замена утверждённого master требует новой версии, provenance и повторной проверки в UI.
