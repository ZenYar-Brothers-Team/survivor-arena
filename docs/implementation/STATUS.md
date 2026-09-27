# Implementation Status

Единственный источник execution status и Execution order. Навигация по коду/данным: [PROJECT_MAP](../PROJECT_MAP.md).

Plan revision: design-sync-R2; startup packets: field-001-start-R1.
Current active packet: нет; REPO-01 завершён в разрешённом scope.
Next Ready packet: нет; F1-09/F2-06 и каталоги сохраняют ручные/data/art gates ниже.
Последний общий Unity smoke: 2026-09-27, 857/857 EditMode + 30/30 PlayMode; generation/audio integrity и manifest 188 PASS; [enemy projectile art evidence](evidence/2026-09-27-enemy-projectile-art.md). Ручную приёмку эти проверки не заменяют.

## Действующие границы

- Дизайн `design-sync-R2` и 121 исходная карточка утверждены (DECISION-0015); оставшиеся TBD и новые proposals не получают approval автоматически.
- FIELD-001: baseline и F1-00…08 выполнены; F1-09 ждёт ручной матрицы, performance bounds и пользовательской приёмки. Пользователь 2026-09-26 разрешил идти дальше, не закрывая эту приёмку. Автоматического перехода через data/art/manual gates нет.
- FIELD-002: F2-01…05 поставлены; F2-06 ждёт ручного прогона. IP-12A gameplay density review остаётся отдельным открытым gate.
- Поздние каталоги и поля сохраняют свои prerequisites/остатки в записях IP. Ни approval арта, ни пройденные автоматические тесты не заменяют gameplay-scale review.
- IP-33 разрешён отдельным поручением вне F1-09; прослушивание остаётся открытым. REPO-01 разрешает только предложенный структурный рефакторинг и его проверки, без изменения баланса и без запуска следующего IP.
- История поручений и оснований: [датированный архив](evidence/2026-09-27-execution-history.md). При выборе работы читать эту шапку, очередь и нужные записи; архив — только при необходимости.

## Execution order

При разрешении на исполнение выбирать первый Ready packet активного этапа ниже,
если пользователь не назвал другой scope. Пока этап активен, поздний backlog
автоматически не выбирать. Порядок IP после этапа сохранён во второй таблице.

<a id="field001-execution"></a>
### FIELD-001 initial slice — приоритетная очередь

Все packets относятся к `field-001-start-R1`. Status ниже относится к packet,
а не к полному каталожному IP. Успех стартового поднабора не закрывает весь каталог.

