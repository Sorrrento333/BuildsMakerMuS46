# Diseño técnico — resolución de `CONTEXT_VALUE`

## Estado y alcance

- Fecha: 2026-07-25.
- Estado: `CLOSED`.
- Implementación productiva: `COMPLETED` el 2026-07-25.
- Ruleset revisado: `mu-s4-global-reference`.
- Primera fórmula consumidora:
  `formula-hp-dark-wizard` `1.1.0`, `PUBLISHED`.
- Capas incluidas en el diseño: Domain, Application y Calculation Engine como
  consumidor del resultado.
- Capas excluidas de esta tarea: WPF, Data, SQLite, borradores, schemas,
  ruleset y nuevas fórmulas.

Este cierre define cómo unir los `source.kind: CONTEXT_VALUE` publicados con un
estado calculado del personaje. No añade información de MU Online ni modifica
la fórmula, sus casos o sus evidencias.

La planificación posterior aprobada por el propietario amplió el cierre de
implementación hasta WPF y el smoke publicado. Esa ampliación no cambia las
autoridades ni la aritmética decididas aquí; Data, SQLite y el payload de
borradores permanecen fuera.

## Problema que se cierra

`CalculatePublishedFormulaUseCase` recibe hoy un diccionario de inputs ya
construido. Esa API es necesaria para pruebas directas del intérprete, pero no
es una frontera suficiente para WPF: permitir que la superficie entregue
directamente `character-level` o `vitality` convertiría controles editables en
autoridad de cálculo.

Además:

- `JsonProgressionRulesetSnapshotReader` conserva los IDs de `stats`, pero
  descarta `baseValue` y sus `evidenceRefs`;
- `FormulaInputDefinition` conserva el input, bounds y error de rango, pero
  descarta `source.valueId`;
- `ProgressionPointBudgetResult` y `StatDistributionResult` no conservan nivel
  ni evolución, por lo que no bastan de forma aislada para reconstruir el
  contexto;
- la suma de stat base y asignación todavía no tiene responsable, overflow,
  error ni traza definidos.

## Decisión

Application será la única capa que construya inputs de fórmula productivos a
partir de un estado del personaje validado por sus casos de uso. WPF podrá
aportar selecciones e inputs de usuario, pero no un diccionario autoritativo de
valores contextuales.

La vertical se separará en tres responsabilidades:

1. materializar definiciones canónicas de clase y de fórmula sin pérdida;
2. calcular un estado inmutable de personaje mediante progresión y
   distribución;
3. resolver cada `source.valueId` declarado y entregar al intérprete el
   diccionario exacto de inputs.

`CalculatePublishedFormulaUseCase` seguirá siendo el ejecutor de bajo nivel por
referencia exacta. Un nuevo caso de uso de composición en Application será la
entrada productiva para WPF.

## Autoridades

### Identidad y nivel

La solicitud de progresión contiene clase, evolución y nivel. Application debe
validarla mediante `CalculateProgressionPointBudgetUseCase` antes de crear el
estado resuelto.

El valor contextual `character-level` procede del `Level` de esa solicitud
validada. No se recupera de un `TextBox`, de una traza previa ni de un valor
entregado junto con la fórmula.

El estado resuelto conserva juntos:

- clase y evolución validadas;
- nivel y quests de la solicitud de progresión;
- `ProgressionPointBudgetResult`;
- `StatDistributionResult`;
- referencia a la definición canónica de clase del mismo ruleset.

Su construcción pertenece a Application. La API pública de fórmula no permite
reemplazar ninguno de esos componentes por un valor contextual suelto.

### Stats base

`character-classes/*.json` es la autoridad única de cada `baseValue` y sus
`evidenceRefs`. El adaptador de progresión deberá materializar por stat una
definición inmutable con:

- `statId`;
- `baseValue` como entero de 64 bits;
- `evidenceRefs`.

