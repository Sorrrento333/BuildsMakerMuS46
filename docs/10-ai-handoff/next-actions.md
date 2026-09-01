# CONTENIDO UNIFICADO (rebase: union preservadora reset-aware-drafts-traced-formulas + origin/main)

# Próximas acciones

## Regla de planificación

Cada tarea prioritaria debe ser una vertical coherente y verificable que, cuando
el estado técnico lo permita, incluya diseño, implementación, integración,
pruebas, documentación y smoke. No dividir una capacidad salvo que exista un
gate real que impida continuar de forma segura.

La instrucción de ejecutar sólo la primera tarea pendiente continúa vigente.

## Prioridad inmediata

1. Cerrar la documentación y el commit de la vertical de Defense/SD de Summoner
   quedó pendiente solo de capturar el hash y la versión definitiva del dataset
   (`2026-07-29.4`) con el smoke WPF de publicación. Ese smoke ya pasó localmente
   el 2026-09-01 con SQLite `3.53.3`, 738 archivos, 149.256.211 bytes, 315 JSON
   del ruleset, treinta fórmulas y 120 casos contextuales; el hash es
   `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.
   Sólo resta el commit y, en su caso, la ejecución del CJ en un runner remoto.
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
  a `2026-07-29.4` con hash
  `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.

## Verificación del cierre

- Restauración y build Release aprobados con 0 advertencias/0 errores; 320/320
  pruebas pasan: 40 validator, 58 motor, 204 Application y 18 Data.
- CLI del validador: las treinta fórmulas `PUBLISHED` pasan sin errores,
  incluidos `formula-defense-summoner` (4 positivos/2 controles) y
  `formula-sd-summoner` (4 positivos/7 controles).
- Smoke WPF `win-x64` (30 fórmulas y 120 casos contextuales): PASS local el
  2026-09-01 con SQLite `3.53.3`, 738 archivos, 149.256.211 bytes, 315 JSON del
  ruleset y hash `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.

## Primera acción concreta

Confirmar con el mantenedor la siguiente vertical elegida entre los candidatos
documentados (contratos facticos restantes, schemas de alto nivel o builds/
flujos de UI) y actualizar esta documentación y `CHANGELOG.md` al cerrarla.

---

## Aportes propios de nuestra rama (no presentes en origin/main)

# Próximas acciones

## Regla de planificación

Cada tarea prioritaria debe ser una vertical coherente y verificable que, cuando
el estado técnico lo permita, incluya diseño, implementación, integración,
pruebas, documentación y smoke. No dividir una capacidad salvo que exista un
gate real que impida continuar de forma segura.

La instrucción de ejecutar sólo la primera tarea pendiente continúa vigente.

## Prioridad inmediata

Confirmar con el mantenedor la siguiente vertical entre los candidatos
documentados y cerrarla con integración, pruebas, documentación y smoke:

1. Schemas de alto nivel restantes (`ruleset`, quests, ítems, skills,
   escenarios y trazas); el validador integral/CI ya cubre el contrato de
   progresión y los contratos de fórmula.
2. Materializar los contratos fácticos restantes que `EVD-0026` preserva sin
   motor: rates, regeneración, buffs, Reflect/Berserker/Innovation/Weakness de
   Summoner y el resto de Defense.
3. Builds completas, resto del motor de cálculo y flujos de UI posteriores al
   presupuesto ganado y los borradores locales.

## Última tarea cerrada

La vertical de Daño y Wizardry de Summoner y Magic Gladiator quedó cerrada
como axiomas del ruleset trazándose exclusivamente desde `EVD-0021` y
`EVD-0026`:

- Ocho fórmulas `1.0.0` nacen `PUBLISHED` contra schema `2.1.0`:
  `formula-{min,max}-damage-{summoner,magic-gladiator}` y
  `formula-{min,max}-wizardry-{summoner,magic-gladiator}`. Son exteriores al
  inventario de 24 claims de `RES-0002`; las cuatro de Magic Gladiator
  conservan el conflicto resuelto `DSP-0002`.
- Summoner conserva `str/8`, `str/4`, `ene/9` y `ene/4` sobre STR 21/ENE 23.
  Magic Gladiator conserva `str/6 + ene/12`, `str/4 + ene/8` y las mismas
  wizardry `ene/9` y `ene/4` sobre 26/26. Cada programa trunca hacia cero una
  sola vez en el paso visible; los pasos de daño de Magic Gladiator suman los
  aportes Strength y Energy con aritmética comprobada antes del truncamiento.
- Treinta y dos positivos (tres por evolución base y uno por la superior) y
  dieciocho controles negativos. No hay frontera RAW/VISIBLE ni controles de
  overflow (la suma máxima de coeficientes por salida es `3/8`).
- Application y WPF materializan treinta y ocho fórmulas ejecutables; el
  dataset avanza a `2026-07-29.5` con hash
  `sha256:cb00836252cf4a22dab6d4343e3c5ad7186907a2e382839334645b5ed6539c3a`.

## Verificación del cierre

- Restauración y build Release aprobados con 0 advertencias/0 errores; 320/320
  pruebas pasan: 40 validator, 58 motor, 204 Application y 18 Data.
- CLI del validador: las treinta y ocho fórmulas `PUBLISHED` pasan sin errores;
  el gate factual cubre 156 positivos y 160 controles negativos.
- Smoke WPF `win-x64` (38 fórmulas y 152 casos contextuales): PASS local el
  2026-09-01 con SQLite `3.53.3`, 796 archivos, 149.324.815 bytes, 373 JSON del
  ruleset y hash `sha256:cb00836252cf4a22dab6d4343e3c5ad7186907a2e382839334645b5ed6539c3a`.

## Primera acción concreta

Confirmar con el mantenedor la siguiente vertical elegida entre los candidatos
documentados (schemas de alto nivel, contrato fáctico restante de `EVD-0026` o
builds/flujos de UI) y actualizar esta documentación y `CHANGELOG.md` al
cerrarla.