| Приоритет | Packet / владельцы | Status | Prerequisites / конкретный gate |
|---:|---|---|---|
| 1 | [F1-00 — полные данные](milestones/FIELD-001-start.md#f1-00); IP-17…26/30/32 | Verified | 2026-09-24: baseline v1 Approved (DECISION-0053), canon синхронизирован; static validator PASS; [evidence](evidence/field001-baseline-v1-2026-09-23.md#approval-2026-09-24) |
| 2 | [F1-01 — 10 skills](milestones/FIELD-001-start.md#f1-01); IP-17 | Verified | SKILL-001…007/010/013/014 (L1–6, art/VFX); Unity 709/709 + 26/26, 2026-09-24. [Evidence](evidence/field001-f1-01-2026-09-24.md), [общий прогон](evidence/field001-f1-08-2026-09-24.md#unity-full-pass) |
| 3 | [F1-02 — 10 passives](milestones/FIELD-001-start.md#f1-02); IP-18 | Verified | 2026-09-24: completed IDs PASSIVE-001…005/007…009/011/012 (L1–6, icons); Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-02-2026-09-24.md) |
| 4 | [F1-03 — Клёпка/profile/UI](milestones/FIELD-001-start.md#f1-03); IP-22/25/26 | Verified | 2026-09-24: CHAR-001 production definition/visual binding, MetaEconomy по DECISION-0050, миграция при загрузке; Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-03-2026-09-24.md) |
| 5 | [F1-04 — enemies/potion](milestones/FIELD-001-start.md#f1-04); IP-20 | Verified | ENEMY-001…005/007 + PICKUP-001; тела ENEMY-003/004/005/007 подключены, Unity 709/709 + 26/26; текущий вид принят пользователем 2026-09-24. [Art review](../playtests/2026-09-24_field001-art-acceptance.md), [art evidence](evidence/field001-art-integration-2026-09-24.md), [packet evidence](evidence/field001-f1-04-2026-09-24.md) |
| 6 | [F1-05 — 5 sets](milestones/FIELD-001-start.md#f1-05); IP-19 | Verified | 2026-09-24: SET-001/004/006/010/017 (пороги, эффекты, SET-017 attack/telegraph); Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-05-2026-09-24.md) |
| 7 | [F1-06 — boss/mid-boss](milestones/FIELD-001-start.md#f1-06); IP-21 | Verified | BOSS-001/MIDBOSS-001; тела и общий снаряд веера/кольца подключены, Unity 709/709 + 26/26; текущий вид принят пользователем 2026-09-24. [Art review](../playtests/2026-09-24_field001-art-acceptance.md), [art evidence](evidence/field001-art-integration-2026-09-24.md), [packet evidence](evidence/field001-f1-06-2026-09-24.md) |
| 8 | [F1-07 — 3 Travelers/Book](milestones/FIELD-001-start.md#f1-07); IP-30 | Verified | TRAVELER-001/002/005 + FIELD-001 schedule, PICKUP-002; три тела подключены, Unity 709/709 + 26/26; текущий вид принят пользователем 2026-09-24. [Art review](../playtests/2026-09-24_field001-art-acceptance.md), [art evidence](evidence/field001-art-integration-2026-09-24.md), [packet evidence](evidence/field001-f1-07-2026-09-24.md) |
| 9 | [F1-08 — production field/run](milestones/FIELD-001-start.md#f1-08); IP-23/24/25/26 | Verified | 2026-09-24: FIELD-001 (поле, 900-s timeline, 64 authored player-only obstacles), production composition без fixture fallback, production профиль `profile-v1.json`; Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-08-2026-09-24.md), [DECISION-0054 §9](../decisions/0054-field001-autonomous-execution.md#9-конкретизации-f1-08) |
| 10 | [F1-09 — доведение/приёмка](milestones/FIELD-001-start.md#f1-09); IP-27/12A/31/32 | Blocked | F1-00…08 проверены; пользователь разрешил дальнейшие отдельные работы 2026-09-26, но приёмку F1-09 не закрыл. Нужны реальные прогоны по матрице, performance bounds и приёмка ощущения карты. Последние правки/проверки — ниже; [матрица](evidence/field001-f1-09-2026-09-24.md), [история исправлений](evidence/2026-09-27-execution-history.md#field-001-follow-ups) |

### Пользовательские правки FIELD-001 — 2026-09-27

По [DECISION-0069](../decisions/0069-field001-feedback-tuning.md) заменён и приглушён level-up cue, ослаблены damage/range/size SKILL-006, смягчена оранжевая вспышка SKILL-014, поле уплотнено до 288 препятствий четырёх типов по миниатюре, HUD показывает обратный отсчёт 15:00 → 00:00. Runtime, исходные данные генератора, арт-пакет и документы синхронизированы. EditMode 855/855 и PlayMode 30/30 PASS; art scope 50/50 и 151 provenance record PASS; audio integrity 28/28, layouts 200 сидов PASS. Художественный review и игровой баланс после изменений ждут пользовательского прогона. [Evidence](evidence/2026-09-27-field001-feedback-tuning.md).

Дополнительный отзыв 2026-09-27: на стартовом экране FIELD-001 гарантированы два видимых объекта вне свободного круга радиуса 6 ([DECISION-0070](../decisions/0070-field001-opening-screen-obstacles.md)); SKILL-007 L1–L3 слегка ослаблен до 22/22/27.5 damage ([DECISION-0071](../decisions/0071-early-chain-lightning-damage.md)). Safe full check: EditMode 856/856, PlayMode 30/30, art provenance 151/151 PASS; layout validator 200 сидов PASS. Игровой баланс ждёт ручного прогона. [Evidence](evidence/2026-09-27-opening-screen-and-lightning.md).

При завершении добавлять сюда completed IDs, дату/revision и evidence ссылку,
пересчитывать downstream. Успех стартового packet не закрывает весь IP; его
оставшиеся ID перечислены в записи владельца. Принятые baseline frameworks —
зависимости по именам в спецификации packet и записям ниже, не повторные работы.

<a id="field002-execution"></a>
### FIELD-002 slice — очередь (DECISION-0063)

Данные: [field002-v1](../balance/field002-v1.md). Status ниже относится к packet, не к полному IP.

| Приоритет | Packet / владельцы | Status | Prerequisites / gate |
|---:|---|---|---|
| 1 | F2-01 — атака в конце рывка и повторный залп (IP-15/IP-21 framework) | Implemented | 2026-09-26: `EnemyDashVolleyProfile/Controller`, JSON `dashEndAttack`/`dashEndRepeat`, масштаб волн; EditMode 211/211 (Enemy/Traveler/Bootstrap) |
| 2 | F2-02 — BOSS-002/MIDBOSS-002 production encounters (IP-21) | Implemented | 2026-09-26; оба body v001 утверждены, импортированы и подключены; gameplay-scale review открыт. [Encounter evidence](evidence/2026-09-26-field002-slice.md), [art evidence](evidence/2026-09-26-field002-art.md) |
| 3 | F2-03 — поле FIELD-002: геометрия, окружение, выбор поля (IP-23) | Implemented | 2026-09-26; ground, boulder, column, shrine и thumbnail v001 подключены; boundary использует прежний плетень, gameplay-scale review открыт. [Field evidence](evidence/2026-09-26-field002-slice.md), [art evidence](evidence/2026-09-26-field002-art.md) |
| 4 | F2-04 — волны FIELD-002 и модификаторы поля (IP-24) | Implemented | 2026-09-26; [evidence](evidence/2026-09-26-field002-slice.md) |
| 5 | F2-05 — общий пул Путников без повторов ролей (IP-29/IP-30) | Implemented | 2026-09-26; [evidence](evidence/2026-09-26-field002-slice.md) |
| 6 | F2-06 — приёмка: прогоны FIELD-002, сложность, производительность | Blocked | F2-01…05 Implemented (Unity 802/802 + 28/28); нужен ручной прогон пользователя |

### Общий IP backlog после этапа

Порядок сохраняется для оставшегося scope. Возобновлять после команды пользователя;
текущие IP statuses и revision exceptions — в записях ниже.

| Приоритет | Модуль |
|---:|---|
| 1 | [IP-00](modules/IP-00-content-contract.md) |
| 2 | [IP-01](modules/IP-01-run-lifecycle.md) |
| 3 | [IP-02](modules/IP-02-player-movement.md) |
| 4 | [IP-03](modules/IP-03-character-stats.md) |
| 5 | [IP-04](modules/IP-04-enemy-core.md) |
| 6 | [IP-05](modules/IP-05-active-skill-runtime.md) |
| 7 | [IP-06](modules/IP-06-xp-progression.md) |
| 8 | [IP-07](modules/IP-07-level-up-draft.md) |
| 9 | [IP-08](modules/IP-08-active-skill-framework.md) |
| 10 | [IP-09](modules/IP-09-passive-framework.md) |
| 11 | [IP-10](modules/IP-10-reroll-banish.md) |
| 12 | [IP-10A](modules/IP-10A-ui-foundation.md) |
| 13 | [IP-31](modules/IP-31-manual-run-telemetry.md) |
| 14 | [IP-32](modules/IP-32-manual-ai-balance.md) |
| 15 | [IP-11](modules/IP-11-set-framework.md) |
| 16 | [IP-12](modules/IP-12-character-framework.md) |
| 17 | [IP-12A](modules/IP-12A-visual-presentation-foundation.md) |
| 18 | [IP-13](modules/IP-13-enemy-patterns.md) |
| 19 | [IP-14](modules/IP-14-wave-director.md) |
| 20 | [IP-15](modules/IP-15-boss-framework.md) |
| 21 | [IP-16](modules/IP-16-field-framework.md) |
| 22 | [IP-28](modules/IP-28-world-pickups.md) |
| 23 | [IP-29](modules/IP-29-traveler-framework.md) |
| 24 | [IP-25](modules/IP-25-meta-progression.md) |
| 25 | [IP-26](modules/IP-26-functional-ui.md) |
| 26 | [IP-17](modules/IP-17-production-skills.md) |
| 27 | [IP-18](modules/IP-18-production-passives.md) |
| 28 | [IP-19](modules/IP-19-production-sets.md) |
| 29 | [IP-20](modules/IP-20-production-enemies.md) |
| 30 | [IP-21](modules/IP-21-production-bosses.md) |
| 31 | [IP-22](modules/IP-22-production-characters.md) |
| 32 | [IP-23](modules/IP-23-production-fields.md) |
| 33 | [IP-30](modules/IP-30-production-travelers.md) |
| 34 | [IP-24](modules/IP-24-production-waves.md) |
| 35 | [IP-27](modules/IP-27-integration.md) |
| 36 | [IP-33](modules/IP-33-production-audio.md) |

## REPO-01 — Структура, навигация и единые проверки

Status: Verified
Scope: PROJECT_MAP, сокращение истории в STATUS, синхронизация entry rules, отдельный Game.Audio, структура генератора, нейтральное имя RuntimeContentCatalog и включение data checks/fingerprints.
Authorization: пользователь 2026-09-27 поручил реализовать предложенный рефакторинг; [DECISION-0072](../decisions/0072-project-structure-and-audio-ownership.md).
Acceptance: те же gameplay данные и аудиоклипы; сохранённые GUID; ссылки/карта актуальны; Python tooling tests, generation/audio integrity и полный безопасный Unity smoke.
Evidence: 2026-09-27, Python 23/23, Unity 6000.6.0f1 EditMode 857/857 + PlayMode 30/30, 0 skipped; generation/audio/manifest PASS. 39 перенесённых GUID сохранены; статусы всех 36 IP сохранены. [Рефакторинг](evidence/2026-09-27-project-structure.md).

## Scope revisions и готовность

IP-00/IP-02 сохраняют Verified: их behavioral acceptance не изменён, API текущего кода совместим; новые RunOutcome/control/presentation deltas проверяют их владельцы. При последующей несовместимой правке пересмотреть affected evidence.

IP-01 и изменённая основа IP-03…IP-14/IP-10A/IP-12A требуют новых дельт. Их прежнее Verified записано только в [архиве прежнего scope](evidence/pre-design-sync-R2.md). Все dependencies ниже относятся к `design-sync-R2`, если явно не указано иначе. IP-15 больше не Ready по старому IP-14 continuous evidence.

Для catalog packets выполненные ID и remaining scope ведутся здесь; pilot не переводит весь IP в Implemented/Verified. CG-01 approval получен; CG-02/03/04 и [G/W gaps](DESIGN_SYNC.md) учитываются только для зависящего packet. Не требуется повторно утверждать принятые designs.

Общие поля записей: Scope revision = `design-sync-R2` для всех IP, пока явно
не указано иное. Для ещё не начатых IP текущий packet — полная спецификация
либо явно согласованный catalog packet после выполнения prerequisites/gates;
Documentation impact регистрации — scope, prerequisites, gates и consumer links.
Это не implementation/verification evidence. Изменённый packet, выполненные ID,
отклонения и новый documentation impact записываются в конкретный IP.

## Модули

### IP-00 — Контракт контента, стабильные ID и конфигурация

Status: Verified
Dependencies: none
Current packet: Новой реализации не требуется; Context обновлён, существующее поведение сохранено.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Нет behavioral delta; новые интеграции проверяются в owning IP.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-00).
Target verification evidence: Сохранённые проверки [неизменного scope](evidence/pre-design-sync-R2.md#ip-00); M-01 проверяет документы/совместимость, Unity заново не запускался.
Documentation impact: Обновлены Context/источники/consumer links.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-00).

### IP-01 — Run lifecycle, pause ownership и результат забега

Status: Verified
Dependencies: IP-00
Current packet: Run identity, terminal snapshot/RunOutcome, reset/teardown contract; целевой Scope завершён с сохранением готового таймера/pause.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-01).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 257/257, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: IP-01 terminal/time/teardown contract; GDD/CD rules unchanged.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-01).

### IP-02 — Перемещение игрока, камера и базовая геометрия

Status: Verified
Dependencies: IP-01
Current packet: Новой реализации не требуется; Context обновлён, существующее поведение сохранено.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Нет behavioral delta; новые интеграции проверяются в owning IP.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-02).
Target verification evidence: Сохранённые проверки [неизменного scope](evidence/pre-design-sync-R2.md#ip-02); M-01 проверяет документы/совместимость, Unity заново не запускался.
Documentation impact: Обновлены Context/источники/consumer links.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-02).

### IP-03 — Character stats, Health и новые stat channels

Status: Verified
Dependencies: IP-01, IP-02
Current packet: Целевой scope завершён; состав реализации — в evidence.
Remaining gates: G-08/G-09 закрыты DECISION-0017. IP-05 фиксирует damage при активации; parameter mapping реализует IP-08.
Remaining acceptance / IDs: none; G-08/G-09 remain gates of consuming IPs.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-03).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 283/283, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: stat/units/JSON dictionary в IP-03, terminology в DECISION-0004; GDD/CD formulas unchanged; applicability оставлена G-08/G-09.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-03).

### IP-04 — Enemy lifecycle, contact damage и per-life identity

Status: Verified
Dependencies: IP-02, IP-03
Current packet: Целевой scope завершён; состав реализации — в evidence.
Remaining gates: Нет дополнительных product gaps для указанного scope.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-04).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 289/289, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: IP-04 lifecycle contract; DECISION-0016 Proposed (architecture review), GDD/CD rules unchanged; TD-001/003 mitigation documented without rewriting debt register.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-04).

### IP-05 — Общий combat pipeline, control effects и target contract

Status: Verified
Dependencies: IP-03, IP-04
Current packet: Unified combat attribution/results, movement-only slow, additive knockback и общий target-query contract.
Remaining gates: G-06…G-09 resolved by DECISION-0017; production control tuning belongs to later content packets.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-05).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 311/311, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: DECISION-0017 approved; GDD combat, PASSIVE-014, DESIGN_SYNC, proposal и affected IP gates синхронизированы. Size/range mapping остаётся реализацией IP-08, set propagation — IP-11.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-05).

