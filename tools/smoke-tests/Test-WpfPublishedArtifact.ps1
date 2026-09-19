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
    "items",
    "skills",
    "progression-rules",
    "reference-cases\progression\valid",
    "reference-cases\progression\invalid"
)
$expectedPublishedFormulaReferences = @(
    "formula-ag-dark-knight@1.0.0",
    "formula-ag-dark-lord@1.0.0",
    "formula-ag-dark-wizard@1.0.0",
    "formula-ag-fairy-elf@1.0.0",
    "formula-ag-magic-gladiator@1.0.0",
    "formula-ag-regen-dark-knight@1.0.0",
    "formula-ag-regen-dark-lord@1.0.0",
    "formula-ag-regen-dark-wizard@1.0.0",
    "formula-ag-regen-fairy-elf@1.0.0",
    "formula-ag-regen-magic-gladiator@1.0.0",
    "formula-ag-summoner@1.0.0",
    "formula-berserker-percent-summoner@1.0.0",
    "formula-defense-dark-knight@1.0.0",
    "formula-defense-dark-lord@1.0.0",
    "formula-defense-dark-wizard@1.0.0",
    "formula-defense-fairy-elf@1.0.0",
    "formula-defense-magic-gladiator@1.0.0",
    "formula-defense-summoner@1.0.0",
    "formula-hp-dark-knight@1.0.0",
    "formula-hp-dark-lord@1.0.0",
    "formula-hp-dark-wizard@1.1.0",
    "formula-hp-fairy-elf@1.0.0",
    "formula-hp-magic-gladiator@1.0.0",
    "formula-hp-summoner@1.0.0",
    "formula-innovation-percent-summoner@1.0.0",
    "formula-mana-dark-knight@1.0.0",
    "formula-mana-dark-lord@1.0.0",
    "formula-mana-dark-wizard@1.0.0",
    "formula-mana-fairy-elf@1.0.0",
    "formula-mana-magic-gladiator@1.0.0",
    "formula-mana-regen-dark-knight@1.0.0",
    "formula-mana-regen-dark-lord@1.0.0",
    "formula-mana-regen-dark-wizard@1.0.0",
    "formula-mana-regen-fairy-elf@1.0.0",
    "formula-mana-regen-magic-gladiator@1.0.0",
    "formula-mana-summoner@1.0.0",
    "formula-max-damage-dark-knight@1.0.0",
    "formula-max-damage-dark-lord@1.0.0",
    "formula-max-damage-fairy-elf@1.0.0",
    "formula-max-damage-magic-gladiator@1.0.0",
    "formula-max-damage-summoner@1.0.0",
    "formula-max-wizardry-magic-gladiator@1.0.0",
    "formula-max-wizardry-summoner@1.0.0",
    "formula-min-damage-dark-knight@1.0.0",
    "formula-min-damage-dark-lord@1.0.0",
    "formula-min-damage-fairy-elf@1.0.0",
    "formula-min-damage-magic-gladiator@1.0.0",
    "formula-min-damage-summoner@1.0.0",
    "formula-min-wizardry-magic-gladiator@1.0.0",
    "formula-min-wizardry-summoner@1.0.0",
    "formula-pvm-attack-rate-dark-knight@1.0.0",
    "formula-pvm-attack-rate-dark-lord@1.0.0",
    "formula-pvm-attack-rate-dark-wizard@1.0.0",
    "formula-pvm-attack-rate-fairy-elf@1.0.0",
    "formula-pvm-attack-rate-magic-gladiator@1.0.0",
    "formula-pvm-attack-rate-summoner@1.0.0",
    "formula-pvm-defense-rate-dark-knight@1.0.0",
    "formula-pvm-defense-rate-dark-lord@1.0.0",
    "formula-pvm-defense-rate-dark-wizard@1.0.0",
    "formula-pvm-defense-rate-fairy-elf@1.0.0",
    "formula-pvm-defense-rate-magic-gladiator@1.0.0",
    "formula-pvm-defense-rate-summoner@1.0.0",
    "formula-pvp-attack-rate-dark-knight@1.0.0",
    "formula-pvp-attack-rate-dark-lord@1.0.0",
    "formula-pvp-attack-rate-dark-wizard@1.0.0",
    "formula-pvp-attack-rate-fairy-elf@1.0.0",
    "formula-pvp-attack-rate-magic-gladiator@1.0.0",
    "formula-pvp-attack-rate-summoner@1.0.0",
    "formula-pvp-defense-rate-dark-knight@1.0.0",
    "formula-pvp-defense-rate-dark-lord@1.0.0",
    "formula-pvp-defense-rate-dark-wizard@1.0.0",
    "formula-pvp-defense-rate-fairy-elf@1.0.0",
    "formula-pvp-defense-rate-magic-gladiator@1.0.0",
    "formula-pvp-defense-rate-summoner@1.0.0",
    "formula-reflect-percent-summoner@1.0.0",
    "formula-sd-dark-knight@1.0.0",
    "formula-sd-dark-lord@1.0.0",
    "formula-sd-dark-wizard@1.0.0",
    "formula-sd-fairy-elf@1.0.0",
    "formula-sd-magic-gladiator@1.0.0",
    "formula-sd-summoner@1.0.0",
    "formula-weakness-percent-summoner@1.0.0",
    "formula-combo-base-dark-knight@1.0.0",
    "formula-critical-damage-dark-lord@1.0.0",
    "formula-damage-buff-fairy-elf@1.0.0",
    "formula-defense-buff-fairy-elf@1.0.0",
    "formula-fenrir-base-max-damage-dark-knight@1.0.0",
    "formula-fenrir-base-max-damage-dark-wizard@1.0.0",
    "formula-fenrir-base-min-damage-dark-knight@1.0.0",
    "formula-fenrir-base-min-damage-dark-wizard@1.0.0",
    "formula-fireburst-bonus-max-damage-dark-lord@1.0.0",
    "formula-fireburst-bonus-min-damage-dark-lord@1.0.0",
    "formula-fortitude-percent-dark-knight@1.0.0",
    "formula-guild-member-capacity-dark-lord@1.0.0",
    "formula-heal-fairy-elf@1.0.0",
    "formula-max-wizardry-dark-wizard@1.0.0",
    "formula-min-wizardry-dark-wizard@1.0.0",
    "formula-nova-max-spell-damage-dark-wizard@1.0.0",
    "formula-skill-damage-death-stab-dark-knight@1.0.0",
    "formula-skill-damage-impale-dark-knight@1.0.0",
    "formula-skill-damage-multi-shot-fairy-elf@1.0.0",
    "formula-skill-damage-penetration-fairy-elf@1.0.0",
    "formula-skill-damage-rageful-blow-dark-knight@1.0.0",
    "formula-skill-damage-twisting-slash-dark-knight@1.0.0",
    "formula-skill-hp-buff-swell-life-dark-knight@1.0.0",
    "formula-skill-percent-dark-knight@1.0.0",
    "formula-skill-percent-dark-lord@1.0.0",
    "formula-soul-barrier-percent-dark-wizard@1.0.0",
    "formula-speed-dark-knight@1.0.0",
    "formula-speed-dark-lord@1.0.0",
    "formula-speed-dark-wizard@1.0.0",
    "formula-speed-fairy-elf@1.0.0",
    "formula-speed-magic-gladiator@1.0.0",
    "formula-speed-summoner@1.0.0"
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
if ($initialReport.AppliedMigrationCount -ne 3 -or
    $initialReport.AlreadyAppliedMigrationCount -ne 0 -or
    $replacementReport.AlreadyAppliedMigrationCount -ne 3) {
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
$initialReferenceDifference = Compare-Object `
    -ReferenceObject $expectedPublishedFormulaReferences `
    -DifferenceObject $initialReport.PublishedFormulaReferences
$replacementReferenceDifference = Compare-Object `
    -ReferenceObject $expectedPublishedFormulaReferences `
    -DifferenceObject $replacementReport.PublishedFormulaReferences
if (-not $initialReport.PublishedFormulaContextVerified -or
    -not $replacementReport.PublishedFormulaContextVerified -or
    $initialReport.PublishedFormulaCount -ne 114 -or
    $replacementReport.PublishedFormulaCount -ne 114 -or
    $initialReport.PublishedFormulaReferences.Count -ne 114 -or
    $replacementReport.PublishedFormulaReferences.Count -ne 114 -or
    $null -ne $initialReferenceDifference -or
    $null -ne $replacementReferenceDifference -or
    ($replacementReport.PublishedFormulaReferences -join "|") -ne
        ($initialReport.PublishedFormulaReferences -join "|") -or
    $initialReport.ApprovedPublishedFormulaCaseCount -ne 456 -or
    $replacementReport.ApprovedPublishedFormulaCaseCount -ne 456) {
    throw "The published artifact did not reproduce all contextual and arithmetic formula traces."
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

if (-not $initialReport.BuildPersistenceVerified -or
    -not $replacementReport.BuildPersistenceVerified -or
    $initialReport.BuildId -ne "publication-smoke-build" -or
    $replacementReport.BuildId -ne $initialReport.BuildId -or
    $initialReport.BuildStatCount -le 0 -or
    $initialReport.BuildStatCount -ne
        $replacementReport.BuildStatCount -or
    $initialReport.BuildStatCount -ne
        $initialReport.SyntheticStatDistributionStatCount) {
    throw "The full character build did not survive replacement and exact revalidation."
}

if (-not $initialReport.BuildListVerified -or
    -not $replacementReport.BuildListVerified -or
    $initialReport.PersistedBuildCount -lt 1 -or
    $replacementReport.PersistedBuildCount -ne
        $initialReport.PersistedBuildCount) {
    throw "The saved-build listing did not expose the persisted build in both phases."
}

if (-not $initialReport.PublishedBuildEvaluationVerified -or
    -not $replacementReport.PublishedBuildEvaluationVerified -or
    $initialReport.PublishedBuildFormulaCount -le 0 -or
    $initialReport.PublishedBuildFormulaCount -ne
        $replacementReport.PublishedBuildFormulaCount -or
    $initialReport.PublishedBuildFormulaCount -gt
        $initialReport.PublishedFormulaCount) {
    throw "The published artifact did not reproduce the full-build grouped evaluation."
}

if (-not $initialReport.PublishedBuildCalculationTraceVerified -or
    -not $replacementReport.PublishedBuildCalculationTraceVerified -or
    $initialReport.PublishedBuildCalculationTraceFormulaCount -le 0 -or
    $initialReport.PublishedBuildCalculationTraceFormulaCount -ne
        $initialReport.PublishedBuildFormulaCount -or
    $replacementReport.PublishedBuildCalculationTraceFormulaCount -ne
        $initialReport.PublishedBuildCalculationTraceFormulaCount -or
    $initialReport.PublishedBuildCalculationTraceDependencyCount -lt 0 -or
    $replacementReport.PublishedBuildCalculationTraceDependencyCount -ne
        $initialReport.PublishedBuildCalculationTraceDependencyCount) {
    throw "The published artifact did not reproduce the high-level build calculation trace."
}

if (-not $initialReport.ItemCatalogVerified -or
    -not $replacementReport.ItemCatalogVerified -or
    $initialReport.ItemCatalogItemCount -ne 3 -or
    $replacementReport.ItemCatalogItemCount -ne 3 -or
    -not $initialReport.SyntheticItemEquipVerified -or
    -not $replacementReport.SyntheticItemEquipVerified) {
    throw "The published bounded item catalog did not materialize or evaluate in both phases."
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
Write-Output "Published formulas: $($initialReport.PublishedFormulaReferences -join ', '), $($initialReport.ApprovedPublishedFormulaCaseCount) contextual cases"
Write-Output "Full build evaluation: $($initialReport.PublishedBuildFormulaCount) formulas grouped"
Write-Output "High-level build calculation trace: $($initialReport.PublishedBuildCalculationTraceFormulaCount) formulas, $($initialReport.PublishedBuildCalculationTraceDependencyCount) dependency edges"
Write-Output "Bounded item catalog: $($initialReport.ItemCatalogItemCount) items, equip evaluated: $($initialReport.SyntheticItemEquipVerified)"
Write-Output "Build draft: $($initialReport.BuildDraftId), dataset $($initialReport.BuildDraftDatasetVersion)"
Write-Output "Full build: $($initialReport.BuildId), $($initialReport.BuildStatCount) stats"
Write-Output "Saved builds listed: $($initialReport.PersistedBuildCount)"
Write-Output "Ruleset files: $($initialRulesetFiles.Count)"
Write-Output "Artifacts: $runRoot"
