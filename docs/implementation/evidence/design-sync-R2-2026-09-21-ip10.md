# design-sync-R2 — IP-10 evidence, 2026-09-21

## IP-10

### Scope и решение

[DECISION-0022](../../decisions/0022-draft-set-control-policy.md) утверждён пользователем перед реализацией. G-02: ordinal content ID; новые set checks при reroll; сохранение остальных результатов при banish. Prerequisite IP-07 подтверждён текущим кодом/новыми тестами. Production global probability и эффекты сетов принадлежат IP-11; production counters/recovery остаются CG-04. Existing progression JSON fixture counts сохранены.

### Реализация

- `Assets/Game/Progression/Draft/SetDraftCheckState.cs`: полный snapshot успешных проверок всех eligible sets, включая overflow. Failed checks сохраняются отсутствием ID в success set; repeated banish не вызывает provider. Вход и результат упорядочены ordinal content ID.
- `DraftPool` использует snapshot, авторитетные previews, set-first, ordinary weighted fill и uniform backfill. `ISetDraftOfferProvider` получает capacity всех eligible sets, а не число видимых slots. Legacy fixture adapter остаётся изолированным; глобальная production вероятность не объявляется реализованной.
- `LevelUpDraftRuntime` создаёт snapshot при открытии request и reroll, переиспользует при banish, очищает при Shutdown. Revision invalidation, shared Book counters, очередь, pause ownership и no-repeat currency сохраняются.
- `GameplayUiPresenter` владеет режимом Banish для конкретной revision. Карточка вызывает banish вместо select; Cancel ничего не расходует; revision/terminal reset возвращает chooser. View получает immutable `DraftViewState`, отображает режим, счётчики, disabled controls/reasons; новые semantic IDs, UXML и USS. Изображения не нужны.

### Acceptance / coverage

| Поведение | Проверки |
|---|---|
| Ordinal checks, overflow A/B/C/D → banish B → A/C/D; одинаково для LevelUp и Book | `DraftRequestTests.Controls_PreserveAllChecksOnBanish_RerollAndNextRequestCheckAgain` (2 cases) |
| Failed check не становится successful при banish; reroll перебрасывает | `DraftBackfillTests.Banish_PreservesFailedCheck_WhileRerollRechecksIt` |
| Alternative offer, exhausted counters, banish persistence, reset | `LevelUpDraftRuntimeTests.Reroll_ReplacesOpenDraftAndStopsAtConfiguredCounter`, `Banish_RemovesOfferedEntryWithoutClosingDraftAndPersistsForRun`, `Controls_ResetOnlyAfterPendingDraftIsResolved`; `PlayerBuildAndDraftTests` |
| Stale/double intents не расходуют counters, не выбирают следующий request | `DraftRequestTests.StaleRevision_CannotSelectNextRequestOrSpendItsCounters`, оба новых control cases |
| Backfill, banished/acquired exclusions, 0/1/2 options | `DraftBackfillTests`, `GameplayUiAssetTests.ShortDraft_RendersThreePositionsWithDisabledEmptyCards` |
| Queue/pause, Book exhaustion без валюты, fresh checks/reset | `DraftRequestTests` (15 cases), включая новые control cases и existing Book/terminal checks |
| Banish mode/cancel/stale revision/exhausted reasons | `GameplayUiPresenterTests.BanishMode_CancelIsFree_AndNewRevisionAndClosedDraftResetMode`, `ViewIntents_AreForwardedToModelCommands` |
| Same-revision mode rerender, semantic UI | `GameplayUiAssetTests.BanishMode_SameRevisionRerendersCardsAndControlState`, UXML semantic checks |
| Real composed Reroll/Banish/Cancel buttons and returning chooser | `GameplaySmokeTests.GameplayScene_ComposesLevelsUpAndResumes` через `NavigationSubmitEvent` |

### Проверки и условия

- Unity **6000.6.0f1**, Windows; `scripts/Test-Unity.ps1`, безопасный batchmode/nographics. Перед каждым запуском свежий CIM process check подтвердил отсутствие Unity.exe; interactive Editor не закрывался и не запускался параллельно.
- Полный **Game.* EditMode: 374/374 passed, 0 failed, 0 skipped**. `TestResults/IP10-2026-09-21/EditMode.xml`, `TestResults/IP10-2026-09-21/EditMode.log`. Third-party tests: 0 (отфильтрованы).
- Полный **Game.* PlayMode: 2/2 passed, 0 failed, 0 skipped**. `TestResults/IP10-2026-09-21/PlayMode.xml`, `TestResults/IP10-2026-09-21/PlayMode.log`. Third-party tests: 0. Изменение test-event target затрагивало только PlayMode assembly; production и EditMode код после успешного полного EditMode не менялись.
- Critical paths представлены: `RunModelTests`, `HealthTests`, `ExperienceDropTests`/`ExperienceAccountingTests`/draft runtime, ActiveSkill tests, `WaveDirectorTests`, `WaveSpawnerTests`/enemy life registry, `GameplayCompositionSceneTests` и runtime rollback tests, UI presenter, content registry/catalog loading; composed gameplay smoke — PlayMode.
- В ходе разработки исправлены пропущенная скобка нового класса, отсутствие factory в новых set fixture tests и target тестового NavigationSubmitEvent. Эти промежуточные неуспешные запуски не являются verification evidence. Финальные XML/logs относятся к последующим прогонам.
- Manual visual screenshot не выполнялся; UI поведение проверяет реальный PlayMode surface и EditMode view/presenter tests. Geometry/resolution review reusable UI относится к IP-10A.

### Documentation impact

GDD set-control policy, UI §7/§12 shared controls, approved DECISION-0022, DESIGN_SYNC/proposal и IP-07/IP-10/IP-10A/IP-11/IP-19/IP-28 синхронизированы. Execution order и пользовательская stop boundary перед IP-31 не изменены. Поведение production content и баланс не назначались. Текущая готовность и следующий модуль хранятся только в STATUS.
