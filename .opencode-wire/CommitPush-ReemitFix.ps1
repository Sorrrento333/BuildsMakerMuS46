$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$ns = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\nspc"
$summary = Join-Path $ns "commitpush-reemit-summary.txt"
Remove-Item -LiteralPath $summary -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $summary -Append -Encoding utf8 }
$repoRoot = "F:\Proyecto Builds\Programa"
Push-Location $repoRoot
try {
    L ("branch-before=" + (& git rev-parse --abbrev-ref HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1))
    L ("status-before=(" + ((& git status --porcelain 2>&1 | ForEach-Object { "$_" }) -join "|") + ")")
    & git add -A *.ps1 2>&1 | ForEach-Object { L "add:$_" }
    & git add -A 2>&1 | ForEach-Object { L "add-all:$_" }
    L ("status-once=(" + ((& git status --porcelain 2>&1 | ForEach-Object { "$_" }) -join "|") + ")")
    & git commit -m "Reemit: disable CPM (NU1008) props when re-emitting reviewed locks under SDK .400" 2>&1 | ForEach-Object { L "commit:$_" }
    L ("commit-exit=" + $LASTEXITCODE)
    L ("head-local=" + (& git rev-parse HEAD 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1))
    & git push origin HEAD 2>&1 | ForEach-Object { L "push:$_" }
    L ("push-exit=" + $LASTEXITCODE)
    L ("head-upstream=" + (& git rev-parse "@{u}" 2>&1 | ForEach-Object { "$_" } | Select-Object -Last 1))
} catch {
    L ("error=" + $_.Exception.Message)
} finally {
    Pop-Location
}
L "== FIN =="
