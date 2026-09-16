$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w = Join-Path $repo ".opencode-wire"
$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"

$spikeInputRoot = Join-Path $repo "spikes\json-everything-source-build"
$locksRoot = Join-Path $spikeInputRoot "locks"
$provenancePath = Join-Path $spikeInputRoot "source-build-provenance.json"
$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$repeatSourceRoot = Join-Path $artifactRoot "source-repeat"
$outputRoot = Join-Path $artifactRoot "output"

$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;$env:PATH"

$log = Join-Path $w "recapture400.log.txt"

function Add-Log($msg) {
    "$(Get-Date -Format 'HH:mm:ss.fff') $msg" | Tee-Object -LiteralPath $log -Append -Encoding UTF8 | Out-Null
}

function Invoke-Checked {
    param([string]$FilePath, [string[]]$Arguments, [string]$WorkingDirectory)
    Add-Log "EXEC: $FilePath $($Arguments -join ' ')"
    Push-Location -LiteralPath $WorkingDirectory
    try {
        & $FilePath @Arguments 2>&1 | ForEach-Object { Add-Log "    OUT: $_" }
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed ($LASTEXITCODE): $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

Add-Log "=== RECAPTURE400 START ==="
Add-Log "dotnet400-exists=$(Test-Path -LiteralPath $dotnet400)"
$sdkVer = (& $dotnet400 --version).Trim()
Add-Log "sdk400-version=$sdkVer"

Add-Log "artifactRoot-exists=$(Test-Path -LiteralPath $artifactRoot)"
Add-Log "sourceRoot-exists=$(Test-Path -LiteralPath $sourceRoot)"
Add-Log "sourceGit-exists=$(Test-Path -LiteralPath (Join-Path $sourceRoot '.git'))"

# Copy the reviewed locks into the source (like the spike does at restore time)
function Copy-ReviewedLocks {
    param([string]$DestinationSourceRoot)
    $lockMappings = [ordered]@{
        "Json.More.packages.lock.json" = "src/Json.More/packages.lock.json"
        "JsonPointer.packages.lock.json" = "src/JsonPointer/packages.lock.json"
        "JsonSchema.packages.lock.json" = "src/JsonSchema/packages.lock.json"
    }
    foreach ($entry in $lockMappings.GetEnumerator()) {
        $srcLock = Join-Path $locksRoot $entry.Key
        $dstLock = Join-Path $DestinationSourceRoot $entry.Value
        if (Test-Path -LiteralPath $srcLock) {
            Copy-Item -LiteralPath $srcLock -Destination $dstLock -Force
            Add-Log "COPIED-LOCK $($entry.Key) -> $($entry.Value)"
        } else {
            Add-Log "WARN: lock input missing: $srcLock"
        }
    }
}

if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot '.git'))) {
    Add-Log "source not cloned yet - spike must clone first; aborting recapture here."
    Add-Log "=== RECAPTURE400 END (source-missing) ==="
    exit 3
}

$sourceCommit = (& git -C $sourceRoot rev-parse HEAD).Trim()
Add-Log "source-head=$sourceCommit"

# Restore WITHOUT locked mode under SDK .400 to re-emit locks to banda Tasks.Git .4xx
$restoreProperties = @(
    "-p:TargetFrameworks=net10.0",
    "-p:RestorePackagesWithLockFile=true",
    "-p:ManagePackageVersionsCentrally=false",
    "-p:CentralPackageTransitivePinningEnabled=false"
)

$projects = @(
    "src\Json.More\Json.More.csproj",
    "src\JsonPointer\JsonPointer.csproj",
    "src\JsonSchema\JsonSchema.csproj"
)

$newTasksGitPerProject = @{}
foreach ($projRel in $projects) {
    $projPath = Join-Path $sourceRoot $projRel
    if (-not (Test-Path -LiteralPath $projPath)) {
        Add-Log "PROJ-MISSING: $projPath"
        continue
    }
    Add-Log "RESTORE-NOLOCKED: $projRel"
    # Run restore WITHOUT --locked-mode so it resolves to SDK banda .4xx and rewrites the lock
    Invoke-Checked $dotnet400 @("restore", $projPath) + $restoreProperties $sourceRoot
    $lockPath = Join-Path (Split-Path -Parent $projPath) "packages.lock.json"
    if (Test-Path -LiteralPath $lockPath) {
        Add-Log "LOCK-REEMITTED: $projRel"
        $tasksGit = Select-String -LiteralPath $lockPath -Pattern '"Microsoft.Build.Tasks.Git"\s*:\s*"([0-9.]+)"' | Select-Object -First 1
        if ($tasksGit) {
            $newTasksGitPerProject[(Split-Path -Leaf (Split-Path -Parent $projPath))] = $tasksGit.Matches[0].Groups[1].Value
            Add-Log "TASKSGIT-NEW: $(Split-Path -Leaf (Split-Path -Parent $projPath))=$($tasksGit.Matches[0].Groups[1].Value)"
        }
    } else {
        Add-Log "LOCK-NOTFOUND-AFTER-RESTORE: $projRel"
    }
}

# Copy re-emitted locks back to the reviewed locks root
foreach ($projRel in $projects) {
    $projName = Split-Path -Leaf (Split-Path -Parent $projRel)
    $lockPath = Join-Path $sourceRoot (Join-Path (Split-Path -Parent $projRel) "packages.lock.json")
    if (Test-Path -LiteralPath $lockPath) {
        $dest = Join-Path $locksRoot "$projName.packages.lock.json"
        Copy-Item -LiteralPath $lockPath -Destination $dest -Force
        Add-Log "LOCK-COPIED-TO-INPUT: $projName.packages.lock.json"
    }
}

Add-Log "TASKSGIT-SUMMARY: $($newTasksGitPerProject | ConvertTo-Json -Compress)"
Add-Log "=== RECAPTURE400 END ==="
