# Changelog

## [Unreleased]

### Added

- Migración SQLite hacia adelante `1/create_build_drafts` y
  `SqliteBuildDraftRepository` como implementación Data de
  `IBuildDraftRepository`. Cada alta/reemplazo confirma payload JSON y metadata
  en una sola fila/transacción inmediata; la carga permanece como `SELECT` sin
  mutaciones.
- Traducción del agotamiento de `SqliteWriteContentionPolicy` al código estable
  `build-draft-write-conflict`, sin filtrar tipos SQLite hacia Application.
- Seis pruebas de integración Data con archivos temporales para payload y
  metadata exactos, reemplazo por ID, rollback ante fallo sintético, reapertura,
  ausencia sin mutaciones y conflicto de escritura tipado. No agregan datos ni
  fórmulas de MU Online.
- Modelo serializable `BuildDraft` en Application alineado campo por campo con
  el schema `1.0.0`, contexto runtime explícito para ruleset, dataset/hash y
  motor, y seis códigos de error estables `build-draft-*`.
- Puerto `IBuildDraftRepository` sin tipos SQLite y casos de uso de guardado y
  carga. La carga recalcula progresión y distribución desde las entradas y
  asignaciones, compara toda la caché y falla cerrada ante identidad,
  dependencia o resultado divergente.
- Siete pruebas sintéticas de Application con repositorio en memoria para
  alta/carga, reemplazo por ID, ausencia, identidad incoherente, dependencia no
  disponible, caché alterada y round-trip JSON exacto. Data, migraciones y WPF
  permanecen fuera de esta vertical.
- Contrato JSON Schema 2020-12 `build-draft.schema.json` `1.0.0` para identidad
  estable, metadata exacto de ruleset/dataset/motor, entradas de progresión y
  un `StatDistribution` compuesto mediante `$ref`.
- Fixtures sintéticos válido/inválido de borrador. El inválido conserva válido
  el envoltorio y falla por la distribución referenciada, demostrando que el
  validador resuelve el contrato compartido.
- Especificación previa a persistencia para autoridad de campos, revalidación al
  cargar, errores estables, límite Application/Data, transacción atómica y
  casos mínimos, sin crear puertos, tablas ni migraciones.
- Flujo WPF de distribución de stats: conserva el
  `ProgressionPointBudgetResult` calculado, genera los inputs desde los
  `StatIds` materializados e invoca `CalculateStatDistributionUseCase` sin
  recalcular progresión ni duplicar nombres o valores del juego.
- Resultado visible con puntos gastados, restantes y asignaciones por ID, más
  traducción en español de los seis errores tipados sin ocultar su código
  estable. Los cambios de identidad, nivel o Hero Status invalidan el
  presupuesto anterior.
- Gate del artefacto publicado para una distribución sintética de un punto
  derivada del snapshot empaquetado. Ambas fases verifican gasto, remanente y
  conjunto de stats sin agregar fixtures factuales ni persistencia.
- Caso de uso `CalculateStatDistributionUseCase` en Application: recibe el
  presupuesto existente y las asignaciones, resuelve una única definición de
  clase del mismo ruleset y delega en el motor sin aceptar clases alternativas
  ni recalcular progresión.
- Cuatro pruebas de integración recorren copia temporal del snapshot → catálogo
  → caso de uso → motor para distribuciones sintéticas parcial/exacta, fallo
  cerrado `budget-source-mismatch` ante origen incoherente y propagación del
  código `allocation-negative` producido por el motor.
- Operación pura `StatDistributionCalculator` en Calculation Engine para
  validar asignaciones no confiables y derivar puntos gastados/remanentes desde
  el presupuesto de progresión, con suma y resta comprobadas de 64 bits.
- Tipos de solicitud/resultado, excepción y seis códigos de error estables en
  Domain para negativos, stats ajenos u omitidos, exceso de presupuesto,
  overflow y divergencias de origen.
- Diez pruebas sintéticas de distribución cubren los seis casos mínimos del
  contrato, las divergencias de ruleset/clase/regla y overflow; una prueba de
  integración adicional confirma que Application materializa los IDs de stats
  directamente desde el snapshot.
