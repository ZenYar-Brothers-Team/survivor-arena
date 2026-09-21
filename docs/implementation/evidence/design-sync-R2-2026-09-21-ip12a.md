# IP-12A — technical packet evidence, 2026-09-21

Execution status и очередь — только в [STATUS](../STATUS.md).

## IP-12A

Решения пользователя: burst игнорирует regular cap; остальные предложенные pause/window/skip/boss/Traveler правила приняты. Palette rule согласован; затем подтверждён CHAR-001 concept = текущий fixture goblin. Approved DECISION-0029 и asset-record сохраняют ответы без переименования gameplay IDs.

Поставлены category-aware importer (world/UI/VFX, explicit body pivot и exact-path overrides), SpriteRole validation, strict production missing-art rejection, in-memory owned portrait crop, generic child adapter с узким source contract, data-driven feedback и шесть synthetic diagnostic roles. Pool reuse/disable чистит subscriptions/pose/color/sprite. Existing player implementation сохранён. Screen-shake request gate читает injected preference; IP-26 владеет actual camera/settings. Technical architecture: DECISION-0030.

Manifest содержит девять owner/role records: fixture body, CHAR-001 concept reference, UI reuse preview, шесть процедурных diagnostic roles. Генерируемые production изображения не добавлялись. Missing player shadow sprite указан явно; generic fixture shadow не подменяет его готовность.

## Checks

- Unity 6000.6.0f1, Windows; свежая CIM-проверка отсутствия Unity перед каждым запуском; `scripts/Test-Unity.ps1`, filter `^Game\.`.
- EditMode: **481/481**, failed 0, skipped 0; 2026-09-21 13:12:27Z…13:12:30Z. `TestResults/EditMode.xml`, `TestResults/EditMode.log`.
- PlayMode: **8/8**, failed 0, skipped 0; 2026-09-21 13:17:06Z…13:17:09Z. `TestResults/PlayMode.xml`, `TestResults/PlayMode.log`. Third-party tests selected: 0.
- Coverage: body import/GUID/pivot; temporary UI/VFX imports/reimports with cleanup; malformed profile, wrong/missing role/production resource; alpha border/bounds; root/collider invariance; hit+proc composition; pause, fade, reinitialize after death; pool return/disable, four copies; preference toggle/pause.
- Critical project paths included in full suite: run lifecycle, damage/death, XP/draft, active skills, wave director, enemy pool, composition, UI presenter and content loading; existing GameplaySmokeTests included.
- `python scripts/validate-art-manifest.py`: 9 unique records, file/resource/meta paths and master/v002 SHA-256 identity. This is provenance/structure, not pixel approval.
- Final Editor-only capture code compiled and ran after the suites; runtime/config/test code unchanged. `PresentationReviewCapture.Capture`, graphics enabled, output `TestResults/presentation-ip12a.png` at 1920×1080. First capture exposed missing textured material for the reference body; explicit shared Sprites/Default preview material fixed it. Final image viewed: body/alpha visible, six colored shape roles in four rows. `TestResults/PresentationCapture.log`, exit 0.
- Cold diagnostic Reset produced a single throttled PerfGuard warning (14.8 ms versus diagnostic warning threshold 10 ms). No production performance claim or balance limit is inferred.

## Visual limits and next review

Capture is a technical shape fixture, not full art or a real four-set density scene. UI crop uses existing approved body texture and centered pivot; slot acceptance in the diagnostic window was confirmed by the user on 2026-09-21 (see below). Manual sequence: Tools → Survivor Arena → Presentation Fixture Review; Idle, Left/Right, Hit/Proc, Pause/Resume, Death/Collect, Reset, 4 copies. Inspect normal and dense gameplay separately, with real active effects and 3–4 sets. Gate E requires user acceptance of visual/motion per ASSET_PIPELINE §12; test pass does not replace it. No new product decision is needed for already confirmed palette/concept/W-01.


## User acceptance — Presentation Fixture Review, 2026-09-21

После инструкции проверить Idle/Left/Right, Hit/Proc, Pause/Resume, Death/Collect, Reset, 4 copies и гоблина в UI-слоте пользователь сообщил: «всё хорошо».

Это принятие визуала/движения диагностического стенда и UI body reuse без запрошенных исправлений. Production art и реальный игровой прогон с эффектами 3–4 сетов не входили в проверку стенда; последний был явно назван отдельным оставшимся шагом. Run/report IDs отсутствуют: отзыв относится к Editor diagnostic, не к записанному gameplay run. Повторного image approval существующего концепта не требуется.
