param(
    [string]$UnityPath,
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent),
    [string]$TestFilter = '^Game\.',
    [ValidateSet('EditMode', 'PlayMode')]
    [string[]]$Platforms = @('EditMode', 'PlayMode'),
    [int]$TimeoutSeconds = 300,
    [switch]$Reuse
)

$ErrorActionPreference = 'Stop'
$boundProject = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent)).TrimEnd('\', '/')
if ([IO.Path]::GetFullPath($ProjectPath).TrimEnd('\', '/') -ne $boundProject) {
    throw 'This runner is bound to its own repository. Run the target project runner instead.'
}
$runnerArgs = @((Join-Path $PSScriptRoot 'check_project.py'), '--scope', 'code',
    '--filter', $TestFilter, '--platforms') + $Platforms + @('--timeout', "$TimeoutSeconds")
if ($UnityPath) { $runnerArgs += @('--unity-path', $UnityPath) }
if ($Reuse) { $runnerArgs += '--reuse' }
& python @runnerArgs
exit $LASTEXITCODE
