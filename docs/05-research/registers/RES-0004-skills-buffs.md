# RES-0004 — Skills y buffs (gate factual de skills)

## Registro

```yaml
id: RES-0004
question: "¿Existe una fuente que demuestre, para Season 4 global/inglesa, la taxonomía, las categorías, los requisitos y los efectos de las skills, buffs e invocaciones de las clases del ruleset?"
scope:
  season: "4"
  class: null
  mode: "general"
  ruleset: "mu-s4-global-reference"
status: VERIFIED
claims:
  - id: SKL-CLM-001
    statement: "Las guías de personaje de Fanz (Dark Knight, Fairy Elf, Summoner) publican, para cada skill, una categoría (`ATK Skill`, `Non-ATK Skill`, `Buff Skill`, `Debuff Skill`, `Summon X`), un coste de Mana y un rango, y en las skills de ataque un Skill DMG base más un crecimiento por nivel de stat."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0043, EVD-0045]
  - id: SKL-CLM-002
    statement: "Fanz declara que la mayoría de las skills se aprenden usando un ítem de skill (Orb, Book, Parchment o Scroll) y que algunas están ligadas a un ítem o se aprenden completando quests."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0043, EVD-0045]
  - id: SKL-CLM-003
    statement: "Fanz publica los requisitos de una skill como ítem de skill combinado con un umbral de nivel de personaje y/o un umbral de stat (`Character Level`, `ENE Level`, `AGI Level`, `STR Level`). Dentro del axioma, sólo el `Character Level` se mapea a `requiredLevel`: ocho skills de las guías Dark Knight y Fairy Elf quedan aceptadas con ese mapeo (Impale 28, Swell Life 120, Death Stab 160, Rageful Blow 170, Twisting Slash 80, Strike of Destruction 220, Penetration 130, Multi-Shot 220)."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0045]
  - id: SKL-CLM-004
    statement: "Dentro del axioma, el mapeo aprobado de `kind` es: `ATK Skill`, `Non-ATK Skill` y `Debuff Skill` → `ACTIVE`; `Buff Skill` → `BUFF`; `Summon X` → `SUMMON`. `PASSIVE` no se acepta (Fanz no publica pasivas en el árbol básico examinado); las categorías `WIZ Skill` y `Curse Skill` quedan fuera del subconjunto."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0043, EVD-0045]
  - id: SKL-CLM-005
    statement: "Dentro del axioma, una skill listada en la guía de una familia aplica a las tres evoluciones de esa familia (`allowedEvolutionIds`): Dark Knight → `evolution-dark-knight`, `evolution-blade-knight`, `evolution-blade-master`; Fairy Elf → `evolution-fairy-elf`, `evolution-muse-elf`, `evolution-high-elf`; Summoner → `evolution-summoner`, `evolution-bloody-summoner`, `evolution-dimension-master`."
    status: VERIFIED
    evidence: [EVD-0045]
  - id: SKL-CLM-006
    statement: "Dentro del axioma, `prerequisiteSkillIds` queda vacío (el prerrequisito de Fanz es un ítem de skill, no una skill) y `buffRef` queda omitido (los efectos de buff se difieren al contrato de buff pendiente)."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0045]
  - id: SKL-CLM-007
    statement: "Los prerrequisitos por stat (`ENE Level`, `AGI Level`, `STR Level`), por quest o por equipo no se mapean a `requiredLevel` ni a `prerequisiteSkillIds`; las skills y buffs que los exigen quedan fuera del subconjunto (Heal, Greater Defense, Greater Damage, invocaciones, Berserker, Darkness, Infinity Arrow, Recovery, Weakness, Innovation, etc.)."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0043, EVD-0045]
  - id: SKL-CLM-008
    statement: "Las mismas páginas de skills de Fanz mezclan el árbol básico con sistemas posteriores a Season 4 (`Master Skill Tree`, `Skill Enhancement Tree`/`Skill Imprint`, cuarta clase y niveles 400/470/480); las skills con requisitos de nivel ≥400, Master Skill, proficiency o Strengthener quedan fuera del subconjunto (Rush, Blood Storm, Fire Blow, Sword Blow, Cure, Party Healing, Poison Arrow, Bless, Summon Satyros, Shadow Step, Evasion, Focus Shot, Death Scythe, Blind, Fire Beast, Aqua Beast)."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0042, EVD-0043, EVD-0044, EVD-0045]
  - id: SKL-CLM-009
    statement: "Fanz publica algunos efectos de buff con valores incompletos (`?`); esas skills y buffs quedan fuera del subconjunto (Berserker `Decreases Max HP by ?`, Recorvery `Recover ? SD`, Focus Shot, Death Scythe, Fire Beast, Aqua Beast, Sword Blow)."
    status: VERIFIED
    evidence: [EVD-0041, EVD-0043, EVD-0045]
conflicts:
  - id: DSP-0008
    statement: "Las guías de personaje y de skills de Fanz no declaran Season 4 y mezclan sistemas y clases posteriores (Master Skill Tree, Skill Enhancement Tree/Skill Imprint, elementos, cuarta clase a nivel 800), por lo que no demuestran la versión objetivo."
    evidence: [EVD-0041, EVD-0042, EVD-0043, EVD-0044]
    scope: "Aplicabilidad a Season 4 global/inglesa de las categorías, requisitos y efectos de skills y buffs."
    impact: "Bloqueaba publicar cualquier `SkillDefinition` como dato factual de Season 4."
    status: RESOLVED
    resolution: OWNER_DECISION
  - id: DSP-0009
    statement: "Las categorías de skill de Fanz (`ATK Skill`, `Non-ATK Skill`, `Buff Skill`, `Debuff Skill`, `Summon X`, `WIZ Skill`, `Curse Skill`) no mapean una a uno con `kind` de `skill.schema.json` (`ACTIVE`, `PASSIVE`, `BUFF`, `SUMMON`)."
    evidence: [EVD-0041, EVD-0042, EVD-0043]
    scope: "Mapeo de la categoría de skill a `kind` de `skill.schema.json`."
    impact: "Impedía derivar `kind` sin una decisión explícita de mapeo."
    status: RESOLVED
    resolution: OWNER_DECISION
  - id: DSP-0010
    statement: "Fanz modela el desbloqueo de una skill como ítem de skill más umbral de personaje y/o umbral de stat (`ENE Level`, `AGI Level`, `STR Level`, `Character Level`), mientras `skill.schema.json` exige un único `requiredLevel` entero y opcionales `prerequisiteSkillIds`."
    evidence: [EVD-0041, EVD-0042]
    scope: "Mapeo de los requisitos de skill a `requiredLevel` y `prerequisiteSkillIds` de `skill.schema.json`."
    impact: "Impedía derivar `requiredLevel` y `prerequisiteSkillIds` sin una decisión explícita."
    status: RESOLVED
    resolution: OWNER_DECISION
  - id: DSP-0011
    statement: "Fanz publica algunos efectos de buff con valores incompletos o genéricos (`?`), sin cifras verificables, lo que impide completar el efecto de un `buffRef`."
    evidence: [EVD-0043]
    scope: "Completitud de los efectos de buff para un futuro contrato de buff."
    impact: "Bloqueaba materializar el efecto de los buffs afectados."
    status: RESOLVED
    resolution: OWNER_DECISION
test_plan: "Materializar las ocho `SkillDefinition` `PUBLISHED` del axioma contra `packages/schemas/v1/skill.schema.json` (1.0.0), enlazando `EVD-0041`–`EVD-0045`: `kind` del mapeo aprobado, `requiredLevel` igual al `Character Level` publicado, `allowedEvolutionIds` con las tres evoluciones de la familia, `prerequisiteSkillIds` vacío, `buffRef` omitido y `conflictIds` `dsp-0008`–`dsp-0011`. Quedan prohibidos por este registro cualquier otra skill, cualquier `buffRef`, cualquier prerrequisito por stat/quest/equipo modelado y cualquier `kind` fuera del mapeo aprobado; ampliarlos exige nueva evidencia o una nueva decisión del propietario."
conclusion: "RES-0004 queda resuelto por decisión del propietario del 2026-09-17 (`EVD-0045`). Fanz no demuestra Season 4, por lo que el propietario acepta como axioma del ruleset un subconjunto acotado y estable: ocho skills de las guías Dark Knight y Fairy Elf cuyos requisitos publicados incluyen un `Character Level` explícito. El axioma fija el mapeo de `kind`, `allowedEvolutionIds` (las tres evoluciones de cada familia), `prerequisiteSkillIds` vacío, `buffRef` omitido y el límite de campos. Los nueve claims pasan a `VERIFIED` sólo dentro de ese alcance; `DSP-0008`, `DSP-0009`, `DSP-0010` y `DSP-0011` quedan `RESOLVED` por `OWNER_DECISION`. El catálogo de skills queda desbloqueado para el subconjunto acotado y sigue bloqueado para cualquier otra skill, buff, campo o grado."
reviewed_by: ["project-owner"]
last_reviewed_at: "2026-09-17"
```

