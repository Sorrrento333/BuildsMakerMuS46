# ADR-0004: WPF para la primera distribución local

- Estado: Aceptado por el propietario.
- Fecha: 2026-07-18.
- Responsables: propietario del proyecto; comparación asistida por IA.
- Decisión aprobada: propietario del proyecto, 2026-07-18.

## Contexto

ADR-0002 seleccionó C#/.NET 10 y dejó la primera interfaz entre Blazor web/PWA
y escritorio. ADR-0003 incorporó SQLite nativo y exige publicar y ejecutar un
smoke test por cada RID finalmente soportado.

La primera distribución debe funcionar offline, conservar builds localmente por
defecto, respetar el monolito modular y no introducir un segundo modelo de
persistencia sin evidencia. Esta decisión compara candidatos concretos y no
declara soportado ningún RID adicional sin sus pruebas de publicación.

## Límites de la evidencia

La comparación usa documentación oficial y dos publicaciones de plantillas
vacías con SDK `10.0.301` en Windows `win-x64`. Los tamaños observados no estiman
el producto final. Ningún prototipo incluyó la capa Data, una base SQLite, datos
de usuario ni información factual de MU Online.

La inspección local confirmó que las plantillas `wpf` y `blazorwasm --pwa`
están disponibles. Después de aceptar el ADR se ejecutó la publicación integrada
con `Microsoft.Data.Sqlite`; el resultado se registra en la sección de
verificación posterior.

## Opciones consideradas

### A. Blazor WebAssembly standalone PWA sobre .NET 10

- Candidato reproducible: `dotnet new blazorwasm --framework net10.0 --pwa`.
- Empaquetado: conjunto de archivos estáticos que requiere un host HTTPS para
  instalación y activación del service worker.
- Offline: la plantilla cachea la aplicación publicada después de una primera
  visita con conectividad. El soporte offline no está activo durante desarrollo.
- Actualización: el service worker comprueba en segundo plano un nuevo snapshot
  de recursos y lo activa después de cerrar las instancias de la versión previa.
- Persistencia: el estado en memoria del navegador no sobrevive por sí mismo a
  recargas o cierres. La persistencia local de builds y del snapshot SQLite
  requeriría una solución de almacenamiento del navegador y pruebas propias.
- Runtime: el destino relevante es `browser-wasm`, no un RID de escritorio. El
  bundle bloqueado contiene un asset `browser-wasm`, pero no se demostró con el
  producto su carga, persistencia entre sesiones, backup ni restauración.
- Publicación mínima observada: 27.811.523 bytes (26,52 MiB), 289 archivos.
- Ventaja: distribución estática multiplataforma a través de navegadores
  compatibles y actualización de recursos incorporada en el modelo PWA.
- Riesgo principal: obliga a resolver y validar una frontera de persistencia
  distinta de la ruta de archivos locales ya probada por Data.

### B. WPF sobre .NET 10, autocontenido para Windows x64

- Candidato reproducible: `dotnet new wpf --framework net10.0`.
- Empaquetado inicial: publicación autocontenida por carpeta para `win-x64`,
  comprimida como ZIP. No requiere que el usuario instale el runtime .NET.
- Offline: proceso y archivos locales sin servidor, navegador ni primera visita.
- Actualización inicial: descarga manual de una versión completa, cierre de la
  aplicación y reemplazo atómico de los binarios; la base de usuario debe vivir
  fuera de la carpeta de la aplicación. MSIX/Store puede evaluarse antes de una
  distribución pública que requiera actualización administrada y firma.
- RIDs: sólo `win-x64` para la primera distribución. `win-arm64` queda fuera del
  soporte hasta publicar y ejecutar su propio smoke test. WPF es sólo Windows.
- SQLite: `win-x64` coincide con el RID probado para el grafo bloqueado y con el
  smoke test posterior del ejecutable WPF publicado con Data integrada.
