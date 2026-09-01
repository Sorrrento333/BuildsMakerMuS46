# Diseño técnico — vertical ejecutable de HP de Dark Wizard

## Estado y alcance

- Fecha: 2026-07-25.
- Estado del diseño: `CLOSED`.
- Implementación: Domain, Calculation Engine y Application completados.
- Fórmula de origen: `formula-hp-dark-wizard` `1.0.0`, `PUBLISHED`.
- Ruleset: `mu-s4-global-reference`.
- Capas incluidas en el diseño: Domain, Calculation Engine y Application.
- Capas excluidas: Data, WPF, perfiles privados y cualquier otra fórmula.
- Resultado de suficiencia: `formula.schema.json` `1.1.0` no permite una
  ejecución cerrada sin interpretar texto libre.

Esta tarea sólo fija el límite técnico y el siguiente cambio de contrato. No
modifica schemas, datos canónicos ni código productivo, y no añade información
de MU Online.

## Entradas revisadas

El diseño parte exclusivamente de:

- `dark-wizard-hp-formula-contract.md`;
- `formula-schema-contract-decision.md`;
- `formula.schema.json` `1.1.0`;
- `calculation-trace.schema.json` `1.0.0`;
- `formula-test-case.schema.json` `1.0.0`;
- la definición y los ocho casos canónicos de
  `formula-hp-dark-wizard` `1.0.0`.

Los valores, la aplicabilidad, el redondeo, la provenance y los resultados
esperados permanecen bajo `EVD-0021`, `EVD-0026` y `DSP-0003`. Este diseño no
los reinterpreta.

## Brecha que bloquea el evaluador

`strategy.definition` contiene
`hp = 30 + (lvl - 1) + vit * 2` como una cadena. El contrato actual no declara:

1. qué tokens son operadores y cuáles son literales;
2. que `lvl` corresponde al input `character-level`;
3. que `vit` corresponde al input `vitality`;
4. qué operación produce cada `stepId`;
5. qué referencias de pasos sólo pueden apuntar hacia atrás;
6. qué código estable corresponde a una violación de bounds de cada input.

`trace.stepIds` fija orden y vocabulario, pero no la aritmética. Los
`numericBounds` fijan límites, pero no el error observable. Por ello un
evaluador basado sólo en `1.1.0` tendría que:

- incorporar aliases, constantes u operaciones específicos en C#; o
- analizar una gramática que el contrato nunca definió.

Ambas opciones convertirían el código o una interpretación no versionada en una
segunda autoridad factual. El gate semántico actual valida que la transcripción
coincida con el diseño aprobado, pero no transforma el texto en un programa
ejecutable.

## Alternativas

### Handler específico por ID y versión

Un handler para `formula-hp-dark-wizard` podría devolver el resultado correcto,
pero duplicaría en C# `30`, `1`, `2`, los pasos o la aplicabilidad. También
permitiría que el JSON publicado y el ejecutable divergieran sin un fallo
estructural. Se rechaza.

### Parser de `strategy.definition`

Un parser general evitaría constantes por fórmula en C#, pero `1.1.0` no define
gramática, precedencia, aliases, tipos intermedios ni semántica de overflow. La
cadena es documentación factual revisada, no un lenguaje ejecutable. Se
rechaza para datos publicados existentes.

### Programa estructurado y versionado

Una secuencia cerrada de operaciones permite materializar tipos de Domain,
validar referencias antes de calcular y ejecutar sin conocimiento específico de
la fórmula. Es la alternativa seleccionada.

## Decisión de versionado e inmutabilidad

`formula-hp-dark-wizard` `1.0.0` permanece inmutable y `PUBLISHED`. No se le
añadirá un programa después de su publicación.

El siguiente cambio deberá:

1. crear `packages/schemas/v2/formula.schema.json` `2.0.0` con una estrategia
   estructurada, conservando el contrato `v1` para validar historia;
2. materializar una nueva versión `formula-hp-dark-wizard` `1.1.0` que preserve
   significado factual, evidencia, conflicto y aplicabilidad, pero sustituya la
   definición textual autoritativa por el programa;
3. crear casos `1.1.0` que conserven las expectativas aprobadas y referencien la
   nueva versión exacta;
