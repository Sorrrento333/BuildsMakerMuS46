# Diseño técnico — reaplicación de build cargada en la Calculadora

## Estado y alcance

- Fecha: 2026-09-16.
- Estado: `CLOSED`.
- Implementación productiva: `COMPLETED` el 2026-09-16.
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-07-30.3` (sin cambios).
- Capas incluidas: Application (modelo `CharacterBuild` `1.1.0` y promoción),
  schema `build.schema.json` (`1.1.0`), WPF (reaplicación al formulario y
  evaluación de atributos derivados) y el smoke de publicación (paridad de
  resets y reproducción de la distribución).
- Capas excluidas: Domain, Calculation Engine, repositorios SQLite (el payload
  es JSON y no requiere migración), borradores y nuevas fórmulas o datos.

Esta vertical cierra la primera parte de «builds completas y flujos de UI
posteriores»: recuperar por ID una build y devolverla al formulario de la
Calculadora, con sus atributos derivados, sin inventar mecánicas ni datos de MU
Online. No modifica fórmulas, casos, evidencias ni el catálogo factual.

## Antecedente

La vertical de persistencia de build completa local
(`docs/04-domain/full-build-persistence-design.md`) cerró el round-trip
`borrador → build → recarga`, pero `CharacterBuild` `1.0.0` persistía
`resetCount` sin `pointsPerReset`. Los resets son configuración del servidor,
ya persistidos completos en los borradores
(`docs/04-domain/reset-point-configuration.md`) y recalculados por el motor
(`ResetPointInputs(resetCount, pointsPerReset)`). Sin `pointsPerReset` la build
no era reproducible hacia el formulario: la distribución no podía rehacerse de
forma exacta y la evaluación de atributos derivados fallaría al no poder
justificar los puntos de reset gastados.

## Problema que se cierra

Al cargar una build, WPF sólo mostraba el mensaje de éxito y los stats
revalidados: no seleccionaba clase ni evolución, no restauraba nivel, estado de
héroe, resets ni asignaciones, y no recalculaba presupuesto, distribución ni
atributos derivados. El usuario no podía continuar trabajando con la build
recuperada. El bloqueo era de contrato: faltaba `pointsPerReset`, por lo que
reconstruir las asignaciones (`stats − base`) no bastaba para rehacer la
distribución ni para evaluar.

## Decisión

Se extiende el modelo persistido para que una build sea un snapshot completo y
reproducible de la configuración del personaje, incluidos sus resets:

- `CharacterBuild` incorpora `pointsPerReset` y avanza a `schemaVersion
  "1.1.0"`. `SaveBuildUseCase` lo toma de
  `draft.ResetInputs.PointsPerReset`; `LoadBuildUseCase` no cambia sus
  validaciones (la reaplicación reproduce la distribución y falla cerrada si el
  estado no fuese coherente).
- `build.schema.json` pasa a `1.1.0`: `pointsPerReset` es requerido y entero no
  negativo (mismo tratamiento que `resetCount`). Los fixtures `valid`/`invalid`
  y el inventario estructural (`tests/schemas/Test-SchemaStructure.ps1`) se
  actualizan. No hay columnas SQLite nuevas: el payload es JSON.
- WPF añade `ApplyLoadedBuild(CharacterBuild)`: selecciona clase y evolución,
  nivel y estado de héroe, restaura `Resets` y `Puntos por reset`, deriva las
  asignaciones como `stat final − valor base canónico`, recalcula el presupuesto
  y la distribución con `ResetPointInputs(build.ResetCount, build.PointsPerReset)`
  y ejecuta la evaluación de atributos derivados. `LoadBuildButtonClick` invoca
  este flujo y traduce los errores de distribución/progresión.

La build sigue sin persistir presupuesto, distribución ni evaluaciones: esos
valores se recalculan al reaplicar y su autoridad es el snapshot exacto
revalidado. Los borradores y su contrato `1.1.0` permanecen intactos.

## Verificación

- Build de Application, Data, WPF y el smoke publicados con 0 advertencias y 0
  errores.
- Application: la promoción persiste `pointsPerReset` (`2 × 100 = 200`), la
  recarga conserva la paridad y un test nuevo deriva las asignaciones de los
  stats (`4` y `3`), recalcula la distribución (`ResetPoints 200`, `SpentPoints
  7`) y comprueba `Total = Spent + Remaining`; el modelo serializado expone
  `pointsPerReset` con su nombre exacto.
- Data: el payload y la metadata exactos siguen round-trip con el campo nuevo
  (pruebas de `SqliteBuildRepository` sin cambios de esquema SQLite).
- El smoke `win-x64` verifica que la build `publication-smoke-build` conserva
  `PointsPerReset`, y que sus entradas reproducen la distribución sintética
  (mismos `ResetPoints`, `SpentPoints` y asignaciones) antes del respaldo, tras
  restaurar y tras el reemplazo de binarios.
- 780/780 pruebas de la solución pasan; el CLI del validador no registra
  diferencias sobre las ciento siete fórmulas `PUBLISHED`.

## Criterios de cierre

Este diseño queda cerrado porque fija contrato, promoción, reaplicación y
pruebas sin decidir datos durante la implementación: ni una constante, ni una
evidencia, ni un caso nuevos.

## Cierre de implementación — 2026-09-16

- `CharacterBuild` (`1.1.0`), `SaveBuildUseCase`, `applyLoadedBuild` en WPF,
  `build.schema.json` `1.1.0` y el smoke quedaron integrados y probados.
- El flujo de UI recupera una build completa a la Calculadora y recalcula sus
  atributos derivados como si el usuario hubiera distribuido los puntos.
- Una prueba de integración de Application, la actualización de las pruebas de
  promoción y Data, y las 780/780 pruebas de la solución pasan.

## Siguiente vertical documentada

Continúan pendientes, sin abrirse aquí: master buys y las pantallas restantes
del flujo. Requieren datos factuales (ítems, skills, inventario) que hoy no
existen; cualquier avance debe esperar a sus contratos factuales y no puede
inventarse.