## Alcance y límites

- La versión objetivo es Season 4 global/inglesa (`mu-s4-global-reference`); este
  registro no reabre clases ni evoluciones (`RES-0001`) ni stats y recursos
  derivados (`RES-0002`).
- Se investiga exclusivamente el eje de **skills, buffs e invocaciones** que
  alimentará `SkillDefinition` (`packages/schemas/v1/skill.schema.json`). Quedan
  fuera daño, defensa, rates, PvM/PvP, precios y drops, y los efectos de buff
  (`buffRef` se difiere a un contrato de buff pendiente).
- Cada skill, campo y valor es un claim independiente. Una coincidencia entre
  skills o fuentes no autoriza reutilizar categorías, requisitos o efectos por
  inferencia.
- La clasificación `VERIFIED` proviene del axioma acotado del propietario
  (`EVD-0045`); no implica que Fanz demuestre Season 4 ni habilita datos fuera
  del subconjunto enumerado.
- Fanz es la fuente inicial prioritaria por decisión del propietario del
  2026-07-18; la prioridad no sustituye el control de versión ni el contraste
  (`docs/05-research/source-policy.md`).

## Subconjunto aprobado (axioma del propietario)

| Skill | Familia | Categoría Fanz | `kind` | `requiredLevel` | Skill Item de Fanz | Estado |
|---|---|---|---|---|---|---|
| Impale | Dark Knight | ATK Skill | ACTIVE | 28 | Orb of Impale | VERIFIED (axioma) |
| Twisting Slash | Dark Knight | ATK Skill | ACTIVE | 80 | Orb of Twisting Slash | VERIFIED (axioma) |
| Swell Life | Dark Knight | Buff Skill | BUFF | 120 | Swell Life Orb | VERIFIED (axioma) |
| Death Stab | Dark Knight | ATK Skill | ACTIVE | 160 | Orb of Death Stab | VERIFIED (axioma) |
| Rageful Blow | Dark Knight | ATK Skill | ACTIVE | 170 | Orb of Rageful Blow | VERIFIED (axioma) |
| Strike of Destruction | Dark Knight | ATK Skill | ACTIVE | 220 | Crystal of Destruction | VERIFIED (axioma) |
| Penetration | Fairy Elf | ATK Skill | ACTIVE | 130 | Orb of Penetration | VERIFIED (axioma) |
| Multi-Shot | Fairy Elf | ATK Skill | ACTIVE | 220 | Crystal of Multi-Shot | VERIFIED (axioma) |

