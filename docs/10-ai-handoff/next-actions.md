# Próximas acciones

## Prioridad inmediata

1. Resolver con el propietario la licencia del repositorio público y registrar
   la decisión mediante ADR: MIT, Apache-2.0 o propietaria. No inferir permisos
   por la visibilidad pública ni publicar una release antes de esa decisión.
2. Mantener bloqueado el diseño de fixtures de personajes: `CLM-0001` y
   `CLM-0004` continúan `PARTIAL`, y `DSP-0001` permanece abierto.
3. No iniciar stats, puntos por nivel ni Marlon hasta disponer de evidencia de
   MU Online Fanz aplicable a Season 4 o una nueva decisión del propietario.

## Última tarea cerrada

El propietario hizo público el repositorio. La API confirmó `visibility: public`
y permitió completar branch protection en `main`.

La protección exige PR incluso a administradores, checks estrictos
`build-and-test` y `wpf-publication-smoke`, historial lineal y resolución de
conversaciones; force-push y borrado están deshabilitados. No se incorporaron
datos ni fórmulas de MU Online.

## Primera acción concreta

El propietario debe elegir **MIT**, **Apache-2.0** o **licencia propietaria**.
Después, crear y aprobar el ADR de licencia, sustituir el marcador actual de
`LICENSE.md` por el texto correspondiente y revisar atribuciones antes de una
release.

No completar stats ni implementar la fórmula de puntos hasta aprobar los claims
de clases/evoluciones.
