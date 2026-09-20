# CONTENIDO UNIFICADO (rebase: union preservadora reset-aware-drafts-traced-formulas + origin/main)

# Próximas acciones

## Regla de planificación

Cada tarea prioritaria debe ser una vertical coherente y verificable que, cuando
el estado técnico lo permita, incluya diseño, implementación, integración,
pruebas, documentación y smoke. No dividir una capacidad salvo que exista un
gate real que impida continuar de forma segura.

La instrucción de ejecutar sólo la primera tarea pendiente continúa vigente.

## Prioridad inmediata

1. Instancia equipada sin bonificaciones implementada (2026-09-19): contratos
   `build-draft` y `build` a `1.2.0` con `equipment` de
   `{ itemId, itemVersion, level }`, sección "Equipo" en WPF y household de smoke
   equipado; sin opciones/sockets/bonificaciones ni progresión de ítem.
2. Traza de cálculo de alto nivel implementada (2026-09-19): contrato
   `build-calculation-trace` `1.0.0` con fixtures y gate semántico; emisión en
   Application (`BuildCalculationTraceFactory`); WPF con la sección "Traza de
   cálculo de alto nivel" y smoke en ambas fases (19 fórmulas, 3 aristas). Es un
   artefacto del motor: no añade datos factuales.
3. Skills como modificador de cálculo implementado (2026-09-19): siete fórmulas
   derivadas `2.1.0` por axioma `EVD-0046`; sin UI de skills ni `buffRef`.
   Ampliar efectos fuera de los siete exige nueva evidencia Season 4 o una nueva
   decisión del propietario. `buffRef` sigue omitido.
4. Catálogo acotado de skills materializado (2026-09-17): ocho `SkillDefinition`
   `PUBLISHED` del axioma `EVD-0045` contra `skill.schema.json`, con inventario
   canónico 119 → 127 y dataset `2026-09-17.1`.
5. Consumo acotado del catálogo de ítems implementado (`EquipItemUseCase`,
   selector de ranura/ítem en WPF y smoke): valida clase y `requiredStats` en
   +0; la instancia equipada acotada cerró la vertical `1.2.0`.
6. Alternativa documentada: ampliar UC-04 (bonificaciones ATK/DEF,
   `requiredLevel`, progresión de `requiredStats`, sockets) exige nueva
   evidencia Season 4 o una nueva decisión del propietario.
7. Alternativa documentada: master buys y pantallas restantes del flujo, sin
   contrato factual todavía.

## Instancia equipada sin bonificaciones — implementado (2026-09-19)

- `build-draft.schema.json` y `build.schema.json` a `1.2.0` con el array
  `equipment` de `{ itemId, itemVersion, level }` (schema y fixtures ya
  actualizados). La ranura y los atributos se deducen de la definición publicada
  del ítem; el nivel es un entero declarado `0..maxItemLevel`.
- `BuildEquipmentEntry` + `BuildEquipmentValidator` (códigos estables en
  `BuildErrorCodes`/`BuildDraftErrorCodes`: `item-not-found`,
  `version-mismatch`, `class-not-allowed`, `level-out-of-range`, `duplicate`,
  `requirements-not-met`). Validan en `SaveBuildDraftUseCase`, revalidan en
  `LoadBuildDraftUseCase`/`LoadBuildUseCase` contra el catálogo publicado y
  `SaveBuildUseCase` promueve el equipo del draft. `EquipItemRequest/Result`
  ganan `Level`/`MaxItemLevel` (`item-equip-level-out-of-range`).
- Migración de carga conserva `1.1.0+1.1.0` → `Equipment = []` y legacy
  `1.0.0+1.0.0`; `BuildDraftStatDistribution` queda en `1.1.0`.
- WPF: sección central "Equipo" (nivel + equipar/desequipar + lista +
  indicador); `_equippedItems` se rellena al cargar drafts/builds.
- Smoke: `publication-smoke-equip-draft`/`publication-smoke-equip-build`
  (`item-kris` `1.0.0` nivel 15, dark-knight 7, EB 30, agility 27, restante 23).
