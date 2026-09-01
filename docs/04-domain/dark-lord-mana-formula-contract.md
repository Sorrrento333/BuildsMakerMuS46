# Contrato de Mana de Dark Lord

## Identidad y autoridad

- Claim: `DR-MANA-DARK-LORD`, `VERIFIED`.
- Fórmula: `formula-mana-dark-lord` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-lord`, `evolution-dark-lord` y
  `evolution-lord-emperor`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Energy 15: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 15, `EVD-0021` |

La expresión autorizada es:

```text
mana = 40 + (character-level - 1) + (energy - 15) * 1.5
```

`CHECKED_DECIMAL_V1` conserva `1.5` exactamente en base 10. El programa declara
seis pasos ordenados: base, desplazamiento de nivel, desplazamiento y aporte de
Energy, resultado crudo y resultado visible. La aritmética decimal es
comprobada, no hay redondeos intermedios y `visible-mana` trunca hacia cero una
sola vez.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Energy | Mana crudo | Mana visible |
|---|---|---:|---:|---:|---:|
| `mana-dark-lord-base` | Dark Lord | 1 | 15 | 40 | 40 |
| `mana-dark-lord-level-step` | Dark Lord | 2 | 15 | 41 | 41 |
| `mana-dark-lord-energy-step` | Dark Lord | 1 | 16 | 41.5 | 41 |
| `mana-dark-lord-combined-step` | Lord Emperor | 2 | 16 | 42.5 | 42 |

Los cuatro controles negativos cubren nivel 0, Energy 14, familia ajena y
overflow al convertir la salida decimal visible a `INT64`. Sólo los cuatro
positivos están enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica,
resuelve nivel y Energy desde progresión/distribución y ejecuta por referencia
exacta. WPF reutiliza la selección genérica entre HP y Mana; no contiene la
expresión, sus constantes ni un handler por fórmula. El smoke publicado debe
reproducir los cuatro positivos en las fases inicial y de reemplazo.
