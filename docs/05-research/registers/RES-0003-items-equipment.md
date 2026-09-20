# RES-0003 — Ítems y equipo (gate factual de master buys)

## Registro

```yaml
id: RES-0003
question: "¿Existe una fuente que demuestre, para Season 4 global/inglesa, la taxonomía de ranuras, los grados/opciones y los requisitos de clase/stats de los ítems de equipo necesarios para el catálogo canónico?"
scope:
  season: "4"
  class: null
  mode: "general"
  ruleset: "mu-s4-global-reference"
status: VERIFIED
claims:
  - id: ITM-CLM-001
    statement: "El catálogo de Fanz declara las ranuras equipables de combate (Helm, Armor, Pants, Gloves, Boots, Weapon, Off-hand, dos anillos, Necklace, dos Earrings, Pentagram, Wings/Cape, Pet, Mounts, Muun x2 y Guardian)."
    status: VERIFIED
    evidence: [EVD-0036, EVD-0040]
  - id: ITM-CLM-002
    statement: "La mayoría de los ítems de combate admiten nivel de ítem de +0 a +15 y el nivel de ítem aumenta los requisitos del ítem."
    status: VERIFIED
    evidence: [EVD-0036, EVD-0040]
  - id: ITM-CLM-003
    statement: "Fanz clasifica los ítems por grado Normal, Excellent, Ancient, Socket, Mastery y Enhanced, y describe las opciones Luck, Jewel of Life y Jewel of Harmony."
    status: VERIFIED
    evidence: [EVD-0036, EVD-0040]
  - id: ITM-CLM-004
    statement: "Fanz afirma que todo ítem de equipo tiene restricciones de clase y que los stats base del personaje deben cumplir los requisitos para equiparlo."
    status: VERIFIED
    evidence: [EVD-0036, EVD-0040]
  - id: ITM-CLM-005
    statement: "Kris (espada de una mano, grado normal) se publica como equipable por todas las clases y con requisitos STR 27 / AGI 27 en nivel +0."
    status: VERIFIED
    evidence: [EVD-0037, EVD-0040]
  - id: ITM-CLM-006
    statement: "Dragon Armor (grado normal) se publica como equipable por DK y MG y con requisitos STR 232 / AGI 73 en nivel +0."
    status: VERIFIED
    evidence: [EVD-0038, EVD-0040]
  - id: ITM-CLM-007
    statement: "Albatross Bow (grado normal) se publica como equipable por `ME` y con requisitos STR 218 / AGI 894 en nivel +0."
    status: VERIFIED
    evidence: [EVD-0039, EVD-0040]
  - id: ITM-CLM-008
    statement: "Las páginas de ítem de Fanz exponen los requisitos por nivel sólo como texto del nivel +0 seleccionado y no publican la progresión numérica de daño/defensa/requisitos por cada nivel +1..+15."
    status: VERIFIED
    evidence: [EVD-0037, EVD-0038, EVD-0039, EVD-0040]
conflicts:
  - id: DSP-0005
    statement: "El catálogo de ítems y la guía de combate de Fanz no declaran Season 4 y mezclan sistemas y clases posteriores (Muun, Earrings, Pentagram, Guardian Mounts, Mastery/Enhanced y cuarta clase nivel 800), mientras las páginas de ítem no distinguen temporada."
    evidence: [EVD-0035, EVD-0036, EVD-0037, EVD-0038, EVD-0039]
    scope: "Aplicabilidad a Season 4 global/inglesa del catálogo de ítems y de la taxonomía de ranuras, grados y opciones."
    impact: "Bloqueaba publicar cualquier `ItemDefinition` como dato factual de Season 4; la coincidencia entre páginas actuales no demuestra por sí sola la versión histórica."
    status: RESOLVED
    resolution: OWNER_DECISION
  - id: DSP-0006
    statement: "Las páginas de ítem de Fanz identifican clases con códigos mixtos (`All Classes`, `DK`, `MG`, `ME`) que no se corresponden uno a uno con las seis familias del ruleset; `ME` designa una evolución (Muse Elf) y no se declara si el ítem aplica a todas las evoluciones de la familia."
    evidence: [EVD-0037, EVD-0038, EVD-0039]
    scope: "Mapeo de la elegibilidad de clase de cada ítem a `allowedClassIds` de `item.schema.json`."
    impact: "Impedía derivar `allowedClassIds` sin una decisión explícita de mapeo clase-familia/evolución."
    status: RESOLVED
    resolution: OWNER_DECISION
  - id: DSP-0007
    statement: "Fanz fija los requisitos en nivel +0, pero EVD-0036 declara que el nivel de ítem y la opción Jewel of Life aumentan los requisitos (STR +5 por nivel de opción) sin publicar la fórmula por nivel de ítem."
    evidence: [EVD-0036, EVD-0037, EVD-0038, EVD-0039]
    scope: "Progresión de `requiredStats` según nivel de ítem y opciones."
    impact: "`requiredStats` sólo puede registrarse como valor de nivel +0; la progresión por nivel y opciones queda fuera del alcance."
    status: RESOLVED
    resolution: OWNER_DECISION
test_plan: "Materializar el catálogo canónico de los tres ítems del axioma (Kris, Dragon Armor, Albatross Bow) contra `item.schema.json`, enlazando `EVD-0037`–`EVD-0040`. Cada ítem es un registro `PUBLISHED` independiente con `displayName`, `slots`, `allowedClassIds`, `requiredStats` en +0, `maxItemLevel` 15, `optionModules` NORMAL y `socketSlots` 0. Quedan prohibidos por este registro `requiredLevel`, la progresión de `requiredStats`, los sockets y cualquier ítem o grado fuera del axioma; ampliarlos exige nueva evidencia o una nueva decisión del propietario."
conclusion: "RES-0003 queda resuelto por decisión del propietario del 2026-09-16 (`EVD-0040`). Fanz no demuestra Season 4, por lo que el propietario acepta como axioma del ruleset un subconjunto acotado y estable (Kris, Dragon Armor y Albatross Bow, grado normal) con los valores publicados, el mapeo clase-familia/evolución y el límite de campos. Los ocho claims pasan a `VERIFIED` sólo dentro de ese alcance; `DSP-0005`, `DSP-0006` y `DSP-0007` quedan `RESOLVED` por `OWNER_DECISION`. El catálogo de ítems queda desbloqueado para el subconjunto acotado y sigue bloqueado para cualquier otro ítem, campo o grado."
reviewed_by: ["project-owner"]
last_reviewed_at: "2026-09-16"
```

