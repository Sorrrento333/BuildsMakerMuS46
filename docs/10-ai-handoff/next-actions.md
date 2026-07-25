# Próximas acciones

## Prioridad inmediata

1. Diseñar la primera vertical productiva de
   `formula-hp-dark-wizard` `1.0.0`, ya `PUBLISHED`, antes de escribir el
   evaluador.
2. Resolver explícitamente cómo Application materializará la definición JSON
   `1.1.0` en tipos de Domain y cómo Calculation Engine ejecutará expresión,
   aritmética comprobada, aplicabilidad, errores y traza sin duplicar `30`, `2`,
   evoluciones ni resultados canónicos en C#.
3. Mantener Data y WPF fuera de esa decisión. Si el contrato actual no permite
   una ejecución cerrada sin interpretar texto libre, documentar primero la
   ampliación de schema necesaria y sus gates.

## Última tarea cerrada

Se revisaron `formula-hp-dark-wizard` `1.0.0` y sus ocho casos contra
`docs/04-domain/dark-wizard-hp-formula-contract.md`, `EVD-0021`, `EVD-0026` y
`DSP-0003`. Identidad, provenance, aplicabilidad, inputs, bounds, expresión,
cinco pasos, redondeo, cuatro positivos enlazados y cuatro controles negativos
separados coinciden sin divergencias.

La única mutación factual fue promover `status` de `DRAFT` a `PUBLISHED`. La
prueba de contrato fija el nuevo estado. No se implementó cálculo de HP ni se
modificaron Domain, Calculation Engine, Application, Data o WPF para consumir
la fórmula.

## Verificación del cierre

- Restauración bloqueada: aprobada.
- Build Release: 0 advertencias, 0 errores.
- Solución .NET: 92/92 pruebas aprobadas.
- Schemas: 10 contratos y 20 fixtures estructuralmente legibles.
- Validador integral: 10/10 válidos aceptados, 10/10 inválidos rechazados,
  9/9 registros canónicos válidos, 10/10 casos de progresión conservados y
  4/4 casos positivos más 4/4 controles de fórmula válidos.
- Pruebas del validador: 20/20 aprobadas; el estado esperado es `PUBLISHED`.
- Smoke WPF `win-x64`: PASS con 450 archivos, 27/27 JSON canónicos y hash
  idéntico entre publicaciones. El dataset conserva versión `2026-07-24.1` y
  su hash observado es
  `sha256:b45eda3083634c43aa4eaead02e02945793075a3c6ee865973c8b4776917a7ad`.
  WPF empaqueta la fórmula publicada, pero no la materializa ni ejecuta.

## Primera acción concreta

Crear el diseño técnico de materialización y evaluación para la vertical
Domain/Calculation Engine de `formula-hp-dark-wizard` `1.0.0`, con una decisión
explícita sobre la suficiencia de `strategy.definition` antes de modificar
código productivo.
