# IP-23 — Production Fields FIELD-001…010

Уточнение FIELD-007: алтари резервируются до генерации предметов; obstacle seed не влияет на выбранные алтари. Предметы обходят foundation circles. Другие поля сохраняют порядок.

FIELD-007: по прямому поручению подключены шесть approved obstacle Prop с плотностью FIELD-001; область эффекта может накрывать предметы, foundation clearance отдельно от effect radius. [DECISION-0146](../../decisions/0146-monastery-existing-obstacles.md).

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Спецификация описывает целевой scope и заменяет прежний packet. Текущая реализация, выполненные поднаборы и оставшиеся gates — только в [STATUS](../STATUS.md).

## Зависимости

[IP-16](IP-16-field-framework.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Арт текущих круговых платформ и прямых мостов FIELD-009 по поручению
уточнён отзывом о меньшей яркости и святой земле вместо неба:
[DECISION-0147](../../decisions/0147-field009-holy-ground-art.md). Concept A v3
утверждён пользователем для подключения; [approved packet](../../../Art/Packets/field009-platform-art-v2-approved.json)
и platformLayout.art задают выборку материала из принятых pixels.
Прежний cloud/reuse draft не применяется. Gameplay visual review отдельно.

Первоначальная подготовка по поручению
пользователя 2026-10-02: [brief](../../art/briefs/field009-platform-art-v1.md)
и [review packet](../../../Art/Candidates/field009-platform-art-2026-10-02/README.md).
Основа — существующий [platform profile](../../balance/field009-platforms-v1.json)
и полная FIELD-009 card. Визуальное подключение сохраняет geometry и books/damage rules;
выбранные raster pixels прошли пользовательское утверждение до import.

Набор алтарей FIELD-007: [DECISION-0135](../../decisions/0135-altars-mechanic.md) и утверждённый просмотр непосредственно на карте по [DECISION-0145](../../decisions/0145-field007-altars-preview.md). Только позитивные и негативные основания, без нейтральных алтарей и визуального разделения по эффектам; накопление отложено. Размер 160×160 и состав 12 + 12 + 12 утверждены пользователем.

Концепт road-only сети FIELD-003, тупиков и книг с одним улучшением:
[DECISION-0137](../../decisions/0137-field003-road-network-concept.md).
Вариант v4 принят пользователем 2026-10-02 и сохранён как
[референсный пакет](../../prototypes/field003-roads/approved-v4/README.md).
[План FIELD-003](../milestones/FIELD-003-road-network.md) связывает геометрию
с movement IP-02 и pickup/draft lifecycle IP-28/IP-30. Принятие схемы не
означает завершение runtime packet. По уточнению пользователя 2026-10-02
реализация идёт прямо на FIELD-003, без dev-поля; сущности и награды на траве
сохраняют текущее поведение/доступность. Книги тупиков используют общий Book
lifecycle и монеты с явным override ровно одного выбора.

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
200×200 geometry, 64 obstacle rects и metadata.
Packet Approved 2026-09-24 по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
используется как production data; проверки этого IP сохраняются.
Плотность и набор препятствий FIELD-001 после пользовательского отзыва пересмотрены в [DECISION-0069](../../decisions/0069-field001-feedback-tuning.md).
Гарантия двух удалённых от spawn объектов в стартовом кадре FIELD-001 — [DECISION-0070](../../decisions/0070-field001-opening-screen-obstacles.md).

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD/CD fields, полные выбранные FIELD-001…010 и referenced entities; UI §5; Art Production §§11–13; DECISION-0003.

Field/run configuration API — [IP-16](IP-16-field-framework.md#framework-api-и-fixture-schema).
Production environment adapter заменяет fixture scene-name binding; metadata/refs
проходят тот же pre-run validation. G-20 resolved по DECISION-0038: переносить
explicit difficulty 1–5 из CD; автоматического mapping по ID нет.

## Scope

FIELD-007 altar preview: [DECISION-0145](../../decisions/0145-field007-altars-preview.md), [authoring](../../balance/field007-altars-v1.json). Стабильная production-карточка после существующей Dev-разблокировки, назначенная земля/миниатюра, временные encounters FIELD-001, 36 случайных алтарей и sliding-window cap три на экран. Пререквизиты этого поднабора — field/profile framework, Zones foundation и существующие encounters первого поля; финальные monastery encounters и полная приёмка поздней карты сохраняются отдельными gates.

десять geometry/environment definitions, approved enemy/boss/midboss mapping и unlock/difficulty metadata; ground treatment, нужный decor/obstacle pack и derived thumbnail. Число obstacles определяется gameplay geometry; декоративные props не получают collider автоматически.

## Out of Scope

самостоятельные wave schedules, заранее фиксированное число препятствий на поле, сложный tileset без необходимости.

## Acceptance criteria

selected field загружает correct geometry/environment; ordinary boundaries/obstacles блокируют только игрока; references валидны. Difficulty 1–5 задана явно; thumbnail отражает поле; безопасное направление движения читается, props не маскируют опасности. Неполная geometry/size/card data отмечена per-field.

По DECISION-0102 внешний периметр физически ограничивает игрока, но не рисует
сплошной забор; внутренние obstacle props сохраняют собственный арт и коллайдеры.
По [DECISION-0126](../../decisions/0126-obstacle-transparent-padding-contact.md)
player-only collider prop препятствия охватывает импортированную видимую форму,
без упора в прозрачные края; после поворота стены прямоугольник поворачивается
вместе со спрайтом.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Академия FIELD-006 использует mesh-печати и включение при полном свете по [DECISION-0142](../../decisions/0142-academy-zone-seal-presentation.md), потребитель IP-12A. По прямому поручению пользователя 2026-10-02 дальнейшая настройка идёт на этом постоянном ID: собственные ground/thumbnail/environment, временно общее расписание FIELD-001 и его encounters, доступ через существующий Dev unlock. `field006-zones-v1.json` — источник зон Академии; «Тест 06» сохраняется для регрессий. Это не закрывает финальные enemy profile, геометрию проходов и баланс FIELD-006; циклы «Тест 07» сохраняют отдельный контракт алтарей.

name/description/difficulty/lock condition/thumbnail + loading state; технические wave timings/boss stats не перегружают selection.

## Проверки

Академия preview v2 по уточнению пользователя: без внутренних obstacles/decor,
все виды временные; fixed и random appearance, синхронные fixed порталы;
area-only effects не показывают unit status, lasting effects сохраняют таймер.
Контракт и проверка новых portal/slow/presentation API — DECISION-0142.

per-field load/selection/ref validation и collision, restart/reload cleanup; manual density/contrast на реальном camera scale и thumbnail review.

## Документационные изменения

field→kit→roles mapping, geometry constraints, metadata completeness, bindings владельцев IP-24/IP-30.

## Gates и недостающие решения

G-14: geometry/enemy pools; G-20 resolved по DECISION-0038 (difficulty 1–5). G-15 resolved по DECISION-0037, альтернативные условия открытия — DECISION-0125 и CD; они не создают отсутствующие gameplay definitions. Весь approved mapping переносится, numeric schedules отдельно. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-08](../milestones/FIELD-001-start.md#f1-08):
FIELD-001 geometry/environment/metadata/thumbnail. Packet prerequisites: F1-00…07; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).

Поправка [DECISION-0052](../../decisions/0052-field001-six-ordinary-enemies.md):
ordinary pool FIELD-001 — ENEMY-001…005 и ENEMY-007, ровно шесть типов.
F1-04 поставляет их definitions/art; F1-08 связывает все шесть с timeline,
F1-09 проверяет совместную читаемость и давление. Боссы/Путники считаются отдельно.
