# Diseño de contrato — HP de la familia Dark Wizard

## Estado y alcance

- Claim de investigación: `DR-HP-DARK-WIZARD`.
- ID candidato de fórmula: `formula-hp-dark-wizard`.
- Versión candidata: `1.0.0`.
- Ruleset: `mu-s4-global-reference`.
- Estado materializado: `PUBLISHED` el 2026-07-25.
- Confianza factual: `VERIFIED`.
- Revisión técnica: `APPROVED` el 2026-07-24.
- Clase aplicable: `class-dark-wizard`.
- Evoluciones aplicables:
  - `evolution-dark-wizard`;
  - `evolution-soul-master`;
  - `evolution-grand-master`.
- Autoridad de la fórmula y su aplicabilidad: `EVD-0026`.
- Autoridad del mínimo canónico de Vitality: `EVD-0021`.
- Conflicto trazado: `DSP-0003`, resuelto por decisión del propietario.

Este documento nació como diseño previo a implementación. La definición JSON y
sus ocho casos fueron materializados el 2026-07-25 bajo
`packages/rulesets/mu-s4-global-reference/v1`. La revisión de publicación
confirmó el contrato sin divergencias y promovió únicamente el estado a
`PUBLISHED`; no existe todavía código de cálculo.

La revisión aprueba el ID, versión, alcance, tipos, errores, traza y casos
manuales con la corrección de procedencia indicada arriba. La estrategia de
schemas quedó decidida después en
`../06-data/formula-schema-contract-decision.md` y sus tres contratos técnicos
ya están implementados con fixtures sintéticos.

## Propósito

Calcular el HP base visible de un personaje de la familia Dark Wizard a partir
de su nivel y Vitality, conservando una traza suficiente para reproducir cada
aporte.

Quedan fuera de esta fórmula equipo, opciones, buffs, perfiles privados y
cualquier modificador no declarado por `EVD-0026`. El contrato recibe un valor
entero de Vitality ya resuelto por el contexto de cálculo; no presupone qué
fuentes o modificadores lo componen. Esa composición deberá pertenecer a una
capa anterior y quedar trazada allí antes de incorporar modificadores.

## Entradas y salida

| Campo | Tipo propuesto | Unidad | Restricción |
|---|---:|---|---|
| `character-level` | entero de 32 bits | nivel | rango técnico `1..2147483647`; el extremo superior no es un máximo factual |
| `vitality` | entero de 64 bits | punto de stat | rango técnico `15..9223372036854775807`; 15 es la base canónica trazada a `EVD-0021` |
| `hp` | entero de 64 bits | punto de HP | resultado de aritmética comprobada |

La identidad de clase y evolución forma parte del contexto de resolución, no de
la expresión aritmética. El resolvedor deberá rechazar cualquier clase o
evolución fuera de la familia enumerada.

## Expresión y orden

La expresión autorizada por `EVD-0026` se conserva sin transformación:

```text
hp = 30 + (lvl - 1) + vit * 2
```

El evaluador deberá producir estos pasos lógicos en orden:

1. `base = 30`.
2. `level-contribution = lvl - 1`.
3. `vitality-contribution = vit * 2`.
4. `raw-hp = base + level-contribution + vitality-contribution`.
5. `visible-hp = truncate(raw-hp)`.

Todas las restas, multiplicaciones y sumas se ejecutan con overflow comprobado.
Los tres aportes son enteros; por eso el truncamiento final no cambia el valor,
pero debe permanecer en la traza para respetar la política visible de
`EVD-0026` y evitar que una optimización silenciosa elimine el punto de redondeo.
`truncate` significa truncamiento hacia cero.

No existen dependencias de otras fórmulas ni modificadores para esta versión.

## Traza mínima

Un resultado válido deberá conservar:

- ID y versión de fórmula;
- ruleset, clase y evolución resueltos;
- nivel y Vitality recibidos;
- `base`;
- `level-contribution`;
- `vitality-contribution`;
- `raw-hp`;
- modo y etapa de redondeo;
- `visible-hp`;
- referencias `evd-0021`, `evd-0026` y `dsp-0003`.

La suma de los tres aportes debe ser exactamente igual a `raw-hp`, y
`visible-hp` debe ser exactamente el truncamiento registrado de `raw-hp`.

## Errores candidatos

Los nombres son parte del diseño y no constituyen todavía API productiva:

| Código | Condición |
|---|---|
| `formula-not-applicable` | clase o evolución fuera de la familia Dark Wizard |
| `formula-level-out-of-range` | nivel fuera del rango técnico del input de 32 bits o menor que 1 |
| `formula-stat-below-base` | Vitality menor que 15 |
| `formula-arithmetic-overflow` | una operación no cabe en el entero de salida |

No se define un máximo de nivel o Vitality como regla de MU Online porque la
evidencia aprobada no lo establece para este contrato. Los extremos de los
tipos son límites del contrato de software y se validan sin presentarlos como
límites del juego.

## Casos manuales propuestos

Estos casos son las especificaciones aprobadas materializadas bajo
`reference-cases/formulas`. Los cuatro positivos están enlazados desde
`testCaseRefs`; los cuatro controles negativos permanecen separados.