- Verificación PASS: build Release 0/0; 858/858 pruebas (43 validator, 58 motor,
  730 Application, 27 Data); `Test-SchemaStructure` 17/34; smoke WPF `win-x64`
  (SQLite `3.53.3`, 1369 archivos, 150.666.306 bytes, 946 archivos del ruleset,
  456 casos, 2 builds listados, dataset `2026-09-17.1`). Sin datos factuales
  nuevos. Diseño en `docs/04-domain/equipped-instance-design.md`.

## Traza de cálculo de alto nivel — implementado (2026-09-19)

- Nuevo contrato `build-calculation-trace` `1.0.0` en
  `packages/schemas/v1/build-calculation-trace.schema.json`, fixture válido e
  inválido en `packages/schemas/examples/{valid,invalid}` y registro en el
  validador (16 → 17 contratos; 32 → 34 fixtures).
- Gate semántico `MatchesBuildCalculationTraceSemantics`: posiciones contiguas
  `0..n-1`, fuentes de dependencia dentro de la secuencia y aristas únicas por
  entrada.
- `BuildCalculationTrace` y `BuildCalculationTraceFactory` en
  `packages/application/MuOnline.BuildPlanner.Application/Formulas` emiten la
  macro-traza desde `CharacterBuildEvaluation`: orden determinista, salidas crudo/
  visible, unidades y dependencias directas a partir de inputs declarados
  `FORMULA_OUTPUT` (etapa `RAW`/`VISIBLE`). WPF la muestra en
  "Traza de cálculo de alto nivel".
- Verificación: 847/847 pruebas; `Test-SchemaStructure` 17/34; harness fuente
  2 × 34/34; smoke WPF `win-x64` PASS con `TraceVerified` en ambas fases (19
  fórmulas, 3 aristas). Sin datos factuales nuevos.

## Skills como modificador de cálculo — implementado (2026-09-19)

- Axioma acotado del propietario (`EVD-0046`, `SKL-CLM-010`) para los valores de
  efecto publicados por Fanz; sin `buffRef` ni catálogo/UI.
- Siete fórmulas `PUBLISHED` `VERIFIED` `2.1.0`: Impale (`15 + trunc(STR/35)`),
  Twisting Slash (`15 + trunc(STR/40)`), Death Stab (`70 + trunc(STR/150)`),
  Rageful Blow (`60 + trunc(STR/150)`), Swell Life
  (`12 + trunc(ENE/20) + trunc(VIT/100)`, un solo truncamiento del total),
  Penetration (`70 + trunc(AGI/200)`) y Multi-Shot (`40 + trunc(AGI/200)`).
  Strike of Destruction excluida (sin Skill DMG publicado).
- 28 casos válidos + 15 controles negativos en
  `reference-cases/formulas/{valid,invalid}`; allow-list y conteos actualizados
  en `FormulaApplicationIntegrationTests`, `SchemaContractValidatorTests` y el
  smoke PS1.
- Verificación PASS: build Release 0/0; 840/840 tests (40/58/715/27);
  `Test-SchemaStructure` 16/32; smoke WPF `win-x64` (1369 archivos, 150.618.678
  bytes, 946 archivos del ruleset, 456 casos, dataset `2026-09-17.1`).

## Skills: alineación con la regla solo-modificador — 2026-09-19

- `docs/DECISIONES-PRODUCTO.md` (regla inviolable) exige que la skill sea sólo
  modificador de cálculo (dmg/buff) y prohíbe el catálogo/UI en el cliente y las
  pruebas del catálogo de skills.
- Se retiró la vertical previa de consumo de catálogo: la sección de skills de
  `MainWindow`, `SkillApplicationIntegrationTests.cs`, la verificación de
  catálogo en el smoke y los fixtures `reference-cases/skills/{valid,invalid}`.
- Se conserva el backend mínimo (`Domain.Skills` y `Application.Skills`) sin uso
  desde la app; los datos `skills/*.json` y su registro en
  `SchemaContractValidator` no cambian.
- Diseño en `docs/04-domain/skills-consumption-design.md`, reorientado a
  modificador de cálculo.

