# Общие контракты IP и производство арта

Принятый план `design-sync-R2`; [DECISION-0015](../decisions/0015-design-sync-r2.md). Полные спецификации зарегистрированы в [modules](modules/). Большинство новых требований включено в существующие IP до дальнейшей реализации. Код не изменён этой миграцией.

Пять исходных design documents утверждены; три replacement bodies уже находятся по каноническим путям. Номера IP сохраняются; единственная рабочая Execution order и статусы — в [STATUS](STATUS.md).

## Общий контракт

- Каждый linked packet является полной действующей спецификацией. Evidence прежнего scope сохранён в STATUS и не является вторым конкурирующим Scope. Реализацию переиспользуем; новые требования и проверки добавляем только там, где есть delta.
- Game/Content/Art target — утверждённые новые документы, ASSET_PIPELINE сохраняет technical authority. Context перечисляет секции и ID; полный catalog не читается для каждой задачи.
- Все entity/tuning values — JSON в общем дереве; required per-kind DTO fields без hidden tuning defaults; validating constructors и NumericValidation. Symbols/units/ranges определены владельцем; отсутствующий production параметр блокирует только зависящую поставку.
- Runtime logic testable вне scene; source contracts не зависят от view/exporter; Shutdown/rollback, pause/end, repeated Initialize, pool reuse и отсутствие stale subscriptions входят в Scope изменяемых систем. Scale-sensitive loops guarded with PerfGuard.
- UI — immutable snapshots и presenter intents, semantic IDs и feature-owned vertical smoke. IP-10A даёт components, feature IP — их смысл и состояния, IP-26 — навигацию/Settings. Lower-level feature не ждёт полного IP-26.
- Чтобы избежать UI dependency cycle, нижние IP публикуют свои snapshots через существующую foundation и тестируют adapters; новые общие rendering features IP-10A используют fake feature snapshots. IP-10A не зависит от полного IP-11/IP-12/IP-29.
- Visual motion — child VisualRoot, один compositor, renderer baseline restored при Shutdown/pool return; gameplay knockback/control/trajectories не подменяются анимацией.
- DEV gated/collapsed/bounded ≤25% width и 45% height при 1920×1080, tabs/scroll. Телеметрия — optional consumer. Обязательные player Results используют RunOutcome и producers даже при disabled recorder.
- Tests проверяют behavior/negative cases, не зеркалят constructors. EditMode/PlayMode через безопасный smoke-check, не batch поверх открытого Editor. Manual visual evidence отдельно от import validation. Regression/debt registers меняются только по их правилам.
- После реализации owning specification/evidence в STATUS и готовность потребителей синхронизируются. Historical Verified не является target verification; правила ревизий описаны в [WORKFLOW](WORKFLOW.md).

## Карта всех 30 существующих IP

