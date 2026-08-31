# Contrato de Defense y SD de Dark Wizard

## Identidad y autoridad

- Claims: Defense adicional preservada por `EVD-0026` y
  `DR-SD-DARK-WIZARD`, `VERIFIED`.
- Fórmulas: `formula-defense-dark-wizard` `1.0.0` y
  `formula-sd-dark-wizard` `1.0.0`.
- Contrato: `formula.schema.json` `2.1.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado: ambas `PUBLISHED`.
- Aplicabilidad: `class-dark-wizard`, `evolution-dark-wizard`,
  `evolution-soul-master` y `evolution-grand-master`.
- Evidencia de stats mínimos: `EVD-0021`.
- Evidencia de expresiones, alcance y truncamiento visible: `EVD-0026`.
- Etapa de dependencia: `EVD-0033` fija Defense `RAW`.
- Conflictos aplicables conocidos: ninguno; se conserva `conflictIds: []`.

`EVD-0033` es una decisión del propietario limitada a Dark Wizard. No se
reclasifican las fuentes previas ni se extiende `RAW` por analogía a otras
familias.

## Defense

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 18, `EVD-0021` |

La expresión autorizada es `defense = agility / 4`.
`CHECKED_DECIMAL_V1` usa `DIVIDE` con el literal exacto 4, conserva el cociente
decimal en `raw-defense` y trunca hacia cero una sola vez en
`visible-defense`.

| Caso | Evolución | Agility | Defense raw | Defense visible |
|---|---|---:|---:|---:|
| `defense-dark-wizard-base` | Dark Wizard | 18 | 4.5 | 4 |
| `defense-dark-wizard-fraction-step` | Soul Master | 19 | 4.75 | 4 |
| `defense-dark-wizard-integer-step` | Soul Master | 20 | 5 | 5 |
| `defense-dark-wizard-grand-master-step` | Grand Master | 21 | 5.25 | 5 |

Los dos controles negativos cubren Agility bajo base y familia ajena. No se
inventa overflow: dividir cualquier `INT64` no negativo admitido por 4 mantiene
la salida visible dentro de `INT64`.

## SD

| Input | Tipo | Fuente | Límite |
|---|---|---|---|
| `character-level` | `INT32` | `CONTEXT_VALUE/character-level` | mínimo técnico 1 |
| `strength` | `INT64` | `CONTEXT_VALUE/resolved-strength` | mínimo factual 18 |
| `agility` | `INT64` | `CONTEXT_VALUE/resolved-agility` | mínimo factual 18 |
| `vitality` | `INT64` | `CONTEXT_VALUE/resolved-vitality` | mínimo factual 15 |
| `energy` | `INT64` | `CONTEXT_VALUE/resolved-energy` | mínimo factual 30 |
| `defense` | `DECIMAL` | `FORMULA_OUTPUT/formula-defense-dark-wizard@1.0.0/RAW` | mínimo técnico 0 |

La expresión autorizada es:

```text
sd = (strength + agility + vitality + energy) * 1.2
   + defense / 2
   + (level * level) / 30
```

El programa conserva el orden escrito en siete pasos: suma de stats, aporte
`× 1.2`, aporte de Defense `÷ 2`, cuadrado del nivel, aporte `÷ 30`, SD crudo
y SD visible. No existe truncamiento intermedio; `visible-sd` trunca el total
hacia cero una sola vez.

| Caso | Evolución | Nivel | STR/AGI/VIT/ENE | Defense raw | SD raw | SD visible |
|---|---|---:|---|---:|---:|---:|
| `sd-dark-wizard-base` | Dark Wizard | 1 | `18/18/15/30` | 4.5 | 99.483333… | 99 |
| `sd-dark-wizard-level-step` | Soul Master | 2 | `18/18/15/30` | 4.5 | 99.583333… | 99 |
| `sd-dark-wizard-raw-defense-boundary` | Soul Master | 4 | `18/19/15/30` | 4.75 | 101.308333… | 101 |
| `sd-dark-wizard-combined-step` | Grand Master | 5 | `19/19/16/31` | 4.75 | 105.208333… | 105 |

El caso de frontera hace observable la decisión: con Defense `VISIBLE=4`, el
mismo resto produciría SD visible 100; con `RAW=4.75`, el resultado aprobado es
101. Los siete controles negativos cubren nivel, cada stat bajo su base,
familia y overflow de la salida `INT64`.

## Integración

Application resuelve ambas fórmulas desde el snapshot. Para SD ejecuta Defense
una vez sobre el mismo estado validado, selecciona `RawOutput`, conserva la
traza productora y entrega el decimal al intérprete consumidor. WPF mantiene
selección genérica por referencia exacta y muestra las trazas contextual,
de dependencia y aritmética sin handlers ni constantes factuales en C#.

El gate estructural valida identidades, inputs, aridades, dependencia y casos.
Las pruebas de Application reproducen los casos directos y la composición
snapshot → progresión → distribución → Defense RAW → SD. El smoke publicado
debe repetir los ocho positivos en las fases inicial y de reemplazo.
