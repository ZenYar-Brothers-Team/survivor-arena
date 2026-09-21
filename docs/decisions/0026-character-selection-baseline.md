# DECISION-0026 — Character Select: explicit baseline and authored highlights

Status: Approved
Date: 2026-09-21
Related IP: IP-10A, IP-12, IP-22, IP-26
Related content IDs: CHAR-001…010 (presentation contract; no numeric changes)

## Context

Character Select должен показывать краткие значимые отличия от baseline. Content Design уже задаёт базовую точку сравнения (100 HP, 100% movement, damage и cooldown duration, 0% disappearing-XP recovery), но отдельный data contract и критерий значимости не были определены; IP-12 не мог выбирать их из первого персонажа или самостоятельно назначать процентный порог. В STATUS этот presentation gap ошибочно обозначался G-15, который в DESIGN_SYNC относится к meta/run-exit semantics.

## Decision

- База сравнения задаётся отдельными явными данными, независимо от roster и выбранного персонажа. Она служит для представления отличий и не подменяет gameplay base stats персонажа.
- Данные каждого персонажа содержат явно заданный упорядоченный список особенностей, которые нужно показать на Character Select. Именно авторский выбор определяет значимость; автоматический процентный порог не применяется.
- Для числовой особенности указывается характеристика, сравниваемая с отдельной базой. Число должно соответствовать actual character data и baseline; UI не хранит самостоятельную копию gameplay balance. Полный внутренний stat dump не показывается.
- Пустой список допустим для персонажа без выделенных особенностей; renderer не заполняет его автоматически. Отсутствующую baseline/невалидную ссылку нельзя заменять первым персонажем или текущим выбором.
- IP-12 использует явно заданные non-production fixture baseline/highlights и fake unlock provider. Существующая каноническая базовая точка сравнения сохраняется; её перенос в отдельные данные не меняет числа. Новые baseline channels, highlights каждого CHAR-ID, draft weights, цены и unlock semantics не утверждаются этим решением и поставляются соответствующим production packet.

## Consequences

UI §§4/23, Content character authoring contract, IP-12/IP-22 и STATUS синхронизируются. Presentation gap регистрируется отдельно как G-19 и закрывается этим решением; G-15 сохраняет исходный смысл. Требования IP-12 проверяют независимость baseline от порядка roster, explicit highlights, пустой список и ошибки ссылок. Это решение о представлении данных, без изменения игровых характеристик, формул или баланса. Runtime implementation и Unity verification остаются отдельной работой IP-12.

## Approval

Пользователь 2026-09-21 ответил «да, делаем как ты предлагаешь» на предложение отдельной явно заданной базы и списка показываемых особенностей в данных персонажа. Дополнительное подтверждение этого решения не требуется.
