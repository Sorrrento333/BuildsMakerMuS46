# Contrato de Mana de Fairy Elf

## Identidad y autoridad

- Claim: `DR-MANA-FAIRY-ELF`, `VERIFIED`.
- Fórmula: `formula-mana-fairy-elf` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-fairy-elf` y las evoluciones
  `evolution-fairy-elf`, `evolution-muse-elf` y `evolution-high-elf`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Energy 15: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual nueva. Este contrato materializa únicamente
el claim ya aprobado y conserva por separado la calidad individual de sus
evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 15, `EVD-0021` |

La expresión autorizada es:

```text
mana = 15 + (character-level - 1) * 1.5 + energy
```

`CHECKED_DECIMAL_V1` conserva `1.5` exactamente en base 10. El programa declara
seis pasos ordenados: base, desplazamiento de nivel, aporte de nivel, aporte de
Energy, resultado crudo y resultado visible. La multiplicación por uno hace
explícito el aporte de Energy en la traza sin cambiar la expresión. La
aritmética decimal es comprobada, no hay redondeos intermedios y
`visible-mana` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Energy | Mana crudo | Mana visible |
|---|---|---:|---:|---:|---:|
| `mana-fairy-elf-base` | Fairy Elf | 1 | 15 | 30 | 30 |
| `mana-fairy-elf-level-step` | Fairy Elf | 2 | 15 | 31.5 | 31 |
| `mana-fairy-elf-energy-step` | Fairy Elf | 1 | 16 | 31 | 31 |
| `mana-fairy-elf-combined-step` | High Elf | 2 | 16 | 32.5 | 32 |

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
