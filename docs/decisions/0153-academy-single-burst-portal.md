# DECISION-0153 — Академия: одиночный импульсный портал

Status: Approved
Date: 2026-10-02
Related IP: IP-23, IP-12A
Related content ID: FIELD-006-ZONE-PORTAL

## Decision

Пользователь заменил парный портал Академии одиночным разовым эффектом.
Портал выбирается на общих random chains (см. DECISION-0151), как другие виды; отдельная линия
и связанная зона выхода убраны. Уточнённая дальность: «давай это будет 5 единиц».
Портал действует только на игрока и сохраняет стандартные исчезновение, появление
и плавный перенос камеры через существующий PortalTransitRuntime.

Моментальный payload использует существующий Burst contract: предупреждение,
одно срабатывание для игрока внутри фактического эллипса в момент завершения
предупреждения, затем короткая вспышка и исчезновение печати. Поздний вход в
затухающий рисунок ничего не делает. Новые preview timings: предупреждение 5 с
(прежняя подготовка портала), вспышка 0.6 с (существующий импульс Академии).
Пауза не двигает часы; мёртвый игрок и игрок вне области не телепортируются.
Большой tick, пересёкший весь момент срабатывания, применяет payload один раз.

`portalJumpDistance` — положительное конечное расстояние в world units, текущее 5.
Пусть p — позиция игрока в момент срабатывания, theta — случайное направление
от 0 до 2π радиан, D=portalJumpDistance. Выход q=p+D×(cos theta, sin theta).
Пример: p=(10,10), theta=π/2, D=5 → q=(10,15).
Выбирается безопасное направление внутри arena margin и с прежним portal body
clearance от препятствий; положение не обрезается и расстояние не сокращается.
Если за placementAttempts безопасный выход не найден, телепортации нет;
печатный импульс завершается без повторного запроса. При одновременных порталах
не более одного переноса игрока за tick. Seed и occurrence определяют направление.

FIELD-006 хранит четыре reusable placements (по числу цепочек) для каждого из девяти видов, всего 36
(уменьшено с шести цепочек по поручению пользователя 2026-10-03);
у одиночных порталов PartnerIndex=-1. Legacy paired portals «Тест 06» сохраняют
прежние gameplay contracts. Эта ревизия заменяет portal parts DECISION-0151,
а геометрия середины штриха и сжатие 0.8 из DECISION-0152 сохраняются.

## Presentation and approval

Общая оправа получает фиолетовый цвет #b780ff. В центре — отдельный прозрачный
плоский spiral glyph; он сохраняет orientation и сжимается по Y вместе с полом.
Пользователь утвердил показанную картинку: «Да, подключить».
Пакет `Art/Packets/field006-portal-burst-v1.json` сохраняет hash, master и prompt.
Вертикальная дверь FIELD-006-PORTAL-VISUAL-PROP снята из sprite registry/import
profiles и runtime PNG удалён вместе с meta. Исторический master/provenance
сохранены вне Assets; manifest помечает отсутствие runtime подключения.
Legacy portal presentation также использует ground glyph вместо старой двери.

### Исправление наложения символов по отзыву пользователя

У книги и портала внутренние вращающиеся дуги могли выглядеть заменой утверждённого
рисунка; у Burst они дополнительно росли из центра поверх символа. При наличии
approved raster glyph скрываются оба procedural interior layers (glyph и motion).
Та же защита применяется к approved стрелкам отталкивания. Символ сохраняет PNG,
ориентацию, прежний свет/затухание; анимация внешней оправы и gameplay не меняются.

Повторный отзыв уточняет presentation guard для пересекающихся зон: центральные
символы всех зон рисуются выше декоративных motion layers всех зон. Ранее motion
соседней зоны имел больший sorting order и мог закрывать даже approved PNG при
отключённых собственных дугах. Gameplay overlap сохраняется. Растровый символ
также получает отдельный material с явно привязанной текстурой; общая оправа
использует свой material/texture. Это защита binding, а не подтверждённая причина
подмены PNG.

## Verification

Дополнение земли (2026-10-02): пользователь утвердил показанный исправленный
dark laboratory floor фразой «подключай пол». Выбран тёмный фиолетовый пол со
стёртыми алхимическими знаками с холодного керамического референса. Пакет
`Art/Packets/field006-ground-v003-dark-laboratory.json` заменяет v002 по прежнему
runtime path/GUID, без gameplay/geometry изменений.

BurstPortalTests: trigger timing, large tick, one application, no timed buff,
no enemy teleport, boundary/death, exact distance/safe exit, ordinary chain,
odd unpaired pool and schema rejection. Seal tests: ground glyph, no doorway,
orientation, flattening, warning state and teardown. Current execution status
and fresh runner evidence belong only to STATUS.md.
