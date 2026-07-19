# Próximas acciones

## Prioridad inmediata

1. Normalizar el bootstrap Git: renombrar la rama predeterminada remota
   `chore/bootstrap-repository` a `main`, activar su protección y conservar desde
   entonces ramas cortas y PR obligatoria para código, schemas y datos. No
   reescribir el commit raíz validado ni ampliar el alcance técnico durante esta
   operación administrativa.
2. Mantener bloqueado el diseño de fixtures de personajes: `CLM-0001` y
   `CLM-0004` continúan `PARTIAL`, y `DSP-0001` permanece abierto.
3. No iniciar stats, puntos por nivel ni Marlon hasta disponer de evidencia de
   MU Online Fanz aplicable a Season 4 o una nueva decisión del propietario.

## Última tarea cerrada

Se publicó el commit raíz `2e886c3` en `chore/bootstrap-repository` para ejecutar
por primera vez el workflow remoto. El run `29666817493` terminó correctamente:
`build-and-test` pasó 14/14 pruebas en Linux y `wpf-publication-smoke` pasó en
Microsoft Windows Server 2025 con SDK .NET `10.0.302`.

La auditoría no encontró paquetes vulnerables en ninguno de los cinco proyectos.
El artefacto autocontenido `win-x64` cargó SQLite `3.53.3`, publicó 407 archivos
y 148.442.430 bytes, y ambas fases del smoke validaron migración, reapertura,
backup/restore, integridad y persistencia externa. No se incorporaron datos ni
fórmulas de MU Online ni se habilitaron RIDs adicionales.

## Primera acción concreta

En la configuración del repositorio GitHub, renombrar la rama predeterminada
`chore/bootstrap-repository` a `main` y activar protección que exija PR y los dos
checks de CI. Después, actualizar el tracking local sin rebase ni reescritura y
registrar la política efectiva en el handoff.

No completar stats ni implementar la fórmula de puntos hasta aprobar los claims
de clases/evoluciones.