- Contrato JSON Schema 2020-12 `stat-distribution.schema.json` `1.0.0` para
  conservar presupuesto ganado, referencia de progresión, asignaciones por
  stat, puntos gastados y remanente, más fixtures sintéticos válido/inválido.
- Especificación previa al código de invariantes, límite entero de 64 bits,
  errores estables y seis casos sintéticos mínimos para la futura distribución;
  `command` se resuelve exclusivamente desde los stats de la clase canónica.
- Cobertura del séptimo contrato en el validador .NET y en la comprobación
  estructural PowerShell: 7 schemas y 14 fixtures.
- Primer flujo funcional WPF para seleccionar clase, evolución, nivel y Hero
  Status desde el catálogo publicado, calcular mediante Application y mostrar
  total, regla/version y traza sin duplicar valores del juego en XAML o C#.
- Empaquetado del snapshot canónico completo en
  `rulesets/mu-s4-global-reference/v1` de la publicación WPF, con resolución
  exclusiva desde la carpeta del artefacto.
- Gate de publicación que carga el snapshot con el adaptador productivo,
  reproduce 7/7 casos positivos y 3/3 rechazos en ambas fases y compara los 18
  JSON por SHA-256 durante el reemplazo simulado.
- Proyecto `MuOnline.BuildPlanner.Application` con adaptador JSON para
  materializar el catálogo de progresión ya validado y caso de uso mínimo que
  invoca `ProgressionPointBudgetCalculator` sin depender de WPF ni SQLite.
- Gate productivo de snapshot con errores tipados para directorios o contenido
  inválidos, IDs duplicados, mezcla de rulesets, reglas no `PUBLISHED` y
  referencias clase/regla incoherentes.
- Doce pruebas de integración de Application que leen entradas y resultados
  desde los diez casos canónicos, reproducen 7/7 resultados y 3/3 rechazos, y
  demuestran fallo cerrado con una regla `REVIEWED` y una referencia inexistente
  en copias temporales.
- Primera vertical productiva del presupuesto de puntos en los proyectos
  `MuOnline.BuildPlanner.Domain` y `MuOnline.BuildPlanner.CalculationEngine`:
  resuelve por clase una única regla `PUBLISHED`, valida evolución, nivel y
  Hero Status, y devuelve puntos ganados con aportes trazables por nivel/quest.
- Errores de dominio tipados para clase/evolución/nivel, resolución de reglas y
  elegibilidad de quest, con los mismos códigos estables de los controles
  canónicos.
- Doce pruebas del motor cargan las definiciones JSON canónicas sin duplicar el
  ruleset: reproducen los siete casos positivos y tres rechazos de progresión,
  fijan la traza 1145+10 del caso de nivel 230 y demuestran que una regla no
  publicada no se ejecuta.
- Gate semántico para `testCaseRefs` de progresión: cada enlace debe resolver a
  un caso positivo del mismo ruleset y regla, y una regla publicada debe cubrir
  todos sus casos positivos. La prueba fija la asignación exacta 5+2 y mantiene
  fuera los tres controles negativos.
- Siete fixtures factuales versionados para los casos aprobados de progresión en
  nivel 1/220/221/230 y MG/DL en nivel 220, con IDs estables y provenance hacia
  `RES-0001`.
- Validación ejecutable de los casos de progresión en el tooling: confirma
  0/1095/1095/1101/1155/1533 puntos y rechaza controles de Hero Status con clase
  base, Magic Gladiator o Dark Lord.
- Ocho registros canónicos `VERIFIED` para
  `mu-s4-global-reference`: seis clases con stats/evoluciones trazados y las
  reglas `progression-five-per-level-hero-status` y
  `progression-seven-per-level`, separadas de los fixtures sintéticos.
- Validación integral de los registros canónicos contra los contratos de clase
  y progresión, más una prueba que fija el conjunto de ocho IDs estables.
