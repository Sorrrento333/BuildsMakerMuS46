# Spike: compilación propia de Json Everything

Este pipeline comprueba una ruta de compilación independiente desde la fuente
MIT de Json Everything y genera los ensamblados consumidos por el validador
normal. No contiene datos ni fórmulas de MU Online.

Desde la raíz del repositorio:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/spikes/Test-JsonEverythingSourceBuild.ps1
```

El script exige el SDK indicado, obtiene y verifica los commits fijados, copia
los locks revisados, restaura con `--locked-mode`, compila en dos rutas fuente
independientes con `PathMap`, compara hashes y el SPDX revisado,
ejecuta dos veces los veinte fixtures y una
prueba aislada de formatos `uri`/`date`, audita paquetes y genera bajo
`artifacts/json-everything-source-build/`:

- los tres ensamblados y `Humanizer.dll`;
- licencia MIT fuente y metadatos de Humanizer;
- `SHA256SUMS` y `provenance.json`;
- `sbom.spdx.json`, lock files y auditoría JSON que debe quedar sin entradas de
  vulnerabilidad.

`artifacts/` está ignorado: los binarios no se guardan en Git ni se incorporan a
la aplicación WPF. El validador los consume como referencias directas. Después
del build, la publicación se inspecciona con:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/validators/Test-JsonEverythingSourceIntegration.ps1
```
