# Próximas acciones

## Prioridad inmediata

1. Resolver con el propietario la capacidad de proteger `main`: mantener el
   repositorio privado y habilitar GitHub Pro, o hacerlo público. No cambiar
   visibilidad ni contratar un plan por inferencia. Mientras tanto, usar ramas
   cortas, PR y los checks `build-and-test`/`wpf-publication-smoke` como controles
   operativos manuales.
2. Mantener bloqueado el diseño de fixtures de personajes: `CLM-0001` y
   `CLM-0004` continúan `PARTIAL`, y `DSP-0001` permanece abierto.
3. No iniciar stats, puntos por nivel ni Marlon hasta disponer de evidencia de
   MU Online Fanz aplicable a Season 4 o una nueva decisión del propietario.

## Última tarea cerrada

La rama predeterminada remota `chore/bootstrap-repository` se renombró a `main`
sin rebase ni reescritura; la rama local y su tracking quedaron alineados con el
mismo commit `3935d9b`.

Se intentó configurar PR obligatorio, checks estrictos `build-and-test` y
`wpf-publication-smoke`, historial lineal y bloqueo de force-push/borrado. GitHub
rechazó la operación con `403` porque branch protection para este repositorio
privado requiere GitHub Pro o visibilidad pública. No se alteró ninguno de esos
dos estados y no se incorporaron datos ni fórmulas de MU Online.

## Primera acción concreta

El propietario debe elegir entre: **(A)** conservar el repositorio privado y
habilitar GitHub Pro, o **(B)** hacerlo público. Tras esa decisión, activar en
`main` PR obligatorio, checks estrictos `build-and-test` y
`wpf-publication-smoke`, historial lineal, resolución de conversaciones y bloqueo
de force-push/borrado.

No completar stats ni implementar la fórmula de puntos hasta aprobar los claims
de clases/evoluciones.