## Alcance y límites

- La versión objetivo es Season 4 global/inglesa (`mu-s4-global-reference`); este
  registro no reabre las clases, evoluciones ni stats base de `RES-0001`.
- Se investiga exclusivamente el eje de **ítems/equipo** que alimentará
  `ItemDefinition` (`packages/schemas/v1/item.schema.json`). Quedan fuera daño,
  defensa, rates, PvM/PvP, precios y drops.
- Cada ítem, campo y valor es un claim independiente. Una coincidencia entre
  ítems o fuentes no autoriza reutilizar requisitos por inferencia.
- La clasificación `VERIFIED` proviene del axioma acotado del propietario
  (`EVD-0040`); no implica que Fanz demuestre Season 4 ni habilita datos fuera
  del subconjunto enumerado.
- Fanz es la fuente inicial prioritaria por decisión del propietario del
  2026-07-18; la prioridad no sustituye el control de versión ni el contraste
  (`docs/05-research/source-policy.md`).

## Matriz inicial de muestra

| Ítem | Ranura (inferida) | Clase declarada | STR +0 | AGI +0 | Grado | Estado |
|---|---|---|---|---|---|---|
| Kris | Weapon (1-H) | All Classes | 27 | 27 | Normal | VERIFIED (axioma) |
| Dragon Armor | Armor | DK, MG | 232 | 73 | Normal | VERIFIED (axioma) |
| Albatross Bow | Weapon | ME | 218 | 894 | Normal | VERIFIED (axioma) |

La ranura se registra sólo dentro del axioma del propietario (`EVD-0040`) y no se
extiende a otros ítems; `item.schema.json` exige `slots` como campo requerido y
su materialización se limita al subconjunto acotado.

## Alcance del axioma del propietario

Decisión del propietario del 2026-09-16 (`EVD-0040`) para desbloquear el
catálogo sin inventar datos:

- Subconjunto aceptado: exactamente los tres ítems de grado normal con evidencia
  capturada (`item-kris`, `item-dragon-armor`, `item-albatross-bow`).
- Campos cubiertos: `displayName`, `slots`, `allowedClassIds`, `requiredStats`
  en nivel +0, `maxItemLevel` 15, `optionModules` NORMAL y `socketSlots` 0.
