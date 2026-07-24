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
  calculado y las asignaciones, resuelve su única clase en el catálogo y delega
  en `StatDistributionCalculator` sin aceptar una definición alternativa;
- tipos serializables `BuildDraft` alineados con el contrato JSON `1.0.0`,
  `IBuildDraftRepository` como puerto sin tipos SQLite y casos de uso de
  guardado/carga;
- errores tipados de snapshot para ausencia, formato, IDs duplicados, mezcla de
  rulesets, reglas no publicadas y referencias incoherentes.

El adaptador es un segundo gate semántico, no un reemplazo de JSON Schema. La
publicación del snapshot debe pasar primero el validador integral del repositorio.
Al cargar, Application vuelve a fallar cerrado si los archivos no forman un
único ruleset coherente o contienen una regla distinta de `PUBLISHED`.

El proyecto sólo referencia Domain y Calculation Engine. La aplicación WPF lo
referencia de forma unidireccional y le entrega el snapshot empaquetado bajo
`rulesets/mu-s4-global-reference/v1`. Application no incorpora valores de MU
Online en código: todos los puntos, niveles, clases, evoluciones, nombres y
quests se leen del snapshot canónico versionado. La distribución conserva los
códigos tipados de Domain y falla con `budget-source-mismatch` si el presupuesto
no resuelve a una única clase del mismo ruleset.

## Borradores de build

`BuildDraftRuntimeContext` recibe de forma explícita el catálogo y las
identidades versionadas de ruleset, dataset/hash y motor. Application no las
deduce de carpetas, fechas ni ensamblados. El guardado calcula progresión y
distribución antes de entregar el borrador completo al repositorio; guardar el
mismo ID tiene semántica de reemplazo para las implementaciones del puerto.

La carga exige el schema `1.0.0`, coherencia entre las identidades persistidas y
disponibilidad exacta del contexto. Después recalcula progresión y distribución
desde las entradas y asignaciones autoritativas, compara toda la caché y sólo
entonces devuelve una copia con el resultado recalculado. Los fallos usan los
seis códigos estables `build-draft-*` definidos por la especificación.

Las pruebas usan un repositorio sintético en memoria. Application continúa sin
referenciar Data, SQLite ni WPF. Data implementa el puerto en un adaptador
separado; la siguiente vertical conectará esa composición desde WPF.
