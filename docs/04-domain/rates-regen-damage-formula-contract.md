# Contrato de Rates, Regeneración y Daño restante

## Identidad y autoridad

- Cuarenta fórmulas `1.0.0` nacen `PUBLISHED` contra schema `2.1.0` y cierran
  los contratos fácticos que `EVD-0026` preservaba sin motor: veinticuatro rates
  (`pvm-attack-rate`, `pvm-defense-rate`, `pvp-attack-rate` y
  `pvp-defense-rate` por cada una de las seis familias), diez regeneraciones
  (`mana-regen` y `ag-regen` de Dark Wizard, Dark Knight, Fairy Elf, Magic
  Gladiator y Dark Lord) y seis daños físicos restantes (`min-damage` y
  `max-damage` de Dark Knight, Fairy Elf y Dark Lord). Summoner y Magic
  Gladiator ya materializaron daño y wizardry en la vertical anterior.
- Contracto: `formula.schema.json` `2.1.0`. Ruleset: `mu-s4-global-reference`.
- Estado: las cuarenta `PUBLISHED`; `confidence: VERIFIED`.
- Aplicabilidad: cada fórmula a las tres evoluciones de su familia
  (Dark Wizard/Soul Master/Grand Master, Dark Knight/Blade Knight/Blade Master,
  Fairy Elf/Muse Elf/High Elf, Summoner/Bloody Summoner/Dimension Master,
  Magic Gladiator/Duel Master y Dark Lord/Lord Emperor).
- Evidencia de mínimos factuales de stats por familia: `EVD-0021`.
- Evidencia de expresiones, alcance, truncamiento visible y consumo `RAW` de
  dependencias de regeneración: `EVD-0026`.
- Conflicto aplicable: ninguno conocido en este conjunto; las trazas conservan
  `conflictIds: []`. El daño de Magic Gladiator/Summoner no pertenece a este
  contrato; `DSP-0002` no se hereda.

Estas expresiones se conservan como axiomas del ruleset fuera del inventario de
24 claims de `RES-0002`. No se añadió evidencia ni claim, y no se reclasificó
ninguna fuente: el contrato materializa únicamente el catálogo aprobado en
`EVD-0026`.

## Reglas comunes

- Todas las fórmulas usan `CHECKED_DECIMAL_V1`: literales e intermedios
  decimales exactos, aritmética comprobada, sin redondeos intermedios y un
  único `APPLY_ROUNDING` (`TRUNCATE`) en el paso visible.
- Rates y daños consumen únicamente `CONTEXT_VALUE` (`character-level` y
  `resolved-{statId}`) y no declaran dependencias entre fórmulas.
- Regeneración declara el stat de anclaje contextual (`character-level` para
  `mana-regen` y `agility` para `ag-regen`) y consume la salida `RAW` decimal
  de su Mana/AG publicada mediante `FORMULA_OUTPUT`/`RAW`, igual que Defense→SD
  (`EVD-0034`); el mínimo técnico de la dependencia (`mana`/`ag`) es 0 con
  `rangeErrorCode: formula-dependency-out-of-range`.
- Cada fórmula enlaza cuatro casos positivos: `{prefix}-base`,
  `{prefix}-fraction-step`, `{prefix}-integer-step` y un paso sobre la
  evolución superior de la familia. Cada fórmula enlaza además dos controles
  negativos: el stat o la dependencia fuera de su base y una familia ajena
  (`formula-not-applicable`). Sólo los positivos están enlazados desde
  `testCaseRefs`.
- No se fabricaron controles de overflow en este cierre (regeneración y daño
  mantienen la salida dentro de `INT64` para el dominio admitido; los rates
  conservan los dos controles de rango/familia sin añadir un caso de overflow).
- El daño de Dark Lord consume Energy (`min = str/7 + ene/14`,
  `max = str/5 + ene/10`); el de Fairy Elf consume Agility
  (`min = str/14 + agi/7`, `max = str/8 + agi/4`); el de Dark Knight consume
  sólo Strength.

## Rates de ataque y defensa PVM

### PVM attack rate — `5 × level + 1.5 × agility + strength / 4`

Summoner declara el término de Agility como `agility × 1.5`. Dark Lord
sustituye `strength / 4` por `strength / 6 + command / 10`, único rate del
conjunto que consume Command; dicha adición se resuelve con `ADD` de cuatro
operandos sobre la misma aritmética comprobada.

