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
Los únicos scripts ejecutados hasta ahora son fixtures sintéticos de pruebas;
todavía no existe un schema factual de MU Online ni almacenamiento de usuario.

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

## Separación
Datos canónicos, traducciones, evidencia y assets se almacenan por separado para evitar duplicación.
