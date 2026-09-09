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

1. Materializar los contratos fácticos restantes que `EVD-0026` preserva sin
   motor: rates, regeneración y el resto de Defense. Los buffs de Summoner
   quedaron cerrados el 2026-09-09.
2. Builds completas, resto del motor de cálculo y flujos de UI posteriores al
   presupuesto ganado y los borradores locales.
3. Trazas de cálculo de alto nivel aún sin contrato propio si el motor lo
   exige en una vertical posterior.

## Última tarea cerrada

La vertical de Buffs de Summoner quedó cerrada como axiomas del ruleset,
trazándose exclusivamente desde `EVD-0021` y `EVD-0026`:

- Cuatro fórmulas `1.0.0` nacen `PUBLISHED` contra schema `2.1.0`:
  `formula-reflect-percent-summoner` (`30 + ene/42`),
  `formula-berserker-percent-summoner` (`ene/30`),
  `formula-innovation-percent-summoner` (`ene/90 + 20`) y
  `formula-weakness-percent-summoner` (`ene/65 + 7`), sobre la base de
  Energy 23 y salida `percent`. Son exteriores al inventario de 24 claims de
  `RES-0002`.
- Cada programa trunca hacia cero una sola vez en el paso visible; las tres de
  constante exponen `energy-term` y suman la constante exacta con aritmética
  decimal comprobada.
- Dieciséis positivos (tres por evolución base y uno por Dimension Master) y
  ocho controles negativos; `conflictIds` vacío porque `DSP-0002` no aplica
  fuera de Magic Gladiator.
- Application y WPF materializan cuarenta y dos fórmulas ejecutables; el
  dataset avanza a `2026-07-30.1` con hash
  `sha256:ea9ff19fbc487d55de526e1faed3ffac2f7d49e0ee19051602b229b88ddf4655`.

## Verificación del cierre

- Restauración y build Release aprobados con 0 advertencias/0 errores;
  344/344 pruebas pasan: 40 validator, 58 motor, 228 Application y 18 Data.
- CLI del validador: las cuarenta y dos fórmulas `PUBLISHED` pasan sin
  errores; el gate factual cubre 172 positivos y 168 controles negativos.
- Smoke WPF `win-x64` (42 fórmulas y 168 casos contextuales): PASS local el
  2026-09-09 con SQLite `3.53.3`, 824 archivos, 149.359.513 bytes, 401 JSON
  del ruleset y hash
  `sha256:ea9ff19fbc487d55de526e1faed3ffac2f7d49e0ee19051602b229b88ddf4655`.

## Última tarea cerrada — schemas de alto nivel

Los cinco contratos de alto nivel restantes quedaron materializados en
`packages/schemas/v1` `1.0.0` con fixtures sintéticos:

- `ruleset` identifica el ruleset, contenido habilitado, fórmulas y fuentes;
- `quest-rule` modela series, etapas, prerrequisitos, elegibilidad y casos de
  prueba de quests;
- `item` modela la `ItemDefinition` canónica con módulos de opciones
  (Normal/Excellent/Ancient/Harmony/Socket) y sockets; la instancia con nivel,
  opciones y sockets elegidos permanece en `build.schema.json`;
- `skill` modela definiciones activas, pasivas y buffs con niveles y evoluciones
  elegibles;
- `scenario` modela modalidad (PVP/PVM/híbrido), objetivo, mapas y buffs
  externos.

El inventario del validador y de la comprobación estructural pasa de once a
dieciséis contratos y de veintidós a treinta y dos fixtures. No se añadieron
datos ni fórmulas factuales al ruleset canónico.

## Última tarea cerrada — daño y wizardry de Summoner y Magic Gladiator

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

## Cierre de Schemas de alto nivel — verificación

- Cinco contratos nuevos en `1.0.0` con diez fixtures sintéticos; el validador
  integral acepta los dieciséis fixtures válidos y rechaza los dieciséis
  inválidos (16 contratos/32 fixtures).
- Comprobación estructural PowerShell y pruebas `AllVersionedFixtures…` de
  contrato aprobadas para el nuevo inventario; el harness fuente ejecuta
  32/32 fixtures en dos rutas independientes.
- No se alteraron contrato, ruleset, dataset, motor ni smoke de publicación:
  los cambios son exclusivamente de inventario de schemas y verificación.

## Primera acción concreta

Confirmar con el mantenedor el primer candidato restante (contratos fácticos de
`EVD-0026` sin motor, builds/flujos de UI posteriores al borrador, o trazas de
cálculo de alto nivel) y actualizar esta documentación y `CHANGELOG.md` al
cerrarlo.