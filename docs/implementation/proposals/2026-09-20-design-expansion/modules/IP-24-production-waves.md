# IP-24 — Canonical Wave / Encounter Content и field bindings

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-24-production-waves.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-14](IP-14-wave-director.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-23](IP-23-production-fields.md), [IP-29](IP-29-traveler-framework.md), [IP-30](IP-30-production-travelers.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

новые GDD run/waves/fields/Travelers; Content Wave / Encounter Content и selected FIELD/ENEMY/BOSS/MIDBOSS/TRAVELER cards; IP-14 burst и IP-29 Traveler contracts; DECISION-0014.

## Scope

15-minute field schedules: continuous/burst composition, pressure/rest phases, rates/counts/caps, final/midboss timings и Traveler schedule/scaling. Regular-wave и Traveler policies остаются раздельно видимыми. Балансировочное предложение создаёт IP-32; в production попадает его конкретно принятый вариант.

## Out of Scope

молчаливое заполнение пустых schedules, adaptive difficulty/автоматический AI rebalance, фиксация FPS-target без измерения.

## Acceptance criteria

каждый in-scope field имеет законченный, валидируемый encounter config; невозможно объявить каталог завершённым по одному пилотному полю. Pressure/rest, burst и Traveler events исполняются по данным; final boss появляется к заданному времени; пауза не сдвигает run-time schedule; capped/omitted spawns диагностируются.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

phase/timer HUD, boss/Traveler vertical slices, DEV timeline со scheduled и actual events; IP-31 получает поле/phase/source и фактическую нагрузку.

## Проверки

accelerated boundary timeline, same-time events/caps/catch-up, reference/number validation; реальный 15-minute run representative fields и per-field targeted smoke; repeated seed comparison без обещания deterministic physics replay.

## Документационные изменения

versioned encounter data и rationale принятого баланса, field bindings, run evidence; отделить proposal чисел от утверждённого config.

## Gates и недостающие решения

CG-02/G-11/G-14/W-01: full per-field encounter/scaling packets; пустой Wave section не разрешает coding AI придумать канон. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
