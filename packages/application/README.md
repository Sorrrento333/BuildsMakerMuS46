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
quests se leen del snapshot canónico versionado.
