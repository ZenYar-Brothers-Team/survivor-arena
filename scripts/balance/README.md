# Automated balance runs (IP-34)

The experiment contract is versioned JSON. See [`examples/fresh.json`](examples/fresh.json)
and [`examples/preset.json`](examples/preset.json). `chains` counts independent profile
histories; `maxRunsPerChain` limits runs within each history. A fresh chain begins
with the production `ProfileCodec.Create()` state. A preset chain starts from a
copy of the referenced profile; completed runs and purchases alter only that copy.

All content identifiers are validated against the production Meta catalog before
launch. The first route field also needs a complete runtime binding. Each output
directory must be new and confined to the runner's experiment root. Runs use
random gameplay seeds. `runSpeed` is the game's existing 1×, 2×, 3× or 5× speed,
not a faster simulation engine.

To prepare a laboratory starting profile, edit
[`examples/starting-profile-declaration.json`](examples/starting-profile-declaration.json),
then run:

```powershell
python scripts/balance/prepare_preset.py --declaration scripts/balance/examples/starting-profile-declaration.json --output scripts/balance/examples/starting-profile.json
```

The output path must not exist. The generator validates IDs, ownership, caps and
recorded upgrade spending from the production Meta catalog. It starts with the
canonical initial unlocks and does not invent run receipts. `currency` and any
extra unlocks are a declared laboratory starting condition, not earned results.
Unity's `ProfileCodec` validates the resulting file again before use. Preserve
the original declaration and generated profile with experiment evidence.

The local standalone runner and analysis commands are delivered in AB-06/07.
