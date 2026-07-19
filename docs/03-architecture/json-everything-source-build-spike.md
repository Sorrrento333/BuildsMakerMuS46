# Spike de compilación propia de Json Everything

- Estado: completado e integrado localmente en el grafo normal.
- Fecha de corte: 2026-07-19.
- Alcance: herramienta interna `MuOnline.SchemaValidator`; no modifica ni
  incorpora el validador a la aplicación WPF.
- Fuente: repositorio oficial `json-everything/json-everything`.

## Objetivo y límites

Demostrar que los tres ensamblados usados por el validador pueden producirse de
forma independiente desde la fuente MIT fijada por los paquetes actuales, con
provenance, inventario, auditoría y paridad sobre los contratos del repositorio.
El spike no constituye por sí mismo una aprobación jurídica. La integración
posterior sí reemplazó el grafo restaurado y se documenta en
`json-everything-source-integration.md`.

## Insumos fijados

| Componente | Versión | Commit declarado |
|---|---:|---|
| JsonSchema.Net | 9.2.2 | `8f112c415735af9519a5b478447ad8239fa18642` |
| JsonPointer.Net | 7.0.1 | `8b8ab34027de5ad9f4ed50808b8e4889ca69cf4d` |
| Json.More.Net | 3.0.1 | `8b8ab34027de5ad9f4ed50808b8e4889ca69cf4d` |

El árbol de `JsonPointer` y `Json.More` no presenta diferencias entre ambos
commits. La licencia fuente MIT revisada tiene SHA-256
`baebca0309090f4eca1b7a82c836cc91e48b2b92139c2280fb0ff69af922c2ae`
con checkout LF explícito.
La ejecución demostrada usa SDK .NET `10.0.301` y `net10.0`.

## Implementación reproducible

`tools/spikes/Test-JsonEverythingSourceBuild.ps1` realiza lo siguiente:

1. exige la versión de SDK indicada y obtiene dos checkouts independientes del
   commit exacto;
2. comprueba el segundo commit y la igualdad de los dos subárboles transitivos;
3. restaura únicamente `net10.0` con lock files y compila sin modificar la
   fuente, aislando las reglas de análisis del repositorio anfitrión;
4. aplica `Deterministic`, `ContinuousIntegrationBuild` y un `PathMap` estable;
5. exige hashes idénticos de los tres DLL entre rutas fuente diferentes;
6. ejecuta dos veces los diez fixtures y una prueba aislada que exige rechazo
   de formatos `uri` y `date` inválidos;
7. genera SHA-256, provenance JSON, SPDX 2.3, lock files, licencia fuente,
   metadatos de Humanizer y salida de auditoría bajo `artifacts/` ignorado.

El harness reutiliza exactamente `SchemaContractValidator.cs` mediante un
archivo enlazado y referencias directas a los DLL recién compilados. No usa los
binarios NuGet de Json Everything para esa ejecución.

## Resultado observado

- dos compilaciones en rutas independientes: 0 advertencias, 0 errores;
- hashes idénticos entre ambas rutas:
  - `Json.More.dll`: `450646267cc10ad4cbf33f1f568787874675642055376d12bed01a8d1044739e`;
  - `JsonPointer.Net.dll`: `8f7be030e432b255f81812ec7b774c6827fdc06c0a6833313f6f653b82a550f5`;
  - `JsonSchema.Net.dll`: `1f2dc6dfad8d76594c7e7107d13b4cad54a135e45dcd8f677c803ed169b4be55`;
- dos ejecuciones consecutivas: 10/10 fixtures cada una;
- prueba aislada de `uri`/`date`: rechazada como se esperaba;
- auditoría de NuGet: ningún paquete vulnerable para `JsonSchema` en los
  orígenes consultados;
- runtime del harness: los tres DLL propios más `Humanizer.Core 3.0.10` MIT.

El SPDX también registra los paquetes usados sólo durante compilación —como
SourceLink y PolySharp— sin confundirlos con el runtime copiado. Los hashes son
evidencia para estos commits, SDK, TFM y opciones; no se extrapolan a otro SDK.

## Decisión técnica resultante

La ruta de compilación propia queda técnicamente viable y preferida frente a
distribuir los binarios NuGet evaluados. La integración posterior cambió la
solución: el validador usa referencias directas a estos DLL, su lock normal sólo
resuelve `Humanizer.Core 3.0.10` y la publicación inspeccionada no contiene
`OSMFEULA.txt` ni metadatos de los paquetes publicados. Los locks fuente, el
SPDX y la provenance pasaron a ser entradas versionadas de CI.

El validador continúa fuera de WPF. La verificación local está completa y falta
confirmar el workflow actualizado en un runner remoto limpio. El spike no añade
información ni fórmulas de MU Online.
