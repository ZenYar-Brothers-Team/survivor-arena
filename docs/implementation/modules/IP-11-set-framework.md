# IP-11 — Set framework: recipes, eligibility и extra abilities

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-11--set-framework).

## Цель

Recipe fulfillment делает set probabilistically eligible, а выбор set добавляет independent extra ability вне 6+6 slots.

## Зависимости

IP-07, IP-08, IP-09.

## Scope

Recipes with active/passive components; minimum levels; eligibility; set draft weight/probability; acquisition; no levels; unlimited simultaneous sets; shared components; extra ability runtime.

## Context

- Game Design: «Сеты» и релевантная часть «Опыт и level-up».
- Content Design: SET-001…008 как Draft compatibility targets; связанные SKILL/PASSIVE IDs читать по ссылкам рецепта.

## UI / observability

При eligibility/acquisition set появляется в draft и затем в отдельном build sets state без занятия active/passive slot; recipe progress доступен fixture/debug view.

## Acceptance criteria

Unmet recipe never eligible; fulfilled recipe can appear but not guaranteed; selected set does not occupy slot; duplicate acquisition blocked; multiple sets including shared components work independently.

## Проверки

Thresholds, shared component, probability hook, extra ability.

## Out of scope

Production SET-001…008 — IP-19.
