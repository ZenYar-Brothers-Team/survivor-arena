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
- Development/debug surfaces follow the full contract below (DECISION-0005).
- **Never block the game thread**: no synchronous loading or heavy work in view updates; rebuild only what changed.
- **Animations respect pause** (no presentation time advances while paused) and never drive gameplay state.
- Test presenters against fake model/view (`GameplayUiPresenterTests`); UXML/USS assets verified by `GameplayUiAssetTests`.

## Development UI stays compact and non-obstructive

Development/debug controls must be hidden in non-development builds and
collapsed by default behind a small launcher in Editor/Development Build.
Expanded tooling uses a bounded drawer with thematic tabs and scrolling for
long content; never append new controls to an unbounded horizontal strip over
the gameplay viewport. At the 1920x1080 reference resolution, an expanded
debug surface should stay within 25% of viewport width and 45% of viewport
height unless a dedicated full-screen diagnostic view is explicitly required.
Debug commands still follow the normal View -> presenter intent -> model/runtime
boundary and must not make UI the owner of gameplay state. See DECISION-0005.
