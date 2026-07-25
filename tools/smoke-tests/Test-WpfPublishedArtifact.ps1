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
$requiredLegalFiles = @(
    "LICENSE.md",
    "NOTICE",
    "THIRD-PARTY-NOTICES.md",
    "licenses\Microsoft.Data.Sqlite-MIT.txt",
    "licenses\SQLitePCLRaw-Apache-2.0.txt",
    "licenses\dotnet-runtime\LICENSE.txt",
    "licenses\dotnet-runtime\THIRD-PARTY-NOTICES.txt",
    "licenses\windowsdesktop-runtime\LICENSE.txt",
    "licenses\aspnetcore-runtime\LICENSE.txt",
    "licenses\aspnetcore-runtime\THIRD-PARTY-NOTICES.txt"
)
$publishedRulesetRelativePath = "rulesets\mu-s4-global-reference\v1"
$requiredRulesetDirectories = @(
    "character-classes",
    "progression-rules",
    "reference-cases\progression\valid",
    "reference-cases\progression\invalid"
)

function Assert-PublishedLegalFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PublishDirectory,

        [Parameter(Mandatory = $true)]
        [string]$Phase
    )

    foreach ($relativePath in $requiredLegalFiles) {
        $publishedPath = Join-Path $PublishDirectory $relativePath
        if (-not (Test-Path -LiteralPath $publishedPath -PathType Leaf)) {
            throw "The $Phase publication is missing required legal file '$relativePath'."
        }

        if ((Get-Item -LiteralPath $publishedPath).Length -eq 0) {
            throw "The $Phase publication contains an empty legal file '$relativePath'."
        }
    }
}

function Assert-PublishedRuleset {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PublishDirectory,

        [Parameter(Mandatory = $true)]
        [string]$Phase
    )

    $rulesetRoot = Join-Path $PublishDirectory $publishedRulesetRelativePath
    foreach ($relativePath in $requiredRulesetDirectories) {
        $publishedPath = Join-Path $rulesetRoot $relativePath
        if (-not (Test-Path -LiteralPath $publishedPath -PathType Container)) {
            throw "The $Phase publication is missing ruleset directory '$relativePath'."
        }

        if ((Get-ChildItem -LiteralPath $publishedPath -Filter "*.json" -File).Count -eq 0) {
            throw "The $Phase publication contains no JSON files in '$relativePath'."
        }
    }
}

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

Assert-PublishedLegalFiles `
    -PublishDirectory $initialPublishDirectory `
    -Phase "initial"
Assert-PublishedRuleset `
    -PublishDirectory $initialPublishDirectory `
    -Phase "initial"

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

Assert-PublishedLegalFiles `
    -PublishDirectory $replacementPublishDirectory `
    -Phase "replacement"
Assert-PublishedRuleset `
    -PublishDirectory $replacementPublishDirectory `
    -Phase "replacement"