### IP-06 — XP lifecycle, effective pickup radius и progression

Status: Verified
Dependencies: IP-04, IP-05
Current packet: Effective XP radius, source/drop identities, producer events и separate base/awarded lifetime totals.
Remaining gates: Нет дополнительных product gaps для указанного scope.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-06).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 320/320, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: IP-06 units/producer contract и fixture rationale; DECISION-0018 Proposed для архитектурного ревью реализации принятого scope. Product formulas PASSIVE-006/007/010 не изменены; G-01/G-03 позднее закрыты DECISION-0019/0020 в IP-07. IP-07 добавил atomic LevelsEarned range перед legacy per-level events, чтобы одна XP награда ставила requests подряд.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-06).

### IP-07 — Трёхслотовый драфт, request queue и build progression

Status: Verified
Dependencies: IP-01, IP-06
Current packet: Целевой fixture framework завершён; общая очередь/preview/revisions и immediate empty-Book currency по DECISION-0019/0020.
Remaining gates: Нет для IP-07. Production Book ID/сумма/lifetime — IP-28/IP-30/IP-25; G-02 закрыт DECISION-0022; controls snapshot — IP-10, global set chance поставлен IP-11; production значение остаётся balance-data.
Remaining acceptance / IDs: Нет для принятого scope IP-07.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-07).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 343/343, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: GDD XP/Book/meta rules, UI §§7/12, approved DECISION-0019/0020, DESIGN_SYNC, proposal и consumer IP-10/IP-10A/IP-11/IP-25/IP-28 синхронизированы. Fixture currency = 1 не утверждает production баланс. Legacy per-set fixture chance позднее заменён единым provider в IP-11; production значение не назначено.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-07).

### IP-08 — Active-skill levels, targeting и effect families

Status: Verified
Dependencies: IP-05, IP-07
Current packet: Framework design-sync-R2, 13 fixture definitions L1…L6; production IDs/art остаются IP-17.
Remaining gates: Для framework нет. G-08/G-09 закрыты DECISION-0017, additive level bonuses — DECISION-0021. G-04 остаётся только affected production/set gate; return для SKILL-008 не выдуман.
Remaining acceptance / IDs: Нет в обязательном framework scope; production SKILL-001…016 не зарегистрированы.
Target implementation evidence: Targeting, cumulative JSON resolver, spatial mapping, shared boomerang ledger, deceleration/pool, diagnostics и compatibility matrix — [IP-08 evidence](evidence/design-sync-R2-2026-09-21-ip08.md#ip-08).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1, Game.* EditMode 364/364, PlayMode 2/2 passed, 0 skipped. Условия и coverage — по ссылке выше.
Documentation impact: Content Design additive upgrades, approved DECISION-0021, IP-08 parameter/code/test matrix и consumer IP-03/IP-09/IP-17 синхронизированы. Fixture numbers не утверждают production balance.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-08).

### IP-09 — Passive modifiers и новые stat effects

Status: Verified
Dependencies: IP-03, IP-06, IP-07, IP-08
Current packet: Framework compatibility matrix PASSIVE-001…014; 9 non-production fixture definitions L1…L6, keyed lifecycle и slot descriptions.
Remaining gates: Нет для framework; actual potion roll/cap поставлен IP-28 по approved DECISION-0033, production definitions/icons — IP-18.
Remaining acceptance / IDs: Нет для обязательного framework scope; production PASSIVE-001…014 не зарегистрированы.
Target implementation evidence: Channels/migration/default ownership, reinitialize cleanup и dynamic UI — [IP-09 evidence](evidence/design-sync-R2-2026-09-21-ip09.md#ip-09).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 369/369, PlayMode 2/2 passed, 0 skipped. Условия и coverage — по ссылке выше.
Documentation impact: IP-09 mapping/defaults и PASSIVE-007 migration, IP-18 consumer contract, regression map и готовность потребителей синхронизированы. GDD/CD и production balance не изменены.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-09).

### IP-10 — Reroll/banish для обновлённого драфта

Status: Verified
Dependencies: IP-07
Current packet: Request-local set checks snapshot, reroll/banish policy, shared Book controls и UI Banish mode/cancel/revision reset.
Remaining gates: Нет для fixture framework. G-02 закрыт approved DECISION-0022; G-03 — DECISION-0020. Production counts/recovery остаются CG-04; global set chance provider поставлен IP-11; production значение остаётся balance-data.
Remaining acceptance / IDs: Нет для обязательного scope IP-10.
Target implementation evidence: Snapshot всех checks, ordinal ID, сохранение при banish, mode/cancel/control hints — [IP-10 evidence](evidence/design-sync-R2-2026-09-21-ip10.md#ip-10).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 374/374, PlayMode 2/2 passed, 0 skipped. Условия, coverage и XML/log paths — по ссылке выше.
Documentation impact: Approved DECISION-0022, GDD/UI, DESIGN_SYNC/proposal, IP-07/IP-10/IP-10A/IP-11/IP-19/IP-28 и readiness consumers синхронизированы. Production balance не изменён.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-10).

### IP-10A — UI Foundation, reusable cards, HUD и test harness

Status: Verified
Dependencies: IP-01, IP-03, IP-06, IP-07, IP-10
Remaining gates: G-01/G-03 short/book states определены DECISION-0019/0020 и проверены IP-07. Foundation сохраняет существующий contract. Baseline-relative character filtering — IP-12.
Remaining acceptance / IDs: none for the foundation scope; real recipe/character semantics belong to IP-11/IP-12.
Target implementation evidence: Reusable cards, recipe projection contract, compact HUD/Pause grid, notifications, changed-state rendering — [IP-10A evidence](evidence/design-sync-R2-2026-09-21-ip10a.md#ip-10a).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 383/383, PlayMode 3/3, 0 skipped; geometry/input and reviewed captures at 1920x1080 / 1280x720. Pre-existing post-results teardown exception recorded in evidence.
2026-09-24 HUD speed extension по прямому запросу пользователя: 1×/2×/3×/5× через RunModel/RunController и UI presenter, выбор сохраняется через паузу, `Time.timeScale` сбрасывается при завершении/выходе. [DECISION-0054](../decisions/0054-run-speed-controls.md); Unity 6000.6.0f1: 711/711 Game.* EditMode, 27/27 PlayMode, 0 skipped, geometry 1920×1080/1280×720; [summary](../../TestResults/checks/20260924T180701-450922Z/summary.json). Визуальная проверка подтверждена пользователем 2026-09-24: «всё хорошо, проверено».
Documentation impact: Component/semantic contracts, IP-11/IP-12 consumers and readiness synchronized; no GDD/CD or production balance change.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-10a).

### IP-31 — Локальная телеметрия ручных прогонов

Status: Verified
Dependencies: IP-01, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-10, IP-10A
Current packet: Bounded local recorder, immutable JSON/summary/feedback export, provenance/capabilities, Playtest UI, feature-owned producers и ordered composition teardown.
Remaining gates: Нет product gates для реализации; отсутствующие boss/Traveler/meta/set-effect/character-detail adapters явно unsupported.
Remaining acceptance / IDs: Нет для telemetry scope. Реальный marker и companion feedback связаны; точное expected поведение по наблюдению «опыт стреляет» не уточнено, причина требует отдельной диагностики.
Target implementation evidence: [IP-31 evidence](evidence/design-sync-R2-2026-09-21-ip31.md#ip-31), [schema/metric dictionary](PLAYTEST_REPORT.md).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 407/407 Game.* EditMode, 4/4 PlayMode, 0 skipped; snapshots 1920×1080/1280×720. Ручной aborted run 108ff5b3e8ed457e84704dcbfa25e0f8: linked feedback/marker, pause, hashes, counters и final export проверены; [manual evidence](evidence/design-sync-R2-2026-09-21-ip31.md#manual-run-2026-09-21).
Documentation impact: Schema/retention/capabilities, BALANCE_WORKFLOW, IP-01/IP-04/IP-06/IP-07/IP-10A/IP-31 contracts, regression-map и readiness. DECISION-0023 Proposed: technical ownership/teardown; GDD/CD и баланс не менялись.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-31).