| Case ID candidato | Contexto/entradas | Traza esperada | Resultado |
|---|---|---|---:|
| `hp-dark-wizard-base` | Dark Wizard, nivel 1, Vitality 15 | `30 + 0 + 30 = 60`; `truncate(60) = 60` | 60 |
| `hp-dark-wizard-level-step` | Dark Wizard, nivel 2, Vitality 15 | `30 + 1 + 30 = 61`; `truncate(61) = 61` | 61 |
| `hp-dark-wizard-vitality-step` | Dark Wizard, nivel 1, Vitality 16 | `30 + 0 + 32 = 62`; `truncate(62) = 62` | 62 |
| `hp-dark-wizard-combined-step` | Grand Master, nivel 2, Vitality 16 | `30 + 1 + 32 = 63`; `truncate(63) = 63` | 63 |
| `hp-dark-wizard-invalid-level` | Dark Wizard, nivel 0, Vitality 15 | se rechaza antes de evaluar aportes | `formula-level-out-of-range` |
| `hp-dark-wizard-vitality-below-base` | Dark Wizard, nivel 1, Vitality 14 | se rechaza antes de evaluar aportes | `formula-stat-below-base` |
| `hp-dark-wizard-invalid-family` | Fairy Elf, nivel 1, Vitality 20 | se rechaza antes de evaluar aportes | `formula-not-applicable` |
| `hp-dark-wizard-overflow` | Dark Wizard, nivel 1, Vitality `4611686018427387904` | falla `vitality * 2` con aritmética comprobada | `formula-arithmetic-overflow` |

Los valores pequeños separan el incremento de nivel del incremento de Vitality.
El caso combinado demuestra que la misma fórmula aplica a una evolución de la
familia. El último caso es un borde técnico sintético, no un stat alcanzable ni
un límite factual de MU Online.

## Encaje con los contratos actuales

`formula.schema.json` `1.0.0` ya puede expresar el ID, versión, ruleset,
confianza, dos entradas enteras, salida entera, estrategia `EXPRESSION`,
redondeo `TRUNCATE`, evidencia, conflicto y referencias de casos.

Antes de materializar este diseño debe resolverse documentalmente cómo expresar:

1. clase y evoluciones aplicables;
2. restricciones mínimas y ancho numérico de las cantidades;
3. que `vitality` es un valor resuelto por el contexto, sin inventar su
   composición;
4. la estructura ordenada de la traza y sus invariantes;
5. el contrato de los casos de fórmula y de sus errores esperados.

La decisión técnica establece que aplicabilidad, bounds, procedencia de inputs y
declaración ordenada de pasos amplían `formula.schema.json` `1.1.0`. La ejecución
concreta pertenece a `calculation-trace.schema.json` `1.0.0`, y los casos a
`formula-test-case.schema.json` `1.0.0`. No se debe publicar una fórmula usando
texto libre para eludir estos requisitos.

## Resultado de la revisión y gate para implementación

La revisión técnica del 2026-07-24 aprueba:

- `formula-hp-dark-wizard` `1.0.0`;
- la aplicabilidad a Dark Wizard, Soul Master y Grand Master;
- los tipos y límites técnicos declarados, sin convertirlos en máximos del
  juego;
- los cuatro errores candidatos;
- la traza ordenada y los ocho casos manuales;
- `EVD-0026` como autoridad de la fórmula y `EVD-0021` como autoridad del
  mínimo canónico de Vitality.

El paso de este diseño a datos quedó cerrado mediante:

1. los tres contratos decididos y sus fixtures sintéticos válidos e inválidos;
2. la definición canónica `formula-hp-dark-wizard` `1.0.0` y sus ocho casos;
3. el gate semántico de identidad, catálogo, provenance, traza, redondeo y
   cobertura exacta de positivos.

La revisión de publicación del 2026-07-25 confirmó identidad, provenance,
aplicabilidad, inputs, bounds, expresión, traza, redondeo y cobertura exacta de
los cuatro positivos, manteniendo separados los cuatro controles negativos. La
única mutación factual fue `status: PUBLISHED`.

La futura implementación del motor deberá leer constantes, alcance y casos
desde el snapshot. No podrá duplicar `30`, `2`, los IDs de evolución ni los
resultados manuales directamente en WPF o en el código de prueba productivo.

El diseño de esa vertical está cerrado en
`dark-wizard-hp-execution-vertical-design.md`. Concluye que
`strategy.definition` `1.1.0` no es ejecutable de forma cerrada: no enlaza los
aliases con inputs ni los pasos con operaciones y obligaría a interpretar texto
libre. La fórmula publicada `1.0.0` permanece inmutable; el siguiente gate es un
contrato estructurado `formula.schema.json` `2.0.0` y una nueva versión de
fórmula antes de escribir el evaluador. Ambos ya están materializados: la nueva
definición `1.1.0` está `PUBLISHED` y enlaza cuatro positivos de una serie
propia de ocho casos que conserva exactamente las expectativas aprobadas. El
gate demuestra la coexistencia por identidad compuesta y el rechazo de
duplicados. La revisión de publicación independiente no encontró divergencias y
cambió únicamente `status`; todavía no existe ejecución productiva.
