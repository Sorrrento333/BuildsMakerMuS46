# Diseño técnico — evaluación de build en lote

## Estado y alcance

- Fecha: 2026-09-11.
- Estado: `CLOSED`.
- Implementación productiva: `COMPLETED` el 2026-09-11.
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-07-30.3` (sin cambios).
- Capas incluidas: Application (nuevo caso de uso), WPF (botón y presentación
  agrupada) y el smoke de publicación (verificación del lote y nuevos campos de
  reporte).
- Capas excluidas: Domain, Calculation Engine, Data, SQLite, borradores,
  schemas y nuevas fórmulas.

Este cierre define cómo evaluar en una sola pasada todas las fórmulas
publicadas aplicables a la clase y evolución de un personaje validado. No añade
información de MU Online, ni modifica fórmulas, casos, evidencias ni datos.

## Problema que se cierra

`CalculateCharacterFormulaUseCase` evalúa exactamente una referencia y
resuelve o reutiliza un estado validado por invocación. Para mostrar todos los
atributos derivados de una build, WPF debería iterar el catálogo o ejecutar el
caso de uso individual repetidas veces; cada repetición resolvería sus propias
dependencias, sin una caché compartida, y los resultados no expondrían un
orden estable ni trazas de dependencia anidadas coherentes para el conjunto.

## Decisión

Application expone `CalculateCharacterBuildUseCase`. Su `Execute` recibe el
presupuesto de progresión, los resets y las asignaciones de stats, y:

1. obtiene la distribución validada y resuelve una sola vez
   `ResolvedCharacterState` (clase, evolución, nivel, puntos ganados y de
   resets, stats base + asignación);
2. recorre el catálogo de fórmulas ejecutables y conserva las aplicables a la
   clase y evolución solicitadas, ordenadas por `Reference.Id` y luego por
   `Reference.Version` de forma determinista (comparación ordinal);
3. evalúa cada fórmula con la resolución contextual genérica
   (`FormulaContextValueResolver`), la selección `RAW`/`VISIBLE` y el
   `CalculatePublishedFormulaUseCase` existentes, guardando en una caché
   compartida el resultado completo (traza contextual, traza de dependencia y
   cálculo) por referencia, con un conjunto activo para rechazar ciclos;
4. entrega `CharacterBuildEvaluation` con el estado y una evaluación por
   fórmula; cada evaluación conserva sus trazas anidadas aunque un mismo
   dependiente alimente varias fórmulas del mismo lote;
5. lanza `FormulaContextException` con el código nuevo
   `formula-context-no-applicable-formula` cuando ninguna fórmula publicada es
   aplicable a la clase/evolución pedidas.

El modelo de salida distingue `CharacterBuildFormulaEvaluation` (fórmula,
trazas y cálculo) de `CharacterBuildEvaluation` (estado y evaluaciones), que
rechaza un conjunto vacío. Los códigos de contexto existentes
(`dependency-cycle`, `dependency-incoherent`, `state-mismatch`, …) se
reutilizan; no se introduce una familia de excepciones nueva.

WPF agrega el botón "Evaluar atributos derivados", agrupa los resultados por
salida derivada y muestra referencia, versión y crudo visible por línea; los
errores de contexto se traducen por código, incluido el nuevo caso sin fórmula
aplicable. Los cambios de progresión, resets o asignaciones invalidan el
resultado porque la ejecución recalcula el estado desde las mismas entradas
autoritativas.

## Verificación

- Build de Application y WPF aprobados con 0 advertencias y 0 errores.
- Seis pruebas de integración de Application cierran la vertical y fijan:
  cobertura exacta (todos los pares de clase×evolución con nivel 1 y cero
  gasto), paridad crudo/visible/trazas con el caso de uso individual en cada
  par, traza anidada compartida del par `formula-mana-regen-dark-wizard` sobre
  `formula-mana-dark-wizard`, orden determinista por Id y versión, fallo
  cerrado por nivel inválido y asignación negativa, y rechazo por ausencia de
  fórmula aplicable.
- El smoke de publicación reproduce el lote sobre el personaje sintético
  (cinco stats y 201 gastados, resets `2 × 100 = 200`): 19 fórmulas agrupadas
  con paridad por fórmula contra el camino individual, y reporta
  `PublishedBuildEvaluationVerified` y `PublishedBuildFormulaCount`.
- No se incorporan JSON factuales: ruleset `1.0.0`, motor `0.2.0` y dataset
  `2026-07-30.3` permanecen sin cambios.

## Criterios de cierre

Este diseño queda cerrado porque fija autoridad, composición, orden,
caché/detección de ciclos, errores y pruebas sin decidir datos durante la
implementación.

## Cierre de implementación — 2026-09-11

- `CalculateCharacterBuildUseCase` ejecuta el lote sobre un único estado
  validado y reutiliza la resolución contextual, el intérprete decimal y la
  selección `RAW`/`VISIBLE` existentes; la caché compartida calcula cada
  dependencia una sola vez y conserva la traza anidada en cada consumidor.
- El código `formula-context-no-applicable-formula` está traducido en WPF; el
  botón "Evaluar atributos derivados" muestra las fórmulas agrupadas por salida
  derivada con referencia, versión y crudo visible.
- El smoke `win-x64` verifica el lote sobre el personaje sintético con paridad
  por fórmula y 19 fórmulas agrupadas, y amplía el reporte con
  `PublishedBuildEvaluationVerified` y `PublishedBuildFormulaCount`.
- Seis pruebas de integración de Application y 761/761 pruebas de la solución
  pasan; el CLI del validador no registra diferencias sobre las ciento siete
  fórmulas `PUBLISHED`.