- Documentación del paquete de ruleset, su convención de IDs y el gate de casos
  ejecutables mediante `testCaseRefs` resolubles.
- Contrato JSON Schema 2020-12 `progression-rule.schema.json` `1.0.0` para
  puntos por nivel y bonus opcional de quest con elegibilidad y retroactividad
  explícitas, más fixtures sintéticos válido e inválido.
- Cobertura del sexto contrato en el validador .NET, sus pruebas de contrato y
  la comprobación estructural PowerShell: 6 schemas y 12 fixtures.
- `EVD-0021` como decisión del propietario que fija la matriz completa de
  `RES-0001` como axioma estable del ruleset y habilita su implementación.
- `EVD-0019` y `EVD-0020` en `RES-0001`: transcripciones históricas Season 1/3
  con la matriz completa, seis guías fechadas en 2010 y auditoría CDX de sus
  primeras capturas disponibles, sin promover claims ni fixtures.
- `EVD-0014`–`EVD-0018` en `RES-0001` para las seis matrices de stats base,
  stats distribuibles, puntos por nivel, Hero Status/Marlon, contraste oficial
  de Webzen y confirmación del propietario.
- Matriz candidata `STR/AGI/VIT/ENE[/CMD]`, fórmulas de puntos acumulados y seis
  casos numéricos de investigación, sin promover fixtures productivos.
- `DSP-0002` para Energy inicial de Magic Gladiator: 26 en MU Online Fanz y la
  decisión del propietario frente a 16 en la guía actual de Webzen.
- Integración reproducible de Json Everything en el build normal del validador:
  commits, SDK `10.0.301`, locks fuente, SPDX, provenance, hashes y licencia MIT
  son entradas versionadas; los DLL se generan bajo `artifacts/` y no se guardan
  en Git.
- Control CI de publicación del validador que exige hashes revisados, aviso MIT,
  referencias directas y ausencia de `OSMFEULA.txt` y metadatos de los paquetes
  NuGet publicados.
- Spike reproducible de compilación propia de Json Everything desde los commits
  fuente MIT fijados: dos rutas independientes producen hashes idénticos,
  generan SBOM/provenance/locks, pasan dos veces 14/14 fixtures y rechazan una
  prueba aislada de formatos inválidos, sin cambiar todavía el grafo normal.
- Evaluación reproducible de Json Everything para el validador: registra el
  `OSMFEULA.txt` idéntico de los tres paquetes, su hash, commits fuente, alcance
  real y comparación entre binario NuGet, compilación propia MIT y Corvus.
- Copia normalizada a LF de `OSMFEULA.txt` bajo `legal/tooling/json-everything`
  para que las condiciones observadas no dependan de la caché local de NuGet.
- ADR-0005, aceptado por el propietario, que licencia el material original del
  proyecto bajo Apache License 2.0 y separa expresamente marcas, evidencias y
  contenido de terceros.
- Texto oficial Apache-2.0 en `LICENSE.md`, atribución y límites en `NOTICE`, e
  inventario inicial de dependencias en `THIRD-PARTY-NOTICES.md`.
- Reglas operativas para licenciar contribuciones y exigir licencia, avisos e
  inspección del artefacto antes de una distribución.
- Empaquetado legal de la publicación WPF con avisos del proyecto,
  Microsoft.Data.Sqlite, SQLitePCLRaw y runtime packs autocontenidos; el smoke
  verifica diez archivos no vacíos y sus hashes tras el reemplazo simulado.
- Proyecto WPF mínimo `MuOnline.BuildPlanner.App` para .NET 10 y `win-x64`, con
  ventana técnica sin datos del juego y referencia unidireccional hacia Data.
- Modo headless y script de smoke de publicación que verifican SQLite nativo,
  migración, round-trip, backup/restore, integridad y persistencia de una base
  externa a través de una copia de reemplazo de los binarios.
- Job Windows `wpf-publication-smoke` en CI para auditar dependencias, publicar y
  ejecutar el artefacto autocontenido en un runner limpio.