La colección se indexa por el nombre exacto de la propiedad bajo `stats`. No se
crearán listas de clases, stats o bases en C#.

### Asignaciones

`StatDistributionResult.Allocations` es la autoridad de la asignación vigente
después de que el motor haya exigido el conjunto exacto de stats, valores no
negativos y gasto dentro del presupuesto. El resolvedor no acepta un segundo
mapa de asignaciones y no vuelve a interpretar controles WPF.

## Vocabulario de valores contextuales

`source.valueId` debe preservarse en el tipo de input materializado. El
resolvedor construye valores disponibles desde el estado, no desde la identidad
de una fórmula:

- `character-level` representa el nivel validado;
- para cada `statId` canónico se expone `resolved-{statId}`.

La segunda regla es una convención técnica general, no una tabla factual. Para
la primera fórmula, `resolved-vitality` se genera porque la clase contiene el
stat `vitality`; ningún handler deberá mencionar Dark Wizard, Vitality o HP.

Cada input de la fórmula busca exactamente su `source.valueId` en ese conjunto.
El `input.id` y el `source.valueId` siguen siendo conceptos distintos: el
primero es el nombre consumido por el programa; el segundo identifica quién
resuelve el valor.

No se parsearán rutas JSON ni nombres de controles. `FORMULA_OUTPUT` continúa
fuera de esta vertical.

## Resolución numérica

Para cada stat disponible:

```text
resolved-stat = checked(baseValue + allocation)
```

En particular:

```text
resolved-vitality =
    checked(canonical vitality baseValue + current vitality allocation)
```

La suma usa `Int64` comprobado. No aplica redondeos, modifiers, equipo, buffs ni
perfiles privados. Los resets sólo afectan el presupuesto del que sale la
asignación; no se suman otra vez al stat resuelto.

El resolvedor entrega al intérprete exactamente un valor por input declarado.
Los bounds y sus códigos continúan siendo responsabilidad de
`CheckedIntegerFormulaInterpreter`; resolver una procedencia no sustituye la
validación propia de la fórmula.

## Resultado y traza de resolución

La composición devuelve el `FormulaCalculationResult` junto con una traza de
resolución de inputs anterior a la traza aritmética. Por cada input conserva:

- `inputId` y `contextValueId`;
- valor resuelto;
- clase, evolución y ruleset;
- para nivel, el nivel validado;
- para stat, `statId`, `baseValue`, `allocation`, operación `CHECKED_ADD` y
  `evidenceRefs` del valor base.

La traza de la fórmula permanece como autoridad de los pasos aritméticos,
evidencias y conflictos de la definición. La traza contextual explica cómo se
obtuvieron sus inputs; no copia ni altera provenance de la fórmula.

No se requiere cambiar los JSON de traza o de borrador para esta vertical.
Mientras no exista una decisión de persistencia, el resultado derivado y su
traza contextual son runtime y se recalculan.

## Fallos cerrados

Los códigos estables de Application quedan fijados como:

- `formula-context-state-mismatch`: ruleset, clase o evolución del estado no
  coincide con la fórmula solicitada;
- `formula-context-source-not-supported`: `source.kind` no está habilitado en
  esta vertical;
- `formula-context-value-not-resolvable`: `source.valueId` no corresponde al
  nivel ni a un stat de la clase;
- `formula-context-base-stat-missing`: la definición materializada no contiene
  la base requerida;
- `formula-context-allocation-missing`: la distribución validada no contiene la
  asignación requerida;
- `formula-context-arithmetic-overflow`: `baseValue + allocation` no cabe en
  `Int64`.

Se probarán como contrato antes de exponer la nueva API pública. No se
reutilizarán códigos de bounds de la fórmula para un fallo de composición.

Errores anteriores se conservan sin traducción:

- progresión inválida falla antes de crear estado;
- distribución inválida falla antes de resolver contextos;
- bounds, aplicabilidad aritmética y overflow del programa pertenecen al
  intérprete.

