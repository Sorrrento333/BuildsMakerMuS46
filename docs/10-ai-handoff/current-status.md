# Estado actual

## Alcance activo

- Versión objetivo única: **MU Online Season 4 global/inglesa**.
- ID: `mu-s4-global-reference`.
- Estado de decisión: `APPROVED` por el propietario el 2026-07-18.
- Episodio y número de `main` no son requisitos ni bloqueos.
- El número exacto de `main` no es una prioridad ni un bloqueo.
- Todo dato y fórmula conserva requisitos individuales de provenance, versión,
  confianza y pruebas.

## Completado

- Diseño documental v1.0: visión, alcance, requisitos, roadmap, arquitectura,
  dominio, estrategia de datos, investigación, pruebas, seguridad y continuidad.
- Spikes equivalentes de TypeScript/Node.js 24 y C#/.NET 10 para cálculo
  sintético trazable, SQLite y ejecución offline.
- C#/.NET 10 seleccionado para el núcleo y la aplicación inicial (ADR-0002).
- Cinco schemas JSON 2020-12 `1.0.0`: evidencia, fórmula, clase de personaje,
  perfil de servidor y build, con diez fixtures sintéticos.
- Solución mínima .NET 10, validador integral con `JsonSchema.Net 9.2.2`, dos
  pruebas de contrato y CI básico para los cinco schemas y sus diez fixtures.
- Evaluación de Json Everything completada. Las copias de `OSMFEULA.txt` de
  `JsonSchema.Net 9.2.2`, `JsonPointer.Net 7.0.1` y `Json.More.Net 3.0.1` son
  idénticas y quedaron preservadas con provenance y hash. Los binarios NuGet se
  mantienen sólo para desarrollo interno y no están aprobados para distribución.
  La ruta preferida es demostrar una compilación propia reproducible desde los
  commits fuente MIT; Corvus queda como contingencia aún no validada.
- Spike de compilación propia completado: dos checkouts independientes, SDK
  `.NET 10.0.301`, `net10.0` y `PathMap` produjeron hashes idénticos para los
  tres DLL. El harness directo pasó dos veces 10/10 fixtures y rechazó formatos
  `uri`/`date` inválidos; generó SPDX, provenance, locks, hashes y auditoría.
  La ruta quedó integrada localmente y los binarios NuGet publicados fueron
  retirados del grafo normal.
- Integración fuente de Json Everything completada localmente: commits, SDK,
  locks, SPDX, hashes y licencia MIT son entradas versionadas de CI; los DLL se
  generan bajo `artifacts/` y no se guardan en Git. El validador usa referencias
  directas y su lock sólo resuelve `Humanizer.Core 3.0.10`.
- La publicación inspeccionada del validador contiene los tres DLL, Humanizer y
  `JsonEverything-MIT.txt`; no contiene `OSMFEULA.txt`, `.nuspec` de los
  paquetes publicados ni dependencias Json Everything de tipo `package`. Falta
  confirmar el workflow actualizado en un runner remoto limpio.
- Evaluación productiva de SQLite para .NET 10 completada. ADR-0003, aceptado
  por el propietario el 2026-07-18, selecciona
  `Microsoft.Data.Sqlite 10.0.10` con
  `SQLitePCLRaw.bundle_e_sqlite3 2.1.12`, migraciones SQL propias y aislamiento
  en Data; ambas dependencias quedaron incorporadas en la primera vertical.
- El smoke test aislado detectó que el grafo predeterminado de
  `Microsoft.Data.Sqlite 10.0.10` resuelve SQLitePCLRaw `2.1.11`, bloqueado por
  `NU1903`. El pin `2.1.12` pasó round-trip en `win-x64`, cargó SQLite `3.53.3`
  y no presentó vulnerabilidades conocidas en la auditoría de NuGet.
- Primera vertical productiva de Data implementada: proyecto SQLite, versiones
  centralizadas, lock files, ledger `schema_migrations`, SHA-256 por migración,
  transacciones individuales y errores de integridad tipados.
- Cinco pruebas de integración sintéticas cubren base nueva, persistencia tras
  reapertura, reejecución sin duplicados, rechazo de hash alterado y rollback de
  schema/ledger ante fallo SQL. No existe todavía un schema factual del juego.
