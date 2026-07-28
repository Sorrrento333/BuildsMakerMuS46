# Contrato de Mana de Summoner

## Identidad y autoridad

- Claim: `DR-MANA-SUMMONER`, `VERIFIED`.
- Fórmula: `formula-mana-summoner` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-summoner` y las evoluciones
  `evolution-summoner`, `evolution-bloody-summoner` y
  `evolution-dimension-master`.
- Evidencia del mínimo canónico de Energy 23: `EVD-0021`.
- Evidencia de expresión y alcance: `EVD-0027`–`EVD-0029` como contraste y
  `EVD-0031` como autoridad final.
- Evidencia del truncamiento visible: `EVD-0026` y `EVD-0031`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual nueva ni se reclasificó ninguna fuente.
`EVD-0030` se limita a HP y no se hereda como provenance de esta fórmula.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 23, `EVD-0021` |

La expresión autorizada es:

```text
mana = 40 + (character-level - 1) * 1.5 + (energy - 23) * 1.7
```

`CHECKED_DECIMAL_V1` conserva `1.5` y `1.7` exactamente en base 10. El programa
declara siete pasos ordenados: base, desplazamiento y aporte de nivel,
desplazamiento y aporte de Energy, resultado crudo y resultado visible. La
aritmética decimal es comprobada, no hay redondeos intermedios y
`visible-mana` trunca hacia cero una sola vez.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Energy | Mana crudo | Mana visible |
|---|---|---:|---:|---:|---:|
| `mana-summoner-base` | Summoner | 1 | 23 | 40 | 40 |
| `mana-summoner-level-step` | Summoner | 2 | 23 | 41.5 | 41 |
| `mana-summoner-energy-step` | Summoner | 1 | 24 | 41.7 | 41 |
| `mana-summoner-combined-step` | Dimension Master | 2 | 24 | 43.2 | 43 |

Los cuatro controles negativos cubren nivel 0, Energy 22, familia ajena y
overflow al convertir la salida decimal visible a `INT64`. Sólo los cuatro
positivos están enlazados desde `testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica,
resuelve nivel y Energy desde progresión/distribución y ejecuta por referencia
exacta. WPF reutiliza la selección genérica entre HP y Mana; no contiene la
expresión, sus constantes ni un handler por fórmula. El smoke publicado debe
reproducir los cuatro positivos en las fases inicial y de reemplazo.