| Целевая спецификация | Изменение на месте |
|---|---|
| [IP-00](modules/IP-00-content-contract.md) | Context/references; общий registry сохраняется. |
| [IP-01](modules/IP-01-run-lifecycle.md) | Run identity/terminal snapshot/RunOutcome, reset contract без dev recorder. |
| [IP-02](modules/IP-02-player-movement.md) | Сохранить movement/camera/geometry; проверить совместимость IP-05, не проектировать заново. |
| [IP-03](modules/IP-03-character-stats.md) | Новые stats, action-speed terminology, validated channels и low-HP formula. |
| [IP-04](modules/IP-04-enemy-core.md) | Per-life identity, immutable death/despawn payload и category contract. |
| [IP-05](modules/IP-05-active-skill-runtime.md) | Source/applied damage + target adapter + authoritative knockback/slow. |
| [IP-06](modules/IP-06-xp-progression.md) | Effective XP pickup radius, включая существующие drops, accurate XP events. |
| [IP-07](modules/IP-07-level-up-draft.md) | 3 slots, LevelUp/Book request queue и provider extension point. |
| [IP-08](modules/IP-08-active-skill-framework.md) | Новые targeting/effect families, cumulative levels, boomerang ledger/deceleration. |
| [IP-09](modules/IP-09-passive-framework.md) | Mappings 14 passives, new channels, dynamic modifiers и applicability. |
| [IP-10](modules/IP-10-reroll-banish.md) | Origin-aware reroll/banish, Banish mode/cancel и queue-safe counters. |
| [IP-10A](modules/IP-10A-ui-foundation.md) | Reusable cards, icon/description metadata, elapsed HUD, Pause/Build и states. |
| [IP-11](modules/IP-11-set-framework.md) | Global priority set selection, recipes 3–6, transforms/buffs/procs/set-attacks. |
| [IP-12](modules/IP-12-character-framework.md) | New stat/metadata mapping, baseline-relative selection, locked presentation. |
| [IP-12A](modules/IP-12A-visual-presentation-foundation.md) | Category import profiles, role adapters, fixture kit и reconciled art inventory. |
| [IP-13](modules/IP-13-enemy-patterns.md) | Control/dash/slow integration и enemy projectile origin. |
| [IP-14](modules/IP-14-wave-director.md) | Continuous + burst timeline, cap/catch-up policy и actual spawn diagnostics. |
| [IP-15](modules/IP-15-boss-framework.md) | Полный target boss framework, target adapter и final-boss UI. |
| [IP-16](modules/IP-16-field-framework.md) | Field/run selection, metadata/difficulty и optional encounter extension points. |
| [IP-17](modules/IP-17-production-skills.md) | 16 skills: complete definitions L1…6, world roles и 16 icons. |
| [IP-18](modules/IP-18-production-passives.md) | 14 passives: complete values/mappings,14 icons и potion integration. |
| [IP-19](modules/IP-19-production-sets.md) | 20 sets: concrete recipes/effects,20 icons и restrained VFX. |
| [IP-20](modules/IP-20-production-enemies.md) | 20 enemies: required combat/drop fields, production bodies; PICKUP-001 и его art/drop bindings. |
| [IP-21](modules/IP-21-production-bosses.md) | 10 bosses+10 midbosses: phases/control/telegraphs и 20 bodies. |
| [IP-22](modules/IP-22-production-characters.md) | 10 characters: new roster/loadout/weights, body/crops и selection. |
| [IP-23](modules/IP-23-production-fields.md) | 10 fields: geometry/environment/metadata, kits и thumbnails. |
| [IP-24](modules/IP-24-production-waves.md) | Complete continuous/burst field schedules и Traveler settings/bindings. |
| [IP-25](modules/IP-25-meta-progression.md) | Persistent profile/economy/unlocks, idempotent result application и UI. |
| [IP-26](modules/IP-26-functional-ui.md) | Full navigation/results/retry плюс basic working Settings. |
| [IP-27](modules/IP-27-integration.md) | Updated system/content-complete regression, real run/readability/balance evidence. |

Scope меняется у 28 существующих packets; IP-00/IP-02 сохраняют исходные behavioral contracts и получают актуальный Context/consumer links. Совместимость их evidence с изменениями APIs проверяется при регистрации и реализации, а не предполагается по номеру модуля.

## Почему нет отдельных «догоняющих» IP

- Новая set selection принадлежит IP-11. IP-07 предоставляет request queue/provider contract и не начинает зависеть от своего потребителя IP-11.
- Новые skill mechanics входят в IP-08 до production IP-17; burst входит в IP-14 до boss/field/encounter integration.
- Stats IP-03 и mappings IP-09 отделены от применения damage/control IP-05. Enemy integration IP-13 использует общий contract; production cards не создают собственные combat rules.
- IP-12A расширяет уже существующий pipeline; каждый production owner поставляет собственные per-ID assets. Новый generic pipeline после всех production IP не создаётся.
- Settings входят в обновлённый IP-26 вместе с нужными минимальными services. Out of Scope final soundtrack не превращает volume sliders в декоративные элементы.
- Только world pickups, Traveler framework/content и локальный playtest/balance процесс получили пять новых IDs [IP-28…32](README.md).

## Asset production: ownership и размер inventory

Утверждённый Art Production задаёт целевой inventory; он не утверждает ещё не созданные изображения. Ни одна строка `NOT STARTED` не отменяет уже существующую foundation. Asset lifecycle ведётся по **owner ID + role**, а исполнение IP — только в STATUS. Для hybrid строки image и procedural integration имеют отдельное evidence.