## Catálogo acotado de skills — materializado (2026-09-17)

- Ocho `SkillDefinition` `PUBLISHED` `VERIFIED`: `skill-impale`,
  `skill-twisting-slash`, `skill-swell-life`, `skill-death-stab`,
  `skill-rageful-blow`, `skill-strike-of-destruction` (Dark Knight) y
  `skill-penetration`, `skill-multi-shot` (Fairy Elf).
- `kind` del mapeo aprobado (ATK/Non-ATK/Debuff→ACTIVE, Buff→BUFF);
  `requiredLevel` = `Character Level` publicado; `allowedEvolutionIds` = las
  tres evoluciones de la familia; `prerequisiteSkillIds` vacío; `buffRef`
  omitido; `evidenceRefs` `evd-0041`–`evd-0045` (minúsculas en JSON);
  `conflictIds` `dsp-0008`–`dsp-0011`.
- `SchemaContractValidator.ValidateRulesetRecords` registra `("skill","skills")`;
  inventario canónico 119 → 127; el smoke WPF exige ahora el directorio `skills`.
- Axioma y límites en `RES-0004` y `docs/04-domain/skills-factual-gate-design.md`.
- Verificación PASS: build Release 0/0; 797/797 tests; `Test-SchemaStructure`
  16/32; smoke WPF `win-x64` (SQLite `3.53.3`, 1319 archivos, 150.442.137
  bytes, 896 archivos del ruleset, dataset `2026-09-17.1`).

## Gate factual de skills y buffs — resuelto (2026-09-17)

El gate se cerró por axioma acotado del propietario, sin inventar datos:

- `RES-0004-skills-buffs` queda `VERIFIED` con nueve claims y las evidencias
  `EVD-0041`–`EVD-0045`; `DSP-0008`, `DSP-0009`, `DSP-0010` y `DSP-0011` quedan
  `RESOLVED` por `OWNER_DECISION`.
- Axioma: ocho skills con `Character Level` publicado (Impale, Twisting Slash,
  Swell Life, Death Stab, Rageful Blow y Strike of Destruction de Dark Knight;
  Penetration y Multi-Shot de Fairy Elf) con `requiredLevel` = `Character Level`,
  mapeo `kind` (ATK/Non-ATK/Debuff→ACTIVE, Buff→BUFF, Summon→SUMMON),
  `allowedEvolutionIds` = las tres evoluciones de cada familia,
  `prerequisiteSkillIds` vacío y `buffRef` omitido.
- Excluidos: prerrequisitos por stat/quest/equipo, skills nivel ≥400 y sistemas
  post-S4, categorías `WIZ`/`Curse`, `PASSIVE` y buffs con valores incompletos.
- `docs/04-domain/skills-factual-gate-design.md` queda `CLOSED`; no se añadieron
  datos, fixtures, constantes ni código.

## Consumo acotado del catálogo de ítems — implementado (2026-09-16)

- `ItemDefinition` en Domain y `ItemCatalog`/`JsonItemCatalogSnapshotReader` en
  Application leen `items/*.json` (schema `1.0.0`, `PUBLISHED`, un ruleset).
- `EquipItemUseCase` valida elegibilidad por clase y `requiredStats` en +0 con
  códigos estables `item-equip-not-found`, `item-equip-class-not-allowed` y
  `item-equip-requirements-not-met`.
- Reference cases `reference-cases/items/{valid,invalid}` y
  `ItemApplicationIntegrationTests` (12 pruebas) reproducen 4 casos aprobados y
  4 rechazos y fallan en cerrado ante ítem no publicado, ruleset mixto y
  directorio ausente.
- WPF añade el selector de ranura/ítem y el resultado de elegibilidad; el smoke
  exige catálogo de 3 ítems y una evaluación de equipado.
- Diseño en `docs/04-domain/items-consumption-design.md`.

## Catálogo acotado de ítems — materializado (2026-09-16)

- Tres `ItemDefinition` `PUBLISHED` `VERIFIED`: `item-kris`, `item-dragon-armor`
  y `item-albatross-bow`.