- Segunda vertical Data implementada: `SqliteBackupService` crea una copia
  candidata con la API de backup online, exige `PRAGMA integrity_check = ok`
  antes de reemplazar un backup previo y verifica backup y destino al restaurar.
- Tres pruebas de integración adicionales demuestran backup consistente,
  recuperación de schema/ledger/datos sintéticos y preservación del último
  backup válido si falla la verificación de una candidata corrupta. El servicio
  aún no se conecta a migraciones destructivas.
- Tercera vertical Data implementada: `SqliteWriteContentionPolicy` adquiere una
  transacción inmediata con timeout positivo explícito, reintenta sólo
  `SQLITE_BUSY`/`SQLITE_LOCKED` y devuelve un error tipado al agotar el límite.
- Cuatro pruebas adicionales verifican rechazo de timeout cero, espera acotada
  ante un segundo escritor, reintento con un único commit después de liberar el
  bloqueo y rollback sin reintento para errores ajenos a contención. Application
  conserva la responsabilidad de configurar tiempos, mantener operaciones
  cortas/idempotentes y comunicar el fallo final.
- ADR-0004 aceptado por el propietario el 2026-07-18: selecciona WPF .NET 10
  autocontenido y `win-x64` para la primera UI/distribución por alinearse con la
  persistencia local ya implementada. No se creó todavía ningún proyecto UI.
- Dos publicaciones de plantillas vacías documentan el empaquetado mínimo
  observado: WPF `win-x64` autocontenido, 139,26 MiB/400 archivos; Blazor PWA,
  26,52 MiB/289 archivos. No incluyeron SQLite ni datos del juego y no sustituyen
  el smoke test integrado requerido tras la decisión.
- Proyecto WPF mínimo `MuOnline.BuildPlanner.App` incorporado sin referencias
  inversas desde Data y sin flujos factuales del juego. Su modo headless de
  publicación usa exclusivamente un schema y un valor sintéticos.
- Smoke test WPF integrado aprobado localmente para `win-x64`: publicación
  Release autocontenida con `--no-restore`, SQLite `3.53.3`, migración única,
  round-trip tras reapertura, backup/restore con integridad `ok` y conservación
  de la base externa al ejecutar desde binarios reemplazados.
- Workflow CI ampliado con un job Windows dedicado a auditoría de dependencias y
  smoke publicado. La primera ejecución remota quedó aprobada en GitHub Actions
  (`run 29666817493`, 2026-07-19 UTC): ambos jobs terminaron correctamente.
- `RES-0001` abierto para clases, evoluciones, stats, puntos por nivel y Marlon.
- Dieciocho evidencias registradas en `RES-0001`. Los seis claims están
  `PARTIAL`: clases/evoluciones, stats iniciales/distribuibles, puntos por nivel
  y Marlon tienen matrices candidatas, pero ningún dato fue publicado al producto.
- MU Online Fanz continúa como fuente inicial prioritaria. Por autorización del
  propietario del 2026-07-19 pueden usarse otras fuentes para extracción,
  contraste y resolución de conflictos; cada hallazgo conserva versión,
  trazabilidad y confianza propias.
- Auditoría de procedencia completada para `EVD-0005`, `EVD-0008` y
  `EVD-0009`: sólo se demostró separación de publicaciones, no independencia
  del contenido ni origen global/inglés. `EVD-0010` localiza un cliente inglés
  Season 4 oficial y un mirror archivado, pero no un binario hasheable.
- La restricción anterior de usar otras fuentes sólo como contexto fue ampliada
  por el propietario: Webzen y fuentes adicionales pueden aportar datos, sin
  eximir ninguna celda de demostrar aplicabilidad a Season 4.
- Auditoría histórica completada para las páginas de segunda y tercera clase de
  MU Online Fanz. Las primeras capturas disponibles son de marzo de 2023, ya
  mezclan clases posteriores y abren `DSP-0001` por `Soul Wizard` frente a
  `Soul Master`; EVD-0011/EVD-0012 permanecen `PARTIAL`.
- El propietario aclaró que la cadena del mago en Season 4 termina en Dark
  Wizard → Soul Master → Grand Master; Grand Master se incorporó en esa
  temporada y se obtiene tras la serie de quests de tercera clase culminada a
  nivel 400. Soul Wizard es posterior. EVD-0012/EVD-0013 confirman la estructura
  actual, pero no la frontera histórica; la clasificación sigue `PARTIAL`.
