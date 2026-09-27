# Production content tooling

Редактируемые численные источники — approved JSON в `docs/balance/`.
`sources.py:SOURCE_PATHS` перечисляет все входы, включая Content Design (имена/поля карточек)
и fixture environment template. `generate.py:TARGETS` — единственный список выходов.
Остальные runtime JSON не принадлежат этому генератору.

```powershell
python scripts/content/read_card.py SKILL-006
python scripts/content/read_card.py --list
python scripts/content/generate.py
python scripts/content/generate.py --check
python scripts/check_project.py --scope content
```

Все команды запускаются из корня. `read_card` читает канон напрямую, не хранит
копию карточек; связанные карточки/правила читать отдельно по Context задачи.
`--check` не записывает результаты, а завершается ошибкой при рассинхронизации.
`--scope content` проверяет генерацию без Unity и не подтверждает runtime acceptance.
Прежний `scripts/generate_field001_content.py` — совместимая оболочка без собственной логики.

## Где менять преобразования

- `sources.py` — загрузка/approval входных пакетов и извлечение имён/полей канона.
- `common.py` — общие конструкции waves/controls/pierce.
- `skills.py` — active skill levels и визуальные привязки.
- `progression.py` — passives, sets и дополнительные атаки сетов.
- `actors.py` — characters, enemies, pickups, Travelers.
- `bosses.py` — boss/mid-boss patterns.
- `fields.py` — геометрия, environment presentation, timelines, run setup.
- `generate.py` — маршрутизация выходов и стабильная сериализация.

Порядок изменения: approved source → генерация → `--check` → затронутые Unity tests.
Не исправлять generated JSON вручную. Новый input добавлять в `SOURCE_PATHS`,
новый output — в `TARGETS`; fingerprint единого runner использует тот же список входов.
Gameplay values и product decisions остаются в существующих канонических источниках.
