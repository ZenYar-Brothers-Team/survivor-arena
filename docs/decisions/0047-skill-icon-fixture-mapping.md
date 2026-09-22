# DECISION-0047 — Иконки production-навыков в текущем fixture UI

Status: Approved
Date: 2026-09-22
Related IP: IP-12A, IP-17, IP-26

## Основание

Пользователь поручил сделать иконки навыков и утвердил представленный комплект `SKILL-001…016`. Production definitions IP-17 ещё заблокированы неполными gameplay-карточками, но текущий draft и Pause / Build уже должны показывать новый арт для визуальной проверки.

## Решение

Все 16 утверждённых masters импортируются как `SpriteRole.Icon` под стабильными ID `SKILL-XXX-VISUAL-ICON`. Тринадцать существующих fixture-навыков получают ссылки на механически соответствующие production-иконки; это presentation mapping и не переименовывает fixture content и не создаёт production gameplay definitions. Иконки `SKILL-002`, `SKILL-013` и `SKILL-015` зарегистрированы в presentation catalog, но не назначаются несоответствующему fixture-навыку.

`GameplayUiPresenter` разрешает иконку через общий `ContentRegistry` и передаёт один и тот же `Sprite` в draft card и occupied active build slot. При отсутствии ссылки или registry старые synthetic/unit-test definitions сохраняют пустую иконку.

## Следствия

- пользователь может проверить читаемость 13 иконок в реально доступных draft/build состояниях до завершения IP-17;
- наличие raster и visual ID не объявляет production skill реализованным;
- окончательное соответствие всех 16 иконок production definitions выполняется в IP-17 без смены visual ID;
- замена утверждённого master требует новой версии, provenance и повторной проверки в UI slot.
