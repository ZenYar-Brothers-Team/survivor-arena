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
