# Contrato de distribución de stats

## Alcance

`stat-distribution.schema.json` `1.0.0` describe el resultado trazable de
distribuir el presupuesto de puntos que ya devuelve Application. Esta vertical
no calcula puntos por nivel, no introduce resets y no deriva HP, Mana, AG, SD,
daño ni defensa.

El contrato es independiente de una clase concreta. Los fixtures son
exclusivamente sintéticos y no agregan datos ni fórmulas de MU Online.

## Entradas de la futura operación

- `ProgressionPointBudgetResult`, sin recalcular ni alterar `EarnedPoints`.
- La definición canónica de la clase del mismo `rulesetId`.
- Un mapa de asignaciones solicitadas, indexado por ID de stat.

La operación deberá rechazar un presupuesto cuya clase, ruleset o referencia de
regla no coincidan con el catálogo cargado. Esta comprobación no puede
expresarse sólo con JSON Schema y queda como gate semántico obligatorio.

## Resultado serializable

- `rulesetId` y `characterClassId`.
- `progressionRule.id` y `progressionRule.version`, copiados de la fuente del
  presupuesto.
- `earnedPoints`, copiado del presupuesto ganado.
- `allocations`, con exactamente una entrada por cada stat declarado por la
  clase; un stat sin gasto se representa con cero.
- `spentPoints`, igual a la suma comprobada de todas las asignaciones.
- `remainingPoints`, igual a `earnedPoints - spentPoints`.

Todos los contadores están limitados al intervalo entero
`0..9_223_372_036_854_775_807`, consistente con el `long` del presupuesto
actual. La suma y la resta deberán ejecutarse con control de overflow.

## Invariantes semánticos

1. Las claves de `allocations` coinciden exactamente con las claves de `stats`
   de la definición canónica de la clase.
2. Ninguna asignación es negativa.
3. `spentPoints` es la suma de `allocations`.
4. `spentPoints` no supera `earnedPoints`.
5. `remainingPoints` es no negativo y conserva la identidad
   `earnedPoints = spentPoints + remainingPoints`.
6. `command` no es una excepción codificada: sólo está disponible cuando la
   definición canónica de la clase contiene el stat `command`.
7. Los valores base determinan disponibilidad, pero no consumen presupuesto;
   `allocations` contiene únicamente puntos gastados.

JSON Schema impone forma, IDs, versiones y rangos escalares. Las igualdades,
sumas y referencias al catálogo requieren la futura validación de dominio.

## Errores estables previstos

| Código | Condición |
|---|---|
| `allocation-negative` | Alguna asignación solicitada es menor que cero. |
| `stat-not-available` | La solicitud contiene un stat que la clase no declara. |
| `stat-allocation-missing` | Falta un stat declarado por la clase. |
| `allocation-exceeds-earned-points` | La suma solicitada supera el presupuesto ganado. |
| `allocation-overflow` | Una suma no puede representarse como entero de 64 bits. |
| `budget-source-mismatch` | Ruleset, clase o regla del presupuesto no coincide con el catálogo. |

`spentPoints` y `remainingPoints` son salidas calculadas, no valores confiados
al llamador de la futura operación.

## Casos sintéticos para la implementación posterior

| Caso | Presupuesto | Asignaciones | Resultado |
|---|---:|---|---|
| Distribución parcial | 10 | `stat-alpha=4`, `stat-beta=3` | gastados 7, restantes 3 |
| Distribución exacta | 10 | `stat-alpha=4`, `stat-beta=6` | gastados 10, restantes 0 |
| Valor negativo | 10 | `stat-alpha=-1` | `allocation-negative` |
| Stat ajeno | 10 | `stat-gamma=1` | `stat-not-available` |
| Gasto excesivo | 10 | `stat-alpha=11` | `allocation-exceeds-earned-points` |
| Stat omitido | 10 | falta `stat-beta` | `stat-allocation-missing` |

Los ejemplos JSON válido e inválido cubren el gate estructural. La tabla fija
la cobertura semántica mínima, pero no se convierte en casos factuales del
ruleset ni autoriza todavía código productivo.
