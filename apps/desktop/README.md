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
Después de distribuir, la pantalla obtiene del catálogo las fórmulas publicadas
aplicables, ofrece una selección genérica por referencia exacta y ejecuta
`CalculateCharacterFormulaUseCase`. Muestra HP y Mana para las seis familias,
y AG para Dark Wizard, con traza
contextual de los valores realmente consumidos más traza
aritmética. WPF no construye inputs contextuales ni duplica IDs, bases,
expresiones o constantes de las fórmulas.

## Publicación inicial

La primera distribución se publica por carpeta, autocontenida y exclusivamente
para `win-x64`. La base de usuario debe residir fuera de la carpeta de binarios.
No se consideran soportados `win-arm64`, otros sistemas operativos ni single-file.

La base productiva se ubica en
`%LOCALAPPDATA%\MuOnline.BuildPlanner\build-planner.sqlite`. Al iniciar, la
composición aplica `SqliteBuildDraftMigrations.All` antes de crear el repositorio.
Declara ruleset `1.0.0`, dataset `2026-07-28.2`, motor `0.2.0` y calcula el hash
SHA-256 sobre las rutas relativas y bytes exactos de los 188 JSON publicados.

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
- carga del snapshot por el adaptador productivo y reproducción de sus ocho
  casos positivos y tres rechazos canónicos en ambas fases.
- configuración sintética `2 × 100 = 200` y distribución de esos puntos sobre
  los stats materializados, con total, gasto, remanente y conjunto de
  asignaciones verificados en ambas fases.
- reproducción de los 56 casos positivos de las fórmulas publicadas mediante
  snapshot → progresión → distribución → contexto → intérprete, incluida la
  igualdad de las trazas contextual y aritmética en ambas fases;
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

Verificación local del 2026-07-28 después de cerrar AG de Magic Gladiator:
PASS en `win-x64`, SQLite `3.53.3`, 611 archivos y 149.076.719 bytes. Conservó
10 avisos legales y 188/188 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 68/68 casos positivos de las
diecisiete referencias publicadas con resolución contextual en ambas fases. El
dataset `2026-07-28.2` produjo el hash
`sha256:5246861cec04e5e618611091d365e7e0a4c03d8227013f84c13e93354253d901`.

Verificación local del 2026-07-28 después de cerrar AG de Summoner:
PASS en `win-x64`, SQLite `3.53.3`, 601 archivos y 149.063.645 bytes. Conservó
10 avisos legales y 178/178 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 64/64 casos positivos de las
dieciséis referencias publicadas con resolución contextual en ambas fases. El
dataset `2026-07-28.1` produjo el hash
`sha256:6380346e97b61e31a6da3329b86f91954f2b120d880e751733e778f4cbb75f43`.

Verificación local del 2026-07-26 después de cerrar AG de Fairy Elf:
PASS en `win-x64`, SQLite `3.53.3`, 591 archivos y 149.051.441 bytes. Conservó
10 avisos legales y 168/168 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 60/60 casos positivos de las
quince referencias publicadas con resolución contextual en ambas fases. El
dataset `2026-07-26.12` produjo el hash
`sha256:432b6a062fbd5ed996a9d58dbfa32daafec5cf09c447dc7c40bb0ac98e645177`.

Verificación local del 2026-07-26 después de cerrar AG de Dark Knight:
PASS en `win-x64`, SQLite `3.53.3`, 580 archivos y 149.038.270 bytes. Conservó
10 avisos legales y 157/157 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 56/56 casos positivos de las
catorce referencias publicadas con resolución contextual en ambas fases. El
dataset `2026-07-26.11` produjo el hash
`sha256:67d6fd0e614d3072f214b7dc09f7295e2450d2b4b7c0cd24a91f70a14dad13ef`.