- Stats candidatos confirmados en orden `STR/AGI/VIT/ENE[/CMD]`: DW
  `18/18/15/30`, DK `28/20/25/10`, ELF `22/25/20/15`, SUM `21/21/18/23`, MG
  `26/26/26/26` y DL `26/20/20/15/25`. `VIT` normaliza `Stamina`; sólo DL posee
  `CMD`.
- DW/DK/ELF/SUM ganan 5 puntos por nivel y 6 tras completar Hero Status/Marlon
  desde nivel 220, con extra retroactivo por niveles posteriores a 220. MG/DL
  ganan 7 desde el inicio y no realizan Marlon. Son reglas candidatas `PARTIAL`.
- `DSP-0002` quedó resuelto por decisión explícita del propietario: el proyecto
  adopta Energy 26 para MG. La guía actual de Webzen que publica 16 permanece
  trazada como divergencia documental de aplicabilidad histórica no demostrada.
- Alcance anterior retirado y documentación migrada a Season 4 global/inglesa.

## No iniciado

- Núcleo de dominio y aplicación productiva; los componentes actuales son el
  validador de schemas, Data y una shell WPF técnica sin flujos de producto.
- Promoción a `VERIFIED` e implementación productiva de stats, puntos por nivel
  y Marlon; la investigación candidata está registrada pero sigue bloqueada.
- Schemas restantes y validador JSON Schema integral/CI.
- Dataset, motor de cálculo y UI funcional.

## Decisiones abiertas

- Ninguna decisión inmediata de arquitectura o gobierno. El canal público de
  actualización y firma continúa como decisión posterior de distribución.

## Verificación más reciente — 2026-07-19

- Schemas: 5/5 contratos y 10/10 fixtures estructuralmente legibles.
- Validador integral: 5/5 fixtures válidos aceptados y 5/5 inválidos
  rechazados; 2/2 pruebas .NET aprobadas, incluida ejecución repetida en el
  mismo proceso.
- Solución .NET 10 de cinco proyectos: restauración bloqueada y build Release aprobados con 0
  advertencias y 0 errores; CLI del validador y formato verificados.
- Persistencia SQLite: 12/12 pruebas de integración aprobadas en `win-x64`;
  junto con las 2 pruebas de schemas, la solución ejecuta 14/14 pruebas
  correctamente.
- Json Everything fuente: 2 compilaciones independientes con SDK `10.0.301`,
  restore bloqueado de los tres proyectos fuente, hashes esperados para 3/3
  DLL, 2 × 10/10 fixtures, prueba de formatos y SPDX contrastado: PASS.
- Integración del validador: lock con sólo `Humanizer.Core 3.0.10`, tres DLL
  clasificados como referencias directas, aviso MIT presente y ausencia de
  `OSMFEULA.txt`/`.nuspec` publicados en la salida: PASS local. GitHub Actions
  actualizado pero todavía no ejecutado remotamente para este cambio.
- Dependencias: los cinco proyectos restauran con lock files. El proyecto WPF no
  añade paquetes y conserva el grafo Data previamente auditado. La restauración
  bloqueada se repitió con acceso a NuGet y fue aprobada; el lock del validador
  resuelve únicamente `Humanizer.Core 3.0.10`, mientras los tres DLL Json
  Everything se compilan desde fuente fijada. La auditoría directa y transitiva
  no encontró paquetes vulnerables en ninguno de los cinco proyectos según los
  orígenes consultados.
- Spike C#: 4/4 comprobaciones aprobadas con .NET SDK 10.0.301.
- Node.js y `pwsh` no están disponibles en el `PATH`; la prueba de schemas se
  ejecuta con Windows PowerShell.
- No se incorporaron datos ni fórmulas factuales al producto.
- Licencia: texto Apache-2.0 contrastado con la publicación oficial; ADR-0005,
  `NOTICE` e inventario de terceros incorporados. La auditoría leyó metadatos
  `.nuspec` de todas las dependencias restauradas y el acuerdo incluido por la
  familia Json Everything.
