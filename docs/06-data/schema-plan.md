# Plan de esquemas

## Estado

Los primeros contratos están en `packages/schemas/v1`. Cinco
permanecen en `1.0.0`; fórmula y distribución están en `1.1.0`; borrador y
build están en `1.2.0` para la instancia equipada sin bonificaciones:

- `evidence.schema.json`
- `formula.schema.json`
- `calculation-trace.schema.json`
- `formula-test-case.schema.json`
- `character-class.schema.json`
- `progression-rule.schema.json`
- `stat-distribution.schema.json`
- `build-draft.schema.json`
- `server-profile.schema.json`
- `build.schema.json`

Incluyen ejemplos técnicos válidos e inválidos en
`packages/schemas/examples`. No contienen datos ni fórmulas factuales del
juego.

El contrato de progresión `1.0.0` modela puntos por nivel desde un primer nivel
premiado y un bonus opcional de quest con nivel mínimo, evoluciones elegibles,
puntos adicionales y origen retroactivo. Este diseño representa sin fórmulas
ocultas tanto la regla estándar con Hero Status como la progresión de MG/DL sin
quest; los fixtures actuales siguen siendo sintéticos.

El contrato de distribución `1.1.0` conserva la referencia al presupuesto de
progresión, inputs/producto de resets configurables, total distribuible,
asignaciones por stat, total gastado y puntos restantes. Sus contadores son
enteros no negativos de 64 bits.
Las igualdades entre totales y la disponibilidad de cada stat dependen del
catálogo de clases y se declaran como invariantes semánticas en
`docs/04-domain/stat-distribution-contract.md`; no se simulan con datos del
juego dentro del schema.

El contrato de borrador `1.2.0` conserva metadata exacto del ruleset, dataset y
motor, las entradas de progresión/resets, un `StatDistribution` completo compuesto
mediante `$ref` y el array `equipment` de instancias
`{ itemId, itemVersion, level }` (sin `uniqueItems`, el rechazo de duplicados es
semántico en Application). Se mantiene separado de `build.schema.json`: el borrador
actual no trata asignaciones como stats finales. Sus totales
calculados son una caché que Application recalcula y contrasta al cargar. Data
persiste payload y metadata atómicamente mediante la migración
`1/create_build_drafts`, según
`docs/06-data/build-draft-persistence-contract.md`.

El contrato de build `1.2.0` añade el mismo array `equipment` a la snapshot
validada de stats finales. Ambas versiones conservan la migración de carga
(preservando la anterior `1.1.0` y la legacy `1.0.0`) y revalidan `equipment`
contra el catálogo publicado al cargar. El diseño completo está en
`../04-domain/equipped-instance-design.md`.

Los registros canónicos viven en
`packages/rulesets/mu-s4-global-reference/v1`: seis definiciones de clase, dos
reglas de progresión `PUBLISHED` y dieciocho definiciones de fórmula `PUBLISHED`,
diecisiete de ellas ejecutables. El validador integral comprueba los veintiséis archivos
contra el contrato seleccionado por `schemaVersion` y exige identidad compuesta
`id` + `version` única. La regla
estándar enlaza cinco casos y la regla de Magic Gladiator/Dark Lord enlaza dos.
El gate semántico exige que cada `testCaseRef` resuelva a un fixture positivo
del mismo ruleset y regla, y que una regla publicada cubra todos sus casos
positivos. Tres controles negativos prueban segunda clase y exclusión de Magic
Gladiator/Dark Lord sin añadir claims factuales ni ser referencias publicadas.

La fórmula publicada enlaza sus cuatro casos positivos de la misma identidad y
versión. Sus cuatro controles negativos permanecen separados. La revisión de
publicación del 2026-07-25 confirmó el contrato aprobado sin cambiar expresión,
aplicabilidad, provenance, traza, redondeo ni expectativas.

