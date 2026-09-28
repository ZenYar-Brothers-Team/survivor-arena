# DECISION-0081 — UI visual language «Полевой фолиант»

Status: Approved (пользовательское утверждение 2026-09-28)
Date: 2026-09-28
Related IP: IP-10A, IP-12A, IP-26, IP-27
Related content IDs: none
Supersedes: простые placeholder/shapes как целевое оформление; функциональные UI-контракты и ранее утверждённые экраны не меняет

## Основание

Функциональный UI и production visuals уже существуют, но простые тёмные панели,
системная типографика и локальные цвета не задают общего чистового языка. Нужен
классический для fantasy/storybook направления стиль, совместимый с существующими
персонажами, полями и иконками и не создающий новый обязательный raster backlog.

## Решение

- Утвердить направление **«Полевой фолиант»**: современная функциональная
  структура в матовом сказочном оформлении из угольно-сливового контура, кожи,
  тёплого пергамента, ржавчины, приглушённой зелени и дозированного золота.
- Использовать умеренную декоративность: слабая бумажная фактура, слегка живой
  контур и небольшие маркеры; без тяжёлых рам, свитков, сургуча, glass/blur,
  глянцевого псевдо-3D и постоянного золотого орнамента.
- Принять palette, typography, component states, icon sizing и motion defaults из
  [Art Direction §12.2](../art/ART_DIRECTION.md#122-визуальный-язык-ui--полевой-фолиант).
- Существующие production-иконки, character body crops и field thumbnails остаются
  главными visuals. UI не запекает в них frame/state и не требует их перерисовки.
- Первый эталонный production slice — `HUD → Level-up Draft → Pause / Build` при
  `1920×1080` и `1280×720`. Перенос темы на остальные экраны выполняется после
  его visual approval.

## Следствия

- UI/UX screen flow, gameplay rules, navigation, settings и meta economy не меняются.
- Новые portraits, reroll/banish art, meta icons и декоративные menu backgrounds
  создаются только после target-scale review, если существующих visuals недостаточно.
- Approval этого решения закрывает направление, но не implementation, responsive,
  capture-matrix или manual visual acceptance gates IP-26/IP-27.
- Общие визуальные правила принадлежат Art Direction; screen composition — UI/UX;
  execution status остаётся только в STATUS.
- Review 2026-09-28 подтвердил стиль, но не геометрию первого прохода. Текущая
  UXML/USS-раскладка не становится эталоном; player/DEV и расположение HP уточнены
  в [DECISION-0083](0083-player-ui-layout-and-dev-boundary.md). Стилевой проход
  на fixture-кадрах не закрывает production-layout acceptance.
