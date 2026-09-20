# RES-0005 — Ampliación de UC-04: bonificaciones, requiredLevel y sockets (gate factual)

## Registro

```yaml
id: RES-0005
question: "¿Existe evidencia que permita ampliar UC-04 (bonificaciones ATK/DEF por nivel de ítem, requiredLevel, progresión de requiredStats y sockets) dentro del subconjunto acotado de ítems (Kris, Dragon Armor, Albatross Bow) de Season 4 global/inglesa?"
scope:
  season: "4"
  class: null
  mode: "general"
  ruleset: "mu-s4-global-reference"
status: VERIFIED
claims:
  - id: ITM-CLM-009
    statement: "Las páginas de ítem de Fanz re-capturadas el 2026-09-20 conservan los valores de nivel +0 de EVD-0037–EVD-0039 (Kris 1-H ATK DMG +6 ~ 11, ATK Speed +50, STR 27 / AGI 27, All Classes; Dragon Armor DEF +37, STR 232 / AGI 73, DK y MG; Albatross Bow 1-H ATK DMG +155 ~ 177, ATK Speed +45, STR 218 / AGI 894, ME) y no exponen valores por nivel +1..+15 en el HTML estático; el selector de nivel no demuestra que el sitio publique progresión numérica verificable."
    status: VERIFIED
    evidence: [EVD-0047, EVD-0048, EVD-0049]
  - id: ITM-CLM-010
    statement: "Fanz no publica un nivel de personaje requerido (requiredLevel) para Kris, Dragon Armor ni Albatross Bow; el campo permanece fuera del axioma EVD-0040."
    status: VERIFIED
    evidence: [EVD-0047, EVD-0048, EVD-0049]
  - id: ITM-CLM-011
    statement: "La única regla numérica genérica de nivel de ítem localizada en fuentes autorizadas es Webzen '+5% de defensa final por upgrade' (armadura, juego actual, sin Season 4 declarada) y '+4 hasta 16' para opciones de daño/defensa; StrategyWiki sólo declara, de forma cualitativa, que un mayor nivel implica mayor daño/defensa y mayores requisitos de STR/AGI. Ninguna fuente publica una progresión numérica por +1..+15 para los tres ítems del subconjunto."
    status: VERIFIED
    evidence: [EVD-0050, EVD-0051, EVD-0052, EVD-0053]
  - id: ITM-CLM-012
    statement: "La progresión de requiredStats por nivel de ítem no está publicada por ninguna fuente contrastada; sólo existe la regla por opción Jewel of Life (+5 STR por nivel de opción, EVD-0036), adoptada como axioma parcial en EVD-0053 y que no aplica al nivel de ítem."
    status: VERIFIED
    evidence: [EVD-0047, EVD-0048, EVD-0049, EVD-0051, EVD-0052, EVD-0053]
  - id: ITM-CLM-013
    statement: "Los sockets no aplican al subconjunto (grado normal, socketSlots 0, según EVD-0040); la categoría `Socket Items` de Fanz es un sistema aparte y posterior sin evidencia de pertenecer al subconjunto o a Season 4."
    status: VERIFIED
    evidence: [EVD-0047, EVD-0048, EVD-0049]
  - id: ITM-CLM-014
    statement: "Por decisión del propietario del 2026-09-20 (EVD-0053) se adoptan como axiomas parciales dentro de UC-04 y del subconjunto acotado: (a) la regla Webzen de armadura '+5% de defensa final por cada upgrade de nivel de ítem' (EVD-0050) y (b) la regla de opción Jewel of Life '+5 STR por nivel de opción' (EVD-0036). Quedan excluidos requiredLevel, los valores de ATK por nivel de ítem de armas (sin fuente numérica), la progresión de requiredStats por nivel de ítem y los sockets."
    status: VERIFIED
    evidence: [EVD-0053]
conflicts:
  - id: DSP-0012
    statement: "La ampliación de UC-04 con bonificaciones ATK/DEF por nivel de ítem, requiredLevel y progresión de requiredStats carece de fuente Season 4 con valores numéricos publicados; las reglas genéricas localizadas (Webzen +5% de defensa por upgrade; JOL +5 STR por opción) tienen alcance del juego actual o de opciones, no del nivel de ítem."
    evidence: [EVD-0047, EVD-0048, EVD-0049, EVD-0050, EVD-0051, EVD-0052]
    scope: "Ampliación de `item.schema.json` y de los tres `ItemDefinition` del subconjunto acotado con bonificaciones, requiredLevel, progresión de requiredStats y sockets."
    impact: "Bloqueaba la ampliación de UC-04 hasta una decisión del propietario; la regla de opciones JOL de EVD-0036 no cubre el nivel de ítem."
    status: RESOLVED
    resolution: OWNER_DECISION
test_plan: "La ampliación de UC-04 se limita al axioma parcial de EVD-0053: la regla Webzen de armadura (+5% de defensa final por nivel de ítem) y la regla JOL (+5 STR por nivel de opción) como reglas del ruleset. No se materializan requiredLevel, valores de ATK por nivel de armas, progresión de requiredStats por nivel ni sockets; requiredStats permanece en +0, optionModules NORMAL y socketSlots 0 (RES-0003). La semántica exacta de '+5% de defensa final' (base de cómputo, acarreo y redondeo) se fija en el diseño de la vertical antes de implementar."
conclusion: "El gate de la ampliación de UC-04 queda resuelto por decisión del propietario del 2026-09-20 (EVD-0053, axio-axioma parcial); DSP-0012 pasa a RESOLVED por OWNER_DECISION. La re-captura del 2026-09-20 confirma ITM-CLM-008: Fanz no publica progresión numérica por nivel +1..+15 ni requiredLevel, y el contraste (Webzen, StrategyWiki, foros) aporta sólo reglas genéricas. Se adoptan como axiomas la regla de armadura Webzen +5% de defensa por nivel y la regla JOL +5 STR por opción; quedan excluidos requiredLevel, valores de ATK por nivel de armas, progresión de requiredStats por nivel y sockets. Sin cambios en datos ni código."
reviewed_by: ["project-owner"]
last_reviewed_at: "2026-09-20"
```