La validación integral está implementada en .NET 10 bajo
`tools/validators/MuOnline.SchemaValidator`, con `JsonSchema.Net 9.2.2`
compilado reproduciblemente desde fuente MIT, validación de formatos y un
registro de schemas aislado por ejecución. Las
pruebas verifican que los diecisiete fixtures válidos sean aceptados, los diecisiete
inválidos sean rechazados, que los veintiséis registros canónicos sean válidos y que
los diez casos de progresión coincidan con su resultado esperado, además de que
las dos reglas resuelvan exactamente sus siete casos positivos y de que el
validador pueda ejecutarse repetidamente dentro del mismo proceso. Para las
dieciocho definiciones factuales comprueba identidad, catálogo, inputs, pasos,
outputs, redondeo, provenance y cobertura. Las referencias HP/Mana conservan
cuatro positivos y cuatro negativos; AG de Dark Wizard, Dark Knight y Fairy Elf
conservan cuatro positivos y seis negativos cada una para cubrir cada mínimo
factual. AG de Summoner y Magic Gladiator conservan cuatro positivos y cinco
negativos cada una: cubren los cuatro mínimos y familia sin inventar un
overflow imposible en su dominio. El
workflow `.github/workflows/ci.yml` restaura en modo
bloqueado, compila y ejecuta esas pruebas con Microsoft Testing Platform.

La comprobación PowerShell de estructura se conserva como control
complementario de JSON legible, metadatos y cobertura de fixtures.

La integración del 2026-07-19 retiró los binarios NuGet publicados de Json
Everything del grafo normal. El lock del validador sólo resuelve
`Humanizer.Core 3.0.10`; CI compila dos checkouts fijados, exige hashes y SPDX,
y publica una salida inspeccionada con aviso MIT y sin `OSMFEULA.txt`. La
verificación local pasó; falta confirmar el workflow actualizado en un runner
remoto limpio.

## Contratos de fórmula implementados

`formula-schema-contract-decision.md` resuelve el gate previo a implementación.
La definición usa `formula.schema.json` `1.1.0` para conservar
aplicabilidad, bounds clasificados, procedencia cerrada de inputs y los IDs
ordenados de traza. Las ejecuciones usarán
`calculation-trace.schema.json` `1.0.0`, y cada expectativa positiva o negativa
usará `formula-test-case.schema.json` `1.0.0`.

La separación evita mezclar metadata factual con resultados runtime o casos.
Los casos positivos componen el contrato de traza mediante `$ref`; las
referencias desde una fórmula conservarán ID y versión exactos y tendrán un gate
semántico equivalente al ya usado por progresión. La vertical posterior ya
materializó la primera fórmula factual y sus ocho casos, pero no añadió código
productivo. El validador prueba además
aplicabilidad, bounds factuales, variantes cerradas de procedencia, unicidad de
pasos, pertenencia de salidas y la unión exclusiva positivo/negativo.

El diseño de ejecución posterior detectó que `strategy.definition` continúa
siendo sólo texto: no enlaza aliases con inputs, pasos con operaciones ni
bounds con códigos de error. `packages/schemas/v2/formula.schema.json` `2.0.0`
ya implementa el programa `CHECKED_INT64_V1`, operaciones y operandos cerrados,
aridades explícitas y `rangeErrorCode` por input. Los gates rechazan inputs no
declarados, referencias adelantadas, bounds incoherentes y divergencias entre
programa, traza, salidas y redondeo. El contrato `v1` y la definición publicada
`formula-hp-dark-wizard` `1.0.0` permanecen inmutables. La versión factual
`1.1.0` está `PUBLISHED`, conserva la semántica aprobada mediante el programa
estructurado y enlaza cuatro positivos de su serie exacta de ocho casos. La
revisión de publicación no encontró divergencias y cambió sólo `status`; antes
de cualquier evaluación falta implementar el intérprete. La decisión completa está en
`../04-domain/dark-wizard-hp-execution-vertical-design.md`.

La evolución compatible `2.1.0` añade `CHECKED_DECIMAL_V1` y literales
decimales exactos para coeficientes finitos en base 10. Mantiene inputs y salida
`INT32`/`INT64`, conserva los intermedios fraccionarios en la traza y aplica el
redondeo exclusivamente en `APPLY_ROUNDING`. Las definiciones `2.0.0` continúan
exigiendo `CHECKED_INT64_V1`.

