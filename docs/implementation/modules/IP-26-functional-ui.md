# IP-26 — Functional UI и полный player flow

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-26--functional-ui-и-полный-player-flow).

## Цель

Игрок проходит цикл selection → run → level-ups → result → meta → next run без debug commands.

## Зависимости

IP-07, IP-10, IP-10A, IP-16, IP-25. Production content screens используют только approved IDs.

## Scope

Character/field selection; integration and production presentation of the foundation HUD/draft/pause/result surfaces; build/sets; meta currency/unlocks/upgrades; complete screen navigation.

## Context

- Game Design: «Концепт и core loop» и все непосредственно отражаемые UI-секции: «Опыт и level-up», «Активные умения, пассивные предметы и слоты», «Сеты», «Поля», «Персонажи», «Мета-прогрессия».
- Content Design: только Approved content cards, которые показывает текущий экран; весь catalog не читать.

## UI / observability

Модуль интегрирует, визуально унифицирует и связывает уже существующие feature-owned surfaces; новые gameplay rules здесь не появляются. Проверяются navigation, focus/input ownership и full-flow consistency.

## Acceptance criteria

Новый profile может выбрать available character/field, сыграть run, управлять level-up, увидеть win/loss и применённый progress, затем начать следующий run.

## Проверки

Happy path, death path, draft path, unlock path.

## Out of scope

Final art, animation, audio, polish/accessibility spec unless separately defined.