- Publicación mínima observada: 146.022.710 bytes (139,26 MiB), 400 archivos.
- Ventaja: reutiliza directamente la capa Data, sus migraciones, backup y
  política de contención sobre un archivo local.
- Riesgo principal: limita la primera UI a Windows y distribuye un artefacto
  mayor; el canal de actualización pública sigue sin decidirse.

## Comparación

| Criterio | Blazor WebAssembly PWA | WPF `win-x64` autocontenido |
|---|---|---|
| Offline inicial | Requiere publicación HTTPS y primera visita conectada | Funciona desde archivos locales |
| Persistencia SQLite actual | Integración y persistencia en navegador no demostradas | Ruta de archivo compatible; publicación integrada pendiente |
| Empaquetado mínimo observado | 26,52 MiB / 289 archivos estáticos | 139,26 MiB / 400 archivos |
| Actualización | Snapshot del service worker | ZIP manual inicialmente; MSIX/Store por evaluar |
| Alcance de plataforma | Navegadores compatibles | Sólo Windows |
| Destino/RID inicial | `browser-wasm` | `win-x64` |
| Cambio arquitectónico | Nueva frontera de almacenamiento local | Reutiliza Data sin cambiar su frontera |

Los tamaños corresponden a plantillas vacías en Release y no son comparables
como rendimiento, tiempo de descarga ni huella final instalada.

## Decisión

Usar **WPF sobre .NET 10**, publicado por carpeta y autocontenido para
`win-x64`, como primera interfaz y distribución técnica. El ZIP será sólo el
canal de desarrollo/preview; el canal público de actualización se decidirá antes
de una release para usuarios.

La decisión prioriza la ruta de persistencia SQLite ya implementada y reduce
incertidumbre antes de construir el dominio y el motor. No afirma que WPF sea la
UI permanente ni rechaza una PWA futura para consulta o distribución adicional.

El propietario aceptó esta decisión el 2026-07-18. La implementación comienza
con el smoke test integrado definido abajo; la aceptación no demuestra todavía
el soporte del artefacto publicado.

## Reproducción de la comparación

Ejecutar desde una carpeta temporal que no herede la gestión central de paquetes
del repositorio, con SDK .NET `10.0.301` o un parche posterior de .NET 10:

```powershell
dotnet new wpf --name WpfCandidate --framework net10.0
dotnet restore WpfCandidate/WpfCandidate.csproj --runtime win-x64
dotnet publish WpfCandidate/WpfCandidate.csproj --configuration Release `
  --runtime win-x64 --self-contained true