| Fórmula | Entradas (mínimo `EVD-0021`) | Base (raw/visible) |
|---|---|---|
| `formula-pvm-attack-rate-dark-wizard` | level 1, agility 18, strength 18 | 36.5/36 |
| `formula-pvm-attack-rate-dark-knight` | level 1, agility 20, strength 28 | 42/42 |
| `formula-pvm-attack-rate-fairy-elf` | level 1, agility 25, strength 22 | 48.0/48 |
| `formula-pvm-attack-rate-summoner` | level 1, agility 21, strength 21 | 41.75/41 |
| `formula-pvm-attack-rate-magic-gladiator` | level 1, agility 26, strength 26 | 50.5/50 |
| `formula-pvm-attack-rate-dark-lord` | level 1, agility 20, strength 26, command 25 | 41.8333…/41 |

Los casos `fraction-step` incrementan Agility en uno y conservan el resto de la
base; los `integer-step` fijan un valor que hace entera la contribución de
Agility; el cuarto caso usa la evolución superior de la familia con valores de
nivel 220. `character-level` acota `1..2147483647` con
`formula-level-out-of-range`; los stats fijan su mínimo factual con
`formula-stat-below-base`.

### PVM defense rate — `agility / divisor`

| Fórmula | Divisor | Integer-step |
|---|---|---|
| `formula-pvm-defense-rate-dark-wizard` | 3 | agility 18 → 6 |
| `formula-pvm-defense-rate-dark-knight` | 3 | agility 21 → 7 |
| `formula-pvm-defense-rate-fairy-elf` | 4 | agility 28 → 7 |
| `formula-pvm-defense-rate-summoner` | 4 | agility 24 → 6 |
| `formula-pvm-defense-rate-magic-gladiator` | 3 | agility 27 → 9 |
| `formula-pvm-defense-rate-dark-lord` | 7 | agility 21 → 3 |

Los casos `integer-step` eligen el mínimo múltiplo del divisor que respeta el
mínimo factual de Agility (`Ceil(base / divisor) × divisor`), de modo que la
división visible sea exacta. Dark Wizard coincide con su base 18 porque 18 es
múltiplo de 3. Las bases son `6/6`, `6.666…/6`, `6.25/6`, `5.25/5`,
`8.666…/8` y `2.857…/2` en el orden de la tabla.

### PVP attack rate — `3 × level + k × agility`

| Fórmula | k | Base (raw/visible) |
|---|---|---|
| `formula-pvp-attack-rate-dark-wizard` | 4 | 75/75 |
| `formula-pvp-attack-rate-dark-knight` | 4.5 | 93.0/93 |
| `formula-pvp-attack-rate-fairy-elf` | 0.6 | 18.0/18 |
| `formula-pvp-attack-rate-summoner` | 3.5 | 76.5/76 |
| `formula-pvp-attack-rate-magic-gladiator` | 3.5 | 94.0/94 |
| `formula-pvp-attack-rate-dark-lord` | 4 | 83/83 |

### PVP defense rate — `2 × level + k × agility`

| Fórmula | k | Base (raw/visible) |
|---|---|---|
| `formula-pvp-defense-rate-dark-wizard` | 0.25 | 6.50/6 |
| `formula-pvp-defense-rate-dark-knight` | 0.5 | 12.0/12 |
| `formula-pvp-defense-rate-fairy-elf` | 0.1 | 4.5/4 |
| `formula-pvp-defense-rate-summoner` | 0.5 | 12.5/12 |
| `formula-pvp-defense-rate-magic-gladiator` | 0.25 | 8.50/8 |
| `formula-pvp-defense-rate-dark-lord` | 0.5 | 12.0/12 |

## Regeneración

### Mana regeneration — `mana / 27.5`

Consume el `RAW` decimal de `formula-mana-{familia}` `1.0.0`. Declara
`character-level` (`CONTEXT_VALUE`, mínimo técnico 1) como anclaje del mismo
estado validado aunque el programa sólo consume `mana`, y `mana`
(`FORMULA_OUTPUT`/`RAW`, mínimo técnico 0).

