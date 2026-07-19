# Flujo de datos

1. El usuario selecciona un `ruleset` y un `server_profile`.
2. La aplicación construye un `CharacterBuild` con datos base inmutables.
3. `progression` calcula presupuesto y restricciones.
4. `items` valida equipamiento y emite modificadores.
5. `formulas` ordena dependencias y evalúa cada nodo.
6. `scenarios` aplica contexto de objetivo y modalidad.
7. Se produce `CalculationResult` con valores y `CalculationTrace`.
8. La UI presenta resultado, advertencias, versión y fuentes.

Las importaciones externas pasan por staging, validación, normalización, revisión y publicación de snapshot. Nunca escriben directamente sobre datos productivos.
