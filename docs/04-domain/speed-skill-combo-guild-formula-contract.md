# Contrato de Speed, Combo, Skills, Wizardry y Guild

## Identidad y autoridad

- Veinticinco fórmulas `1.0.0` nacen `PUBLISHED` contra schema `2.1.0` y cierran
  el catálogo fáctico que `EVD-0026` preservaba sin motor: seis `speed`
  (`agility / divisor` para las seis familias), `combo-base` de Dark Knight,
  los `skill-percent` de Dark Knight y Dark Lord, `fortitude-percent`,
  `soul-barrier-percent`, los `min/max-wizardry` (DW y MG/Summoner ya
  publicados) quedan completos con `Dark Wizard`, `nova-max-spell-damage`,
  los buffs de Fairy Elf (`damage-buff`, `defense-buff`, `heal`),
  `fenrir-base-{min,max}-damage` de Dark Knight y Dark Wizard,
  `critical-damage`, `fireburst-bonus-{min,max}-damage` y
  `guild-member-capacity`.
- Contrato: `formula.schema.json` `2.1.0`. Ruleset: `mu-s4-global-reference`.
- Estado: las veinticinco `PUBLISHED`; `confidence: VERIFIED`.
- Aplicabilidad: cada fórmula a todas las evoluciones de su familia, igual que
  el resto del catálogo (Dark Wizard/Soul Master/Grand Master,
  Dark Knight/Blade Knight/Blade Master, Fairy Elf/Muse Elf/High Elf,
  Summoner/Bloody Summoner/Dimension Master, Magic Gladiator/Duel Master y
  Dark Lord/Lord Emperor).
- Evidencia de mínimos factuales de stats por familia: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Conflicto: sólo `formula-speed-magic-gladiator` hereda `conflictIds:
  ["dsp-0002"]` (el rate de Magic Gladiator comparte álgebra de Agility con la
  familia; ver `DSP-0002`). El resto conserva `conflictIds: []`.

Estas expresiones se conservan como axiomas del ruleset fuera del inventario de
24 claims de `RES-0002`. No se añadió evidencia ni claim y no se reclasificó
ninguna fuente: el contrato materializa únicamente el catálogo aprobado en
`EVD-0026`.

## Reglas comunes

- Todas las fórmulas usan `CHECKED_DECIMAL_V1`: literales e intermedios
  decimales exactos, aritmética comprobada, sin redondeos intermedios y un
  único `APPLY_ROUNDING` (`TRUNCATE`) en el paso visible.
- Consumen únicamente `CONTEXT_VALUE` (`character-level` y `resolved-{statId}`)
  y no declaran dependencias entre fórmulas.
- Los stats fijan `minimum = base EVD-0021` con
  `rangeErrorCode: formula-stat-below-base`. `character-level` acota
  `1..2147483647` con `formula-level-out-of-range` y `source` `CONTEXT_VALUE`
  + `valueId: character-level`.
- Salidas: `speed-point` y `member-count` son unidades nuevas en el catálogo;
  el resto reutiliza `damage-point`, `percent` y `hp-point`. Los bounds del
  programa provienen del `numericBounds` declarado y la resolución
  `CONTEXT_VALUE` emite `valorVisible = base` conforme a los casos aprobados.
- Cada fórmula enlaza cuatro casos positivos (`{prefix}-base`,
  `{prefix}-fraction-step`, `{prefix}-integer-step` y un paso sobre la
  evolución superior) y controles negativos de stat fuera de base (44), de
  familia ajena (25), de nivel inválido para `guild-member-capacity` (1) y de
  overflow para `combo-base` (1).
- `combo-base-dark-knight` es el único que declara un control de overflow:
  `(strength + agility + energy) / 2` sobre los tres stats a `INT64.Max`
  dispara `formula-arithmetic-overflow` en la suma comprobada.

## Velocidad

`speed = agility / divisor`

| Fórmula | Divisor | Base (agility → raw/visible) |
|---|---|---|
| `formula-speed-dark-knight` | 15 | 20 → 1.3333…/1 |
| `formula-speed-dark-wizard` | 10 | 18 → 1.8/1 |
| `formula-speed-fairy-elf` | 50 | 25 → 0.5/0 |
| `formula-speed-magic-gladiator` | 15 | 26 → 1.7333…/1 |
| `formula-speed-dark-lord` | 10 | 20 → 2/2 |
| `formula-speed-summoner` | 20 | 21 → 1.05/1 |

Los `integer-step` fijan la Agility en el mínimo múltiplo del divisor que
respeta el mínimo factual (`Ceil(base / divisor) × divisor`), de modo que la
división visible sea exacta. Los `fraction-step` suman un punto de Agility; el
cuarto caso usa la evolución superior de la familia.

## Combo

