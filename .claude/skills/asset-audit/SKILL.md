---
name: asset-audit
description: "Read-only audit of runtime art assets under Assets/Resources/Art and their content references against docs/art/ASSET_PIPELINE.md: naming, folder layout, .meta presence, PNG/import contract, orphaned or missing sprite references. Use after adding or replacing art."
argument-hint: "[folder under Assets/Resources/Art | content ID | 'full']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `asset-audit`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Read-only. The rules come from **`docs/art/ASSET_PIPELINE.md`** and `docs/art/ART_DIRECTION.md`, not from generic conventions — read the relevant sections before checking (sections 3–4 layout/naming, 7 raster contract, 8 Unity import contract, 9 `.meta`/GUID, 10 Git/binary policy). Never move, rename, regenerate or delete an asset; preview images must not exist under `Assets`.

## Checks
1. **Location**: runtime rasters only under `Assets/Resources/Art/...` as the pipeline defines; no master/concept/provenance files under `Assets/Resources`.
2. **Naming**: folder and filename follow section 4 (content-ID based, allowed role suffixes); flag deviations with the expected name.
3. **`.meta`**: every asset has a `.meta`; no `.meta` without an asset; GUID unchanged for replaced images (`git log -p -- <file>.meta` for suspicious changes). Do not "fix" `.meta` noise the Editor produced — report it.
4. **Import contract** (read the `.meta` YAML): texture type, PPU (320 world default unless documented otherwise), pivot, max size/compression, filter mode, alpha settings against sections 7–8; UI sprites vs world sprites use different defaults.
5. **References**: sprite paths/IDs referenced from `Assets/Resources/Content/Presentation/*.json` and other content JSON → the file exists and is a Sprite; **orphans** — art with no reference from any JSON/code (grep the id and path).
6. **Provenance** (if the pipeline requires a record): each approved runtime asset has the provenance the document specifies; approved-image replacements preserved the existing path.

Also flag procedural-sprite/`VisualRoot` presentation profiles whose sprite reference does not resolve.

## Output
```
## Asset audit: <scope>
Standards read: <sections>
### Violations   (path → rule/section → expected)
### Missing files / broken references
### Orphaned assets
### `.meta` issues
### Clean
### Not checked (e.g. pixel-level checks require opening the image)
```
Pixel contents (canvas size, alpha edges) can only be judged if you actually open the image — otherwise say "not verified".
