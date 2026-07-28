# Contrato de AG de Dark Wizard

## Identidad y autoridad

- Claim: `DR-AG-DARK-WIZARD`, `VERIFIED`.
- Fórmula: `formula-ag-dark-wizard` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-wizard`, `evolution-dark-wizard`,
  `evolution-soul-master` y `evolution-grand-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia de mínimos canónicos de stats: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 30, `EVD-0021` |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 15, `EVD-0021` |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 18, `EVD-0021` |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 18, `EVD-0021` |

La expresión autorizada es:

```text
ag = energy * 0.2 + vitality * 0.3 + agility * 0.4 + strength * 0.2
```

La fórmula no consume nivel ni otra fórmula derivada. `CHECKED_DECIMAL_V1`
conserva `0.2`, `0.3` y `0.4` exactamente en base 10. El programa declara seis
pasos ordenados: cuatro aportes de stat, resultado crudo y resultado visible.
La aritmética decimal es comprobada, no hay redondeos intermedios y
`visible-ag` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Evolución | ENE/VIT/AGI/STR | AG crudo | AG visible |
|---|---|---|---:|---:|
| `ag-dark-wizard-base` | Dark Wizard | `30/15/18/18` | 21.3 | 21 |
| `ag-dark-wizard-energy-vitality-step` | Soul Master | `31/16/18/18` | 21.8 | 21 |
| `ag-dark-wizard-agility-strength-step` | Soul Master | `30/15/19/19` | 21.9 | 21 |
| `ag-dark-wizard-combined-step` | Grand Master | `31/16/19/19` | 22.4 | 22 |

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
