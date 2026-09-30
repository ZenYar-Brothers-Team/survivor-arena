# Размеры снарядов и новый вид диска — 2026-09-30

## Изменения

- [DECISION-0116](../../decisions/0116-projectile-scale-and-disk-circle.md):
  камень −10%, клинок −15%, сфера −10%, бумеранг без изменений.
- Авторский `late-skills-passives-v1.json`: hit radius SKILL-008 L1…L5
  0.2 → 0.162, L6 0.24 → 0.1944; production JSON регенерирован.
- Новый top-down диск сохранён в `Art/Source/Skills/skill-008/v002/concept-01.png`
  и подключён по прежнему runtime path через
  `Art/Packets/skill-008-topdown-v002-2026-09-30.json`. GUID `.meta` сохранён.
  Пользователь после показа кандидата уточнил его размер и границу попадания;
  эти уточнения зафиксированы в approval evidence пакета.
- [Сравнение при 108 px/world unit](../../art/reviews/2026-09-30-projectile-size-proposal.png)
  показывает пять снарядов на базовом уровне. Для диска использована
  подготовка 256×256, alpha cutoff 16 и padding 32.
  Видимый bounds кандидата: x32…223, y33…222; горизонтальный круг 192 px.
  При hit radius 0.162 и visualScale 1.333333 его видимый диаметр 0.324.

## Проверки и gate

Art pipeline plan и apply прошли; повторный plan — 0 изменений. Числовые
данные до замены PNG прошли full graphics: 1085/1085 EditMode + 59/59
PlayMode, manifest 270/270 (`TestResults/checks/20260930T094456-890891Z`).
После замены PNG art graphics прошёл 63/63 EditMode и manifest 270/270
(`TestResults/checks/20260930T095221-640148Z/summary.json`). Art pipeline
unit tests 8/8 PASS; новая проверка диаметра диска прошла в EditMode.
Два первых full graphics прогона после замены дали 1086/1086 EditMode + 59/59
PlayMode, но runner пометил их INCOMPLETE из-за параллельных изменений файлов.
Повтор на стабильной рабочей копии завершился **PASS**: Unity 6000.6.0f1,
1086/1086 EditMode + 59/59 PlayMode, 0 failed/skipped, manifest 270/270,
generation/audio PASS (`TestResults/checks/20260930T100714-126686Z/summary.json`).
Игровой visual review вращения и рикошетов остаётся открытым.
