# Acceso a datos SQLite

Implementación inicial de la capa Data aprobada por ADR-0003. Este paquete no
contiene datos ni fórmulas de MU Online.

## Dependencias fijadas

- `Microsoft.Data.Sqlite 10.0.10`.
- `SQLitePCLRaw.bundle_e_sqlite3 2.1.12`.

Las versiones se administran en `Directory.Packages.props`; cada proyecto
conserva `packages.lock.json` y CI restaura con `--locked-mode`. El bundle se
referencia directamente para impedir que el grafo resuelva la versión
vulnerable `2.1.11` observada durante ADR-0003.

## Contrato de migraciones

`SqliteMigration` define versión positiva, nombre, SQL y un SHA-256 calculado
sobre los tres campos. `SqliteMigrationRunner` requiere una conexión abierta y
un catálogo completo. El runner:

1. crea `schema_migrations` si no existe;
2. compara cada entrada aplicada con nombre y hash del catálogo;
3. rechaza versiones desconocidas, duplicadas o alteradas;
4. aplica cada migración pendiente en su propia transacción;
5. registra versión, nombre, SHA-256 y fecha UTC en la misma transacción.

Una migración fallida revierte tanto su SQL como su entrada de ledger. Las
migraciones anteriores ya confirmadas permanecen aplicadas. Los scripts son
artefactos de confianza, no deben incluir `BEGIN`, `COMMIT` ni `ROLLBACK`, y no
deben modificarse después de publicados.

## Contrato de backup y restauración

`SqliteBackupService` usa la API de backup online del proveedor sobre conexiones
abiertas. Al crear un backup:

1. escribe una copia candidata en el mismo directorio del destino;
2. ejecuta `PRAGMA integrity_check` sobre esa copia;
3. reemplaza el destino únicamente cuando SQLite devuelve exactamente `ok`;
4. elimina la candidata si la copia o su verificación falla.

La restauración abre el backup en modo de sólo lectura, verifica primero su
integridad, lo copia sobre la conexión de trabajo y vuelve a verificar el
resultado restaurado. Un fallo de integridad produce
`SqliteBackupIntegrityException`. Orquestación, retención y conexión automática
a migraciones destructivas quedan fuera de esta vertical.

## Contrato de contención de escritura

`SqliteWriteContentionPolicy` ejecuta una operación dentro de una transacción
inmediata propia. `SqliteWriteContentionOptions` obliga a Application a declarar
un timeout positivo por intento, un número máximo finito de reintentos y una
espera entre ellos. El valor cero se rechaza porque `Microsoft.Data.Sqlite` lo
interpreta como ausencia de timeout.

La política adquiere el bloqueo de escritura antes de invocar la operación,
reintenta únicamente `SQLITE_BUSY`/`SQLITE_LOCKED`, revierte cualquier intento
fallido y restaura el timeout anterior de la conexión. Si se agotan los intentos
produce `SqliteWriteContentionException`; si completa, informa cuántos intentos
necesitó. El callback opcional de reintento es para observabilidad síncrona.

Application debe seleccionar los tiempos según el flujo de usuario, mantener
las operaciones cortas e idempotentes y presentar el error final. No debe abrir
una transacción antes de llamar a la política ni asumir escrituras paralelas.

## Uso mínimo

```csharp
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Data;

using var connection = new SqliteConnection("Data Source=planner.sqlite");
connection.Open();

var migrations = new[]
{
    new SqliteMigration(1, "create_example", "CREATE TABLE example (id INTEGER PRIMARY KEY);")
};

var result = new SqliteMigrationRunner().Apply(connection, migrations);

SqliteBackupService.CreateVerifiedBackup(connection, "planner.backup.sqlite");
SqliteBackupService.RestoreVerifiedBackup("planner.backup.sqlite", connection);

var writePolicy = new SqliteWriteContentionPolicy(
    new SqliteWriteContentionOptions(
        commandTimeoutSeconds: 1,
        maximumRetryCount: 1,
        retryDelay: TimeSpan.FromMilliseconds(100)));

var writeResult = writePolicy.Execute(
    connection,
    (writeConnection, transaction) =>
    {
        using var command = writeConnection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO example (id) VALUES (1);";
        command.ExecuteNonQuery();
    });
```

El ejemplo es técnico y sintético. No representa un schema aprobado del juego.

## Verificación

```powershell
dotnet restore MUOnline.BuildPlanner.slnx --locked-mode
dotnet build MUOnline.BuildPlanner.slnx --configuration Release --no-restore
dotnet test MUOnline.BuildPlanner.slnx --configuration Release --no-build
dotnet list MUOnline.BuildPlanner.slnx package --vulnerable --include-transitive --no-restore
```

Las pruebas de integración usan archivos SQLite temporales con pooling
desactivado. Cubren base nueva, reapertura, reejecución, alteración de hash,
rollback por fallo, backup consistente, restauración de schema/ledger/datos y
protección del último backup válido ante una candidata corrupta. También cubren
timeout acotado ante un segundo escritor, error tipado, reintento después de
liberar el bloqueo y commit único de la operación.
