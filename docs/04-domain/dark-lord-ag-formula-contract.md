# Contrato de AG de Dark Lord

## Identidad y autoridad

- Claim: `DR-AG-DARK-LORD`, `VERIFIED`.
- Fórmula: `formula-ag-dark-lord` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-lord`, `evolution-dark-lord` y
  `evolution-lord-emperor`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia de mínimos canónicos de stats: `EVD-0021`.
- Conflictos aplicables: ninguno documentado.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 15, `EVD-0021` |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 20, `EVD-0021` |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 20, `EVD-0021` |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 26, `EVD-0021` |
| `command` | `INT64` | `CONTEXT_VALUE/resolved-command` | mínimo factual 25, `EVD-0021` |

La expresión autorizada es:

```text
ag = energy * 0.15 + vitality * 0.1 + agility * 0.2 + strength * 0.3 + command * 0.3
```

La fórmula no consume nivel ni otra fórmula derivada. `CHECKED_DECIMAL_V1`
conserva `0.15`, `0.1`, `0.2` y `0.3` exactamente en base 10. El programa
declara siete pasos ordenados: cinco aportes de stat, resultado crudo y
resultado visible. La aritmética decimal es comprobada, no hay redondeos
intermedios y `visible-ag` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Evolución | ENE/VIT/AGI/STR/CMD | AG crudo | AG visible |
|---|---|---|---:|---:|
| `ag-dark-lord-base` | Dark Lord | `15/20/20/26/25` | 23.55 | 23 |
| `ag-dark-lord-energy-vitality-step` | Lord Emperor | `16/21/20/26/25` | 23.80 | 23 |
| `ag-dark-lord-agility-strength-command-step` | Lord Emperor | `15/20/21/27/26` | 24.35 | 24 |
| `ag-dark-lord-combined-step` | Lord Emperor | `16/21/21/27/26` | 24.60 | 24 |

Los siete controles negativos cubren cada uno de los cinco stats por debajo de
su base, una familia ajena y overflow de la salida `INT64`. El overflow es
alcanzable porque la suma de coeficientes es `1.05`: cinco entradas
`Int64.MaxValue` producen un resultado crudo superior a `Int64.MaxValue`.
Sólo los cuatro positivos están enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica y
resuelve los cinco stats desde distribución; `command` usa la ruta contextual
genérica ya existente. WPF reutiliza la selección genérica entre atributos
derivados y no contiene la expresión, sus constantes ni un handler por fórmula.
El smoke publicado debe reproducir los cuatro positivos en las fases inicial y
de reemplazo.