| Fórmula | Base (mana → raw/visible) |
|---|---|
| `formula-mana-regen-dark-wizard` | 60 → 2.1818…/2 |
| `formula-mana-regen-dark-knight` | 20.0 → 0.727…/0 |
| `formula-mana-regen-fairy-elf` | 30.0 → 1.0909…/1 |
| `formula-mana-regen-magic-gladiator` | 60 → 2.1818…/2 |
| `formula-mana-regen-dark-lord` | 40 → 1.4545…/1 |

Los `integer-step` fijan un Mana divisible por 27.5 (110 → 4; 55 → 2) para la
familia correspondiente; los `fraction-step` suben una unidad de nivel sobre la
base. Los dos controles usan `mana` negativo
(`formula-dependency-out-of-range`) y familia ajena.

### AG regeneration — `constante + ag / divisor`

Consume el `RAW` decimal de `formula-ag-{familia}` `1.0.0`. Declara `agility`
(`CONTEXT_VALUE`, mínimo factual `EVD-0021`) como anclaje del estado aunque el
programa sólo consume `ag`, y `ag` (`FORMULA_OUTPUT`/`RAW`, mínimo técnico 0).

| Fórmula | Constante | Divisor | Base (ag → raw/visible) |
|---|---|---|---|
| `formula-ag-regen-dark-wizard` | 2 | 20 | 21.3 → 3.065/3 |
| `formula-ag-regen-dark-knight` | 2 | 20 | 25.7 → 3.285/3 |
| `formula-ag-regen-fairy-elf` | 2 | 33.33 | 20.6 → 2.618…/2 |
| `formula-ag-regen-magic-gladiator` | 1.9 | 33 | 23.40 → 2.609…/2 |
| `formula-ag-regen-dark-lord` | 1.9 | 33 | 23.55 → 2.6136…/2 |

Los `integer-step` fijan la Agility en base + 50 e incrementan la `ag`
resultante en +50 × coeficiente; el caso de evolución superior sube la base en
200. Los dos controles usan `ag` negativa (`formula-dependency-out-of-range`) y
familia ajena. El caso base de Dark Knight consume el `RAW` canónico 25.7 de
`formula-ag-dark-knight`; la variante documental 17.7 identificada durante la
generación no corresponde a la fórmula publicada y queda descartada (ver
`RES-0002`, bitácora del 2026-09-11).

## Daño restante

### Min damage

| Fórmula | Expresión | Base (raw/visible) |
|---|---|---|
| `formula-min-damage-dark-knight` | `strength / 6` | 28 → 4.666…/4 |
| `formula-min-damage-fairy-elf` | `strength / 14 + agility / 7` | 22 + 25 → 5.1428…/5 |
| `formula-min-damage-dark-lord` | `strength / 7 + energy / 14` | 26 + 15 → 4.7857…/4 |

### Max damage

| Fórmula | Expresión | Base (raw/visible) |
|---|---|---|
| `formula-max-damage-dark-knight` | `strength / 4` | 28 → 7/7 |
| `formula-max-damage-fairy-elf` | `strength / 8 + agility / 4` | 22 + 25 → 9.00/9 |
| `formula-max-damage-dark-lord` | `strength / 5 + energy / 10` | 26 + 15 → 6.7/6 |

Cada daño fija `{stat}-below-base` (Strength para Dark Knight y Dark Lord,
Agility para Fairy Elf; los pasos fraccionarios suben un punto sobre la base de
ese stat) y familia ajena. La suma máxima de coeficientes por salida
(`1/6`, `3/14` y `3/14` respectivamente) es inferior a 1, por lo que no se
fabrican controles de overflow.

## Casos aprobados

Total 160 positivos (40 fórmulas × 4) y 80 controles negativos (40 × 2). El
gate canónico exige cobertura completa de positivos y rechazo de los controles
para cada fórmula `PUBLISHED`; la identidad visible de cada caso deriva del
truncamiento único en `visible-{output}` y la traza declara exactamente los
pasos del programa.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa las dependencias de
regeneración con el mismo estado validado usado para Defense→SD; el
`character-level` y `resolved-{statId}` provienen del estado inmutable de la
pantalla. WPF reutiliza la selección genérica entre los atributos derivados
adicionales y no contiene las expresiones, sus constantes ni un handler por
fórmula. El smoke publicado de `2026-07-30.2` reproduce los 328 casos
contextuales de las ochenta y dos fórmulas ejecutables en las fases inicial y
de reemplazo.