- Mapeo clase-familia/evolución aprobado: `All Classes` → las seis familias;
  `DK` → `class-dark-knight`; `MG` → `class-magic-gladiator`; `ME` →
  `class-fairy-elf`.
- Fuera del axioma: `requiredLevel`, progresión de `requiredStats`, sockets y
  cualquier otro ítem, ranura, campo o grado.
- La divergencia de versión se conserva: estos valores no demuestran Season 4 por
  sí mismos; se aceptan como axioma sólo para este subconjunto y alcance.

## Plan de investigación

1. La evidencia disponible quedó registrada en `EVD-0035`–`EVD-0039` sin promover
   claims automáticamente.
2. Se documentó que ninguna fuente demuestra Season 4 y se abrieron `DSP-0005` a
   `DSP-0007` en lugar de inferir.
3. El propietario resolvió los tres conflictos por `OWNER_DECISION` mediante un
   axioma acotado (`EVD-0040`).
4. El catálogo puede materializarse para el subconjunto acotado; ampliarlo exige
   nueva evidencia o una nueva decisión explícita.

## Evidencias capturadas

### EVD-0035 — MU Online Fanz, índice de la base de datos de ítems

- URL canónica: https://muonlinefanz.com/tools/items/
- Título/editor: `Item Database`, MU Online Fanz.
- Consulta: 2026-09-16. La página declara actualización del 2025-05-08 y build
  `b2023.09.11.001`.
- Versión declarada: ninguna. El encabezado enumera clases posteriores a Season 4
  (Slayer, Gun Crusher, White Wizard, Mage, Rune Mage, Grow Lancer, Rage Fighter)
  y múltiples sistemas de ítems posteriores, por lo que no demuestra Season 4.
- Dato extraído: el índice enumera `3090 items` y expone categorías como Combat
  Items, Excellent, Ancient, Socket, Mastery, Enhanced, Muun y Guardian Mounts.
- Transformación: se conserva sólo como caracterización de la fuente y su
  inventario; no se extrae ningún valor numérico de este índice.
- Condición de uso: `PARTIAL`, sólo investigación y contraste.
- Licencia/uso conocido: fan site operado con permiso de Webzen; se conserva URL
  y paráfrasis, sin copiar imágenes ni capturar snapshot.
- Snapshot/hash: no capturado.

### EVD-0036 — MU Online Fanz, guía de ítems de combate

- URL canónica: https://muonlinefanz.com/guide/items/combat-items/
- Título/editor: `Combat Items`, MU Online Fanz.
- Consulta: 2026-09-16. Actualización declarada 2025-05-08, build
  `b2023.09.11.001`.
- Versión declarada: ninguna. La guía incluye Pentagram (nivel 100), Earrings
  (nivel 300), Mastery (nivel 400) y Enhanced/Guardian (nivel 800), sistemas
  posteriores a Season 4; además afirma que todos los ítems de equipo tienen
  restricciones de clase.
- Dato extraído:
  - Ranuras equipables: Helm, Armor, Pants, Gloves, Boots (x1 cada una),
    Weapon (x1), Off-hand (x1), Rings (x2), Necklace (x1), Earrings (x2),
    Pentagram (x1), Wings/Cape (x1), Pet (x1), Mounts y Muun (x2), Guardian.
  - Nivel de ítem: la mayoría hasta `+15`; sube el nivel con Jewel of Bless/Soul
    y sube los requisitos.
  - Opciones: Luck, Jewel of Life (`STR +5` por nivel de opción) y Jewel of
    Harmony; los ítems deben ser al menos `+9` para Harmony.
  - Grados: Normal, Excellent, Ancient, Socket, Mastery y Enhanced, con
    subcategorías (Siege, Moss, Archangel, Lucky, Mastery Sets).
- Transformación: se extraen las listas y reglas cualitativas; no se convierten
  en `ItemDefinition` ni se adoptan los niveles de acceso como hechos de Season 4.
- Condición de uso: `PARTIAL`, contraste y taxonomía; no demuestra por sí sola la
  versión objetivo.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0037 — MU Online Fanz, página del ítem Kris

