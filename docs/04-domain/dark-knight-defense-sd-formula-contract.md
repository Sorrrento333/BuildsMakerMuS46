# Contrato de Defense y SD de Dark Knight

## Identidad y autoridad

- Claims: Defense adicional preservada por `EVD-0026` y
  `DR-SD-DARK-KNIGHT`, `VERIFIED`.
- Fórmulas: `formula-defense-dark-knight` `1.0.0` y
  `formula-sd-dark-knight` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: ambas `PUBLISHED`.
- Aplicabilidad: `class-dark-knight`, `evolution-dark-knight`,
  `evolution-blade-knight` y `evolution-blade-master`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Etapa de dependencia: `EVD-0034` fija Defense `RAW`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

`EVD-0034` decide `RAW` para las cinco familias de SD que estaban pendientes.
Una revisión futura requiere nueva evidencia, nueva versión de fórmula y casos
que hagan observable el cambio; no modifica silenciosamente `1.0.0`.

## Defense

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 20, `EVD-0021` |

La expresión autorizada es `defense = agility / 3`.
`CHECKED_DECIMAL_V1` usa `DIVIDE` con el literal exacto 3, conserva el cociente
decimal en `raw-defense` y trunca hacia cero una sola vez en
`visible-defense`.

| Caso | Evolución | Agility | Defense raw | Defense visible |
|---|---|---:|---:|---:|
| `defense-dark-knight-base` | Dark Knight | 20 | 6.666666… | 6 |
| `defense-dark-knight-integer-step` | Blade Knight | 21 | 7 | 7 |
| `defense-dark-knight-fraction-step` | Blade Knight | 22 | 7.333333… | 7 |
| `defense-dark-knight-blade-master-step` | Blade Master | 23 | 7.666666… | 7 |

Los dos controles negativos cubren Agility bajo base y familia ajena. No se
inventa overflow: dividir cualquier `INT64` no negativo admitido por 3 mantiene
la salida visible dentro de `INT64`.

## SD

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 28 |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 20 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 25 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 10 |
| `defense` | `DECIMAL` | `FORMULA_OUTPUT/formula-defense-dark-knight@1.0.0/RAW` | mínimo técnico 0 |

La expresión autorizada es:

```text
sd = (strength + agility + vitality + energy) * 1.2
   + defense / 2
   + (level * level) / 30
```

El programa conserva el orden escrito en siete pasos: suma de stats, aporte
`× 1.2`, aporte de Defense `÷ 2`, cuadrado del nivel, aporte `÷ 30`, SD crudo
y SD visible. No existe truncamiento intermedio; `visible-sd` trunca el total
hacia cero una sola vez.

| Caso | Evolución | Nivel | STR/AGI/VIT/ENE | Defense raw | SD raw | SD visible |
|---|---|---:|---|---:|---:|---:|
| `sd-dark-knight-base` | Dark Knight | 1 | `28/20/25/10` | 6.666666… | 102.966666… | 102 |
| `sd-dark-knight-level-step` | Blade Knight | 2 | `28/20/25/10` | 6.666666… | 103.066666… | 103 |
| `sd-dark-knight-raw-defense-boundary` | Blade Knight | 1 | `28/23/25/10` | 7.666666… | 107.066666… | 107 |
| `sd-dark-knight-combined-step` | Blade Master | 5 | `29/21/26/11` | 7 | 108.733333… | 108 |

El caso de frontera hace observable la decisión: con Defense `VISIBLE=7`, el
mismo resto produciría SD visible 106; con Defense `RAW=7.666666…`, el
resultado aprobado es 107. Los siete controles negativos cubren nivel, cada
stat bajo su base, familia y overflow de la salida `INT64`.

## Integración

Application resuelve ambas fórmulas desde el snapshot. Para SD ejecuta Defense
una vez sobre el mismo estado validado, selecciona `RawOutput`, conserva la
traza productora y entrega el decimal al intérprete consumidor. WPF mantiene
selección genérica por referencia exacta y muestra las trazas contextual, de
dependencia y aritmética sin handlers ni constantes factuales en C#.

El gate estructural valida identidades, inputs, aridades, dependencia y casos.
Las pruebas de Application reproducen los casos directos y la composición
snapshot → progresión → distribución → Defense RAW → SD. El smoke publicado
debe repetir los ocho positivos en las fases inicial y de reemplazo.
