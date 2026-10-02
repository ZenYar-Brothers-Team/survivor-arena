# Импульсная волна и Хлопушка — визуальная доработка, 2026-10-02

Пользователь утвердил предложенное направление: «Давай сделай это всё».
Исходный отзыв: эффекты выглядят как светлый круг/дуга; нужны разные цвета,
более выразительная форма и более плотная заливка у внешнего края.

## Реализация

- SKILL-004: бирюзовое кольцо, светлый внешний гребень, прозрачный хвост внутрь,
  небольшая нерегулярность контура и восемь коротких радиальных штрихов.
- SET-022-ATTACK: медово-янтарный серп с кремовым гребнем, сужением к концам,
  пятью штрихами и быстрым выбросом с замедлением к краю.
- `PressureWaveSprites` создаёт две 512×512 runtime-маски на профиль/угол:
  гребень и хвост. Они cached на время жизни presenter, после Dispose освобождаются.
  Два renderer берутся из существующего pool; `Clear` возвращает оба.
- У хвоста отдельная степень затухания: он растворяется быстрее гребня.
  У кольца authoritative expansion остаётся в executor; у хлопушки только
  presentation travel использует ease-out. Pause не продвигает время.
- Центр прозрачен, весь рисунок находится внутри переданного радиуса/угла.
  Damage, knockback, targeting, seeded random, colliders и cooldowns не меняются.
  У Хлопушки damage остаётся мгновенным, до визуального движения.

Настройки принадлежат `SkillWorldEffects.json` и validated profile:
`bandFraction` — ширина полосы / радиус (0…0.5; 0 отключает доработанную форму),
`contourVariation` — смещение края / ширина полосы (0…0.25),
`accentCount` — число штрихов (0…12),
`accentLengthFraction` — длина штриха / радиус (0…0.5),
`accentWidthRadians` — угловая полуширина штриха (0…0.1 радиана),
`tailFadePower` — степень alpha при затухании (1…8 для pressure band).
Другие profiles сохраняют нейтральные нули и старый renderer path.
Пример: при радиусе 3.5 и `bandFraction=0.2` центральная ширина серпа 0.7 world units;
`accentLengthFraction=0.12` даёт штрих длиной 0.42 units.

Новых raster files, immutable masters, runtime PNG и replacements нет:
`art_pipeline.py` принимает только packets с реальными approved input PNG,
поэтому preparation/apply packet к этой процедурной доработке не применяется.
Art manifest проверяется общим runner. Каталог напрямую authoring; он не входит в TARGETS генератора.

## Проверки

Первый scoped EditMode: 79/81, два mask-теста не прошли из-за GPU readback
в `-nographics`. Тест исправлен на прямое чтение пикселей: CPU-копия сохраняется
только по явному `keepReadable` для теста; runtime освобождает её после upload.

Unity 6000.6.0f1, безопасный batch runner при закрытом Editor,
`python scripts/check_project.py --scope full --graphics`:

- EditMode: 1296/1296, 0 failed/skipped.
- В затронутом scope: PressureWaveSpritesTests 4/4, LowTierSetMechanicsTests 11/11,
  ProductionSkillPatternTests 7/7, ProductionActiveSkillCatalogTests 10/10.
- PlayMode: 62/64, 2 failed, 0 skipped; ActiveSkillPatternSmokeTests 2/2 PASS
  (в том числе production SET-021/022, pause/stop/pool).
- Общий full verdict: FAIL. Полные имена ошибок вне pressure-wave scope:
  `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`
  — expected 70, actual 85; тест фиксирует старое число unlocks до low-tier каталога.
  `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`
  — `field-select-FIELD-DEV-ZONES` bottom 1250 > 1081.
- XML/log: `TestResults/checks/20261002T054924-453412Z/EditMode.xml`,
  `PlayMode.xml` и соседние logs. Runner остановился на PlayMode failures,
  поэтому общий reusable PASS/summary не заявляется.
- Generation UP TO DATE; audio validator PASS (28 files / 15 cues).
  Отдельный `python scripts/validate-art-manifest.py`: 308 records PASS.
- Scoped `git diff --check` PASS.

Ручная художественная оценка в плотном бою этими тестами не заменяется.
Текущая приёмка и очередь находятся только в [STATUS](../STATUS.md).

## Documentation impact

Art Direction §12 и Art Production описывают утверждённый визуальный язык.
Продуктовые правила и scope IP не меняются; отдельная архитектурная deviation отсутствует.
