# Смерть селянина и невидимые враги

Источник: сообщение пользователя, 2026-09-22. Raw report, reportId/runId и точное время не предоставлены.

## OBS-01 — death effect виден на placeholder, но не на селянине

Цитата: «квадраты сжимаются, то есть у них есть этот эффект умирания, а sereянин нет».

Диагностика: animated body сбрасывал sprite в baseline до копирования в `DeathVisual`; clone получал null sprite. Исправление: snapshot активного renderer выполняется до `SpritePresentationRuntime.Shutdown()`.

Состояние: проверено пользователем 2026-09-22 — «Сейчас выглядит хорошо»; `EnemyBodySmokeTests.GameplaySpawner_UsesApprovedVillager_AndResetsPooledBody` проверяет sprite у death clone.

## OBS-02 — некоторые враги становятся невидимыми

Цитата: «И кажется ещё некоторые враги стали невидимыми».

Диагностика: death snapshot выключал renderer animated body, а presentation baseline не сохранял и не восстанавливал `enabled`; pooled reuse мог вернуть sprite с выключенным renderer.

Состояние: проверено пользователем 2026-09-22 — «Сейчас выглядит хорошо»; baseline восстанавливает `enabled`, smoke проверяет видимость после pool reuse и выключенный старый `DeathVisual`.
