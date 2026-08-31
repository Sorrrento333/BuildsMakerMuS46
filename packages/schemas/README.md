# Schemas de dominio

Contratos JSON Schema 2020-12 versionados de forma independiente. Las versiones
iniciales viven en `v1/`; una versión publicada no se modifica de forma
incompatible. `v2/` conserva el primer salto incompatible, limitado al contrato
ejecutable de fórmula.

Los ejemplos de `examples/valid` y `examples/invalid` son fixtures técnicos.
Los identificadores, valores y expresiones que contienen son sintéticos y no
representan datos ni fórmulas de MU Online.

## Convenciones

- IDs estables en minúsculas, separados por guiones.
- Referencias entre agregados mediante IDs, nunca nombres visibles.
- `schemaVersion` identifica la versión del contrato.
- Datos factuales y fórmulas enlazan registros de evidencia.
- Campos no declarados se rechazan salvo en mapas de stats y overrides, cuyas
  claves dependen del ruleset.

La validación integral se implementa en .NET 10 con `JsonSchema.Net 9.2.2`
compilado desde fuente MIT fijada y exige validación de formatos. En un checkout
limpio, primero se generan y verifican los ensamblados sin guardarlos en Git:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/spikes/Test-JsonEverythingSourceBuild.ps1
```

Después, desde la raíz del repositorio, el validador se ejecuta con:

```powershell
dotnet run --project tools/validators/MuOnline.SchemaValidator -- --repository-root .
```

Los locks, SPDX, provenance y hashes revisados viven en
`spikes/json-everything-source-build/`. El diseño, la integración y sus límites
están en `docs/03-architecture/json-everything-source-integration.md`.

El comando valida siete contratos `1.0.0`, los contratos `formula`,
`stat-distribution` y `build-draft` `1.1.0`, y
`v2/formula.schema.json` `2.0.0`/`2.1.0`. Acepta los once fixtures de
`examples/valid`, rechaza los once de `examples/invalid` y devuelve un código
distinto de cero si alguna expectativa no se cumple. También valida contra
`character-class.schema.json`, `progression-rule.schema.json` y
`formula.schema.json` los veintinueve registros canónicos de
`packages/rulesets/mu-s4-global-reference/v1`; estos registros no son fixtures
sintéticos. Además ejecuta siete casos de referencia
factuales de progresión y exige el rechazo de tres controles semánticos
inválidos. El gate de fórmula valida ochenta y cuatro casos factuales positivos
y noventa y seis controles negativos entre la definición histórica y las
veinte definiciones ejecutables, sin
ejecutar un cálculo productivo. Esta comprobación
pertenece al tooling de validación y no constituye el motor. Las pruebas de
contrato se ejecutan con:

```powershell
dotnet test --solution MUOnline.BuildPlanner.slnx
```

La comprobación estructural complementaria se mantiene disponible con Windows
PowerShell:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/schemas/Test-SchemaStructure.ps1
```

CI restaura las dependencias bloqueadas, compila en Release y ejecuta las
pruebas mediante Microsoft Testing Platform, seleccionado en `global.json`.
Los fixtures siguen siendo exclusivamente técnicos y sintéticos.

La decisión de contratos de fórmula está documentada en
`docs/06-data/formula-schema-contract-decision.md`. `formula.schema.json`
`1.1.0` conserva aplicabilidad, bounds clasificados, procedencia cerrada de
inputs y declaración de pasos. Las ejecuciones y los casos usan respectivamente
`calculation-trace.schema.json` y `formula-test-case.schema.json` `1.0.0`.
Los tres forman parte del inventario validado con fixtures exclusivamente
sintéticos. El caso positivo resuelve mediante `$ref` el contrato real de traza;
ninguno de esos fixtures materializa una fórmula de MU Online. La primera
definición factual y sus casos viven separadamente en el ruleset canónico.

