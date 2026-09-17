# Diseño técnico — listado de builds guardadas y carga desde la lista

## Estado y alcance

- Fecha: 2026-09-16.
- Estado: `CLOSED`.
- Implementación productiva: `COMPLETED` el 2026-09-16.
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-07-30.3` (sin cambios).
- Capas incluidas: Application (`CharacterBuildSummary`, `ListBuildsUseCase` y
  `IBuildRepository.ListAsync`), Data (`SqliteBuildRepository.ListAsync`), WPF
  (listado local y carga desde la lista) y el smoke de publicación (presencia de
  la build en el listado).
- Capas excluidas: Domain, Calculation Engine, schemas (`build` sigue en
  `1.1.0`) y migraciones SQLite (la tabla `builds` ya existe y el payload es
  JSON).

Este incremento continúa «builds completas y flujos de UI posteriores». Hace
descubrible la build persistida y permite cargarla sin recordar su ID, sin
inventar mecánicas, fórmulas ni datos de MU Online y sin tocar el contrato
persistido.

## Antecedente

La reaplicación de build (`docs/04-domain/load-build-reapplication-design.md`)
cerró la vuelta del snapshot al formulario, pero siempre a partir de un ID
escrito a mano. `IBuildRepository` sólo exponía `SaveAsync`/`LoadAsync`, de modo
que la pantalla no podía enumerar lo ya guardado y el usuario dependía de
recordar el identificador exacto.

## Problema que se cierra

No existía forma de ver las builds guardadas ni de seleccionar una para
cargarla. La consulta de lectura no existía en el puerto ni en el repositorio, y
WPF no ofrecía ningún listado. El bloqueo era exclusivamente de lectura y de UI:
los datos persistidos ya bastaban para reconstruir cada build, pero no se
exponían.

## Decisión

Se añade una proyección de sólo lectura y su superficie de UI, sin cambiar el
contrato persistido:

- Application define `CharacterBuildSummary` (Id, SchemaVersion,
  CharacterClassId, EvolutionId, Level, ResetCount, PointsPerReset,
  DatasetVersion) y amplía `IBuildRepository` con `ListAsync`. `ListBuildsUseCase`
  ordena por `Id` con comparación ordinal como autoridad canónica del listado.
- Data implementa `ListAsync` con un `SELECT payload_json FROM builds ORDER BY
  id;`, deserializa cada `CharacterBuild` y proyecta el summary; no toca
  columnas, migraciones ni payload. Sólo lectura: no hay escritura ni mutación.
- WPF añade un `ListBox` de builds guardadas, un botón «Cargar seleccionada» y
  un estado con el recuento. El listado se refresca al abrir la ventana
  (`WindowLoaded`) y tras cada guardado. Seleccionar una entrada fija el ID y
  reutiliza la misma ruta de carga (`LoadBuildByIdAsync` → `LoadBuildUseCase` →
  `ApplyLoadedBuild`), con la traducción de errores existente.
- El smoke lista las builds persistidas, exige que `publication-smoke-build`
  aparezca con la paridad exacta de schema, clase, evolución, nivel, resets,
  puntos por reset y versión de dataset, y verifica el orden determinista por
  ID. Añade los campos `BuildListVerified` y `PersistedBuildCount`.

Los summaries son una proyección derivada: su autoridad siguen siendo el
`CharacterBuild` revalidado y el snapshot exacto. El listado no persiste estado
adicional ni decide datos.

## Verificación

- Build Release de Application, Data, WPF y el smoke con 0 advertencias y 0
  errores.
- Application: `ListBuildsUseCase` devuelve las builds en orden ordinal de `Id`
  con los campos exactos del snapshot y una colección vacía cuando no hay
  ninguna guardada.
- Data: `ListAsync` enumera todas las builds ordenadas por `Id`, proyecta los
  campos exactos, devuelve vacío en una base nueva y no muta la base
  (`total_changes()` y `COUNT(*)` invariantes).
- El smoke `win-x64` verifica que la build `publication-smoke-build` aparece en
  el listado con la paridad esperada antes del respaldo, tras restaurar y tras
  el reemplazo de binarios.
- 785/785 pruebas de la solución pasan; la comprobación estructural conserva 16
  contratos/32 fixtures.

## Criterios de cierre

Este diseño queda cerrado porque fija contrato, consulta, UI y pruebas sin
decidir datos durante la implementación: ni una constante, ni una evidencia, ni
un caso nuevos, y sin cambiar el schema `build` ni las migraciones SQLite.

## Cierre de implementación — 2026-09-16

- `CharacterBuildSummary`, `IBuildRepository.ListAsync`, `ListBuildsUseCase`,
  `SqliteBuildRepository.ListAsync`, el listado WPF y las comprobaciones del
  smoke quedaron integrados y probados.
- La pantalla enumera las builds guardadas y carga la seleccionada por la misma
  ruta revalidada que el ID manual.
- Dos pruebas nuevas de Application y tres de Data, con el smoke `win-x64` en
  verde (`Saved builds listed: 1`), cierran el incremento.

## Siguiente vertical documentada

Continúan pendientes, sin abrirse aquí: master buys y las pantallas restantes
del flujo. Requieren datos factuales (ítems, skills, inventario) que hoy no
existen; cualquier avance debe esperar a sus contratos factuales y no puede
inventarse.