- Investigación documental: 18 evidencias registradas; 6/6 claims principales
  `PARTIAL`, 0 `VERIFIED`, un conflicto abierto y uno resuelto. EVD-0014–EVD-0018
  trazan stats, puntos, Marlon, contraste oficial y decisiones del propietario.
- Contraste técnico: 2 registros comunitarios nuevos y matriz completa para
  `WZ-CLM-001`–`WZ-CLM-010`; la auditoría concluyó que su independencia y
  procedencia global/inglesa no son demostrables con los artefactos disponibles.
- Preservación técnica: localizado el nombre oficial `MU1_03 full(Eng).exe` y
  un mirror archivado que lo clasifica como cliente inglés Season 4 de 456.77
  MB; no se recuperó el binario ni un checksum del editor.
- MU Online Fanz: no hay capturas indexadas de rutas de segunda/tercera clase
  entre 2012 y 2022. Las primeras capturas de 2023 ya mezclan clases posteriores
  y no declaran Season 4; `DSP-0001` queda abierto y los fixtures bloqueados.
- Cadena candidata del mago para Season 4: Dark Wizard → Soul Master → Grand
  Master. La obtención candidata de Grand Master requiere culminar a nivel 400
  la serie de quests de tercera clase. Soul Wizard queda excluido por
  clasificación del propietario; falta evidencia histórica de Season 4.
- SQLite productivo: comparación documental de tres opciones y smoke test
  aislado con SDK `10.0.301`. El grafo recomendado fijado restauró sin
  advertencias, completó el round-trip transaccional en `win-x64` y pasó la
  auditoría transitiva. ADR-0003 fue aprobado y las verticales de migraciones,
  backup/restore y contención de escritura están implementadas; falta validar
  los RIDs que determine la futura decisión de distribución.
- UI/distribución: las plantillas WPF y Blazor WebAssembly PWA de .NET 10 están
  disponibles con SDK `10.0.301`; ambas publicaron en Release. La medición se
  limita a plantillas vacías y ADR-0004 conserva como pendientes la ejecución
  offline, SQLite integrado, actualización y persistencia de datos de usuario.
- Publicación WPF integrada: PASS local en `win-x64`, 407 archivos y
  148.339.336 bytes. Los dos reportes JSON confirman SQLite `3.53.3`, integridad
  `ok`, 1 migración aplicada/1 reconocida y datos fuera de los binarios.
- Publicación WPF con avisos: PASS local más reciente en `win-x64` con SDK
  `10.0.301`, runtime packs `10.0.9`, SQLite `3.53.3`, 417 archivos y
  148.507.425 bytes. Los diez archivos legales requeridos estuvieron presentes,
  no vacíos y conservaron sus hashes entre ambas carpetas de publicación. Una
  inspección adicional no encontró ensamblados Json Everything ni
  `OSMFEULA.txt` en el artefacto.
- Publicación WPF en runner limpio: PASS en Microsoft Windows Server 2025,
  imagen `windows-2025-vs2026`, runner `2.335.1` y SDK .NET `10.0.302`. El job
  auditó los cinco proyectos sin paquetes vulnerables en los orígenes
  consultados y el smoke publicó 407 archivos/148.442.430 bytes con SQLite
  `3.53.3`; las dos fases y sus reportes superaron todas las aserciones del
  script. Sólo `win-x64` queda demostrado; no se amplían RIDs.
- Repositorio remoto inicializado mediante el commit raíz `2e886c3`. La rama
  temporal `chore/bootstrap-repository` se renombró a `main` sin reescribir el
  historial; `main` es ahora la rama predeterminada y el tracking local/remoto
  apunta a `648989c` antes de este cierre documental.
- El propietario hizo público el repositorio el 2026-07-19. Branch protection
  quedó activa en `main`: PR obligatorio con administradores incluidos, checks
  estrictos `build-and-test` y `wpf-publication-smoke`, historial lineal,
  resolución de conversaciones y bloqueo de force-push/borrado. Se mantienen
  cero aprobaciones obligatorias mientras exista un único propietario.
- Apache-2.0 resuelve la licencia del material original desde el 2026-07-19. La
  visibilidad pública no amplía derechos sobre marcas, evidencias o contenido de
  terceros; una release requiere todavía empaquetar y probar sus avisos.
- `.gitattributes` fija LF para texto después de comprobar que un checkout con
  `core.autocrlf` invalidaba hashes del manifiesto; la verificación vuelve a ser
  reproducible sin cambiar el contenido documental.
