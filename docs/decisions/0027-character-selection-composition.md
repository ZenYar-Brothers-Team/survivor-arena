# DECISION-0027 — Pre-run character selection ownership

Status: Proposed

Date: 2026-09-21

Related IP: IP-10A, IP-12, IP-22, IP-25, IP-26, IP-31

## Context

Approved DECISION-0026 определяет отдельный comparison baseline и authored highlights. IP-12 требует player-facing selection до полного navigation IP-26. Ранее GameplayCompositionRoot немедленно создавал loadout по startingCharacterId; informational DEV-карточки не управляли выбором.

## Implementation record

Progression владеет CharacterPresentation, CharacterComparisonBaseline, ICharacterAccessProvider, CharacterRoster и CharacterSelectionSession. UI получает session/registry, строит immutable ContentCard snapshots и передаёт intents через presenter. Bootstrap реализует ICharacterRunLauncher; нижние gameplay assemblies не зависят от UI.

На входе в Gameplay composition owner временно выключает управляемые Behaviour adapters, сохраняя их enabled flags. RunModel остаётся NotStarted. Подтверждение доступного персонажа использует существующий rollback initialization и только после успешной сборки восстанавливает adapters/запускает время. Стартовый ID из run setup задаёт initial selection, но не подменяет последующий выбор. Telemetry сохраняет actual draft.Character.Id и исходные JSON вместе с baseline.

Profile access не входит в immutable content definition. Provider может сообщать доступность/причину; перед подтверждением доступность проверяется повторно. Fixture provider хранится в памяти. Production persistence/conditions относятся к IP-25.

Selection UIDocument расположен на корне owning scene и освобождается composition owner. Это отдельная panel: вложенный UIDocument наследует panel родителя, поэтому parent под объектом с HUD мешал повторному открытию selection. PlayMode сценарий выбора Sturdy → Shutdown → Agile воспроизводит и защищает это исправление. HUD Shutdown также отсоединяет visualTree/panel.

## Consequences and limits

Техническая межслойная запись для architecture review; новых продуктовых правил или утверждённых balance values не вводит. Полный navigation/retry flow остаётся IP-26. Explicit initialization API для изолированных scene tests сохраняется. CharacterPresentation может отсутствовать у низкоуровневых test doubles, но JSON catalog и selection presenter требуют её; production loader должен применять тот же gate.

Fixture crop/icon/body placeholders не означают production asset approval. Species не закодирован в runtime. Существующие пять канонических baseline values сохраняются в Content Design; synthetic complete stats baseline используется только fixture packet.