El ítem de skill no tiene campo en `skill.schema.json`; se documenta en las
evidencias y no se modela. `Impale` incluye además requisitos de equipo
(Mount, Spear) en Fanz; se documenta como nota y no se modela.

## Excluidos (con motivo)

- **Prerrequisito por stat** (`ENE Level`, `AGI Level`, `STR Level`): Heal,
  Greater Defense, Greater Damage, invocaciones de Elf, Berserker, Darkness,
  Damage Reflection, Sleep, Explosion, etc.
- **Nivel ≥400 / Master Skill / proficiency / Strengthener** (post-S4): Rush,
  Blood Storm, Fire Blow, Sword Blow, Cure, Party Healing, Poison Arrow, Bless,
  Summon Satyros, Shadow Step, Evasion, Focus Shot, Death Scythe, Blind, Fire
  Beast, Aqua Beast.
- **Con quest**: Infinity Arrow, Recovery, Weakness, Innovation.
- **Summoner completo**: ninguna skill osseus cumple sólo `Character Level`.
- **Valores incompletos (`?`)**: Berserker, Recovery, Focus Shot, Death Scythe,
  Fire Beast, Aqua Beast, Sword Blow.
- **`PASSIVE`**: Fanz no publica pasivas en el árbol básico examinado.
- **`WIZ Skill` / `Curse Skill`**: categorías de Summoner fuera del mapeo
  aprobado; sus skills quedan fuera del subconjunto.

