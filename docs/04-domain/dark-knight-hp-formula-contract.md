# Contrato de HP de Dark Knight

## Identidad y autoridad

- Claim: `DR-HP-DARK-KNIGHT`, `VERIFIED`.
- Fórmula: `formula-hp-dark-knight` `1.0.0`.
- Contrato: `formula.schema.json` `2.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-knight` y las evoluciones
  `evolution-dark-knight`, `evolution-blade-knight` y
  `evolution-blade-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Vitality 25: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual nueva. Este contrato materializa únicamente
el claim ya aprobado y conserva por separado la calidad individual de sus
evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 25, `EVD-0021` |

La expresión autorizada es:

```text
hp = 35 + (character-level - 1) * 2 + vitality * 3
```

`CHECKED_INT64_V1` la representa como seis pasos ordenados: base, desplazamiento
de nivel, aporte de nivel, aporte de Vitality, resultado crudo y truncamiento
visible. Resta, multiplicaciones y suma usan aritmética comprobada. El
truncamiento final no altera estos casos enteros y no introduce redondeos
intermedios.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Vitality | HP |
|---|---|---:|---:|---:|
| `hp-dark-knight-base` | Dark Knight | 1 | 25 | 110 |
| `hp-dark-knight-level-step` | Dark Knight | 2 | 25 | 112 |
| `hp-dark-knight-vitality-step` | Dark Knight | 1 | 26 | 113 |
| `hp-dark-knight-combined-step` | Blade Master | 2 | 26 | 115 |

Los cuatro controles negativos cubren nivel 0, Vitality 24, familia ajena y
overflow de `vitality * 3`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica, resuelve
nivel y Vitality desde progresión/distribución y ejecuta por referencia exacta.
WPF obtiene la única fórmula publicada aplicable a la clase seleccionada; no
contiene la expresión ni sus constantes. El smoke publicado reproduce los
cuatro positivos en las fases inicial y de reemplazo.
