# Repository instructions

The canonical product documents are `docs/Game_design.md`, `docs/Content_design.md`, and `docs/implementation/`.

When implementing an IP module:

1. Follow `docs/implementation/WORKFLOW.md`.
2. Use `docs/implementation/STATUS.md` as the only source of execution status and select the first numerically ordered `Ready` module unless the user names another module.
3. Read the selected file in `docs/implementation/modules/` and only the design sections and content IDs listed in its Context section.
4. Treat Game Design as canonical for system rules, Content Design for concrete entities, the IP module for scope, and the repository for implementation state.
5. Never implement Draft content as production content or silently invent a missing product rule.
6. Complete the module's checks, update `STATUS.md`, and synchronize affected design documents in the same change.
7. Record cross-layer or user-approved deviations in `docs/decisions/`; do not turn an implementation workaround into game design automatically.

Do not duplicate module status inside module specification files.
