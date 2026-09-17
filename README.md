# Survivor Arena

Unity/C# survivor-like project with a Git-managed design-to-implementation workflow.

## Project documentation

- [Game Design](docs/Game_design.md)
- [Content Design](docs/Content_design.md)
- [Implementation Plan](docs/implementation/README.md)
- [Current implementation status](docs/implementation/STATUS.md)
- [AI implementation workflow](docs/implementation/WORKFLOW.md)

For implementation work, follow [`AGENTS.md`](AGENTS.md). When no IP module is named explicitly, the next task is the first numerically ordered module marked `Ready` in the status file.

## Tests

Run both the EditMode suite and the gameplay PlayMode smoke test from PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Unity.ps1
```

If Unity is installed outside the detected project-version paths, pass `-UnityPath`. Reports are written to the ignored `TestResults/` directory. Do not add `-quit`: the installed Unity Test Framework exits by itself after writing the reports.
