# Application

`MuOnline.BuildPlanner.Application` orquesta casos de uso sin depender de WPF
ni SQLite. La primera vertical contiene:

- `JsonProgressionRulesetSnapshotReader`, adaptador que materializa las clases y
  reglas de progresión desde `character-classes/` y `progression-rules/`;
- `ProgressionRulesetCatalog`, catálogo inmutable entregado al motor;
- opciones de selección con nombres de clase/evolución leídos de los mismos
  registros, para que una superficie no duplique esos valores;
- `CalculateProgressionPointBudgetUseCase`, caso de uso que delega el cálculo en
  `ProgressionPointBudgetCalculator`;
- `CalculateStatDistributionUseCase`, caso de uso que recibe el presupuesto ya
  calculado, los inputs configurables de resets y las asignaciones, resuelve su
  única clase en el catálogo y delega en `StatDistributionCalculator` sin
  aceptar una definición alternativa;
- `JsonExecutableFormulaSnapshotReader`, adaptador dedicado que inspecciona
  `character-classes/` y `formulas/`, conserva `1.0.0` como historia no
  ejecutable y materializa definiciones contra los schemas `2.0.0`/`2.1.0`;
- `ExecutableFormulaCatalog`, catálogo inmutable indexado por la referencia
  exacta `id` + `version`, y `CalculatePublishedFormulaUseCase`, que no elige
  versiones implícitas y delega en `CheckedIntegerFormulaInterpreter`;
- stats base inmutables con `baseValue` y `evidenceRefs`, más
  `CalculateCharacterFormulaUseCase`, que compone progresión, distribución,
  resolución de `CONTEXT_VALUE` y ejecución exacta sin aceptar un diccionario
  contextual desde la superficie;
- tipos serializables `BuildDraft` alineados con el contrato JSON `1.1.0`,
  `IBuildDraftRepository` como puerto sin tipos SQLite y casos de uso de
  guardado/carga;
- errores tipados de snapshot para ausencia, formato, IDs duplicados, mezcla de
  rulesets, reglas no publicadas y referencias incoherentes.

El adaptador es un segundo gate semántico, no un reemplazo de JSON Schema. La
publicación del snapshot debe pasar primero el validador integral del repositorio.
Al cargar, Application vuelve a fallar cerrado si los archivos no forman un
único ruleset coherente o contienen una regla distinta de `PUBLISHED`.

El adaptador de fórmulas exige identidades compuestas únicas, aplicabilidad
resoluble contra una clase y sus evoluciones, inputs contextuales o derivados,
aridades y
referencias hacia atrás coherentes, igualdad exacta programa/traza, salida
visible producida por el paso de redondeo, y dependencias exactas, compatibles
y acíclicas. Los fallos del snapshot usan códigos
`formula-snapshot-*`. Solicitar una referencia histórica o ausente produce
`formula-not-executable` antes de invocar el motor. Los casos de referencia no
se leen durante la ejecución normal.

El proyecto sólo referencia Domain y Calculation Engine. La aplicación WPF lo
referencia de forma unidireccional y le entrega el snapshot empaquetado bajo
`rulesets/mu-s4-global-reference/v1`. Application no incorpora valores de MU
Online en código: todos los puntos, niveles, clases, evoluciones, nombres y
quests se leen del snapshot canónico versionado. La distribución conserva los
códigos tipados de Domain y falla con `budget-source-mismatch` si el presupuesto
no resuelve a una única clase del mismo ruleset.

Las pruebas de integración leen las veintiocho definiciones ejecutables. Los
casos de Defense/SD de Dark Wizard, Dark Knight, Fairy Elf, Magic Gladiator y
Dark Lord añaden cobertura
directa de sus ocho positivos y nueve controles por familia; la composición
contextual recorre todos los positivos y conserva la dependencia `RAW`.
También
demuestran que el `1.0.0` histórico de Dark Wizard no es ejecutable. No copian
constantes, inputs ni resultados de HP, Mana, AG, Defense o SD a C#.

La resolución productiva de `CONTEXT_VALUE` está implementada según
`../../docs/04-domain/formula-context-value-resolution-design.md`.
`ResolvedCharacterState` conserva copias inmutables de solicitud, presupuesto y
distribución; `FormulaContextValueResolver` obtiene `character-level` de la
solicitud validada y cada `resolved-{statId}` mediante suma `Int64` comprobada
de base canónica más asignación. Devuelve una traza contextual separada.
`FORMULA_OUTPUT` se resuelve por referencia exacta y etapa `RAW`/`VISIBLE`,
conserva precisión decimal y expone otra traza. El snapshot y la ejecución
rechazan referencias incoherentes y ciclos. Data y borradores no cambiaron.

Las pruebas comparan bases/evidencias y sources con los JSON,
reproducen 112/112 casos positivos por la ruta productiva y cubren mismatch,
fuentes/valores no resolubles, base/asignación ausentes, overflow,
inmutabilidad, fallos previos de progresión/distribución y dependencias
sintéticas `RAW`/`VISIBLE`.

## Borradores de build

`BuildDraftRuntimeContext` recibe de forma explícita el catálogo y las
identidades versionadas de ruleset, dataset/hash y motor. Application no las
deduce de carpetas, fechas ni ensamblados. El guardado calcula progresión y
distribución antes de entregar el borrador completo al repositorio; guardar el
mismo ID tiene semántica de reemplazo para las implementaciones del puerto.

La carga acepta `1.1.0` y normaliza borradores `1.0.0` a resets cero sin
reescribirlos. Exige coherencia entre las identidades persistidas y
disponibilidad exacta del contexto. Después recalcula progresión y distribución
desde progresión, resets y asignaciones autoritativas, compara toda la caché y sólo
entonces devuelve una copia con el resultado recalculado. Los fallos usan los
seis códigos estables `build-draft-*` definidos por la especificación.

Las pruebas usan un repositorio sintético en memoria. Application continúa sin
referenciar Data, SQLite ni WPF. Data implementa el puerto en un adaptador
separado; WPF compone ese adaptador y mantiene la carga a través del caso de uso
para no omitir la revalidación.