## Alcance del axioma del propietario

Decisión del propietario del 2026-09-17 (`EVD-0045`) para desbloquear el
catálogo de skills sin inventar datos:

- Subconjunto aceptado: exactamente las ocho skills de la tabla anterior
  (Dark Knight y Fairy Elf) con `requiredLevel` igual al `Character Level`
  publicado por Fanz.
- Mapeo `kind` aprobado: `ATK Skill`, `Non-ATK Skill` y `Debuff Skill` →
  `ACTIVE`; `Buff Skill` → `BUFF`; `Summon X` → `SUMMON`. `PASSIVE` no se acepta.
- `allowedEvolutionIds` aprobado: para cada familia, sus tres evoluciones
  (etapas 0, 1 y 2).
- `prerequisiteSkillIds`: vacío en todas las skills del subconjunto.
- `buffRef`: omitido; los efectos de buff quedan diferidos al contrato de buff.
- Fuera del axioma: cualquier otra skill o buff, los prerrequisitos por stat,
  quest o equipo, las categorías `WIZ Skill`/`Curse Skill`, `PASSIVE`, `buffRef`
  y cualquier efecto numérico de buff.
- La divergencia de versión se conserva: estos valores no demuestran Season 4 por
  sí mismos; se aceptan como axioma sólo para este subconjunto y alcance.

## Mapas de campos frente a `skill.schema.json` (resueltos por el axioma)

| Campo `SkillDefinition` | Origen en Fanz | Estado |
|---|---|---|
| `displayName` | Nombre de la skill en el árbol | Cubierto |
| `kind` | Categoría Fanz | Cubierto por mapeo aprobado (`EVD-0045`) |
| `requiredLevel` | `Character Level` publicado | Cubierto (nivel de personaje) |
| `prerequisiteSkillIds` | — | Vacío (decisión del axioma) |
| `allowedEvolutionIds` | Guía de familia | Cubierto: tres evoluciones de la familia |
| `buffRef` | Efectos de buff | Omitido (diferido) |
| `evidenceRefs` | `EVD-0041`–`EVD-0045` | Disponible |

## Plan de investigación

1. La evidencia disponible quedó registrada en `EVD-0041`–`EVD-0044` sin promover
   claims automáticamente.
2. Se documentó que ninguna fuente demuestra Season 4 y se abrieron `DSP-0008` a
   `DSP-0011` en lugar de inferir.
3. El propietario resolvió los cuatro conflictos por `OWNER_DECISION` mediante un
   axioma acotado (`EVD-0045`), fijando el subconjunto, el mapeo de `kind`, el
   mapeo a `requiredLevel`/`prerequisiteSkillIds`, `allowedEvolutionIds` y la
   exclusión de buffs con efectos incompletos.
4. La materialización del subconjunto acotado es la siguiente vertical; ampliar
   el catálogo exige nueva evidencia o una nueva decisión explícita.

## Evidencias capturadas

### EVD-0041 — MU Online Fanz, guía de personaje Dark Knight (árbol de skills)

- URL canónica: https://muonlinefanz.com/guide/characters/dk/
- Título/editor: `Dark Knight`, MU Online Fanz.
- Consulta: 2026-09-17.
- Versión declarada: ninguna. La página no menciona Season 4 y contiene secciones
  `Basic Skill Tree`, `Master Skill Tree` y `Skill Enhancement Tree`, además de
  referencias a 4ª clase y niveles 400/800.
