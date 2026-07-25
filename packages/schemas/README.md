# Schemas de dominio

Contratos JSON Schema 2020-12 versionados de forma independiente. La versión
inicial es `1.0.0` y vive en `v1/`; una versión publicada no se modifica de
forma incompatible.

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

El comando valida siete contratos `1.0.0` y los contratos `formula`,
`stat-distribution` y `build-draft` `1.1.0`, acepta los diez fixtures de
`examples/valid`, rechaza los diez de `examples/invalid` y devuelve un código
distinto de cero si alguna expectativa no se cumple. También valida contra
`character-class.schema.json`, `progression-rule.schema.json` y
`formula.schema.json` los nueve registros canónicos de
`packages/rulesets/mu-s4-global-reference/v1`; estos registros no son fixtures
sintéticos. Además ejecuta siete casos de referencia
factuales de progresión y exige el rechazo de tres controles semánticos
inválidos. El gate de fórmula valida cuatro casos factuales positivos y cuatro
controles negativos sin ejecutar un cálculo productivo. Esta comprobación
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
