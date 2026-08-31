# Contrato de Defense y SD de Magic Gladiator

## Identidad y autoridad

- Claims: Defense adicional preservada por `EVD-0026` y
  `DR-SD-MAGIC-GLADIATOR`, `VERIFIED`.
- Fórmulas: `formula-defense-magic-gladiator` `1.0.0` y
  `formula-sd-magic-gladiator` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: ambas `PUBLISHED`.
- Aplicabilidad: `class-magic-gladiator`, `evolution-magic-gladiator` y
  `evolution-duel-master`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Etapa de dependencia: `EVD-0034` fija Defense `RAW`.
- Conflicto aplicable: `DSP-0002` sólo en SD porque consume Energy; permanece
  resuelto a favor de Energy 26.

## Defense

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 26, `EVD-0021` |

La expresión autorizada es `defense = agility / 5`. El programa conserva el
cociente decimal como salida cruda y trunca hacia cero una sola vez para la
salida visible.

| Caso | Evolución | Agility | Defense raw | Defense visible |
|---|---|---:|---:|---:|
| `defense-magic-gladiator-base` | Magic Gladiator | 26 | 5.2 | 5 |
| `defense-magic-gladiator-fraction-step` | Magic Gladiator | 27 | 5.4 | 5 |
| `defense-magic-gladiator-integer-step` | Magic Gladiator | 30 | 6 | 6 |
| `defense-magic-gladiator-duel-master-step` | Duel Master | 31 | 6.2 | 6 |

Los controles negativos cubren Agility bajo base y familia ajena. Dividir un
`INT64` no negativo por 5 no puede desbordar la salida visible `INT64`.

## SD

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 26 |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 26 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 26 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 26 |
| `defense` | `DECIMAL` | `FORMULA_OUTPUT/formula-defense-magic-gladiator@1.0.0/RAW` | mínimo técnico 0 |

La expresión autorizada es:

```text
sd = (strength + agility + vitality + energy) * 1.2
   + defense / 2
   + (level * level) / 30
```

No existe truncamiento intermedio. Los aportes por stats, Defense y nivel se
conservan como decimales exactos y `visible-sd` trunca el total una sola vez.

| Caso | Evolución | Nivel | STR/AGI/VIT/ENE | Defense raw | SD raw | SD visible |
|---|---|---:|---|---:|---:|---:|
| `sd-magic-gladiator-base` | Magic Gladiator | 1 | `26/26/26/26` | 5.2 | 127.433333… | 127 |
| `sd-magic-gladiator-level-step` | Magic Gladiator | 5 | `26/26/26/26` | 5.2 | 128.233333… | 128 |
| `sd-magic-gladiator-raw-defense-boundary` | Magic Gladiator | 1 | `26/28/26/26` | 5.6 | 130.033333… | 130 |
| `sd-magic-gladiator-combined-step` | Duel Master | 5 | `27/27/27/27` | 5.4 | 133.133333… | 133 |

El caso de frontera distingue la etapa aprobada: con Defense visible 5, el
mismo resto produciría SD visible 129; con Defense cruda 5.6 produce 130. Los
siete controles negativos cubren nivel, cada stat bajo base, familia y overflow.

## Integración

Application resuelve la dependencia por referencia exacta sobre el mismo estado
validado y entrega la salida `RAW` a SD. WPF y el smoke reutilizan el recorrido
genérico, sin handlers ni constantes factuales de Magic Gladiator en C#.
