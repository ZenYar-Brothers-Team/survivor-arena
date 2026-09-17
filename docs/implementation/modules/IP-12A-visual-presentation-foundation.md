# IP-12A — Visual Presentation Foundation

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-12a--visual-presentation-foundation).

## Цель

Создать воспроизводимый pipeline от утверждённых правил генерации растрового арта до подключённого в Unity процедурно анимированного спрайта, проверив весь путь на одном fixture-персонаже.

## Зависимости

IP-00, IP-02, IP-03, IP-12; [DECISION-0007](../../decisions/0007-content-referenced-visuals.md).

Production content gates не требуются: вертикальный срез использует `FIXTURE-CHARACTER-AGILE` и не превращает Draft `CHAR-001…010` в production content.

## Scope

- Канонический Art Bible с визуальным языком, правилами читаемости, генерационными ограничениями, reference/anti-reference guidance и шаблоном промпта.
- Asset Pipeline с каталогами, naming convention, draft/final lifecycle, форматами, alpha/padding/pivot/PPU/import requirements и правилами хранения prompt/provenance metadata.
- Зафиксированная presentation-архитектура: gameplay root и collider не деформируются; процедурная анимация применяется только к дочернему `VisualRoot`; независимые motion channels сводятся одним pose-композитором.
- Переиспользуемый, pause-aware procedural sprite runtime с конфигурируемым motion profile, facing, locomotion, hit reaction, spawn/reset semantics и безопасной повторной инициализацией.
- Character visual reference, валидируемый общей content-системой и разрешаемый composition root без прямой зависимости gameplay model от Unity asset loading.
- Test/showcase observability для просмотра idle, движения, facing, hit reaction, pause и reset на реальном игровом масштабе.
- Генерация, пользовательское утверждение, подготовка и интеграция одного прозрачного спрайта для `FIXTURE-CHARACTER-AGILE` вместо текущего placeholder.
- Инструкция и checklist, по которым следующий character/enemy/projectile/pickup asset проходит тот же pipeline без повторного проектирования.

## Context

- Game Design: «Статус и область документа» — детальная визуальная спецификация определяется отдельно; «Концепт и core loop» и «Персонажи» — playable character является маленьким гоблином и должен оставаться читаемым в текущем масштабе игры.
- Content Design: не требуется для реализации fixture vertical slice; `CHAR-001…010` остаются Draft и используются только как будущие compatibility targets.
- [DECISION-0007](../../decisions/0007-content-referenced-visuals.md) — presentation assets представлены content-ссылками и валидируются через `ContentRegistry`.
- [DECISION-0013](../../decisions/0013-procedural-sprite-presentation.md) — gameplay/presentation transform boundary, single-writer pose composition, authoritative motion/damage signals и pause/reset lifecycle.
- Repository: `Game.Presentation` уже содержит `SpriteDefinition`; текущий Player в `Gameplay.unity` использует `PlaceholderColorRenderer`, а character visual reference ещё не доведён до runtime rendering.

## UI / observability

Отдельный player-facing экран не требуется. Модуль предоставляет test/showcase surface или эквивалентный debug harness, где можно воспроизводимо включить idle, движение, смену направления, hit reaction, pause и reset. Финальный fixture-спрайт также проверяется в основной Gameplay-сцене при реальном camera scale.

## Acceptance criteria

- `ART_DIRECTION.md` и `ASSET_PIPELINE.md` являются однозначными входами для следующей генерации и интеграции ассета; `AGENTS.md` обязывает читать их перед visual work.
- Имена, расположение, рабочие версии, production-файлы и provenance metadata соответствуют задокументированному asset contract.
- `CharacterDefinition` может ссылаться на visual через стабильный content ID; missing/wrong-type reference обнаруживается при сборке registry.
- Player presentation использует дочерний `VisualRoot`; procedural animation не изменяет gameplay root, collider geometry или authoritative movement state.
- Motion parameters конфигурируемы и не размазаны по hard-coded MonoBehaviour literals; несколько одновременных reactions композиционно объединяются вместо взаимного перезаписывания transform.
- Idle/locomotion, facing, squash/stretch, hit reaction, hit flash, spawn/reset и pause behavior воспроизводимо наблюдаемы.
- Утверждённый пользователем прозрачный спрайт `FIXTURE-CHARACTER-AGILE` импортирован по контракту и отображается в Gameplay-сцене вместо placeholder.
- Повторная инициализация и reset возвращают presentation в исходное состояние; реализация пригодна для будущих pooled entities.
- Документация содержит проверенный пошаговый checklist добавления следующего ассета.

## Проверки

- EditMode: character visual reference success/missing/wrong-type; motion-profile validation; pose composition; pause/reset semantics; root/collider invariance; asset naming/import-contract validation там, где это надёжно автоматизируется.
- PlayMode: Gameplay scene resolves the fixture character sprite; locomotion/facing/hit reaction affect only `VisualRoot`; pause stops presentation time; reset restores the baseline pose.
- Manual visual check: transparent edges, pivot, scale, silhouette readability, directional flip, motion amplitudes and hit feedback at gameplay camera scale.
- Full relevant EditMode and PlayMode regression suites.

## Out of scope

- Production `CHAR-001…010`, их финальные образы, balance и unlock metadata — IP-22 после отдельного approval.
- Production enemies, bosses, skills, fields, UI restyle, environment art, audio и покадровая character animation.
- Изменение collider, movement, combat timing или иных gameplay rules ради визуального эффекта.
- Массовая генерация каталога арта до утверждения вертикального среза и его pipeline.