## Contratos de alto nivel implementados

Los cinco contratos restantes se materializaron en `packages/schemas/v1` con
versión `1.0.0`, fixtures válidos e inválidos sintéticos y registro en el
validador, la comprobación estructural y las pruebas: `ruleset` (identifica
reglaset, contenido habilitado, fórmulas y fuentes), `quest-rule` (series,
etapas, prerrequisitos y elegibilidad con casos de prueba), `item` (definición
canónica con módulos de opciones y sockets), `skill` (definiciones y buffs) y
`scenario` (modalidad, objetivo, mapas y buffs externos). El inventario del
validador pasa de once a diecisiete contratos y de veintidós a treinta y cuatro
fixtures.

La definición canónica de item se materializa como `item.schema.json`; la
instancia con nivel, opciones y sockets elegidos sigue siendo dato del usuario
en `build.schema.json`. Desde `1.2.0`, la instancia guardada es acotada y sin
bonificaciones: `equipment` de `{ itemId, itemVersion, level }`, donde el nivel
es un entero 0..`maxItemLevel` declarado y la ranura, requisitos y atributos se
derivan de la definición publicada del ítem (`../04-domain/equipped-instance-design.md`).

## Plan restante

El catálogo canónico de ítems quedó materializado para el axioma acotado del
propietario (`RES-0003`, `EVD-0040`): `item-kris`, `item-dragon-armor` y
`item-albatross-bow` `PUBLISHED` en
`packages/rulesets/mu-s4-global-reference/v1/items/`, validados contra
`item.schema.json` y registrados como `("item","items")` en el validador. El
dataset avanza a `2026-09-16.1`. Application ya consume el catálogo en la
vertical acotada de UC-04: `JsonItemCatalogSnapshotReader` materializa
`ItemDefinition` y `EquipItemUseCase` valida la elegibilidad de equipado por
clase y `requiredStats` en +0, sin bonificaciones ni instancia equipada
(`../04-domain/items-consumption-design.md`). La vertical `1.2.0` añade la
instancia equipada persistible sin bonificaciones, la elegibilidad como nivel
declarado 0..`maxItemLevel` y la revalidación al cargar
(`../04-domain/equipped-instance-design.md`). Quedan fuera del axioma
`requiredLevel`, la progresión de `requiredStats`, los sockets, las opciones y
cualquier otro ítem, ranura, campo o grado; su ampliación exige nueva evidencia
o una nueva decisión del propietario. La decisión del gate está en
`../04-domain/items-factual-gate-design.md` y el registro factual en
  `../05-research/registers/RES-0003-items-equipment.md`.

El catálogo acotado de skills quedó materializado para el axioma del propietario
(`RES-0004`, `EVD-0045`): ocho `SkillDefinition` `PUBLISHED` `VERIFIED` en
`packages/rulesets/mu-s4-global-reference/v1/skills/`
(`skill-impale`, `skill-twisting-slash`, `skill-swell-life`, `skill-death-stab`,
`skill-rageful-blow`, `skill-strike-of-destruction`, `skill-penetration` y
`skill-multi-shot`), validados contra `skill.schema.json` y registrados como
`("skill","skills")` en el validador (inventario canónico 119 → 127). El mapeo
`kind` aprobado asigna `ATK`/`Non-ATK`/`Debuff` → `ACTIVE` y `Buff` → `BUFF`;
`requiredLevel` es el `Character Level` publicado y `allowedEvolutionIds` son las
tres evoluciones de cada familia. El dataset avanza a `2026-09-17.1`. Quedan
fuera del axioma cualquier otra skill o buff, los prerrequisitos por
stat/quest/equipo y el `buffRef`; su ampliación exige nueva evidencia o una
nueva decisión del propietario. La decisión completa está en
`../04-domain/skills-factual-gate-design.md` y el registro factual en
`../05-research/registers/RES-0004-skills-buffs.md`.
