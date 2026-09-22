# Checklist ручного review

1. Выбрать один вопрос и OBS; записать условия по [шаблону прогона](TEMPLATE.md). Неизвестные условия не заполнять догадками.
2. Выполнить ручной сценарий, marker/pause/end; сохранить local packet по [PLAYTEST_REPORT](../implementation/PLAYTEST_REPORT.md).
3. Перенести выбранный пакет в Git по [README](README.md); проверить IDs, hashes, final/incomplete, quality/capabilities. Ограничения входят в вывод.
4. Оформить [review](REVIEW_TEMPLATE.md): facts отдельно от estimates и hypotheses; сравнение только с обозначенными различиями условий. Результат no-change/insufficient-evidence допустим.
5. При proposal показать конкретную revision: ID/file/JSON path/before/after/units, risk, tests и rollback. Привязать explicit approval к revision, baseline и выбранным строкам. Pending/rejected/deferred не применять; при изменившемся baseline сначала обновить предложение.
6. Применить только одобренные строки, сохранить actual diff и выполнить проверки владельца. При частичном approval проверить совместимость выбранного поднабора; непригодный поднабор вернуть на review. Runtime не применяет proposals автоматически.
7. Провести comparable follow-up и связать новый report с исходным OBS. Исправление закрывать после проверки. Rollback возвращает только собственный patch; если значения уже изменились, не затирать чужие правки — проверить текущий diff.

Полный процесс: [BALANCE_WORKFLOW](../implementation/BALANCE_WORKFLOW.md). Пример реального review: [OBS-01](../balance/balance-progression-2026-09-21.md). Учебный accept/apply/rollback: [synthetic exercise](exercises/IP32/README.md).
