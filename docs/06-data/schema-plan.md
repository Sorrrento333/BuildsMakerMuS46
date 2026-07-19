# Plan de esquemas

## Estado

La versión `1.0.0` de los primeros contratos está en `packages/schemas/v1`:

- `evidence.schema.json`
- `formula.schema.json`
- `character-class.schema.json`
- `server-profile.schema.json`
- `build.schema.json`

Incluyen ejemplos técnicos válidos e inválidos en
`packages/schemas/examples`. No contienen datos ni fórmulas factuales del
juego.

La validación integral está implementada en .NET 10 bajo
`tools/validators/MuOnline.SchemaValidator`, con `JsonSchema.Net 9.2.2`
compilado reproduciblemente desde fuente MIT, validación de formatos y un
registro de schemas aislado por ejecución. Las
pruebas verifican que los cinco fixtures válidos sean aceptados, los cinco
inválidos sean rechazados y el validador pueda ejecutarse repetidamente dentro
del mismo proceso. El workflow `.github/workflows/ci.yml` restaura en modo
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
