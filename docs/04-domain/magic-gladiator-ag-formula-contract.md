# Contrato de AG de Magic Gladiator

## Identidad y autoridad

- Claim: `DR-AG-MAGIC-GLADIATOR`, `VERIFIED`.
- Fórmula: `formula-ag-magic-gladiator` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-magic-gladiator`, `evolution-magic-gladiator` y
  `evolution-duel-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia de mínimos canónicos de stats: `EVD-0021`.
- Conflicto aplicable: `DSP-0002`, resuelto por decisión del propietario a favor
  de Energy 26; la guía actual de Webzen que publica 16 conserva su
  clasificación y no se reescribe.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 26, `EVD-0021`/`DSP-0002` |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 26, `EVD-0021` |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 26, `EVD-0021` |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 26, `EVD-0021` |

La expresión autorizada es:

```text
ag = energy * 0.15 + vitality * 0.3 + agility * 0.25 + strength * 0.2
```

La fórmula no consume nivel ni otra fórmula derivada. `CHECKED_DECIMAL_V1`
conserva `0.15`, `0.3`, `0.25` y `0.2` exactamente en base 10. El programa
declara seis pasos ordenados: cuatro aportes de stat, resultado crudo y
resultado visible. La aritmética decimal es comprobada, no hay redondeos
intermedios y `visible-ag` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Evolución | ENE/VIT/AGI/STR | AG crudo | AG visible |
|---|---|---|---:|---:|
| `ag-magic-gladiator-base` | Magic Gladiator | `26/26/26/26` | 23.40 | 23 |
| `ag-magic-gladiator-energy-vitality-step` | Duel Master | `27/27/26/26` | 23.85 | 23 |
| `ag-magic-gladiator-agility-strength-step` | Duel Master | `26/26/27/27` | 23.85 | 23 |
| `ag-magic-gladiator-combined-step` | Duel Master | `27/27/27/27` | 24.30 | 24 |

Los cinco controles negativos cubren cada uno de los cuatro stats por debajo
de su base y una familia ajena. No se crea un control de overflow imposible:
los inputs válidos son no negativos y la suma de coeficientes es `0.9`, por lo
que aun cuatro valores `INT64` máximos producen una salida menor que
`Int64.MaxValue`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance, conflicto y cobertura. Application materializa la definición
genérica y resuelve los cuatro stats desde distribución; no requiere un input
de nivel para la fórmula. WPF reutiliza la selección genérica entre atributos
derivados y no contiene la expresión, sus constantes ni un handler por fórmula.
El smoke publicado debe reproducir los cuatro positivos en las fases inicial y
de reemplazo.
