# IP-16

Date: 2026-09-21. Scope revision: design-sync-R2.
Execution status/order: only [STATUS](../STATUS.md#ip-16--field-definitions-selection-и-run-configuration).

## Implementation

- `Assets/Game/Field/`: validating field/environment definitions, typed optional Traveler
  token, JSON DTO/catalog, profile access boundary, roster and selection session.
- `Assets/Resources/Content/Fields/FixtureFields.json`: default available Open Arena
  и locked Focused Pursuit; authored difficulty 2/1 по fixture шкале 1–5. Новый
  `FixtureFieldWaveTimeline.json` даёт seeker-only вариант без midboss.
- `GameplayCompositionRoot`: character confirm теперь открывает field selection.
  Start проверяет обе доступности, resolves refs, validates scene geometry, creates
  fresh wave/boss owners, возвращает player на spawn. Back сохраняет IDs. Отмена
  выбора не включает компоненты, ожидающие initialization. Existing smoke driver
  обновлён для двух шагов navigation.
- `FieldSelectPresenter`, immutable card state и `FieldSelect.uxml`: title/description,
  authored difficulty, lock reason, text-only thumbnail placeholder, Start/Back,
  semantic IDs и scroll. Новых raster operations/scene edits нет.
- `RunModel.Selection`/`RunOutcome.Selection`: immutable character/field/environment/
  timeline identity до запуска и после terminal cleanup; работает без telemetry.
  Telemetry provenance использует выбранное поле и actual wave timeline/seed.

Оба fixture fields используют существующую player-only Gameplay arena. Новые
production geometry/enemies/bosses/field IDs не поставлялись. Traveler token
валидируется без обратной зависимости; runtime consumer — IP-29. Неподдержанный
non-null token явно отвергается до composition.

## Checks

Runner: `scripts/Test-Unity.ps1`, Unity **6000.6.0f1**. Перед каждым запуском
проверены процессы Unity через `Get-CimInstance Win32_Process`; интерактивный
Editor отсутствовал. Batch `-nographics`, filter `^Game\.`.

Финальный код: **538/538 EditMode, 12/12 PlayMode, 0 failed, 0 skipped**.
Third-party tests: 0. Полные результаты/логи в локальном `TestResults`:

- `IP16-2026-09-21-EditMode.xml`, `IP16-2026-09-21-EditMode.log`
- `IP16-2026-09-21-PlayMode.xml`, `IP16-2026-09-21-PlayMode.log`

Первый compile attempt выявил недостающую `Game.Content` reference в
`Game.Run.Tests`; исправлено до финальных прогонов. Этот attempt не является PASS.

Coverage:

- `FieldConfigurationTests`: различимые configs; missing/wrong environment/timeline/
  boss refs, enemy outside pool, swapped boss roles, hook/definition mismatch;
  typed optional Traveler token, required difficulty/unlock metadata, invalid
  player-only scene collision до run.
- `FieldSelectPresenterTests`: fake access/view/launcher; description/difficulty/
  placeholder/lock reason, access revocation between selection and start, rejected
  start retry, one-shot start, Back selection, unsubscribe, semantic UXML IDs.
- `RunSelectionTests`: bind once before Start, immutable terminal identity, clean new run.
- `FieldSelectionSmokeTests`: реальная Gameplay scene/UI, locked cannot start,
  Back сохраняет field/character; Focused → Shutdown → Open сбрасывает run ID,
  timeline, boss hooks/lives и spawn; actual provenance совпадает; old director
  не запускает encounters нового run. UI elements имеют ненулевой layout.
- Отдельный PlayMode cancel-before-launch guard: adapters остаются выключенными,
  после двух frames нет uninitialized Start, последующий запуск работоспособен.
- Existing `GameplaySceneIntegrationTests.Field_HasClosedBoundsAndFixtureObstacle`
  и `PlayerObstacleCollisionTests` подтверждают player-only geometry. Enemy/projectile
  collision code не изменялся. Полный набор сохраняет lifecycle, damage/death,
  XP/draft/active skill, wave/spawn/pool, composition/content/UI и `GameplaySmokeTests`.

JSON syntax и `git diff --check` проверены. Manual visual approval новых thumbnails
не заявлен: это текстовые placeholders. Gameplay density review IP-12A не закрывается
этими тестами.

## Documentation and decisions

[IP-16 API/schema](../modules/IP-16-field-framework.md#framework-api-и-fixture-schema),
consumer contracts IP-23/IP-24/IP-25/IP-26/IP-29 и readiness синхронизированы.
[DECISION-0032](../../decisions/0032-field-run-configuration.md) — Proposed technical
record, не утверждение нового product rule.

G-20 выявлен при чтении канона: UI/IP шкала 1–5, CD cards 1–10. Вопрос пользователю
задан; ответа/approval на изменение шкалы нет. Fixtures сохраняют 1–5, production
mapping не выдуман. GDD/CD/art не изменены. После этого packet Ready нет;
следующий planning packet — G-10 для IP-28 либо CG-03/G-15 для IP-25.
