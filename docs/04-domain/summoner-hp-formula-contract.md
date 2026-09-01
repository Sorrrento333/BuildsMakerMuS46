# Contrato de HP de Summoner

## Identidad y autoridad

- Claim: `DR-HP-SUMMONER`, `VERIFIED`.
- Fórmula: `formula-hp-summoner` `1.0.0`.
- Contrato: `formula.schema.json` `2.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-summoner` y las evoluciones
  `evolution-summoner`, `evolution-bloody-summoner` y
  `evolution-dimension-master`.
- Evidencia del mínimo canónico de Vitality 18: `EVD-0021`.
- Evidencia de contraste: `EVD-0027`–`EVD-0029`.
- Autoridad de expresión, alcance y redondeo: `EVD-0030`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

No se añadió investigación factual nueva. Este contrato materializa únicamente
el claim ya aprobado, sin elevar la clasificación individual de las evidencias
de contraste.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 18, `EVD-0021` |

La expresión autorizada es:

```text
hp = 70 + (character-level - 1) + (vitality - 18) * 2
```

El programa usa la forma algebraicamente equivalente
`34 + (character-level - 1) + vitality * 2`. `CHECKED_INT64_V1` la representa
como seis pasos ordenados: base normalizada, desplazamiento de nivel, aporte de
nivel, aporte de Vitality, resultado crudo y truncamiento visible. Resta,
multiplicaciones y suma usan aritmética comprobada. El truncamiento final no
altera estos casos enteros y no introduce redondeos intermedios.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Vitality | HP |
|---|---|---:|---:|---:|
| `hp-summoner-base` | Summoner | 1 | 18 | 70 |
| `hp-summoner-level-step` | Summoner | 2 | 18 | 71 |
| `hp-summoner-vitality-step` | Summoner | 1 | 19 | 72 |
| `hp-summoner-combined-step` | Dimension Master | 2 | 19 | 73 |

Los cuatro controles negativos cubren nivel 0, Vitality 17, familia ajena y
overflow de `vitality * 2`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance y cobertura. Application materializa la definición genérica, resuelve
nivel y Vitality desde progresión/distribución y ejecuta por referencia exacta.
WPF obtiene la única fórmula publicada aplicable a la clase seleccionada; no
contiene la expresión ni sus constantes. El smoke publicado reproduce los
cuatro positivos en las fases inicial y de reemplazo.
