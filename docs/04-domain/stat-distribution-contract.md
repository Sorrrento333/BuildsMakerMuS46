# Contrato de distribución de stats

## Alcance

`stat-distribution.schema.json` `1.1.0` describe el resultado trazable de
distribuir el presupuesto de progresión que devuelve Application más una
configuración explícita de puntos por resets. Esta vertical no deriva HP, Mana,
AG, SD, daño ni defensa.

El contrato es independiente de una clase concreta. Los fixtures son
exclusivamente sintéticos y no agregan datos ni fórmulas de MU Online.

## Entradas de la operación

- `ProgressionPointBudgetResult`, sin recalcular ni alterar `EarnedPoints`; el
  resultado conserva también `CharacterClassId` para validar su origen.
- La definición canónica de la clase del mismo `rulesetId`.
- `ResetPointInputs` con cantidad de resets y puntos por reset configurados por
  el usuario; ambos valen cero de forma predeterminada.
- Un mapa de asignaciones solicitadas, indexado por ID de stat.

La operación rechaza un presupuesto cuya clase, ruleset o referencia de
regla no coincidan con el catálogo cargado. Esta comprobación no puede
expresarse sólo con JSON Schema y queda como gate semántico obligatorio.

## Resultado serializable

- `rulesetId` y `characterClassId`.
- `progressionRule.id` y `progressionRule.version`, copiados de la fuente del
  presupuesto.
- `earnedPoints`, copiado del presupuesto ganado.
- `resetInputs`, copia de los dos inputs configurables.
- `resetPoints`, producto comprobado de cantidad por puntos por reset.
- `totalDistributablePoints`, suma comprobada de `earnedPoints` y
  `resetPoints`.
- `allocations`, con exactamente una entrada por cada stat declarado por la
  clase; un stat sin gasto se representa con cero.
- `spentPoints`, igual a la suma comprobada de todas las asignaciones.
- `remainingPoints`, igual a `totalDistributablePoints - spentPoints`.

Todos los contadores están limitados al intervalo entero
`0..9_223_372_036_854_775_807`, consistente con el `long` del presupuesto
actual. La suma y la resta deberán ejecutarse con control de overflow.

## Invariantes semánticos

1. Las claves de `allocations` coinciden exactamente con las claves de `stats`
   de la definición canónica de la clase.
2. Ninguna asignación es negativa.
3. `spentPoints` es la suma de `allocations`.
4. Los inputs de reset son no negativos y su producto cabe en 64 bits.
5. `totalDistributablePoints = earnedPoints + resetPoints`.
6. `spentPoints` no supera `totalDistributablePoints`.
7. `remainingPoints` es no negativo y conserva la identidad
   `totalDistributablePoints = spentPoints + remainingPoints`.
8. `command` no es una excepción codificada: sólo está disponible cuando la
   definición canónica de la clase contiene el stat `command`.
9. Los valores base determinan disponibilidad, pero no consumen presupuesto;
   `allocations` contiene únicamente puntos gastados.

JSON Schema impone forma, IDs, versiones y rangos escalares. Las igualdades,
sumas y referencias al catálogo requieren la futura validación de dominio.

## Errores estables

| Código | Condición |
|---|---|
| `allocation-negative` | Alguna asignación solicitada es menor que cero. |
| `stat-not-available` | La solicitud contiene un stat que la clase no declara. |
| `stat-allocation-missing` | Falta un stat declarado por la clase. |
| `allocation-exceeds-earned-points` | La suma solicitada supera el presupuesto ganado. |
| `allocation-overflow` | Una suma no puede representarse como entero de 64 bits. |
| `budget-source-mismatch` | Ruleset, clase o regla del presupuesto no coincide con el catálogo. |
| `reset-count-negative` | La cantidad de resets es negativa. |
| `points-per-reset-negative` | Los puntos configurados por reset son negativos. |
| `reset-points-overflow` | El producto de resets por puntos no cabe en 64 bits. |
| `total-distributable-points-overflow` | La suma de progresión y resets no cabe en 64 bits. |

`spentPoints` y `remainingPoints` son salidas calculadas, no valores confiados
al llamador de la futura operación.

## Casos sintéticos de la implementación

| Caso | Presupuesto | Asignaciones | Resultado |
|---|---:|---|---|
| Distribución parcial | 10 | `stat-alpha=4`, `stat-beta=3` | gastados 7, restantes 3 |
| Distribución exacta | 10 | `stat-alpha=4`, `stat-beta=6` | gastados 10, restantes 0 |
| Dos resets de 100 | 10 + 200 | `stat-alpha=104`, `stat-beta=103` | gastados 207, restantes 3 |
| Valor negativo | 10 | `stat-alpha=-1` | `allocation-negative` |
| Stat ajeno | 10 | `stat-gamma=1` | `stat-not-available` |
| Gasto excesivo | 10 | `stat-alpha=11` | `allocation-exceeds-earned-points` |
| Stat omitido | 10 | falta `stat-beta` | `stat-allocation-missing` |

Los ejemplos JSON válido e inválido cubren el gate estructural. La tabla fija
la cobertura semántica mínima de `StatDistributionCalculator`; la suite añade
controles separados para las tres variantes de `budget-source-mismatch`, los
cuatro errores de resets y `allocation-overflow`. Todos estos casos son
sintéticos, no se convierten en
casos factuales del ruleset y no requieren investigación del juego.

## Implementación productiva

Domain expone `StatDistributionRequest`, `StatDistributionResult`, los diez
códigos estables y `StatDistributionException`. Calculation Engine implementa
la operación estática y pura: valida el origen del presupuesto, exige el
conjunto exacto de stats de `CharacterProgressionDefinition`, suma con
`checked`, deriva gasto y remanente y devuelve las asignaciones normalizadas en
orden ordinal.

`CharacterProgressionDefinition` conserva únicamente los IDs de las claves de
`stats`; no materializa ni usa los valores base para gastar presupuesto.
Application lee esos IDs desde el snapshot y expone
`CalculateStatDistributionUseCase`. El llamador entrega sólo el
`ProgressionPointBudgetResult` existente y las asignaciones: el caso de uso
resuelve por `CharacterClassId` y `RulesetId` una única definición del catálogo,
recibe además `ResetPointInputs`, construye `StatDistributionRequest` y delega
sin mutar ni recalcular el
presupuesto. Si no existe exactamente una coincidencia, falla con
`budget-source-mismatch`; los demás códigos tipados se propagan desde el motor.

## Integración WPF y gate de publicación

La pantalla obtiene los IDs de asignación desde la misma definición de clase
que materializa Application y conserva el presupuesto ya calculado. No
recalcula progresión al distribuir ni codifica nombres, valores base o límites
de stats. Un cambio en cualquier entrada de progresión invalida el presupuesto
para impedir que se reutilice con una identidad distinta.

La salida visible presenta puntos por nivel/quests, inputs y producto de resets,
total distribuible, `SpentPoints`, `RemainingPoints` y las asignaciones.
Cada `StatDistributionException` mantiene su código estable entre paréntesis y
añade una explicación en español. El smoke de publicación selecciona desde el
snapshot un caso de progresión con presupuesto positivo, genera el mapa completo
desde `StatIds`, configura dos resets de 100 puntos y exige que los 200 puntos
adicionales puedan gastarse en las fases inicial y de reemplazo. No se añade
un caso factual: los resets permanecen como configuración del servidor.