- ADR-0005 aceptado por el propietario el 2026-07-19: el material original del
  proyecto queda bajo Apache License 2.0. `LICENSE.md`, `NOTICE` y
  `THIRD-PARTY-NOTICES.md` separan licencia propia, marcas/contenido externo y
  dependencias; no se relicencia información de MU Online ni de sus fuentes.
- La revisión inicial de atribuciones confirmó MIT para Microsoft.Data.Sqlite,
  Apache-2.0 para SQLitePCLRaw y dominio público declarado para SQLite. Detectó
  además `OSMFEULA.txt` en JsonSchema.Net/JsonPointer.Net/Json.More.Net: el
  validador no forma parte hoy de la publicación WPF. La evaluación posterior
  mantiene sus binarios NuGet limitados al desarrollo interno y no aprobados
  para distribución.
- El propietario aclaró el 2026-07-19 que busca continuidad comunitaria de
  código abierto y no una atribución personal. `NOTICE` atribuye el trabajo al
  conjunto de contribuidores. La aplicación se define como herramienta local de
  apoyo: no solicita, almacena ni transmite cuentas o credenciales de MU Online.
- El empaquetado legal WPF quedó implementado. La publicación incluye avisos del
  proyecto y de Microsoft.Data.Sqlite/SQLitePCLRaw, más licencia y avisos de los
  runtime packs exactos de .NET, Windows Desktop y ASP.NET. El smoke exige diez
  archivos no vacíos y hashes idénticos tras el reemplazo simulado.
- La evaluación técnica de Json Everything registró el texto contractual, su
  hash y los commits declarados por los paquetes. Los 10 fixtures actuales pasan
  con la API evaluada, pero los binarios NuGet publicados fueron retirados. La
  compilación propia MIT integrada demuestra hashes idénticos entre dos rutas
  fuente, 2 × 10/10 fixtures, formatos, SBOM, locks, auditoría y publicación
  inspeccionada. Corvus 4.6.7 se conserva como contingencia sin asumir paridad.

## Decisión del propietario — 2026-07-18

- El alcance se simplifica a Season 4 global/inglesa; no se exige episodio.
- El ruleset activo pasa a `mu-s4-global-reference`.
- Se elimina el requisito de fuente contemporánea para un juego histórico.
- Webzen fue establecido inicialmente como fuente prioritaria por ser
  creador/editor oficial de MU Online; la decisión posterior limita su uso.
- Siguen vigentes provenance, clasificación por versión, confianza y contraste.
- Decisión posterior: toda nueva información factual del juego se extrae de MU
  Online Fanz. Webzen y el resto quedan como fuentes de contexto, procedencia y
  contraste; MU Online Fanz tampoco se atribuye a Season 4 sin evidencia de
  versión.
- ADR-0004 aceptado: la primera UI/distribución será WPF .NET 10 autocontenida
  para `win-x64`. El soporte efectivo queda condicionado al smoke test de
  publicación integrado. El smoke local y el runner limpio ya lo demuestran para
  `win-x64`; no se aprueban otros RIDs por inferencia.
- ADR-0005 aceptado: el proyecto adopta Apache License 2.0 para su material
  original. Se conservan por separado los derechos y condiciones de terceros.
- Aclaración posterior: atribución colectiva a contribuidores, sin mención
  personal adicional del propietario; cuentas y credenciales de MU Online quedan
  fuera del alcance de la aplicación.

## Decisión del propietario — 2026-07-19

- Se autoriza consultar, extraer y contrastar fuentes adicionales a MU Online
  Fanz; se mantienen provenance, clasificación por versión, confianza y conflictos.
- Se aprueba como matriz candidata la tabla de stats de `RES-0001`, incluidos
  Energy 26 para MG y Command 25 para DL.
- Se confirma que MG/DL ganan 7 puntos por nivel desde el inicio y no realizan
  Marlon; DW/DK/ELF/SUM ganan 5 y pasan a 6 tras completar Hero Status desde 220.
- El propietario clasifica estos valores como invariantes entre versiones y
  resuelve `DSP-0002` a favor de Energy 26 para MG. El proyecto conserva
  `PARTIAL` hasta cerrar la evidencia histórica del conjunto.