dotnet new blazorwasm --name PwaCandidate --framework net10.0 --pwa
dotnet restore PwaCandidate/PwaCandidate.csproj
dotnet publish PwaCandidate/PwaCandidate.csproj --configuration Release
```

Contar todos los archivos publicados y sumar sus longitudes. Registrar SDK, OS,
RID, fecha, comandos y cualquier descarga. La primera ejecución necesitó acceso
a NuGet para runtime packs y paquetes de plantilla; después la publicación PWA
se repitió con `--no-restore`.

## Pruebas requeridas si se acepta WPF

1. Crear el proyecto UI sin referencias desde Domain, Calculation Engine o Data
   hacia WPF.
2. Publicar en Release, autocontenido y por carpeta para `win-x64` con
   restauración bloqueada.
3. Ejecutar el artefacto en una máquina Windows x64 limpia y sin red.
4. Abrir SQLite desde la capa Data, comprobar versión nativa, migrar, escribir,
   cerrar, reabrir y leer un fixture exclusivamente sintético.
5. Verificar backup/restore e `integrity_check` desde el artefacto publicado.
6. Confirmar que la base de usuario está fuera de la carpeta de binarios y
   sobrevive a una actualización simulada.
7. Auditar dependencias, licencias y vulnerabilidades del grafo publicado.
8. Probar teclado, escalado y contraste antes de aceptar flujos de UI.

`win-arm64` sólo podrá añadirse repitiendo las pruebas 2–7 en hardware o runner
nativo de ese RID. No se inferirá soporte por la mera presencia de assets NuGet.

## Verificación posterior a la aceptación — 2026-07-18

Se incorporaron `MuOnline.BuildPlanner.App` y
`tools/smoke-tests/Test-WpfPublishedArtifact.ps1`. El script publicó por carpeta
en Release, autocontenido y `win-x64` con `--no-restore`, y ejecutó directamente
el `.exe` en dos ubicaciones de binarios con un único directorio externo de datos.

Resultado observado con SDK `10.0.301` y red restringida por el entorno:

- SQLite nativo `3.53.3` cargado desde el artefacto;
- migración sintética aplicada una vez y reconocida tras el reemplazo;
- escritura, cierre, reapertura y lectura correctos;
- backup verificado, mutación deliberada y restauración con
  `PRAGMA integrity_check = ok`;
- base y backup fuera de ambas carpetas de binarios;
- valor sintético conservado al ejecutar desde la copia de reemplazo;
- 407 archivos publicados y 148.339.336 bytes (141,47 MiB).

La solución restauró en modo bloqueado, compiló Release sin advertencias y pasó
14/14 pruebas existentes. La consulta explícita local de
vulnerabilidades a NuGet no se repitió porque la política del entorno rechazó
enviar el grafo al servicio externo; el proyecto WPF no añade paquetes y su lock
file conserva el grafo Data previamente auditado.

La primera ejecución remota se completó después en GitHub Actions (`run
29666817493`, 2026-07-19 UTC). El job `wpf-publication-smoke` pasó en Microsoft
Windows Server 2025, imagen `windows-2025-vs2026`, runner `2.335.1` y SDK .NET
`10.0.302`. La auditoría directa y transitiva informó cero paquetes vulnerables
para los cinco proyectos en los orígenes consultados. El artefacto `win-x64`
cargó SQLite `3.53.3` y produjo 407 archivos/148.442.430 bytes; las fases
`initialize` y `verify-update` superaron las aserciones de ambos reportes. El job
Linux `build-and-test` del mismo run también pasó las 14 pruebas. Esta evidencia
demuestra sólo `win-x64` y no autoriza otros RIDs.

## Pruebas requeridas si se elige PWA en su lugar

Además de las pruebas de arquitectura y accesibilidad aplicables, habrá que
demostrar publicación HTTPS, instalación, arranque offline tras la primera
visita, actualización coherente del service worker, persistencia de builds tras
cerrar el navegador y compatibilidad real de SQLite, migraciones y backup en el
almacenamiento persistente elegido. Sin esas pruebas no se reutilizará el estado
de soporte `win-x64` para `browser-wasm`.

## Consecuencias

- La primera UI quedaría limitada a Windows x64 y mantendría puro el núcleo.
- El tamaño mayor se acepta provisionalmente a cambio de ejecución sin runtime
  preinstalado; trimming y single-file no se habilitan sin pruebas específicas.
- La base de usuario y sus backups deberán ubicarse en un directorio de datos
  estable, nunca junto a binarios reemplazables.
- Firma, instalador y canal de actualización pública siguen siendo decisiones de
  distribución posteriores, no supuestos de este ADR.

## Plan de reversión

Mantener Application y los contratos sin tipos WPF. Si el smoke test integrado
falla o el propietario prioriza multiplataforma, rechazar o reemplazar este ADR
antes de implementar flujos de UI y ejecutar el spike PWA con persistencia real.

## Fuentes primarias

- [PWA de Blazor y comportamiento offline](https://learn.microsoft.com/en-us/aspnet/core/blazor/progressive-web-app/?view=aspnetcore-10.0)
- [Estado en Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/webassembly?view=aspnetcore-10.0)
- [Descripción y límite de plataforma de WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [Publicación autocontenida y single-file](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
- [Catálogo de RIDs de .NET](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)
- [Empaquetado de aplicaciones Windows](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/packaging/)
- [Microsoft.Data.Sqlite y bundles nativos](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions)
