# Быстрый рабочий цикл

Команды запускаются из корня репозитория. Нужны Python 3 и Pillow (уже используются существующими art scripts). Настройки модели и плагины команды не меняют. Следующий IP автоматически не начинают.

## 1. Подготовка утверждённого арта

AI заполняет [шаблон пакета](../Art/Templates/art-packet.template.json) фактическими значениями и сохраняет рабочий JSON вне `Assets`, например в `TestResults/art-packet.json`. Пользователь не обязан вручную заполнять JSON. Перед подготовкой обязательны content gate, полный brief и выбранный пользователем candidate по [Asset Pipeline](../docs/art/ASSET_PIPELINE.md).

```powershell
python scripts/art_pipeline.py TestResults/art-packet.json
python scripts/art_pipeline.py TestResults/art-packet.json --apply
python scripts/check_project.py --scope art
```

Первая команда проверяет весь пакет и выводит список изменений без записи ассетов. Вторая применяет тот же утверждённый пакет. `--apply` — выбор режима команды, не запрос повторного пользовательского approval. Хеш `sha256` привязывает approval к конкретным входным bytes. Утверждение пакета не означает согласие на последующие содержательные замены.

Команда сохраняет immutable `vNNN/concept-01.png`, `selected-master.png`, provenance, runtime PNG, запись manifest и `FixtureSprites.json`. Существующий GUID сохраняется: `.meta` никогда не создаётся и не редактируется скриптом. Повтор того же пакета не переписывает совпадающие файлы. Перед каждой записью сверяются исходные bytes; при ошибке собственные завершённые записи откатываются, чужие изменения сохраняются. Это защита от обычных ошибок записи, не транзакция на случай отключения питания. Импортировать пакет следует между проверками, без параллельного редактирования тех же файлов.

`preparation` имеет два режима:

- `copy`: byte-identical PNG; действующий Unity import profile задаёт импортное разрешение. Для body нужен заранее подготовленный approved input с правильной ground-contact line.
- `fit`: только технический downscale без увеличения силуэта, прозрачный квадратный canvas и центрирование. `size`, `padding` и `cropAlpha` задаются явно. `cropAlpha: false` сохраняет авторский canvas при масштабировании. Body этим режимом не центрируется автоматически.

Новые import overrides задаются полным `importProfile` с `pixelsPerUnit`, `maxSize`, `pivotX`, `pivotY`, `reason`. Для body нужны точный profile и явно подготовленные `sprite.contactRadius` / `contactCenterY`. Для projectile нужен полный `sprite.projectile`. Существующие sprite/import profiles должны совпадать: смена gameplay geometry или поведения не является пакетной заменой картинки.

Привязки необязательны и всегда перечисляются явно. Например, внутри asset:

```json
"bindings": [{
  "file": "Assets/Resources/Content/ActiveSkills/FixtureActiveSkills.json",
  "ownerId": "FIXTURE-SKILL-BOLT",
  "pointer": "/iconVisualId",
  "expect": "SKILL-001-VISUAL-ICON"
}]
```

Команда назначит этой ссылке `visualId` asset. Вложенные пути вида `/levels/0/effect/visualId` допустимы только при существующей структуре и совпадении `expect`; новые owners/production definitions команда не создаёт. При отсутствии привязок сохраняется только зарегистрированный visual asset. Существующий runtime path и visual ID не мигрируются.

Для замены нужны следующая `version`, `replacementApproved: true`, `replacesSha256` предыдущего master и новое фактическое approval evidence. Кандидаты прежних версий неизменяемы. Новый manifest entry получает только `Prepared`; Unity import, технические проверки и визуальное принятие остаются отдельными этапами. Альфа, отсутствие halo, художественное качество, сложные разложения на слои и collider authoring не объявляются проверенными этой командой.

## 2. Быстрая визуальная итерация

Для уже подключённого эффекта AI меняет только запрошенные существующие числовые параметры, затем выполняет:

```powershell
python scripts/check_project.py --scope visual-preview --paths Assets/Resources/Content/Presentation/FixtureGroundShadowPresentation.json
```

Разрешены четыре файла: `FixtureGroundShadowPresentation`, `FixtureEnemyDeathPresentation`, `FixtureSpriteMotionProfiles`, `FixturePresentationFeedback` в `Assets/Resources/Content/Presentation/`. Проверяются JSON, неизменность структуры/ID и конечные числовые значения относительно `HEAD`; `--base <commit>` задаёт другой явный baseline. Переупорядочивание записей, новые поля, смена строк/ссылок, runtime code, collision, import pivot/PPU и другие content-файлы требуют обычного scope.