## Alcance y límites

- Este registro investiga exclusivamente la **ampliación de UC-04** sobre el
  subconjunto acotado ya aprobado en `RES-0003` (Kris, Dragon Armor y Albatross
  Bow, grado normal). No reabre clases, stats base, iteración de ítem ni el
  consumo ya implementado.
- Objetivo preguntado: ¿existen datos factuales de Season 4 para
  `requiredLevel`, bonificaciones ATK/DEF por nivel de ítem, progresión de
  `requiredStats` por nivel y sockets, dentro del subconjunto?
- Cada claim es independiente y pequeña. Un hallazgo de una fuente no autoriza
  reutilizar valores en otra.
- Fanz es la fuente inicial prioritaria por decisión del propietario del
  2026-07-18; las fuentes adicionales autorizadas del 2026-07-19 se usan para
  contraste y segunda línea, sin sustituir el control de versión
  (`docs/05-research/source-policy.md`).
- El estado `VERIFIED` proviene del axio-axioma parcial del propietario
  (`EVD-0053`); no implica que las reglas genéricas demuestren Season 4, ni que
  existan valores por ítem fuera del alcance adoptado, ni habilita materiales
  fuera del subconjunto enumerado.

## Re-captura de muestra (2026-09-20)

| Ítem | Ranura (inferida) | Clase declarada | Detalles +0 (Fanz) | STR +0 | AGI +0 | Grado | Estado |
|---|---|---|---|---|---|---|---|
| Kris | Weapon (1-H) | All Classes | 1-H ATK DMG +6 ~ 11, ATK Speed +50 | 27 | 27 | Normal | VERIFIED (Fanz + axioma) |
| Dragon Armor | Armor | DK, MG | DEF +37 | 232 | 73 | Normal | VERIFIED (Fanz + axioma) |
| Albatross Bow | Weapon | ME | 1-H ATK DMG +155 ~ 177, ATK Speed +45 | 218 | 894 | Normal | VERIFIED (Fanz + axioma) |

