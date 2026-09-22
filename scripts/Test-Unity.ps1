param(
    [string]$UnityPath,
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent),
    [string]$TestFilter = '^Game\.',
    [ValidateSet('EditMode', 'PlayMode')]
    [string[]]$Platforms = @('EditMode', 'PlayMode')
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $versionLine = Get-Content (Join-Path $ProjectPath 'ProjectSettings/ProjectVersion.txt') |
        Where-Object { $_ -like 'm_EditorVersion:*' } |
        Select-Object -First 1
    $version = ($versionLine -split ':', 2)[1].Trim()
    $candidates = @(
        "D:\Unity\editor\$version\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
    )
    $UnityPath = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($UnityPath) -or -not (Test-Path -LiteralPath $UnityPath)) {
    throw 'Unity executable was not found. Pass it with -UnityPath.'
}

$resultsDirectory = Join-Path $ProjectPath 'TestResults'
New-Item -ItemType Directory -Force -Path $resultsDirectory | Out-Null

function Invoke-UnityTests([string]$Platform) {
    $resultPath = Join-Path $resultsDirectory "$Platform.xml"
    $logPath = Join-Path $resultsDirectory "$Platform.log"
    if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
    $arguments = @(
        '-batchmode', '-nographics',
        '-projectPath', $ProjectPath,
        '-runTests', '-testPlatform', $Platform,
        '-testResults', $resultPath,
        '-logFile', $logPath
    )
    if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
        $arguments += @('-testFilter', $TestFilter)
    }

    # Start-Process -Wait may wait for the entire descendant tree on Windows.
    # Unity can leave UPM/licensing helpers alive after the Editor has exited,
    # which turns a fast test failure into a several-minute apparent hang.
    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    $process.WaitForExit()
    $timer.Stop()
    $exitCode = $process.ExitCode
    if (-not (Test-Path -LiteralPath $resultPath)) {
        throw "$Platform tests did not produce a result file after $([math]::Round($timer.Elapsed.TotalSeconds, 1)) s. See $logPath."
    }

    [xml]$results = Get-Content -LiteralPath $resultPath
    $run = $results.'test-run'
    Write-Host "${Platform}: $($run.passed)/$($run.total) passed; Unity process $([math]::Round($timer.Elapsed.TotalSeconds, 1)) s"
    if ($exitCode -ne 0 -or [int]$run.failed -ne 0 -or [int]$run.total -eq 0) {
        throw "$Platform tests failed. See $resultPath and $logPath."
    }
}

foreach ($platform in $Platforms) { Invoke-UnityTests $platform }
