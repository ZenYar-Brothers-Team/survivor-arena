# DECISION-0060 — Данные поздних умений и пассивок v1

Status: Approved (пользователь 2026-09-26: «Утверждаю пакет»)
Date: 2026-09-26
Related IP: IP-17, IP-18
Related content IDs: SKILL-008/009/011/012/015/016, PASSIVE-006/010/013/014

## Context

После F1-01/F1-02 в IP-17 и IP-18 остались ID без полных данных уровней: в карточках Content Design
нет дистанций, скоростей, hit size, lifetime, trigger radius и задержек второй волны. Отсутствующий
runtime-параметр нельзя заполнять скрытым default (IP-18), поэтому оба IP оставались Blocked.
Пользователь разрешил идти дальше по общему backlog и попросил подготовить пакет по образцу F1-00.

## Decision

Утверждены [данные v1](../balance/late-skills-passives-v1.md) и
[таблица L1–L6](../balance/late-skills-passives-v1.json) как production data для этих десяти ID.
Все числа карточек сохранены; дополнены только отсутствовавшие параметры (см. таблицу дополнений).
Правила применения — как в baseline v1 и DECISION-0021; unlock — DECISION-0050 без изменений.

## Consequences

- IP-17/IP-18 больше не блокируются недостающими параметрами этих ID; реализация и per-ID тесты — дальше.
- World art SKILL-009/011/012/015/016 — отдельные per-ID gates; fixture fallback не используется.
- Настройка после плейтеста — через BALANCE_WORKFLOW, отдельной правкой данных.
- Проверка таблицы: `python -X utf8 docs/balance/validate_late_skills_passives.py` (PASS 2026-09-26).
- Визуал 2026-09-26: процедурные растровые кандидаты пользователь отклонил; луч SKILL-012 рисуется
  процедурно (выбор пользователя), world art SKILL-009/011/015/016 пользователь сгенерирует отдельно, до этого
  они показывают явный placeholder. Реализация: [evidence](../implementation/evidence/2026-09-26-late-skills-passives.md).
