# Иконки low-tier SET-021…035 — поставка 2026-10-02

Пользователь утвердил весь показанный набор: «утверждаю, встраивай в игру».
[Review](../proposals/2026-10-02-low-tier-set-icons/review.md) содержит общий лист и точные briefs;
[approved packet](../../../Art/Packets/low-tier-set-icons-2026-10-02.json) — hashes и approval.

AI выполнил `scripts/art_pipeline.py` plan и apply: 15 immutable sources/master/provenance,
15 runtime PNG 256×256, registry Icon references и manifest records. Runtime paths:
`Assets/Resources/Art/UI/Icons/Sets/set-021-icon.png`…`set-035-icon.png`.
Unity создала `.meta`; общий icon profile — center pivot, maxSize 256, FullRect,
Bilinear, Clamp, sRGB, без mipmaps, Uncompressed. Masters 1254×1254 не изменены;
fit использует padding 16, cropAlpha true и alphaNoiseCutoff 2.

Authoring `scripts/content/progression.py` назначает `iconVisualId` всем low-tier sets и
двум set-attack templates. `ProductionSets.json` и `ProductionSetAttacks.json` regenerated;
проверки каталога теперь требуют иконки у всех 35 сетов. UI потребляет существующую typed icon reference;
новых UI/layout/gameplay rules нет.

## Проверки

- Unity 6000.6.0f1, безопасный runner, закрытый Editor → batch.
- `check_project.py --scope art`: 80/80 EditMode PASS, 0 failed/skipped;
  `TestResults/checks/20261001T220550-172545Z/summary.json`.
- `ProductionSetCatalogTests`: 6/6 EditMode PASS, 0 failed/skipped;
  `TestResults/checks/20261001T220656-680116Z/summary.json`.
- generation `--check`: UP TO DATE; low-set packet validator: 15/15, 14 стартовых, PASS.
- Manifest integrity: 308 owner/role records PASS. После тестов изменены только
  art stage/evidence и описания icon contract в документации, без изменения runtime payload.
- Первый art run обнаружил устаревший expectedCount=20; исправлен до 35, свежий повтор выше PASS.

## Что ещё проверить или доделать

Каталожные рецепты и механики уже присутствуют; иконки не заменяют runtime-приёмку эффектов.
SET-021-ATTACK пока без world `visualId`, поэтому использует общий projectile fallback;
можно подключить подходящий existing stone sprite либо подготовить собственный.
SET-022 в `ExecuteArea` применяет конус/урон/нокбэк, но не рисует короткую дугу-конус;
для него нужен краткий visual-only effect по карточке, без persistent entities.
Остальные сеты по approved cards используют существующие visuals и не требуют 13 новых world sprites.

Отдельно нужен игровой прогон покупки/access → пороги → предложение/получение каждого нового сета →
эффект в бою, совместной работы нескольких сетов, pause и terminal cleanup. Текущие EditMode
проверки не являются этим прогоном. PlayMode/manual review именно пакета low-tier в этой задаче
не выполнялись. Цены 150/100 и стартовые числа требуют плейтеста; для остальных 13 сетов
конкретного отсутствующего production effect в рамках этого осмотра не обнаружено.

Текущий status/order и остающиеся gates — только [STATUS](../STATUS.md).