### IP-32 — Ручные прогоны и AI-assisted balance review

Status: Verified
Dependencies: IP-31
Current packet: Checklist/review templates, реальный fixture review OBS-01 (insufficient-evidence / no-change) и synthetic accept/apply/rollback exercise.
Remaining gates: Нет для workflow scope; BG-01 и explicit approval сохраняются для будущего применения конкретных чисел/механик.
Remaining acceptance / IDs: Нет для workflow scope. OBS-01 остаётся открытым; диагностика и реальный follow-up описаны в review, исправление не заявлено.
Target implementation evidence: [IP-32 evidence](evidence/design-sync-R2-2026-09-21-ip32.md#ip-32), [реальный review](../balance/balance-progression-2026-09-21.md), [checklist](../playtests/CHECKLIST.md).
Target verification evidence: 2026-09-21, Python exercise exit 0: source hashes/arithmetic, 8 отказов, partial approval, apply/rollback/drift, Content JSON unchanged. Markdown links/diff checks. Unity не запускалась: runtime/config не менялись.
Documentation impact: BALANCE_WORKFLOW, playtest templates/review/OBS, IP-32 и readiness; GDD/CD без изменений, tuning не применён.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-32).

### IP-11 — Set recipes, priority draft policy и effect families

Status: Verified
Dependencies: IP-07, IP-08, IP-09, IP-10, IP-10A
Current packet: Global chance/order/backfill, 3–6-component recipes, six reusable effect families in four real JSON fixtures, source/non-recursion, keyed cleanup, recipe projection/acquisition feedback and DEV counters.
Remaining gates: Нет для fixture framework. G-04/G-05/G-13, exact production payloads/thresholds/art остаются у IP-19; real potion event binding поставлен IP-28; production SET-001…020 не зарегистрированы.
Remaining acceptance / IDs: Нет для обязательного framework scope. Per-ID production correctness и manual art review не заявлены.
Target implementation evidence: [IP-11 evidence](evidence/design-sync-R2-2026-09-21-ip11.md#ip-11), [schema/compatibility matrix](modules/IP-11-set-framework.md#реализованный-framework-contract).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 426/426 Game.* EditMode, 5/5 PlayMode, 0 skipped. Four simultaneous sets via queued choices и deterministic producer-first teardown проверены; XML/log paths — в evidence.
Documentation impact: IP-03/IP-08/IP-10/IP-11/IP-19/IP-28 contracts, regression guard и readiness; DECISION-0025 Proposed для source/ownership architecture review. GDD/CD и production balance без изменений; OBS-01 открыт.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-11).

### IP-12 — Character definitions, weighted draft и selection presentation