4. mantener disponibles los archivos `1.0.0` como historia inmutable, sin
   seleccionarlos para ejecución.

El salto mayor del schema es intencional: la estrategia deja de ser texto libre
y pasa a ser una representación ejecutable. La versión de fórmula sube de forma
independiente porque se crea un nuevo artefacto publicado; no se reescribe el
anterior.

Ambas versiones de fórmula deberán coexistir en el snapshot. El archivo actual
puede conservar su ruta; el nuevo deberá usar un nombre de archivo distinto que
incluya `1.1.0`. Lo mismo aplica a los casos. El validador seleccionará el
contrato por `schemaVersion` y la unicidad del catálogo será la pareja
`id` + `version`, no el ID aislado. Esta migración de layout y el nuevo hash de
dataset pertenecen a la tarea de materialización factual posterior, no al
cambio de schema sintético inmediato.

## Contrato estructurado mínimo propuesto

La estrategia `2.0.0` deberá declarar:

- `kind: PROGRAM`;
- `executionModel: CHECKED_INT64_V1`;
- una lista ordenada y no vacía de `steps`.

`CHECKED_INT64_V1` admite exclusivamente:

- inputs `INT32` o `INT64`, ampliando `INT32` a `INT64` sin pérdida;
- literales enteros de 64 bits;
- operaciones `CONSTANT`, `ADD`, `SUBTRACT`, `MULTIPLY` y
  `APPLY_ROUNDING`;
- overflow comprobado en cada operación;
- el modo de redondeo declarado por la definición, aplicado sólo por
  `APPLY_ROUNDING`.

Cada paso declara un `id`, una operación y operandos cerrados. Un operando es
exactamente una de estas variantes:

- `INPUT`, con un `inputId` declarado;
- `STEP`, con un `stepId` anterior;
- `LITERAL`, con un entero de 64 bits.

El programa de la nueva versión deberá expresar, en los mismos cinco pasos ya
aprobados:

1. un literal para `base`;
2. una resta entre `character-level` y un literal para
   `level-contribution`;
3. una multiplicación entre `vitality` y un literal para
   `vitality-contribution`;
4. la suma de los tres aportes para `raw-hp`;
5. `APPLY_ROUNDING` sobre `raw-hp` para `visible-hp`.

Esta enumeración describe la forma ya aprobada; los valores concretos deberán
vivir sólo en el nuevo JSON canónico y en sus casos, nunca en tipos, handlers o
pruebas C#.

Cada input añadirá `rangeErrorCode`. Así los errores
`formula-level-out-of-range` y `formula-stat-below-base` procederán de la
definición materializada y no de una asociación por nombre escrita en el
motor. `formula-not-applicable` y `formula-arithmetic-overflow` son invariantes
generales del evaluador.

Los contratos de traza y caso pueden conservar sus versiones actuales si la
implementación demuestra que representan sin pérdida la ejecución de
`CHECKED_INT64_V1`. Cualquier cambio necesario en ellos deberá decidirse antes
de modificar fixtures.

## Tipos y responsabilidades por capa

### Domain

Domain contendrá tipos inmutables y ajenos a JSON:

- `FormulaDefinition` y `FormulaReference`;
- `FormulaApplicability`;
- `FormulaInputDefinition` con tipo, bounds y `RangeErrorCode`;
- `CheckedIntegerFormulaProgram`, pasos, operaciones y operandos;
- `FormulaCalculationRequest`, contexto e inputs;
- `FormulaCalculationResult` y pasos de traza;
- `FormulaCalculationException` con códigos estables.

Los constructores deberán impedir colecciones mutables, IDs vacíos y valores
nulos. Las invariantes relacionales complejas se cerrarán durante la
materialización.

### Application

Un adaptador JSON dedicado leerá `character-classes/` y `formulas/` desde el
snapshot. No leerá casos de referencia durante la ejecución normal.

Antes de entregar un catálogo inmutable deberá exigir:

- un único `rulesetId`;
- cada pareja de identidad y versión única;
- estado `PUBLISHED`;
- clase y evoluciones existentes y coherentes;
- inputs y output con tipos soportados por el modelo de ejecución;
- `rangeErrorCode` presente por input;
- pasos únicos, en orden, con referencias sólo a inputs declarados o pasos
  anteriores;
