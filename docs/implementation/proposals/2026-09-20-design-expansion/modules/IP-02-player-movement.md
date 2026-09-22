# IP-02 — Перемещение игрока, камера и базовая геометрия

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-02-player-movement.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Сохранить текущие movement/camera/player-only collision contracts. Новые механики не требуют повторно проектировать движение.

## Зависимости

[IP-01](IP-01-run-lifecycle.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD «Управление, бой и выживание», «Поля»; Content Fields schema, FIELD-001…010 для geometry compatibility; DECISION-0001/0003; PlayerMover/camera/geometry tests.

## Scope

Movement input, configurable speed, spawn, field bounds и ordinary obstacles только для player; orthographic follow без задержки и screen offset. Предоставлять authoritative movement/direction новым skills; forced displacement исполняется через согласованный motion boundary IP-05, не через sprite pose.

## Out of Scope

Production layouts, character collider resizing по изображению, camera shake gameplay offset, attack input.

## Acceptance criteria

Направления и скорость корректны; pause/end прекращают движение; camera сохраняет depth и центр player. Enemy/projectile/XP проходят ordinary geometry. Подключение knockback не меняет этот collision contract.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Gameplay fixture, player position/direction и camera observations; отдельный постоянный HUD не нужен.

## Проверки

Сохранить existing movement/bounds/camera tests; cross-check forced displacement в IP-05 и camera presentation в IP-26. Текстовые ссылки сами по себе не требуют повторной полной верификации.

## Документационные изменения

Обновить Context и обратные ссылки на IP-05/IP-08/IP-26. Историческое evidence не объявляет screen shake или knockback реализованными.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-16](IP-16-field-framework.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