Verificación local del 2026-07-26 después de cerrar AG de Dark Wizard:
PASS en `win-x64`, SQLite `3.53.3`, 569 archivos y 149.025.417 bytes. Conservó
10 avisos legales y 146/146 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 52/52 casos positivos de las trece
referencias publicadas con resolución contextual en ambas fases. El dataset
`2026-07-26.10` produjo el hash
`sha256:469f18aec26813dcb75dab054df3df7b2469d730d76d532ed2ef8432711e4651`.

Verificación local del 2026-07-26 después de cerrar Mana de Dark Lord:
PASS en `win-x64`, SQLite `3.53.3`, 558 archivos y 149.012.089 bytes. Conservó
10 avisos legales y 135/135 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 48/48 casos positivos de las doce
referencias publicadas con resolución contextual en ambas fases. El dataset
`2026-07-26.9` produjo el hash
`sha256:440b7fb4a1ef1bd5b57202323bf7cb447c3bf252abe81cf112f8832220e7817a`.

Verificación local del 2026-07-26 después de cerrar Mana de Magic Gladiator:
PASS en `win-x64`, SQLite `3.53.3`, 549 archivos y 149.001.252 bytes. Conservó
10 avisos legales y 126/126 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 44/44 casos positivos de las once
referencias publicadas con resolución contextual en ambas fases. El dataset
`2026-07-26.8` produjo el hash
`sha256:cff8d5726f433448ac7212a7a6e7465475024cc4346e904a649bcc2615e706f5`.

Verificación local del 2026-07-26 después de cerrar Mana de Summoner:
PASS en `win-x64`, SQLite `3.53.3`, 540 archivos y 148.989.299 bytes. Conservó
10 avisos legales y 117/117 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 40/40 casos positivos de las diez
referencias publicadas con resolución contextual en ambas fases. El dataset
`2026-07-26.7` produjo el hash
`sha256:7d0e75d9212837a9245a339253b9622ecff2ec4b157cb839606601c3ee73331b`.

Verificación local del 2026-07-26 después de cerrar Mana de Fairy Elf:
PASS en `win-x64`, SQLite `3.53.3`, 531 archivos y 148.977.675 bytes. Conservó
10 avisos legales y 108/108 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 36/36 casos positivos de las nueve
referencias publicadas con resolución contextual en ambas fases. El dataset
`2026-07-26.6` produjo el hash
`sha256:78d8688b3b08a02b13ea9971fa45f90ff59612c6c5b1a2981cb102b82c1adc7e`.

Verificación local del 2026-07-26 después de cerrar Mana de Dark Knight:
PASS en `win-x64`, SQLite `3.53.3`, 522 archivos y 148.967.275 bytes. Conservó
10 avisos legales y 99/99 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 32/32 casos positivos de las ocho
referencias publicadas con resolución contextual en ambas fases. El dataset
`2026-07-26.5` produjo el hash
`sha256:868ef53f5238066e928f51a56e2f375124c8d2f7b2a5fb6d75078c61c557c120`.

Verificación local del 2026-07-26 después de cerrar HP de Dark Lord:
PASS en `win-x64`, SQLite `3.53.3`, 504 archivos y 148.940.709 bytes. Conservó
10 avisos legales y 81/81 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 24/24 casos positivos entre
`formula-hp-dark-knight` `1.0.0`, `formula-hp-dark-wizard` `1.1.0`,
`formula-hp-fairy-elf` `1.0.0`, `formula-hp-summoner` `1.0.0` y
`formula-hp-magic-gladiator` `1.0.0`, más
`formula-hp-dark-lord` `1.0.0` con resolución contextual en ambas fases.
El dataset `2026-07-26.3` produjo el hash
`sha256:d7810c927ec692c161d0adcfcfa8cc6374d2213cac97805e9e77ec9d2ecefb32`.

