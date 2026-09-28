# UI «Полевой фолиант» — первый стилевой проход и ограничения evidence

Date: 2026-09-28
Related: [DECISION-0081](../../decisions/0081-field-folio-ui-visual-language.md),
[IP-26](../modules/IP-26-functional-ui.md)

## Результат

Подготовлен первый стилевой проход существующего `HUD → Draft → Pause / Build`.
Capture harness отрисовал его при `1920×1080` и `1280×720`; это fixture-кадры,
а не демонстрация чистовой production composition:

- общая угольно-сливовая палитра, parchment text, rust/gold/moss/azure/violet
  семантические акценты и матовые панели;
- USS-правила normal/hover/focus/pressed/selected/disabled/locked и короткие
  transitions для карточек/кнопок; наличие правил не доказывает прохождение
  полной интерактивной state/motion матрицы;
- Draft type marker и semantic ID, крупная icon well, recipe/completing/set
  акценты;
- Pause / Build разделён на Active, Passive, acquired sets, recipe progress и
  missed sets; cards получают role-specific markers;
- HUD получил HP/XP semantic colors, но сохранил временную раскладку и
  speed controls вне DEV;
- подключены runtime-шрифты с кириллицей: `Alegreya Sans Medium` для заголовков
  (ближайший существующий вес семейства к утверждённому SemiBold) и `PT Sans`
  для основного текста. Оба распространяются по SIL Open Font License, тексты
  лицензий лежат рядом с файлами.

Font sources: Google Fonts repository, `ofl/alegreyasans` и `ofl/ptsans`.
SHA-256: `AlegreyaSans-Medium.ttf` —
`4B89FE7804FD1485EC2757795A53FFDB66E1206DD56F844C2D72B3C944815B43`;
`PTSans-Regular.ttf` —
`9CC831490532009BAE2B3CE0D39C62ADFC889060BEB421593BFD9D2396D0F10A`.

## Проверки

- UI EditMode: **65/65 PASS**, 0 skipped —
  `TestResults/checks/20260928T084649-324971Z/summary.json`.
- Responsive PlayMode smoke: **1/1 PASS**, 0 skipped —
  `TestResults/checks/20260928T084720-747076Z/summary.json`.
- Дополнительный DX11 capture того же PlayMode harness: **1/1 PASS** —
  `TestResults/ui-field-folio-PlayMode.xml`.
- Просмотрены свежие HUD, Draft, Pause и scrolled Pause/Build PNG при обоих
  целевых разрешениях. В просмотренных состояниях явного overlap не обнаружено;
  тестовые assertions проверяют лишь часть bounds и submit, не всю state matrix.

## Ограничения и результат review 2026-09-28

- Harness использует однотонный фон, synthetic labels/длинный текст и пустые icon
  wells; production-иконки и type markers в этих captures не продемонстрированы.
- Показан Book fixture, а не полный production level-up flow. Timer в HUD fixture
  показывает 00:00, что не является демонстрацией начала production run.
- Pause harness не включает AppShell Settings/Quit. Его один Resume не доказывает
  геометрию всей Pause-композиции.
- Пользователь принял направление стиля, но потребовал пересмотреть layout и
  плотность текста; уточнил DEV-only скорость и HP возле персонажа.
- Утверждение предыдущего отчёта «чистовой vertical slice готов» было слишком
  широким для этого evidence. Tests подтверждают проверенный fixture scope;
  новую раскладку, release gating и HP anchoring они не проверяли.

Основание корректировки: [DECISION-0083](../../decisions/0083-player-ui-layout-and-dev-boundary.md).
Предложение переработки — [layout R2](../proposals/2026-09-28-ui-layout-r2.md).
Актуальная приёмка и остаток работ ведутся только в [STATUS](../STATUS.md).
