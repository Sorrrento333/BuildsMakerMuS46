# Configuración de puntos por reset

## Decisión del propietario — 2026-07-24

Los resets no forman parte del ruleset estándar de MU Online Season 4
global/inglés. Su cantidad y recompensa dependen de cada servidor. Por ello el
producto no investigará ni publicará una fórmula estándar de resets y no
incorporará estos valores a `mu-s4-global-reference`.

La calculadora los modela como configuración explícita del escenario de usuario:

- `resetCount`: cantidad de resets;
- `pointsPerReset`: puntos otorgados por cada reset;
- `resetPoints`: producto derivado `resetCount × pointsPerReset`;
- `totalDistributablePoints`: puntos de nivel/quests más `resetPoints`.

Los valores predeterminados de ambos inputs son cero. En consecuencia, el valor
predeterminado de `resetPoints` también es cero y el comportamiento previo de
la distribución no cambia.

## Invariantes

1. `resetCount` y `pointsPerReset` son enteros de 64 bits no negativos.
2. `resetPoints` se calcula con multiplicación comprobada; nunca se acepta un
   total ingresado manualmente.
3. `totalDistributablePoints` se calcula con suma comprobada a partir de
   `ProgressionPointBudgetResult.EarnedPoints` y `resetPoints`.
4. Las asignaciones pueden consumir el presupuesto combinado, pero la salida
   conserva separados puntos de progresión y puntos por resets.
5. Un cambio en clase, evolución, nivel o quests invalida el presupuesto de
   progresión. Un cambio en resets invalida sólo la distribución derivada.
6. Los borradores persisten los dos inputs autoritativos y vuelven a calcular
   producto, presupuesto total, gasto y remanente al cargar.
7. Ningún valor de resets se almacena en el paquete factual del ruleset ni se
   presenta como comportamiento oficial del juego.

## Errores estables

| Código | Condición |
|---|---|
| `reset-count-negative` | La cantidad de resets es negativa. |
| `points-per-reset-negative` | Los puntos configurados por reset son negativos. |
| `reset-points-overflow` | El producto no cabe en un entero de 64 bits. |
| `total-distributable-points-overflow` | La suma con el presupuesto de progresión desborda. |

Los errores existentes de distribución continúan aplicándose a asignaciones,
stats y procedencia. `allocation-exceeds-earned-points` conserva su código
estable, pero compara contra `totalDistributablePoints`.

## Versionado

`stat-distribution.schema.json` y `build-draft.schema.json` avanzan a `1.1.0`
para incorporar el desglose y los inputs de resets. La tabla SQLite no cambia:
el payload JSON y la columna `schema_version` ya almacenan la versión exacta.
El motor de la composición WPF avanza a `0.2.0`; ruleset y dataset permanecen
sin cambios porque no se añadió información factual del juego.
