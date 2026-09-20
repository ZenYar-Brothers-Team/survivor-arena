# IP-21 — Production Final Bosses и Mid-bosses

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-21-production-bosses.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-15](IP-15-boss-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

новые GDD boss/run sections; полные выбранные BOSS-001…010/MIDBOSS-001…010; UI §13; Art Production §§3–4.

## Scope

двадцать encounters с phases/HP/attacks/movement/resistance, body assets и нужными telegraph/projectile/impact references. Утверждённая field correspondence переносится как данные; отсутствующие времена/числа не придумываются.

## Out of Scope

invention boss phase rules, cutscenes, самостоятельные production timings.

## Acceptance criteria

каждая фаза воспроизводит карточку, telegraph соответствует реальному effect timing/geometry; final death никогда не вызывает victory; HP/name корректны; end прекращает phase actions; visual size не меняет gameplay geometry.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

production final-boss bar/name, incoming notification, attack warning; midboss phase feedback по owning encounter; phase/source diagnostics для отчёта IP-31.

## Проверки

per-ID phase transitions, threshold/zero-HP/end, defeat и timer interactions; PlayMode UI cleanup, pool/lifecycle; manual light/dark field contrast.

## Документационные изменения

карточка→phases/assets/tests, missing attack-number gaps, links на authoritative field bindings.

## Gates и недостающие решения

G-14: точные attack timings/phase payload, rewards и required fields каждой карточки. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-23](IP-23-production-fields.md), [IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