| Owner / диапазон | Минимальный объём / роли | Владелец поставки | Особенности |
|---|---|---|---|
| CHAR-001…010 | 10 body; selection image как derivative/reuse | IP-22; support IP-12A | CHAR-001 concept уже заявлен approved; сопоставить фактические files/provenance. Не считать fixture автоматически его production body |
| ENEMY-001…020 | 20 body | IP-20 | Shared arrow/bolt/stone/magic/holy families вместо уникального projectile каждого врага |
| BOSS-001…010 | 10 body + необходимые attack roles | IP-21 | Размер/PPU/pivot и memory/readability проверяются отдельно |
| MIDBOSS-001…010 | 10 body + необходимые attack roles | IP-21 | Reuse VFX families допустим по явному binding |
| TRAVELER-001…010 | 10 body + нужные projectile/support roles | IP-30; support IP-29/IP-12A | Все имеют procedural HP bar; off-screen arrow не требует raster |
| SKILL-001…016 | 16 icons + необходимые projectile/mine/beam/telegraph/impact roles | IP-17 | Механика IP-08/IP-03/IP-05/IP-09/IP-13; motion/return/orbit/stretch/timing не требуют кадровых наборов |
| PASSIVE-001…014 | 14 icons | IP-18 | Нет обязательных world sprites или постоянных auras |
| SET-001…020 | 20 icons + только необходимые accent/replacement/proc roles | IP-19 | Большинство переиспользуют active/generic VFX; large bolt/sphere/ice/boulder отдельно только по карточке |
| FIELD-001…010 | 10 field kits и 10 thumbnails/derivatives | IP-23 | Ground/background + реально нужные decor/obstacle roles; thumbnail можно получать из field art |
| XP pickup | World sprite, optional variants только при пользе | IP-12A integration с IP-06 | Existing XP gameplay сохраняется; не создавать 3 варианта по умолчанию |
| PICKUP-001 healing potion; Traveler Book | World sprites + optional collect feedback | IP-20 — potion; IP-30 — Book; framework IP-28 | Book card/ID заполняется до production binding; ID не придумывается в art filename |
| Meta currency / upgrades | Currency icon и icons реально реализованных upgrades | IP-25 | Нет каталога картинок под неописанную economy; world currency art только если нужен gameplay |
| Generic VFX | Hit/impact/slash/explosion/lightning/ice/heal/level-up/set/death/slow/trail/beam/aura/telegraph | IP-12A reusable support + owning gameplay IP | Каждая роль Generate/Procedural/Hybrid; owner trigger и время authoritative. Shared asset имеет один manifest record и несколько consumers |
| Procedural-only | Bob/lean/squash/flip/hit feedback, rotation/travel/orbit/return/stretch/pulse/fade, UI bars/cards/arrow, screen shake/damage text по scope | IP-12A + owning runtime; UI IP-10A/IP-26/feature slices | Не дублировать сделанный player animator; death/lifetime policy и actual gameplay movement не подменяются visual animation |

Итого: **60 body entries**, **50 skill/passive/set icons**, **10 field kits + 10 thumbnails**. Это не фиксированное число уникальных PNG: crop/reuse/procedural меняют file count, а shared impacts/проекции добавляются по потребности. Portraits, optional pickup variants и marketing art не входят в обязательные численные итоги.

Обычные buttons/panels/borders, HP/XP/boss/Traveler bars, timer/level, DraftCard background, recipe markers/details, notifications, lock/hover/selected overlays, difficulty marks и settings controls сначала создаются UI Toolkit shapes/text. Reroll/Banish не требуют отдельных generated icons. Art Production дополняет `ASSET_PIPELINE.md`, а не заменяет его paths/import/GUID/provenance contract.

## Per-ID production stages и gates

Для каждого owner+role manifest хранит: content/visual ID, category/role, Generate/Procedural/Hybrid, brief/source/reference paths, runtime resource path, reuse consumers, owning IP, missing dependency, proof каждого stage и конкретные approval records. Не заменять этим список execution statuses.

