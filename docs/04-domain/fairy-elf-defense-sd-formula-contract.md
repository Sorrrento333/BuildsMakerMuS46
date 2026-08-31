# Contrato de Defense y SD de Fairy Elf

## Identidad y autoridad

- Claims: Defense adicional preservada por `EVD-0026` y
  `DR-SD-FAIRY-ELF`, `VERIFIED`.
- Fórmulas: `formula-defense-fairy-elf` `1.0.0` y
  `formula-sd-fairy-elf` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: ambas `PUBLISHED`.
- Aplicabilidad: `class-fairy-elf`, `evolution-fairy-elf`,
  `evolution-muse-elf` y `evolution-high-elf`.
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
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 25, `EVD-0021` |

La expresión autorizada es `defense = agility / 10`.
`CHECKED_DECIMAL_V1` usa `DIVIDE` con el literal exacto 10, conserva el cociente
decimal en `raw-defense` y trunca hacia cero una sola vez en
`visible-defense`.

| Caso | Evolución | Agility | Defense raw | Defense visible |
|---|---|---:|---:|---:|
| `defense-fairy-elf-base` | Fairy Elf | 25 | 2.5 | 2 |
| `defense-fairy-elf-fraction-step` | Muse Elf | 26 | 2.6 | 2 |
| `defense-fairy-elf-integer-step` | Muse Elf | 30 | 3 | 3 |
| `defense-fairy-elf-high-elf-step` | High Elf | 35 | 3.5 | 3 |

Los dos controles negativos cubren Agility bajo base y familia ajena. No se
inventa overflow: dividir cualquier `INT64` no negativo admitido por 10 mantiene
la salida visible dentro de `INT64`.

## SD

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 22 |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 25 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 20 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 15 |
| `defense` | `DECIMAL` | `FORMULA_OUTPUT/formula-defense-fairy-elf@1.0.0/RAW` | mínimo técnico 0 |

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
| `sd-fairy-elf-base` | Fairy Elf | 1 | `22/25/20/15` | 2.5 | 99.683333… | 99 |
| `sd-fairy-elf-level-step` | Muse Elf | 4 | `22/25/20/15` | 2.5 | 100.183333… | 100 |
| `sd-fairy-elf-raw-defense-boundary` | Muse Elf | 1 | `22/27/20/15` | 2.7 | 102.183333… | 102 |
| `sd-fairy-elf-combined-step` | High Elf | 5 | `23/26/21/16` | 2.6 | 105.333333… | 105 |

El caso de frontera hace observable la decisión: con Defense `VISIBLE=2`, el
mismo resto produciría SD visible 101; con Defense `RAW=2.7`, el resultado
aprobado es 102. Los siete controles negativos cubren nivel, cada stat bajo su
base, familia y overflow de la salida `INT64`.

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