- correspondencia exacta entre pasos del programa y `trace.stepIds`;
- salidas cruda/visible declaradas y etapa de redondeo coherente;
- ausencia de dependencias para esta primera vertical.

Un caso de uso recibirá una referencia exacta de fórmula, clase, evolución e
inputs. No resolverá “la última versión”, no conocerá constantes de HP y no
aceptará definiciones alternativas aportadas por la UI.

### Calculation Engine

El motor evaluará cualquier `CheckedIntegerFormulaProgram` válido mediante un
intérprete cerrado. No ramificará por ID de fórmula, clase, evolución, input o
paso.

El orden será:

1. exigir fórmula `PUBLISHED`;
2. comprobar clase y evolución;
3. exigir el conjunto exacto de inputs;
4. validar tipo y bounds usando el `rangeErrorCode` materializado;
5. ejecutar pasos en orden con aritmética `checked`;
6. aplicar el redondeo sólo en el paso declarado;
7. construir la traza desde los valores realmente calculados;
8. copiar identidad, provenance y conflictos desde la definición.

Una solicitud inválida no devuelve traza parcial.

## Errores

La primera implementación deberá conservar los cuatro códigos aprobados:

- `formula-not-applicable`;
- `formula-level-out-of-range`;
- `formula-stat-below-base`;
- `formula-arithmetic-overflow`.

Además necesita errores generales para input ausente, input no declarado,
fórmula no publicada y programa no soportado. Sus nombres exactos deberán
quedar fijados en el contrato `2.0.0` antes de crear la API pública; no se
inventarán durante la implementación del evaluador.

Los errores de snapshot o programa incoherente pertenecen a Application y no
deben confundirse con un input de usuario inválido.

## Estrategia de pruebas para la implementación posterior

La siguiente vertical deberá probar:

1. schemas y fixtures sintéticos para cada nueva forma y error;
2. gates semánticos mediante copias temporales mutadas;
3. intérprete con programas exclusivamente sintéticos para cada operación,
   orden, referencia, bounds, redondeo y overflow;
4. camino snapshot → Application → Domain → Calculation Engine usando la
   definición y los ocho casos canónicos `1.1.0`;
5. 4/4 trazas positivas y 4/4 códigos negativos leídos desde JSON, sin copiar
   inputs, constantes ni resultados en C#;
6. rechazo de `formula-hp-dark-wizard` `1.0.0` como definición no ejecutable
   cuando se solicite cálculo, conservando su validez histórica.

Las pruebas de contrato y las productivas son gates independientes. Que un JSON
valide contra schema no demuestra que el programa produzca la aritmética
esperada.

## Implementación Domain y Calculation Engine — 2026-07-25

La primera parte ejecutable quedó implementada sin materializar JSON:

- Domain representa definición/referencia, estado/confianza, aplicabilidad,
  inputs de 32/64 bits, bounds técnicos o factuales, `rangeErrorCode`, output
  `INT64`, redondeo, programa, pasos, operandos, solicitud, resultado y traza;
- las colecciones públicas se copian a estructuras inmutables y los
  constructores rechazan IDs vacíos, nulos, inputs duplicados y definiciones
  mínimas incompletas;
- `CheckedIntegerFormulaInterpreter` no conoce IDs de fórmula, clase,
  evolución, input o paso. Ejecuta únicamente `CONSTANT`, `ADD`, `SUBTRACT`,
  `MULTIPLY` y `APPLY_ROUNDING`, con `checked` en cada operación;
- la validación previa exige estado `PUBLISHED`, aplicabilidad y conjunto exacto
  de inputs. Los límites usan el código materializado por cada input;
- una ejecución correcta emite todos los pasos en orden y copia referencia,
  ruleset, contexto, inputs, redondeo, evidencia y conflictos. Un error no
  devuelve una traza parcial.

Los errores generales quedan fijados como:

- `formula-not-published`;
- `formula-not-applicable`;
- `formula-input-missing`;
- `formula-input-not-declared`;
- `formula-arithmetic-overflow`;
- `formula-program-not-supported`;
- `formula-program-invalid`.