| Stage | Условие перехода / evidence | Что не подразумевается |
|---|---|---|
| Brief ready | ID/role известны; выбранный playable scope; content requirements и нужные runtime contracts полны; Art Direction и category checklist указаны | Полная реализация всего каталога не нужна; отсутствующие gameplay числа не выдумываются из картинки |
| Generated / Review | Preview вне Assets, reference/provenance и назначение понятны; показывается пользователю | Preview не является runtime asset |
| Image approved | Пользователь выбрал конкретный вариант; фиксируются source/version и вид approval | Approval пяти документов не равен approval изображения; concept approval не доказывает integration |
| Prepared | `Art/Source/.../selected-master.png` + заполненный asset record; documented alpha/padding/sRGB/downscale; runtime derivative без изменения дизайна | Design-changing edit возвращает visual review; UI crop также проверяется в своём slot |
| Imported | Canonical `Assets/Resources/Art/...` path, Unity-generated `.meta`, role-specific import/pivot, typed visual reference resolved | Наличие PNG не доказывает gameplay use; automatic fallback не маскирует production missing asset |
| Integrated | Реальный owning scene/factory/screen использует asset; VisualRoot/adapter/pool reset; approved replacement сохраняет path/GUID | Ни collider, ни damage/attack timing не подгоняются к рисунку |
| Verified in game | Technical checks + target-scale/manual density review; пользователь принял visual/motion по Gate E; evidence связано с owning IP | Валидатор alpha/import не оценивает силуэт и ощущение движения |

Соответствие existing pipeline: Brief = Gate A, Image approved = B, Prepared = C, Imported = D, gameplay acceptance = E. Gates не одобряют друг друга автоматически. Для pure procedural элементов нет искусственного image approval/master PNG: проверяются implementation, config, pause/reset и manual presentation evidence. Для hybrid проверяются обе ветки.

### Phase A — первый визуально собранный playable scope

1. Выбрать небольшой список **ID и roles**, которые доступны в текущем playable build; зафиксировать весь remainder catalog отдельно. Scope choice определяет порядок, а не повторно утверждает уже принятый дизайн.
2. Reconcile существующий `FIXTURE-CHARACTER-AGILE` body/provenance и CHAR-001 concept claim. Проверить возможность reuse, различия card/образа, production reference и selection crop; сохранить честное имя каждого owner. В runtime tree сейчас найден только fixture body; отдельную shadow/crop роль нельзя объявить готовой по наличию renderer.
3. Закрыть необходимые IP-12A category adapters/validation и только нужные enemy/skill world roles. Не вводить character rig для каждого UI icon/projectile ради единообразия.
4. XP + potion/Book art подключать к работающим соответствующим systems; один выбранный field kit; минимальные hit/death/heal/level-up effects по существующим событиям; icons доступного в этом scope build content.
5. Per-ID stages до actual gameplay acceptance, затем совместный UI/density check. Один визуальный pilot не завершает весь production IP-17…IP-23 и IP-30.

### Phase B — расширение доступного playable content

Для каждого следующего ID: актуализировать manifest → проверить completeness/mechanics dependency → reusable asset choice → generate только недостающие roles → image review → prepare/import → procedural integration → automated + target-scale checks → actual `IN GAME` evidence. Добавлять его icons/crops/recipe presentation в тот же vertical slice. Если новый ID использует уже готовый shared VFX, проверяется binding и combined readability, а не создаётся новая картинка.

### Phase C — оставшийся каталог и совместная читаемость

Постепенно закрыть оставшиеся owner IDs после стабилизации gameplay/balance loop и заполнения чисел. Проверить full sets (включая 3–4 simultaneous), late-game enemies на светлых полях, boss/Traveler overlap, полный HUD/build и максимальную запланированную плотность. Atlas/compression/memory profile вводить по измерению и before/after review; не создавать один global atlas всего. Marketing/store art остаётся отдельным поздним scope.


## Проверяемое repository evidence

Existing UI/presentation подтверждаются STATUS и Game.UI/Game.Presentation; они не доказывают готовность target UI/catalog. DraftOptionViewState пока содержит Id/Title/Detail; recipe progress находится в DEV; importer обслуживает Art/Sprites единым body profile; enemy/projectile/XP renderers требуют подходящих adapters. Это конкретные дельты IP-07/IP-10A/IP-11/IP-12A, а не причина полностью переписывать foundations.

В runtime tree найден fixture body, source/master/provenance существуют. Связь approved CHAR-001 concept с fixture master v002 подтверждена пользователем в DECISION-0029 и provenance; production binding остаётся IP-22. Текущий проверяемый asset inventory — Art/asset-manifest.json. Asset lifecycle живёт в inventory, execution status модулей — только STATUS.

Применяются .claude gameplay/content/UI/foundation/Unity-test rules и helpers design-review, consistency-check, content-audit, asset-audit, perf-audit, test-quality-review, smoke-check. Hook проверяет только JSON syntax в своём клиенте, не domain/ref validity. Регистрация плана изменила документацию и разрешённые repository rules; runtime/config/assets остались без изменений.
