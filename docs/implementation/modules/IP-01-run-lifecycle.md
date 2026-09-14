# IP-01 — Run lifecycle, таймер, pause и завершение

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-01--run-lifecycle-таймер-pause-и-завершение).

## Цель

Один забег имеет явные состояния и корректно завершается победой/поражением.

## Зависимости

IP-00.

## Scope

Start/run/pause/resume/won/lost; gameplay timer; 15:00 как default; короткий test duration; остановка симуляции после end.

## Context

- Game Design: «Структура забега и условия завершения», «Управление, бой и выживание», «Ключевые параметры текущей версии».
- Content Design: не требуется.

## UI / observability

HUD показывает remaining time и pause action; pause/won/lost имеют отдельные overlay states. Surface поставляется через IP-10A и остаётся контрактом run lifecycle.

## Acceptance criteria

Pause останавливает gameplay time; живой игрок при 15:00 получает won; смерть раньше времени даёт lost; end state не продолжает gameplay.

## Проверки

State transitions, pause/resume, shortened test run.

## Out of scope

Enemies, boss spawn, result UI, meta rewards.