- `slots` (`weapon`/`armor`), `allowedClassIds` del mapeo aprobado,
  `requiredStats` en +0, `maxItemLevel` 15, `optionModules` NORMAL,
  `socketSlots` 0, `evidenceRefs` `evd-0037`–`evd-0040`.
- `SchemaContractValidator.ValidateRulesetRecords` incluye `("item","items")`;
  inventario canónico 116 → 119.
- Axioma y límites en `RES-0003` y `docs/04-domain/items-factual-gate-design.md`.
- Verificación PASS: build Release 0/0, 785/785 tests, `Test-SchemaStructure`
  16/32 y smoke WPF `win-x64` (SQLite `3.53.3`, 1303 archivos, 150.400.240 bytes,
  880 JSON del ruleset, dataset `2026-09-16.1`).

## Gate factual de ítems — resuelto (2026-09-16)

El gate se cerró por axioma acotado del propietario, sin inventar datos:

- `RES-0003-items-equipment` queda `VERIFIED` con ocho claims y las evidencias
  `EVD-0035`–`EVD-0040`; `DSP-0005`, `DSP-0006` y `DSP-0007` quedan `RESOLVED`
  por `OWNER_DECISION`.
- Axioma: tres ítems de grado normal (Kris, Dragon Armor, Albatross Bow) con
  `displayName`, `slots`, `allowedClassIds`, `requiredStats` en +0,
  `maxItemLevel` 15, `optionModules` NORMAL y `socketSlots` 0.
- Mapeo aprobado: `All Classes` → las seis familias, `DK` →
  `class-dark-knight`, `MG` → `class-magic-gladiator`, `ME` →
  `class-fairy-elf`.
- `docs/04-domain/items-factual-gate-design.md` queda `CLOSED`; no se añadieron
  datos, fixtures, constantes ni código.

## Última tarea cerrada — listado de builds guardadas y carga desde la lista

El incremento hizo descubrible la build persistida y la cargó desde la lista,
sin nuevos datos factuales:

- Application añade `CharacterBuildSummary` y `ListBuildsUseCase`, y amplía
  `IBuildRepository` con `ListAsync` (orden ordinal de `Id` como autoridad).
- Data implementa `SqliteBuildRepository.ListAsync` con
  `SELECT payload_json FROM builds ORDER BY id;`: proyecta el summary, no muta
  la base y no añade columnas ni migraciones.
- WPF añade un `ListBox` de builds guardadas, el botón «Cargar seleccionada» y
  un recuento de estado; refresca el listado al abrir la ventana y tras cada
  guardado, y la selección reutiliza `LoadBuildByIdAsync` →
  `LoadBuildUseCase` → `ApplyLoadedBuild` con la traducción de errores existente.
- El smoke exige que `publication-smoke-build` aparezca en el listado con
  paridad exacta y añade `BuildListVerified` y `PersistedBuildCount`.

## Verificación del cierre — listado de builds guardadas y carga desde la lista

- Restauración y build Release aprobados con 0 advertencias/0 errores; 785/785
  pruebas pasan: 40 validator, 58 motor, 660 Application y 27 Data.
- Comprobación estructural: 16 contratos/32 fixtures, sin cambios en `build`
  (`1.1.0`).
- Smoke WPF `win-x64`: PASS local el 2026-09-16 con SQLite `3.53.3`, 1300
  archivos, 150.397.632 bytes, 10 avisos legales, 877 JSON del ruleset,
  `Saved builds listed: 1` y dataset `2026-07-30.3` con hash
  `sha256:ef6fd756c2a69245906019d4c4cf01c3a7baba460067bbfffc4c4906361b0f18`.

## Última tarea cerrada — reaplicación de build en la Calculadora

La vertical devolvió la build persistida al formulario de la Calculadora sin
nuevos datos factuales:

- `CharacterBuild` avanza a `schemaVersion "1.1.0"` con `pointsPerReset`, que
  `SaveBuildUseCase` toma de `draft.ResetInputs.PointsPerReset`;
  `LoadBuildUseCase` no cambia sus validaciones. `build.schema.json` pasa a
  `1.1.0` con `pointsPerReset` requerido y no negativo; sin columnas SQLite
  nuevas.