La re-captura coincide con `EVD-0037`–`EVD-0039` (2026-09-16): no hay deriva en
los valores publicados de nivel +0. Los tres ítems conservan el selector de
nivel `+0..+15` sin exponer valores por nivel en el HTML estático y sin publicar
`requiredLevel`.

## Estado del gate por campo

| Campo solicitado | Hallazgo | Estado |
|---|---|---|
| `requiredLevel` | No publicado por Fanz ni por las fuentes contrastadas para ninguno de los tres ítems. | Excluido (sin fuente; no se adopta) |
| Bonificaciones ATK/DEF por nivel de ítem | Armadura: regla genérica Webzen `+5% de defensa final por upgrade`; armas: sin valores numéricos por +1..+15. | Armadura: axioma parcial (EVD-0053); armas: sin datos (no se adopta) |
| Progresión de `requiredStats` por nivel | No publicada; única regla JOL `+5 STR` por **nivel de opción** (EVD-0036), adoptada como axioma parcial. No cubre el nivel de ítem. | Sólo opción JOL (EVD-0053); por nivel de ítem: sin datos (no se adopta) |
| Sockets | El subconjunto es grado normal con `socketSlots` 0; `Socket Items` es categoría separada y posterior en Fanz. | Excluido del subconjunto (sin cambio) |

## Plan de investigación

1. La evidencia quedó registrada en `EVD-0047`–`EVD-0049` sin promover claims
   automáticamente y se contrastó en `EVD-0050`–`EVD-0052`.
2. Se verificó que ninguna fuente demuestra Season 4 con valores numéricos por
   nivel de ítem para el subconjunto; se abrió `DSP-0012` en lugar de inferir.
3. El propietario resolvió `DSP-0012` por `OWNER_DECISION` mediante un
   axio-axioma parcial (`EVD-0053`): se adoptan las reglas genéricas con fuente
   (armadura `+5%` de defensa por nivel; opción JOL `+5 STR` por nivel) y se
   descartan los campos sin datos (`requiredLevel`, ATK por nivel de armas,
   progresión de `requiredStats` por nivel, sockets).
4. La ampliación de UC-04 queda desbloqueada sólo dentro de ese alcance; la
   semántica exacta de la regla de armadura se fija en el diseño de la vertical.

## Evidencias capturadas

### EVD-0047 — MU Online Fanz, página del ítem Kris (re-captura 2026-09-20)

- URL canónica: https://muonlinefanz.com/tools/items/data/itemdb/Kris.php
- Título/editor: `Kris`, MU Online Fanz. Página marcada `b2023.11.20.001`.
- Consulta: 2026-09-20. Versión declarada: ninguna.
- Dato extraído: `1-H ATK DMG: +6 ~ 11`, `ATK Speed: +50`; requisitos
  `Strength: 27`, `Agility: 27`; `Can be equipped by All Classes`; selector de
  nivel `+0..+15` (sin valores por nivel en HTML estático); opciones posibles
  `+Jewel of Life option*`, `+Jewel of Harmony option`, `+Luck option`;
  nota `*the item's STR requirement increases by 5, per option level!`; grado
  normal (`This item is a 'normal' grade item`); relacionado `Excellent Kris`.
- Transformación: se re-confirman requisitos y elegibilidad en +0 ya registrados
  en `EVD-0037`. No se extraen valores por nivel.
- Condición de uso: `PARTIAL`, muestra factual sin temporada demostrada.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0048 — MU Online Fanz, página del ítem Dragon Armor (re-captura 2026-09-20)

- URL canónica:
  https://muonlinefanz.com/tools/items/data/itemdb/Dragon%20Armor.php
- Título/editor: `Dragon Armor`, MU Online Fanz. Página marcada `b2023.11.20.001`.
- Consulta: 2026-09-20. Versión declarada: ninguna.
- Dato extraído: `DEF: +37`; requisitos `Strength: 232`, `Agility: 73`;
  `Can be equipped by DK, MG`; selector de nivel `+0..+15` (sin valores por
  nivel en HTML estático); opciones `+Jewel of Life option*`, `+Jewel of Harmony
  option`, `+Luck option`; nota `STR requirement increases by 5, per option
  level!`; grado normal; relacionada con el resto de la pieza `Dragon`.
