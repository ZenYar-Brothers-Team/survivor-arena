# Anti-blob movement delta — 2026-09-29

## Scope

По прямому поручению пользователя реализованы пять O(1) movement kind из
[DECISION-0099](../../decisions/0099-anti-blob-enemy-movement.md), per-spawn
weighted variants и первая production-настройка ENEMY-001.

## Implementation

- `OffsetPursuit` получает при спавне личное направление, следует к смещённой
  относительно игрока точке и линейно замедляется в arrival radius.
- `CommittedPursuit` сохраняет снимок направления до следующего interval и может
  пройти мимо изменившей положение цели.
- `movementVariants[]` задаёт ordered chances; остаток оставляет основной
  `movement`. Сумма больше 1, отсутствующие chance/profile и некорректные
  per-kind параметры отвергаются при загрузке.
- WaveDirector использует отдельный seeded stream для variant/initial state;
  composition и spawn geometry сохраняют прежние последовательности.
- Горячий путь не ищет соседей, не выполняет physics query и не выделяет память.
  На врага выполняется постоянное число операций над `Vector2`.
- `BlockedSidestep` сравнивает фактическое смещение с предыдущей командой
  движения и при застревании временно направляет врага сбоку от игрока.
  `ArcPassPursuit` чередует дугу с прямой атакой; `InertialPursuit` плавно
  поворачивает курс. Все три шаблона используют только состояние самого врага.
- ENEMY-001: movement speed 0.96 → 1.056 wu/s. Первоначальный основной
  `OffsetPursuit` (точка 0.55 wu, arrival 0.35 wu) после первого просмотра
  получил 1.4/0.25 wu и цикл 3 s с последними 1.5 s прямой погони. Затем
  точка была увеличена до 2 wu. Пользователь сообщил, что этот шаблон не
  разбил blob. Смесь 50% Seek / 50% CommittedPursuit 4 s частично размыкала
  толпу по следующему отзыву пользователя. Усиленный `BlockedSidestep` также
  помог лишь частично. Первая настройка (trigger 0.4 s, progress threshold 0.35,
  minimum distance 1.1 wu, lateral weight 2, sidestep 0.9 s) не дала заметного
  результата по отзыву пользователя. Последняя настройка этого шаблона: trigger 0.25 s, threshold 0.95,
  minimum distance 0.5 wu, lateral weight 3, sidestep 1.8 s, cooldown 1.2 s.
  Внутри 2.2 wu обход также включается после 1.2 s даже при полном продвижении.
  После автоматической пробы текущий выбор при спавне: 75% BlockedSidestep
  (боковой вес 4, проход 2.4 s, near 3 wu/0.8 s), по 5% Seek,
  CommittedPursuit 6 s, OffsetPursuit 3.5/0.25 wu, ArcPassPursuit
  (радиус 4 wu, вес 2, цикл 2.5 s, прямой заход 0.6 s) и InertialPursuit
  (отклик 2 s). Профиль выбирается при
  спавне один раз; горячий путь остаётся O(1), без поиска соседей или пути.

## Automated checks

Первые результаты ниже относятся к начальной настройке 0.55/0.35. Позднее
пользователь поручил автоматическое сравнение и настройку: после итоговой
конфигурации Enemy EditMode прошёл 207/207 без failed/skipped,
`TestResults/checks/20260929T135711-754119Z/summary.json`. Сценарии и
метрики описаны в [отдельном evidence](2026-09-29-anti-blob-sweep.md).

- `python scripts/content/generate.py --check` — PASS, `UP TO DATE`.
- Targeted final Enemy EditMode: 200/200 Game.* passed, 0 failed/skipped;
  `TestResults/checks/20260929T115534-588171Z/summary.json`.
- Full EditMode перед последним test-only дополнением: 973/973 Game.* passed,
  0 failed/skipped; `TestResults/checks/20260929T115151-986774Z/EditMode.xml`.
- Full PlayMode попытка и один отдельный повтор: NOT RUN. Unity 6000.6.0f1
  завершился с кодом 3221225477 внутри
  `UnityEngine.Rendering.RenderPipelineManager.DoRenderLoop_Internal` до создания
  result XML. Логи:
  `TestResults/checks/20260929T115151-986774Z/PlayMode.log` и
  `TestResults/checks/20260929T115353-683018Z/PlayMode.log`.

## Open verification

IP-13 delta остаётся Implemented: PlayMode предыдущей итерации не подтверждён
из-за повторяемого сбоя Unity renderer. Ручной плейтест должен
проверить распределение ENEMY-001, контактное давление и то, исчез ли
управляемый единый blob; эти ощущения не подменяются unit-тестами.
