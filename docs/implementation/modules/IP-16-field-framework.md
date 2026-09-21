# IP-16 — Field definitions, selection и run configuration

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-02](IP-02-player-movement.md), [IP-12](IP-12-character-framework.md), [IP-14](IP-14-wave-director.md), [IP-15](IP-15-boss-framework.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD/CD «Поля», schema FIELD-001…010; UI §§4–5,23; player-only geometry DECISION-0003; ContentRef contract.

## Scope

selected character/field run configuration; typed environment/enemy/wave/boss/midboss references; field display name/description/thumbnail/difficulty/unlock presentation metadata, source профиля для available IDs. Difficulty 1–5 задаётся данными, а не вычисляется из номера ID. Field-defined Traveler schedule extension point не требует раннего production binding. IP-16 не зависит от IP-29/IP-30 или IP-24: это field framework с optional encounter extension points, а не готовый production timeline.

## Out of Scope

production geometry/картинки/расписания всех полей, придумывание чисел сложности или unlock conditions.

## Acceptance criteria

два fixture fields дают различимую configuration; locked нельзя запустить; Back сохраняет допустимую selection; missing/wrong refs отвергаются до run; player-only obstacles не блокируют enemies/projectiles. Недостающее конкретное difficulty/unlock значение выявлено явно.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

функциональный field selection slice с lock reason, description и difficulty; placeholder thumbnail допустим до image approval, выбранный ID виден в run snapshot.

## Проверки

selection/locked/back, invalid refs, geometry, reinitialization другого field; presenter и PlayMode selection→correct field.

## Документационные изменения

field/run-start contract и metadata units, поле для будущего Traveler binding без circular dependency; IP-23/IP-24/IP-26 Context.

## Gates и недостающие решения

G-14/G-15: конкретные geometry/difficulty/unlock values; fixture metadata отдельно. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-23](IP-23-production-fields.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Boss encounter binding

Field configuration выбирает timeline и encounter definitions согласованно с
[IP-15](IP-15-boss-framework.md#fixture-schema-и-phase-contract): final definition/hook
обязательны, midboss hook optional; каждый используемый hook имеет definition.
BossEncounterRuntime получает fresh WaveDirector/RunModel перед началом run.
Смена field не переносит consumed hooks, phase или boss life предыдущего run.
Fixture boss schema не закрывает production G-14 schedules/rewards.
