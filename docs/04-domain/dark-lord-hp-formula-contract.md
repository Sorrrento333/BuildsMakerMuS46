# Contrato de HP de Dark Lord

## Identidad y autoridad

- Claim: `DR-HP-DARK-LORD`, `VERIFIED`.
- Fórmula: `formula-hp-dark-lord` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-lord`, `evolution-dark-lord` y
  `evolution-lord-emperor`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Vitality 20: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual ni se reclasificó ninguna fuente. Este
contrato materializa únicamente el claim aprobado y conserva la clasificación
individual de sus evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 20, `EVD-0021` |

La expresión autorizada es:

```text
hp = 50 + (character-level - 1) * 1.5 + vitality * 2
```

El coeficiente `1.5` es exacto en base 10. `CHECKED_INT64_V1` no puede
representar el aporte fraccionario sin pérdida, por lo que el contrato
ejecutable avanza de forma compatible a `2.1.0` y la fórmula usa
`CHECKED_DECIMAL_V1`. Sus seis pasos conservan exactamente base, desplazamiento
de nivel, aporte de nivel, aporte de Vitality, resultado crudo y salida visible.
No se redondea ningún aporte: `TRUNCATE` se aplica una sola vez sobre `raw-hp`.
La salida publicada sigue siendo `INT64`.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Vitality | HP crudo | HP visible |
|---|---|---:|---:|---:|---:|
| `hp-dark-lord-base` | Dark Lord | 1 | 20 | 90 | 90 |
| `hp-dark-lord-level-step` | Dark Lord | 2 | 20 | 91.5 | 91 |
| `hp-dark-lord-vitality-step` | Dark Lord | 1 | 21 | 92 | 92 |
| `hp-dark-lord-combined-step` | Lord Emperor | 2 | 21 | 93.5 | 93 |

Los cuatro controles negativos cubren nivel 0, Vitality 19, familia ajena y
salida fuera de `INT64`. Sólo los positivos se enlazan desde `testCaseRefs`.

## Integración

El schema y el gate semántico validan identidad, ejecución decimal, catálogo,
inputs, programa, traza, provenance y cobertura. Domain conserva valores de
traza `decimal`; Calculation Engine ejecuta el programa sin handlers factuales;
Application selecciona `CHECKED_INT64_V1` o `CHECKED_DECIMAL_V1` desde el
programa materializado. La resolución de nivel y Vitality, WPF y el smoke
reutilizan la ruta genérica existente y no contienen constantes de Dark Lord.
