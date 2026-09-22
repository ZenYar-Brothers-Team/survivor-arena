# DECISION-0040 — Единая процедурная смерть врагов

Status: Approved
Date: 2026-09-22
Related IP: IP-04, IP-12A, IP-20, IP-21, IP-30

## Основание

Пользователь подтвердил короткую процедурную смерть вместо мгновенного исчезновения и отдельно уточнил: алгоритм одинаков для всех врагов, без специальной ветки для селянина и без толчка в момент смерти.

После исправления порядка snapshot и pool reset пользователь проверил результат в Gameplay и принял его: «Сейчас выглядит хорошо», затем разрешил коммит и подтвердил запись полной процедуры в документации.

## Решение

Смерть разделена на authoritative gameplay event и короткий visual tail. В момент lethal damage враг немедленно:

- публикует `Died`, выдаёт награду и засчитывается в статистику;
- исключается из target registry;
- прекращает движение, атаки и contact damage;
- отключает Rigidbody2D simulation, collider и telegraph;
- остаётся в текущей мировой позиции: death не добавляет impulse, knockback или displacement.

После этого один общий presentation profile выполняет squash, shrink, darken/fade и небольшой процедурный dust burst. По завершении объект публикует `Despawned` и возвращается в pool. Pause замораживает visual tail; terminal cleanup отменяет его и освобождает объект сразу. Алгоритм и профиль общие для ordinary enemies, bosses и Travelers. Он копирует фактический активный sprite в отдельный `DeathVisual`, поэтому не зависит от конкретного body rig и не содержит проверок content ID.

Текущий fixture timing: squash 0.10 s + fade 0.20 s = 0.30 s. Значения масштаба, цвета и пяти dust particles находятся в `FixtureEnemyDeathPresentation.json`; после gameplay-просмотра размер пылинки установлен в 0.08 world units, цвет — приглушённый земляной без красной/blood-семантики. Runtime не содержит entity-specific tuning. Отдельная покадровая анимация и новый raster asset не нужны.

## Границы

`Died` остаётся моментом смерти для gameplay, наград и telemetry. `Despawned` означает завершение визуального хвоста либо cleanup. Мёртвый объект не считается alive и не занимает target registry, но до `Despawned` остаётся в owner collection, чтобы pool reuse не мог начаться посреди эффекта. Эффект не меняет баланс, damage timing до смерти или коллайдер живого врага.

Pipeline и проверки: [ASSET_PIPELINE §23](../art/ASSET_PIPELINE.md#23-единая-процедурная-смерть-врагов), [evidence](../implementation/evidence/2026-09-22-shared-enemy-death.md).
