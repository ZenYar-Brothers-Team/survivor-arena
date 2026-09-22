# DECISION-0025 — Set effect source propagation and ownership

Status: Proposed

Date: 2026-09-21

Related IP: IP-05, IP-07, IP-08, IP-10, IP-10A, IP-11, IP-19, IP-28

## Context

GDD «Сеты» уже запрещает рекурсивные set-to-set procs, задаёт fixed cooldown самостоятельных атак и additive bonuses к обычным skill stats. IP-11 должен связать эти правила с existing combat executor, без обратной зависимости Progression → ActiveSkill.

## Implementation record

Progression владеет `SetEffectDefinition`, `SetEffectAbility`, `ISetEffectHost` и жизненным циклом acquired sets. ActiveSkill предоставляет `SetEffectHost`, Bootstrap связывает его с player/XP/skills и освобождает после draft, до XP/player. `SetRewardEvent` — typed boundary для fake reward; реальный potion adapter остаётся IP-28/IP-19.

Обычный `ActiveSkillInstance` публикует одну activation через runtime, после schedule всего activation packet. Delayed waves/projectiles не публикуют дополнительные activations. Set attack получает `CombatSourceOrigin.Set`, owner identity, content ID сета и отсутствующий skill level. Этот snapshot сохраняется во всех scheduled effects и damage requests. Set counters отвергают Set/SecondaryProc; reward proc также не принимает set-created reward source. Cooldown коммитится до callback, предотвращая reentrant повтор.

Skill transforms используют keys `set:<id>:<effect-index>` и additive damage/action-speed/size/range/knockback channels для указанного обычного skill. Generic damage и outgoing knockback применяются к set attacks; global size/range и action speed не переносятся туда автоматически. Таймеры принадлежат set ability. Attack templates используют L1 существующего fixture effect definition, не добавляются в build и не получают transforms других сетов.

Каждый set effect владеет своим executor/pools; Dispose очищает scheduled attacks/projectiles, ledger, подписки и только собственные modifiers. Частичная инициализация и пакет Synchronize откатывают добавленные эффекты. На terminal state ability очищается; pause замораживает таймеры. Level-heal допускает draft pause, возникшую из того же XP award, но отвергает terminal run.

## Consequences and limits

Это техническая межслойная запись для architecture review, а не новое product rule или approval чисел. GDD/CD не изменены. Fixtures подтверждают семейства; точные production payloads, return/disc и trash-explosion conflicts, thresholds, visual budgets и potion event binding остаются у IP-19/IP-28. Не объявляется готовой production-реализация двадцати сетов.

Fixture OVERCHARGE/RECALL сохраняют non-production ID, но получают настоящие эффекты вместо tick counters, третий компонент и описание. Снимки прежней JSON-конфигурации в telemetry не переинтерпретируются; content hash различает конфигурации. Старые tick-only types остаются test doubles для изолированных draft/telemetry tests; composition root их больше не использует.

## Teardown observation

Повторный PlayMode run воспроизвёл nondeterministic scene unload: Unity вызвала PlayerCharacterRuntime.OnDestroy раньше root.OnDisable, несмотря на execution order. Новый `ShuttingDown` event сообщает composition owner до очистки Health/Stats; guard установлен до callback, поэтому обратный Shutdown не рекурсивен. Root снимает подписку и завершает consumers в существующем порядке. PlayMode smoke с четырьмя сетами теперь явно вызывает producer-first Shutdown и проверяет освобождение UI/root/sets. Это исправление необходимого teardown contract, не изменение gameplay.