El código de rango no se deriva del nombre del input: procede de
`FormulaInputDefinition.RangeErrorCode`. `CHECKED_INT64_V1` conserva
intermedios enteros, por lo que todos los modos declarados de redondeo son
identidad al aplicarse a un `Int64`; el paso sigue siendo obligatorio y
trazable.

Veinticinco pruebas exclusivamente sintéticas cubren las cinco operaciones,
estado, aplicabilidad, inputs exactos, bounds inclusivos/exclusivos y tipo de
32 bits, código de rango materializado, seis modos de redondeo, orden,
referencias adelantadas, programas no soportados, overflow de suma/resta/
multiplicación, traza/provenance e inmutabilidad frente a colecciones del
llamador.

Application, Data, WPF, las definiciones canónicas y los casos factuales no
cambiaron en esta tarea. La siguiente parte debe materializar el schema `2.0.0`
en Application y recorrer los ocho casos canónicos `1.1.0` sin copiar valores
del juego a C#.

## Implementación de Application — 2026-07-25

`JsonExecutableFormulaSnapshotReader` completa la materialización productiva
sin leer `reference-cases/`. Inspecciona `character-classes/` y `formulas/`,
reconoce `1.1.0` como schema histórico no ejecutable y construye tipos de
Domain exclusivamente para definiciones `2.0.0`.

Antes de crear `ExecutableFormulaCatalog` exige:

- identidad compuesta única `id` + `version` para todas las versiones;
- un único `rulesetId` entre clases y fórmulas;
- estado `PUBLISHED` para cada definición ejecutable;
- clase y evoluciones aplicables existentes en la misma familia;
- inputs contextuales, tipos, bounds y códigos de rango materializables;
- operaciones y aridades cerradas, inputs declarados y referencias sólo a
  pasos anteriores;
- correspondencia exacta entre pasos del programa y `trace.stepIds`;
- outputs raw/visible resolubles, etapa visible igual al redondeo y
  `APPLY_ROUNDING` como operación visible;
- cero dependencias de fórmulas en esta primera vertical.

El catálogo se copia a estructuras inmutables y resuelve únicamente una
`FormulaReference` exacta. `CalculatePublishedFormulaUseCase` obtiene esa
definición y delega sin transformación en
`CheckedIntegerFormulaInterpreter`. Una referencia histórica o inexistente se
rechaza con `formula-not-executable`; no existe selección de “última versión”.

Catorce pruebas nuevas de Application recorren archivos reales. Ocho reproducen
los 4/4 casos positivos con traza completa y los 4/4 errores canónicos de
`formula-hp-dark-wizard` `1.1.0`; otra rechaza `1.0.0`. Cinco controles fijan la
selección ejecutable exacta y el fallo cerrado ante estado no publicado,
evolución de otra familia, referencia adelantada y fórmula duplicada. No se
añadieron datos, constantes ni resultados de MU Online a C#; Data y WPF
permanecieron sin cambios.

## Criterios de salida del diseño

Este diseño queda cerrado cuando:

- la insuficiencia de `strategy.definition` está declarada;
- existe una única alternativa seleccionada;
- la inmutabilidad de `1.0.0` tiene una ruta de migración versionada;
- Domain, Application y Calculation Engine tienen responsabilidades separadas;
- Data y WPF permanecen fuera;
- la siguiente tarea puede modificar contratos sin decidir semántica durante
  la codificación.

Todos esos criterios quedan satisfechos por este documento. La implementación
del schema `2.0.0`, fixtures y gates se completó después como una tarea
independiente: conserva `v1`, representa las cinco operaciones con operandos
cerrados y rechaza referencias adelantadas, bounds incoherentes y divergencias
programa/traza. La tarea posterior ya materializó
`formula-hp-dark-wizard` `1.1.0` en `DRAFT`, conservó intacta `1.0.0` y adaptó
el gate a `schemaVersion` e identidad compuesta. Una tarea posterior
materializó también los ocho casos `1.1.0`, enlazó exclusivamente sus cuatro
positivos y demostró equivalencia exacta salvo la versión referenciada. La
revisión posterior de los nueve JSON no encontró divergencias y promovió
únicamente `status` a `PUBLISHED`. Domain, Calculation Engine y Application ya
completan la ejecución desde snapshot; la unión con inputs resueltos y WPF
pertenece a una vertical posterior.
