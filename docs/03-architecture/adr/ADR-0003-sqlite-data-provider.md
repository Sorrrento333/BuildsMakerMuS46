# ADR-0003: Microsoft.Data.Sqlite para la persistencia inicial

- Estado: Aceptado por el propietario.
- Fecha: 2026-07-18.
- Responsables: propietario del proyecto; evaluación asistida por IA.

## Contexto

ADR-0002 seleccionó C#/.NET 10 y dejó abierta la dependencia SQLite productiva.
El producto debe funcionar offline, separar dominio y persistencia, aplicar
migraciones recuperables y controlar vulnerabilidades y bibliotecas nativas.

La evaluación y su smoke test están en
`../sqlite-dependency-evaluation.md`.

## Opciones consideradas

- `Microsoft.Data.Sqlite 10.0.10`, proveedor ADO.NET ligero mantenido dentro del
  proyecto EF Core.
- `Microsoft.EntityFrameworkCore.Sqlite 10.0.10`, ORM y proveedor con
  migraciones integradas.
- `System.Data.SQLite 2.0.3`, proveedor ADO.NET oficial del proyecto SQLite.

## Decisión

Usar `Microsoft.Data.Sqlite 10.0.10` en la futura capa Data, sin exponerlo al
dominio ni al motor. Fijar además `SQLitePCLRaw.bundle_e_sqlite3 2.1.12` como
dependencia directa y centralizada: el grafo predeterminado observado resolvió
la versión vulnerable `2.1.11`, mientras el grafo fijado pasó restauración,
round-trip en `win-x64` y auditoría de vulnerabilidades.

Implementar migraciones SQL hacia adelante con ledger y hashes, backup previo a
cambios destructivos y restauración de backup como rollback. No adoptar EF Core
hasta que la forma real de los repositorios justifique su coste y acoplamiento.

El propietario aprobó esta decisión el 2026-07-18. La primera vertical —paquetes
fijados, lock files, runner y pruebas sintéticas— quedó implementada ese día.

## Consecuencias

- SQLite queda confinado a Data detrás de interfaces.
- El producto distribuye una biblioteca nativa y debe verificar cada RID, sus
  licencias y su ejecución offline.
- Proveedor y bundle se actualizan juntos mediante Central Package Management,
  lock files, auditoría y pruebas de migración.
- Las operaciones ADO.NET serán sincrónicas: SQLite no ofrece I/O asíncrono
  real y Application no debe asumir escrituras paralelas.
- El equipo mantiene un runner de migraciones pequeño y explícito.
- EF Core sigue siendo una alternativa reversible dentro de Data.

## Plan de reversión

Las interfaces de repositorio no referenciarán tipos del proveedor. Si el spike
vertical no satisface distribución, concurrencia o mantenibilidad, retirar el
adaptador antes de publicar datos de usuario y proponer un ADR sustituto con el
mismo conjunto de pruebas.