## Inmutabilidad e invalidación

El estado resuelto y sus colecciones serán copias inmutables. Una fórmula se
ejecuta sobre una única instantánea; no observa cambios parciales de UI.

La integración WPF posterior deberá invalidar:

- presupuesto, distribución, contexto y fórmula al cambiar clase, evolución,
  nivel o quests;
- distribución, contexto y fórmula al cambiar resets o cualquier asignación;
- sólo el resultado de fórmula al cambiar la referencia de fórmula.

La carga de un borrador deberá completar primero su revalidación existente y
crear un nuevo estado resuelto. Esta tarea no añade HP ni trazas al payload
persistido.

## Ruta de implementación habilitada

La siguiente y única vertical de código habilitada por este gate deberá:

1. preservar `source.valueId` en las definiciones de fórmula de Domain;
2. materializar `baseValue` y `evidenceRefs` desde `character-classes/`;
3. introducir el estado resuelto y el caso de uso de composición en
   Application;
4. generar `character-level` y `resolved-{statId}` desde ese estado;
5. ejecutar `formula-hp-dark-wizard` `1.1.0` por referencia exacta;
6. mantener Data, borradores y WPF sin cambios.

## Estrategia de pruebas de la siguiente vertical

- Pruebas sintéticas de Domain/Application para inmutabilidad, contexto
  desconocido, stat ausente, mismatch y overflow de `baseValue + allocation`.
- Prueba del adaptador que compara stats base y evidencias materializados con el
  propio JSON, sin duplicar valores de MU Online en C#.
- Integración snapshot → progresión → distribución → resolución →
  `CalculatePublishedFormulaUseCase`.
- Casos positivos derivados de los JSON canónicos: la asignación se obtiene
  restando al input esperado el `baseValue` leído del snapshot.
- Comprobación de que nivel cero y asignación negativa fallan en progresión o
  distribución antes del resolvedor.
- Comprobación de que la ruta productiva no acepta un diccionario de valores
  contextuales desde WPF.
- `CalculatePublishedFormulaUseCase` y sus casos directos actuales permanecen
  para probar el intérprete y los controles negativos de la fórmula de forma
  aislada.

## Criterios de cierre

Este diseño queda cerrado porque fija autoridad, composición, aritmética,
provenance, errores, invalidación y pruebas sin decidir datos durante la
implementación. No autoriza todavía cambios en WPF ni la incorporación de otra
fórmula.

## Cierre de implementación — 2026-07-25

- Domain preserva `FormulaInputSource.kind/valueId`; el adaptador de progresión
  materializa `baseValue` y `evidenceRefs` de cada stat sin tablas en C#.
- `CalculateCharacterFormulaUseCase` recalcula progresión y distribución desde
  entradas autoritativas, construye `ResolvedCharacterState`, resuelve
  `character-level` y `resolved-{statId}` y ejecuta una referencia exacta.
- La traza contextual conserva nivel o `baseValue + allocation` con evidencia;
  la traza aritmética del intérprete permanece separada.
- Los seis códigos `formula-context-*` están implementados. Los controles
  prueban mismatch, fuente no soportada, valor no resoluble, base/asignación
  ausentes y overflow; nivel y asignación inválidos fallan antes en sus casos de
  uso de origen.
- WPF no acepta diccionarios contextuales: después de una distribución obtiene
  las fórmulas publicadas aplicables del catálogo, selecciona una referencia
  exacta, ejecuta la composición y muestra resultado y ambas trazas. Los cambios
  de progresión, resets o asignaciones invalidan el resultado.
- El smoke `win-x64` reproduce desde JSON los cuatro casos positivos
  `formula-hp-dark-wizard` `1.1.0` en las fases inicial y de reemplazo.

La frase anterior que no autorizaba WPF describe el límite del gate de diseño.
La ampliación explícita de `next-actions.md`, aprobada por el propietario para
verticales completas, habilitó y ya cerró esa integración.
