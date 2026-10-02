# Academy — central symbols and overlapping motion

Repeated user report: book/portal and possibly knockback appear periodically
replaced by a purple circle. The user could not confirm whether the outer rim
changes too. Code inspection found that neighboring zones' decorative motion
had a higher sorting order than approved central symbols. Disabling a raster
zone's own motion did not prevent that overlap.

Central glyphs now sort above all seal motion layers. Gameplay overlap, timing,
ellipse and approved PNGs are preserved. Rim and approved glyph use separate
materials with explicit texture bindings as a defensive guard; texture rebinding
has not been established as the cause of the reported symptom.

EditMode assertions cover material/texture identity and sorting across the full
occurrence. A new graphics PlayMode regression compares actual central pixels
against approved references at 22 times for each of three symbols, with identical
neighboring arcane motion crossing both centers. The reference symbol is on top.

The initial render comparison exposed fixture problems: the reference needed
the same underlying rim, reinitialization needed a frame for deferred PlayMode
destruction, and comparison centers needed enough separation so one side's arcs
could not enter the other side's crop. Explicit white texture binding for solid
ink is retained as a defensive guard; the mismatch it was investigated for was
caused by insufficient fixture separation, not confirmed texture corruption.

Fresh graphics checks passed 14/14 EditMode and 2/2 PlayMode, zero skips,
`TestResults/checks/20261002T205256-168506Z/summary.json`. All 66 central crops
matched the fixed references within two RGB levels. Captures for experience,
portal and knockback were visually inspected.

Negative control restored only the old sorting orders, retaining texture guards:
`TestResults/checks/20261002T205410-984733Z/PlayMode.xml` fails the pixel regression
at EXPERIENCE 2 seconds (maximum RGB difference 6, allowed 2). This proves the
graphics regression catches the old decorative-over-symbol order. The corrected
orders were restored immediately. Final fresh run after restoration:
`TestResults/checks/20261002T205527-717228Z/EditMode.xml` 14/14 and `PlayMode.xml`
2/2, zero failures/skips. The runner did not create a reusable PASS because inputs
changed during checks; test results are saved and no full-suite PASS is claimed.
Current execution status belongs only to STATUS.md.
