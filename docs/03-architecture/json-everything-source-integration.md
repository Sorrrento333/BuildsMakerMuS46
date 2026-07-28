# Integración de Json Everything compilado desde fuente

- Estado: completada y verificada localmente y en GitHub Actions.
- Fecha de corte: 2026-07-19.
- Alcance: `MuOnline.SchemaValidator` y sus pruebas; WPF continúa fuera.
- Fuente: commit fijado del repositorio oficial
  `json-everything/json-everything`, bajo MIT.

## Grafo resultante

El validador ya no declara `JsonSchema.Net` como `PackageReference`. Consume
`JsonSchema.Net.dll`, `JsonPointer.Net.dll` y `Json.More.dll` como referencias
directas producidas por el pipeline fuente. Su único paquete de runtime propio
es `Humanizer.Core 3.0.10`, bajo MIT.

El lock normal del validador contiene sólo `Humanizer.Core`. En la publicación,
los tres componentes Json Everything aparecen en `deps.json` con tipo
`reference`, no `package`. No se almacenan DLL en Git.

## Insumos revisados

`spikes/json-everything-source-build/` conserva como entradas de CI:

- commits fuente y SDK `.NET 10.0.301` en `source-build-provenance.json`;
- hashes esperados de los tres ensamblados y de la licencia MIT;
- locks de `Json.More`, `JsonPointer` y `JsonSchema` para `net10.0`;
- inventario SPDX 2.3 de dependencias de compilación y runtime;
- licencia MIT fuente preservada también bajo
  `legal/tooling/json-everything-source/MIT.txt`.

El hash completo revisado de `Microsoft.Build.Tasks.Git 10.0.201` tiene 88
caracteres Base64. Un primer traslado manual truncó un carácter; el restore
bloqueado lo rechazó con `NU1403`, y los tres locks se corrigieron sólo después
de compararlos línea por línea con locks regenerados desde el origen configurado.

## Pipeline

`tools/spikes/Test-JsonEverythingSourceBuild.ps1` obtiene dos checkouts
independientes del commit exacto, inyecta los locks revisados, restaura con
`--locked-mode`, compila con `PathMap`, compara los hashes esperados y ejecuta
dos veces los veintidós fixtures más la prueba de formatos `uri`/`date`. También
contrasta el inventario generado con el SPDX revisado y rechaza
`OSMFEULA.txt` en la salida runtime.

Los checkouts fijan `core.autocrlf=false` y `core.eol=lf`, y la compilación
deshabilita símbolos/PDB. De este modo los hashes revisados no dependen de la
configuración Git ni de los checksums de fin de línea del runner.

Después del build normal,
`tools/validators/Test-JsonEverythingSourceIntegration.ps1` comprueba el lock
del validador, los tres hashes, publica la herramienta y exige el aviso MIT,
las referencias directas y la ausencia de `OSMFEULA.txt` y `.nuspec` de los
paquetes publicados.

CI instala el SDK `10.0.301` y `global.json` lo selecciona exactamente con
`rollForward: disable`; no basta con instalarlo mientras un `latestFeature`
pueda preferir otro SDK ya presente en el runner. Después ejecuta primero el
pipeline fuente y luego restore bloqueado, build, pruebas e inspección de
publicación. El job WPF no adquiere esta dependencia y mantiene su verificación
separada.

El harness enlazado al validador ejecuta los once contratos actuales: 22/22
fixtures en cada una de dos rutas fuente independientes, más la prueba explícita
de formatos.

## Verificación local

La ejecución del 2026-07-19 aprobó:

- 2 compilaciones fuente, 0 advertencias y 0 errores;
- hashes esperados para 3/3 DLL;
- 2 × 10/10 fixtures y rechazo de formatos inválidos;
- restore bloqueado y build Release de la solución;
- 14/14 pruebas .NET;
- 5 schemas y 10 fixtures estructuralmente legibles;
- auditoría directa/transitiva sin vulnerabilidades en los orígenes consultados;
- publicación con los tres DLL, `Humanizer.dll` y `JsonEverything-MIT.txt`, sin
  `OSMFEULA.txt` ni metadatos de los paquetes publicados.

La primera ejecución remota, `run 29697684666`, expuso que los hashes iniciales
dependían del checkout CRLF de Windows. Tras fijar LF y eliminar símbolos/PDB,
el `run 29697921106` sobre el commit `d0626d2` aprobó `build-and-test` y
`wpf-publication-smoke`. El workflow fijó SDK `10.0.301`; sus selectores fueron
`ubuntu-latest` y `windows-latest`. La API pública consultada no expuso la
versión interna exacta de esas imágenes, por lo que no se infiere.

## Límite

Esta integración no incorpora datos ni fórmulas de MU Online y no autoriza por
sí sola a distribuir la aplicación WPF con el validador. Cualquier incorporación
a otro artefacto requiere repetir su inspección legal y técnica.
