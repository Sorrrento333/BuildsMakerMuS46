# Contrato de Defense y SD de Summoner

## Identidad y autoridad

- Claims: Defense y SD de Summoner preservadas por `EVD-0032` y
  `DR-SD-SUMMONER`, `VERIFIED`.
- Fórmulas: `formula-defense-summoner` `1.0.0` y `formula-sd-summoner` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: ambas `PUBLISHED`.
- Aplicabilidad: `class-summoner`, `evolution-summoner`,
  `evolution-bloody-summoner` y `evolution-dimension-master`.
- Evidencia de stats mínimos: `EVD-0021` (STR 21, AGI 21, VIT 18, ENE 23; sin
  Command).
- Evidencia de expresiones, alcance y truncamiento: `EVD-0032`.
- Etapa de dependencia: `EVD-0034` fija Defense `RAW`.
- Conflicto aplicable: `DSP-0004` sólo en SD porque consume Stat; permanece
  resuelto y se conserva trazado resuelto en `conflictIds`.

## Defense

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 21, `EVD-0021` |

La expresión autorizada es `defense = agility / 3`. El programa conserva el
cociente decimal como salida cruda y trunca hacia cero una sola vez para la
salida visible.

| Caso | Evolución | Agility | Defense raw | Defense visible |
|---|---|---|---:|---:|---:|
| `defense-summoner-base` | Summoner | 21 | 7 | 7 |
| `defense-summoner-fraction-step` | Summoner | 22 | 7.333333… | 7 |
| `defense-summoner-integer-step` | Summoner | 24 | 8 | 8 |
| `defense-summoner-dimension-master-step` | Dimension Master | 25 | 8.333333… | 8 |

Los controles negativos cubren Agility bajo base y familia ajena. Dividir un
`INT64` no negativo por 3 no puede desbordar la salida visible `INT64`.

## SD

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 21 |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 21 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 18 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23 |
| `defense` | `DECIMAL` | `FORMULA_OUTPUT/formula-defense-summoner@1.0.0/RAW` | mínimo técnico 0 |

La expresión autorizada es:

```text
sd = trunc((strength + agility + vitality + energy) * 1.2)
   + trunc(defense / 2)
   + trunc((level * level) / 30)
```

`EVD-0032` exige **tres truncamientos independientes** hacia cero antes de
sumar: el aporte de stats, el de Defense y el de nivel se truncan por separado;
sólo después se suman y `visible-sd` trunca el total una vez más. Esto difiere
de Magic Gladiator, cuyo SD no trunca aportes intermedios. El caso base
discrimina la semántica: truncar cada aporte antes de sumar da 99+3+0 = **102**,
mientras que truncar el total de precisión completa daría 103.

| Caso | Evolución | Nivel | STR/AGI/VIT/ENE | Defense raw | SD raw | SD visible |
|---|---|---|---:|---|---:|---:|---:|
| `sd-summoner-base` | Summoner | 1 | `21/21/18/23` | 7 | 102 | 102 |
| `sd-summoner-level-step` | Summoner | 6 | `21/21/18/23` | 7 | 103 | 103 |
| `sd-summoner-stat-step` | Bloody Summoner | 10 | `22/22/19/24` | 7.333333… | 110 | 110 |
| `sd-summoner-dimension-master-step` | Dimension Master | 15 | `21/21/18/23` | 7 | 109 | 109 |

No existe ningún caso de frontera Defense RAW-vs-VISIBLE en Summoner: como
`trunc(RAW/2) == trunc(VISIBLE/2)` se cumple siempre para esta familia (la
frontera Defense salta en enteros y los restos fraccionarios se conservan al
mitad), no se fabrica uno. Los siete controles negativos cubren nivel, cada
stat bajo base, familia y overflow.

## Integración

Application resuelve la dependencia por referencia exacta sobre el mismo estado
validado y entrega la salida `RAW` a SD. Supporta múltiples pasos
`APPLY_ROUNDING` intermedios (aportes) con la salida visible como último paso.
WPF y el smoke reutilizan el recorrido genérico, sin handlers ni constantes
factuales de Summoner en C#.