`combo-base-dark-knight = (strength + agility + energy) / 2`

| Fórmula | Base (raw/visible) |
|---|---|
| `formula-combo-base-dark-knight` | (28 + 20 + 10) / 2 → 29/29 |

Se resuelve con `stat-sum = ADD(strength, agility, energy)` y
`raw-combo-base = DIVIDE(stat-sum, literal 2)`.

## Skills y porcentajes

| Fórmula | Expresión | Base (raw/visible) |
|---|---|---|
| `formula-skill-percent-dark-knight` | `200 + energy / 10` | 10 → 201/201 |
| `formula-skill-percent-dark-lord` | `200 + energy / 20` | 15 → 200.75/200 |
| `formula-fortitude-percent-dark-knight` | `12 + vitality / 100 + energy / 20` | 25 + 10 → 12.75/12 |
| `formula-soul-barrier-percent-dark-wizard` | `10 + agility / 50 + energy / 200` | 18 + 30 → 10.51/10 |
| `formula-damage-buff-fairy-elf` | `3 + energy / 7` | 15 → 5.1428…/5 |
| `formula-defense-buff-fairy-elf` | `2 + energy / 8` | 15 → 3.875/3 |
| `formula-heal-fairy-elf` | `2 + energy / 9` | 15 → 3.6666…/3 |

Los buffs de Fairy Elf reutilizan la salida `damage-point`/`defense-point`/
`hp-point` con unidad declarada y el mínimo factual de Energy 15 (`EVD-0021`).

## Fenrir

| Fórmula | Expresión | Base (raw/visible) |
|---|---|---|
| `formula-fenrir-base-min-damage-dark-knight` | `45 + str/3 + agi/5 + vit/5 + ene/6` | 28+20+25+10 → 65/65 |
| `formula-fenrir-base-max-damage-dark-knight` | `75 + str/3 + agi/5 + vit/5 + ene/6` | 28+20+25+10 → 95/95 |
| `formula-fenrir-base-min-damage-dark-wizard` | `60 + str/5 + agi/5 + vit/7 + ene/3` | 18+18+15+30 → 79.3428…/79 |
| `formula-fenrir-base-max-damage-dark-wizard` | `90 + str/5 + agi/5 + vit/7 + ene/3` | 18+18+15+30 → 109.3428…/109 |

## Wizardry y Nova

| Fórmula | Expresión | Base (raw/visible) |
|---|---|---|
| `formula-min-wizardry-dark-wizard` | `energy / 9` | 30 → 3.3333…/3 |
| `formula-max-wizardry-dark-wizard` | `energy / 4` | 30 → 7.5/7 |
| `formula-nova-max-spell-damage-dark-wizard` | `1320 + strength / 2` | 18 → 1329/1329 |

## Crítico y Fireburst

| Fórmula | Expresión | Base (raw/visible) |
|---|---|---|
| `formula-critical-damage-dark-lord` | `command / 25 + strength / 30` | 25 + 26 → 1.8666…/1 |
| `formula-fireburst-bonus-min-damage-dark-lord` | `100 + strength / 25 + energy / 50` | 26 + 15 → 101.34/101 |
| `formula-fireburst-bonus-max-damage-dark-lord` | `150 + strength / 25 + energy / 50` | 26 + 15 → 151.34/151 |

`critical-damage` es el único del conjunto que consume Command además de
Strength, coherente con el mínimo factual `command = 25` de `EVD-0021`.

## Guild

`guild-member-capacity-dark-lord = level / 10 + command / 10`

| Fórmula | Base (raw/visible) |
|---|---|
| `formula-guild-member-capacity-dark-lord` | level 1 + command 25 → 2.6/2 |

Es la única fórmula del contrato con entrada de nivel: declara
`character-level` `1..2147483647` con `formula-level-out-of-range` y añade el
control `invalid-level` (nivel 0) además de `command-below-base` y familia
ajena.

## Casos aprobados

Total 100 positivos (25 fórmulas × 4) y 71 controles negativos (44 stat fuera
de base + 25 familia ajena + 1 nivel inválido + 1 overflow). El gate canónico
exige cobertura completa de positivos y rechazo de los controles para cada
fórmula `PUBLISHED`; la identidad visible deriva del truncamiento único en
`visible-{output}` y la traza declara exactamente los pasos del programa.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa los `resolved-{statId}` desde
el estado inmutable de la pantalla mediante la resolución `CONTEXT_VALUE`;
WPF reutiliza la selección genérica entre los atributos derivados y no contiene
las expresiones, sus constantes ni un handler por fórmula. El catálogo avanza a
ciento siete fórmulas ejecutables y el dataset a `2026-07-30.3`; el smoke
publicado reproduce los 428 casos aprobados (107 fórmulas × 4 positivos) en
las fases inicial y de reemplazo.