Verificación local del 2026-07-26 después de cerrar HP de Summoner:
PASS en `win-x64`, SQLite `3.53.3`, 486 archivos y 148.908.207 bytes. Conservó
10 avisos legales y 63/63 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 16/16 casos positivos entre
`formula-hp-dark-knight` `1.0.0`, `formula-hp-dark-wizard` `1.1.0`,
`formula-hp-fairy-elf` `1.0.0` y `formula-hp-summoner` `1.0.0` con resolución
contextual en ambas fases. El dataset `2026-07-26.1` produjo el hash
`sha256:afb77b6daa3112da782dbcc68685f0f7e5bc3cbb1ae8f9bf3f6d1f80d0b61dc8`.

Verificación local del 2026-07-25 después de cerrar HP de Fairy Elf:
PASS en `win-x64`, SQLite `3.53.3`, 477 archivos y 148.896.977 bytes. Conservó
10 avisos legales y 54/54 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 12/12 casos positivos entre
`formula-hp-dark-knight` `1.0.0`, `formula-hp-dark-wizard` `1.1.0` y
`formula-hp-fairy-elf` `1.0.0` con resolución contextual en ambas fases. El
dataset `2026-07-25.2` produjo el hash
`sha256:aa3c761e9c3a8a2739c2cf424175c5d5b2ee703793f1489d2b8ebbb823521afa`.

Verificación local del 2026-07-25 después de cerrar HP de Dark Knight:
PASS en `win-x64`, SQLite `3.53.3`, 468 archivos y 148.884.866 bytes. Conservó
10 avisos legales y 45/45 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó 8/8 casos positivos entre
`formula-hp-dark-knight` `1.0.0` y `formula-hp-dark-wizard` `1.1.0` con
resolución contextual en ambas fases. El dataset `2026-07-25.1` produjo el hash
`sha256:11a3d88ed670f998ba8ff3d5c149aa2f4017ae9ef1dd4a34994f35644a7024b3`.

Verificación local del 2026-07-25 después de cerrar HP de Dark Wizard en WPF:
PASS en `win-x64`, SQLite `3.53.3`, 459 archivos y 148.871.955 bytes. Conservó
10 avisos legales y 36/36 JSON, reprodujo progresión 7/7+3/3, distribución,
resets, backup/restore y borrador, y ejecutó los 4/4 casos positivos de
`formula-hp-dark-wizard` `1.1.0` con resolución contextual en ambas fases.

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

Verificación local del 2026-07-25 después de añadir la definición ejecutable
`1.1.0` en `DRAFT`: PASS en `win-x64`, SQLite `3.53.3`, 451 archivos y
148.758.166 bytes. Las dos publicaciones conservaron 28/28 JSON y el hash
`sha256:369c9e19fcc337a08df86df5e7744a111bfe80f3aaf9b3bcb17ce715f0636279`.
El flujo WPF continuó sin materializar ni ejecutar fórmulas.

Verificación local del 2026-07-25 después de añadir los ocho casos versionados
`1.1.0`: PASS en `win-x64`, SQLite `3.53.3`, 459 archivos y 148.766.267 bytes.
Las dos publicaciones conservaron 36/36 JSON y el hash
`sha256:cfe267e51cf07532d5d1828fd524078bf832a122f367cf6381309e0d9010dbf7`.
Progresión, distribución, resets, backup/restore y borrador persistido
continuaron aprobados; WPF sólo empaqueta los casos y no ejecuta la fórmula.

Verificación local del 2026-07-25 después de publicar la definición ejecutable
`1.1.0`: PASS en `win-x64`, SQLite `3.53.3`, 459 archivos y 148.766.271 bytes.
Las dos publicaciones conservaron 36/36 JSON y el hash
`sha256:712afd0b572e68d9025a07f3a8eabc7ee4ac1f8496552d19b4c4172b68332efe`.
El único cambio del artefacto factual fue `status`; WPF continúa sin
materializar ni ejecutar fórmulas.

Verificación local del 2026-07-24 después de incorporar resets configurables:
PASS en `win-x64`, SQLite `3.53.3`, 441 archivos y 148.744.294 bytes. Ambas fases
conservaron el snapshot, calcularon `2 × 100 = 200`, habilitaron esos puntos para
distribución y revalidaron el mismo desglose desde el borrador persistido.