- Dato extraído:
  - Encabezado del árbol básico: "Most skills are learned by using a skill items,
    such as an Orb or Book. However, some skills are bound to items or learned
    from completing quests!".
  - `Impale` — ATK Skill; Skill DMG 15; +1 per 35 STR Levels; Range 3; Mana 8;
    requisitos: Orb of Impale, Mount equipado, Character Level 28, Spear weapon.
  - `Twisting Slash` — ATK Skill; Skill DMG 15; +1 per 40 STR Levels; Mana 8;
    requisitos: Orb of Twisting Slash, Character Level 80.
  - `Death Stab` — ATK Skill; Skill DMG 70; +1 per 150 STR Levels; Mana 15;
    requisitos: Orb of Death Stab, Character Level 160.
  - `Rageful Blow` — ATK Skill; Skill DMG 60; +1 per 150 STR Levels; Mana 25;
    requisitos: Orb of Rageful Blow, Character Level 170.
  - `Swell Life` — Buff Skill; Base HP +12%; +1% every 20 ENE Levels; +1% every
    100 STA Level; Mana 22; requisitos: Swell Life Orb, Character Level 120.
  - `Strike of Destruction` — ATK Skill; Mana 30; requisitos: Crystal of
    Destruction, Character Level 220.
- Transformación: se conservan categorías, costes, rangos y requisitos literales;
  el mapeo de `kind` y `requiredLevel` se realiza sólo dentro del axioma
  (`EVD-0045`).
- Condición de uso: `PARTIAL`, muestra factual sin temporada demostrada.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis,
  sin copiar imágenes ni capturar snapshot.
- Snapshot/hash: no capturado.

### EVD-0042 — MU Online Fanz, guía de personaje Fairy Elf (árbol de skills)

- URL canónica: https://muonlinefanz.com/guide/characters/elf/
- Título/editor: `Fairy Elf`, MU Online Fanz.
- Consulta: 2026-09-17.
- Versión declarada: ninguna. La página mezcla `Basic Skill Tree`, `Master Skill
  Tree` y `Skill Enhancement Tree` (4ª clase, niveles 400/800).
- Dato extraído:
  - `Penetration` — ATK Skill; Skill DMG 70; +1 per 200 AGI Levels; Mana 7;
    requisitos: Orb of Penetration, Character Level 130.
  - `Multi-Shot` — ATK Skill; Skill DMG 40; +1 per 200 AGI Levels; Mana 10;
    requisitos: Crystal of Multi-Shot, Character Level 220.
  - `Triple Shot`, `Heal`, `Greater Defense`, `Greater Damage` y las
    invocaciones (`Summon Goblin`…`Summon Bali`): requisitos por stat
    (`AGI Level`/`ENE Level`), excluidos del axioma.
  - `Infinity Arrow`, `Recovery`: con quest `Treasures of MU`, excluidos.
  - Skills con nivel ≥400 (Cure, Party Healing, Poison Arrow, Bless, Summon
    Satyros, Shadow Step, Evasion, Focus Shot): post-S4, excluidas.
- Transformación: se conservan requisitos y categorías literales; se aplica el
  mapeo del axioma sólo a Penetration y Multi-Shot.
- Condición de uso: `PARTIAL`.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0043 — MU Online Fanz, guía de personaje Summoner (árbol de skills)

- URL canónica: https://muonlinefanz.com/guide/characters/sum/
- Título/editor: `Summoner` (Dimension Summoner), MU Online Fanz.
- Consulta: 2026-09-17.
- Versión declarada: ninguna; incluye `Master Skill Tree` y `Skill Enhancement
  Tree` con referencias a 4ª clase y niveles 400/800.