Status: Verified
Dependencies: IP-07, IP-08, IP-09, IP-10A
Current packet: Fixture character framework с отдельным baseline, ordered highlights и pre-run selection по approved DECISION-0026.
Remaining gates: Нет для framework packet. Production G-14 остаётся IP-22; G-15 resolved по DECISION-0037, profile поставляет IP-25; G-17/G-18 и image approval — IP-12A/IP-22. Fixture numbers/placeholders не являются production balance/art.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [IP-12 evidence](evidence/design-sync-R2-2026-09-21-ip12.md#ip-12), [schema/API](modules/IP-12-character-framework.md#framework-api-и-fixture-schema).
Target verification evidence: 2026-09-21 — Unity 6000.6.0f1, **439/439 Game.* EditMode, 6/6 PlayMode, 0 skipped**. Реальный selection→Sturdy loadout→Shutdown→Agile, locked rejection, baseline independence/highlights, weights и telemetry; [details](evidence/design-sync-R2-2026-09-21-ip12.md#coverage-and-verification).
Documentation impact: IP-12/IP-22/IP-25/IP-26 contracts, DECISION-0027 technical record (Proposed), regression guard и consumer readiness. GDD/CD production values без изменений.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-12).

### IP-12A — Visual Presentation Foundation и asset production pipeline

Status: Implemented
Dependencies: IP-00, IP-02, IP-03, IP-04, IP-05, IP-08, IP-12
Current packet: Category import/role validation, provenance/inventory reconciliation, generic presentation adapters и synthetic fixture kit.
Remaining gates: G-17 concept mapping и G-18 закрыты DECISION-0029; per-image/replacement gates сохраняются для новых assets. Пользователь принял Presentation Fixture Review; остаётся gameplay density часть gate E.
Remaining acceptance / IDs: Реальный gameplay density review с 3–4 сетами. UI body reuse и idle/flip/hit/proc/death/collect/pause/reset в Presentation Fixture Review приняты пользователем 2026-09-21 («всё хорошо»). Четыре synthetic copies не являются этим прогоном; production enemy/pickup/VFX art не заявлен.
Target implementation evidence: [IP-12A evidence](evidence/design-sync-R2-2026-09-21-ip12a.md#ip-12a), [pipeline/API](../art/ASSET_PIPELINE.md#21-category-profiles-и-reusable-adapters-ip-12a), [manifest](../../Art/asset-manifest.json).
Workflow tooling: пакетная подготовка approved art, numeric-only preview и безопасный scoped runner; [DECISION-0049](../decisions/0049-art-workflow-automation-and-preview.md), [tooling evidence](evidence/2026-09-22-workflow-tools.md). Density/per-image gates сохраняются.
Art follow-up: approved ENEMY-001 body подключён к FIXTURE-ENEMY-SEEKER; импорт, отдельный child motion и pool reset проверены. 641/641 EditMode, 23/23 PlayMode; [evidence](evidence/2026-09-21-enemy001-art.md). Текущий gameplay-визуал принят пользователем 2026-09-22; документированный density-прогон с 3–4 сетами остаётся отдельной проверкой.
Contact follow-up (IP-02/IP-04/IP-12A): по поручению пользователя от 2026-09-22 выполнен опыт с меньшими кругами внутри двух текущих body; [DECISION-0039](../decisions/0039-conservative-body-contact-circles.md), [evidence](evidence/2026-09-22-body-contact-circles.md). Повторный пользовательский плейтест ощущения открыт; этот опыт не закрывает production/density gates и не начинает следующий IP.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: **481/481 Game.* EditMode, 8/8 PlayMode, 0 skipped**. Body/UI/VFX import/reimport, role/resource negatives, alpha border, child-root invariance, pool/disable/reinitialize и preferences; manifest audit 9 records. Diagnostic capture 1920×1080 просмотрен; пользователь отдельно принял интерактивный стенд («всё хорошо»). Это не подтверждает плотный gameplay с 3–4 сетами. [Details](evidence/design-sync-R2-2026-09-21-ip12a.md#checks).
Documentation impact: Approved DECISION-0029, Proposed technical DECISION-0030, GDD/Art Direction, pipeline/inventory/provenance/manifest, IP-12A/IP-14/IP-22/IP-26 и readiness. W-01 runtime burst поставлен отдельным IP-14; его spawn-only checks не закрывают gameplay density review.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-12a).

### IP-13 — Enemy movement/attack patterns и control integration

Status: Verified
Dependencies: IP-03, IP-04, IP-05
Current packet: Fixture movement/attack/control integration, explicit per-kind JSON и category-neutral projectile lifecycle.
Remaining gates: Нет для fixture framework. G-07 закрыт DECISION-0017. G-14 остаётся для production cards; новые wind-up/control values — synthetic fixtures.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [IP-13 evidence](evidence/design-sync-R2-2026-09-21-ip13.md#ip-13), [schema/compatibility matrix](modules/IP-13-enemy-patterns.md#schema-и-runtime-contract).
Target verification evidence: 2026-09-21 — Unity 6000.6.0f1, **467/467 Game.* EditMode, 7/7 PlayMode, 0 skipped**. Все семь attack families, dash+slow+knockback, source после смерти/reuse стрелка, pool/terminal cleanup и representative physics smoke; [details](evidence/design-sync-R2-2026-09-21-ip13.md#coverage-and-verification).
Documentation impact: IP-13/IP-15/IP-20/IP-21/IP-29 contracts, DECISION-0028 (Proposed technical record), regression guards и consumer readiness. GDD/CD production values не изменены.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-13).

### IP-14 — Wave Director: continuous и burst timeline

Status: Verified
Dependencies: IP-04, IP-13
Current packet: Explicit continuous/burst schema, one-shot uncapped windows, skipped-window expiry, seeded composition/geometry, deterministic hooks, actual spawn outcomes и existing HUD/DEV projection.
Remaining gates: Нет для synthetic framework. W-01 выполнен по DECISION-0029; G-11/G-14 production schedules/Traveler timing остаются у catalog packets. IP-12A density review отдельно.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [IP-14 evidence](evidence/design-sync-R2-2026-09-21-ip14.md#ip-14), [runtime/schema](modules/IP-14-wave-director.md#runtime-и-fixture-schema).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: **496/496 Game.* EditMode, 9/9 PlayMode, 0 skipped**. 100 enemies ×10 cycles: empty-pool 10.342 ms, pooled max 1.376 ms; approved bounds 250/50 ms, unique objects 100, registry baseline restored. [Conditions/results](evidence/design-sync-R2-2026-09-21-ip14.md#checks).
Documentation impact: DECISION-0014 supplement, IP-14 schema/fixture rationale, IP-15/IP-24 consumer contracts и readiness. GDD/CD/art без изменений; production balance/FPS guarantees не заявлены.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-14).

### IP-15 — Boss/mid-boss encounter framework

Status: Verified
Dependencies: IP-01, IP-05, IP-08, IP-13, IP-14, IP-10A
Current packet: Synthetic final/mid encounters, ordered attack sequences и HP thresholds, one-shot uncapped hooks, lifecycle/source events и final HUD через producer Changed.
Remaining gates: Нет для synthetic framework. G-07 закрыт DECISION-0017; G-14 production attack payload/rewards/timings/assets остаются IP-21/IP-24.
Remaining acceptance / IDs: none for fixture framework; BOSS-/MIDBOSS- production IDs не поставлялись.
Target implementation evidence: [IP-15 evidence](evidence/design-sync-R2-2026-09-21-ip15.md#ip-15), [schema/runtime](modules/IP-15-boss-framework.md#fixture-schema-и-phase-contract).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: **516/516 Game.* EditMode, 10/10 PlayMode, 0 skipped**. Все 13 fixture skills повреждают boss; real scene bar/telegraph/pause/terminal cleanup. [Conditions/results](evidence/design-sync-R2-2026-09-21-ip15.md#checks).
Documentation impact: IP-15 schema/ownership/missing-rule list, IP-16/IP-21 bindings, DECISION-0031 Proposed technical record, regression guards и readiness. GDD/CD/art без изменений; IP-12A density review отдельно.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-15).

### IP-16 — Field definitions, selection и run configuration

Status: Verified
Dependencies: IP-02, IP-12, IP-14, IP-15, IP-10A
Current packet: Два synthetic поля, typed refs/validation, profile access, Character→Field→Run/Back, immutable run identity и telemetry, fresh director/encounters/spawn reset.
Remaining gates: Нет для fixture framework. G-14 production geometry/schedules относятся к IP-23; G-20 resolved по DECISION-0038; G-15 resolved по DECISION-0037, profile поставляет IP-25; placeholder thumbnail без image approval.
Remaining acceptance / IDs: none for fixture framework; FIELD-001…010 не поставлялись.
Target implementation evidence: [IP-16 evidence](evidence/design-sync-R2-2026-09-21-ip16.md#ip-16), [schema/API](modules/IP-16-field-framework.md#framework-api-и-fixture-schema).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1 — **538/538 Game.* EditMode, 12/12 PlayMode, 0 skipped**. Selection/locked/Back, typed refs/hooks, two-field reinit, release-safe snapshot, actual telemetry, player-only geometry и cancellation; [checks](evidence/design-sync-R2-2026-09-21-ip16.md#checks).
Documentation impact: IP-16/IP-23/IP-24/IP-25/IP-26/IP-29 contracts, Proposed DECISION-0032, G-20 и consumer readiness. GDD/CD/art без изменений.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-16).

### IP-28 — World pickup framework: зелье лечения и Book

Status: Verified
Dependencies: IP-05, IP-06, IP-07, IP-09, IP-10, IP-11, IP-12A
Current packet: Пользователь разрешил реализацию запросом «работаем дальше». Synthetic potion/Book framework по approved DECISION-0033 завершён; разрешение ограничено IP-28.
Remaining gates: Нет для fixture framework. Production числа и Book ID/card/art остаются у IP-20/IP-30; IP-12A density review отдельно.
Remaining acceptance / IDs: none для fixture framework; production IDs не зарегистрированы.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21-ip28.md): pooled lifecycle, reachable death drops, Health/Book/set rewards, UI/telemetry.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 571/571 Game.* EditMode, 14/14 PlayMode, 0 failed, 0 skipped; [coverage/results](evidence/design-sync-R2-2026-09-21-ip28.md#checks).
Documentation impact: Approved DECISION-0033/GDD/CD/DESIGN_SYNC; Proposed technical DECISION-0034, IP-28 schema/ownership, consumer contracts/readiness синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-28).

### IP-29 — Traveler encounter framework

Status: Verified
Dependencies: IP-08, IP-13, IP-15, IP-16, IP-28
Current packet: По запросу «реализуй» выполнен synthetic encounter framework DECISION-0035: восемь fixtures/три роли, field schedules, support, Book, UI/telemetry. Дальнейшие IP автоматически не начинать.
Remaining gates: Нет для fixture framework. G-14 production presence/XP/support/attack values, field pools и art остаются IP-24/IP-30. G-20 UI difficulty не используется как scaling rank; IP-12A density review отдельно.
Remaining acceptance / IDs: none для fixture framework; production TRAVELER-001…010 не зарегистрированы.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21-ip29.md): runtime/schedules/placement, Enemy protection/target integration, HUD/dev/telemetry.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 598/598 Game.* EditMode, 15/15 PlayMode, 0 failed, 0 skipped; [coverage/results](evidence/design-sync-R2-2026-09-21-ip29.md#verification).
Documentation impact: Approved DECISION-0035/GDD/CD/DESIGN_SYNC; Proposed technical DECISION-0036; IP-13/IP-16/IP-24/IP-26/IP-27/IP-29/IP-30, regression-map и consumer readiness синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-29).

### IP-25 — Persistent profile, meta currency, unlocks и permanent progression

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — новый production profile 10/10/5 и DECISION-0050 unlock metadata; terminal integration в F1-08. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-01, IP-03, IP-12, IP-16, IP-10A
Current packet: F1-03 по DECISION-0050/0051, затем F1-08 integration. Поздний gameplay не включён.
Remaining gates: F1-00/01/02; ещё не проверены новые production unlock/UI contracts и startup bindings.
Remaining acceptance / IDs: Unity verification; terminal/save/retry на production профиле — F1-09 matrix. F1-08: production `profile-v1.json` и composition по экономике профиля Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md). F1-03 subset (10/10/5, DECISION-0050 mapping, load-time migration) Implemented 2026-09-24 — [evidence](evidence/field001-f1-03-2026-09-24.md). 2026-09-26: галочка «Disable permanent upgrades» на экране Meta Progression — отключает все постоянные улучшения без возврата, уровни/валюта/unlocks сохраняются; профиль schemaVersion 2 с миграцией v1 ([DECISION-0064](../decisions/0064-disable-permanent-upgrades.md)); EditMode Meta/UI/Bootstrap/Character PASS 169/169 (`TestResults/checks/20260926T195542-249877Z/summary.json`); Unity full PASS (EditMode 810/810, PlayMode 28/28, `TestResults/checks/20260926T195615-348234Z/summary.json`); в игре не проверено. 2026-09-26: DEV-кнопка «reset all progression» рядом с DEV-открытием персонажей/полей (только Editor/Development build, главное меню, подтверждение вторым кликом) — заменяет профиль новым, прежние файлы сохраняются как `.preserved-*`; Unity full PASS (EditMode 812/812, PlayMode 28/28, `TestResults/checks/20260926T200344-388646Z/summary.json`); в игре не проверено.
Prior implementation evidence (design-sync-R2): [IP-25 evidence](evidence/design-sync-R2-2026-09-21-ip25.md#implementation), [runtime/schema](modules/IP-25-meta-progression.md#runtime-api--schema--reset).
Prior verification evidence (design-sync-R2): 2026-09-21, Unity 6000.6.0f1: **624/624 Game.* EditMode, 18/18 PlayMode, 0 skipped**; [coverage/results](evidence/design-sync-R2-2026-09-21-ip25.md#checks).
Documentation impact: IP-25 API/schema/save/reset и IP-26 consumers, regression map; GDD/CD правила DECISION-0037 сохранены. Fixture Book=50, новые raster assets не создавались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-25).

Target implementation evidence: none для field-001-start-R1 delta.
Target verification evidence: none для field-001-start-R1 delta; прежние smoke не переносятся автоматически.

### IP-26 — Functional UI и полный player flow

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — startup/locks/recipe UI; Results и actual-content integration в F1-08. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-01, IP-10A, IP-11, IP-12, IP-15, IP-16, IP-25, IP-28, IP-29, IP-12A
Current packet: F1-03 по DECISION-0050/0051, затем F1-08 integration. Поздний gameplay не включён.
Remaining gates: F1-00/01/02; ещё не проверены новые production unlock/UI contracts и startup bindings.
Remaining acceptance / IDs: Unity verification и F1-09 matrix (Results/save/retry на production composition). F1-08: selection/run/Results подключены к production composition — [evidence](evidence/field001-f1-08-2026-09-24.md). F1-03: production roster/lock reasons data — [evidence](evidence/field001-f1-03-2026-09-24.md).
Prior implementation evidence (design-sync-R2): Main Menu/full navigation, settings persistence/video rollback/audio routing/shake, notifications, result sets/special kills и permanent modifier display; [IP-26 evidence](evidence/design-sync-R2-2026-09-21-ip26.md#ip-26).
Documentation impact: DECISION-0038, GDD/CD/UI settings/difficulty, IP-12A/16/23/26 contracts, DESIGN_SYNC, regression map и consumer readiness.
Prior verification evidence (design-sync-R2): 2026-09-21, Unity 6000.6.0f1, **637/637 Game.* EditMode, 22/22 PlayMode, 0 skipped**, Windows release build exit 0. Interactive menu/settings/contrast checked at native 2560×1440; Пользователь сообщил «всё в порядке», кроме недоступного Retry после поражения; [OBS-01](../playtests/2026-09-21_defeat-ui.md#obs-01--после-поражения-нельзя-перезапустить-забег) воспроизведён и исправлен с failing-before/passing-after regression. После отчёта об исправлении пользователь явно поручил «ставь верифайд и комить»: оставшиеся manual acceptance gates закрыты его приёмкой. Новые измерения 1920×1080 или повторный ручной прогон не заявляются; см. evidence/DECISION-0038.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-26).

Target implementation evidence: none для field-001-start-R1 delta.
Target verification evidence: none для field-001-start-R1 delta; прежние smoke не переносятся автоматически.

### IP-17 — Production Active Skills SKILL-001…016

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-01 — SKILL-001…007/010/013/014. Required packets: F1-00; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-08, IP-10A, IP-12A
Blocked by: нет для реализации; данные, поведение и world art всех 16 ID подключены.
Remaining gates: ручная visual acceptance на реальной скорости; G-08/G-09 закрыты DECISION-0017, параметры 16 skills Approved по DECISION-0053/0060.
Remaining acceptance / IDs: ручная проверка читаемости луча SKILL-012, мины и снарядов SKILL-009/011/015/016 на реальной скорости.
Late IDs 2026-09-26: SKILL-008/009/011/012/015/016 Implemented (production JSON, per-ID тесты L1…L6), луч SKILL-012 — процедурный; Unity full PASS 766/766 + 27/27 — [evidence](evidence/2026-09-26-late-skills-passives.md).
Data packet 2026-09-26: [late-skills-passives-v1](../balance/late-skills-passives-v1.md) для SKILL-008/009/011/012/015/016 — Approved 2026-09-26 ([DECISION-0060](../decisions/0060-late-skills-passives-data-v1.md)); static validator PASS. SKILL-012 использует процедурный луч; world art остальных четырёх подключён ниже.
World art 2026-09-26: пользователь утвердил SKILL-009/011/015/016; immutable masters, provenance, runtime PNG и typed references подключены. Unity full PASS 769/769 EditMode + 27/27 PlayMode, manifest 108/108; gameplay-scale review остаётся открытым. [Evidence](evidence/2026-09-26-late-skill-world-art.md).
Startup subset F1-01: SKILL-001…007/010/013/014 Implemented 2026-09-24 — [evidence](evidence/field001-f1-01-2026-09-24.md).
Target implementation evidence: F1-01 subset и поздние SKILL-008/009/011/012/015/016; см. evidence выше.
Target verification evidence: автоматические проверки PASS 2026-09-26; ручная visual acceptance не проведена.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-17).

### IP-18 — Production Passive Items PASSIVE-001…014

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-02 — PASSIVE-001…005/007…009/011/012. Required packets: F1-00/01; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-09, IP-10A, IP-12A, IP-28
Blocked by: нет.
Remaining gates: G-08/G-09 закрыты DECISION-0017; G-10 закрыт DECISION-0033/IP-28; полные значения 14 passives остаются; отсутствие конкретного runtime parameter не заполняется hidden default.
Remaining acceptance / IDs: все 14 ID реализованы; осталась ручная проверка читаемости иконок PASSIVE-006/010/013/014 в draft/build slots.
Late IDs 2026-09-26: PASSIVE-006/010/013/014 Implemented, Unity full PASS 766/766 + 27/27 — [evidence](evidence/2026-09-26-late-skills-passives.md).
Data packet 2026-09-26: [late-skills-passives-v1](../balance/late-skills-passives-v1.md) для PASSIVE-006/010/013/014 — значения карточек и каналы, Approved 2026-09-26 ([DECISION-0060](../decisions/0060-late-skills-passives-data-v1.md)); static validator PASS.
Startup subset F1-02: PASSIVE-001…005/007…009/011/012 Implemented 2026-09-24 — [evidence](evidence/field001-f1-02-2026-09-24.md).
Target implementation evidence: F1-02 subset only.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-18).

### IP-19 — Production Sets SET-001…020

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-05 — SET-001/004/006/010/017. Required packets: F1-00/01/02/04; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-11, IP-17, IP-18, IP-28, IP-12A
Blocked by: нет для реализации; IP-17/IP-18 Implemented, IP-11/IP-28 Verified, IP-12A Implemented.
Remaining gates: G-08 закрыт DECISION-0017, G-02 закрыт DECISION-0022; G-04/G-05/G-13 закрыты DECISION-0061 для sets-v1. Финальная проверка и visual acceptance остаются после реализации.
Remaining acceptance / IDs: все 20 SET ID реализованы; остаются реальный прогон и ручное сочетание 3–4 сетов на реальном масштабе.
Data packet 2026-09-26: [sets-v1](../balance/sets-v1.md) — пороги и числа 15 сетов, решения G-04/G-05 и орбита SET-014; Approved 2026-09-26 ([DECISION-0061](../decisions/0061-sets-data-v1.md), мусор +50%, орбита +35% вращения); static validator PASS. G-04/G-05/G-13 для этих ID закрыты; IP-17 world art подключён 2026-09-26.
World art 2026-09-26: пользователь утвердил отдельные projectile-спрайты SET-008/016/018/019/020; masters, provenance, runtime PNG, typed references и специальные маршруты SET-008/set-attacks подключены. Art PASS 44/44, full PASS 792/792 EditMode + 27/27 PlayMode, manifest 117/117. Gameplay-scale и ручной обзор сочетания сетов остаются открытыми. [Evidence](evidence/2026-09-26-set-world-art.md).
Startup subset F1-05: SET-001/004/006/010/017 Implemented 2026-09-24 — [evidence](evidence/field001-f1-05-2026-09-24.md).
Target implementation evidence: 2026-09-26 — SET-002/003/005/007/008/009/011/012/013/014/015/016/018/019/020 и новый вид `SkillMechanics`; [evidence](evidence/2026-09-26-sets-v1.md).
Target verification evidence: Unity full PASS 2026-09-26, EditMode 784/784 + PlayMode 27/27 (автоматические); ручная проверка сочетаний не выполнена.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-19).

### IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-04 — ENEMY-001…005, ENEMY-007 и PICKUP-001. Required packets: F1-00; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-04, IP-13, IP-28, IP-12A
Blocked by: gameplay-scale review новых body и projectile visuals. ENEMY-006 переиспользует ENEMY-005 projectile. Данные и поведение всех 20 врагов реализованы 2026-09-26; per-ID projectile art gate поздних ranged IDs закрыт 2026-09-27.
Remaining gates: G-10 semantics/lifecycle закрыты DECISION-0033/IP-28; G-14 для поздних ID закрыт DECISION-0062; AG-01 сохраняется только для игрового визуального review.
Remaining acceptance / IDs: ENEMY-006/008/009 и ENEMY-010…020 gameplay-scale body review; ENEMY-010/011/012/014/015/018/019 projectile gameplay-scale review; startup body art принят 2026-09-24.
Data packet 2026-09-26: [enemies-v1](../balance/enemies-v1.md) — недостающие параметры 14 врагов, скорость ×1.3 к карточной по образцу FIELD-001, прочие карточные числа без изменений; Approved 2026-09-26 ([DECISION-0062](../decisions/0062-enemies-data-v1.md)); static validator PASS; G-14 для этих ID закрыт.
Late IDs 2026-09-26: ENEMY-006, 008…020 Implemented (production JSON, per-ID тесты); ENEMY-006/008/009 body art — [FIELD-002 evidence](evidence/2026-09-26-field002-enemy-art.md), ENEMY-010…020 body art — [late-art evidence](evidence/2026-09-26-late-enemy-body-art.md). Projectile v001 для ENEMY-010/011/012/014/015/018/019 утверждены и подключены 2026-09-27; полный PASS 857/857 EditMode + 30/30 PlayMode, manifest 188/188. Открыт только ручной gameplay-scale review. [Projectile art evidence](evidence/2026-09-27-enemy-projectile-art.md).
Startup subset F1-04: ENEMY-001…005/007 + PICKUP-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-04-2026-09-24.md).
ENEMY-007 body contact refit to its approved half-size v002 sprite: radius 0.266696, centerY 0.299833; global contact fit PASS, Unity full smoke 784/784 EditMode и 27/27 PlayMode, zero skipped — [evidence](evidence/2026-09-26-enemy007-contact-refit.md). Остальные gates и статус IP-20 не изменились.
Target implementation evidence: ENEMY-001 v002 принят пользователем; runtime 256×256 импортирован и подключён как body существующего FIXTURE-ENEMY-SEEKER с отдельным motion profile/child rig. Fixture ID, баланс и collider сохранены. Production ENEMY-001 binding не выполнен; G-14 и пользовательский gameplay/density review остаются. [Art integration evidence](evidence/2026-09-21-enemy001-art.md).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 641/641 Game.* EditMode и 23/23 PlayMode, 0 skipped. Import/reimport GUID, registry refs, child-only motion, hit/pause, death/mixed-pool reuse и Gameplay spawner. [Условия и ограничения](evidence/2026-09-21-enemy001-art.md#verification).
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-20).