- WPF `ApplyLoadedBuild` selecciona clase y evolución, nivel y estado de héroe,
  restaura resets y puntos por reset, deriva las asignaciones como
  `stat final − base`, recalcula presupuesto y distribución y evalúa atributos
  derivados; `LoadBuildButtonClick` lo invoca y traduce errores.
- Application añade una prueba de reproducibilidad (asignaciones derivadas,
  `ResetPoints 200`, `SpentPoints 7`, `Total = Spent + Remaining`) y fija la
  paridad de `pointsPerReset`; Data conserva payload y metadata exactos.
- El smoke verifica la paridad de resets y la reproducción de la distribución
  sintética de `publication-smoke-build`.

## Verificación del cierre — reaplicación de build en la Calculadora

- Restauración y build Release aprobados con 0 advertencias/0 errores; 780/780
  pruebas pasan: 40 validator, 58 motor, 658 Application y 24 Data.
- Comprobación estructural: 16 contratos/32 fixtures, incluido `build` `1.1.0`.
- CLI del validador: las ciento siete fórmulas `PUBLISHED` pasan sin errores
  (los datos no cambian respecto al dataset `2026-07-30.3`).
- Smoke WPF `win-x64`: PASS local el 2026-09-16 con SQLite `3.53.3`, 1300
  archivos, 150.384.676 bytes, 10 avisos legales, 877 JSON del ruleset y
  dataset `2026-07-30.3` con hash
  `sha256:ef6fd756c2a69245906019d4c4cf01c3a7baba460067bbfffc4c4906361b0f18`.

## Primera acción concreta

Confirmar con el mantenedor si la siguiente vertical es master buys/pantallas
restantes del flujo (con gate factual) o trazas de cálculo de alto nivel, y
actualizar esta documentación y `CHANGELOG.md` al cerrarla.

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

1. Builds completas y flujos de UI posteriores al presupuesto ganado, los
   borradores locales, la evaluación en lote y la persistencia local: master
   buys y pantallas restantes del flujo.
2. Trazas de cálculo de alto nivel aún sin contrato propio si el motor lo
   exige en una vertical posterior.

## Última tarea cerrada — persistencia de build completa local

La vertical de persistencia de la build completa quedó cerrada sobre la capa de
datos local sin nuevos datos factuales:

- `SaveBuildUseCase` promueve un borrador a `CharacterBuild` (schema `1.0.0`)
  con clase, evolución, nivel, stats finales (base + asignación), quests y
  resets, y `LoadBuildUseCase` recarga y revalida contra el contexto exacto
  (ruleset, dataset y motor) con códigos estables para ausencia, schema no
  soportado, dependencia indisponible, identidad incoherente, evolución no
  ofrecida y stats no alcanzables o ajenos a la clase; entrega copias
  defensivas de stats y quest ids.
- Data implementa `SqliteBuildRepository` con la migración 2 `create_builds`,
  payload y metadata exactos, reemplazo atómico por ID y rollback ante fallo
  intermedio; la contención de escritura usa el código estable `WriteConflict`.
- El smoke verifica el round-trip del borrador sintético
  `publication-smoke-draft` a la build `publication-smoke-build` (cinco stats,
  resets `2 × 100 = 200`) y su supervivencia al respaldo/restauración y al
  reemplazo simulado de binarios. Nuevos campos `BuildPersistenceVerified`,
  `BuildId` y `BuildStatCount`.
- Doce pruebas de integración de Application y seis de Data cierran la
  vertical; ruleset `1.0.0`, motor `0.2.0` y dataset `2026-07-30.3` permanecen
  sin cambios.

## Verificación del cierre — persistencia de build completa local

- Restauración y build Release aprobados con 0 advertencias/0 errores; 779/779
  pruebas pasan: 40 validator, 58 motor, 657 Application y 24 Data.
- CLI del validador: las ciento siete fórmulas `PUBLISHED` pasan sin errores
  (los datos no cambian respecto al dataset `2026-07-30.3`).
