# Meta stat icons — 2026-09-29

Основание: запрос простых иконок перед названиями и [approval](../proposals/ui-meta-icons-r1/approval.md).
Execution status — STATUS. Пакет Art/Packets/ui-meta-stat-icons-r1.json содержит
SHA256, точные prompts и constraints/references. Встроенный image generation;
12 отдельных approved candidates, без изменения gameplay/цен/лимитов.

art_pipeline.py plan/apply: 50 файлов — immutable versions, masters/provenance,
runtime PNG 256×256, sprite registry и manifest. Unity создал .meta; общий UI
import profile: Sprite/Single/FullRect/Bilinear/Clamp, alpha, no mipmaps/read-write,
uncompressed, max 256. Исходные PNG не редактировались после approval;
fit/cropAlpha/padding 16 — техническая подготовка по packet.

RuntimeContentCatalog включает visual ID каждого production MetaUpgrade по
owner-role convention. MetaShopProjection проверяет SpriteRole.Icon и передаёт
Sprite в immutable viewstate; view не грузит source paths. Isolated fixtures без
registry сохраняют текст. Image 32×32 слева от title; абсолютный slot и padding
заголовка не увеличивают его высоту и не пересоздают карточки при toggle.

## Проверки

- Первая попытка full: 20260929T073800-056153Z — NOT RUN, CS0136 из-за локального
  имени row в новом тесте; исправлено имя.
- Следующий full: 20260929T073830-813121Z — 947/947 EditMode, 38/39 PlayMode.
  MetaShopSmokeTests поймал отсутствие META-003 в runtime registry. Исправлено
  включение meta icons в catalog; добавлен ProductionCatalog_ContainsEveryPersonalUpgradeIcon.
- Финальный `python scripts/check_project.py --scope full --graphics`:
  948/948 EditMode, 39/39 PlayMode, 0 failed/skipped, third-party 0.
  Unity 6000.6.0f1, закрытый Editor → safe batch; generation актуален,
  audio 28 файлов PASS, art 268 записей PASS.
  `TestResults/checks/20260929T074141-306064Z/summary.json`, XML/logs рядом.

Просмотрены свежие meta-personal-1280x720.png и meta-personal-1920x1080.png
в TestResults: все 12 значков видны, названия и уровни не пересекаются,
высота карточек сохранена. Tests проверяют ненулевые sprites, slot 32 px,
позицию перед title, scroll/toggle/refund/counters. Ручное принятие runtime
пользователем не заявляется. После обновления стадии manifest выполнен статический audit.