foreach ($relativePath in $requiredLegalFiles) {
    $initialLegalHash = (Get-FileHash `
        -LiteralPath (Join-Path $initialPublishDirectory $relativePath) `
        -Algorithm SHA256).Hash
    $replacementLegalHash = (Get-FileHash `
        -LiteralPath (Join-Path $replacementPublishDirectory $relativePath) `
        -Algorithm SHA256).Hash
    if ($initialLegalHash -cne $replacementLegalHash) {
        throw "Legal file '$relativePath' changed during the simulated update."
    }
}

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
if ($initialReport.AppliedMigrationCount -ne 2 -or
    $initialReport.AlreadyAppliedMigrationCount -ne 0 -or
    $replacementReport.AlreadyAppliedMigrationCount -ne 2) {
    throw "The build-draft and smoke migrations were not recognized across binary replacement."
}
if ($initialReport.SqliteVersion -ne $replacementReport.SqliteVersion) {
    throw "The SQLite runtime version changed between publication phases."
}
if ($initialReport.RulesetId -ne "mu-s4-global-reference" -or
    $replacementReport.RulesetId -ne $initialReport.RulesetId) {
    throw "The published progression ruleset was absent or changed between phases."
}
if ($initialReport.ApprovedProgressionCaseCount -ne 7 -or
    $replacementReport.ApprovedProgressionCaseCount -ne 7 -or
    $initialReport.RejectedProgressionCaseCount -ne 3 -or
    $replacementReport.RejectedProgressionCaseCount -ne 3) {
    throw "The published ruleset did not reproduce all 7 approved cases and 3 rejections."
}
if (-not $initialReport.SyntheticStatDistributionVerified -or
    -not $replacementReport.SyntheticStatDistributionVerified -or
    $initialReport.SyntheticStatDistributionStatCount -le 0 -or
    $replacementReport.SyntheticStatDistributionStatCount -ne
        $initialReport.SyntheticStatDistributionStatCount -or
    $initialReport.SyntheticStatDistributionSpentPoints -ne 201 -or
    $replacementReport.SyntheticStatDistributionSpentPoints -ne 201 -or
    $replacementReport.SyntheticStatDistributionRemainingPoints -ne
        $initialReport.SyntheticStatDistributionRemainingPoints) {
    throw "The published snapshot did not preserve the synthetic stat distribution."
}
if ($initialReport.SyntheticResetCount -ne 2 -or
    $replacementReport.SyntheticResetCount -ne 2 -or
    $initialReport.SyntheticPointsPerReset -ne 100 -or
    $replacementReport.SyntheticPointsPerReset -ne 100 -or
    $initialReport.SyntheticResetPoints -ne 200 -or
    $replacementReport.SyntheticResetPoints -ne 200 -or
    $replacementReport.SyntheticTotalDistributablePoints -ne
        $initialReport.SyntheticTotalDistributablePoints) {
    throw "The configurable reset contribution was not preserved."
}
if (-not $initialReport.BuildDraftPersistenceVerified -or
    -not $replacementReport.BuildDraftPersistenceVerified -or
    $initialReport.BuildDraftId -ne "publication-smoke-draft" -or
    $replacementReport.BuildDraftId -ne $initialReport.BuildDraftId -or
    $replacementReport.BuildDraftDatasetVersion -ne
        $initialReport.BuildDraftDatasetVersion -or
    $replacementReport.BuildDraftDatasetHash -ne
        $initialReport.BuildDraftDatasetHash -or
    -not $initialReport.BuildDraftDatasetHash.StartsWith("sha256:")) {
    throw "The external build draft did not survive replacement and exact revalidation."
}

$initialRulesetRoot = Join-Path $initialPublishDirectory $publishedRulesetRelativePath
$replacementRulesetRoot = Join-Path $replacementPublishDirectory $publishedRulesetRelativePath
$initialRulesetFiles = Get-ChildItem -LiteralPath $initialRulesetRoot -Recurse -File
$initialRulesetPrefix = $initialRulesetRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar
foreach ($initialRulesetFile in $initialRulesetFiles) {
    if (-not $initialRulesetFile.FullName.StartsWith(
        $initialRulesetPrefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Ruleset file '$($initialRulesetFile.FullName)' is outside the published root."
    }

    $relativePath = $initialRulesetFile.FullName.Substring(
        $initialRulesetPrefix.Length)
    $replacementRulesetFile = Join-Path $replacementRulesetRoot $relativePath
    if (-not (Test-Path -LiteralPath $replacementRulesetFile -PathType Leaf)) {
        throw "Ruleset file '$relativePath' was lost during the simulated update."
    }

    $initialRulesetHash = (Get-FileHash `
        -LiteralPath $initialRulesetFile.FullName `
        -Algorithm SHA256).Hash
    $replacementRulesetHash = (Get-FileHash `
        -LiteralPath $replacementRulesetFile `
        -Algorithm SHA256).Hash
    if ($initialRulesetHash -cne $replacementRulesetHash) {
        throw "Ruleset file '$relativePath' changed during the simulated update."
    }
}

$publishedFiles = Get-ChildItem -LiteralPath $initialPublishDirectory -Recurse -File
$publishedBytes = ($publishedFiles | Measure-Object -Property Length -Sum).Sum

Write-Output "PASS: WPF publication smoke test"
Write-Output "RID: $Runtime"
Write-Output "SQLite: $($initialReport.SqliteVersion)"
Write-Output "Published files: $($publishedFiles.Count)"
Write-Output "Published bytes: $publishedBytes"
Write-Output "Verified legal files: $($requiredLegalFiles.Count)"
Write-Output "Progression cases: $($initialReport.ApprovedProgressionCaseCount) approved, $($initialReport.RejectedProgressionCaseCount) rejected"
Write-Output "Synthetic stat distribution: $($initialReport.SyntheticStatDistributionStatCount) stats, $($initialReport.SyntheticStatDistributionSpentPoints) spent"
Write-Output "Reset configuration: $($initialReport.SyntheticResetCount) x $($initialReport.SyntheticPointsPerReset) = $($initialReport.SyntheticResetPoints)"
Write-Output "Build draft: $($initialReport.BuildDraftId), dataset $($initialReport.BuildDraftDatasetVersion)"
Write-Output "Ruleset files: $($initialRulesetFiles.Count)"
Write-Output "Artifacts: $runRoot"
