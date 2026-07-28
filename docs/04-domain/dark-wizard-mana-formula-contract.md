# Contrato de Mana de Dark Wizard

## Identidad y autoridad

- Claim: `DR-MANA-DARK-WIZARD`, `VERIFIED`.
- Fórmula: `formula-mana-dark-wizard` `1.0.0`.
- Contrato: `formula.schema.json` `2.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-dark-wizard` y las evoluciones
  `evolution-dark-wizard`, `evolution-soul-master` y
  `evolution-grand-master`.
- Evidencia de expresión, alcance y truncamiento: `EVD-0026`.
- Evidencia del mínimo canónico de Energy 30: `EVD-0021`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual nueva. Este contrato materializa únicamente
el claim ya aprobado y conserva por separado la calidad individual de sus
evidencias.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 30, `EVD-0021` |

La expresión autorizada es:

```text
mana = (character-level - 1) * 2 + energy * 2
```

No existe una constante base separada en la expresión aprobada y el programa no
la infiere. `CHECKED_INT64_V1` la representa como cinco pasos ordenados:
desplazamiento de nivel, aporte de nivel, aporte de Energy, resultado crudo y
truncamiento visible. La resta, las multiplicaciones y la suma usan aritmética
comprobada. El truncamiento final no altera estos casos enteros y no introduce
redondeos intermedios.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Energy | Mana |
|---|---|---:|---:|---:|
| `mana-dark-wizard-base` | Dark Wizard | 1 | 30 | 60 |
| `mana-dark-wizard-level-step` | Dark Wizard | 2 | 30 | 62 |
| `mana-dark-wizard-energy-step` | Dark Wizard | 1 | 31 | 62 |
| `mana-dark-wizard-combined-step` | Grand Master | 2 | 31 | 64 |

Los cuatro controles negativos cubren nivel 0, Energy 29, familia ajena y
overflow de `energy * 2`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica,
resuelve nivel y Energy desde progresión/distribución y ejecuta por referencia
exacta. WPF ofrece una selección genérica cuando existe más de una fórmula
publicada aplicable; no contiene la expresión, sus constantes ni un handler de
Mana. El smoke publicado reproduce los cuatro positivos en las fases inicial y
de reemplazo.
