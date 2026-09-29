# Начальный запас controls — 2026-09-29

Основание: [DECISION-0094](../../decisions/0094-starting-draft-controls.md).
Execution status — только STATUS.

Авторский baseline изменён с 3/2 на 1/1; `scripts/content/generate.py`
обновил только ProductionRunSetup.json. Fixture, сохранения и архивы прежних
плейтестов не изменялись. Runtime продолжает прибавлять личные бонусы героя.

Свежий safe runner `python scripts/check_project.py --scope full --graphics`,
Unity 6000.6.0f1, закрытый Editor → batch:
947/947 EditMode + 39/39 PlayMode, 0 failed/skipped, third-party 0.
Generated content актуален, audio 28 файлов PASS, art audit 256 записей PASS.
Receipt: `TestResults/checks/20260929T072829-723141Z/summary.json`, XML/logs рядом.
Новый config test проверяет 1/1; production Meta smoke после покупки +1 проверяет
2/2. Общие regression tests расхода/исчерпания/reset/Book также прошли.
`git diff --check` PASS. Ручной плейтест и выводы о сложности этим не заявляются.

Параллельный art request: 12 отдельных preview-иконок сохранены вне Assets в
`docs/implementation/proposals/ui-meta-icons-r1/`, prompts в candidates.json.
Использован встроенный image generation. Иконки ещё не approved/imported;
art_pipeline.py plan/apply и проверки их import отложены до Gate B из
ASSET_PIPELINE. Общий art audit выше относится к существующим 256 записям,
не к новым preview-кандидатам. Пример slot 32 px — index.html.