- ADR-0004, aceptado por el propietario, con comparación reproducible entre Blazor
  WebAssembly PWA y WPF .NET 10 para la primera UI/distribución; documenta
  offline, empaquetado, actualización, destinos/RIDs, SQLite nativo, reversión y
  pruebas requeridas sin implementar todavía una UI.
- Publicaciones de plantillas vacías en Release: Blazor PWA con 26,52 MiB/289
  archivos y WPF autocontenido `win-x64` con 139,26 MiB/400 archivos, registradas
  como medidas de empaquetado mínimo y no como estimaciones del producto.
- Spikes equivalentes de TypeScript/Node.js 24 y C#/.NET 10 con cálculo
  sintético trazable, validación, persistencia SQLite y pruebas reproducibles.
- ADR-0002, aceptado por el propietario, que selecciona C#/.NET 10.
- Schemas JSON 2020-12 `1.0.0` para evidencia, fórmula, clase de personaje,
  perfil de servidor y build, con diez fixtures sintéticos.
- Registro `RES-0001` para clases, evoluciones, estadísticas, puntos por nivel y
  Marlon, sin datos factuales publicados.
- Nota de alcance `season-4-reference-scope.md` con ID
  `mu-s4-global-reference`, reglas de evidencia y fronteras oficiales.
- `EVD-0005`, contraste técnico comunitario contemporáneo de códigos y cadenas
  de clase, conservado como `PARTIAL` por no declarar Season 4.
- `EVD-0006` y `EVD-0007`, dos guías oficiales de Webzen sobre quests y alas,
  junto con diez claims atómicos de familias/evoluciones clasificados `PARTIAL`.
- `EVD-0008` y `EVD-0009`, dos registros técnicos comunitarios, y una matriz de
  contraste técnico para `WZ-CLM-001`–`WZ-CLM-010`.
- `EVD-0010`, locator oficial del instalador inglés
  `MU1_03 full(Eng).exe` y un mirror archivado que lo clasifica como cliente
  Season 4, con resultado negativo para binario descargable y hasheable.
- `EVD-0011` y `EVD-0012`, auditoría de las páginas de segunda y tercera clase
  de MU Online Fanz y de sus primeras capturas archivadas disponibles.
- `DSP-0001`, inconsistencia nominal abierta entre `Soul Wizard` en la página de
  segunda clase y `Soul Master` como entrada de la página de tercera clase.
- `EVD-0013`, página de cuarta clase de MU Online Fanz que sitúa Soul Wizard
  después de Grand Master en el contenido actual, sin atribución a Season 4.
- Solución mínima .NET 10 y validador integral JSON Schema Draft 2020-12 para
  los cinco contratos `1.0.0` y sus diez fixtures sintéticos.
- Dos pruebas de contrato, restauración bloqueada y workflow CI para compilar y
  probar en Release mediante Microsoft Testing Platform.
- Evaluación comparativa de `Microsoft.Data.Sqlite`, EF Core SQLite y
  `System.Data.SQLite` para .NET 10, con licencias, soporte nativo, política de
  versiones, estrategia de migraciones y pruebas mínimas.
- ADR-0003, aceptado por el propietario, para usar
  `Microsoft.Data.Sqlite 10.0.10` con
  `SQLitePCLRaw.bundle_e_sqlite3 2.1.12` fijado, sin incorporar aún dependencias
  al producto.
- Smoke test aislado `win-x64` de SQLite: round-trip transaccional con SQLite
  `3.53.3` y auditoría transitiva sin vulnerabilidades para el grafo fijado.
- Proyecto `MuOnline.BuildPlanner.Data` con runner de migraciones SQLite,
  ledger de versión/nombre/SHA-256/fecha UTC, transacciones individuales y
  detección tipada de catálogos alterados o incompletos.
- Proyecto de integración con cinco pruebas sintéticas para base nueva,
  reapertura, reejecución, hash alterado y rollback ante fallo SQL.
- Lock files obligatorios para los cuatro proyectos y referencias productivas
  fijadas a `Microsoft.Data.Sqlite 10.0.10` y SQLitePCLRaw `2.1.12`.
