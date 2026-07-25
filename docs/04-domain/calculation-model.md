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

## Catálogo factual aprobado pendiente de implementación

EVD-0026 de `RES-0002` registra como axiomas del ruleset Season 4 global/inglés
las expresiones aportadas por el propietario para las seis familias canónicas.
HP, Mana, AG y SD están completos para Dark Knight, Dark Wizard, Fairy Elf,
Magic Gladiator y Dark Lord; para Summoner están definidos HP, Mana y AG entre
esos cuatro recursos. También se preservan fórmulas de daño, wizardry, velocidad,
defensa, rates, regeneración, buffs, Fenrir, Dark Horse, Dark Raven y capacidad
de clan para futuros contratos.

EVD-0027–EVD-0029 contrastan los tres recursos faltantes de Summoner. Confirman
como evidencia `PARTIAL` los valores iniciales HP 70, Mana 40 y SD 102, además
de 1 HP y 1.5 Mana por nivel; sólo la fuente actual de Fanz publica incrementos
por stat y componentes de SD. No se convierten esos componentes en fórmulas:
ninguna fuente demuestra el corte Season 4 global/inglés ni define semántica de
la base, orden y truncamientos. Por tanto HP, Mana y SD de Summoner continúan
fuera del catálogo factual ejecutable.

EVD-0030 fija por decisión del propietario HP 70 en nivel 1/Vitality 18,
+2 HP por cada punto adicional de Vitality y +1 HP por nivel. La fórmula
Season 4 queda `70 + (lvl - 1) + (vit - 18) * 2`, equivalente a
`34 + (lvl - 1) + vit * 2`. El claim factual está `VERIFIED`, pero todavía
requiere ID, contrato, casos manuales y traza antes de entrar al motor.

EVD-0031 conserva la corrección final del propietario: +1.7 Mana por cada punto
de Energy de Summoner y Mana 40 al nacer en nivel 1 con Energy 23, coincidente
con Fanz. La confirmación de +1.5 Mana por nivel cierra la fórmula Season 4:
`40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`. El claim está `VERIFIED`, pero
requiere ID, contrato, casos manuales y traza antes de entrar al motor.

EVD-0032 identifica la fórmula de SD de Summoner como
`(str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30`, con
`defense = agi / 3`. EVD-0032 resuelve `DSP-0004`: cada término se trunca
independientemente antes de la suma. En nivel 1 y stats `21/21/18/23`, la traza
es `99 + 3 + 0 = 102`. El claim queda `VERIFIED`, pendiente todavía de ID,
contrato y casos ejecutables antes de entrar al motor.

El valor visible trunca la parte decimal. No se aprobó ningún redondeo
intermedio adicional: las dependencias como `defense`, `mana` y `AG` deberán
declarar en su contrato si consumen el valor anterior o posterior al truncamiento
visible. El catálogo todavía no vive en schemas, ruleset ni Calculation Engine;
antes requiere IDs estables, grafo de dependencias, casos manuales aprobados y
trazas de redondeo.

El primer diseño previo a esa vertical está en
`dark-wizard-hp-formula-contract.md`. Propone `formula-hp-dark-wizard` `1.0.0`,
la aplicabilidad a las tres evoluciones de la familia, entradas y límites
técnicos, traza de aportes, truncamiento visible final y ocho casos manuales.
La revisión técnica del 2026-07-24 aprobó el diseño con una corrección de
provenance: `EVD-0026` autoriza la fórmula y `EVD-0021` sustenta Vitality 15
como mínimo canónico. El diseño identifica campos que
`formula.schema.json` `1.0.0` todavía no puede representar y no materializa
datos ni código. La estrategia quedó resuelta en
`../06-data/formula-schema-contract-decision.md`: definición `1.1.0`, traza
runtime y casos versionan por separado. Los contratos, la primera definición
factual y sus ocho casos ya están materializados. La revisión de publicación del
2026-07-25 confirmó el alcance aprobado y promovió la fórmula a `PUBLISHED` sin
crear todavía el evaluador ni integrar HP en Application, Data o WPF.

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
definición de su clase, `ResetPointInputs` y un mapa no confiable de asignaciones. No recalcula
progresión: comprueba que ruleset, clase y referencia de regla coincidan, exige
exactamente los stats declarados por la clase y deriva `spentPoints` y
`remainingPoints` con operaciones comprobadas de 64 bits.

La operación es estática y pura. Devuelve la referencia completa al origen del
presupuesto, el desglose de resets, el total distribuible, las asignaciones
normalizadas y errores tipados para negativos, stats ajenos u omitidos, exceso,
overflow y origen incoherente. Los resets son configuración del servidor
aprobada por el propietario, no una regla del ruleset.

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
entrega, junto con `ResetPointInputs` y las asignaciones, a
`CalculateStatDistributionUseCase`. Los controles se generan en orden ordinal
desde `CharacterProgressionDefinition.StatIds`; la UI no contiene una lista
factual de stats ni acepta una definición de clase alternativa.

Un cambio de clase, evolución, nivel o Hero Status invalida el presupuesto
conservado. Cambiar resets sólo invalida la distribución. La pantalla muestra
cantidad, puntos por reset, producto, total distribuible, gasto, remanente y
asignaciones. El smoke publicado configura `2 × 100 = 200`, gasta esos puntos
adicionales y verifica el resultado antes y después del reemplazo. Atributos
derivados permanecen fuera de esta vertical.
