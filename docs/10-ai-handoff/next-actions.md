# Próximas acciones

## Prioridad inmediata

1. Ejecutar el job `wpf-publication-smoke` del workflow CI en un runner Windows
   limpio y registrar tanto la auditoría de dependencias como el smoke publicado.
   No ampliar los RIDs aunque el job pase; ADR-0004 sólo acepta `win-x64`.
2. Mantener bloqueado el diseño de fixtures de personajes: `CLM-0001` y
   `CLM-0004` continúan `PARTIAL`, y `DSP-0001` permanece abierto.
3. No iniciar stats, puntos por nivel ni Marlon hasta disponer de evidencia de
   MU Online Fanz aplicable a Season 4 o una nueva decisión del propietario.

## Última tarea cerrada

Se implementó la primera vertical WPF autorizada por ADR-0004. La shell mínima
incluye un modo headless y un script que publica autocontenido para `win-x64`,
ejecuta SQLite desde el artefacto y simula una actualización usando una segunda
carpeta de binarios con los mismos datos externos.

El smoke local pasó con SQLite `3.53.3`, integridad `ok`, migración aplicada una
vez, backup/restore, reapertura y conservación del valor sintético. La
publicación produjo 407 archivos y 148.339.336 bytes. La solución pasa 14/14
pruebas y el job Windows quedó agregado a CI. No se incorporaron datos ni
fórmulas de MU Online.

## Primera acción concreta

Publicar los cambios en un entorno que ejecute GitHub Actions y comprobar que el
job `wpf-publication-smoke` termina correctamente en `windows-latest`. Registrar
SDK, SQLite, cantidad/tamaño de archivos y el resultado de ambos reportes; si
falla, corregir únicamente portabilidad/empaquetado antes de iniciar una UI
funcional.

No completar stats ni implementar la fórmula de puntos hasta aprobar los claims
de clases/evoluciones.