- `SqliteBackupService` con backup online mediante copia candidata, verificación
  `PRAGMA integrity_check`, reemplazo posterior a validación y restauración
  verificada de la base completa.
- `SqliteBackupIntegrityException` y tres pruebas de integración sintéticas para
  consistencia, recuperación de schema/ledger/datos y preservación del último
  backup válido ante una candidata corrupta.
- `SqliteWriteContentionPolicy` con transacción inmediata, timeout positivo por
  intento, reintentos finitos para `SQLITE_BUSY`/`SQLITE_LOCKED`, restauración
  del timeout de conexión y error de agotamiento tipado.
- Cuatro pruebas sintéticas para rechazo de timeout ilimitado, espera acotada
  ante un segundo escritor, commit único después de liberar el bloqueo y
  rollback sin reintento de errores SQLite ajenos a contención.

### Changed

- El validador integral, la prueba de contrato, la comprobación PowerShell y el
  harness de compilación fuente cubren ahora 8 schemas y 16 fixtures; el
  harness autocompilado aprueba dos ejecuciones 16/16 y la prueba de formatos.
- `ProgressionPointBudgetResult` conserva ahora `CharacterClassId`, y
  `CharacterProgressionDefinition` los IDs de stats del snapshot, para validar
  la procedencia y disponibilidad sin duplicar datos factuales.
- `MuOnline.BuildPlanner.App` referencia Application de forma unidireccional y
  su lock registra sólo las nuevas dependencias internas de proyecto.
- Las reglas `progression-five-per-level-hero-status` y
  `progression-seven-per-level` pasan de `REVIEWED` a `PUBLISHED` tras enlazar
  respectivamente cinco y dos casos factuales aprobados.
- La matriz de `RES-0001` reemplaza los seis IDs provisionales por IDs
  definitivos con prefijo `class-` y registra la materialización canónica sin
  cambiar la clasificación individual de ninguna fuente.
- `RES-0001` queda cerrado: `CLM-0001`–`CLM-0006` pasan a `VERIFIED`; la
  búsqueda histórica deja de ser un gate para clases, evoluciones, stats base,
  puntos por nivel y Marlon.
- `DSP-0001` queda resuelto a favor de Dark Wizard → Soul Master → Grand Master,
  con Soul Wizard fuera de Season 4. `DSP-0002` conserva Energy 26 para Magic
  Gladiator y su divergencia documental.
- El modelo de confianza admite axiomas de ruleset aprobados explícitamente por
  el propietario, sin reclasificar la calidad o independencia de las fuentes.
- La política permanente de fuentes queda alineada con la decisión del
  propietario del 2026-07-19: Fanz sigue como fuente inicial prioritaria y se
  autorizan fuentes adicionales con provenance, versión y confianza propias.
- `RES-0001` pasa de 18 a 20 evidencias. `CLM-0002` y `CLM-0003` permanecen
  `PARTIAL` porque no se localizó un original de Webzen ni un snapshot
  contemporáneo que demuestre continuidad de la matriz hasta Season 4.
- `DSP-0002` queda resuelto por decisión explícita del propietario: el proyecto
  adopta `ENE 26` para Magic Gladiator (`26/26/26/26`). El valor 16 de Webzen se
  conserva como divergencia documental de otra versión no demostrada, sin
  bloquear el valor elegido ni promover el conjunto completo a `VERIFIED`.
- `CLM-0002`, `CLM-0003`, `CLM-0005` y `CLM-0006` pasan de no investigados a
  `PARTIAL`; los seis claims de `RES-0001` tienen ahora cobertura candidata y
  ninguno está todavía `VERIFIED`.
- Por decisión del propietario del 2026-07-19, MU Online Fanz continúa como
  fuente inicial prioritaria y se autorizan fuentes adicionales para extracción,
  contraste y resolución de conflictos con clasificación individual de versión.
