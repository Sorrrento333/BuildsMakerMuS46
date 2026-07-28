# Contrato de HP de Magic Gladiator

## Identidad y autoridad

- Claim: `DR-HP-MAGIC-GLADIATOR`, `VERIFIED`.
- Fórmula: `formula-hp-magic-gladiator` `1.0.0`.
- Contrato: `formula.schema.json` `2.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-magic-gladiator` y las evoluciones
  `evolution-magic-gladiator` y `evolution-duel-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Vitality 26: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; `DSP-0002` sólo afecta Energy y no
  es una entrada de esta fórmula, por lo que se conserva `conflictIds: []`.

No se añadió investigación factual nueva. Este contrato materializa únicamente
el claim ya aprobado y conserva por separado la calidad individual de sus
evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 26, `EVD-0021` |

La expresión autorizada es:

```text
hp = 58 + (character-level - 1) + vitality * 2
```

`CHECKED_INT64_V1` la representa como seis pasos ordenados: base, desplazamiento
de nivel, aporte de nivel, aporte de Vitality, resultado crudo y truncamiento
visible. Resta, multiplicaciones y suma usan aritmética comprobada. El
truncamiento final no altera estos casos enteros y no introduce redondeos
intermedios.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Vitality | HP |
|---|---|---:|---:|---:|
| `hp-magic-gladiator-base` | Magic Gladiator | 1 | 26 | 110 |
| `hp-magic-gladiator-level-step` | Magic Gladiator | 2 | 26 | 111 |
| `hp-magic-gladiator-vitality-step` | Magic Gladiator | 1 | 27 | 112 |
| `hp-magic-gladiator-combined-step` | Duel Master | 2 | 27 | 113 |

Los cuatro controles negativos cubren nivel 0, Vitality 25, familia ajena y
overflow de `vitality * 2`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica, resuelve
nivel y Vitality desde progresión/distribución y ejecuta por referencia exacta.
WPF obtiene la única fórmula publicada aplicable a la clase seleccionada; no
contiene la expresión ni sus constantes. El smoke publicado reproduce los
cuatro positivos en las fases inicial y de reemplazo.
