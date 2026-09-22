# IP-12A — Visual Presentation Foundation и asset production pipeline

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить approved fixture goblin/pipeline/VisualRoot/single writer. Расширить этот же модуль category profiles, generic adapters и inventory audit; выполненную player основу не писать заново.

## Зависимости

[IP-00](IP-00-content-contract.md), [IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md), [IP-08](IP-08-active-skill-framework.md), [IP-12](IP-12-character-framework.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

Approved Art Direction §§2,4,6,7,9,11,12,15–19; ASSET_PIPELINE §§3–9,12–20; Art Production §§1–19; UI §§20–23; DECISION-0007/0013; importer/catalog/provenance/rig/runtime.

## Scope

Category-aware world/UI/VFX import/validation, explicit PPU/pivot/size overrides; body-derived crops, stable visual-role refs; manifest owner/role/source/runtime/Generate-Procedural-Hybrid and evidence. Reconcile existing FIXTURE body vs approved CHAR-001 concept без false relabel. Generic child presentation/pool adapters и первый ограниченный fixture kit enemy/projectile/pickup/telegraph/shadow/impact; authoritative motion/hit signals входят через interfaces. Complete common pause/reset/death/collect/proc presentation policy; screen shake preference consumed by IP-26 service.

## Out of Scope

Массовый production catalog, новые gameplay mechanics ради арта, gameplay knockback как sprite recoil, Addressables/atlas migration без измерений, marketing/Steam/music/skeletal animation.

## Acceptance criteria

Preview/master/provenance вне Assets; approved runtime derivative, path/GUID retained on replacement. UI icon не получает body ground pivot; profile reimport сохраняет согласованный override. По проверенному representative world/UI/VFX asset; missing role/resource rejected. Independent motion channels compose, gameplay root/collider не меняются; pause freezes, Shutdown/pool return restore baseline. Inventory stages соответствуют files/review evidence. Наличие ShadowRenderer без sprite не равно готовому shadow asset.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Bounded DEV showcase или обоснованная diagnostic view: idle/flip/hit/pause/reset, target-scale и dense effects. Численные motion/VFX limits — validated config, не hardcoded gameplay.

## Проверки

Import/ref/provenance/path/GUID tests; alpha/bounds где автоматизируемо; adapters/root invariance/pool/rollback; manual silhouette/edges/contrast/motion и 3–4-set density после появления effects. Pixel quality не объявлять проверенной без просмотра.

## Документационные изменения

Полная замена Art Direction выполнена в M-01; уточнения ASSET_PIPELINE/category checklists, reconciled Art Production inventory и source mapping; presentation lifecycle DECISION при новом cross-layer policy.

## Gates и недостающие решения

G-17 concept mapping подтверждён пользователем в DECISION-0029 и asset-record; per-image replacement/approval gates остаются. G-18 согласован [DECISION-0029](../../decisions/0029-burst-pressure-and-player-palette.md): зелёная skin-family относится к текущим гоблинам, другие виды сохраняют свою палитру при читаемом player silhouette. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-17](IP-17-production-skills.md), [IP-18](IP-18-production-passives.md), [IP-19](IP-19-production-sets.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-22](IP-22-production-characters.md), [IP-23](IP-23-production-fields.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-30](IP-30-production-travelers.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт технического пакета

Scoped follow-up: [DECISION-0039](../../decisions/0039-conservative-body-contact-circles.md) разрешает conservative circle authoring для текущих goblin/villager. IP-02 применяет player circle, IP-04 — enemy circle; presentation смещает body относительно центра, не анимирует collider. Evidence и пользовательский повторный review: [contact circles](../evidence/2026-09-22-body-contact-circles.md).

Shared enemy death follow-up: [DECISION-0040](../../decisions/0040-shared-enemy-death-presentation.md) задаёт один config-driven visual tail для обычных врагов, боссов и Travelers. Presentation клонирует активный renderer, делает squash/shrink/fade/dust без death push, уважает pause и полностью очищается при pool reuse. [Evidence](../evidence/2026-09-22-shared-enemy-death.md).

Category/import overrides, role validation, crop ownership и generic adapter API описаны в [ASSET_PIPELINE §21](../../art/ASSET_PIPELINE.md#21-category-profiles-и-reusable-adapters-ip-12a), [DECISION-0030](../../decisions/0030-generic-presentation-adapters.md) и [asset manifest](../../../Art/asset-manifest.json). Existing player animator не заменяется. Fixture kit и Editor diagnostic не подменяют production binding и manual dense-gameplay review. Gameplay consumers передают motion/signals через IPresentationSource; визуальный fade не задерживает их authoritative teardown. IP-26 потребляет preference/request boundary для screen shake, сохраняя ownership Settings/camera.

## Settings consumer policy

[DECISION-0038](../../decisions/0038-settings-and-field-difficulty.md) задаёт G-16:
IP-26 владеет persistent preference и bounded camera offset consumer. IP-12A
предоставляет прежний IScreenShakePreference/ScreenShakeRequestGate; actual-damage
trigger, pause/off/terminal reset и независимость gameplay camera queries проверяет
IP-26. Gameplay density review этого модуля остаётся отдельным acceptance.


## Потребитель — стартовый FIELD-001

Существующие presentation adapters и approved asset evidence переиспользуются пакетами первой карты. Оставшийся gameplay density review проверяется на production FIELD-001 в F1-09, без повторного approval неизменённых изображений.

Scope — [field-001-start-R1](../milestones/FIELD-001-start.md);
порядок, packet readiness и evidence — только [STATUS](../STATUS.md#field001-execution).
