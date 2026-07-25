# Decisión técnica — contratos de fórmula, traza y casos

## Estado

- Fecha: 2026-07-25.
- Estado: `IMPLEMENTED` el 2026-07-25.
- Alcance: evolución de contratos; no materializa fórmulas, casos factuales ni
  código productivo.
- Primer consumidor previsto: el diseño revisado
  `DR-HP-DARK-WIZARD`.

## Problema

`formula.schema.json` `1.0.0` identifica una fórmula, sus cantidades, una
estrategia textual, el redondeo y referencias de evidencia/casos. No puede
representar de forma estructurada:

- clase y evoluciones a las que aplica;
- límites numéricos y su clasificación factual o técnica;
- procedencia semántica de cada input;
- pasos ordenados que debe exponer una traza;
- resultados positivos y errores esperados de casos ejecutables.

La definición de una fórmula, una ejecución concreta y un caso de referencia
tienen ciclos de vida distintos. Guardarlos en un único documento produciría
duplicación de resultados dentro del catálogo y obligaría a versionar una
definición factual cada vez que cambie el formato de una ejecución o se añada
un caso.

## Opciones comparadas

### Ampliar sólo `formula.schema.json`

Permitiría validar todo con un único contrato, pero mezclaría metadata estable,
inputs de una ejecución, resultados calculados y expectativas de pruebas. Una
definición canónica crecería con cada caso y no existiría un contrato reutilizable
para devolver trazas desde Calculation Engine, Application o UI.

### Crear contratos separados sin ampliar la fórmula

Separaría correctamente ejecuciones y casos, pero dejaría aplicabilidad, límites
y origen de inputs como texto libre o conocimiento implícito del motor. También
impediría comprobar que una traza usa los pasos declarados por la versión exacta
de la fórmula.

### Ampliar la definición y separar artefactos

Mantiene en la fórmula sólo lo estable y factual, y asigna contratos propios a
la ejecución y a las expectativas. Es la única opción que conserva a la vez
resolución cerrada, trazabilidad y evolución independiente.

## Decisión

Se adoptan tres contratos con responsabilidades no superpuestas:

1. `formula.schema.json` avanza de `1.0.0` a `1.1.0`.
2. `calculation-trace.schema.json` nace en `1.0.0`.
3. `formula-test-case.schema.json` nace en `1.0.0`.

La implementación materializa los tres contratos y fixtures exclusivamente
sintéticos. No publica `formula-hp-dark-wizard`, no copia sus ocho casos al
ruleset y no crea el evaluador.

## Responsabilidad de `formula.schema.json` `1.1.0`

La definición canónica seguirá conservando identidad, ruleset, estado,
confianza, estrategia, redondeo, evidencia, conflictos y referencias de casos.
Además declarará:

- `applicability`, con un `characterClassId` estable y el conjunto no vacío y
  sin duplicados de `evolutionIds`;
- límites por cantidad mediante `numericBounds`;
- procedencia semántica de cada input mediante `source`;
- una especificación ordenada de traza mediante `trace`.

### Aplicabilidad

`applicability` pertenece a la fórmula porque determina si la regla puede
resolverse antes de evaluar aritmética. No pertenece a una ejecución ni a un
caso. El schema comprobará forma e IDs; el gate semántico comprobará que clase
y evoluciones existan en el mismo ruleset y que cada evolución pertenezca a esa
clase.

No se introducirá una lista de nombres visibles ni una regla implícita de
herencia por familia. Un perfil privado podrá sustituir una definición completa
en el futuro, pero no mutará silenciosamente su aplicabilidad.

### Límites y clasificación

Cada cantidad podrá declarar `numericBounds` con:

- `minimum` y/o `maximum`;
- inclusividad explícita de cada extremo;
- `classification`: `TECHNICAL` o `FACTUAL`;
- `evidenceRefs` obligatorio cuando la clasificación sea `FACTUAL`.

El ancho de representación se declarará de forma independiente mediante
`numericType`, inicialmente `INT32`, `INT64` o `DECIMAL`. Así un extremo de tipo
no se presenta como máximo del juego. Los bounds técnicos no requieren evidencia
factual; los factuales sí y deben conservar su propia provenance.

JSON Schema validará presencia, tipos y rangos escalares. La coherencia
`minimum <= maximum`, la compatibilidad con `numericType` y la resolución de
evidencias pertenecen al gate semántico.

### Procedencia del input

Cada input declarará `source` con una de estas variantes cerradas:

- `CONTEXT_VALUE`, más un `valueId` semántico estable;
- `FORMULA_OUTPUT`, más ID y versión exacta de la fórmula dependiente y
  `outputStage` `RAW` o `VISIBLE`.

`CONTEXT_VALUE` significa que el valor llega ya resuelto por el contexto de
cálculo. No autoriza al motor de la fórmula a inventar cómo se compone. Un input
derivado deberá usar `FORMULA_OUTPUT`; no bastará con mencionarlo en
`strategy.definition`.

