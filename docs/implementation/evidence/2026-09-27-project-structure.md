# Структурный рефакторинг — 2026-09-27

Основание: [DECISION-0072](../../decisions/0072-project-structure-and-audio-ownership.md).
Текущий execution status и незакрытые acceptance — только в [STATUS](../STATUS.md#repo-01--структура-навигация-и-единые-проверки).

## Изменения

- PROJECT_MAP связывает точки входа, канон, редактируемые источники, generated outputs и checks.
- История разрешений/проверок отделена от текущей очереди; актуальные gates сохранены.
- Game.Audio выделен из Bootstrap/Settings; runtime paths разделены на Music/Sfx/Ambience.
- Field ambience выбирается по fieldId в JSON; прежний FIELD-001 clip/gain сохранён.
- RuntimeContentCatalog явно различает fixture и production factories.
- Генератор разделён по категориям с совместимым старым CLI; исходные числа и выходы сохранены.
- Общий runner включает audio/generation validators и authoring/provenance fingerprint.
- read_card читает полную карточку, включая поля с heading того же уровня.

## Проверки

- Python tooling: `python -m unittest discover -s scripts/tests -v` — **23/23 PASS**.
  Включены regression cases для выбора audio/source scopes, additive data validators,
  отказа при ошибке generation check, fingerprint внешних authoring/provenance inputs
  и извлечения карточек с одноуровневыми heading-полями. Первый sandboxed запуск
  не смог писать Windows Temp; повтор с разрешённым доступом прошёл.
- `python scripts/content/generate.py --check` и совместимый старый CLI — **UP TO DATE**.
  Generated gameplay JSON не изменились; среди runtime content JSON менялся только Audio.
- Audio integrity — **28 файлов / 15 cue families PASS**. В 28 source records изменены
  только пути; hashes, лицензии, processing и прочие поля сохранены.
- Полный безопасный smoke через `scripts/check_project.py --scope full`, runner **batch**,
  Unity **6000.6.0f1**: **857/857 Game.* EditMode, 30/30 PlayMode, 0 failed, 0 skipped**.
  Art manifest: **151/151 PASS**. Финальный fingerprint стабилен, verdict **PASS**.
  Receipt: `TestResults/checks/20260927T111851-175703Z/summary.json`;
  рядом `EditMode.xml`, `PlayMode.xml` и logs. Data validator results включены в receipt.
- Критические пути присутствуют в общем suite: lifecycle, combat/death, XP/draft,
  skills, wave/spawn/pooling, composition, UI, catalog loading и gameplay/production smoke.
  Audio tests проверяют импорт общих дорожек/SFX/атмосферы и отсутствие fallback для FIELD-002.
- Структурный audit: **39 перенесённых metadata GUID сохранены**, 45 asmdef без циклов,
  все новые Audio assets имеют Unity-generated `.meta`; ссылки изменённых документов
  проверены. Неактивный `#audio` в UI/UX получил явный anchor без изменения текста правил.
- Статусы всех **36 IP** сохранены; REPO-01 ведётся отдельно в STATUS. Размер STATUS
  уменьшен примерно на треть за счёт вынесения истории, без удаления действующих gates.

## Промежуточные прогоны

- `20260927T111315-188856Z`: NOT RUN — компилятор потребовал Game.Presentation для
  унаследованного Settings-контракта IScreenShakePreference; ссылка добавлена в Game.Audio.
- `20260927T111458-379747Z`: 857/857 EditMode и 30/30 PlayMode прошли, но Unity дописал
  importer metadata новых папок/сборок; runner отклонил итоговый reusable PASS из-за
  изменения inputs. Повтор после стабилизации дал финальный PASS выше.

Ручной review звука, баланса, плотности и приёмка F1-09/F2-06 не относятся к этому автоматическому прогону.
