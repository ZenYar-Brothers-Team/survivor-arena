# DECISION-0017 — Combat controls и момент фиксации damage

Status: Approved

Date: 2026-09-20

Related IP: IP-03, IP-05, IP-08, IP-09, IP-11, IP-13, IP-15, IP-29, IP-31

## Context / approval

Пользователю предложены конкретные варианты G-06…G-09. В ответ на запрос выбрать изменения он уточнил, что knockback не должен полностью прерывать движение и dash, и поручил выбрать простой и экономный по вычислениям способ как knockback, так и low-HP damage. Выбрано сложение скоростей: один вектор и таймер на цель, без очереди импульсов и корутин. Остальные пункты предложения сохраняются; оно синхронизировано с этой записью.

## Decision

- Knockback — дополнительная скорость/дистанция поверх обычного движения. Input, steering, dash state и их таймеры продолжают работать. Единственный gameplay writer складывает normal velocity и knockback velocity. При обычном движении вправо 3 wu/s и отбрасывании влево 5 wu/s итоговая скорость — 2 wu/s влево.
- Дистанция = base knockback × outgoing multiplier × (1 − resistance). Смещение равномерное, duration явно задан в JSON при ненулевой дистанции. Новый hit заменяет остаток старого knockback. Нулевое направление не создаёт произвольного смещения. Player сохраняет collision с обычной геометрией; enemy проходит её по DECISION-0003. Заблокированная стеной дистанция не накапливается.
- Slow имеет отдельные source timers (owner life + content ID + effect channel). Повтор того же источника заменяет magnitude и перезапускает duration; эффекты разных источников сохраняются, применяется наиболее сильный. После expiry сильного может снова действовать слабый. Slow меняет только movement speed, не attack cadence или knockback. Pause/end останавливают control time; pool return очищает controls.
- Low-HP coefficient кэшируется при HP/stat changes по утверждённой кривой. В момент активации атаки фиксируется итоговый damage; все её delayed waves, projectiles, mines и повторные ticks сохраняют этот damage. Изменение здоровья влияет на следующую активацию. Это использует существующий immutable ActiveSkillActivation и не требует live owner lookup на hit.
- Low-HP применяется один раз. Производный damage, рассчитанный от уже усиленного damage родителя, не умножается повторно. Самостоятельная новая set/proc-активация получает собственный snapshot. Разрешённые proc chains и set-to-set amplification остаются в scope IP-11.
- Action speed меняет activation cooldown. Внутренние задержки, beam/orbit ticks, travel speed и duration сами по себе не меняются. Size масштабирует projectile/blade hit size, beam width и area/pulse/explosion radius. Range — targeting/search/chain radius, orbit radius, beam length, travel distance или управляющий им lifetime; при range=speed×lifetime меняется только lifetime. Таблица mapping реализуется владельцем IP-08.

## Consequences / checks

Проверить движение+knockback и dash+knockback, overlap/expiry slow, immunity, zero direction, pause/end/reset и player walls. Проверить low HP→cast→heal→delayed hit: старая атака сохраняет damage, следующая использует новый коэффициент. Source/target identity фиксируется до lethal callbacks; фактическая потеря HP отделяется от requested damage и max-HP rescale.

Production timing новых controls остаётся required content data. Fixture duration 0.15 s — техническое тестовое значение, не общий production default.
