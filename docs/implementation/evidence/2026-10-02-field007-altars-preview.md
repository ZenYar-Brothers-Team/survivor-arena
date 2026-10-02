# FIELD-007 — evidence алтарного просмотра

Дата: 2026-10-02. Scope и approved отклонения: [DECISION-0145](../../decisions/0145-field007-altars-preview.md). Текущий статус только в [STATUS](../STATUS.md).

## Подключение

Authoring — `docs/balance/field007-altars-v1.json`; штатный generator выпускает FIELD-007 environment/selection и presentation с собственной землёй/миниатюрой. Временно используются encounters первого поля. Fresh profile закрывает карту; существующий Dev unlock открывает её без нового обходного флага.

36 фиксированных после старта центров: 12 позитивных циклических, 12 негативных циклических, 12 святынь; arena 160×160. Новый seed меняет раскладку, reference seed повторяет. Sliding-window cap учитывает стартовую камеру и padding 2 с каждой стороны; это лимит оснований, а не пересечения областей действия. После изменения zoom/aspect посреди забега раскладка сохраняется.

Три prepared Prop имеют source master/version/provenance, runtime PNG, import profiles, manifest и typed refs. Art packet `Art/Packets/field007-altars-v1.json`: pipeline `--apply` подготовил 15 записей/файлов. Body visual-only без collider; непрерывная видимость в near window, яркость активности, boundary и charge/rest arc используют zone clock. Старый dev-набор алтарей сохраняется отдельно.

## Наблюдаемые результаты

- `python scripts/content/generate.py --check`: UP TO DATE. Content scope: STATIC PASS, не заменяет runtime проверки.
- Общий запуск `TestResults/checks/20261002T093538-672136Z`: EditMode 1346/1346 PASS, в том числе `ProductionMonasteryContentTests` и `ZoneScreenDensityTests`. Расстановка проверена на 32 seeds, во всех скользящих прямоугольниках, с детерминизмом повторного seed.
- В первом общем PlayMode: 64/67; новый monastery smoke проверил spawn раньше первого wave tick. Ожидание исправлено: до первого enemy, максимум 500 FixedUpdates.
- Повторный graphics PlayMode: **1/1 PASS**, `TestResults/checks/20261002T094542-395923Z/summary.json`. Покрывает закрытую карту свежего профиля → существующий Dev unlock → карточку FIELD-007 → запуск; 36 runtime altar views, enemy spawn, стабильность центров, pause, shutdown и отсутствие оставшихся views.
- Повторный полный art scope: **PASS**, EditMode **95/95**, manifest **311/311**, `TestResults/checks/20261002T095125-816894Z/summary.json`. Первый запуск прерван изменением inputs параллельной работой; промежуточный обнаружил interactive Editor без REST. После его самостоятельного закрытия safe runner провёл повторный batch. Открытый Editor не закрывался агентом и batch поверх него не запускался.
- `git diff --check`: нет whitespace errors.

Неисправленные сбои общего PlayMode вне этого поднабора:

- `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`: Expected 70, actual 85.
- `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`: `field-select-FIELD-DEV-ZONES` extends to 1250; expected ≤1081.

Общий full verdict не объявляется PASS. Эти два UI failures уже были в baseline до алтарного просмотра.

## Визуальные наблюдения и ограничения

Graphics smoke сохранил `TestResults/field007-positive.png`, `field007-negative.png`, `field007-shrine.png`. На снимках видны назначенная земля, общий жезл с открытой молнией, разные основания и святыня. Персонаж стоит непосредственно на центре и частично перекрывает основание. Граница действия тонкая, у крупных cyclic radii выходит за экран; это gameplay radius, не масштаб body. Видимые стыки ground tile требуют отдельного художественного просмотра существующей текстуры; её raster этим проходом не изменялся.

User acceptance в игровом масштабе, окончательные monastery encounters/баланс и decor/obstacles остаются открытыми. Подготовленный просмотр не объявляет полную FIELD-007 или IP-23 Verified.