### IP-21 — Production Final Bosses и Mid-bosses

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-06 — BOSS-001 и MIDBOSS-001. Required packets: F1-00/01/04; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-15, IP-12A
Blocked by: поля FIELD-004…010 и их волны (IP-23/IP-24) ещё не созданы. FIELD-003 уже есть, но полный живой прогон поздних боссов на своих полях пока невозможен. Данные/поведение всех 16 реализованы (bosses-v1), body и projectile art подключены.
Remaining gates: gameplay-scale body/projectile review и живая проверка зон/лучей/призыва на своих полях.
Remaining acceptance / IDs: BOSS-002/MIDBOSS-002 gameplay-scale review; BOSS-003…010, MIDBOSS-003…010 — gameplay-scale review утверждённых тел/снарядов и ручная проверка в забеге; startup body/projectile art принят 2026-09-24. [FIELD-002 art](evidence/2026-09-26-field002-art.md), [late boss body art](evidence/2026-09-27-boss-body-art.md), [projectile art](evidence/2026-09-27-boss-projectile-art.md).
Startup subset F1-06: BOSS-001, MIDBOSS-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-06-2026-09-24.md).
Data packet 2026-09-27: [bosses-v1](../balance/bosses-v1.md) — недостающие параметры BOSS-003…010/MIDBOSS-003…010 (урон как доля контакта, XP, тайминги, телепорт финальных), фирменные атаки каждому боссу на трёх новых семействах (зона, луч, призыв; редакция 2 по просьбе пользователя) и расширения схемы E1…E6; карточные числа без изменений; **Approved 2026-09-27** ([DECISION-0066](../decisions/0066-bosses-data-v1.md)); GDD (правило зон/лучей/призыва) и 16 карточек CD синхронизированы; static validator PASS. Следующая работа IP-21 — реализация F1…F3, E1…E6 и 16 encounters по пакету; автоматически не начинать.
Packet bosses-v1 Implemented 2026-09-27: семейства зона/луч/призыв, расширения E1…E6 и 16 encounters из утверждённой таблицы; Unity 6000.6.0f1 full PASS — **EditMode 843/843, PlayMode 29/29, 0 skipped**, `TestResults/checks/20260927T080300-187440Z/summary.json`; [evidence](evidence/2026-09-27-ip21-bosses-v1.md).
Body art packet 2026-09-27: пользователь утвердил 16 поз после правок разнообразия рук; BOSS-003…010/MIDBOSS-003…010 body v001 импортированы и подключены к production data. [Art evidence](evidence/2026-09-27-boss-body-art.md).
Projectile art packet 2026-09-27: пользователь утвердил девять shared families FIELD-002…010; `BOSS-002…010-VISUAL-PROJECTILE` импортированы, соответствующие boss/midboss attacks переведены с fallback BOSS-001, зоны/лучи остаются procedural. [Art evidence](evidence/2026-09-27-boss-projectile-art.md).
Target implementation evidence: F1-06 (BOSS-001/MIDBOSS-001), F2-02 (BOSS-002/MIDBOSS-002), bosses-v1 (остальные 16) — [encounters](evidence/2026-09-27-ip21-bosses-v1.md), [body art](evidence/2026-09-27-boss-body-art.md), [projectile art](evidence/2026-09-27-boss-projectile-art.md).
Target verification evidence: boss art — Unity full PASS 2026-09-27 (**857/857 EditMode + 30/30 PlayMode**, 0 skipped), `TestResults/checks/20260927T125843-334689Z/summary.json`; art scope 50/50, manifest 176/176, `TestResults/checks/20260927T125557-368084Z/summary.json`; gameplay-scale review и живой прогон на своих полях открыты.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-21).

### IP-22 — Production Characters CHAR-001…010

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — CHAR-001; поздние character IDs только unlock metadata. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-12, IP-17, IP-12A
Blocked by: IP-17 (Blocked, target scope).
Remaining gates: G-14: weights; G-15 resolved по DECISION-0037, unlock metadata определены; concept/master identity подтверждена DECISION-0029, production runtime binding/art review остаются per-ID. CHAR-006 огр и прочие approved roster choices не переутверждаются.
Remaining acceptance / IDs: CHAR-002…010 production bindings, gameplay-scale body review и Unity verification CHAR-001. Утверждённые body CHAR-002…005 подготовлены как visual assets 2026-09-26 — [evidence](evidence/2026-09-26-character-body-art.md). Body CHAR-006…010 подготовлены единым art packet 2026-09-27: runtime imports и contact profiles зарегистрированы, Unity art scope 44/44 EditMode, manifest 143/143, global contact fit PASS; production binding и gameplay-scale review открыты — [evidence](evidence/2026-09-27-character-body-art.md).
Startup subset F1-03: CHAR-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-03-2026-09-24.md).
Target implementation evidence: F1-03 subset only.
Target verification evidence: Art-prep CHAR-006…010: Unity 6000.6.0f1, 44/44 Game.* EditMode, zero skipped, manifest 143/143 (`TestResults/checks/20260927T071305-197020Z/summary.json`); полный IP-22 не проверен.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-22).

