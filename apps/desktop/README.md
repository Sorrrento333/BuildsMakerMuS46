# Aplicación de escritorio WPF

Primera superficie aprobada por ADR-0004. El proyecto es una base técnica WPF
para .NET 10. Su primer flujo funcional calcula el presupuesto de puntos por
nivel y Hero Status a través de Application: clase, evolución, nivel y quest se
seleccionan desde el snapshot canónico publicado; la UI no contiene valores ni
fórmulas duplicados. La pantalla conserva ese presupuesto y permite distribuirlo
mediante `CalculateStatDistributionUseCase`. Los inputs se generan desde los IDs
de stats de la clase materializada; el resultado muestra puntos gastados,
restantes y cualquier código de error estable con una explicación visible.
Los resets son configuración del servidor: cantidad y puntos por reset empiezan
en cero, su producto aparece separado y se suma al presupuesto distribuible sin
modificar el ruleset.
La sección de borrador local guarda y carga por ID a través de los casos de uso
de Application; una carga recalcula antes de repoblar la pantalla.

## Publicación inicial

La primera distribución se publica por carpeta, autocontenida y exclusivamente
para `win-x64`. La base de usuario debe residir fuera de la carpeta de binarios.
No se consideran soportados `win-arm64`, otros sistemas operativos ni single-file.

La base productiva se ubica en
`%LOCALAPPDATA%\MuOnline.BuildPlanner\build-planner.sqlite`. Al iniciar, la
composición aplica `SqliteBuildDraftMigrations.All` antes de crear el repositorio.
Declara ruleset `1.0.0`, dataset `2026-07-24.1`, motor `0.2.0` y calcula el hash
SHA-256 sobre las rutas relativas y bytes exactos de los 27 JSON publicados.

```powershell
dotnet restore MUOnline.BuildPlanner.slnx --locked-mode
dotnet publish apps/desktop/MuOnline.BuildPlanner.App/MuOnline.BuildPlanner.App.csproj `
  --configuration Release --runtime win-x64 --self-contained true --no-restore
```

## Smoke test de publicación

`tools/smoke-tests/Test-WpfPublishedArtifact.ps1` publica el proyecto con
`--no-restore` y ejecuta el binario WPF directamente. Verifica:

- carga de SQLite nativo y versión informada;
- migración y ledger;
- escritura, cierre, reapertura y lectura;
- backup, mutación y restauración con `integrity_check = ok`;
- datos y backup fuera de la carpeta de binarios;
- reapertura de la misma base desde una copia reemplazada del artefacto;
- presencia, contenido no vacío e identidad SHA-256 de los diez archivos legales
  requeridos antes y después del reemplazo.
- presencia e identidad SHA-256 del snapshot
  `rulesets/mu-s4-global-reference/v1` dentro de ambas publicaciones;
- carga del snapshot por el adaptador productivo y reproducción de sus siete
  casos positivos y tres rechazos canónicos en ambas fases.
- configuración sintética `2 × 100 = 200` y distribución de esos puntos sobre
  los stats materializados, con total, gasto, remanente y conjunto de
  asignaciones verificados en ambas fases.
- guardado y carga revalidada de un borrador sintético mediante los casos de uso
  y `SqliteBuildDraftRepository`, antes y después de backup/restore y reemplazo.

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

Verificación local del 2026-07-23 después de incorporar el flujo de progresión:
PASS en `win-x64`, SQLite `3.53.3`, 441 archivos y 148.644.801 bytes. El
artefacto incluyó 18 JSON del snapshot y reprodujo 7/7 casos positivos y 3/3
rechazos antes y después del reemplazo simulado.

Verificación local del 2026-07-24 después de incorporar la distribución WPF:
PASS en `win-x64`, SQLite `3.53.3`, 441 archivos y 148.672.301 bytes. Ambas
fases conservaron los 18 JSON, reprodujeron 7/7 casos positivos y 3/3 rechazos
de progresión, y verificaron una distribución sintética sobre los cinco stats
materializados de la clase elegida por el smoke, con un punto gastado.

Verificación local del 2026-07-24 después de incorporar borradores WPF: PASS en
`win-x64`, SQLite `3.53.3`, 441 archivos y 148.728.406 bytes. La fase inicial
aplicó las migraciones productiva y técnica; la fase de reemplazo reconoció
ambas. El borrador `publication-smoke-draft` sobrevivió backup/restore,
reapertura y reemplazo con dataset `2026-07-24.1`, hash
`sha256:8069a5b4687066323bd70fe3863a11030c26b65f781887bb64f7c9e8b3514954`

Verificación local del 2026-07-25 después de materializar la primera fórmula
factual: PASS en `win-x64`, SQLite `3.53.3`, 450 archivos y 148.754.557 bytes.
Las dos publicaciones conservaron 27/27 JSON y el hash del snapshot; el flujo
WPF continuó limitado a progresión, distribución y borradores, sin consumir HP.
El hash resultante fue
`sha256:b45eda3083634c43aa4eaead02e02945793075a3c6ee865973c8b4776917a7ad`.
y resultado revalidado.

Verificación local del 2026-07-24 después de incorporar resets configurables:
PASS en `win-x64`, SQLite `3.53.3`, 441 archivos y 148.744.294 bytes. Ambas fases
conservaron el snapshot, calcularon `2 × 100 = 200`, habilitaron esos puntos para
distribución y revalidaron el mismo desglose desde el borrador persistido.
