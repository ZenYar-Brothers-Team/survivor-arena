---
paths:
  - "Assets/Game/Audio/**/*.cs"
  - "Assets/Resources/Audio/**/*"
  - "Assets/Resources/Content/Audio/**/*.json"
  - "docs/audio/**/*"
  - "scripts/audio/**/*"
---

# Audio ownership and checks

- Context: `docs/implementation/modules/IP-33-production-audio.md`, its referenced Audio plan and DECISION-0038. Read the current IP-33 entry in STATUS for remaining listening gates.
- `Game.Audio` owns catalogs, playback, voice limits, music and preview implementation. `Game.Settings` owns preferences and `IAudioPreview`; Bootstrap composes the services. Gameplay/UI must not depend on Audio.
- DTOs live in `Assets/Game/Audio/Json`; tuning and bindings in `Assets/Resources/Content/Audio/ProductionAudio.json`. No per-entity audio system or AudioSource on projectiles.
- Shared tracks/cues live in `Resources/Audio/Music` and `Sfx`; field atmosphere in `Ambience/<fieldId>`, selected by `fieldAmbiences[].fieldId`. No binding means no ambience, not a fallback to FIELD-001.
- Preserve real-time cooldowns, bounded voices, Master/Music/SFX routing and the distinct gameplay/UI/music pause policies in IP-33. Automatic tests must remain silent on system speakers.
- Every clip must have matching source/license/processing/hash evidence in `docs/audio/SOURCES.json`. Move an asset with its `.meta`; update paths in the catalog, source record, importer/tests together.
- Clips/catalog tuning: `python scripts/check_project.py --scope audio` checks integrity and Audio EditMode tests. Code, schema, lifecycle or cross-system changes require the relevant smoke/full scope; IP acceptance wins. Listening is separate from automated PASS.