### IP-23 — Production Fields FIELD-001…010

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-08 — FIELD-001 geometry/environment/metadata/thumbnail. Required packets: F1-00…07; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-16, IP-20, IP-21, IP-12A
Blocked by: IP-20 (Blocked, target scope), IP-21 (Blocked, target scope).
Remaining gates: G-14: geometry/enemy pools; G-20 resolved по DECISION-0038; G-15 resolved по DECISION-0037. Весь approved mapping переносится, numeric schedules отдельно.
Remaining acceptance / IDs: FIELD-002 gameplay-scale review; FIELD-003 ручной прогон и gameplay-scale review утверждённых руин/воды/миниатюры; FIELD-004…010 geometry/metadata/environment kits, production bindings и target-scale review подготовленных thumbnails; FIELD-001 thumbnail image и Unity verification. [FIELD-002 art](evidence/2026-09-26-field002-art.md), [FIELD-003 art](evidence/2026-09-27-field003-art-and-projectile-halo.md), [FIELD-004…010 thumbnails](evidence/2026-09-27-field004-010-thumbnails.md).
Startup subset F1-08: FIELD-001 geometry/obstacles/metadata/environment Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md).
Data packet 2026-09-27: [field003-v1](../balance/field003-v1.md) — FIELD-003 «Пограничные руины»: волны на каркасе FIELD-001 (HP ×1.24, урон ×1.16, частота +20%, стрелки 28%, лимит ≤220), 16 кластеров руин (107 кусков), ENEMY-010 с первой волны (карточка FIELD-003…008 по выбору пользователя); Proposed ([DECISION-0067](../decisions/0067-field003-v1.md)); static validator PASS.
FIELD-003 Implemented 2026-09-27: поле, окружение (107 авторских препятствий, вертикальные стены повёрнуты), FIELD-003-TIMELINE и FIELD-003-TRAVELERS из утверждённого пакета; арт руин, воды и миниатюры — per-ID gates, до них плетень/валун прежних полей; Unity full PASS 2026-09-27 (EditMode 847/847, PlayMode 30/30, `TestResults/checks/20260927T084545-984347Z/summary.json`); [evidence](evidence/2026-09-27-field003.md).
FIELD-003 art packet 2026-09-27: ground, ruined wall, rubble, visual-only water и thumbnail v001 утверждены и подключены; hostile projectile halo приглушён без изменения размера и остаётся под sprite. Art scope 50/50, manifest 181/181, FIELD-003 tests 4/4 PASS; gameplay-scale review открыт. [Evidence](evidence/2026-09-27-field003-art-and-projectile-halo.md).
FIELD-004…010 thumbnail packet 2026-09-27: семь v001 изображений утверждены пользователем, включая отдельную закатную палитру FIELD-010; source/runtime PNG, import profiles и `FIELD-004…010-VISUAL-BACKGROUND` зарегистрированы. Art scope 50/50 и manifest 195/195 PASS; production field definitions/bindings и target-scale UI review остаются открытыми. [Evidence](evidence/2026-09-27-field004-010-thumbnails.md).
Obstacle art packet FIELD-001…010 2026-09-27: 52 утверждённых prop v001 подготовлены и зарегистрированы (2/4/4 дополнения для FIELD-001/002/003, по 6 для FIELD-004…010). FIELD-001…003 используют optional per-piece visual overrides поверх прежней player-only geometry; FIELD-004…010 ждут production layouts. Art scope 50/50, manifest 247/247 PASS; Unity full PASS — EditMode 859/859, PlayMode 30/30, 0 skipped (`TestResults/checks/20260927T153748-635944Z/summary.json`); gameplay-scale review открыт. [Evidence](evidence/2026-09-27-field-obstacle-art.md).
Ground texture packet FIELD-004…010 2026-09-27: семь утверждённых tile v001 подготовлены и зарегистрированы как `FIELD-004…010-VISUAL-GROUND`; FIELD-010 использует отдельную закатную палитру. Production bindings ждут definitions полей. Art scope 51/51, manifest 254/254 PASS (`TestResults/checks/20260927T160506-865622Z/summary.json`); repeat seams и gameplay-scale readability остаются открыты. [Evidence](evidence/2026-09-27-field004-010-ground-textures.md).
Per-run obstacle layouts 2026-09-27 ([DECISION-0068](../decisions/0068-per-run-obstacle-layouts.md)): FIELD-001…003 расставляют препятствия заново каждый забег из фиксированных паттернов по сетке ячеек (равномерная плотность, свободный старт, проходы между ячейками); Unity EditMode 854/854 + PlayMode 30/30 (раннер без итогового PASS из-за параллельных правок входных файлов) — [evidence](evidence/2026-09-27-per-run-obstacle-layouts.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-23).

### IP-30 — Production Travelers TRAVELER-001…010 и Book

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-07 — TRAVELER-001/002/005 и production Book. Required packets: F1-00/01/02/04/05; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-29, IP-12A
Blocked by: production Book card/ID/параметры, required Traveler/support/XP/presence data, production bindings и gameplay-scale art review; prerequisite IP-29 выполнен.
Remaining gates: G-03/G-10 semantics закрыты DECISION-0020/0033 и IP-28; G-11/G-12/scaling semantics — DECISION-0035. G-14/G-17, production Book card/ID/параметры, complete Traveler/support data, production bindings и gameplay-scale review. Designs TRAVELER-001…010 уже approved; body images для всех десяти подготовлены.
Remaining acceptance / IDs: TRAVELER-003/004/006…010 production content и bindings; startup body art принят 2026-09-24. Body для остальных семи IDs утверждены пользователем и подготовлены единым art packet 2026-09-27 — [evidence](evidence/2026-09-27-traveler-body-art.md). Поздний стабильный Unity art scope: 50/50 EditMode, manifest 151/151, global contact fit и общий runner PASS.
Startup subset F1-07: TRAVELER-001/002/005, FIELD-001 schedule, PICKUP-002 Implemented 2026-09-24 — [evidence](evidence/field001-f1-07-2026-09-24.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Production checks новых требований не запускались; art-preparation checks — [evidence](evidence/2026-09-27-traveler-body-art.md), общий PASS `TestResults/checks/20260927T103139-228226Z/summary.json`.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-30).

### IP-24 — Canonical Wave / Encounter Content и field bindings

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-08 — FIELD-001 900-second schedule и startup bindings. Required packets: F1-00…07; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-14, IP-20, IP-21, IP-23, IP-29, IP-30
Blocked by: IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-23 (Blocked, target scope), IP-30 (Blocked, target scope).
Remaining gates: CG-02/G-11/G-14/W-01: full per-field encounter/scaling packets; пустой Wave section не разрешает coding AI придумать канон.
Remaining acceptance / IDs: Полные production encounter schedules и bindings полей 004…010 (002/003 реализованы, ручные прогоны открыты); CG-02/CG-04; Unity verification FIELD-001.
Startup subset F1-08: FIELD-001-TIMELINE (900 s, hooks 450/810) и startup bindings Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md).
FIELD-003-TIMELINE Implemented 2026-09-27 по field003-v1 (DECISION-0067): 24 фазы, HP ×1.24, урон ×1.16, ENEMY-010 с первой волны, лимит со всплеском ≤ 250; Unity full PASS 2026-09-27 (EditMode 847/847, PlayMode 30/30, `TestResults/checks/20260927T084545-984347Z/summary.json`); [evidence](evidence/2026-09-27-field003.md). Ручной прогон не выполнен.
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-24).

### IP-27 — End-to-end integration, regression и content validation

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-09 — полный стартовый run и приёмка FIELD-001 только initial content. Required packets: F1-00…08; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-00, IP-01, IP-02, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-09, IP-10, IP-10A, IP-11, IP-12, IP-12A, IP-13, IP-14, IP-15, IP-16, IP-17, IP-18, IP-19, IP-20, IP-21, IP-22, IP-23, IP-24, IP-25, IP-26, IP-28, IP-29, IP-30, IP-31, IP-32
Blocked by: IP-17 (Blocked, target scope), IP-18 (Blocked, target scope), IP-19 (Blocked, target scope), IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-22 (Blocked, target scope), IP-23 (Blocked, target scope), IP-24 (Blocked, target scope), IP-25 (Blocked, field-001-start-R1 delta), IP-26 (Blocked, field-001-start-R1 delta), IP-30 (Blocked, target scope).
Remaining gates: Только реальные missing required contracts/data/asset checks полного scope этого плана. Уменьшение каталога возможно лишь как отдельное явное изменение плана; один smoke не закрывает content-complete verification.
Remaining acceptance / IDs: Все criteria/IDs из [спецификации](modules/IP-27-integration.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-27).

### IP-33 — Production audio, общий звуковой язык

Status: Implemented
Scope revision: audio-first-pass-R1, явное поручение пользователя 2026-09-27 вне заблокированной F1-09 очереди.
Dependencies: F1-03 settings subset (DECISION-0038), F1-01/04/06/07/08 event and composition subsets — выполнены для FIELD-001.
Remaining acceptance: прослушивание обычной/плотной волны, босса, паузы и 5×; возможная корректировка громкости/тембров. Специальный low-HP и attack-telegraph contract остаются за пределами первого среза.
Target implementation evidence: 28 CC0-клипов, один общий boss track, 15 семейств событий, ограниченные голоса и real-time cooldown; [подробности](evidence/2026-09-27-production-audio.md).
Target verification evidence: audio integrity 28/28; после REPO-01 Unity 6000.6.0f1 EditMode 857/857 (включая 3 Audio tests) + PlayMode 30/30, 0 skipped; [evidence](evidence/2026-09-27-project-structure.md). Автоматические PlayMode-прогоны без вывода звука на динамики; художественный review остаётся открытым. Предыдущие прогоны — в исходном audio evidence.

## Status maintenance rule

После изменения статуса/API/acceptance пересчитать готовность потребителей и Next Ready по Execution order. Implemented означает выполненный полный обязательный scope; Verified — фактически пройденные проверки с evidence. Исторический test count не переносится автоматически. Каталоги ведут completed/remaining IDs здесь; если ни один оставшийся packet не готов, указывать конкретный Blocked gate. В STATUS оставлять краткий результат, дату, revision и ссылку на подробное evidence в `evidence/`; старые проверки не читать при выборе следующего IP. Подробности — [WORKFLOW](WORKFLOW.md).
