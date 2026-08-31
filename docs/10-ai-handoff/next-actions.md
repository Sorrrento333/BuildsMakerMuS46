# Próximas acciones

## Regla de planificación

Cada tarea prioritaria debe ser una vertical coherente y verificable que, cuando
el estado técnico lo permita, incluya diseño, implementación, integración,
pruebas, documentación y smoke. No dividir una capacidad salvo que exista un
gate real que impida continuar de forma segura.

La instrucción de ejecutar sólo la primera tarea pendiente continúa vigente.

## Prioridad inmediata

1. Cerrar la documentación y el commit de la vertical de Defense/SD de Summoner,
   que materializa el último claim `VERIFIED` de `RES-0002`. Capturar el hash y la
   versión definitiva del dataset (`2026-07-29.4`) con el smoke WPF de publicación
   y registrar el archivo/byte counts correspondientes.
2. Definir la siguiente vertical coherente entre los candidatos documentados:
   primeros contratos facticos restantes (daño y wizardry de Summoner/MG ya
   aprobados y sin motor), o los schemas de alto nivel (ruleset, quests, ítems,
   skills, escenarios y trazas), o builds completas/flujos de UI posteriores.

## Última tarea cerrada

La vertical de Defense/SD de Summoner quedó cerrada sobre la decisión general
`RAW` del propietario y sobre los truncamientos independientes de `EVD-0032`:

- `formula-defense-summoner` y `formula-sd-summoner` `1.0.0` están
  `PUBLISHED` contra schema `2.1.0` y trazan `EVD-0021`, `EVD-0026`, `EVD-0032`
  y `EVD-0034` (conflicto `DSP-0004` conservado y resuelto).
- Defense conserva `agility / 3` sobre los mínimos STR 21/AGI 21/VIT 18/ENE 23.
  SD conserva los tres truncamientos independientes
  `trunc((str+agi+vit+ene)*1.2) + trunc(defense/2) + trunc((lvl*lvl)/30)`,
  consumiendo Defense `RAW` por `FORMULA_OUTPUT`.
- `CHECKED_DECIMAL_V1` y sus gates aceptan varios pasos intermedios
  `APPLY_ROUNDING`; el último paso visible sigue siendo el redondeo que consume
  `rawOutputStepId`. El intérprete entero se relaja de forma inocua.
- Cuatro casos de Defense y cuatro de SD reproducen outputs/trazas.
  `sd-summoner-base` fija SD 102 y discrimina la semántica independiente (a
  plena precisión sería 103). No hay frontera RAW/VISIBLE para Summoner.
- Application y WPF materializan treinta fórmulas ejecutables; el dataset avanza
  a `2026-07-29.4` (hash capturado por el smoke de publicación pendiente).

## Verificación del cierre

- Restauración y build Release aprobados con 0 advertencias/0 errores; 320/320
  pruebas pasan: 40 validator, 58 motor, 204 Application y 18 Data.
- CLI del validador: las treinta fórmulas `PUBLISHED` pasan sin errores,
  incluidos `formula-defense-summoner` (4 positivos/2 controles) y
  `formula-sd-summoner` (4 positivos/7 controles).
- Smoke WPF `win-x64` (30 fórmulas y 120 casos contextuales): requiere una
  ejecución del mantenedor para capturar hash/versión/bytes del dataset.

## Primera acción concreta

Actualizar `docs/10-ai-handoff/current-status.md`, `next-actions.md` y
`CHANGELOG.md`, y confirmar con el mantenedor la siguiente vertical elegida.
