# Estrategia de pruebas

## Pirámide
- Unitarias: fórmulas, redondeos, invariantes.
- Contract: schemas, API, import/export.
- Integración: ruleset + perfil + motor + persistencia.
- End-to-end: flujos críticos de calculadora.
- Regresión: fixtures aprobados por clase y escenario.
- Property-based: límites, monotonicidad cuando corresponda, serialización.
- Golden tests: trazas y resultados de referencia, revisados conscientemente.

## Regla
Una fórmula no entra al ruleset publicado sin casos normales, bordes, valores inválidos y al menos un caso de referencia verificable.

Los casos numéricos asociados a claims `PARTIAL` se etiquetan como pruebas de
investigación. Pueden verificar que una transformación fue transcrita de forma
coherente —por ejemplo los bordes 1, 220 y 221 de Hero Status en `RES-0001`—,
pero no se convierten en golden tests ni fixtures del ruleset hasta que la
evidencia sea promovida y el conflicto aplicable esté cerrado.

Las dependencias compiladas desde fuente deben fijar commit, SDK y TFM, generar
SBOM/provenance, comparar hashes en rutas independientes y ejecutar los mismos
contratos que el proveedor sustituido. La repetibilidad en una sola carpeta no
se considera por sí sola evidencia suficiente de reproducibilidad.
