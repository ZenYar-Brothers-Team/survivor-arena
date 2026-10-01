# DECISION-0138 — Low-tier сеты SET-021…035

Status: Approved (пользователь 2026-10-01/02 в чате по GI-13: состав, числа и «начинай писать реализацию»; разблокировка — за золото)
Date: 2026-10-02
Related IP: IP-19 (IP-11 framework), IP-25 (Meta)
Related content IDs: SET-021…SET-035

## Context

GI-13: сетам нужны большее разнообразие и влияние. Все 20 сетов SET-001…020 — средние и поздние
(не раньше L8…L17). Нужен слой ранних слабых сетов, различающихся по типу эффекта.

## Decision

Добавлено 15 low-tier сетов ([sets-low-v1](../balance/sets-low-v1.md), [json](../balance/sets-low-v1.json)):
2 собственные атаки (SET-021 камешек по ходу движения, SET-022 конус по ближайшему врагу с сильным
нокбэком), 3 общих пассивных усиления из одних пассивок (SET-023…025), 10 усилений умений своего рецепта
(SET-026…035, из них пять — точечный бонус одному умению: SET-031…035).

- **Отклонение от «3–6 компонентов»** (Game Design «Сеты», Content Design): у low-tier сетов 2–4 компонента
  и суммарный уровень компонентов 5–6; domain-проверка `SetDefinition` принимает 2–6. SET-001…020 не меняются.
- **Разблокировка за золото:** `condition: access`, цена 150 (атаки) / 100 (остальные); `requiredId` —
  поле, которое должно быть открыто (FIELD-001 открыто с начала; SET-029 требует SKILL-008 и поэтому
  FIELD-002). Наличие компонентов рецепта при покупке не проверяется. 14 из 15 сетов можно купить с первого
  профиля; для них все компоненты рецепта стартовые.
- Ни один low-tier сет не усиливает другой сет; бонусы складываются по DECISION-0021.
- Новые узкие механики: `AreaEffect.arcDegrees` (конус, SET-022), `SkillMechanicBonus.ExtraProjectiles`
  (SET-034), `SlowStrengthBonus` (SET-028, усиливает только уже существующий slow). Пробивание Игл
  (SET-035) использует существующий `ExtraPierce`.
- Иконок пока нет (`iconVisualId` отсутствует); арт добавляется отдельно.
- Цены и числа — стартовые, без плейтеста.

## Consequences

- Content Design: карточки SET-021…035; общий текст раздела «Sets» помечает исключение.
- Код: `AreaEffect`, `EnemyDamageArea.InsideCone`, `SceneActiveSkillEffectExecutor`, `SkillMechanicBonus(+Data)`,
  `SetDefinition`; генератор `scripts/content/progression.py` читает пакет через `LOW_SETS_PACKET`.
- Контент: `ProductionSets.json`, `ProductionSetAttacks.json` (генерация), `MetaEconomy.json` (+15 unlocks).
- Тесты: `ProductionLowSetCatalogTests`, `LowTierSetMechanicsTests`; счётчики каталога/магазина обновлены.
- Проверка пакета: `python -X utf8 docs/balance/validate_sets_low_v1.py`.

## Approval

Пользователь утвердил состав по ходу чата 2026-10-01 («да, всё хорошо сейчас»), поручил обновить документы и
реализовать, и выбрал разблокировку за золото. Цены 150/100 предложены AI и ждут отдельной оценки.
