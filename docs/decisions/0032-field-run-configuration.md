# DECISION-0032 — Field configuration и pre-run ownership

Status: Proposed

Date: 2026-09-21

Related IP: IP-01, IP-12, IP-14, IP-15, IP-16, IP-23, IP-24, IP-25, IP-26, IP-29, IP-31

## Context

IP-16 добавляет field selection между выбором персонажа и запуском. Старый launcher
запускал один глобальный timeline. UI §5/IP-16 указывают difficulty 1–5, а approved
FIELD cards — 1–10; это отдельный product gap G-20, не решаемый преобразованием ID.

## Implementation record

`Game.Field` хранит typed content references и validating resolver, не зависит от
UI, Bootstrap или будущего Traveler runtime. Composition root загружает конкретный
enemy pool/timeline/boss bindings и существующее environment scene binding до Start.
`ICharacterRunLauncher` теперь переводит в Field Select; `IFieldRunLauncher` выполняет
launch или Back. Profile adapters поставляют причины блокировки через interfaces.

`RunSelectionSnapshot` в Game.Run сохраняет character/field/environment/timeline IDs
независимо от DEV telemetry. `RunOutcome.Selection` удерживает ту же immutable
идентичность после teardown. JSON sources и actual wave seed входят в telemetry provenance.
Retry UI остаётся IP-26; этот packet даёт ему configuration identity, не новый reward policy.

Оба fixture fields используют одну существующую player-only arena; различаются
waves/pools/midboss presence. Environment adapter validates bindings без изменения
scene/assets. Placeholder thumbnail — текстовый блок. Optional Traveler token
типизирован, но non-null runtime binding отвергается до появления consumer IP-29.

## Consequences and approval boundary

Это запись реализованной межслойной технической схемы для review, не изменение
GDD/CD. Production geometry/schedules/economy не выдуманы. Difficulty fixtures
следуют 1–5 из IP/UI; конфликт шкал G-20 сохраняется до явного ответа пользователя.
Документ не объявляет Proposed architecture или product scale утверждёнными.
