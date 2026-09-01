# Contrato de HP de Fairy Elf

## Identidad y autoridad

- Claim: `DR-HP-FAIRY-ELF`, `VERIFIED`.
- Fórmula: `formula-hp-fairy-elf` `1.0.0`.
- Contrato: `formula.schema.json` `2.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-fairy-elf` y las evoluciones
  `evolution-fairy-elf`, `evolution-muse-elf` y `evolution-high-elf`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Vitality 20: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual nueva. Este contrato materializa únicamente
el claim ya aprobado y conserva por separado la calidad individual de sus
evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 20, `EVD-0021` |

La expresión autorizada es:

```text
hp = 40 + (character-level - 1) + vitality * 2
```

`CHECKED_INT64_V1` la representa como seis pasos ordenados: base, desplazamiento
de nivel, aporte de nivel, aporte de Vitality, resultado crudo y truncamiento
visible. Resta, multiplicaciones y suma usan aritmética comprobada. El
truncamiento final no altera estos casos enteros y no introduce redondeos
intermedios.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Vitality | HP |
|---|---|---:|---:|---:|
| `hp-fairy-elf-base` | Fairy Elf | 1 | 20 | 80 |
| `hp-fairy-elf-level-step` | Fairy Elf | 2 | 20 | 81 |
| `hp-fairy-elf-vitality-step` | Fairy Elf | 1 | 21 | 82 |
| `hp-fairy-elf-combined-step` | High Elf | 2 | 21 | 83 |

Los cuatro controles negativos cubren nivel 0, Vitality 19, familia ajena y
overflow de `vitality * 2`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica, resuelve
nivel y Vitality desde progresión/distribución y ejecuta por referencia exacta.
WPF obtiene la única fórmula publicada aplicable a la clase seleccionada; no
contiene la expresión ni sus constantes. El smoke publicado reproduce los
cuatro positivos en las fases inicial y de reemplazo.