- Transformación: se re-confirman requisitos y elegibilidad en +0 ya registrados
  en `EVD-0038`. No se extraen valores por nivel.
- Condición de uso: `PARTIAL`.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0049 — MU Online Fanz, página del ítem Albatross Bow (re-captura 2026-09-20)

- URL canónica:
  https://muonlinefanz.com/tools/items/data/itemdb/Albatross%20Bow.php
- Título/editor: `Albatross Bow`, MU Online Fanz. Página marcada `b2023.11.20.001`.
- Consulta: 2026-09-20. Versión declarada: ninguna.
- Dato extraído: `1-H ATK DMG: +155 ~ 177`, `ATK Speed: +45`; requisitos
  `Strength: 218`, `Agility: 894`; `Can be equipped by ME`; selector de nivel
  `+0..+15` (sin valores por nivel en HTML estático); opciones `+Jewel of Life
  option*`, `+Jewel of Harmony option`, `+Luck option`; nota `STR requirement
  increases by 5, per option level!`; grado normal; relacionado
  `Excellent Albatross Bow`.
- Transformación: se re-confirman requisitos y elegibilidad en +0 ya registrados
  en `EVD-0039`; `ME` se conserva literal (ver `DSP-0006`, resuelto por el mapeo
  de `EVD-0040`).
- Condición de uso: `PARTIAL`.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0050 — Webzen oficial, guía de upgrade de ítem (consulta 2026-09-20)

- URL canónica: https://muonline.webzen.com/en/gameinfo/guide/detail/32
- Título/editor: `Item Upgrade`, guía oficial de Webzen (sitio actual).
- Consulta: 2026-09-20. Versión declarada: ninguna (juego actual; no declara
  Season 4).
- Dato extraído: `+10 ~ +15` se mejoran en el Chaos Goblin (`+10`: 1 Jewel of
  Chaos, 1 Jewel of Bless, 1 Jewel of Soul; `+15`: 1 Chaos, 6 Bless, 6 Soul);
  cada upgrade suma `+5% final defense`; las opciones alcanzan `+4 hasta 16`
  para daño y defensa.
- Transformación: se registra como regla genérica **del juego actual** para el
  contraste del eje de armadura; no se aplica a Season 4 ni a valores por ítem.
- Condición de uso: `PARTIAL`, contraste; no demuestra la versión objetivo.
- Licencia/uso conocido: contenido oficial de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0051 — StrategyWiki, página Mu Online/Items (consulta 2026-09-20)

- URL canónica: https://strategywiki.org/wiki/Mu_Online/Items
- Título/editor: `Mu Online/Items`, StrategyWiki (publicada 2009-09-11).
- Consulta: 2026-09-20. Versión declarada: ninguna (describe servidores globales
  con niveles 0..13 y extensiones posteriores en otros servidores).
- Dato extraído: un nivel de ítem superior es más fuerte **y** exige más
  `strength` y `agility`; las opciones con Jewel of Life suben en incrementos de
  4 en armas/armaduras y de 5 en escudos, con máximo +16 (global) / +28
  (privado); en armas la opción aumenta el daño, en armaduras la defensa total
  en porcentaje y en escudos la defensa rate; Jewel of Life tiene 50% de éxito.
- Transformación: se registra como regla **cualitativa** de mecánica de niveles y
  opciones; no publica valores numéricos por ítem ni demuestra Season 4.
- Condición de uso: `PARTIAL`, contraste y huella histórica (2009, época cercana
  a Season 4 pero sin declaración de versión).
- Licencia/uso conocido: wiki colaborativa; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0052 — Contraste de foros y guías adicionales (consulta 2026-09-20)

- Fuentes: RaGEZONE (hilo `Calculation of items stats requirements and more...`,
  2017), ViciadosMU (guías de ítems y de excelentes, 2024/2026), muonline.net
  (guía de opciones Harmony).
