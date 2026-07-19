# Evaluación tecnológica

## Criterios
- Precisión numérica y pruebas.
- Ecosistema para aplicación web/escritorio.
- Tipado y mantenibilidad.
- Facilidad de empaquetado offline.
- Integración con JSON Schema y SQLite.

## Opción A: TypeScript end-to-end
Frontend React/Vue/Svelte, motor TypeScript, SQLite mediante capa local. Ventaja: un lenguaje y excelente UI. Riesgo: disciplina necesaria para matemática y decimales.

## Opción B: C#/.NET + frontend web
Motor y API en C#, UI Blazor o web. Ventaja: tipado, rendimiento y escritorio. Riesgo: empaquetado y curva de UI según elección.

## Opción C: Rust core + web UI
Máxima robustez y portabilidad. Riesgo: complejidad prematura.

## Resultado de los spikes (2026-07-18)

Se implementaron prototipos equivalentes en
`spikes/technology-selection/`. Ambos cubren un cálculo sintético —sin datos ni
fórmulas de MU Online—, traza JSON, validación, SQLite y ejecución offline.

| Criterio | TypeScript / Node.js 24 | C# / .NET 10 |
|---|---|---|
| Pruebas | 3/3 aprobadas | 4/4 comprobaciones aprobadas |
| Control numérico observado | enteros seguros comprobados manualmente | overflow comprobado con `checked` |
| SQLite del spike | `node:sqlite`, advertencia experimental | `winsqlite3.dll`, estable pero sólo Windows |
| Dependencias del spike | ninguna externa | ninguna externa |
| Offline observado | requiere Node 24 compatible | binario autocontenido ejecutado |
| Tamaño observado | no empaquetado en el spike | ~73,4 MB autocontenido; ~194 KB con framework |

Las cifras son mediciones del fixture técnico en Windows x64, no benchmarks del
producto. Comandos, alcance y limitaciones reproducibles están documentados en
el README del spike.

## Decisión resultante

C#/.NET 10 queda seleccionado para el núcleo y la aplicación inicial mediante
ADR-0002, aceptado por el propietario el 2026-07-18. La
biblioteca SQLite queda decidida por ADR-0003, aceptado por el propietario; la
forma concreta de UI quedó decidida por ADR-0004: WPF `win-x64` autocontenido
para la primera distribución, aceptado por el propietario el 2026-07-18. El
P/Invoke del spike no es código productivo.

## Evaluación SQLite productiva (2026-07-18)

Se compararon `Microsoft.Data.Sqlite`, `Microsoft.EntityFrameworkCore.Sqlite` y
`System.Data.SQLite` para .NET 10. ADR-0003 selecciona `Microsoft.Data.Sqlite
10.0.10` con `SQLitePCLRaw.bundle_e_sqlite3 2.1.12` fijado explícitamente; el
bundle transitivo predeterminado observado resolvió una versión con advisory de
severidad alta. La decisión, licencias, migraciones, política de versiones y
smoke test se documentan en `sqlite-dependency-evaluation.md`. ADR-0003 fue
aceptado por el propietario y su primera vertical de persistencia se implementó
el 2026-07-18.

## Comparación de UI y distribución (2026-07-18)

Se publicaron plantillas vacías .NET 10 de WPF y Blazor WebAssembly PWA. En
Windows x64, WPF autocontenido produjo 139,26 MiB y 400 archivos; la PWA produjo
26,52 MiB y 289 archivos estáticos. Son mediciones de empaquetado mínimo, no del
producto ni de rendimiento.

La PWA ofrece actualización de recursos mediante service worker y alcance de
navegador, pero su persistencia SQLite local, migraciones y backup no se han
demostrado. WPF es sólo Windows y tiene mayor huella, pero alinea el primer RID
con el único smoke test SQLite ya ejecutado y permite reutilizar directamente la
capa Data sobre archivos locales. Criterios, comandos, fuentes y pruebas
pendientes quedan en `adr/ADR-0004-ui-distribution.md`.
