# DECISION-0041 — Общие процедурные ground shadows

Status: Approved
Date: 2026-09-22
Related IP: IP-12A, IP-20, IP-21, IP-22, IP-30

## Основание

Пользователь поручил выполнить первые три следующих art-пункта одним пакетом: сгенерировать гонца, встроить его в fixture и добавить общие тени под гоблина и врагов. Отдельные shadow PNG не нужны для текущей простой игры и увеличивали бы число ассетов без новой игровой информации.

## Решение

Gameplay body используют одну процедурно созданную radial alpha mask 32×32. Она создаётся один раз на процесс и разделяется всеми `SpriteRenderer`; ellipse получается только масштабом renderer. Размер, цвет, прозрачность и положение находятся в одном validated JSON-профиле.

Тень позиционируется по ground point body, полученному из `SpriteContactProfile.CenterY`. Её ширина один раз при инициализации вычисляется как диаметр authored contact-круга × общий коэффициент; это дешёвая зависимость от ширины персонажа без чтения пикселей и per-frame расчётов. Если у placeholder нет contact profile, используется общий fallback. Enemy root scale компенсируется при настройке local transform. Тень находится вне анимируемого `BodyRoot`, поэтому bob, tilt, hit squash и flip не двигают её. Она не имеет collider и не влияет на gameplay.

Профиль подключается к игроку, ordinary enemies, bosses и Travelers. Pool reuse переиспользует renderer и общую маску. Отдельные raster shadows можно добавить позже только при доказанной визуальной необходимости конкретного slot.

## Границы

Решение не утверждает финальный цвет/размер тени по пользовательскому gameplay review и не закрывает density gate IP-12A. Оно не меняет collision, damage, movement или balance. ENEMY-002 также остаётся на review: его body временно подключён к `FIXTURE-ENEMY-FAN`, а production binding относится к IP-20.

Pipeline и проверки: [ASSET_PIPELINE §24](../art/ASSET_PIPELINE.md#24-единая-процедурная-ground-shadow), [evidence](../implementation/evidence/2026-09-22-courier-and-ground-shadows.md).