La primera versión no incorporará rutas JSON, nombres de controles WPF ni tipos
de persistencia. La resolución concreta pertenece a Application/Calculation
Engine y debe cerrarse contra el catálogo del mismo ruleset.

### Declaración de traza

La fórmula incluirá una declaración `trace` con:

- `stepIds`, lista ordenada, no vacía y sin duplicados;
- `rawOutputStepId`;
- `visibleOutputStepId`.

Los IDs describen el vocabulario estable de pasos de esa versión. La expresión
y el punto de redondeo continúan en `strategy` y `rounding`; no se introduce en
esta tarea un segundo lenguaje de expresiones ni un AST no aprobado.

El schema comprobará la forma. El gate semántico exigirá que ambos IDs de salida
estén en `stepIds`, que el paso visible sea el último y que una traza concreta
contenga exactamente esos pasos en ese orden.

## Responsabilidad de `calculation-trace.schema.json` `1.0.0`

Este contrato representará una ejecución inmutable, no una definición. Tendrá:

- identidad exacta de ruleset, fórmula y versión;
- clase y evolución resueltas;
- mapa de inputs recibidos;
- lista ordenada de pasos con `stepId` y valor;
- modo y etapa de redondeo aplicados;
- salida cruda y salida visible;
- referencias de evidencia y conflictos heredadas de la definición ejecutada.

El contrato estructural no decidirá si la aritmética es correcta. Calculation
Engine deberá producir la traza desde la definición cargada, y los gates
semánticos comprobarán orden, cobertura de pasos, correspondencia de contexto,
redondeo y resultado.

Una traza nunca podrá seleccionar otra fórmula por ID solamente: la versión es
obligatoria. Tampoco contendrá valores de UI ni metadata de SQLite.

## Responsabilidad de `formula-test-case.schema.json` `1.0.0`

Cada archivo será un caso independiente con ID estable, ruleset y referencia
exacta de fórmula/version. Declarará contexto e inputs y usará una unión cerrada:

- caso positivo: `expectedTrace`, validado mediante `$ref` a
  `calculation-trace.schema.json`;
- caso negativo: `expectedErrorCode`, sin resultado ni traza.

La definición de fórmula conservará `testCaseRefs`; no embutirá casos. Para una
fórmula `PUBLISHED`, el gate semántico exigirá que cada referencia resuelva a un
caso del mismo ruleset, fórmula y versión, y que todos los casos positivos
canónicos de esa versión estén enlazados. Los controles negativos se conservarán
en una carpeta separada y no serán referencias de publicación, igual que en la
vertical de progresión.

## Versionado y compatibilidad

- `1.0.0` de fórmula permanece como contrato histórico inmutable.
- `1.1.0` añade capacidad; no se reescribirá el contenido de `1.0.0` en Git.
- No existen fórmulas canónicas que deban migrarse. El fixture sintético de
  fórmula se actualiza para probar `1.1.0`.
- Trazas y casos versionan por separado porque pueden evolucionar sin cambiar
  el significado factual de una fórmula.
- Toda referencia ejecutable usa ID y versión; no se resuelve implícitamente
  “la última”.

## Validación de la implementación

La implementación de schemas añade:

- un fixture válido y uno inválido, ambos sintéticos, para cada contrato nuevo
  o actualizado;
- resolución real de los `$ref` entre caso y traza;
- pruebas de rechazo para aplicabilidad vacía/duplicada, bounds factuales sin
  evidencia, source incompleto, pasos duplicados o salidas fuera de la lista;
- prueba de la unión positiva/negativa de casos;
- actualización del inventario del validador, prueba estructural y harness de
  compilación fuente.

Las relaciones con clases, evoluciones, evidencias y cobertura de casos se
añadirán al gate semántico al materializar la primera fórmula factual.
JSON Schema por sí solo no sustituye esas comprobaciones.

El validador integral cubre diez contratos y veinte fixtures. Las pruebas
focalizadas ejercitan por separado los rechazos enumerados y alteran una traza
anidada para demostrar la resolución real del `$ref`. El gate semántico mínimo
de esta tarea exige que ambas salidas pertenezcan a `stepIds` y que la salida
visible sea el último paso. Todos los valores e IDs usados son sintéticos.

## Fuera de alcance

- materializar fórmulas o casos de MU Online;
- implementar el motor de HP;
- definir un lenguaje ejecutable nuevo;
- decidir precisión intermedia de otras fórmulas;
- incorporar límites de nivel o stats no demostrados;
- cambiar ruleset, dataset, Application, Data o WPF.

## Consecuencias

- La vertical de schema aumenta el inventario de ocho a diez contratos.
- La definición permanece compacta y auditable; las ejecuciones y pruebas no
  alteran su versión factual.
- El motor deberá fallar cerrado ante aplicabilidad, dependencias o versiones
  que no resuelvan exactamente.
- La traza visible podrá viajar entre capas sin acoplarse a WPF o SQLite.
- `DR-HP-DARK-WIZARD` conserva su gate: después de los schemas todavía faltarán
  definición canónica, casos ejecutables, validación semántica y motor.
