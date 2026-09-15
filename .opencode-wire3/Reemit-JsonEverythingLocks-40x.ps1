$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$repo = "F:\Proyecto Builds\Programa"
$w = Join-Path $repo ".opencode-wire3"
if (-not (Test-Path -LiteralPath $w)) { New-Item -ItemType Directory -Path $w | Out-Null }

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$spikePs1 = Join-Path $repo "spikes\json-everything-source-build\Test-JsonEverythingSourceBuild.ps1"
$provenancePath = Join-Path $repo "spikes\json-everything-source-build\source-build-provenance.json"

# flags EXACTOS que el spike ajeno usa en restore (vistos byte-exacto en el ps1)
$restoreProperties = @(
    "-p:TargetFrameworks=net10.0",
    "-p:RestorePackagesWithLockFile=true",
    "-p:ManagePackageVersionsCentrally=false",
    "-p:CentralPackageTransitivePinningEnabled=false"
)
$commonBuildProperties = @(
    "-p:TreatWarningsAsErrors=false",
    "-p:AnalysisLevel=none",
    "-p:ContinuousIntegrationBuild=true",
    "-p:Deterministic=true",
    "-p:DebugType=None",
    "-p:DebugSymbols=false"
)

$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

$logPath = Join-Path $w "reemit400.log.txt"
function L($s) { $s | Out-File -LiteralPath $logPath -Append -Encoding utf8 }

L "== REEMIT400-locks-40x inicio =="
L "sdk400-exe=$(Test-Path -LiteralPath $dotnet400)"
$ver = & $dotnet400 --version 2>&1 | ForEach-Object { $_ }
L "sdk400-version=$($ver -join ';')"

# 1) re-emitir locks a banda .4xx (restore SIN locked-mode, bajo SDK .400)
$projects = @(
    "src\Json.More\Json.More.csproj",
    "src\JsonPointer\JsonPointer.csproj",
    "src\JsonSchema\JsonSchema.csproj"
)

$locksReviewedRoot = Join-Path $repo "spikes\json-everything-source-build\locks"
$lockNameByProj = @{
    "Json.More"   = "Json.More.packages.lock.json"
    "JsonPointer" = "JsonPointer.packages.lock.json"
    "JsonSchema"  = "JsonSchema.packages.lock.json"
}

foreach ($projRel in $projects) {
    $projPath = Join-Path $sourceRoot $projRel
    $projName = Split-Path -Leaf (Split-Path -Parent $projRel)
    L "== PROJ: $projRel exists=$(Test-Path -LiteralPath $projPath) =="
    if (-not (Test-Path -LiteralPath $projPath)) {
        L "  PROJ-MISSING"
        continue
    }

    # restore SIN locked-mode => re-emite el lock a banda SDK actual (.400 => Tasks.Git .4xx)
    $restoreOut = & $dotnet400 restore $projPath @restoreProperties 2>&1 | ForEach-Object { $_ }
    L "  restore-exit=$LASTEXITCODE"
    $restoreOut | ForEach-Object { L "    restore: $_" }

    # el lock regenerado queda en el dir del proyecto (sourceRoot); copiar a los revisados
    $lockSrc = Join-Path (Split-Path -Parent $projPath) "packages.lock.json"
    if (Test-Path -LiteralPath $lockSrc) {
        $lockDst = Join-Path $locksReviewedRoot $lockNameByProj[$projName]
        Copy-Item -LiteralPath $lockSrc -Destination $lockDst -Force
        L "  LOCK-COPIED: $($lockNameByProj[$projName])"
        $tasks = Select-String -LiteralPath $lockDst -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[0-9.]+' -AllMatches | Select-Object -First 1
        if ($tasks) {
            L "  LOCK-TASKSGIT-BAND: $($tasks.Matches[0].Value)"
        } else {
            L "  LOCK-TASKSGIT-BAND: none-found"
        }
    } else {
        L "  LOCK-SRC-MISSING: $lockSrc"
    }
}

# 2) verificar: restore locked-mode bajo SDK .400 sobre locks .4xx (debe pasar NU1004-clean)
L "== VERIFY locked-mode bajo .400 (gate real del spike) =="
foreach ($projRel in $projects) {
    $projPath = Join-Path $sourceRoot $projRel
    $projName = Split-Path -Leaf (Split-Path -Parent $projRel)
    if (-not (Test-Path -LiteralPath $projPath)) { continue }
    $lockedOut = & $dotnet400 restore $projPath @restoreProperties -p:RestoreLockedMode=true 2>&1 | ForEach-Object { $_ }
    L "  locked-restore-exit($projName)=$LASTEXITCODE"
    $lockedOut | ForEach-Object { L "    locked: $_" }
}

# 3) ahora el spike REAL ajeno completo bajo SDK .400 (verificación reproducibilidad + hashes provenance)
L "== SPIKE real bajo SDK .400 (debe quedar verde) =="
if (Test-Path -LiteralPath $spikePs1) {
    $spikeOut = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { $_ }
    L "  spike-exit=$LASTEXITCODE"
    $spikeOut | ForEach-Object { L "    spike: $_" }
} else {
    L "  SPIKE-PS1-MISSING: $spikePs1"
}

L "== REEMIT400 fin =="
"REEMIT400-DONE"