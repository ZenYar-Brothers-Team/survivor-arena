# World visuals SET-021/022 — 2026-10-02

Пользователь поручил закрыть два visual gaps после поставки иконок:
«сделай то что нужно по 21 и 22 сетам».

SET-021-ATTACK переиспользует утверждённый `SKILL-001-VISUAL-PROJECTILE`.
Ссылка задана в authoring `scripts/content/progression.py`, `ProductionSetAttacks.json` regenerated.
Все шесть template levels наследуют спрайт камня, вращение и общий impact;
collision radius 0.3, скорость, дальность, damage и правила отдельной set-атаки не меняются.
Новых PNG, aliases visual ID или provenance masters нет.

SET-022-ATTACK получает `ConeArc` в `SkillWorldEffects.json`: тёплая amber/cream дуга,
толщина 0.12 world units, fade 0.18 running seconds. Shared presenter рисует
две боковые границы и дугу из 12 отрезков через существующий SpriteRenderer pool.
Радиус (`effect.Radius × SizeMultiplier`), угол и направление передаются после damage
из той же выборки конуса. Если цели нет, визуал использует уже выбранное seeded random
направление, не расходует дополнительный random и не подменяет его AimDirection.
Effect root/colliders не перемещаются; нет нового projectile или persistent entity.

Pause не двигает fade; затухание возвращает shapes в pool; terminal/Clear/Dispose
немедленно очищают shapes через существующий executor lifecycle.
Geometry subdivision — техническая точность дуги, не gameplay tuning.
Цвет, толщина и время — validated presentation config.

## Проверки

Unity 6000.6.0f1, закрытый Editor → безопасный batch runner:

- `Game.ActiveSkill.Tests`: EditMode 122/122 PASS, 0 failed/skipped;
  `TestResults/checks/20261001T221624-530031Z/summary.json`.
- `--scope art`: EditMode 80/80 PASS, 0 failed/skipped; manifest 308 PASS;
  `TestResults/checks/20261001T221708-426713Z/summary.json`.
- `ActiveSkillPatternSmokeTests`, PlayMode с graphics: 2/2 PASS, 0 failed/skipped;
  `TestResults/checks/20261001T221837-313043Z/summary.json`.
- generation `--check` и scoped whitespace checks PASS.

Новые EditMode cases проверяют stone reference/radius на всех уровнях и обе ветки направления
конуса, длину/угол боковых границ при SizeMultiplier=2, pause/fade, pool reuse и terminal cleanup.
Production PlayMode smoke загружает реальный catalog/registry/profile, запускает обе атаки,
проверяет настоящий SpriteRenderer камня, наличие конуса, pause и stop.

Ручной плотный забег, художественная оценка в толпе и баланс чисел/цен этим прогоном
не закрываются. Текущий execution status/order — только [STATUS](../STATUS.md).

## Обратная связь: движение конуса

После первого просмотра пользователь указал на статичный контур:
[OBS-01](../../playtests/2026-10-02_set-022-cone-motion.md).
Добавлен нейтральный optional `expansionSeconds` в DTO/catalog/validated profile.
Он допустим только для ConeArc, неотрицательный и строго меньше общей lifetime `fadeSeconds`.
Хлопушка: 0.14 s outward travel + 0.10 s fade. Все остальные profile сохраняют expansion=0.
Каждый сегмент движется от snapshot origin к полной позиции, его длина растёт,
толщина остаётся прежней. Damage применяется до travel без изменений.

Свежие проверки после коррекции: ActiveSkill EditMode 122/122 PASS
(`TestResults/checks/20261001T222812-796108Z/summary.json`), PlayMode graphics 3/3 PASS
(`TestResults/checks/20261001T222926-632005Z/summary.json`), 0 failed/skipped.
PlayMode включает два сохранённых lifecycle smoke и временный capture harness,
удалённый после получения 14 настоящих кадров Unity с шагом 20 ms.
EditMode дополнительно проверяет нулевой стартовый радиус, половину/полную дальность,
pause во время движения и мгновенный damage до визуального расширения.

[GIF](../proposals/2026-10-02-low-tier-set-icons/set-022-cone-travel.gif) и
[четыре стадии](../proposals/2026-10-02-low-tier-set-icons/set-022-cone-travel-frames.png)
собраны из кадров камеры без рисования эффекта. Inspect: конус движется наружу, держит
угол/толщину, затем затухает. Красный квадрат — fixture target capture-стенда.

Уточнение [OBS-02](../../playtests/2026-10-02_set-022-cone-motion.md#obs-02--убрать-прямые-стороны):
по команде «только дуга, без прямых боков» удалены оба radial strokes. Осталась curved arc.
Свежие проверки: EditMode 122/122 (`TestResults/checks/20261001T223203-380548Z/summary.json`)
и PlayMode graphics 3/3 (`TestResults/checks/20261001T223300-501671Z/summary.json`) PASS, 0 failed/skipped.
Regressions проверяют endpoints на половинном/полном радиусе, отсутствие прямых сторон,
сохранение угла, aimed/random direction и lifecycle.
[GIF только дуги](../proposals/2026-10-02-low-tier-set-icons/set-022-arc-only.gif) и
[4 кадра](../proposals/2026-10-02-low-tier-set-icons/set-022-arc-only-frames.png)
сняты в Unity; область эффекта приближена crop без изменения самого изображения.
