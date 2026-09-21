# DECISION-0030 — Generic presentation adapters и category imports

Status: Proposed

Date: 2026-09-21

Related IP: IP-12A; consumers IP-17…23, IP-26, IP-28, IP-30

## Context

Существующий player runtime и approved sprite сохраняются. Целевой scope требует других visual roles, resource validation, pool lifecycle и audit; gameplay systems нельзя привязать к конкретному character rig.

## Technical implementation record

- Новый `SpritePresentationAdapter` — отдельный thin child renderer writer, переиспользующий `ProceduralSpriteAnimator` для body. Простые projectile/pickup/VFX не получают пустой character rig. Sources реализуют узкий `IPresentationSource`; fixture actor показывает контракт, не меняет gameplay.
- Hit/proc/death/collect принимаются только от источника при running. Death/collect имеют одноразовый fade, но lifetime gameplay owner имеет приоритет: Shutdown немедленно восстанавливает baseline. Отдельный detached pooled effect может пережить owner; этот packet не задерживает enemy death/XP collection и не делает terminal animation exception.
- Proc envelope обновляется без суммирования; pose и color имеют одного writer. Pause и terminal freeze; return/disable и повторный Initialize очищают source subscription/flip/sprite/color/pose.
- Import profiles хранятся вне runtime Resources. Exact path overrides сохраняются при reimport; body pivot задаётся явно. UI/VFX centered и category-sized. Texture policy compression не меняется.
- Role validation и owned in-memory portrait crop не мигрируют existing content IDs. Production missing sprites не замещаются fixture fallback.
- Screen-shake boundary читает preference при каждом request; IP-26 владеет camera implementation и settings persistence. Новый camera behavior не вводится.
- Diagnostic kit — opt-in Editor view и synthetic geometry. Он не утверждает production assets или density четырёх реальных сетов.

## Consequences

DECISION-0013 остаётся основой player implementation; указанное там историческое исключение death относится к прежнему scope. Новый adapter реализует только presentation fade contract текущего IP-12A, не новое gameplay правило. Record имеет Proposed как техническая запись для review; продуктовые approvals W-01/G-18 и концепт CHAR-001 находятся в Approved DECISION-0029. Проверки/остаток IP — только STATUS.