- Dato extraído:
  - `Berserker` — Buff Skill; Mana 100; aumenta WIZ y Attack Speed, reduce DEF y
    HP; no usable con `Darkness`; lista `Decreases Max HP by ?` y
    `Decreases DEF by ?` (incompleto).
  - `Darkness` — Buff Skill; Mana 100; aumenta Curse y Defense, reduce HP; no
    usable con `Berserker`; debe estar activa para usar cualquier Curse Skill.
  - `Weakness` e `Innovation` — Debuff Skill (AoE); requisitos: Parchment,
    Character Level 220, quest `Treasures of MU` y `ENE Level` 663/912
    (excluidas).
  - `Death Scythe`, `Fire Beast`, `Aqua Beast`, `Blind`: requisitos nivel ≥430
    o Master Skill; `WIZ Skill`/`Curse Skill` (excluidas del subconjunto).
  - Skills elementales y de buff con `ENE Level` (Meteorite, Fire Ball, Ice,
    Power Wave, Drain Life, Chain Lightning, Damage Reflection): excluidas.
- Transformación: se registran categorías, costes y textos de efecto sin adoptar
  cifras ni `kind`; ninguna skill de Summoner queda en el subconjunto.
- Condición de uso: `PARTIAL`; los valores `?` quedan marcados como incompletos.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0044 — MU Online Fanz, índice de personajes

- URL canónica: https://muonlinefanz.com/guide/characters
- Título/editor: `Characters`, MU Online Fanz.
- Consulta: 2026-09-17. Página actualizada 2025-05-08, build `b2023.09.11.001`.
- Versión declarada: ninguna. El índice declara "There are currently 14 playable
  character classes!" e incluye clases posteriores a Season 4 (Slayer, Gun
  Crusher, White Wizard, Mage, Rune Mage, Grow Lancer, Rage Fighter) y un
  `4th Class Upgrade` a nivel 800.
- Dato extraído: enumeración de 14 clases y de las rutas de progresión
  (`2nd Class Upgrade` nivel 150, `3rd Class Upgrade` nivel 400, `4th Class
  Upgrade` nivel 800).
- Transformación: se conserva sólo como caracterización de la fuente; no se
  extrae ningún valor de skill.
- Condición de uso: `PARTIAL`, investigación y contraste.
- Licencia/uso conocido: fan site con permiso de Webzen; sólo URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0045 — Decisión del propietario: axioma acotado de skills

- Fuente: decisión explícita del propietario comunicada el 2026-09-17.
- Alcance declarado: `mu-s4-global-reference`, Season 4 global/inglés.
- Subconjunto aceptado como axioma estable: las ocho skills con `Character
  Level` publicado (`EVD-0041`, `EVD-0042`): Impale, Twisting Slash, Swell Life,
  Death Stab, Rageful Blow y Strike of Destruction (Dark Knight) y Penetration y
  Multi-Shot (Fairy Elf).
- Mapeo `kind` aprobado: `ATK Skill`, `Non-ATK Skill` y `Debuff Skill` →
  `ACTIVE`; `Buff Skill` → `BUFF`; `Summon X` → `SUMMON`; `PASSIVE` no se acepta.
- `allowedEvolutionIds` aprobado: las tres evoluciones de cada familia
  (etapas 0, 1 y 2).
- `prerequisiteSkillIds` vacío y `buffRef` omitido en todo el subconjunto.
- Fuera del axioma: cualquier otra skill o buff, prerrequisitos por
  stat/quest/equipo, categorías `WIZ Skill`/`Curse Skill`, `PASSIVE`, `buffRef`
  y efectos de buff.
- Confianza y uso permitido: `VERIFIED` sólo para el subconjunto y alcance
  anteriores. Autoriza materializar las ocho `SkillDefinition` contra
  `skill.schema.json`; no autoriza inferir ningún otro dato de skills.
- Divergencia conservada: el axioma no reclasifica a Fanz como fuente Season 4;
  mantiene `DSP-0008` como límite de versión y `EVD-0041`–`EVD-0044` como
  procedencia.

## Pendiente

- Catálogo acotado materializado y validado: ocho `SkillDefinition` en
  `packages/rulesets/mu-s4-global-reference/v1/skills/`.
- Ampliar el catálogo exige nueva evidencia Season 4 o una nueva decisión del
  propietario; hoy queda fuera de alcance.
- Contrato de buff (`buffRef`) pendiente para modelar efectos de buff.
- Segunda línea de evidencia independiente para futuras skills y buffs.