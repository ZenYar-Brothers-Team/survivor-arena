# DECISION-0018 — XP producer events и lifetime totals

Status: Proposed

Date: 2026-09-20

Related IP: IP-01, IP-04, IP-06, IP-07, IP-09, IP-31

## Context

Принятый IP-06 требует effective pickup radius, source life, base/awarded XP и результат забега без зависимости от recorder. Прежний drop копировал только базовый radius в scaled collider, а XP progression хранил лишь остаток текущего уровня.

## Implementation record

- Каждое Initialize капли создаёт новый `ExperienceDropIdentity`: drop life, run ID и nullable enemy source life/content. `EnemyExperienceDropSink` получает immutable death snapshot и выдаёт один drop на жизнь. Повторная доставка того же death snapshot не дублирует reward.
- Подбор использует текущий `CharacterStats.PickupRadius` и расстояние между world positions. Размер спрайта и collider игрока не меняют effective radius. Старые и новые drops читают его при следующем tick; trigger callback дополнительно проверяет это же расстояние. Expiry проверяется до автоматического подбора в одном tick, сохраняя прежний порядок таймера.
- `ExperienceAwardEvent` содержит run/drop/source identity, origin Collected/Expired/DevelopmentIntervention и base/awarded amounts. Expired с recovery=0 — полноценное событие с нулевым award. Капля помечена consumed до любых callbacks и возвращается в пул в finally.
- `RunExperienceSnapshot` фиксирует collected base/award, expired base/recovered award и intervention base/award. `RunOutcomeContribution.Experience` остаётся остатком текущего уровня; `ExperienceTotals.TotalAwarded` — lifetime award. `PlayerExperienceRuntime` регистрирует единственный contributor `experience`, снимает регистрацию и очищает active/inactive drops при Shutdown.
- XP transition и lifetime counters фиксируются целиком до LevelUp callbacks. Поэтому завершение забега из callback не захватывает половину одного award. Нормальное начисление нескольких уровней публикует их последовательно. Повторный последний configured threshold вычисляется делением; непредставимый level range отклоняется до изменения состояния.
- DEV add-XP использует явный intervention origin. Runtime producer не зависит от UI или export; IP-31 позднее подписывается на события. Наличие событий не означает, что recorder уже реализован.

## Boundaries / review

Техническая запись реализации принятого IP-06, Proposed для архитектурного ревью. Она не назначает новые XP curve/reward значения и не утверждает missing Book/short-draft policy G-01/G-03. GDD и PASSIVE-006/007/010 уже задают используемые formulas; их product meaning не меняется.

## Checks

Radius до/после spawn и после remove modifier; distinct life на pool reuse; zero/nonzero recovery без pickup multiplier; no double award; multi-threshold carry; immutable terminal totals; stale producer teardown; DEV origin; PlayMode pickup после изменения radius. Результаты запусков — в STATUS.