- URL canónica: https://muonlinefanz.com/tools/items/data/itemdb/Kris.php
- Título/editor: `Kris`, MU Online Fanz.
- Consulta: 2026-09-16. Página marcada `b2023.11.20.001`.
- Versión declarada: ninguna; la página no distingue temporada.
- Dato extraído: 1-H ATK DMG +6 ~ 11, ATK Speed +50; requisitos Strength 27,
  Agility 27; `Can be equipped by All Classes`; selector de nivel +0..+15;
  grado normal; relacionado `Excellent Kris`.
- Transformación: se registran requisitos y elegibilidad tal como se publican en
  nivel +0. La clase se conserva literal (`All Classes`) sin mapearla aún a
  familias.
- Condición de uso: `PARTIAL`, muestra factual sin temporada demostrada.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0038 — MU Online Fanz, página del ítem Dragon Armor

- URL canónica:
  https://muonlinefanz.com/tools/items/data/itemdb/Dragon%20Armor.php
- Título/editor: `Dragon Armor`, MU Online Fanz.
- Consulta: 2026-09-16. Página marcada `b2023.11.20.001`.
- Versión declarada: ninguna.
- Dato extraído: DEF +37; requisitos Strength 232, Agility 73; `Can be equipped
  by DK, MG`; resto de la pieza `Dragon` enlazada como conjunto; grado normal.
- Transformación: se registran requisitos y elegibilidad en nivel +0. Los códigos
  `DK` y `MG` se conservan literales; no se decide aquí su equivalencia con
  `class-dark-knight` ni `class-magic-gladiator`.
- Condición de uso: `PARTIAL`.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0039 — MU Online Fanz, página del ítem Albatross Bow

- URL canónica:
  https://muonlinefanz.com/tools/items/data/itemdb/Albatross%20Bow.php
- Título/editor: `Albatross Bow`, MU Online Fanz.
- Consulta: 2026-09-16. Página marcada `b2023.11.20.001`.
- Versión declarada: ninguna.
- Dato extraído: 1-H ATK DMG +155 ~ 177, ATK Speed +45; requisitos Strength 218,
  Agility 894; `Can be equipped by ME`; grado normal; relacionado
  `Excellent Albatross Bow`.
- Transformación: se registra literalmente `ME` como identificador de clase.
  `ME` no coincide con ninguna familia del ruleset y apunta a una evolución
  (Muse Elf); se abre `DSP-0006` en lugar de inferir el mapeo.
- Condición de uso: `PARTIAL`.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0040 — Decisión del propietario: axioma acotado de ítems

- Fuente: decisión explícita del propietario comunicada el 2026-09-16.
- Alcance declarado: `mu-s4-global-reference`, Season 4 global/inglés.
- Subconjunto aceptado como axioma estable: Kris, Dragon Armor y Albatross Bow
  de grado normal, con los valores publicados por Fanz el 2026-09-16
  (`EVD-0037`–`EVD-0039`).
- Campos cubiertos: `displayName`, `slots`, `allowedClassIds`, `requiredStats`
  en nivel +0, `maxItemLevel` 15, `optionModules` NORMAL y `socketSlots` 0.
- Mapeo clase-familia/evolución aprobado: `All Classes` → `class-dark-knight`,
  `class-dark-wizard`, `class-fairy-elf`, `class-summoner`,
  `class-magic-gladiator` y `class-dark-lord`; `DK` → `class-dark-knight`;
  `MG` → `class-magic-gladiator`; `ME` → `class-fairy-elf`.
- Fuera del axioma: `requiredLevel`, la progresión de `requiredStats` por nivel
  u opciones, los socket slots y cualquier ítem, ranura, campo o grado no
  enumerados.
- Confianza y uso permitido: `VERIFIED` sólo para el subconjunto y alcance
  anteriores. Autoriza materializar los tres `ItemDefinition` contra
  `item.schema.json`; no autoriza inferir ningún otro dato de ítems.
- Divergencia conservada: el axioma no reclasifica a Fanz como fuente Season 4;
  mantiene `DSP-0005` como límite de versión y `EVD-0035`–`EVD-0039` como
  procedencia.

## Pendiente

- Catálogo acotado materializado y validado: `item-kris`, `item-dragon-armor` y
  `item-albatross-bow` en
  `packages/rulesets/mu-s4-global-reference/v1/items/`.
- Ampliar el catálogo exige nueva evidencia Season 4 o una nueva decisión del
  propietario; hoy queda fuera de alcance.
- Segunda línea de evidencia independiente para futuros ítems.
