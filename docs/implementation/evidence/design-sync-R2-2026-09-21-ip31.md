# design-sync-R2 — IP-31 evidence, 2026-09-21

## IP-31

Исполняемый packet и оставшиеся проверки хранятся только в [STATUS](../STATUS.md). Контракт: [IP-31](../modules/IP-31-manual-run-telemetry.md), [report schema](../PLAYTEST_REPORT.md).

### Реализация

- Game.Telemetry: main-thread bounded counters/timeline, immutable report payload, explicit unsupported capabilities и unknown attribution, ordinal config hashing/snapshots, sink на worker. Gameplay не зависит от diagnostics. Release/disabled путь не создаёт подписок.
- Bootstrap подключает recorder после готовых producers. Изначальный JSON сохранён вместе с cached catalog, git commit/dirty доступны в Editor либо honest unknown. Default fixture sources, actual run duration, initial stats/build, draft/wave seeds входят в provenance.
- Feature-owned events: player/spawner measured CombatResult, enemy life, XP, draft queue/offer/resolution/control, pause ownership, legacy wave phase/hook. Required RunOutcome дополнен acquired sets независимо от recorder.
- Finalization после unwinding callbacks захватывает lethal player damage и enemy pool-return result. Live/final export упорядочены. Repeat final/export не меняет snapshot/rewards; feedback не перезаписывается.
- DEV Playtest: collapsed launcher, bounded tabs/scroll, marker+simulation time, export state/error. UXML semantic IDs и отдельный presenter/view; release UI и intents gated.
- Composition Shutdown захватывает run, снимает UI/telemetry, затем остальные consumers до character/Health. Это устраняет воспроизведённые scene-reload NullReferenceException (UI/draft, passive modifiers, UI Dispose).

### Проверки и трассировка

Unity **6000.6.0f1**, полный filter `^Game\.`: **407/407 EditMode**, **4/4 PlayMode**, 0 failed, 0 skipped. Перед каждым запуском — свежая Win32_Process проверка отсутствия interactive Editor. EditMode: `scripts/Test-Unity.ps1`. Final PlayMode: hidden batch Test Runner с graphics для captures, без запуска поверх interactive Editor. Third-party tests filter-ом исключены.

Основные артефакты: `TestResults/EditMode.xml`, `TestResults/EditMode.log`, `TestResults/IP31-PlayMode.xml`, `TestResults/IP31-PlayMode.log`. Более ранние целевые 19/19 EditMode и промежуточный 4/4 no-graphics PlayMode не заменяют итоговые прогоны. Первоначальная ошибка missing Newtonsoft test references исправлена. Первоначальный PlayMode 3/4 выявил teardown defect; после исправления итоговый лог не содержит этих exceptions.

Финальный synthetic packet: `C:/Users/zheni/AppData/LocalLow/DefaultCompany/survivor-arena/Playtests/deb29c71b52f4ca2aa337b841650423e/`; config hash `f11cbc70dae46145df990c79bc897acff02f0316efa92cf223dd4f92a8b1154e`. Проверены файлы/linked IDs и сохранённый comment, ordinal `FIXTURE-SKILL-BOLT` dictionary key, commit и dirty=true. Raw packet не добавлялся в git. Оба финальных Playtest capture просмотрены; после первого capture улучшен контраст note input и выполнен новый полный PlayMode с graphics.

