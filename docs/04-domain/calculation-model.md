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
que ya pasó JSON Schema. La definición de clase incluye los IDs de las claves
de `stats`, sin copiar valores base al motor. El adaptador no sustituye ese gate
estructural: añade un cierre semántico productivo para exigir un solo
`rulesetId`, IDs únicos, referencias bidireccionales clase/regla coherentes y
estado `PUBLISHED` en todas las reglas cargadas.

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
ni fórmulas.

## Segunda vertical del motor: distribución de stats

`StatDistributionCalculator` consume un `ProgressionPointBudgetResult`, la
definición de su clase y un mapa no confiable de asignaciones. No recalcula
progresión: comprueba que ruleset, clase y referencia de regla coincidan, exige
exactamente los stats declarados por la clase y deriva `spentPoints` y
`remainingPoints` con operaciones comprobadas de 64 bits.

La operación es estática y pura. Devuelve la referencia completa al origen del
presupuesto, las asignaciones normalizadas y errores tipados para negativos,
stats ajenos u omitidos, exceso, overflow y origen incoherente. Diez pruebas
sintéticas cubren los seis casos mínimos, las tres variantes de origen y el
overflow; no duplican ninguna lista factual de clases o stats.

Application materializa los IDs requeridos y
`CalculateStatDistributionUseCase` resuelve la definición por la clase y el
ruleset del presupuesto. Su API no acepta una clase alternativa ni recalcula
progresión: construye la solicitud de Domain y delega en el motor. Una
resolución ausente o ambigua produce `budget-source-mismatch`, y los demás
códigos tipados se propagan sin traducción.

Cuatro pruebas de integración leen una copia temporal del snapshot, atraviesan
catálogo → caso de uso → motor para distribuciones sintéticas parcial y exacta,
demuestran fallo cerrado ante un ruleset de origen incoherente y confirman la
propagación de `allocation-negative`.

## Superficie WPF de distribución

`MainWindow` conserva el último `ProgressionPointBudgetResult` calculado y lo
entrega, junto con las asignaciones, a
`CalculateStatDistributionUseCase`. Los controles se generan en orden ordinal
desde `CharacterProgressionDefinition.StatIds`; la UI no contiene una lista
factual de stats ni acepta una definición de clase alternativa.

Un cambio de clase, evolución, nivel o Hero Status invalida el presupuesto
conservado. La pantalla muestra `SpentPoints`, `RemainingPoints` y las
asignaciones resultantes. Los seis códigos tipados se traducen a una explicación
visible sin ocultar su identificador estable. El smoke publicado ejecuta además
una asignación sintética de un punto sobre los IDs materializados y verifica el
resultado antes y después del reemplazo del artefacto. Persistencia, resets y
atributos derivados permanecen fuera de esta vertical.