- El validador dejó de referenciar `JsonSchema.Net` desde NuGet. Su lock normal
  sólo resuelve `Humanizer.Core 3.0.10`; `JsonSchema.Net.dll`,
  `JsonPointer.Net.dll` y `Json.More.dll` proceden de la compilación fuente
  fijada y pasan localmente 14/14 pruebas y la inspección de publicación.
- CI fija el SDK `.NET 10.0.301` para reproducir los hashes del pipeline fuente;
  la verificación del workflow actualizado en runner limpio queda pendiente.
- Los binarios NuGet de Json Everything quedan limitados al desarrollo interno
  histórico y ya no se resuelven en el grafo normal. La compilación propia desde
  los commits MIT fijados es la ruta integrada; Corvus queda como contingencia.
- El proyecto WPF obtiene los avisos de .NET/Windows Desktop/ASP.NET desde los
  runtime packs exactos resueltos por MSBuild, evitando acoplar la distribución
  a la versión instalada localmente.
- La licencia deja de ser una decisión abierta. Una release sigue condicionada
  a empaquetar y probar los avisos de los binarios realmente distribuidos; la
  familia Json Everything usada por el validador declara MIT para el código
  fuente pero incorpora `OSMFEULA.txt` para determinados usos de sus binarios.
- Por aclaración del propietario, `NOTICE` usa atribución colectiva a los
  contribuidores sin atribución personal adicional. La aplicación queda
  definida como herramienta de apoyo sin autenticación, cuentas ni credenciales
  de MU Online.
- Por decisión del propietario, el repositorio pasa a visibilidad pública.
  `main` queda protegido con PR obligatorio para administradores, checks
  estrictos `build-and-test` y `wpf-publication-smoke`, historial lineal,
  resolución de conversaciones y bloqueo de force-push/borrado. En ese momento
  la licencia seguía pendiente; ADR-0005 la resuelve posteriormente sin tratar
  la visibilidad como permiso sobre material de terceros.
- La rama predeterminada remota `chore/bootstrap-repository` se renombró a
  `main` conservando el commit `3935d9b`, y el tracking local quedó normalizado.
  GitHub rechazó activar branch protection con `403` porque el repositorio
  privado requiere GitHub Pro o visibilidad pública; no se cambió ninguna de
  esas opciones y el flujo de ramas/PR/checks queda como control operativo.
- Primera ejecución remota de CI aprobada en GitHub Actions (`run 29666817493`):
  14/14 pruebas en Linux y smoke WPF en Microsoft Windows Server 2025 con SDK
  .NET `10.0.302`, SQLite `3.53.3`, 407 archivos y 148.442.430 bytes. La auditoría
  de los cinco proyectos no encontró paquetes vulnerables y ambas fases del
  smoke pasaron; el alcance permanece limitado a `win-x64`.
- El repositorio remoto quedó inicializado con el commit raíz `2e886c3`.
- La solución incorpora la shell WPF y su lock file. El smoke local `win-x64`
  pasó con SQLite `3.53.3`, 407 archivos y 148.339.336 bytes publicados; la
  primera ejecución del job remoto de CI también quedó aprobada.
- Por decisión del propietario del 2026-07-18, WPF .NET 10 autocontenido y
  `win-x64` quedan seleccionados para la primera UI/distribución mediante
  ADR-0004; SQLite integrado y persistencia ante reemplazo de binarios quedaron
  validados por el smoke local y remoto.
- Por decisión del propietario, Webzen pasa a ser la fuente prioritaria para
  información de MU Online y se elimina el requisito de contemporaneidad; se
  mantienen provenance, clasificación por versión, confianza y contraste.
- Todo el proyecto adopta Season 4 global/inglés como versión objetivo única por
  orden del propietario; episodio y `main` dejan de ser requisitos.
- `RES-0001`, gobierno, producto, roadmap y handoff quedan alineados con Season
  4 global/inglés, sin exigir episodio, y mantienen `PARTIAL` los claims aún no
  demostrados.
- El manifiesto de integridad se actualiza para reflejar la migración documental.
- `RES-0001` mantiene `CLM-0001`/`CLM-0004` en `PARTIAL` y dirige la siguiente
  investigación a la aplicabilidad histórica de MU Online Fanz al corte objetivo.