- Smoke WPF `win-x64`: PASS local el 2026-09-15 con SQLite `3.53.3`, 1300
  archivos, 150.382.040 bytes, 10 avisos legales, 877 JSON del ruleset y
  dataset `2026-07-30.3` con hash
  `sha256:ef6fd756c2a69245906019d4c4cf01c3a7baba460067bbfffc4c4906361b0f18`.

## Última tarea cerrada — evaluación de build en lote

La vertical de evaluación de build en lote quedó cerrada sobre el motor
existente sin nuevos datos factuales:

- `CalculateCharacterBuildUseCase` evalúa en una sola pasada todas las fórmulas
  publicadas aplicables a la clase y evolución del estado validado, con caché
  compartida, orden determinista por referencia y versión, detección de ciclos
  y trazas anidadas de contexto y dependencia por fórmula.
- Sin fórmula aplicable lanza el código nuevo
  `formula-context-no-applicable-formula`; la resolución del estado (nivel,
  evolución, puntos y distribución) ocurre una sola vez y WPF agrega "Evaluar
  atributos derivados" con resultados agrupados por salida derivada.
- El smoke verifica el lote sobre el personaje sintético (cinco stats y 201
  gastados, resets `2 × 100 = 200`) con paridad por fórmula contra el camino
  individual: 19 fórmulas agrupadas. Nuevos campos
  `PublishedBuildEvaluationVerified` y `PublishedBuildFormulaCount`.
- Seis pruebas de integración de Application cierran la vertical; ruleset
  `1.0.0`, motor `0.2.0` y dataset `2026-07-30.3` permanecen sin cambios.

## Verificación del cierre — evaluación de build en lote

- Restauración y build Release aprobados con 0 advertencias/0 errores; 761/761
  pruebas pasan: 40 validator, 58 motor, 645 Application y 18 Data.
- CLI del validador: las ciento siete fórmulas `PUBLISHED` pasan sin errores
  (los datos no cambian respecto al dataset `2026-07-30.3`).
- Smoke WPF `win-x64`: PASS local el 2026-09-11 con SQLite `3.53.3`, 1300
  archivos, 150.038.168 bytes, 10 avisos legales, 877 JSON del ruleset y
  dataset `2026-07-30.3` con hash
  `sha256:ef6fd756c2a69245906019d4c4cf01c3a7baba460067bbfffc4c4906361b0f18`.

## Última tarea cerrada — Speed, Combo, Skills, Wizardry y Guild

La vertical del catálogo restante de `EVD-0026` quedó cerrada como axiomas del
ruleset trazándose exclusivamente desde `EVD-0021` y `EVD-0026`:

- Veinticinco fórmulas `1.0.0` `PUBLISHED` contra schema `2.1.0`: seis `speed`
  (`agi/15`, `agi/10`, `agi/50`, `agi/15`, `agi/10` y `agi/20` por familia,
  unidad nueva `speed-point`), `combo-base-dark-knight` (`(str + agi + ene)/2`,
  único control de overflow), `skill-percent` de Dark Knight (`200 + ene/10`)
  y Dark Lord (`200 + ene/20`), `fortitude-percent` (`12 + vit/100 + ene/20`),
  `soul-barrier-percent` (`10 + agi/50 + ene/200`), `damage-buff` (`3 + ene/7`),
  `defense-buff` (`2 + ene/8`), `heal` (`2 + ene/9`),
  `fenrir-base-{min,max}-damage` de Dark Knight (`45/75 + str/3 + agi/5 +
  vit/5 + ene/6`) y Dark Wizard (`60/90 + str/5 + agi/5 + vit/7 + ene/3`),
  `min/max-wizardry-dark-wizard` (`ene/9` y `ene/4`),
  `nova-max-spell-damage` (`1320 + str/2`), `critical-damage`
  (`cmd/25 + str/30`), `fireburst-bonus-{min,max}-damage`
  (`100/150 + str/25 + ene/50`) y `guild-member-capacity`
  (`lvl/10 + cmd/10`, unidad nueva `member-count`, única con entrada de nivel).
