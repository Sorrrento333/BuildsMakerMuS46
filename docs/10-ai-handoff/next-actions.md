# CONTENIDO UNIFICADO (rebase: union preservadora reset-aware-drafts-traced-formulas + origin/main)

# Próximas acciones

## Regla de planificación

Cada tarea prioritaria debe ser una vertical coherente y verificable que, cuando
el estado técnico lo permita, incluya diseño, implementación, integración,
pruebas, documentación y smoke. No dividir una capacidad salvo que exista un
gate real que impida continuar de forma segura.

La instrucción de ejecutar sólo la primera tarea pendiente continúa vigente.

## Prioridad inmediata

1. Cerrar la documentación y el commit de la vertical de Defense/SD de Summoner
   quedó pendiente solo de capturar el hash y la versión definitiva del dataset
   (`2026-07-29.4`) con el smoke WPF de publicación. Ese smoke ya pasó localmente
   el 2026-09-01 con SQLite `3.53.3`, 738 archivos, 149.256.211 bytes, 315 JSON
   del ruleset, treinta fórmulas y 120 casos contextuales; el hash es
   `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.
   Sólo resta el commit y, en su caso, la ejecución del CJ en un runner remoto.
2. Definir la siguiente vertical coherente entre los candidatos documentados:
   primeros contratos facticos restantes (daño y wizardry de Summoner/MG ya
   aprobados y sin motor), o los schemas de alto nivel (ruleset, quests, ítems,
   skills, escenarios y trazas), o builds completas/flujos de UI posteriores.

## Última tarea cerrada

La vertical de Defense/SD de Summoner quedó cerrada sobre la decisión general
`RAW` del propietario y sobre los truncamientos independientes de `EVD-0032`:

- `formula-defense-summoner` y `formula-sd-summoner` `1.0.0` están
  `PUBLISHED` contra schema `2.1.0` y trazan `EVD-0021`, `EVD-0026`, `EVD-0032`
  y `EVD-0034` (conflicto `DSP-0004` conservado y resuelto).
- Defense conserva `agility / 3` sobre los mínimos STR 21/AGI 21/VIT 18/ENE 23.
  SD conserva los tres truncamientos independientes
  `trunc((str+agi+vit+ene)*1.2) + trunc(defense/2) + trunc((lvl*lvl)/30)`,
  consumiendo Defense `RAW` por `FORMULA_OUTPUT`.
- `CHECKED_DECIMAL_V1` y sus gates aceptan varios pasos intermedios
  `APPLY_ROUNDING`; el último paso visible sigue siendo el redondeo que consume
  `rawOutputStepId`. El intérprete entero se relaja de forma inocua.
- Cuatro casos de Defense y cuatro de SD reproducen outputs/trazas.
  `sd-summoner-base` fija SD 102 y discrimina la semántica independiente (a
  plena precisión sería 103). No hay frontera RAW/VISIBLE para Summoner.
- Application y WPF materializan treinta fórmulas ejecutables; el dataset avanza
  a `2026-07-29.4` con hash
  `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.

## Verificación del cierre

- Restauración y build Release aprobados con 0 advertencias/0 errores; 320/320
  pruebas pasan: 40 validator, 58 motor, 204 Application y 18 Data.
- CLI del validador: las treinta fórmulas `PUBLISHED` pasan sin errores,
  incluidos `formula-defense-summoner` (4 positivos/2 controles) y
  `formula-sd-summoner` (4 positivos/7 controles).
