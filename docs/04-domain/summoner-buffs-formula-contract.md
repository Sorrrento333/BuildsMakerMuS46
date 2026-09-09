# Contrato de Buffs de Summoner

## Identidad y autoridad

- Fórmulas: `formula-reflect-percent-summoner` `1.0.0`,
  `formula-berserker-percent-summoner` `1.0.0`,
  `formula-innovation-percent-summoner` `1.0.0` y
  `formula-weakness-percent-summoner` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: las cuatro `PUBLISHED`.
- Aplicabilidad: `class-summoner`, `evolution-summoner`,
  `evolution-bloody-summoner` y `evolution-dimension-master`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Conflicto aplicable: ninguno conocido; la traza conserva `conflictIds: []`.

Las expresiones de buffs se conservan como axiomas del ruleset fuera del
inventario de 24 claims de `RES-0002`. No se añadió evidencia, fórmula ni
claim, y no se reclasificó ninguna fuente: este contrato materializa
únicamente el catálogo aprobado en `EVD-0026`.

## Reflect

`formula-reflect-percent-summoner` expresa `reflect_percent = 30 + ene / 42`,
conserva el término de Energy como paso crudo y trunca hacia cero una sola vez
para la salida visible.

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

| Caso | Evolución | Energy | Reflect crudo | Reflect visible |
|---|---:|---:|---:|
| `reflect-percent-summoner-base` | Summoner | 23 | 30.547619047619047619047619048 | 30 |
| `reflect-percent-summoner-fraction-step` | Summoner | 25 | 30.595238095238095238095238095 | 30 |
| `reflect-percent-summoner-integer-step` | Summoner | 42 | 31 | 31 |
| `reflect-percent-summoner-dimension-master-step` | Dimension Master | 28 | 30.666666666666666666666666667 | 30 |

## Berserker

`formula-berserker-percent-summoner` expresa `berserker_percent = ene / 30` y
trunca hacia cero una sola vez para la salida visible, sin término constante.

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

| Caso | Evolución | Energy | Berserker crudo | Berserker visible |
|---|---:|---:|---:|
| `berserker-percent-summoner-base` | Summoner | 23 | 0.7666666666666666666666666667 | 0 |
| `berserker-percent-summoner-fraction-step` | Summoner | 25 | 0.8333333333333333333333333333 | 0 |
| `berserker-percent-summoner-integer-step` | Summoner | 30 | 1 | 1 |
| `berserker-percent-summoner-dimension-master-step` | Dimension Master | 28 | 0.9333333333333333333333333333 | 0 |

## Innovation

`formula-innovation-percent-summoner` expresa `innovation_percent = ene / 90 + 20`,
conserva el término de Energy como paso crudo y trunca hacia cero una sola vez
para la salida visible.

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

| Caso | Evolución | Energy | Innovation crudo | Innovation visible |
|---|---:|---:|---:|
| `innovation-percent-summoner-base` | Summoner | 23 | 20.255555555555555555555555556 | 20 |
| `innovation-percent-summoner-fraction-step` | Summoner | 25 | 20.277777777777777777777777778 | 20 |
| `innovation-percent-summoner-integer-step` | Summoner | 90 | 21 | 21 |
| `innovation-percent-summoner-dimension-master-step` | Dimension Master | 28 | 20.311111111111111111111111111 | 20 |

## Weakness

`formula-weakness-percent-summoner` expresa `weakness_percent = ene / 65 + 7`,
conserva el término de Energy como paso crudo y trunca hacia cero una sola vez
para la salida visible.

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

| Caso | Evolución | Energy | Weakness crudo | Weakness visible |
|---|---:|---:|---:|
| `weakness-percent-summoner-base` | Summoner | 23 | 7.3538461538461538461538461538 | 7 |
| `weakness-percent-summoner-fraction-step` | Summoner | 25 | 7.3846153846153846153846153846 | 7 |
| `weakness-percent-summoner-integer-step` | Summoner | 65 | 8 | 8 |
| `weakness-percent-summoner-dimension-master-step` | Dimension Master | 28 | 7.4307692307692307692307692308 | 7 |

## Casos aprobados

Cada fórmula enlaza cuatro positivos y dos controles: el stat por debajo de su
base (`formula-stat-below-base`, Energy 22) y una familia ajena
(`formula-not-applicable`, Dark Wizard); en total 16 positivos y 8 controles.
No se crean controles de overflow: los inputs válidos son no negativos y el
coeficiente máximo es `1/30`, por lo que aun un `INT64` máximo produce una
salida acotada por la constante y ningún overflow. Sólo los positivos están
enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance, conflicto y cobertura. Application materializa la definición
genérica y resuelve el stat desde distribución; no requiere un input de nivel
para la fórmula. WPF reutiliza la selección genérica entre atributos derivados
y no contiene la expresión, sus constantes ni un handler por fórmula. El smoke
publicado debe reproducir los positivos en las fases inicial y de reemplazo.