# Estructura futura del repositorio

```text
/apps
  /web
  /desktop            # opcional
/packages
  /domain
  /calculation-engine
  /rulesets
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
/packages/data-access/MuOnline.BuildPlanner.Data
/tests/MuOnline.BuildPlanner.Data.IntegrationTests
/tools/validators/MuOnline.SchemaValidator
/tests/MuOnline.SchemaValidator.Tests
```

`MuOnline.BuildPlanner.App` es la base WPF aprobada por ADR-0004. Sólo contiene
una ventana mínima y el modo automatizado de smoke test de publicación; todavía
no implementa flujos ni datos del producto.

`MuOnline.BuildPlanner.Data` contiene exclusivamente infraestructura SQLite y
no es referenciado por el dominio ni el motor, que todavía no se implementaron.
