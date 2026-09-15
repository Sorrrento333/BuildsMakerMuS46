$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$nsDir = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\nspc"
$helper = Join-Path $nsDir "do-commit-push-nu1008-fix.ps1"
$summary = Join-Path $nsDir "do-commit-push-nu1008-summary.txt"
Remove-Item -LiteralPath $summary -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $summary -Append -Encoding utf8 }

$helperContent = @'
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = "F:\Proyecto Builds\Programa"
$nsSummary = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\nspc\do-commit-push-nu1008-summary.txt"
Remove-Item -LiteralPath $nsSummary -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $nsSummary -Append -Encoding utf8 }

Push-Location $repoRoot
try {
    L ("branch=" + ((& git rev-parse --abbrev-ref HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1)))
    L ("head-before=" + ((& git rev-parse --short=7 HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1)))
    L ("upstream-before=" + ((& git rev-parse --abbrev-ref "@{upstream}" 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1)))
    L ("status-before=" + (((& git status --porcelain 2>&1 | ForEach-Object { "$_" }) -join " || ")))
    L ("changed-files=" + (((& git diff --name-only 2>&1 | ForEach-Object { "$_" }) -join " || ")))
    L ("staged-before=" + (((& git diff --cached --name-only 2>&1 | ForEach-Object { "$_" }) -join " || ")))

    & git add -A 2>&1 | ForEach-Object { L ("add:" + $_) }
    L ("add-exit=" + $LASTEXITCODE)
    L ("staged-after=" + (((& git diff --cached --name-only 2>&1 | ForEach-Object { "$_" }) -join " || ")))

    L "--- commit ---"
    & git commit -m "fix(spike): disable CPM props when re-emitting reviewed locks under SDK .400 (NU1008)" 2>&1 | ForEach-Object { L ("commit:" + $_) }
    L ("commit-exit=" + $LASTEXITCODE)
    L ("head-after-commit=" + ((& git rev-parse --short=7 HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1)))

    L "--- push ---"
    & git push origin HEAD 2>&1 | ForEach-Object { L ("push:" + $_) }
    L ("push-exit=" + $LASTEXITCODE)
    L ("head-still=" + ((& git rev-parse --short=7 HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1)))
    $local = (& git rev-parse HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1)
    L ("head-local=" + $local)
    L ("remote-head=" + ((& git ls-remote origin "refs/heads/$( (& git rev-parse --abbrev-ref HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1))" 2>&1 | ForEach-Object { ($_ -split "`t")[0] } | Select-Object -Last 1)))
} catch {
    L ("error=" + $_.Exception.Message)
} finally {
    Pop-Location
}
L "== FIN =="
'@
Set-Content -LiteralPath $helper -Value $helperContent -Encoding utf8

$stdout = Join-Path $nsDir "do-commit-push-nu1008-stdout.txt"
$stderr = Join-Path $nsDir "do-commit-push-nu1008-stderr.txt"
Remove-Item -LiteralPath $stdout, $stderr -ErrorAction SilentlyContinue
$p = Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoProfile","-NonInteractive","-ExecutionPolicy","Bypass","-File",$helper) -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru -WindowStyle Hidden
$p.WaitForExit()
L ("exit=" + $p.ExitCode)
L ("summary-bytes=" + ((Get-Item -LiteralPath $summary).Length))