## Feedback v2: мини-карта, радиусы и физическое основание

Радиус каждой зоны выбирается один раз на забег в диапазоне [Rmax/3, Rmax], где Rmax = 1.125 прежнего радиуса. Containment, clearance, strike centers, world boundary и мини-карта используют одно фактическое значение. Мини-карта показывает все 36 объектов вне зависимости от near window, различает round/toothed основания и active/rest яркость. Физический контакт создаётся scene adapter отдельно от visual-only body, сохраняется вне видимого окна и блокирует только игрока.

- Свежий safe graphics runner `TestResults/checks/20261002T132952-328179Z/summary.json`: **118/118 EditMode + 1/1 PlayMode PASS**, генерация UP TO DATE. Scoped filter включает Zones, MapPreviewPresenter, ProductionMonasteryContent, FieldMapPreviewSource и ProductionMonasterySmoke.
- Regression проверяет детерминированные разнообразные радиусы, отношение max/min 3, clearance и strike circles внутри actual radius; передачу всех отметок мини-карте; два размера non-trigger collider и player-only mask. Четыре physics cases подтверждают, что при движении игрок останавливается перед обоими основаниями, а enemy layer проходит.
- Предыдущий smoke искусственно телепортировал player внутрь collider и проверял нулевое пересечение; это не проверка движения. Capture position перенесена ниже основания, допустимая глубина контакта учитывает solver tolerance; обычное движение проверено отдельно. Первый повторный physics test прошёл contact assertions, но ошибся при восстановлении пустого SceneManagerSetup; cleanup исправлен, затем выполнен свежий проход выше.
- `TestResults/field007-minimap.png` просмотрен: 36 round/toothed отметок, фактические круги разных размеров и рамка камеры читаются; shrine и player на игровом поле не перекрывают основание на этом capture. Positive/negative/shrine captures обновлены тем же smoke.
- `git diff --check`: whitespace errors отсутствуют. Существующие raster assets не изменялись.

По референсам GW2 capture points и PoE shrines подготовлено [предложение](../proposals/2026-10-02-altar-state-presentation.md): небольшой наземный clock ring и локальный свет навершия. Это авторская адаптация; новое состояние presentation и перенос кольца этим проходом не реализованы.

## Feedback v3: компактное поле и утверждённая индикация

Пользователь изменил размер на 120×120 и прямо поручил реализовать предложенную индикацию. Число 36, состав, actual radii, screen cap и collider сохраняются. Generated presentation пересобрана из authoring. Маленький фиксированный эллиптический clock ring обрамляет основание, большой contour сохраняет только radius. Отдельный radial mesh над молнией/реликвией светится в authoritative active state, с 0.22 с вспышкой на переходах; body не перекрашивается. Во время отдыха дуга заполняется, во время активного циклического окна опустошается; shrine использует charge/cooldown. Нет изменения raster assets или gameplay timing.

- Safe graphics runner `TestResults/checks/20261002T135934-152422Z/summary.json`: **10/10 EditMode + 1/1 PlayMode PASS**, генерация UP TO DATE. Scope: ProductionMonasteryContentTests и ProductionMonasterySmokeTests.
- CompactArena проверил все 36 размещений на 32 seeds и все sliding rectangles стартового screen 24×14 с padding 2: максимум три основания. State test проверил небольшой radius независимо от effect radius, half rest/active progress, отсутствие света в отдыхе, steady/flash light, стабильный body color и pause.
- Первая попытка теста встретила nullable ArenaSideLength в аргументе Generate; исправлено явным Value обязательного для FIELD-007 размера, затем выполнен свежий проход выше.
- Просмотрен обновлённый `TestResults/field007-positive.png`: небольшое кольцо окружает основание, молния с локальным светом, большой actual-radius contour отдельно. Mini-map/shrine/negative captures также обновлены игровым smoke. Окончательная художественная оценка остаётся пользовательской.
