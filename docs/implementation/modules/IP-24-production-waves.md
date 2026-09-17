# IP-24 — Canonical Wave / Encounter Content и field bindings

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-24--canonical-wave--encounter-content).

## Цель

После появления утверждённых schedules связать fields, enemy pools, pressure phases, mid-bosses и final bosses в полные 15-minute encounter definitions.

## Зависимости

IP-14, IP-20, IP-21, IP-23; Approved Wave / Encounter Content.

## Scope

Только Approved Wave / Encounter Content; exact timings/compositions/spawn rates; field-to-enemy/boss bindings; optional mid-boss schedule; final boss time.

## Context

- Game Design: «Структура забега и условия завершения», «Враги, волны, элиты и боссы», «Поля».
- Content Design: «Wave / Encounter Content» и полные карточки referenced FIELD/ENEMY/BOSS/MIDBOSS IDs.

## UI / observability

Production wave data питает HUD phase state и development timeline inspector; отсутствующие display labels/phase mappings валидируются.

## Acceptance criteria

Каждый production field имеет валидный full-run encounter config; pressure/rest rhythm читается из data; final boss appears at configured late-run time; no missing refs.

## Проверки

Accelerated timeline tests, full 15-minute smoke for representative fields, config validation.

## Out of scope

Самостоятельное придумывание schedule coding AI. Пока раздел пуст, модуль не должен молча изобретать production waves.
