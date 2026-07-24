# Modelo de cálculo

## Pipeline
1. Normalizar entradas.
2. Validar build y escenario.
3. Resolver reglas activas.
4. Calcular progresión y presupuesto.
5. Generar modificadores base/equipo/buffs.
6. Construir grafo de fórmulas.
7. Evaluar en orden topológico.
8. Aplicar redondeos en puntos documentados.
9. Emitir valores, advertencias y traza.

## Tipos de modificador
- `flat_before`: suma previa.
- `percent_additive`: porcentajes agrupados.
- `percent_multiplicative`: multiplicadores secuenciales.
- `flat_after`: suma posterior.
- `override`: reemplazo condicionado.
- `cap/floor`: límite superior o inferior.
- `chance`: efecto probabilístico.

El orden exacto no se supone globalmente: cada atributo o fórmula declara su pipeline.

## Primera vertical implementada: presupuesto por progresión

`ProgressionPointBudgetCalculator` es una operación pura del motor. Recibe
clase, evolución, nivel y quests completadas sobre un catálogo de definiciones
de dominio ya materializado. La resolución exige exactamente una regla
`PUBLISHED` referenciada por la clase, perteneciente al mismo ruleset y
aplicable a esa clase.

El resultado conserva `rulesetId`, ID y versión de la regla, total ganado y una
lista ordenada de aportes. El aporte base identifica la regla y declara niveles
premiados, puntos por nivel y subtotal; cuando corresponde, el aporte de quest
identifica `quest-hero-status` y declara los mismos campos para el bonus. No se
incluyen resets, puntos gastados, stats ni fórmulas derivadas en esta vertical.

Las entradas inválidas producen `ProgressionPointBudgetException` con un código
estable. Los controles factuales existentes fijan
`quest-ineligible-evolution` para Hero Status con evolución base y
`quest-not-supported` para Magic Gladiator/Dark Lord.

## Orquestación en Application

`JsonProgressionRulesetSnapshotReader` materializa las definiciones anteriores
desde los directorios `character-classes` y `progression-rules` de un snapshot
que ya pasó JSON Schema. El adaptador no sustituye ese gate estructural: añade
un cierre semántico productivo para exigir un solo `rulesetId`, IDs únicos,
referencias bidireccionales clase/regla coherentes y estado `PUBLISHED` en todas
las reglas cargadas.

`CalculateProgressionPointBudgetUseCase` recibe el catálogo materializado y
delega la solicitud al cálculo puro. Application no conoce WPF, SQLite ni los
casos de referencia y no contiene constantes factuales del juego.

## Primera superficie productiva

La publicación WPF copia el snapshot canónico a
`rulesets/mu-s4-global-reference/v1` y lo resuelve exclusivamente desde
`AppContext.BaseDirectory`. La shell referencia Application en un único sentido
y presenta clase/evolución con los nombres materializados, nivel y Hero Status.
El resultado visible incluye total, regla/version y cada aporte de la traza.

El checkbox de Hero Status obtiene el ID de quest y su elegibilidad desde la
regla publicada; XAML y C# no duplican puntos, niveles, IDs de clases/evoluciones
ni fórmulas. La UI no introduce resets, puntos gastados, distribución de stats
ni atributos derivados.

## Siguiente contrato: distribución de stats

El contrato estructural `stat-distribution.schema.json` `1.0.0` y sus fixtures
sintéticos ya están definidos. Conserva presupuesto ganado, asignaciones,
gasto, remanente y la referencia a la regla que originó el presupuesto. Las
invariantes, límites, errores estables y casos mínimos para la futura operación
pura viven en `stat-distribution-contract.md`.

No existe todavía implementación productiva de esta operación. En particular,
el contrato no calcula resets ni atributos derivados y no incorpora nuevos
valores factuales del juego.
