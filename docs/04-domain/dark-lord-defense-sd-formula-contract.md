# Contrato de Defense y SD de Dark Lord

## Identidad y autoridad

- Claims: Defense adicional preservada por `EVD-0026` y
  `DR-SD-DARK-LORD`, `VERIFIED`.
- Fórmulas: `formula-defense-dark-lord` `1.0.0` y
  `formula-sd-dark-lord` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: ambas `PUBLISHED`.
- Aplicabilidad: `class-dark-lord`, `evolution-dark-lord` y
  `evolution-lord-emperor`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Etapa de dependencia: `EVD-0034` fija Defense `RAW`.
- Conflictos aplicables: ninguno.

## Defense

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 20, `EVD-0021` |

La expresión autorizada es `defense = agility / 7`. El programa conserva el
cociente decimal como salida cruda y trunca hacia cero una sola vez para la
salida visible.

| Caso | Evolución | Agility | Defense raw | Defense visible |
|---|---|---:|---:|---:|
| `defense-dark-lord-base` | Dark Lord | 20 | 2.857142857… | 2 |
| `defense-dark-lord-fraction-step` | Dark Lord | 22 | 3.142857142… | 3 |
| `defense-dark-lord-integer-step` | Dark Lord | 28 | 4 | 4 |
| `defense-dark-lord-lord-emperor-step` | Lord Emperor | 29 | 4.142857142… | 4 |

Los controles negativos cubren Agility bajo base y familia ajena. Dividir un
`INT64` no negativo por 7 no puede desbordar la salida visible `INT64`.

## SD

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 26 |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 20 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 20 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 15 |
| `command` | `INT64` | `CONTEXT_VALUE/resolved-command` | mínimo factual 25 |
| `defense` | `DECIMAL` | `FORMULA_OUTPUT/formula-defense-dark-lord@1.0.0/RAW` | mínimo técnico 0 |

La expresión autorizada es:

```text
sd = (strength + agility + vitality + energy + command) * 1.2
   + defense / 2
   + (level * level) / 30
```

No existe truncamiento intermedio. Los aportes por stats, Defense y nivel se
conservan como decimales exactos y `visible-sd` trunca el total una sola vez.

| Caso | Evolución | Nivel | STR/AGI/VIT/ENE/CMD | Defense raw | SD raw | SD visible |
|---|---|---:|---|---:|---:|---:|
| `sd-dark-lord-base` | Dark Lord | 1 | `26/20/20/15/25` | 2.857142857… | 128.661904761… | 128 |
| `sd-dark-lord-level-step` | Dark Lord | 5 | `26/20/20/15/25` | 2.857142857… | 129.461904761… | 129 |
| `sd-dark-lord-raw-defense-boundary` | Dark Lord | 1 | `26/25/20/15/25` | 3.571428571… | 135.019047619… | 135 |
| `sd-dark-lord-combined-step` | Lord Emperor | 5 | `27/21/21/16/26` | 3 | 135.533333333… | 135 |

El caso de frontera distingue la etapa aprobada: con Defense visible 3, el
mismo resto produciría SD visible 134; con Defense cruda 3.571428571… produce
135. Los ocho controles negativos cubren nivel, los cinco stats bajo base,
familia y overflow.

## Integración

Application resuelve la dependencia por referencia exacta sobre el mismo estado
validado y entrega la salida `RAW` a SD. La ruta contextual genérica resuelve
`command` desde la clase materializada. WPF y el smoke reutilizan ese recorrido,
sin handlers ni constantes factuales de Dark Lord en C#.
