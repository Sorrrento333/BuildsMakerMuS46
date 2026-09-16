# Contrato de Daño y Wizardry de Summoner

## Identidad y autoridad

- Fórmulas: `formula-min-damage-summoner` `1.0.0`,
  `formula-max-damage-summoner` `1.0.0`, `formula-min-wizardry-summoner`
  `1.0.0` y `formula-max-wizardry-summoner` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: las cuatro `PUBLISHED`.
- Aplicabilidad: `class-summoner`, `evolution-summoner`,
  `evolution-bloody-summoner` y `evolution-dimension-master`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Conflicto aplicable: ninguno conocido; la traza conserva `conflictIds: []`.

Las expresiones de daño y wizardry se conservan como axiomas del ruleset fuera
del inventario de 24 claims de `RES-0002`. No se añadió evidencia, fórmula ni
claim, y no se reclasificó ninguna fuente: este contrato materializa
únicamente el catálogo aprobado en `EVD-0026`.

## Daño

`formula-min-damage-summoner` y `formula-max-damage-summoner` comparten entrada:

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 21, `EVD-0021` |

Las expresiones autorizadas son `min_damage = str / 8` y
`max_damage = str / 4`. Cada programa conserva el cociente decimal como salida
cruda y trunca hacia cero una sola vez para la salida visible.

| Caso | Evolución | Strength | Daño crudo | Daño visible |
|---|---|---:|---:|---:|
| `min-damage-summoner-base` | Summoner | 21 | 2.625 | 2 |
| `min-damage-summoner-fraction-step` | Summoner | 25 | 3.125 | 3 |
| `min-damage-summoner-integer-step` | Summoner | 24 | 3 | 3 |
| `min-damage-summoner-dimension-master-step` | Dimension Master | 28 | 3.5 | 3 |
| `max-damage-summoner-base` | Summoner | 21 | 5.25 | 5 |
| `max-damage-summoner-fraction-step` | Summoner | 25 | 6.25 | 6 |
| `max-damage-summoner-integer-step` | Summoner | 24 | 6 | 6 |
| `max-damage-summoner-dimension-master-step` | Dimension Master | 28 | 7 | 7 |

## Wizardry

`formula-min-wizardry-summoner` y `formula-max-wizardry-summoner` comparten
entrada:

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

Las expresiones autorizadas son `min_wizardry_damage = ene / 9` y
`max_wizardry_damage = ene / 4`. Cada programa conserva el cociente decimal
como salida cruda y trunca hacia cero una sola vez para la salida visible.

| Caso | Evolución | Energy | Daño crudo | Daño visible |
|---|---|---:|---:|---:|
| `min-wizardry-summoner-base` | Summoner | 23 | 2.5555555555555555555555555556 | 2 |
| `min-wizardry-summoner-fraction-step` | Summoner | 26 | 2.8888888888888888888888888889 | 2 |
| `min-wizardry-summoner-integer-step` | Summoner | 27 | 3 | 3 |
| `min-wizardry-summoner-dimension-master-step` | Dimension Master | 29 | 3.2222222222222222222222222222 | 3 |
| `max-wizardry-summoner-base` | Summoner | 23 | 5.75 | 5 |
| `max-wizardry-summoner-fraction-step` | Summoner | 25 | 6.25 | 6 |
| `max-wizardry-summoner-integer-step` | Summoner | 24 | 6 | 6 |
| `max-wizardry-summoner-dimension-master-step` | Dimension Master | 27 | 6.75 | 6 |

## Casos aprobados

Cada fórmula enlaza cuatro positivos y dos controles: el stat por debajo de su
base (`formula-stat-below-base`) y una familia ajena (`formula-not-applicable`);
en total 16 positivos y 8 controles. No se crean controles de overflow: los
inputs válidos son no negativos y el coeficiente máximo es `1/4`, por lo que
aun un `INT64` máximo produce una salida menor que `Int64.MaxValue`. Sólo los
positivos están enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance, conflicto y cobertura. Application materializa la definición
genérica y resuelve el stat desde distribución; no requiere un input de nivel
para la fórmula. WPF reutiliza la selección genérica entre atributos derivados
y no contiene la expresión, sus constantes ni un handler por fórmula. El smoke
publicado debe reproducir los positivos en las fases inicial y de reemplazo.