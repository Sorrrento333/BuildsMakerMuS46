# Contrato de Daño y Wizardry de Magic Gladiator

## Identidad y autoridad

- Fórmulas: `formula-min-damage-magic-gladiator` `1.0.0`,
  `formula-max-damage-magic-gladiator` `1.0.0`,
  `formula-min-wizardry-magic-gladiator` `1.0.0` y
  `formula-max-wizardry-magic-gladiator` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: las cuatro `PUBLISHED`.
- Aplicabilidad: `class-magic-gladiator`, `evolution-magic-gladiator` y
  `evolution-duel-master`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Conflicto aplicable: `DSP-0002` en las cuatro fórmulas porque consumen
  Energy; permanece resuelto a favor de Energy 26 y sólo se conserva en la
  traza, sin reescribir la fuente.

Las expresiones de daño y wizardry se conservan como axiomas del ruleset fuera
del inventario de 24 claims de `RES-0002`. No se añadió evidencia, fórmula ni
claim, y no se reclasificó ninguna fuente: este contrato materializa
únicamente el catálogo aprobado en `EVD-0026`.

## Daño

`formula-min-damage-magic-gladiator` y `formula-max-damage-magic-gladiator`
comparten entradas:

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 26, `EVD-0021` |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 26, `EVD-0021`/`DSP-0002` |

Las expresiones autorizadas son `min_damage = str / 6 + ene / 12` y
`max_damage = str / 4 + ene / 8`. Cada programa conserva los dos aportes como
pasos decimales, los suma con aritmética decimal comprobada y trunca hacia
cero una sola vez para la salida visible; no hay redondeos intermedios.

| Caso | Evolución | STR/ENE | Daño crudo | Daño visible |
|---|---|---:|---:|---:|
| `min-damage-magic-gladiator-base` | Magic Gladiator | `26/26` | 6.5000000000000000000000000000 | 6 |
| `min-damage-magic-gladiator-fraction-step` | Magic Gladiator | `29/27` | 7.0833333333333333333333333333 | 7 |
| `min-damage-magic-gladiator-integer-step` | Magic Gladiator | `30/30` | 7.5 | 7 |
| `min-damage-magic-gladiator-duel-master-step` | Duel Master | `35/28` | 8.166666666666666666666666667 | 8 |
| `max-damage-magic-gladiator-base` | Magic Gladiator | `26/26` | 9.75 | 9 |
| `max-damage-magic-gladiator-fraction-step` | Magic Gladiator | `29/27` | 10.625 | 10 |
| `max-damage-magic-gladiator-integer-step` | Magic Gladiator | `30/30` | 11.25 | 11 |
| `max-damage-magic-gladiator-duel-master-step` | Duel Master | `35/28` | 12.25 | 12 |

## Wizardry

`formula-min-wizardry-magic-gladiator` y
`formula-max-wizardry-magic-gladiator` comparten entrada:

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 26, `EVD-0021`/`DSP-0002` |

Las expresiones autorizadas son `min_wizardry_damage = ene / 9` y
`max_wizardry_damage = ene / 4`. Cada programa conserva el cociente decimal
como salida cruda y trunca hacia cero una sola vez para la salida visible.

| Caso | Evolución | Energy | Daño crudo | Daño visible |
|---|---|---:|---:|---:|
| `min-wizardry-magic-gladiator-base` | Magic Gladiator | 26 | 2.8888888888888888888888888889 | 2 |
| `min-wizardry-magic-gladiator-fraction-step` | Magic Gladiator | 28 | 3.1111111111111111111111111111 | 3 |
| `min-wizardry-magic-gladiator-integer-step` | Magic Gladiator | 27 | 3 | 3 |
| `min-wizardry-magic-gladiator-duel-master-step` | Duel Master | 32 | 3.5555555555555555555555555556 | 3 |
| `max-wizardry-magic-gladiator-base` | Magic Gladiator | 26 | 6.5 | 6 |
| `max-wizardry-magic-gladiator-fraction-step` | Magic Gladiator | 27 | 6.75 | 6 |
| `max-wizardry-magic-gladiator-integer-step` | Magic Gladiator | 28 | 7 | 7 |
| `max-wizardry-magic-gladiator-duel-master-step` | Duel Master | 32 | 8 | 8 |

## Casos aprobados

Las fórmulas de daño enlazan tres controles cada una: Strength bajo base,
Energy bajo base y una familia ajena. Las de wizardry enlazan dos: Energy bajo
base y familia ajena; en total 16 positivos y 10 controles. No se crean
controles de overflow: los inputs válidos son no negativos y la suma máxima de
coeficientes por salida es `3/8`, por lo que aun dos valores `INT64` máximos
producen una salida menor que `Int64.MaxValue`. Sólo los positivos están
enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance, conflicto y cobertura. Application materializa la definición
genérica y resuelve los stats desde distribución; no requiere un input de
nivel para la fórmula. WPF reutiliza la selección genérica entre atributos
derivados y no contiene la expresión, sus constantes ni un handler por
fórmula. El smoke publicado debe reproducir los positivos en las fases inicial
y de reemplazo.