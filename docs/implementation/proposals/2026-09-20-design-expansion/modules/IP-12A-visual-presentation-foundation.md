# IP-12A — Visual Presentation Foundation и asset production pipeline

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-12A-visual-presentation-foundation.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Сохранить approved fixture goblin/pipeline/VisualRoot/single writer. Расширить этот же модуль category profiles, generic adapters и inventory audit; выполненную player основу не писать заново.

## Зависимости

[IP-00](IP-00-content-contract.md), [IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md), [IP-08](IP-08-active-skill-framework.md), [IP-12](IP-12-character-framework.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

Approved Art Direction v2 §§2,4,6,7,9,11,12,15–19; ASSET_PIPELINE §§3–9,12–20; Art Production §§1–19; UI §§20–23; DECISION-0007/0013; importer/catalog/provenance/rig/runtime.

## Scope

Category-aware world/UI/VFX import/validation, explicit PPU/pivot/size overrides; body-derived crops, stable visual-role refs; manifest owner/role/source/runtime/Generate-Procedural-Hybrid and evidence. Reconcile existing FIXTURE body vs approved CHAR-001 concept без false relabel. Generic child presentation/pool adapters и первый ограниченный fixture kit enemy/projectile/pickup/telegraph/shadow/impact; authoritative motion/hit signals входят через interfaces. Complete common pause/reset/death/collect/proc presentation policy; screen shake preference consumed by IP-26 service.

## Out of Scope

Массовый production catalog, новые gameplay mechanics ради арта, gameplay knockback как sprite recoil, Addressables/atlas migration без измерений, marketing/Steam/music/skeletal animation.

## Acceptance criteria

Preview/master/provenance вне Assets; approved runtime derivative, path/GUID retained on replacement. UI icon не получает body ground pivot; profile reimport сохраняет согласованный override. По проверенному representative world/UI/VFX asset; missing role/resource rejected. Independent motion channels compose, gameplay root/collider не меняются; pause freezes, Shutdown/pool return restore baseline. Inventory stages соответствуют files/review evidence. Наличие ShadowRenderer без sprite не равно готовому shadow asset.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Bounded DEV showcase или обоснованная diagnostic view: idle/flip/hit/pause/reset, target-scale и dense effects. Численные motion/VFX limits — validated config, не hardcoded gameplay.

## Проверки

Import/ref/provenance/path/GUID tests; alpha/bounds где автоматизируемо; adapters/root invariance/pool/rollback; manual silhouette/edges/contrast/motion и 3–4-set density после появления effects. Pixel quality не объявлять проверенной без просмотра.

## Документационные изменения

Полная замена Art Direction — M-01; уточнения ASSET_PIPELINE/category checklists, reconciled Art Production inventory и source mapping; presentation lifecycle DECISION при новом cross-layer policy.

## Gates и недостающие решения

G-17/G-18: сопоставить approved CHAR-001 concept с master и исправить always-green vs nongreen family conflict; per-image replacement/approval gates остаются. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-17](IP-17-production-skills.md), [IP-18](IP-18-production-passives.md), [IP-19](IP-19-production-sets.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-22](IP-22-production-characters.md), [IP-23](IP-23-production-fields.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-30](IP-30-production-travelers.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
