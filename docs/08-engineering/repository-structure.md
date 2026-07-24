# Estructura futura del repositorio

```text
/apps
  /web
  /desktop            # opcional
/packages
  /domain
  /calculation-engine
  /rulesets            # registros canónicos versionados por ruleset
  /schemas
  /data-access
  /ui-components
  /test-fixtures
/tools
  /importers
  /validators
  /research
/data
  /raw                # normalmente ignorado o LFS según licencia
  /normalized
  /published
/docs
/tests
```

La implementación exacta se ajustará a la tecnología aprobada, conservando fronteras conceptuales.

## Estructura .NET implementada

```text
/apps/desktop/MuOnline.BuildPlanner.App
/packages/domain/MuOnline.BuildPlanner.Domain
/packages/calculation-engine/MuOnline.BuildPlanner.CalculationEngine
/packages/application/MuOnline.BuildPlanner.Application
/packages/data-access/MuOnline.BuildPlanner.Data
/tests/MuOnline.BuildPlanner.Application.IntegrationTests
/tests/MuOnline.BuildPlanner.CalculationEngine.Tests
/tests/MuOnline.BuildPlanner.Data.IntegrationTests
/tools/validators/MuOnline.SchemaValidator
/tests/MuOnline.SchemaValidator.Tests
```

`MuOnline.BuildPlanner.App` es la superficie WPF aprobada por ADR-0004. Referencia
Application y Data en un único sentido, empaqueta el snapshot canónico bajo una
ruta estable del artefacto y expone el flujo mínimo de presupuesto ganado. El
modo automatizado de smoke test conserva además las verificaciones de SQLite y
publicación.

`MuOnline.BuildPlanner.Data` contiene exclusivamente infraestructura SQLite y
no es referenciado por el dominio ni el motor.

`MuOnline.BuildPlanner.Domain` contiene las definiciones puras de progresión,
la solicitud/resultado trazable del presupuesto y sus errores tipados. No
referencia persistencia, UI ni formatos de serialización.

`MuOnline.BuildPlanner.CalculationEngine` referencia únicamente Domain. Su
primera operación resuelve una regla de progresión `PUBLISHED` y calcula puntos
por nivel/Hero Status sin E/S. El test project materializa los JSON canónicos
desde el repositorio y ejecuta los siete casos positivos y tres controles
negativos; esa carga es infraestructura de prueba, no una dependencia del
motor.

`MuOnline.BuildPlanner.Application` referencia Domain y Calculation Engine.
Materializa el subconjunto de progresión de un snapshot JSON previamente
validado, exige reglas publicadas y referencias coherentes, y expone el primer
caso de uso sin depender de WPF ni SQLite. Sus pruebas de integración vuelven a
leer los siete casos positivos y tres controles negativos desde el ruleset, y
alteran copias temporales para demostrar el fallo cerrado. El catálogo expone
también nombres de clase/evolución provenientes del JSON para consumo de UI.

`packages/rulesets/mu-s4-global-reference/v1` contiene las primeras definiciones
factuales canónicas del juego. Se mantienen separadas de los fixtures sintéticos
de schemas y de futuros snapshots compilados/distribuibles. Sus casos de
referencia factuales viven bajo `reference-cases/progression/valid`; los
controles semánticos negativos se aíslan en el directorio hermano `invalid`.
El `.csproj` de WPF copia este árbol completo a
`rulesets/mu-s4-global-reference/v1` tanto en build como en publish; el código en
ejecución no busca rutas del repositorio.
