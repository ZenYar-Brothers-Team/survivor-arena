# Решения перед IP-05: G-06 / G-07 / G-08 / G-09

Дата: 2026-09-20. Варианты приняты с поправками пользователя о непрерывности движения/dash и выборе простого low-HP расчёта. Итог зафиксирован в [DECISION-0017](../../decisions/0017-combat-control-semantics.md), GDD и PASSIVE-014; evidence реализации хранится в STATUS.

## G-06 — Повторный slow

Принят отдельный таймер для каждого источника (владелец + content ID + канал эффекта). Осколки одного залпа не создают независимые источники. Повторное попадание того же источника заменяет его magnitude текущим значением и перезапускает duration от нового попадания; длительности не складываются.

Между источниками действует максимум magnitude. Таймеры слабых эффектов продолжаются; после истечения сильного ещё действующий слабый снова определяет скорость. Pause/end не продвигают время. Slow не меняет attack cadence и дистанцию/длительность knockback.

Пример: A даёт 20% на 3 с в t=0; B даёт 40% на 1 с в t=1. В t=1…2 действует 40%, в t=2…3 — 20%. Повтор A в t=2.5 сохраняет его до t=5.5.

## G-07 — Исполнение knockback

Принято:

- Равномерное принудительное смещение за отдельную длительность из JSON. Дистанция = base × outgoing × (1 − resistance), как уже утверждено.
- Обычное управление/AI movement и dash не прерываются: их velocity складывается с knockback velocity. Их таймеры и attack cadence продолжаются. Выбран один дополнительный вектор/таймер на цель как простая реализация.
- Новый knockback заменяет незавершённый остаток предыдущего своим направлением, дистанцией и duration. Без накопления knockback-векторов и очереди.
- Player упирается в обычные препятствия; недостигнутая дистанция не переносится на потом. Enemy проходит обычную field geometry по DECISION-0003.
- Используется явное направление атаки; для радиального эффекта — origin→target. При нулевом направлении displacement равен нулю, искусственное направление не добавляется.
- Duration обязателен в content/config для ненулевого knockback. Для технических fixtures используется 0.15 с; production tuning — в owning content packet.

## G-08 — Применимость параметров

Принятая таблица для IP-05/08/11/13:

| Канал | Применяется | Не применяется автоматически |
|---|---|---|
| Action speed | cooldown между активациями active skill | travel speed, effect duration, задержки внутри одной активации, beam/orbit damage tick interval |
| Effect size | projectile/blade hit size, beam width, area/pulse/explosion radius | targeting/search radius, orbit radius, beam length, travel distance/lifetime |
| Effect range | targeting/search/chain radius, orbit radius, beam length, travel distance или определяющий её lifetime | beam width, area radius, projectile/blade size, duration неподвижного эффекта |

Для range = speed × lifetime увеличивается только lifetime. Если дальность ограничена distance, увеличивается она; время замедления не получает второй scale. Effect должен назвать один управляющий параметр; отсутствующий параметр не придумывается ради бонуса. Set modifiers получают отдельный явный mapping IP-11 без повторного применения общего scale.

## G-09 — Low-HP damage во времени

Принят **snapshot при активации**: кэшируемый HP/stat-dependent coefficient применяется один раз при создании атаки. Все её delayed waves, projectiles, mines и ticks сохраняют итоговый damage. Новое HP влияет на следующую активацию. Immutable source identity остаётся валидным после исчезновения владельца; live lookup на каждом hit не требуется.

Multiplier применяется один раз ко всему damage персонажа, включая set/secondary damage. Копирование уже усиленной damage value в proc не применяет его повторно. Это не разрешает новые proc chains/set-to-set amplification: связи определяет IP-11.

## Исполнение

G-06…G-09 закрыты решением пользователя. IP-05 владеет общими controls/source/damage boundary; IP-08 реализует mapping параметров эффектов, IP-11 — разрешённые set interactions. Production tuning остаётся required content data.
