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

La validación integral se implementa en .NET 10 con `JsonSchema.Net 9.2.2` y
exige validación de formatos. Desde la raíz del repositorio se ejecuta con:

```powershell
dotnet run --project tools/validators/MuOnline.SchemaValidator -- --repository-root .
```

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
