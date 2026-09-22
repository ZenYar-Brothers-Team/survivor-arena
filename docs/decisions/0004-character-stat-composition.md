# DECISION-0004 — Композиция процентных характеристик

Status: Approved

Date: 2026-09-14

Related IP: IP-03, IP-09, IP-12, IP-18, IP-22, IP-25

Related content IDs: PASSIVE-001…010 и CHAR-001…010 как Draft compatibility targets

## Context

Перед IP-09 требовалось определить единый порядок пересчёта характеристик. Без этого пассивные предметы, персонажи, сеты и мета-прогрессия могли бы применять одинаковые проценты по разным формулам.

## Decision

- Процентные бонусы разных источников одной характеристики складываются.
- Обычный повышающий процент применяется к базе как `base × (1 + сумма бонусов)`.
- Cooldown reduction хранится положительным процентом и применяется к длительности как `base cooldown duration / (1 + сумма reduction)`. Жёсткого минимального cooldown нет, но положительная базовая длительность не может стать нулевой.
- Incoming damage reduction хранится положительным процентом, суммируется и ограничивается 99%; итоговый множитель входящего урона не ниже `0.01`.
- При изменении max HP текущее здоровье сохраняет прежнюю долю от максимума: `new current HP = old health ratio × new max HP`.

## Consequences

- Все runtime-источники используют одну order-independent формулу и keyed replacement без double counting.
- Character base values остаются базой; пассивы, сеты и мета-источники добавляют собственные проценты.
- IP-09 обязан проверить stacking, замену уровня, удаление источника, cooldown asymptote, 99% damage-reduction cap и пропорциональный max-HP rescale.

## Approval

Пользователь явно утвердил правила 2026-09-14: проценты между источниками складываются, здоровье при росте максимума меняется пропорционально, cooldown уменьшается множителем без достижения нуля, damage reduction ограничен 99%.

## Дополнение 2026-09-20

Каноническая терминология теперь action speed; формула сохраняется. Draft compatibility targets этой исторической записи не являются текущим статусом новых утверждённых карточек. См. [DECISION-0015](0015-design-sync-r2.md); прежние decision/approval сохранены.

API/fixture JSON используют `ActionSpeedBonus` / `actionSpeedBonus`; output `ActiveSkillCooldownMultiplier` по-прежнему означает множитель длительности. Словарь новых каналов, low-HP curve и их consumers записан в [IP-03](../implementation/modules/IP-03-character-stats.md). G-08/G-09 остаются открытыми до решения applicability/hit-time policy; наличие канала само по себе не применяет его ко всем effect families.