- Smoke WPF `win-x64` (30 fórmulas y 120 casos contextuales): PASS local el
  2026-09-01 con SQLite `3.53.3`, 738 archivos, 149.256.211 bytes, 315 JSON del
  ruleset y hash `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.

## Primera acción concreta

Confirmar con el mantenedor la siguiente vertical elegida entre los candidatos
documentados (contratos facticos restantes, schemas de alto nivel o builds/
flujos de UI) y actualizar esta documentación y `CHANGELOG.md` al cerrarla.

---

## Aportes propios de nuestra rama (no presentes en origin/main)

# Próximas acciones

## Regla de planificación

Cada tarea prioritaria debe ser una vertical coherente y verificable que, cuando
el estado técnico lo permita, incluya diseño restante, implementación,
integración, pruebas, documentación y smoke aplicable. No dividir una misma
capacidad en iteraciones separadas de diseño, código y UI salvo que exista un
gate real que impida continuar de forma segura.

La instrucción de ejecutar sólo la primera tarea pendiente continúa vigente,
pero esa primera tarea debe producir avance funcional observable y dejar el
repositorio en un punto completo, no limitarse a un paso documental intermedio.

## Prioridad inmediata

1. Completar la siguiente vertical factual de Fase 4 para AG de la familia
   Dark Lord. Partir exclusivamente del claim `DR-AG-DARK-LORD`,
   `EVD-0021` y `EVD-0026`: revisar
   aplicabilidad, inputs, límites, orden,
   truncamiento y casos; asignar ID/version; materializar la definición
   ejecutable y referencias; extender gates, Application, WPF y smoke hasta un
   resultado trazable. No inferir bases, dependencias o redondeos fuera de la
   expresión aprobada
   `ene * 0.15 + vit * 0.1 + agi * 0.2 + str * 0.3 + cmd * 0.3`. Reutilizar
   `resolved-energy`, `resolved-vitality`, `resolved-agility` y
   `resolved-strength`, incorporar `resolved-command` sólo por la ruta
   contextual genérica ya definida, y reutilizar el intérprete decimal
   versionado y la selección
   genérica sin handlers, bases o fórmulas factuales en C#. Conservar los
   coeficientes exactamente, no redondear aportes y truncar únicamente en la
   salida declarada.

## Última tarea cerrada

Se cerró la vertical funcional de AG de Magic Gladiator desde
`DR-AG-MAGIC-GLADIATOR` hasta WPF. `formula-ag-magic-gladiator` `1.0.0`
materializa exactamente `ene * 0.15 + vit * 0.3 + agi * 0.25 + str * 0.2`,
conserva `EVD-0021`, `EVD-0026` y `DSP-0002`, y aplica a Magic
Gladiator/Duel Master.

La definición usa `2.1.0`/`CHECKED_DECIMAL_V1`, conserva los cuatro
coeficientes exactamente, no redondea aportes y trunca una sola vez en
`visible-ag`. No consume nivel ni dependencias. Sus cuatro positivos fijan
`23.40/23`, `23.85/23`, `23.85/23` y `24.30/24`; cinco controles cubren
mínimos y familia sin inventar un overflow imposible. Application y smoke
ejecutan la referencia desde JSON; WPF reutiliza la selección genérica sin
handlers ni constantes factuales en C#. El dataset avanza a `2026-07-28.2`;
ruleset y motor permanecen en `1.0.0`/`0.2.0`.

## Verificación del cierre

- Estado Git inicial: árbol con cambios modificados y no rastreados de las
  verticales anteriores; se preservaron sin descartar ni reescribir trabajo.
- Restauración bloqueada inicial: aprobada.
- Build Release inicial: 0 advertencias, 0 errores.
- Solución .NET inicial: 211/211 pruebas aprobadas.
- Build Release final: 0 advertencias, 0 errores.
- Solución .NET final: 211/211 pruebas aprobadas: 40 validator, 56 motor, 97
  Application y 18 Data. Las pruebas dinámicas recorren los 68 positivos, 76
  negativos y 68 casos contextuales.
- Restauración bloqueada final: aprobada.
- Comprobación estructural PowerShell: 11 contratos y 22 fixtures aprobados.
- CLI integral: 11 fixtures válidos/11 inválidos, 26 registros canónicos,
  progresión 7/7+3/3 y dieciocho definiciones de fórmula aprobadas.
- Smoke WPF `win-x64`: PASS con SQLite `3.53.3`, 611 archivos,
  149.076.719 bytes, 10 avisos legales y 188/188 JSON idénticos entre fases.
  Aprobó progresión 7/7+3/3, distribución, resets, backup/restore, borrador y
  68/68 casos positivos de HP/Mana/AG con ambas trazas. El dataset
  `2026-07-28.2` produjo
  `sha256:5246861cec04e5e618611091d365e7e0a4c03d8227013f84c13e93354253d901`.
- `dotnet format --verify-no-changes`: aprobado.
- `git diff --check`: aprobado.

## Primera acción concreta

Ejecutar completa la primera prioridad: entregar AG de Dark Lord funcional y
trazable desde el claim verificado hasta WPF/smoke, sin inferir datos,
dependencias ni redondeos fuera de `EVD-0021` y `EVD-0026`.
