# IP-10A — UI Foundation and test harness

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-10a--ui-foundation-and-test-harness).

## Цель

Текущие gameplay-системы наблюдаемы и управляемы через тестируемый runtime UI до реализации полного player flow.

## Зависимости

IP-01, IP-03, IP-06, IP-07, IP-10.

## Scope

UI Toolkit root and runtime theme; immutable HUD/draft/build/run-overlay ViewState; pure presenter and view/model contracts; visible HP/XP/level/timer HUD; 6+6 build slots; level-up selection/reroll/banish; pause and win/loss overlays; Editor/Development Build test controls; stable semantic element names; removal of gameplay-owned IMGUI.

## UI / observability

Foundation owns shared rendering infrastructure only. Feature meaning and each later ViewState extension remain owned by the IP that introduces the player-visible state.

## Context

- Game Design: «Структура забега и условия завершения», «Опыт и level-up».
- Content Design: не требуется.
- [DECISION-0005](../../decisions/0005-vertical-ui-delivery.md).

## Acceptance criteria

UI renders current model state without owning it; player can see HP/XP bars and 6+6 build slots and complete the existing draft flow; presenter logic runs without a Unity scene; development controls are gated; semantic UI names are stable; scene smoke covers resolved HUD geometry, build and draft visibility.

## Проверки

Presenter state/intent tests, UXML semantic-name validation, Gameplay scene wiring, PlayMode HUD/draft flow, full regression.

## Out of scope

Character/field/meta screens, production styling, final art/animation/audio and the complete IP-26 flow.
