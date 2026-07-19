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
  smoke publicado. Su primera ejecución en un runner limpio todavía no se observó.
- `RES-0001` abierto para clases, evoluciones, stats, puntos por nivel y Marlon.
- Trece evidencias registradas en `RES-0001`; dos guías oficiales de Webzen
  produjeron diez claims atómicos de familias/evoluciones, y registros técnicos
  y páginas de MU Online Fanz aportan sólo contraste con alcance incompleto.
  `CLM-0001` y `CLM-0004` permanecen `PARTIAL` y ningún dato fue publicado.
- Webzen fue establecido inicialmente como fuente prioritaria sin requisito de
  contemporaneidad; la decisión posterior de fuente obligatoria MU Online Fanz
  limita ahora Webzen a contexto, procedencia y contraste. Cada hallazgo
  conserva clasificación de versión, trazabilidad y confianza.
- Auditoría de procedencia completada para `EVD-0005`, `EVD-0008` y
  `EVD-0009`: sólo se demostró separación de publicaciones, no independencia
  del contenido ni origen global/inglés. `EVD-0010` localiza un cliente inglés
  Season 4 oficial y un mirror archivado, pero no un binario hasheable.
- Por decisión posterior del propietario, MU Online Fanz es la fuente
  obligatoria para toda nueva información factual del juego. Las demás fuentes
  se conservan para contexto, procedencia y contraste.
- Auditoría histórica completada para las páginas de segunda y tercera clase de
  MU Online Fanz. Las primeras capturas disponibles son de marzo de 2023, ya
  mezclan clases posteriores y abren `DSP-0001` por `Soul Wizard` frente a
  `Soul Master`; EVD-0011/EVD-0012 permanecen `PARTIAL`.
- El propietario aclaró que la cadena del mago en Season 4 termina en Dark
  Wizard → Soul Master → Grand Master; Grand Master se incorporó en esa
  temporada y se obtiene tras la serie de quests de tercera clase culminada a
  nivel 400. Soul Wizard es posterior. EVD-0012/EVD-0013 confirman la estructura
  actual, pero no la frontera histórica; la clasificación sigue `PARTIAL`.
- Alcance anterior retirado y documentación migrada a Season 4 global/inglesa.

## No iniciado

- Núcleo de dominio y aplicación productiva; los componentes actuales son el
  validador de schemas, Data y una shell WPF técnica sin flujos de producto.
- Investigación de stats iniciales/distribuibles, puntos por nivel y Marlon.
- Schemas restantes y validador JSON Schema integral/CI.
- Dataset, motor de cálculo y UI funcional.

## Decisiones abiertas

- Licencia.

## Verificación más reciente — 2026-07-18

- Schemas: 5/5 contratos y 10/10 fixtures estructuralmente legibles.
- Validador integral: 5/5 fixtures válidos aceptados y 5/5 inválidos
  rechazados; 2/2 pruebas .NET aprobadas, incluida ejecución repetida en el
  mismo proceso.
- Solución .NET 10 de cinco proyectos: restauración bloqueada y build Release aprobados con 0
  advertencias y 0 errores; CLI del validador y formato verificados.
- Persistencia SQLite: 12/12 pruebas de integración aprobadas en `win-x64`;
  junto con las 2 pruebas de schemas, la solución ejecuta 14/14 pruebas
  correctamente.
- Dependencias: los cinco proyectos restauran con lock files. El proyecto WPF no
  añade paquetes y conserva el grafo Data previamente auditado; la consulta
  externa explícita no se repitió porque la política del entorno rechazó enviar
  metadatos del grafo a NuGet.
- Spike C#: 4/4 comprobaciones aprobadas con .NET SDK 10.0.301.
- Node.js y `pwsh` no están disponibles en el `PATH`; la prueba de schemas se
  ejecuta con Windows PowerShell.
- No se incorporaron datos ni fórmulas factuales al producto.
- Investigación documental: 13 evidencias registradas, incluidos 2 guías
  oficiales de nombres, 1 locator oficial de cliente y 3 páginas de MU Online
  Fanz; 10 claims atómicos `PARTIAL` y ninguna promoción a `VERIFIED`.
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
  `ok`, 1 migración aplicada/1 reconocida y datos fuera de los binarios. Falta
  observar el job nuevo en un runner Windows limpio.

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
  publicación integrado; no se aprueban otros RIDs por inferencia.
