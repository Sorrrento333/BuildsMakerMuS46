# Contrato de AG de Summoner

## Identidad y autoridad

- Claim: `DR-AG-SUMMONER`, `VERIFIED`.
- Fórmula: `formula-ag-summoner` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-summoner`, `evolution-summoner`,
  `evolution-bloody-summoner` y `evolution-dimension-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia de mínimos canónicos de stats: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 21, `EVD-0021` |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 21, `EVD-0021` |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 18, `EVD-0021` |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

La expresión autorizada es:

```text
ag = strength * 0.2 + agility * 0.25 + vitality * 0.3 + energy * 0.15
```

La fórmula no consume nivel ni otra fórmula derivada. `CHECKED_DECIMAL_V1`
conserva `0.2`, `0.25`, `0.3` y `0.15` exactamente en base 10. El programa
declara seis pasos ordenados: cuatro aportes de stat, resultado crudo y
resultado visible. La aritmética decimal es comprobada, no hay redondeos
intermedios y `visible-ag` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Evolución | STR/AGI/VIT/ENE | AG crudo | AG visible |
|---|---|---|---:|---:|
| `ag-summoner-base` | Summoner | `21/21/18/23` | 18.30 | 18 |
| `ag-summoner-strength-agility-step` | Bloody Summoner | `22/22/18/23` | 18.75 | 18 |
| `ag-summoner-vitality-energy-step` | Bloody Summoner | `21/21/19/24` | 18.75 | 18 |
| `ag-summoner-combined-step` | Dimension Master | `22/22/19/24` | 19.20 | 19 |

Los cinco controles negativos cubren cada uno de los cuatro stats por debajo
de su base y una familia ajena. No se crea un control de overflow imposible:
los inputs válidos son no negativos y la suma de coeficientes es `0.9`, por lo
que aun cuatro valores `INT64` máximos producen una salida menor que
`Int64.MaxValue`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica y
resuelve los cuatro stats desde distribución; no requiere un input de nivel
para la fórmula. WPF reutiliza la selección genérica entre atributos derivados
y no contiene la expresión, sus constantes ni un handler por fórmula. El smoke
publicado debe reproducir los cuatro positivos en las fases inicial y de
reemplazo.
