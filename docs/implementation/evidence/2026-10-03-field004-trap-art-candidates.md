# FIELD-004 — изображения ловушек для визуального выбора

Запрос пользователя: сделать арт для уже реализованных ловушек тренировочной
площадки. Набор найден в DECISION-0156, полном брифе
`docs/art/briefs/field004-traps-art-v1.md` и `field004-traps-v1.json`.
Полная карточка FIELD-004 прочитана через `read_card.py`.

## Результат генерации

OpenAI built-in image generation: 31 отдельный запрос с прозрачным фоном,
13 оснований + 13 головок турелей, 4 снаряда, 1 общий вид бочки.
Референс рисунка и материалов — approved camp training dummy; его внешность
не переносится. Все PNG скопированы byte-identical в
`Art/Candidates/field004-traps-2026-10-03/`, вне Assets.

Галерея `index.html` показывает отдельные слои и условное наложение,
даёт переключать земляной, светлый и тёмный фоны.
`prompts.json` хранит verbatim запросы; `candidate-records.json` — owner/role,
SHA-256, dimensions и alpha audit. Генератор не описывается как неизвестная
версия модели. Полный список IDs находится в этих материалах.

## Технический осмотр

Все 31 файла — RGBA PNG 1254×1254 с настоящей прозрачностью. На внешней
рамке 2 px нет пикселей с alpha >32. У `trap-003-head`, `trap-004-head`,
`trap-011-head`, `trap-012-base` есть слабый alpha noise у края;
точные значения (alpha=1) записаны в candidate records. Это не runtime-ready alpha:
пиксели оригиналов не менялись, cleanup должен быть явным этапом approved
packet preparation. Силуэты и материалы изображений осмотрены ИИ по
результатам генератора. Итоговый художественный выбор принадлежит пользователю.

Галерея не подтверждает физический footprint, точки крепления или gameplay
readability. Исходные prompts задают ограничения, но не заменяют эти проверки.
Арбалет и баллиста направлены вправо; радиальные и двухсторонние головки
сохраняют свои силуэты. Обычная/взрывная бочка получает один и тот же PNG.

Scoped `check_project.py --scope docs` для брифа, STATUS, этого evidence и
candidate README дал STATIC PASS; Unity NOT RUN. Галерея доступна локально
по HTTP, все 31 уникальные PNG-ссылки разрешаются. Это проверки файлов и
документации, не visual approval или runtime verification.

## Границы поставки

Runtime PNG, registry, manifest, import profiles, C# и gameplay данные этой
работой не менялись. Пользовательского выбора конкретных PNG ещё не было,
поэтому art_pipeline plan/apply и Unity art/FIELD-004 tests не выполнялись:
ASSET_PIPELINE §§5,12,19 требуют сначала выбрать candidate.
После выбора — точный approved packet, pipeline plan/apply, Unity import,
visual-only binding через registry и безопасные проверки из брифа.
Текущее исполнение и оставшиеся gates — только в STATUS.