| Criterion | Evidence |
|---|---|
| Win/loss/abort/retry/error, immutable end/export | RunTelemetryRecorderTests.Terminal_ReasonIsDistinct_AndSnapshotDoesNotChange (5 cases) |
| Pause/DPS/equipped acquisition time | Pause_ExcludesWallTimeFromDps_AndEquippedTimeStartsOnAcquisition: 300 HP / 60 equipped s = 5; pause 20 s не входит |
| Unknown source, zero time, missing producer | UnknownAndZeroTime_AreNotInventedValues; ForeignRunDamage_IsIgnored |
| Bounded timeline/keys/dedup, duplicates/overflow | BoundedBuffers_SaturateWithoutEvictingDedupIds_AndCountDroppedData |
| Hash ordering/config/overrides/dirty, ID stability | Provenance_ReorderedFilesHashIdentically_ContentAndOverridesChangeHash; Snapshot_ContentIdDictionaryKeys_RetainOrdinalCase |
| Byte budget and no gameplay side effects | Export_ByteLimitFailsExplicitly_WithoutCompletingTheRun |
| 100 attempted vs 10 applied, terminal callback | PlaytestSessionTests.LethalPlayerHit_CompletesBeforeResult_ExportStillIncludesAppliedDamage |
| Synchronous pool return, fresh life identity | PooledEnemyLethalResult_SurvivesSynchronousReturn_AndOldLifeIsNotReused; registry restored to captured baseline |
| Delayed projectile after source despawn and skill level-up | DelayedProjectile_AfterSourceDespawnAndLevelUp_RetainsOriginalAttribution; real FixtureProjectileRuntime, original owner life and L1 after SetLevel(2) |
| XP collect/expire/recover/ground and duplicate pickup | XpDrop_CollectExpireRecoverAndGround_AreSeparateAndRepeatedPickupHasNoEffect: dropped 15, ground 3, collected base/award 4/8, expired/recovered 8/2 |
| Healing vs rescale | HealthRescale_IsNotHealing_ActualHealIsCapped: 4 actual healing, max-HP rescale to 20 не увеличивает healing |
| Draft controls/queue/selection/end | Draft_OffersControlsResolutionAndOutcome_AreRecordedWithoutChangingRng plus existing draft regressions |
| No recorder/cleanup/repeated teardown | DisabledRecorder_StillCapturesRequiredResults_AndDisposedSessionStopsObserving; Shutdown_DuringLiveExport_PublishesFinalSnapshotAfterEarlierPacket; scene smoke double Shutdown |
| Failure/retry/no-op | ExportFailure_IsVisibleAndRetryable_AndDoesNotEndRun; disabled session commands are no-op |
| UI gating/intents/unchanged render/semantic IDs | PlaytestPresenterTests (fake view/session and UXML) |
| Real scene and filesystem packet | PlaytestSmokeTests: lethal hit, JSON/summary/feedback, linked IDs, repeated export retains comment; synthetic marker explicitly labelled |
| Geometry/readability | UiFoundationSmokeTests: Playtest open at 1920×1080/1280×720, export within bounded drawer; `TestResults/ip31-playtest-*.png` |

### Performance и ограничения evidence

`Combat_HundredThousandHits_AggregateWithoutHotPathAllocationsOrRngChanges`: **100000 hits, 47.674 ms, 0 B allocations** в измеряемом участке; guard <2000 ms — test regression threshold, не gameplay/frame target. UnityEngine.Random state unchanged. Snapshot/serialization под PerfGuard 20 ms; наблюдался cold snapshot около 49 ms. Это разовая export-boundary стоимость, не отсутствие frame spike. PerfGuard warnings не используются как FPS/p95.

Ручная статическая проверка по perf-audit: combat aggregation не создаёт per-hit strings, не пишет файлы, maps bounded; snapshots линейны по bounded buffers; worker не обращается к Unity. Test-quality review: assertions различают requested/applied, base/award, missing/zero, reset/retained state. Изменение pause denominator, потеря lethal result или camel-casing ID ломают соответствующие tests. Pure tests используют fake clock/sink; реальная файловая запись — только integration smoke.

Synthetic scene run не является ручным прогоном и не доказывает качество баланса. Человек не выполнял ручной сценарий в рамках этого evidence; путь/комментарий такого прогона не заявляются. Нет проверки crash recovery, полного production roster или release player build. No-recorder Results проверены через отключённый adapter, release gating — source path + UI tests.

### Documentation impact

PLAYTEST_REPORT/schema/retention, BALANCE_WORKFLOW, IP-01/IP-04/IP-06/IP-07/IP-10A/IP-31 producer contracts, regression-map и STATUS/readiness. DECISION-0023 Proposed фиксирует technical cross-layer ownership; новые GDD/CD rules/балансные числа не вводились. Существующее пользовательское изменение Packages/manifest.json не редактировалось.
