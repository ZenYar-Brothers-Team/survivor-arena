# FIELD-009: подключение утверждённого арта

2026-10-02. Пользователь утвердил concept A v3 и поручил подключить его
и сделать коммит. Визуальный контракт: [DECISION-0147](../../decisions/0147-field009-holy-ground-art.md).
Текущее исполнение — только в [STATUS](../STATUS.md).

Полная reference texture сохраняет принятые pixels. Shader зеркально
повторяет отдельные material-only области камня и золотистой святой земли.
Общий distance field кругов и мостов даёт низкую кромку, тонкую вставку
и нижнюю грань; в устьях мостов нет замкнутых колец. Geometry, walkability,
damage и книги сохранены. Коллайдер на декоративную кромку не добавляется.

Первоначальная выборка земли захватывала край платформы, повторявшийся
как тёмные ромбы. Верхняя граница исходного участка уменьшена до y=130;
свежий общий кадр проверен визуально, regression test проверяет каждый
pixel выборки на отсутствие холодных фрагментов платформ.

Проверки выполнялись безопасным scoped runner без batch поверх открытого Editor:

- `scripts/art_pipeline.py`: approved packet PLAN/APPLY, 7 подготовленных файлов.
- `scripts/check_project.py --scope art`: 112/112 EditMode, manifest 317/317 PASS;
  `TestResults/checks/20261002T150621-527276Z/summary.json`.
- Graphics ProductionField009SmokeTests: 1/1 PASS;
  `TestResults/checks/20261002T150502-734731Z/summary.json`.
- Итоговый FieldPlatformSurfaceTests: 3/3 EditMode PASS;
  `TestResults/checks/20261002T152137-024324Z/summary.json`.
- Content generator `--check`: UP TO DATE в scoped runner.

Тесты покрывают общий материал старта/остальных платформ, world UV,
открытые мостовые устья, направление нижней грани, отсутствие новых
коллайдеров, dispose/reinitialize и чистоту выборки земли.

Кадры: `TestResults/field009-art-junction.png` (масштаб героя),
`TestResults/field009-art-overview.png` (общая карта). Отдельная пользовательская
оценка игрового вида после подключения не подменяется техническими проверками.