`v2/formula.schema.json` admite `2.0.0` y su evolución compatible `2.1.0`.
Los inputs de `2.1.0` pueden declarar `DECIMAL` para consumir sin pérdida una
salida `RAW`. `FORMULA_OUTPUT` conserva referencia/version exactas y etapa
obligatoria `RAW`/`VISIBLE`; Application rechaza dependencias ausentes,
incompatibles o cíclicas. `CHECKED_DECIMAL_V1` admite además `DIVIDE` binario
con divisor no nulo; `CHECKED_INT64_V1` lo rechaza.
`2.0.0` sustituye la estrategia textual por un `PROGRAM` con modelo
`CHECKED_INT64_V1`. Admite inputs `INT32`/`INT64`, literales `INT64` y
exclusivamente `CONSTANT`, `ADD`, `SUBTRACT`, `MULTIPLY` y
`APPLY_ROUNDING`, con aridades cerradas. Cada input exige bounds y un
`rangeErrorCode`. Sus operandos son una unión exclusiva `INPUT`, `STEP` o
`LITERAL`; el gate semántico exige que las referencias `STEP` apunten hacia
atrás, que los inputs existan, que los bounds sean coherentes con el tipo y que
programa, salidas, redondeo y traza coincidan exactamente. Los dos fixtures
`formula-v2` son sintéticos. El ruleset conserva la fórmula histórica `1.0.0`
contra `v1` y contiene `formula-hp-dark-wizard` `1.1.0` y
`formula-hp-dark-knight` `1.0.0`, `formula-hp-fairy-elf` `1.0.0` y
`formula-hp-summoner` `1.0.0` y `formula-hp-magic-gladiator` `1.0.0`, las cinco
`PUBLISHED` contra `2.0.0` y con cuatro positivos propios. `2.1.0` añade
`CHECKED_DECIMAL_V1`, literales decimales exactos y trazas fraccionarias sin
redondeo intermedio; `formula-hp-dark-lord` `1.0.0` es su primera definición
publicada. El
gate canónico resuelve el schema mediante `schemaVersion`, conserva ambas
versiones sin selección implícita de “la última” y rechaza identidades
compuestas duplicadas de fórmulas o casos.

`progression-rule.schema.json` representa puntos base otorgados al subir de
nivel y un bonus opcional condicionado por quest. Separa el nivel mínimo para
completar la quest del nivel desde el que se aplica la retroactividad y exige
declarar clases, evoluciones elegibles, evidencia y casos de prueba. Las
relaciones semánticas entre esos campos se validarán además en el dominio.

`stat-distribution.schema.json` `1.1.0` representa una distribución trazable del
presupuesto ganado más resets configurables: referencia la regla de progresión,
conserva cantidad/puntos por reset, producto y total distribuible, y enumera
asignaciones, puntos gastados y remanente. El schema valida rangos escalares; la suma de
asignaciones y su coincidencia exacta con los stats declarados por la clase son
invariantes del dominio documentadas en
`docs/04-domain/stat-distribution-contract.md`. Por ello `command` sólo será
admitido cuando exista en la definición canónica de la clase y no mediante una
lista de clases duplicada en el contrato. `StatDistributionCalculator` aplica
ya estas invariantes semánticas sobre el presupuesto productivo; el schema
continúa siendo el contrato serializable independiente.

`build-draft.schema.json` `1.1.0` compone el contrato de distribución mediante `$ref` y
conserva identidad, metadata exacto de ruleset/dataset/motor y las entradas de
progresión y resets. Las entradas y asignaciones son datos del usuario; los totales y la
referencia de regla se guardan sólo como caché que Application deberá recalcular
y comparar al cargar. No sustituye a `build.schema.json`, que representa una
build más completa con otros campos aún fuera del flujo actual. El
límite y sus invariantes están en
`docs/06-data/build-draft-persistence-contract.md`.

Los binarios NuGet publicados de Json Everything ya no forman parte del grafo
normal. El validador consume referencias directas a los DLL autocompilados y su
lock sólo añade `Humanizer.Core 3.0.10`. La publicación WPF sigue sin incluir el
validador. La evaluación y el gate se documentan en
`docs/03-architecture/json-everything-dependency-evaluation.md`.
