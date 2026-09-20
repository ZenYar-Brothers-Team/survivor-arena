# Claude Code PostToolUse hook (Write|Edit): syntax-check content JSON.
# Only reads the edited file; never modifies anything. Exit 0 = ok/not applicable,
# exit 2 = blocking feedback shown to Claude (invalid JSON in a content file).
$ErrorActionPreference = 'Stop'

try {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }
    $payload = $raw | ConvertFrom-Json
    $path = $payload.tool_input.file_path
    if ([string]::IsNullOrWhiteSpace($path)) { exit 0 }

    $normalized = $path -replace '\\', '/'
    if ($normalized -notmatch '(^|/)Assets/Resources/Content/.+\.json$') { exit 0 }
    if (-not (Test-Path -LiteralPath $path)) { exit 0 }

    $null = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
    exit 0
}
catch {
    [Console]::Error.WriteLine("Invalid JSON in content file '$path': $($_.Exception.Message)")
    [Console]::Error.WriteLine("Fix the syntax before continuing - the content catalog will throw on load.")
    exit 2
}
