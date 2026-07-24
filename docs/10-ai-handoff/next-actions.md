# Próximas acciones

## Prioridad inmediata

1. Implementar en Domain y Calculation Engine la operación pura de distribución
   definida en `docs/04-domain/stat-distribution-contract.md`.
2. Consumir el `ProgressionPointBudgetResult` existente y la definición de
   clase materializada, sin recalcular progresión ni duplicar stats factuales.
3. Cubrir con pruebas sintéticas distribución parcial/exacta y los errores
   fijados para negativos, stat ajeno, stat omitido y exceso de presupuesto.
4. Mantener fuera de alcance resets, HP, Mana, AG, SD, daño y defensa hasta
   contar con investigación, contratos y pruebas propios.

## Última tarea cerrada

El contrato `stat-distribution.schema.json` `1.0.0` conserva ruleset, clase,
regla/version de progresión, presupuesto ganado, asignaciones, gasto y
remanente. Sus fixtures válido/inválido son sintéticos.

La especificación de dominio fija el rango `0..Int64.MaxValue`, sumas
comprobadas, coincidencia exacta entre asignaciones y stats de la clase, y los
códigos de error mínimos. `command` no se asocia a una lista duplicada de
clases: sólo existe para la distribución cuando aparece en la definición
canónica cargada.

## Cierre anterior

El validador integral acepta 7/7 fixtures válidos y rechaza 7/7 inválidos; la
comprobación estructural cubre 7 schemas y 14 ejemplos. No se añadieron datos,
fórmulas, evidencias ni registros de investigación de MU Online, y todavía no
existe implementación productiva de la distribución.

## Primera acción concreta

Agregar los tipos de solicitud/resultado y errores de distribución en Domain, y
un calculador puro en Calculation Engine que derive `spentPoints` y
`remainingPoints` desde asignaciones no confiables. Debe resolver disponibilidad
desde `CharacterProgressionDefinition` —ampliada sólo con los IDs de stats ya
materializados— y aprobar los seis casos sintéticos mínimos del contrato. No
conectar todavía Application, WPF, persistencia ni resets.