- Cada programa divide y trunca hacia cero una sola vez en el paso visible con
  aritmética decimal comprobada. Cien positivos (cuatro por fórmula) y setenta
  y un controles negativos (44 stat fuera de base + 25 familia ajena + 1 nivel
  inválido + 1 overflow). Sólo `formula-speed-magic-gladiator` hereda
  `conflictIds: dsp-0002`.
- Application y WPF materializan ciento siete fórmulas ejecutables; el dataset
  avanza a `2026-07-30.3` con hash
  `sha256:ef6fd756c2a69245906019d4c4cf01c3a7baba460067bbfffc4c4906361b0f18`.

## Verificación del cierre — Speed, Combo, Skills, Wizardry y Guild

- Restauración y build Release aprobados con 0 advertencias/0 errores;
  755/755 pruebas pasan: 40 validator, 58 motor, 639 Application y 18 Data.
- CLI del validador: las ciento siete fórmulas `PUBLISHED` pasan sin errores;
  el gate factual cubre 432 positivos y 319 controles negativos.
- Smoke WPF `win-x64` (107 fórmulas y 428 casos contextuales): PASS local el
  2026-09-11 con SQLite `3.53.3`, 1300 archivos, 150.022.448 bytes, 877 JSON
  del ruleset y hash
  `sha256:ef6fd756c2a69245906019d4c4cf01c3a7baba460067bbfffc4c4906361b0f18`.

## Última tarea cerrada — rates, regeneración y daño restante

La vertical de Rates, Regeneración y Daño restante quedó cerrada como axiomas
del ruleset trazándose exclusivamente desde `EVD-0021` y `EVD-0026`:

- Cuarenta fórmulas `1.0.0` `PUBLISHED` contra schema `2.1.0`: veinticuatro
  rates (`pvm-attack-rate`, `pvm-defense-rate`, `pvp-attack-rate` y
  `pvp-defense-rate` por familia), diez regeneraciones (`mana-regen` y
  `ag-regen` de Dark Wizard, Dark Knight, Fairy Elf, Magic Gladiator y Dark
  Lord) y seis daños físicos restantes (`min-damage`/`max-damage` de Dark
  Knight, Fairy Elf y Dark Lord). Son exteriores al inventario de 24 claims de
  `RES-0002`.
- Dark Lord sustituye `strength / 4` por `strength / 6 + command / 10` en
  `pvm-attack-rate` (único rate que consume Command, resuelto con adición de
  cuatro operandos). Las regeneraciones consumen la salida `RAW` de
  `formula-mana-{familia}`/`formula-ag-{familia}` por la misma ruta que
  Defense→SD.
- Cada programa divide y trunca hacia cero una sola vez en el paso visible con
  aritmética decimal comprobada. Ciento sesenta positivos (cuatro por fórmula:
  base, fracción, entero y evolución superior) y ochenta controles negativos.
- Application y WPF materializan ochenta y dos fórmulas ejecutables; el dataset
  avanza a `2026-07-30.2` con hash
  `sha256:08bd49ab45892995c86a7f0b40f1186e71d389f8215fca66530ff10efc9ee2b9`.

## Verificación del cierre — rates, regeneración y daño restante

- Restauración y build Release aprobados con 0 advertencias/0 errores;
  584/584 pruebas pasan: 40 validator, 58 motor, 468 Application y 18 Data.
- CLI del validador: las ochenta y dos fórmulas `PUBLISHED` pasan sin errores;
  el gate factual cubre 332 positivos y 248 controles negativos.
- Smoke WPF `win-x64` (82 fórmulas y 328 casos contextuales): PASS local el
  2026-09-11 con SQLite `3.53.3`, 1104 archivos, 149.737.806 bytes, 681 JSON
  del ruleset y hash
  `sha256:08bd49ab45892995c86a7f0b40f1186e71d389f8215fca66530ff10efc9ee2b9`.

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

Confirmar con el mantenedor el primer candidato restante (master buys y
pantallas restantes del flujo, o contratos fácticos de `EVD-0026` sin motor) y
actualizar esta documentación y `CHANGELOG.md` al cerrarlo. La traza de cálculo
de alto nivel, tercer candidato, quedó cerrada el 2026-09-19.