- Consulta: 2026-09-20. Versión declarada: servidores privados/actuales; ninguna
  declara un cliente global-inglés Season 4 con valores por nivel de ítem.
- Dato extraído: la comunidad RaGEZONE afirma que existe una relación entre el
  nivel del ítem, su drop level y el daño, y que cada *option* Jewel of Life por
  nivel exige +5 STR; muonline.net tabula opciones Harmony por paso de refinado
  (sistema posterior); ViciadosMU describe tiers de refine y opciones sin
  valores por ítem.
- Transformación: sólo corrobora la regla JOL de `EVD-0036` y que la progresión
  numérica por nivel de ítem no está publicada de forma verificable.
- Condición de uso: `PARTIAL`, pistas y contraste; no prueba por sí sola.
- Licencia/uso conocido: foros y guías; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0053 — Decisión del propietario: axio-axioma parcial de UC-04

- Fuente: decisión explícita del propietario comunicada el 2026-09-20.
- Alcance declarado: `mu-s4-global-reference`, Season 4 global/inglés,
  subconjunto acotado de `RES-0003` (Kris, Dragon Armor y Albatross Bow, grado
  normal) y ampliación de UC-04.
- Reglas adoptadas como axiomas parciales del ruleset (sin inventar valores por
  ítem):
  - Armadura: regla genérica de Webzen `+5% de defensa final por cada nivel de
    upgrade` (`EVD-0050`), aplicable como progresión defensiva por nivel de
    ítem dentro del subconjunto. La semántica exacta (base, acumulación y
    redondeo) se fija en el diseño de la vertical antes de implementar.
  - Opción Jewel of Life: `+5 STR` por nivel de opción (`EVD-0036`), ya
    publicada por Fanz y corroborada por el contraste (`EVD-0051`, `EVD-0052`).
- Excluidos (sin fuente Season 4 con valores): `requiredLevel`, los valores de
  ATK por nivel de ítem de armas, la progresión de `requiredStats` por nivel de
  ítem y los sockets.
- Confianza y uso permitido: `VERIFIED` sólo para el alcance anterior; autoriza
  ampliar UC-04 según `DSP-0012` resuelto, sin modificar `item.schema.json`,
  los `ItemDefinition`, `build.schema.json` ni el `BuildEquipmentValidator`
  hasta el diseño de la vertical.
- Divergencia conservada: las reglas provienen de fuentes sin alcance Season 4
  declarado (Webzen juego actual; Fanz sin temporada); se aceptan como axioma
  para este subconjunto y no reclasifica a las fuentes como Season 4.

## Pendiente

- Decisión del propietario para `DSP-0012` resuelta por `OWNER_DECISION`
  (`EVD-0053`): axio-axioma parcial de UC-04. `requiredLevel`, ATK por nivel de
  armas, progresión de `requiredStats` por nivel y sockets quedan **excluidos**
  (keep `requiredStats` en +0, `optionModules` NORMAL, `socketSlots` 0).
- Se adoptan como reglas del ruleset dentro del subconjunto: la regla Webzen de
  armadura (`+5% de defensa final por nivel de ítem`) y la regla JOL (`+5 STR`
  por nivel de opción).
- El 2026-09-20 el propietario fijó la semántica de la regla de armadura
  (aditiva sobre la base: `DEF(n) = trunc(base × (1 + 0,05·n))`, truncamiento
  único en salida), la materialización de `defense` +0 y el aplazamiento de
  JOL hasta que `equipmentEntry` modele opciones. Diseño de la vertical:
  `docs/04-domain/items-defense-level-bonus-design.md`.
- La implementación (schema item `1.1.0`, `ItemDefinition.Defense`,
  `ItemDefenseBonusCalculator`, `EquipItemResult`/`UseCase`,
  `item-dragon-armor` `1.1.0` con `defense` 37, dataset `2026-09-20.1`) queda
  pendiente y se hará con PR a `main`.
- Los sockets permanecen fuera del subconjunto (grado normal); cualquier
  evolución requiere su propia evidencia Season 4 o decisión del propietario.