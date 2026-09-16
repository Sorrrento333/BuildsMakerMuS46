# Diseño técnico — persistencia de build completa local

## Estado y alcance

- Fecha: 2026-09-15.
- Estado: `CLOSED`.
- Implementación productiva: `COMPLETED` entre 2026-09-04 y 2026-09-15.
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-07-30.3` (sin cambios).
- Capas incluidas: Application (modelo, puerto y casos de uso de la build),
  Data (repositorio SQLite y migración 2 `create_builds`), WPF (flujo de
  guardado/carga por ID) y el smoke de publicación (round-trip completo de la
  build y nuevos campos de reporte).
- Capas excluidas: Domain, Calculation Engine, borradores, schemas nuevos y
  nuevas fórmulas.

Este cierre define cómo promover un borrador a una build completa persistida
localmente y cómo recargarla con validaciones propias. No añade información de
MU Online, ni modifica fórmulas, casos, evidencias ni datos.

## Antecedente

`docs/06-data/build-draft-persistence-contract.md` previó esta separación: el
borrador conoce los inputs configurados pero sus asignaciones no equivalen a
valores finales de stats; una build completa exige dataset, versión del motor,
resets y un mapa `stats`. El contrato persistido por esta vertical es el modelo
`CharacterBuild` (`schemaVersion 1.0.0`), cuya promoción debía tener un caso de
uso y validaciones propios.

## Problema que se cierra

Al cerrar los borradores, WPF podía guardar y recargar las entradas de
progresión y las asignaciones, pero cada recarga recalculaba el presupuesto y
la distribución. No existía una representación persistida de la build final —
con clase, evolución, nivel, stats finales, quests y resets de una sola vez—
que permitiera al usuario recuperar el personaje completo por ID, ni un
contrato que verificara la coherencia de ese snapshot al recargarlo.

## Decisión

Application expone `SaveBuildUseCase` y `LoadBuildUseCase` sobre el puerto
`IBuildRepository`, y el modelo seriocontrato `CharacterBuild`:

- `SaveBuildUseCase.ExecuteAsync(SaveBuildRequest(buildId, draftId))` valida el
  ID, carga el borrador con `LoadBuildDraftUseCase` (es decir, recalcula el
  presupuesto desde los inputs exactos), resuelve la clase y evolución,
  garantiza nivel ≥ 1, calcula los stats finales como `base + asignación` y
  persiste `CharacterBuild` con el ruleset, dataset y motor del contexto
  resuelto; un mismo ID reemplaza la fila anterior atómicamente.
- `LoadBuildUseCase.ExecuteAsync(id)` exige el contexto exacto y rechaza:
  ausencia (`BuildErrorCodes.NotFound`), versión de schema no soportada
  (`SchemaUnsupported`), ruleset, dataset o motor no disponibles
  (`DependencyUnavailable`), clase/evolución inexistentes o incoherentes
  (`SourceMismatch`), stats que no coinciden con la clase o no alcanzables
  (`SourceMismatch`/`RevalidationFailed`). Entrega copias defensivas de los
  stats y quest ids para que el modelo persistido nunca escape al llamador.
- Data implementa `SqliteBuildRepository` con la migración 2 `create_builds`
  (tabla `builds` con columnas de metadata derivadas para gates de compatibilidad
  y `payload_json` exacto), reemplazo atómico e insert-on-conflict, bajo el mismo
  umbral de contención de escritura ya autorizado para borradores
  (`WriteConflict`).

El modelo no guarda presupuesto, distribución, puntos ganados ni evaluaciones:
esos valores se recalcularon al promover el borrador y su autoridad al recargar
es la build final verificada, nunca una caché nueva verdad factual. Los borradores
y su contrato se conservan intactos.

## Verificación

- Build de Application, Data, WPF y el smoke publicados con 0 advertencias y 0
  errores.
- Doce pruebas de integración de Application cierran la vertical y fijan:
  promoción de borrador a build y recarga revalidada con snapshot exacto,
  reemplazo por ID del mismo borrador, rechazo de ID inválido, rechazo de borrador
  ausente, rechazo de build ausente, versión de schema no soportada, metadata de
  dependencia indisponible, identidad de personaje incoherente, evolución que la
  clase no ofrece, stats por debajo del valor base canónico, conjunto de stats que
  no corresponde a la clase y nombres de propiedades exactos del modelo
  serializado.
- Seis pruebas de integración de Data fijan: payload y metadata exactos tras
  guardar/cargar, reemplazo atómico (una sola fila), rollback de metadata y
  payload ante fallo intermedio, recarga tras reabrir el repositorio, carga
  ausente sin mutación y contención de escritura con código estable.
- El smoke `win-x64` promueve el borrador sintético (`publication-smoke-draft`)
  a la build `publication-smoke-build`, verifica su recarga revalidada con
  paridad de stats (cinco stats, resets `2 × 100 = 200`) antes de la copia de
  respaldo, tras restaurar el backup y tras el reemplazo simulado de binarios; el
  reporte amplía `BuildPersistenceVerified`, `BuildId` y `BuildStatCount`.
- 779/779 pruebas de la solución pasan; el CLI del validador no registra
  diferencias sobre las ciento siete fórmulas `PUBLISHED`.

## Criterios de cierre

Este diseño queda cerrado porque fija modelo, puerto, casos de uso, validaciones
de recarga, esquema SQLite y pruebas sin decidir datos durante la implementación:
ni una constante, ni una evidencia, ni un caso nuevos.

## Cierre de implementación — 2026-09-15

- `CharacterBuild` (schema `1.0.0`), `IBuildRepository`, `SqliteBuildRepository`
  (migración 2 `create_builds`), `SaveBuildUseCase`, `LoadBuildUseCase`,
  `BuildValidation` y `BuildException` quedaron integrados y probados.
- El smoke `win-x64` verifica el round-trip completo de la build sintética y su
  supervivencia al respaldo/restauración y al reemplazo de binarios.
- Doce pruebas de integración de Application y seis de Data, junto con las
  779/779 pruebas de la solución, pasan.