Результат — `PREVIEW ONLY`, Unity tests не запускаются. Это проверка границ правки, а не всех domain ranges и не доказательство runtime-корректности. AI показывает изменение через существующий `Tools > Survivor Arena > Presentation Fixture Review` либо соответствующий Gameplay showcase; если конкретный эффект виден только в Gameplay, используется именно он. Для прочитанных при старте JSON нужен новый showcase/run, hot reload не обещается. Визуальный feedback может приводить к следующим коротким итерациям без полного smoke после каждой.

После принятия варианта выполнить затронутые presentation/catalog проверки:

```powershell
python scripts/check_project.py --scope art
```

Если менялась логика, schema, lifecycle, pooling или acceptance IP требует полный smoke, выполнить `--scope full`. Нельзя отметить IP Verified на основании `PREVIEW ONLY`.

## 3. Единая команда проверок

```powershell
python scripts/check_project.py --plan
python scripts/check_project.py --scope full
python scripts/check_project.py --scope code --platforms EditMode --filter '^Game\.Combat\.'
python scripts/check_project.py --scope docs --paths docs/implementation/WORKFLOW.md
```

`auto` (по умолчанию) смотрит staged/unstaged изменения и untracked files: только Markdown → static; только art/inventory/sprite registry → art; код, gameplay JSON, инструменты и смешанные изменения → full. `--paths` ограничивает статический scope конкретной задачи при чужих незавершённых изменениях; этот список не является полной проверкой остального working tree. Требования выбранного IP имеют приоритет над auto.

`art` запускает Game.Presentation.Tests и FixtureRuntimeContentCatalogTests в EditMode, затем manifest validator. `code` допускает явные platform/filter. `full` выполняет все `Game.*` EditMode + PlayMode и manifest audit; custom filter/platform с этим scope запрещены. Существующие тесты не удалены. `Test-Unity.ps1` сохранён как совместимая оболочка над безопасным runner; результаты теперь находятся в отдельной папке каждого запуска.

Перед каждым Unity запуском runner проверяет процессы и project lock. При открытом Editor используется UnitySkills REST; Bypass для PlayMode включает только пользователь. Недоступная проверка процессов, другой project, запрет режима или недоступный REST дают `NOT RUN / INCOMPLETE` и exit 2. При закрытом Editor запускается установленная версия из ProjectVersion. Никакого batch поверх открытого Editor и автоматического закрытия Editor. `--unity-url` выбирает явно нужный локальный endpoint; `--unity-path` позволяет указать Editor. Процессам запрещено конкурировать через локальный runner lock.

У UnitySkills literal class/namespace filter, поэтому runner отдельно переводит стандартные scopes и простые anchored namespaces. Непереводимое regex-выражение не расширяется до всех тестов молча. Server mode/grant restrictions сохраняются.

Ожидание ограничено `--timeout 300` секунд на platform/group. По timeout останавливается только собственный batch process. REST job автоматически не перезапускается: его ID сообщается для проверки завершения. Повтор не запускается до разбора ошибки. Каждый запуск сохраняет XML/JSON, log, counts и краткий `summary.json` в `TestResults/checks/<timestamp>/`. Нулевой набор, failures, skipped/inconclusive или несогласованная версия не дают PASS. Exit 0 означает успех заявленного scope, exit 1 — ошибку проверок.

```powershell
python scripts/check_project.py --scope full --reuse
```

`--reuse` явно разрешает использовать предыдущий PASS только для того же scope/filter/platform, при закрытом Editor, совпадении hashes всех Assets/Packages/ProjectSettings/scripts и двух art catalogs, наличии неизменённых evidence-файлов. Вывод `REUSED PASS` содержит исходную дату. Обычные `.md` вне этих деревьев не меняют fingerprint. Изменение проверяемых inputs во время запуска запрещает записать reusable PASS. Единственное исключение — создание новых `.meta` для уже присутствовавших до запуска assets/folders при первом импорте: они перечисляются в receipt и включаются в итоговый fingerprint, без повторного прогона ради этих файлов. Любые другие изменения, включая изменение существующего `.meta`, по-прежнему блокируют reusable PASS. REST evidence не переиспользуется: несохранённое Editor-состояние не имеет файлового fingerprint. Без `--reuse` проверки выполняются заново. Пробелы в генерируемых Unity `.meta` не проверяются через `git diff --check`; импорт проверяется Unity.

Проверка самих инструментов без Unity:

```powershell
python -m unittest discover -s scripts/tests -v
```
