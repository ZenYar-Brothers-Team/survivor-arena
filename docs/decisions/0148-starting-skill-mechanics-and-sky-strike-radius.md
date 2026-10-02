# DECISION-0148 — Механики стартовых умений и радиус Небесного удара

Status: Approved (прямое поручение пользователя 2026-10-02)
Date: 2026-10-02
Related IP: IP-06, IP-17
Related content IDs: SKILL-010, CHAR-001…CHAR-010

## Context

Пользователь 2026-10-02: Небесный удар слишком широкий — радиус уменьшить на 20% на старте и на 35% в финальной
стадии, прогрессию подобрать. Стартовые умения персонажей усилить, но не цифрами, а механикой (у Клёпки камень получает
ещё один рикошет, у владельца клинков — ещё один клинок и т.п.). Заодно оценить, насколько силён старт каждого персонажа.

## Decision

### SKILL-010: радиус

Основной impact radius L1–L6: `0.64 / 1.0 / 1.17 / 1.17 / 1.17 / 1.17` вместо `0.8 / 1.3 / 1.8 / 1.8 / 1.8 / 1.8`
(L1 −20%, L2 −23%, L3–L6 −35%). Форма прогрессии DECISION-0079 сохранена: рост на L1→L2→L3, дальше radius не растёт.
Третий удар L6 остаётся ×1.35: `1.17 × 1.35 = 1.58` (было 2.43). Damage, cooldown, targeting radius, число ударов не менялись.

### Механика стартового умения

`startingSkillBoost` персонажа получает необязательные механические каналы (те же, что у наборов, `SkillMechanicBonus`):
`extraProjectiles`, `extraPierce`, `extraChainTargets`, `extraRicochets`, `extraMines`, `extraStrikes` (+`extraStrikeDelaySeconds`),
`extraProjectileSpreadDegrees`. Действуют только на стартовое умение, с первого уровня, складываются с механиками сетов.

| Персонаж | Умение | Механика |
|---|---|---|
| CHAR-001 Клёпка | SKILL-001 | +1 рикошет |
| CHAR-002 Бугор | SKILL-003 | +1 клинок; damage-бонус +60% → +40% |
| CHAR-003 Шепотка | SKILL-005 | +1 пробивание |
| CHAR-004 Тётка Шмыга | SKILL-009 | +2 одновременные мины |
| CHAR-005 Бабка Искра | SKILL-007 | +1 цель цепи |
| CHAR-006 Гром | SKILL-010 | +1 удар через 0.3 с после последнего |
| CHAR-007 Дед Вертун | SKILL-006 | +1 бумеранг (при одном бумеранге в карточке — веер 20°) |
| CHAR-008 Тётушка Светляк | SKILL-012 | второй луч на втором по близости враге |
| CHAR-009 Иголка | SKILL-013 | +1 пробивание |
| CHAR-010 Старшой Ночка | SKILL-015 | диагональные волны (8) с первого уровня |

Сильнейшим стартам (Бугор, Иголка, Шепотка, Искра) выданы мягкие механики, слабым (Гром, Вертун, Светляк) —
удваивающие. Бугру дополнительно срезан числовой бонус. Остальные характеристики не менялись.

## Consequences

- Content Design: карточка SKILL-010 и карточки CHAR-001…010; authoring — `field001-baseline-v1.json`, `characters-v1.json`,
  вывод генератором в `ProductionActiveSkills.json` / `ProductionCharacters.json`.
- Код: `SkillMechanicBonus` (новые каналы), `CharacterDefinition.StartingSkillMechanic`, `PlayerActiveSkillSetRuntime.SkillMechanics`,
  `SceneActiveSkillEffectExecutor`, `BeamTargetTracker`, `StartingSkillBoostText`.
- Тесты: ProductionActiveSkillCatalogTests, ProductionCharacterCatalogTests, LowTierSetMechanicsTests.
- Это правка баланса без плейтеста: итоговая сила стартов требует проверки в игре.

## Approval

Прямое поручение пользователя в чате 2026-10-02.
