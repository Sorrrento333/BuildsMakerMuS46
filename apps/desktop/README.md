# Aplicación de escritorio WPF

Primera superficie aprobada por ADR-0004. El proyecto es una base técnica WPF
para .NET 10 y no contiene todavía flujos, datos ni fórmulas de MU Online.

## Publicación inicial

La primera distribución se publica por carpeta, autocontenida y exclusivamente
para `win-x64`. La base de usuario debe residir fuera de la carpeta de binarios.
No se consideran soportados `win-arm64`, otros sistemas operativos ni single-file.

```powershell
dotnet restore MUOnline.BuildPlanner.slnx --locked-mode
dotnet publish apps/desktop/MuOnline.BuildPlanner.App/MuOnline.BuildPlanner.App.csproj `
  --configuration Release --runtime win-x64 --self-contained true --no-restore
```

## Smoke test de publicación

`tools/smoke-tests/Test-WpfPublishedArtifact.ps1` publica el proyecto con
`--no-restore`, ejecuta el binario WPF directamente y usa únicamente un schema
sintético. Verifica:

- carga de SQLite nativo y versión informada;
- migración y ledger;
- escritura, cierre, reapertura y lectura;
- backup, mutación y restauración con `integrity_check = ok`;
- datos y backup fuera de la carpeta de binarios;
- reapertura de la misma base desde una copia reemplazada del artefacto;
- presencia, contenido no vacío e identidad SHA-256 de los diez archivos legales
  requeridos antes y después del reemplazo.

La publicación incorpora `LICENSE.md`, `NOTICE`, `THIRD-PARTY-NOTICES.md`, los
textos de Microsoft.Data.Sqlite y SQLitePCLRaw, y las licencias/avisos de los
runtime packs .NET, Windows Desktop y ASP.NET resueltos por la restauración. Los
avisos de runtime se toman del paquete exacto seleccionado; no se fijan a la
versión instalada en una máquina concreta.

Los reportes JSON y binarios quedan bajo `artifacts/`, ignorado por Git.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File tools/smoke-tests/Test-WpfPublishedArtifact.ps1
```

Verificación local del 2026-07-18: PASS en `win-x64`, SQLite `3.53.3`, 407
archivos y 148.339.336 bytes publicados. La ejecución inicial aplicó una
migración sintética; la ejecución desde los binarios de reemplazo reconoció la
migración y conservó el valor restaurado.

Verificación remota del 2026-07-19 UTC: PASS en GitHub Actions, run
`29666817493`, sobre Microsoft Windows Server 2025 (`windows-2025-vs2026`) y SDK
.NET `10.0.302`. La auditoría directa/transitiva no encontró paquetes
vulnerables en los cinco proyectos. El smoke cargó SQLite `3.53.3` y publicó 407
archivos/148.442.430 bytes; ambas fases superaron las mismas aserciones de
migración, integridad, backup/restore y persistencia externa. No se infiere
soporte para ningún RID adicional.

Verificación local del 2026-07-19 después de incorporar avisos: PASS con SDK
`10.0.301`, runtime packs `10.0.9`, SQLite `3.53.3`, 417 archivos y 148.506.472
bytes. Se comprobaron diez archivos legales en ambas publicaciones y sus hashes
permanecieron idénticos durante la actualización simulada.
