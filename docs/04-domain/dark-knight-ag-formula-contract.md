# Contrato de AG de Dark Knight

## Identidad y autoridad

- Claim: `DR-AG-DARK-KNIGHT`, `VERIFIED`.
- Fórmula: `formula-ag-dark-knight` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-knight`, `evolution-dark-knight`,
  `evolution-blade-knight` y `evolution-blade-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia de mínimos canónicos de stats: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 10, `EVD-0021` |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 25, `EVD-0021` |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 20, `EVD-0021` |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 28, `EVD-0021` |

La expresión autorizada es:

```text
ag = energy + vitality * 0.3 + agility * 0.2 + strength * 0.15
```

La fórmula no consume nivel ni otra fórmula derivada. `CHECKED_DECIMAL_V1`
conserva `0.3`, `0.2` y `0.15` exactamente en base 10. El programa declara cinco
pasos ordenados: tres aportes multiplicados, resultado crudo —que consume
`energy` directamente— y resultado visible.
La aritmética decimal es comprobada, no hay redondeos intermedios y
`visible-ag` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Evolución | ENE/VIT/AGI/STR | AG crudo | AG visible |
|---|---|---|---:|---:|
| `ag-dark-knight-base` | Dark Knight | `10/25/20/28` | 25.70 | 25 |
| `ag-dark-knight-energy-vitality-step` | Blade Knight | `11/26/20/28` | 27.00 | 27 |
| `ag-dark-knight-agility-strength-step` | Blade Knight | `10/25/21/29` | 26.05 | 26 |
| `ag-dark-knight-combined-step` | Blade Master | `11/26/21/29` | 27.35 | 27 |

Los seis controles negativos cubren cada uno de los cuatro stats por debajo de
su base, una familia ajena y overflow al convertir la salida decimal visible a
`INT64`. Sólo los cuatro positivos están enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica y
resuelve los cuatro stats desde distribución; no requiere un input de nivel
para la fórmula. WPF reutiliza la selección genérica entre atributos derivados
y no contiene la expresión, sus constantes ni un handler por fórmula. El smoke
publicado debe reproducir los cuatro positivos en las fases inicial y de
reemplazo.
