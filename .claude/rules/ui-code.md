---
paths:
  - "Assets/Game/UI/**/*.cs"
  - "Assets/Game/UI/**/*.uxml"
  - "Assets/Game/UI/**/*.uss"
---

# UI code rules

Adapted from the upstream `ui-code` rules. Repository rules in `AGENTS.md`/DECISION-0005 win on conflict.

## Enforced now
- **UI never owns or modifies game state**: View renders `ViewState`s; user actions go View → presenter intent → model/runtime. No gameplay logic in `UiToolkitGameplayView`.
- **Semantic element IDs** live in `GameplayUiElementIds`; UXML/USS/tests use the same constants; changing an ID updates the asset test.
- **Development/debug UI** is hidden in non-development builds, collapsed behind a small launcher in Editor/Development Build, bounded (≤25% width, ≤45% height at 1920x1080), scrollable, thematically tabbed (AGENTS.md).
- **Never block the game thread**: no synchronous loading or heavy work in view updates; rebuild only what changed.
- **Animations respect pause** (no presentation time advances while paused) and never drive gameplay state.
- Test presenters against fake model/view (`GameplayUiPresenterTests`); UXML/USS assets verified by `GameplayUiAssetTests`.
