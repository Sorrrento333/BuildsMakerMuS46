# Evaluación de dependencia SQLite para .NET 10

- Fecha de corte: 2026-07-18.
- Alcance: persistencia productiva local y offline; no incluye datos ni fórmulas
  de MU Online.
- Resultado: recomendación aprobada por el propietario mediante ADR-0003 e
  implementada en la primera vertical productiva de Data.

## Criterios

- Compatibilidad demostrable con `net10.0` y el soporte de .NET 10.
- Licencia redistribuible y procedencia identificable.
- SQLite nativo empaquetable para ejecución offline.
- Control explícito de versiones y vulnerabilidades transitivas.
- Migraciones reproducibles, recuperables y verificables.
- Acoplamiento limitado a la capa Data del monolito modular.

## Opciones evaluadas

| Opción | Compatibilidad y licencia | Distribución nativa | Migraciones | Evaluación |
|---|---|---|---|---|
| `Microsoft.Data.Sqlite 10.0.10` | Proveedor ADO.NET mantenido en EF Core; paquete `netstandard2.0`, compatible con `net10.0`; MIT | El paquete principal usa `SQLitePCLRaw.bundle_e_sqlite3`, que aporta una versión consistente de SQLite y extensiones comunes | No incluye un framework de migraciones; permite un runner SQL propio aislado en Data | **Recomendado**, con el bundle nativo fijado explícitamente a `2.1.12` |
| `Microsoft.EntityFrameworkCore.Sqlite 10.0.10` | Proveedor oficial EF Core para `net10.0`; MIT | Usa la misma familia `Microsoft.Data.Sqlite`/SQLitePCLRaw | Incluye migraciones, pero SQLite exige reconstrucciones para varias operaciones y no admite scripts idempotentes generales | Reservar para reevaluación si el modelo demuestra que el ORM compensa su mayor superficie y acoplamiento |
| `System.Data.SQLite 2.0.3` | Proveedor ADO.NET `netstandard2.0`; public domain salvo componentes EF6 indicados por el proyecto | La rama 2.x separa el proveedor administrado y la biblioteca compartida `e_sqlite3`; el despliegue nativo queda más a cargo del producto | No aporta migraciones EF Core; requeriría igualmente un runner propio | Viable, pero sin ventaja suficiente frente a la integración y soporte de `Microsoft.Data.Sqlite` |

La compatibilidad calculada por NuGet no equivale a soporte de todos los RIDs.
Sólo `win-x64` fue ejecutado en esta evaluación. Los RIDs finales deberán fijarse
cuando se decida la UI/distribución y verificarse en CI mediante publicación y
ejecución reales.

## Hallazgo de seguridad y bloqueo de versiones

La restauración de `Microsoft.Data.Sqlite 10.0.10` sin una restricción adicional
resolvió `SQLitePCLRaw.lib.e_sqlite3 2.1.11` y falló con `NU1903`, tratado como
error por el repositorio. El advisory `GHSA-2m69-gcr7-jv3q` clasifica las
versiones hasta `2.1.11` como afectadas por una vulnerabilidad alta heredada de
SQLite anterior a `3.50.2`.

Al fijar `SQLitePCLRaw.bundle_e_sqlite3 2.1.12`, NuGet resolvió todo el conjunto
SQLitePCLRaw a `2.1.12`, la auditoría no reportó paquetes vulnerables y el
runtime cargó SQLite `3.53.3`. La adopción aprobada exige ambas referencias
explícitas en el proyecto Data y ambas versiones en `Directory.Packages.props`;
no se admite la selección transitiva predeterminada observada.

El grafo recomendado combina licencia MIT para `Microsoft.Data.Sqlite`,
Apache-2.0 para SQLitePCLRaw y el motor SQLite dedicado al dominio público. La
distribución futura deberá generar el aviso de terceros a partir del grafo
efectivamente bloqueado; esta evaluación no resuelve la licencia del producto.

Política aprobada:

1. Fijar versiones estables exactas mediante Central Package Management y
   conservar lock files.
2. Mantener `Microsoft.Data.Sqlite` en la línea estable `10.0.x` mientras el
   producto use .NET 10; actualizar parches sólo con restauración bloqueada y
   regresión completa.
3. Fijar también SQLitePCLRaw y auditar el grafo transitivo. No usar rangos
   flotantes ni versiones preview.
4. Ejecutar `dotnet list package --vulnerable --include-transitive` en cada
   revisión de dependencias y bloquear severidades alta o crítica.
5. Tratar cambios mayores del proveedor, SQLitePCLRaw o SQLite como una
   actualización deliberada con pruebas de migración, backup y snapshots.

## Estrategia de migraciones aprobada

Usar migraciones SQL hacia adelante, propiedad de Data, sin EF Core inicialmente:

