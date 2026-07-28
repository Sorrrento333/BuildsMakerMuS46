# Estrategia de datos

## Fuentes de verdad
- Definiciones editables en archivos estructurados versionados.
- Snapshots compilados para la aplicación.
- SQLite para búsqueda, cache y datos de usuario.

## Flujo
`raw -> staging -> normalized -> reviewed -> published`.

## Identificadores
IDs estables y semánticamente neutros. El nombre visible puede cambiar sin cambiar ID.

## Versionado
- Schema semver.
- Ruleset semver independiente.
- Dataset snapshot con hash.
- Migraciones hacia adelante y estrategia de rollback.

## Persistencia implementada

La primera infraestructura productiva vive en
`packages/data-access/MuOnline.BuildPlanner.Data` conforme a ADR-0003. Usa
`Microsoft.Data.Sqlite 10.0.10` y SQLitePCLRaw `2.1.12` fijados, con lock files.

El runner aplica migraciones SQL inmutables en transacciones individuales y
mantiene `schema_migrations` con versión, nombre, SHA-256 y fecha UTC. Rechaza
una migración aplicada si su nombre o hash deja de coincidir con el catálogo.
La primera migración productiva crea `build_drafts` para datos de usuario; no
contiene un schema factual de MU Online. Los scripts anteriores continúan
siendo fixtures sintéticos de pruebas.

`SqliteBackupService` crea backups online mediante una copia candidata, exige
`PRAGMA integrity_check = ok` antes de reemplazar un backup anterior y verifica
de nuevo tanto el archivo antes de restaurarlo como la base restaurada. La
restauración recupera el schema, el ledger y los datos completos. El servicio
todavía no está conectado automáticamente al runner: esa integración se hará al
introducir la primera migración destructiva real.

`SqliteWriteContentionPolicy` serializa cada operación en una transacción
inmediata, aplica un timeout positivo explícito por intento y reintenta sólo los
errores SQLite de contención. Application configura el límite y la demora,
mantiene las escrituras cortas e idempotentes y decide cómo comunicar el
agotamiento; Data devuelve un error tipado y no presupone escritores paralelos.

## Primera persistencia de usuario

`build-draft.schema.json` `1.1.0` y
`docs/06-data/build-draft-persistence-contract.md` gobiernan la primera tabla de
usuario. El borrador conserva metadata exacto, entradas de progresión,
asignaciones y una copia verificable del resultado. Los valores calculados no
son autoridad: Application los recalcula al cargar.

`IBuildDraftRepository` vive en Application y
`SqliteBuildDraftRepository` lo implementa en Data sin filtrar tipos del
proveedor hacia el puerto. La migración `1/create_build_drafts` almacena por
`id` el payload JSON completo y columnas de schema, ruleset, dataset/hash y
motor en una sola fila. Guardar usa reemplazo atómico dentro de
`SqliteWriteContentionPolicy`; cargar ejecuta una lectura sin mutaciones.

WPF compone esta persistencia en
`%LOCALAPPDATA%\MuOnline.BuildPlanner\build-planner.sqlite`, aplica el catálogo
antes de construir el repositorio y aporta metadata explícito `1.0.0`,
`2026-07-25.2` y motor `0.2.0`. El hash SHA-256 se deriva de las rutas relativas
y bytes exactos de los 54 JSON publicados. El cambio de dataset corresponde a
la fórmula y los ocho casos factuales de HP de Fairy Elf. El smoke usa la
misma composición sobre su directorio de usuario temporal externo al artefacto.
El cierre calculó
`sha256:aa3c761e9c3a8a2739c2cf424175c5d5b2ee703793f1489d2b8ebbb823521afa`
y confirmó la misma identidad después del reemplazo.

## Separación
Datos canónicos, traducciones, evidencia y assets se almacenan por separado para evitar duplicación.
