# IP-18 — Production Passive Items PASSIVE-001…014

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-18-production-passives.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-09](IP-09-passive-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-28](IP-28-world-pickups.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

новые GDD stats/XP/skills; полные выбранные PASSIVE-001…014 cards; UI §§6–10; Art Production §7. Effects/stacking rules берутся из IP-03/IP-05/IP-09/IP-13 и существующей stat composition.

## Scope

четырнадцать six-level definitions, новые supported channels, icon/label/effect delta. Постоянные world auras и world pickup sprites для passive предмета по умолчанию не производятся.

## Out of Scope

собственная скрытая stat formula в пассивке, новые постоянные world effects, самостоятельный balance redesign.

## Acceptance criteria

каждый уровень даёт указанное изменение; same-key replacement/recompute не накапливает bonus. Action speed UI соответствует формуле, Max HP сохраняет ratio, damage-reduction cap и остальные сохранённые правила проверены. Missing level/число/channel выявляются валидатором.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

6 passive slots, icons/levels и readable changes; соответствующие current stats видны в Pause/Build, dynamic effects — через актуальные snapshots.

## Проверки

per-ID L1…6, stacking/replacement/removal, threshold boundaries, heal/XP/action-speed integration, pause/reset; presenter deltas и manual icon slot readability.

## Документационные изменения

range до 014, stat/effect mapping, numeric completeness и asset/test references.

## Gates и недостающие решения

G-08/G-09/G-10 и значения 14 passives; отсутствие конкретного runtime parameter не заполняется hidden default. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-19](IP-19-production-sets.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