- scripts inmutables con número secuencial, nombre y hash;
- tabla `schema_migrations` con versión, hash y fecha de aplicación;
- aplicación una sola vez, en orden y dentro de transacción cuando SQLite lo
  permita;
- rechazo si cambia el hash de una migración ya aplicada;
- backup verificable antes de una migración destructiva;
- rollback operativo mediante restauración del backup, no scripts `down` que
  puedan perder datos;
- snapshots nuevos creados al último schema y bases existentes migradas paso a
  paso.

EF Core puede reevaluarse sin cambiar las interfaces del dominio si la
complejidad real de consultas o mapeo lo justifica.

## Pruebas mínimas antes de aceptar persistencia

1. Crear una base nueva, aplicar todas las migraciones y reabrirla offline.
2. Insertar, confirmar, leer y conservar datos tras cerrar y reabrir el proceso.
3. Migrar desde cada versión soportada sin perder el fixture de usuario.
4. Reejecutar sin duplicar migraciones y rechazar un hash alterado.
5. Forzar un fallo intermedio y comprobar rollback transaccional.
6. Probar timeout/reintento ante un segundo escritor.
7. Crear y restaurar un backup antes de una migración destructiva.
8. Publicar y ejecutar un smoke test offline por cada RID soportado.
9. Auditar dependencias directas y transitivas en CI.

## Verificación reproducida

Se creó un proyecto temporal ignorado por Git bajo `artifacts/`, sin agregarlo a
la solución. En Windows `win-x64`, con SDK `10.0.301`:

- el grafo predeterminado falló con `NU1903` sobre
  `SQLitePCLRaw.lib.e_sqlite3 2.1.11`;
- el grafo fijado a `Microsoft.Data.Sqlite 10.0.10` y
  `SQLitePCLRaw.bundle_e_sqlite3 2.1.12` restauró sin advertencias;
- un round-trip con tabla, transacción, parámetro, commit, lectura y limpieza
  terminó correctamente con `--no-restore` y sin acceso de red;
- `SqliteConnection.ServerVersion` informó `3.53.3`;
- la auditoría de NuGet no encontró paquetes vulnerables en el grafo fijado.

El artefacto temporal no forma parte del repositorio ni constituye todavía una
prueba productiva multiplataforma.

## Estado de implementación

La primera vertical productiva quedó implementada el 2026-07-18 en
`packages/data-access/MuOnline.BuildPlanner.Data`, con ambos paquetes fijados y
lock files. Doce pruebas de integración en archivos SQLite temporales cubren:

- creación de base y ledger;
- persistencia tras cerrar y reabrir;
- reejecución sin duplicados;
- rechazo de una migración aplicada cuyo hash cambia;
- rollback del schema y ledger ante un fallo SQL.
- backup consistente con `PRAGMA integrity_check`;
- restauración de schema, ledger y datos sintéticos;
- conservación del último backup válido si falla la verificación de la nueva
  copia candidata.
- rechazo de timeout cero, que el proveedor interpreta como espera ilimitada;
- timeout acotado y error tipado mientras otro escritor mantiene el bloqueo;
- reintento de la transacción completa y commit único tras liberar el bloqueo.
- propagación sin reintento de otros errores SQLite y rollback del intento.

El servicio de backup/restore está probado pero permanece deliberadamente
desconectado de las migraciones destructivas hasta que exista una de ellas. Se
implementó también una política síncrona de contención que usa transacciones
inmediatas, timeout positivo por intento y reintentos finitos sólo para
`SQLITE_BUSY`/`SQLITE_LOCKED`. Application debe elegir tiempos explícitos y no
asumir escrituras paralelas. Se mantienen pendientes los smoke tests de los RIDs
finales, bloqueados hasta decidir la forma de UI/distribución.

## Fuentes primarias

- [Microsoft.Data.Sqlite 10.0.10 en NuGet](https://www.nuget.org/packages/Microsoft.Data.Sqlite/10.0.10)
- [Versiones SQLite personalizadas y bundles](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions)
- [SQLitePCLRaw.bundle_e_sqlite3 2.1.12 en NuGet](https://www.nuget.org/packages/SQLitePCLRaw.bundle_e_sqlite3/2.1.12)
- [Microsoft.EntityFrameworkCore.Sqlite 10.0.10 en NuGet](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/10.0.10)
- [Limitaciones SQLite de EF Core](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations)
- [System.Data.SQLite 2.0.3 en NuGet](https://www.nuget.org/packages/System.Data.SQLite/2.0.3)
- [Historial 2.x de System.Data.SQLite](https://system.data.sqlite.org/home/doc/trunk/www/news.md)
- [Advisory GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)
- [Licencia de SQLite](https://www.sqlite.org/copyright.html)
- [Soporte de .NET 10](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
- [Limitaciones asíncronas de Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async)
