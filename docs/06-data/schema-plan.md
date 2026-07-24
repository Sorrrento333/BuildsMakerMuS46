# Plan de esquemas

## Estado

La versión `1.0.0` de los primeros contratos está en `packages/schemas/v1`:

- `evidence.schema.json`
- `formula.schema.json`
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

El contrato de distribución `1.0.0` conserva la referencia al presupuesto de
progresión, el presupuesto ganado, las asignaciones por stat, el total gastado
y los puntos restantes. Sus contadores son enteros no negativos de 64 bits.
Las igualdades entre totales y la disponibilidad de cada stat dependen del
catálogo de clases y se declaran como invariantes semánticas en
`docs/04-domain/stat-distribution-contract.md`; no se simulan con datos del
juego dentro del schema.

El contrato de borrador `1.0.0` conserva metadata exacto del ruleset, dataset y
motor, las entradas de progresión y un `StatDistribution` completo compuesto
mediante `$ref`. Se mantiene separado de `build.schema.json`: el borrador actual
no inventa resets ni trata asignaciones como stats finales. Sus totales
calculados son una caché que Application recalcula y contrasta al cargar. Data
persiste payload y metadata atómicamente mediante la migración
`1/create_build_drafts`, según
`docs/06-data/build-draft-persistence-contract.md`.

Los primeros registros canónicos viven en
`packages/rulesets/mu-s4-global-reference/v1`: seis definiciones de clase y dos
reglas de progresión `VERIFIED`. El validador integral comprueba los ocho
archivos contra sus contratos. Las dos reglas están `PUBLISHED`: la regla
estándar enlaza cinco casos y la regla de Magic Gladiator/Dark Lord enlaza dos.
El gate semántico exige que cada `testCaseRef` resuelva a un fixture positivo
del mismo ruleset y regla, y que una regla publicada cubra todos sus casos
positivos. Tres controles negativos prueban segunda clase y exclusión de Magic
Gladiator/Dark Lord sin añadir claims factuales ni ser referencias publicadas.

La validación integral está implementada en .NET 10 bajo
`tools/validators/MuOnline.SchemaValidator`, con `JsonSchema.Net 9.2.2`
compilado reproduciblemente desde fuente MIT, validación de formatos y un
registro de schemas aislado por ejecución. Las
pruebas verifican que los ocho fixtures válidos sean aceptados, los ocho
inválidos sean rechazados, que los ocho registros canónicos sean válidos y que
los diez casos de progresión coincidan con su resultado esperado, además de que
las dos reglas resuelvan exactamente sus siete casos positivos y de que el
validador pueda ejecutarse repetidamente dentro del mismo proceso. El
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

## Plan restante

- `ruleset.schema.json`
- `quest-rule.schema.json`
- `reset-rule.schema.json`
- `item.schema.json`
- `skill.schema.json`
- `scenario.schema.json`
- `calculation-trace.schema.json`

Cada esquema tendrá ejemplos válidos e inválidos, versión, migración y pruebas de compatibilidad.
