param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($Runtime -ne "win-x64") {
    throw "ADR-0004 currently authorizes only the win-x64 publication smoke test."
}

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot "..\.."))
$projectPath = Join-Path $repositoryRoot "apps\desktop\MuOnline.BuildPlanner.App\MuOnline.BuildPlanner.App.csproj"
$runId = [Guid]::NewGuid().ToString("N")
$runRoot = Join-Path $repositoryRoot "artifacts\wpf-publication-smoke\$runId"
$initialPublishDirectory = Join-Path $runRoot "publish-initial"
$replacementPublishDirectory = Join-Path $runRoot "publish-replacement"
$dataDirectory = Join-Path $runRoot "user-data"
$initialReportPath = Join-Path $runRoot "initialize-report.json"
$replacementReportPath = Join-Path $runRoot "verify-update-report.json"

New-Item -ItemType Directory -Path $initialPublishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $replacementPublishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $dataDirectory -Force | Out-Null

& dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --no-restore `
    --output $initialPublishDirectory
if ($LASTEXITCODE -ne 0) {
    throw "The WPF self-contained publication failed with exit code $LASTEXITCODE."
}

$initialExecutable = Join-Path $initialPublishDirectory "MuOnline.BuildPlanner.App.exe"
if (-not (Test-Path -LiteralPath $initialExecutable)) {
    throw "The published WPF executable was not found."
}

$initialArguments = @(
    "--publication-smoke",
    "--phase", "initialize",
    "--data-directory", ('"' + $dataDirectory + '"'),
    "--report-path", ('"' + $initialReportPath + '"')
)
$initialProcess = Start-Process `
    -FilePath $initialExecutable `
    -ArgumentList $initialArguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden
if ($initialProcess.ExitCode -ne 0) {
    throw "The initialize smoke phase failed with exit code $($initialProcess.ExitCode)."
}

Copy-Item -Path (Join-Path $initialPublishDirectory "*") `
    -Destination $replacementPublishDirectory `
    -Recurse `
    -Force

$replacementExecutable = Join-Path $replacementPublishDirectory "MuOnline.BuildPlanner.App.exe"
$replacementArguments = @(
    "--publication-smoke",
    "--phase", "verify-update",
    "--data-directory", ('"' + $dataDirectory + '"'),
    "--report-path", ('"' + $replacementReportPath + '"')
)
$replacementProcess = Start-Process `
    -FilePath $replacementExecutable `
    -ArgumentList $replacementArguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden
if ($replacementProcess.ExitCode -ne 0) {
    throw "The verify-update smoke phase failed with exit code $($replacementProcess.ExitCode)."
}

$initialReport = Get-Content -Raw -Encoding utf8 -LiteralPath $initialReportPath | ConvertFrom-Json
$replacementReport = Get-Content -Raw -Encoding utf8 -LiteralPath $replacementReportPath | ConvertFrom-Json

if (-not $initialReport.Success -or -not $replacementReport.Success) {
    throw "A publication smoke report declared failure."
}
if ($initialReport.IntegrityResult -ne "ok" -or $replacementReport.IntegrityResult -ne "ok") {
    throw "SQLite integrity verification did not return ok in both phases."
}
if ($initialReport.PersistedValue -ne "persisted-across-update" -or
    $replacementReport.PersistedValue -ne "persisted-across-update") {
    throw "The synthetic value did not survive backup/restore and binary replacement."
}
if (-not $initialReport.DataOutsideBinaryDirectory -or
    -not $replacementReport.DataOutsideBinaryDirectory) {
    throw "The smoke database was stored inside a binary directory."
}
if ($initialReport.AppliedMigrationCount -ne 1 -or
    $replacementReport.AlreadyAppliedMigrationCount -ne 1) {
    throw "The migration was not applied once and recognized after binary replacement."
}
if ($initialReport.SqliteVersion -ne $replacementReport.SqliteVersion) {
    throw "The SQLite runtime version changed between publication phases."
}

$publishedFiles = Get-ChildItem -LiteralPath $initialPublishDirectory -Recurse -File
$publishedBytes = ($publishedFiles | Measure-Object -Property Length -Sum).Sum

Write-Output "PASS: WPF publication smoke test"
Write-Output "RID: $Runtime"
Write-Output "SQLite: $($initialReport.SqliteVersion)"
Write-Output "Published files: $($publishedFiles.Count)"
Write-Output "Published bytes: $publishedBytes"
Write-Output "Artifacts: $runRoot"
