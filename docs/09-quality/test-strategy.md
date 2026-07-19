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
