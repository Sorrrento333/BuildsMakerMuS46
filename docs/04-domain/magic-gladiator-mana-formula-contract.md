# Contrato de Mana de Magic Gladiator

## Identidad y autoridad

- Claim: `DR-MANA-MAGIC-GLADIATOR`, `VERIFIED`.
- Fórmula: `formula-mana-magic-gladiator` `1.0.0`.
- Contrato: `formula.schema.json` `2.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: `PUBLISHED`.
- Aplicabilidad: `class-magic-gladiator` y las evoluciones
  `evolution-magic-gladiator` y `evolution-duel-master`.
- Evidencia del mínimo canónico de Energy 26: `EVD-0021`.
- Evidencia de expresión, alcance y truncamiento visible: `EVD-0026`.
- Conflicto aplicable: `DSP-0002`, resuelto por decisión del propietario a favor
  de Energy 26; la guía actual de Webzen que publica 16 conserva su
  clasificación y no se reescribe.

No se añadió investigación factual nueva ni se reclasificó ninguna fuente.

## Entradas y expresión

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 26, `EVD-0021` |

La expresión autorizada es:

```text
mana = 8 + (character-level - 1) + energy * 2
```

`CHECKED_INT64_V1` declara cinco pasos ordenados: constante 8, desplazamiento
de nivel, aporte de Energy sin desplazamiento, resultado crudo y resultado
visible. La aritmética entera es comprobada. No se introduce una base implícita,
un desplazamiento de Energy, un coeficiente adicional ni redondeo intermedio.
`visible-mana` trunca hacia cero una sola vez, aunque para las entradas enteras
de esta versión el resultado crudo ya es entero.

## Casos aprobados

| Caso | Clase/evolución | Nivel | Energy | Mana crudo | Mana visible |
|---|---|---:|---:|---:|---:|
| `mana-magic-gladiator-base` | Magic Gladiator | 1 | 26 | 60 | 60 |
| `mana-magic-gladiator-level-step` | Magic Gladiator | 2 | 26 | 61 | 61 |
| `mana-magic-gladiator-energy-step` | Magic Gladiator | 1 | 27 | 62 | 62 |
| `mana-magic-gladiator-combined-step` | Duel Master | 2 | 27 | 63 | 63 |

Los cuatro controles negativos cubren nivel 0, Energy 25, familia ajena y
overflow en `energy * 2`. Sólo los cuatro positivos están enlazados desde
`testCaseRefs`.

## Integración

El gate de schemas valida identidad, catálogo, inputs, programa, traza,
provenance, conflicto y cobertura. Application materializa la definición
genérica, resuelve nivel y Energy desde progresión/distribución y ejecuta por
referencia exacta. WPF reutiliza la selección genérica entre HP y Mana; no
contiene la expresión, sus constantes ni un handler por fórmula. El smoke
publicado debe reproducir los cuatro positivos en las fases inicial y de
reemplazo.
