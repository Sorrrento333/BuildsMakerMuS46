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

El comando valida los cinco contratos `1.0.0`, acepta los cinco fixtures de
`examples/valid`, rechaza los cinco de `examples/invalid` y devuelve un código
distinto de cero si alguna expectativa no se cumple. Las pruebas de contrato se
ejecutan con:

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

Los binarios NuGet publicados de Json Everything ya no forman parte del grafo
normal. El validador consume referencias directas a los DLL autocompilados y su
lock sólo añade `Humanizer.Core 3.0.10`. La publicación WPF sigue sin incluir el
validador. La evaluación y el gate se documentan en
`docs/03-architecture/json-everything-dependency-evaluation.md`.
