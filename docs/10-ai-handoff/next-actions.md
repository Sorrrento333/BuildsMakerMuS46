# Próximas acciones

## Prioridad inmediata

1. Conectar en WPF `SaveBuildDraftUseCase` y `LoadBuildDraftUseCase` con
   `SqliteBuildDraftRepository`.
2. Resolver una ruta estable de base de usuario fuera de la carpeta publicada y
   aplicar `SqliteBuildDraftMigrations.All` antes de construir el repositorio.
3. Configurar explícitamente `SqliteWriteContentionPolicy` desde la composición
   WPF y mostrar `build-draft-write-conflict` sin ocultar su código estable.
4. Añadir controles mínimos para guardar y cargar por ID; la carga debe seguir
   pasando exclusivamente por `LoadBuildDraftUseCase` para recalcular y
   contrastar la caché.
5. Extender el smoke publicado para guardar un borrador sintético, reabrirlo
   desde los binarios reemplazados y demostrar que la base externa conserva el
   payload y supera la revalidación.
6. Mantener resets, builds completas, atributos derivados y nuevos datos del
   juego fuera de esta vertical.

## Última tarea cerrada

Data implementa `IBuildDraftRepository` mediante
`SqliteBuildDraftRepository`. La migración hacia adelante
`1/create_build_drafts` guarda por `id` el payload JSON completo y metadata de
schema, ruleset, dataset/hash y motor en la misma fila.

Cada alta o reemplazo se ejecuta dentro de una transacción inmediata de
`SqliteWriteContentionPolicy`; todo el contenido se confirma o revierte junto.
El agotamiento de contención se traduce a
`build-draft-write-conflict`. La carga ejecuta sólo un `SELECT` y no asume la
responsabilidad de revalidación de Application.

## Verificación del cierre

Seis pruebas sintéticas con archivos SQLite temporales cubren alta/carga y
payload exacto, metadata persistido, reemplazo por ID, rollback ante fallo,
reapertura, ausencia sin mutaciones y conflicto de escritura tipado.

La restauración bloqueada, el build Release sin advertencias y las 70/70 pruebas
de la solución aprueban. No se añadieron datos, fórmulas, evidencias ni
registros de investigación de MU Online. Application continúa sin referencias
a Data, SQLite o WPF.

## Primera acción concreta

Componer en WPF la migración y `SqliteBuildDraftRepository` sobre una base
externa a los binarios; exponer guardado/carga por ID a través de los casos de
uso existentes y demostrar persistencia y revalidación después del reemplazo
simulado del artefacto publicado.