- La matriz de `RES-0001` separa ahora las asociaciones nominales confirmadas
  por Webzen de su aplicabilidad aún no demostrada a Season 4.
- `CLM-0001` y `CLM-0004` permanecen `PARTIAL` tras el contraste técnico y la
  auditoría negativa de independencia y procedencia global/inglesa.
- La auditoría de `EVD-0005`, `EVD-0008` y `EVD-0009` separa autor visible,
  publicador y origen del contenido; su independencia técnica y procedencia
  global/inglesa quedan no demostradas y todos los claims siguen `PARTIAL`.
- Por decisión del propietario, MU Online Fanz pasa a ser la fuente obligatoria
  para nueva información factual del juego. Webzen y las fuentes históricas se
  conservan para contexto, procedencia y contraste, y ninguna página se aplica
  a Season 4 sin clasificación explícita de versión.
- La auditoría histórica de MU Online Fanz se cierra con resultado negativo para
  Season 4: no hay capturas indexadas anteriores a 2023, las primeras ya mezclan
  clases posteriores y ningún claim se promueve.
- La cadena candidata del mago se clasifica como Dark Wizard → Soul Master →
  Grand Master para Season 4 por aclaración del propietario; Soul Wizard queda
  fuera del corte. Grand Master queda clasificado como incorporación de Season
  4 obtenida al culminar a nivel 400 la serie de quests de tercera clase;
  EVD-0012 confirma la estructura actual, no la frontera histórica.
- La documentación de schemas describe ahora el validador integral, su CLI y
  los comandos reproducibles de prueba.
- La futura persistencia SQLite adopta, mediante ADR-0003, migraciones SQL hacia
  adelante con ledger/hash y backup
  previo a cambios destructivos, manteniendo el proveedor dentro de Data.
- La solución incorpora los proyectos Data e integración conservando la
  separación del dominio; todos los scripts actuales son fixtures sintéticos.

### Fixed

- `global.json` fija exactamente el SDK `10.0.301` con `rollForward: disable` y
  CI declara la versión como cadena. Así un SDK `10.0.302` preinstalado en el
  runner no reemplaza al SDK revisado durante el build reproducible.
- El harness de compilación fuente valida ahora los 16 fixtures de los ocho
  contratos actuales en cada ejecución, en lugar de conservar inventarios
  anteriores.
- El pipeline reproducible de Json Everything fuerza checkouts LF y elimina
  símbolos/PDB de los DLL auditados. Así los hashes ya no dependen de
  `core.autocrlf` ni de checksums de fuentes propios del sistema operativo.
- GitHub Actions `run 29697921106` confirma la corrección en los jobs
  `build-and-test` y `wpf-publication-smoke`; el run anterior `29697684666`
  queda trazado como detección del defecto CRLF.
- Se fija `eol=lf` para archivos de texto mediante `.gitattributes`, evitando
  que checkouts con `core.autocrlf` invaliden los hashes de `MANIFEST.sha256`.
- El validador usa un registro de schemas aislado por ejecución y reutiliza cada
  contrato entre sus fixtures válido e inválido, evitando colisiones de `$id`.
- El proyecto de pruebas importa xUnit explícitamente y `dotnet test` selecciona
  Microsoft Testing Platform, por lo que CI descubre y ejecuta las pruebas.
- La evaluación SQLite evita el grafo transitivo predeterminado que resolvió
  `SQLitePCLRaw.lib.e_sqlite3 2.1.11` con `NU1903`; el pin aprobado a `2.1.12`
  restaura y audita sin vulnerabilidades conocidas.

## [0.1.0-docs] - 2026-07-18

### Added

- Constitución, alcance y reglas de gobierno.
- Requisitos funcionales y no funcionales.
- Roadmap maestro y criterios de salida.
- Arquitectura propuesta y modelo de dominio.
- Política de investigación, fuentes y confianza.
- Estrategia de pruebas, seguridad y calidad.
- Sistema de traspaso humano/IA.
