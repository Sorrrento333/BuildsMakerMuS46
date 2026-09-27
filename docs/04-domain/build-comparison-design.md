# Diseño técnico — comparador de builds guardadas (UC-06 acotado)

## Estado y alcance

- Fecha: 2026-09-27.
- Estado: `IMPLEMENTED` (Application + WPF + smoke).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-20.1` (sin cambios: no se añade ningún JSON factual).
- Capas incluidas: Application (`CompareBuildsUseCase`, códigos
  `compare-*`), WPF (sección «Comparador de builds») y smoke de publicación.
- Capas excluidas: Domain, Calculation Engine, Data, schemas y migraciones
  SQLite (no hay contrato persistido nuevo).

Este incremento continúa «builds completas y flujos de UI posteriores» y cubre
el primer tramo verificable de UC-06 (Comparar builds) y de la pantalla
«Comparador» de la arquitectura de información, sin inventar datos del juego.

## Antecedente

El listado de builds (`build-list-screen-design.md`) hizo descubribles las
builds persistidas y la evaluación en lote (`CharacterBuildCalculation.cs`)
evalúa todas las fórmulas aplicables de un estado validado. Ambas piezas están
probadas y publicadas. Comparar dos builds es componerlas: revalidar cada una
por su ruta existente y restar sus resultados.

## Problema que se cierra

No existe forma de contrastar dos builds guardadas: ni en diferencias de stats
finales ni en atributos derivados. El usuario debe cargar cada build por
separado y restar a mano.

## Decisión

`CompareBuildsUseCase` (Application, `Builds/`, síncrono como
`CalculateCharacterBuildUseCase`) recibe dos `CharacterBuild` ya cargados y
devuelve `BuildComparison`:

- La revalidación sigue siendo responsabilidad de `LoadBuildUseCase`: el
  llamador (WPF, smoke, tests) carga cada build por su ruta existente antes de
  comparar. El comparador no duplica validación de identidad, schema,
  dependencias ni equipo.
- Por cada build se derivan las asignaciones como `stat final − base`
  (aritmética comprobada, mismo patrón del smoke y de `ApplyLoadedBuild`) y se
  evalúa con `CalculateCharacterBuildUseCase` sobre los catálogos publicados.
  Los errores de fórmula se propagan con sus códigos existentes, sin envolver.
- `StatDifference(StatId, FirstValue, SecondValue, AbsoluteDifference)` sobre
  la unión ordinal de claves de ambos `Stats`. Una clave ausente en un lado se
  conserva `null` con diferencia `null` («sólo en A/B»): no se inventan ceros.
- `DerivedDifference(FormulaId, FormulaVersion, OutputId, OutputUnit,
  FirstVisible, SecondVisible, AbsoluteDifference, PercentDifference)` sólo
  para referencias exactas presentes en ambas evaluaciones. La clave es la
  referencia de fórmula y no el `OutputId`, porque un mismo `OutputId`
  (p. ej. `skill-damage`, `min-damage`) lo producen varias fórmulas de una
  misma familia dentro de una sola build.
- Las referencias presentes en un solo lado se listan aparte
  (`OnlyInFirst`/`OnlyInSecond` con output, unidad y valor visible): entre
  clases distintas los conjuntos derivados son disjuntos por construcción del
  ruleset (cada fórmula aplica a una familia), y el informe lo muestra en vez
  de ocultarlo.
- `PercentDifference` (`decimal?`) = `(segundo − primero) / |primero| × 100`
  exacto, `null` cuando la base es cero («n/d»). Es presentación aritmética
  sobre resultados ya calculados, no una regla del juego; el redondeo a un
  decimal vive sólo en el formato WPF.
- Códigos fail-closed propios (`BuildComparisonException`,
  `BuildComparisonErrorCodes`): `compare-same-build` (mismo ID ordinal),
  `compare-unknown-class` (la clase no resuelve en el catálogo) y
  `compare-stats-mismatch` (claves ajenas a la clase o final bajo la base).

## Exclusiones justificadas (UC-06 completo)

- **Escenario**: UC-06 pide comparar «dos builds y un escenario». No existe
  ningún escenario factual publicado (el contrato `scenario` sólo tiene
  fixtures sintéticos); comparar bajo escenario se difiere hasta su gate.
- **Breakpoints y advertencias**: decidir umbrales o avisos sería inventar
  reglas del dominio sin evidencia. El informe muestra diferencias absolutas y
  porcentuales; no diagnostica.
- Sin cambios en `build.schema.json` (`1.3.0`), Data ni dataset: la
  comparación es cálculo puro sobre documentos ya persistidos.

## Contrato de Application

- `CompareBuildsUseCase(ProgressionRulesetCatalog, ExecutableFormulaCatalog)`,
  con `CalculateCharacterBuildUseCase` interno. Sin referencias a Data, WPF o
  SQLite.
- `PublishedProgressionRuleset.CreateCompareBuildsUseCase()` expone la
  composición en WPF/smoke.

## WPF

- Nueva sección «Comparador de builds» tras «Build completa local»: dos
  `ComboBox` con los summaries guardados (misma fuente que
  `SavedBuildsListBox`, refrescados juntos), botón «Comparar» y
  `CompareResultTextBox` de sólo lectura.
- El handler carga ambas builds con `_loadBuildUseCase` (revalidación
  existente), ejecuta el comparador y formatea stats, derivados compartidos y
  «sólo en» por lado. Los tres códigos `compare-*` tienen explicación visible
  en español; los errores de carga/fórmula reutilizan sus traducciones.

## Smoke WPF

- Compara `publication-smoke-build` con `publication-smoke-equip-build`
  (clases distintas: ejercita claves de stats ausentes en un lado y derivados
  disjuntos), exige la inversión de signo al intercambiar el orden y exige el
  rechazo `compare-same-build` al comparar una build consigo misma. Fallo
  cerrado vía excepción, sin campos nuevos de reporte ni cambios en
  `Test-WpfPublishedArtifact.ps1`.

## Verificación esperada

- Seis pruebas de integración con el snapshot canónico (dos builds DK mismo
  nivel/distinto reparto): diferencias de stats, derivados compartidos con
  porcentaje, porcentaje `null` con base cero, filas «sólo en»,
  conmutatividad de signo, `compare-same-build` y `compare-stats-mismatch`.
- Suite completa en verde; estructura 17/34; smoke WPF `win-x64` PASS.

## Fuera de alcance

- Comparación bajo escenario, breakpoints, advertencias y diagnósticos.
- Borrado/renombrado de builds, exportación y cualquier pantalla restante
  (Enciclopedia, Perfiles, Investigación).
- Cualquier dato, fórmula o regla nuevos de MU Online.
