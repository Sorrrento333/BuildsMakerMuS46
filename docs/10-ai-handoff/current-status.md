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
- Diez schemas JSON 2020-12: siete en `1.0.0` y fórmula, distribución de
  stats/borrador de build en `1.1.0`, con veinte fixtures sintéticos.
- Solución mínima .NET 10, validador integral con `JsonSchema.Net 9.2.2`, dos
  pruebas de contrato y CI básico, ampliado a ocho schemas y dieciséis fixtures.
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
- `RES-0001` cerrado para clases, evoluciones, stats, puntos por nivel y Marlon.
- Veintiuna evidencias registradas en `RES-0001`. Los seis claims están
  `VERIFIED` por axioma explícito del propietario y quedan habilitados para
  contratos y reglas productivas. Aún no se publicaron datos al producto.
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
  MU Online Fanz. Las primeras capturas disponibles son de marzo de 2023 y
  mezclan clases posteriores. `DSP-0001` queda resuelto por decisión del
  propietario a favor de Soul Master para Season 4.
- El propietario aclaró que la cadena del mago en Season 4 termina en Dark
  Wizard → Soul Master → Grand Master; Grand Master se incorporó en esa
  temporada y se obtiene tras la serie de quests de tercera clase culminada a
  nivel 400. Soul Wizard es posterior. EVD-0012/EVD-0013 confirman la estructura
  actual, pero no la frontera histórica de las páginas individuales.
- Stats candidatos confirmados en orden `STR/AGI/VIT/ENE[/CMD]`: DW
  `18/18/15/30`, DK `28/20/25/10`, ELF `22/25/20/15`, SUM `21/21/18/23`, MG
  `26/26/26/26` y DL `26/20/20/15/25`. `VIT` normaliza `Stamina`; sólo DL posee
  `CMD`.
- DW/DK/ELF/SUM ganan 5 puntos por nivel y 6 tras completar Hero Status/Marlon
  desde nivel 220, con extra retroactivo por niveles posteriores a 220. MG/DL
  ganan 7 desde el inicio y no realizan Marlon. EVD-0021 las fija como reglas
  `VERIFIED` del ruleset.
- `DSP-0002` quedó resuelto por decisión explícita del propietario: el proyecto
  adopta Energy 26 para MG. La guía actual de Webzen que publica 16 permanece
  trazada como divergencia documental de aplicabilidad histórica no demostrada.
- `EVD-0019` demuestra que la matriz completa ya aparece en transcripciones
  atribuidas a actualizaciones Season 1/3 de 2005/2007; `EVD-0020` añade seis
  guías fechadas en 2010 y su auditoría CDX. Ninguna línea conserva un original
  de Webzen ni un snapshot contemporáneo: las primeras capturas de GuiasMU son
  de septiembre de 2020. EVD-0021 retira esa búsqueda como gate y conserva el
  resultado sólo como provenance.
- `EVD-0021` registra la decisión del propietario de tratar la matriz completa
  como invariante desde los inicios del juego. `RES-0001` queda cerrado con 6/6
  claims `VERIFIED`, ambos conflictos resueltos y permiso para implementar.
- Contrato `progression-rule.schema.json` `1.0.0` incorporado con puntos por
  nivel, primer nivel premiado y bonus opcional de quest con elegibilidad y
  retroactividad explícitas; sus fixtures sintéticos válido/inválido pasan el
  validador integral.
- IDs definitivos fijados para las seis clases, dieciséis evoluciones, dos reglas
  de progresión y la referencia a Hero Status. Los ocho registros canónicos
  viven en `packages/rulesets/mu-s4-global-reference/v1`, usan
  `confidence: VERIFIED` y trazan EVD-0021, evidencias de contraste y conflictos
  aplicables por campo.
- El validador integral y su CLI comprueban los seis registros de clase contra
  `character-class.schema.json` y las dos reglas contra
  `progression-rule.schema.json`. Una prueba de contrato fija además el conjunto
  de ocho IDs para detectar renombres accidentales.
- Siete casos de referencia factuales y versionados cubren clase estándar en
  niveles 1/220 sin Hero Status, 220/221/230 con Hero Status, y MG/DL en nivel
  220. El validador reproduce 0/1095/1095/1101/1155/1533 puntos y tres controles
  negativos prueban segunda clase y exclusión de Magic Gladiator/Dark Lord.
- Las dos reglas están `PUBLISHED`. La regla estándar enlaza sus cinco casos y
  la regla de MG/DL enlaza sus dos casos; el validador exige resolución dentro
  del mismo ruleset y regla, cobertura completa y exclusión de los tres
  controles negativos.
- Primera vertical productiva de Domain/Calculation Engine implementada. La
  operación pura recibe clase, evolución, nivel y quests; resuelve exactamente
  una regla `PUBLISHED`, valida elegibilidad y devuelve el total con una traza
  separada de puntos por nivel y Hero Status.
- Doce pruebas del motor materializan los registros JSON canónicos y reproducen
  7/7 casos positivos y 3/3 rechazos con códigos estables. Fijan además la
  descomposición 1145+10 del caso estándar de nivel 230 y prueban que una regla
  `REVIEWED` no puede ejecutarse.
- Capa Application y adaptador productivo de progresión implementados. El
  lector materializa exclusivamente clases y reglas desde el snapshot JSON,
  exige un único ruleset, IDs/referencias coherentes y reglas `PUBLISHED`, y
  entrega el catálogo al primer caso de uso sin depender de WPF ni SQLite.
- Doce pruebas de integración de Application reproducen desde archivos los 7/7
  casos positivos y 3/3 rechazos canónicos. Dos copias temporales alteradas
  demuestran fallo cerrado ante una regla `REVIEWED` y una referencia de regla
  inexistente, sin duplicar valores del juego en código.
- La shell WPF referencia Application de forma unidireccional y empaqueta el
  snapshot completo en `rulesets/mu-s4-global-reference/v1`. La ejecución
  resuelve esa ruta desde los binarios publicados, no desde el repositorio.
- Primer flujo funcional disponible: clase y evolución materializadas con sus
  nombres canónicos, nivel y Hero Status como entradas; total, regla/version y
  aportes como salida trazable. La elegibilidad y el ID de quest proceden de la
  regla publicada, sin constantes factuales duplicadas en XAML/C#.
- El smoke WPF publicado carga el snapshot con Application y reproduce 7/7
  casos positivos y 3/3 rechazos en las fases inicial/reemplazo. También exige
  las cuatro carpetas de contenido y conserva SHA-256 idéntico para sus 18 JSON.
- Contrato estructural de distribución de stats definido inicialmente en
  `1.0.0` y ampliado de forma compatible a `1.1.0` para resets configurables.
  Definido antes del código productivo, registra presupuesto de progresión,
  inputs/producto de resets, total distribuible, asignaciones, gasto y remanente
  con contadores no negativos de 64 bits.
- Las invariantes semánticas fijan asignaciones exactamente para los stats de
  la clase canónica, sumas consistentes y gasto dentro del presupuesto.
  `command` sólo está disponible si la clase lo declara. Se documentaron seis
  casos sintéticos y códigos estables para negativos, stats ajenos/omitidos,
  exceso, overflow y origen de presupuesto incoherente.
- Segunda vertical productiva de Domain/Calculation Engine implementada.
  `StatDistributionCalculator` consume el presupuesto ya calculado, la
  definición de clase y asignaciones no confiables; valida origen y conjunto
  exacto de stats, suma con control de overflow y deriva gasto y remanente.
- El presupuesto conserva ahora `CharacterClassId` y la definición de clase los
  IDs de sus stats. Application materializa esos IDs directamente desde las
  claves del snapshot, sin duplicar valores base ni incorporar datos nuevos.
- Diez pruebas sintéticas cubren distribución parcial/exacta, los cuatro
  negativos mínimos, las tres variantes de origen incoherente y overflow. Una
  prueba de integración adicional contrasta los IDs materializados con los JSON
  canónicos.
- `CalculateStatDistributionUseCase` integra la distribución en Application.
  Recibe el presupuesto existente, inputs de resets y asignaciones, resuelve una
  única clase del mismo ruleset desde el catálogo y delega en el motor sin
  recalcular progresión ni aceptar una definición alternativa.
- Cuatro pruebas de integración adicionales recorren una copia temporal del
  snapshot hasta el motor para distribuciones sintéticas parcial/exacta y
  `budget-source-mismatch` ante un ruleset de origen incoherente, y confirman la
  propagación de `allocation-negative`. No se añadieron datos ni fórmulas del
  juego.
- El flujo WPF conserva el presupuesto de progresión calculado y genera los
  inputs de asignación exclusivamente desde los `StatIds` de la clase
  materializada. Cambiar clase, evolución, nivel o Hero Status invalida ese
  presupuesto antes de permitir una distribución.
- La pantalla delega en `CalculateStatDistributionUseCase` y muestra puntos
  de progresión, resets, total, gasto, remanente y asignaciones por ID. Los diez
  códigos tipados tienen
  explicación visible en español sin ocultar el identificador estable.
- El smoke WPF publicado verifica en ambas fases una distribución sintética de
  `2 × 100 = 200` sobre el conjunto de stats obtenido del snapshot. No incorpora
  una regla factual de resets ni valores nuevos del juego.
- Contrato `build-draft.schema.json` definido inicialmente en `1.0.0` y
  ampliado a `1.1.0`. Fue definido antes de la primera
  persistencia de usuario. Conserva identidad, metadata exacto de
  ruleset/dataset/motor, entradas de progresión y el resultado de distribución
  compuesto mediante `$ref`.
- La especificación clasifica las entradas y asignaciones como autoridad del
  usuario y los totales calculados como caché obligatoriamente revalidada. Fija
  invariantes de recarga, seis errores estables propuestos, reemplazo atómico
  por ID y el límite futuro Application/Data.
- Dos fixtures sintéticos prueban el contrato. El inválido mantiene válido el
  envoltorio y es rechazado por el `StatDistribution` referenciado. La tabla y
  migración productivas se incorporaron después de este gate estructural.
- Modelo `BuildDraft` materializado en Application con nombres JSON explícitos
  y estructura alineada con los contratos `build-draft` y `stat-distribution`
  `1.1.0`. `BuildDraftRuntimeContext` recibe explícitamente catálogo,
  ruleset/version, dataset/version/hash y motor/version; no los deriva de rutas,
  fechas o ensamblados.
- Puerto `IBuildDraftRepository` y casos de uso de guardado/carga implementados
  sin referencias a Data, SQLite o WPF. El guardado calcula la caché completa;
  la carga exige identidades y dependencias exactas, recalcula progresión y
  distribución y compara regla, presupuesto, asignaciones, gasto y remanente
  antes de devolver el resultado recalculado.
- Siete pruebas sintéticas de Application cubren alta/carga, reemplazo por ID,
  ausencia, identidad incoherente, dependencia no disponible, caché alterada y
  round-trip JSON con los nombres del schema. Los seis errores estables
  `build-draft-*` ya están definidos.
- Primera persistencia productiva de usuario implementada en Data.
  `SqliteBuildDraftMigrations.All` crea `build_drafts` mediante la migración
  hacia adelante `1/create_build_drafts`; cada fila conserva payload JSON y
  metadata exacto de schema, ruleset, dataset/hash y motor.
- `SqliteBuildDraftRepository` implementa `IBuildDraftRepository` con reemplazo
  atómico por ID dentro de `SqliteWriteContentionPolicy`. El agotamiento se
  traduce a `build-draft-write-conflict`; la carga usa sólo `SELECT` y deja toda
  revalidación en Application.
- Seis pruebas Data sintéticas demuestran alta/carga con payload exacto,
  metadata, reemplazo único, rollback completo, reapertura, ausencia sin
  mutaciones y traducción tipada de contención. No se añadieron datos ni
  fórmulas del juego.
- WPF compone `SaveBuildDraftUseCase` y `LoadBuildDraftUseCase` con
  `SqliteBuildDraftRepository`. La base productiva vive en
  `%LOCALAPPDATA%\MuOnline.BuildPlanner\build-planner.sqlite` y la migración
  productiva se aplica antes de construir el repositorio.
- La composición declara ruleset `1.0.0`, dataset `2026-07-24.1` y motor
  `0.1.0`. El hash SHA-256 se calcula de forma determinista sobre rutas
  relativas y bytes exactos de los 27 JSON publicados; no se infiere desde
  carpetas, fechas del sistema ni versiones de ensamblado.
- La pantalla guarda y carga por ID exclusivamente mediante los casos de uso.
  La carga recalcula y contrasta antes de repoblar entradas y resultados; los
  seis códigos `build-draft-*`, incluido `build-draft-write-conflict`, conservan
  su identificador estable y una explicación visible.
- El smoke publicado guarda un borrador sintético en la base externa, lo
  revalida, ejecuta backup/restore y repite la carga desde los binarios de
  reemplazo. Verifica ID, metadata, hash, asignaciones, gasto y remanente sin
  incorporar datos ni fórmulas nuevos del juego.
- Por decisión del propietario del 2026-07-24, los resets se modelan sólo como
  configuración del servidor y no como mecánica del ruleset Season 4. Los dos
  inputs no negativos son cantidad y puntos por reset; ambos valen cero por
  defecto.
- `StatDistributionCalculator` calcula con overflow controlado
  `resetCount × pointsPerReset`, suma el resultado al presupuesto de progresión
  y habilita el total combinado para asignaciones. La salida conserva separados
  puntos de progresión, inputs/producto de resets, total, gasto y remanente.
- Los contratos `stat-distribution` y `build-draft` avanzan a `1.1.0`; el motor
  WPF avanza a `0.2.0`. Ruleset `1.0.0`, dataset `2026-07-24.1`, sus 18 JSON y
  su hash permanecen sin cambios porque no se añadió información factual.
- WPF presenta `Resets`, `Puntos por reset` y `Puntos totales por resets`.
  Guardado/carga conservan los inputs autoritativos y revalidan producto y
  presupuesto total. Se añadieron cuatro errores tipados para negativos y
  overflow.
- `RES-0002` abierto para investigar HP, Mana, AG y SD. Contiene 24 claims
  independientes —una combinación por atributo y familia de clase canónica—,
  abiertos inicialmente como `UNVERIFIED`, sin evidencia, cifras, constantes ni
  fórmulas candidatas. El registro exige verificar por claim versión, evolución
  aplicable, entradas, orden de operaciones y redondeo antes de publicar.
- Primer claim de `RES-0002` investigado. EVD-0022–EVD-0025 registran que la
  página actual de MU Online Fanz publica HP 60, 1 por nivel y 1 por Stamina
  para Dark Wizard; Webzen confirma sólo HP 60 y 1 por nivel, mientras
  StrategyWiki e InfinityMU publican 2 por Vitality. Como ninguna fuente
  demuestra Season 4 global/inglés y faltan semántica inicial y redondeo,
  el contraste individual permanece `PARTIAL`.
- EVD-0026 registra la decisión explícita del propietario del 2026-07-24: las
  fórmulas entregadas corresponden a Season 4 global/inglés, aplican a las
  familias completas y el valor mostrado trunca la parte decimal. En ese punto
  quedaron
  `VERIFIED` 21/24 claims de `RES-0002`: HP, Mana, AG y SD de cinco familias,
  más AG de Summoner.
- EVD-0027–EVD-0029 investigan HP, Mana y SD de Summoner en MU Online Fanz,
  Webzen y MUonline Helper. Coinciden en HP 70, Mana 40, SD 102 y aumentos por
  nivel de 1 HP y 1.5 Mana; sólo Fanz añade coeficientes de stat y componentes
  de SD. Como ninguna fuente demuestra una fórmula cerrada Season 4 ni la
  semántica de base, orden y truncamientos, los tres claims pasan a `PARTIAL`.
- EVD-0030 registra la corrección del propietario: Summoner en nivel 1 con
  Vitality 18 tiene HP 70 y cada punto adicional de Vitality suma 2 HP. Coincide
  con EVD-0027–EVD-0029. La confirmación posterior de +1 HP por nivel cierra
  `hp = 70 + (lvl - 1) + (vit - 18) * 2`; `DR-HP-SUMMONER` pasa a `VERIFIED`.
- EVD-0031 conserva la corrección final del propietario: +1.7 Mana por cada
  punto de Energy de Summoner, coincidente con Fanz. El valor 1.5 comunicado
  antes queda retirado. El propietario también fija Mana 40 al nacer en nivel 1
  con Energy 23 y confirma +1.5 Mana por nivel. Queda aprobada
  `mana = 40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`;
  `DR-MANA-SUMMONER` pasa a `VERIFIED`.
- EVD-0032 identifica por decisión del propietario la fórmula de SD de Summoner
  y `defense = agi / 3`. El propietario confirma SD visible 102 en nivel 1 con
  stats base y fija truncamiento independiente de cada término antes de sumar.
  `DSP-0004` queda resuelto y `DR-SD-SUMMONER` pasa a `VERIFIED`.
- `DSP-0003` queda resuelto por decisión del propietario a favor de
  `30 + (lvl - 1) + vit * 2` para HP de la familia Dark Wizard. La divergencia
  de Fanz se conserva sin reclasificar su evidencia. EVD-0026 preserva además
  fórmulas de daño, wizardry, defensa, rates, regeneración, buffs, Fenrir, pets y
  capacidad de clan, todavía sin contratos ni implementación productiva.
- Diseño documental completado para `DR-HP-DARK-WIZARD`. Propone el ID
  `formula-hp-dark-wizard` `1.0.0`, alcance sobre las tres evoluciones de la
  familia, inputs enteros, aritmética comprobada, truncamiento visible final,
  traza de tres aportes y ocho casos manuales. El análisis detecta que
  `formula.schema.json` `1.0.0` aún no representa aplicabilidad, límites,
  procedencia del input ni traza estructurada. No se modificaron schemas,
  ruleset, casos ejecutables, motor, Application, Data o WPF.
- Revisión técnica de `formula-hp-dark-wizard` completada y aprobada el
  2026-07-24 para ID, versión, aplicabilidad, tipos, errores, traza y ocho casos
  manuales. Se corrigió la procedencia: `EVD-0026` autoriza la fórmula y
  `EVD-0021` sustenta Vitality 15 como mínimo canónico. Los máximos de los tipos
  son límites técnicos, no límites factuales de MU Online. La revisión no
  creó datos o código.
- Decisión de contratos de fórmula aprobada técnicamente el 2026-07-25.
  `formula.schema.json` avanzó a `1.1.0` para aplicabilidad, bounds
  clasificados, procedencia cerrada de inputs y declaración ordenada de pasos.
  `calculation-trace.schema.json` y `formula-test-case.schema.json` nacieron en
  `1.0.0` para separar ejecución y expectativas de la definición factual.
  La decisión fija versionado, referencias y gates semánticos sin modificar el
  ruleset ni el código productivo.
- Contratos de fórmula materializados según la decisión:
  `formula.schema.json` `1.1.0`, `calculation-trace.schema.json` `1.0.0` y
  `formula-test-case.schema.json` `1.0.0`. Seis fixtures sintéticos prueban
  definición, ejecución, unión positiva/negativa y resolución real del `$ref`
  caso → traza.
- El validador integral cubre 10 contratos/20 fixtures y añade pruebas
  focalizadas para aplicabilidad vacía/duplicada, bounds factuales sin
  evidencia, source incompleto, pasos duplicados, salidas fuera de `stepIds`,
  salida visible no final y expectativas ambiguas.
- `formula-hp-dark-wizard` `1.0.0` materializada, revisada y `PUBLISHED`, con
  aplicabilidad exacta a las tres evoluciones, inputs `INT32`/`INT64`, mínimo
  factual de Vitality trazado a `EVD-0021`, expresión y truncamiento autorizados
  por `EVD-0026`, y conflicto resuelto `DSP-0003`.
- Cuatro casos positivos conservan los aportes y resultados aprobados; cuatro
  controles negativos cubren nivel, Vitality, familia y overflow. Sólo los
  positivos están enlazados mediante referencias versionadas.
- Gate semántico factual incorporado al validador: exige identidad exacta,
  clase/evoluciones del catálogo, inputs, orden de pasos, correspondencia de
  outputs, redondeo, provenance y cobertura completa de positivos. Cuatro
  mutaciones temporales demuestran fallo cerrado.
- La revisión de publicación del 2026-07-25 contrastó los nueve JSON con el
  contrato aprobado sin encontrar divergencias. Sólo promovió `status` de
  `DRAFT` a `PUBLISHED`; una prueba de contrato fija el nuevo estado. No se
  añadió motor de HP ni integración en Application, Data o WPF.
- Alcance anterior retirado y documentación migrada a Season 4 global/inglesa.

## No iniciado

- Motor de `DR-HP-DARK-WIZARD`; la definición canónica está `PUBLISHED`, pero
  todavía no existe cálculo productivo.
- IDs, contratos, casos de referencia y motor para los otros 23 claims
  `VERIFIED` de `RES-0002` y para el resto del catálogo de EVD-0026. La decisión
  factual no sustituye esos gates ni define todavía precisión de dependencias
  intermedias.
- Schemas restantes (`ruleset`, quests, ítems, skills, escenarios y
  trazas); el validador integral/CI ya cubre el contrato de progresión.
- Builds completas, resto del motor de cálculo y flujos de UI
  posteriores al presupuesto ganado y los borradores locales.

## Decisiones abiertas

- El canal público de actualización y firma continúa como decisión posterior de
  distribución.

## Verificación más reciente — 2026-07-25

- Schemas: 10/10 contratos y 20/20 fixtures estructuralmente legibles.
- Validador integral: 10/10 fixtures válidos aceptados, 10/10 inválidos rechazados
  y 9/9 registros canónicos válidos. Ejecuta además 7/7 casos de progresión,
  rechaza 3/3 controles semánticos y valida 4/4 casos de fórmula más 4/4
  controles negativos. La fórmula se informa `PUBLISHED` con cero errores. Sus
  20/20 pruebas .NET aprueban también la
  ejecución repetida, los IDs estables, la retroactividad, la elegibilidad, los
  enlaces exactos 5+2 de las reglas publicadas y los contratos sintéticos de
  fórmula/traza/caso, además de cuatro mutaciones del gate factual.
- Solución .NET 10 de diez proyectos: restauración bloqueada y build Release
  aprobados con 0 advertencias y 0 errores; CLI del validador y formato
  verificados.
- Persistencia SQLite: 18/18 pruebas de integración aprobadas en `win-x64`;
  junto con 20/20 pruebas del validador, 27/27 del motor y 27/27 de Application,
  la solución ejecuta 92/92 pruebas correctamente. Los seis casos nuevos
  cubren la migración y el repositorio de borradores con payload/metadata,
  reemplazo, rollback, reapertura, lectura pura y contención tipada.
- Motor de puntos: 7/7 casos positivos canónicos, 3/3 rechazos semánticos y
  2/2 controles de traza/publicación aprobados. Domain y Calculation Engine no
  incorporan paquetes externos ni referencias a Data, WPF o serialización.
- Distribución de stats: 2/2 resultados sintéticos y 8/8 controles de error
  aprobados; incluyen negativos, conjunto exacto de stats, exceso, tres
  divergencias de origen y overflow. El resultado copia clase, ruleset,
  regla/version y presupuesto sin recalcular progresión.
- Resets configurables: `2 × 100 = 200`, defaults `0 × 0 = 0`, ambos inputs
  negativos, overflow del producto y overflow del total aprobados. Una prueba
  de Application distribuye los 200 puntos adicionales desde un snapshot
  temporal sin convertirlos en datos del ruleset.
- Application: 7/7 casos positivos y 3/3 rechazos reproducidos por el camino
  archivo → adaptador → caso de uso → motor; 2/2 alteraciones de snapshot
  rechazadas antes del cálculo y 1/1 control confirma los IDs de stats
  materializados. La distribución añade 2/2 resultados sintéticos, 1/1 rechazo
  de origen y 1/1 propagación tipada por el camino copia temporal → catálogo →
  caso de uso → motor. Los borradores añaden 7/7 controles sintéticos para
  serialización, guardado/reemplazo, carga con recálculo y fallos cerrados por
  ausencia, identidad, dependencia o caché. El proyecto no incorpora paquetes
  externos ni referencia Data o WPF.
- WPF/Application: el build copia 27 JSON canónicos a una ruta estable tanto en
  salida normal como publicada. El flujo de ventana obtiene clase/evolución y
  Hero Status desde el catálogo/regla, muestra el total junto con la traza y
  conserva ese presupuesto para distribuir sobre controles derivados de
  `StatIds`. Muestra progresión, resets, total, gasto/remanente y errores
  tipados; App referencia
  Application y las capas internas no incorporan referencias inversas. El mismo
  composition root aplica la migración de borradores, configura contención en
  2 s × 3 intentos con 150 ms entre reintentos y conecta guardado/carga por ID
  sin omitir la revalidación de Application.
- Json Everything fuente: 2 compilaciones independientes con SDK `10.0.301`,
  restore bloqueado de los tres proyectos fuente, hashes esperados para 3/3
  DLL y SPDX contrastado. El harness actualizado ejecuta 2 × 20/20 fixtures y
  rechaza formatos inválidos: PASS.
- Integración del validador: lock con sólo `Humanizer.Core 3.0.10`, tres DLL
  clasificados como referencias directas, aviso MIT presente y ausencia de
  `OSMFEULA.txt`/`.nuspec` publicados en la salida: PASS local y remoto.
- GitHub Actions `run 29697921106`, commit `d0626d2`: `build-and-test` y
  `wpf-publication-smoke` terminaron `success` con SDK fijado `10.0.301`. Los
  selectores del workflow fueron `ubuntu-latest` y `windows-latest`; la API
  pública sólo identificó los runners `1000000027` y `1000000026`, no la versión
  interna de imagen. Los hashes remotos revisados fueron `450646267c…` para
  Json.More, `8f7be030e4…` para JsonPointer y `1f2dc6dfad…` para JsonSchema.
- El run previo `29697684666` falló en el build fuente porque el hash de licencia
  procedía de un checkout CRLF. La corrección fuerza LF y deshabilita PDB/símbolos;
  dos builds locales independientes y el run remoto posterior coinciden.
- Dependencias: los diez proyectos restauran con lock files. Application no
  añade paquetes y su proyecto de pruebas reutiliza las versiones centrales ya
  fijadas. Data referencia Application para implementar su puerto sin crear una
  referencia inversa; WPF conserva ambas dependencias de composición. La
  restauración bloqueada se repitió con acceso a NuGet y fue aprobada; el lock
  del validador resuelve únicamente `Humanizer.Core 3.0.10`, mientras los tres
  DLL Json Everything se compilan desde fuente fijada. La auditoría directa y
  transitiva registrada para el grafo anterior no encontró paquetes vulnerables
  según los orígenes consultados.
- Spike C#: 4/4 comprobaciones aprobadas con .NET SDK 10.0.301.
- Node.js y `pwsh` no están disponibles en el `PATH`; la prueba de schemas se
  ejecuta con Windows PowerShell.
- El motor incorpora sólo los datos factuales autorizados por EVD-0021: seis
  clases y dos reglas de progresión canónicas publicadas con siete referencias
  ejecutables. La definición de HP de Dark Wizard está `PUBLISHED` como dato,
  pero HP, Mana, AG, SD, daño y defensa todavía no se ejecutan.
- Licencia: texto Apache-2.0 contrastado con la publicación oficial; ADR-0005,
  `NOTICE` e inventario de terceros incorporados. La auditoría leyó metadatos
  `.nuspec` de todas las dependencias restauradas y el acuerdo incluido por la
  familia Json Everything.
- Investigación documental: `RES-0001` conserva 21 evidencias, 6/6 claims
  `VERIFIED` y dos conflictos resueltos. `RES-0002` queda `VERIFIED` con 24/24
  claims, once evidencias y dos conflictos resueltos.
  EVD-0026 autoriza diseñar contratos y casos para su alcance exacto,
  pero todavía no existen fórmulas productivas.
  EVD-0014–EVD-0018 trazan stats, puntos, Marlon, contraste oficial y decisiones
  del propietario; EVD-0019/EVD-0020 trazan la presencia anterior de la matriz
  candidata y la búsqueda negativa de un original o snapshot contemporáneo;
  EVD-0021 fija la matriz como axioma del ruleset y cierra `RES-0001`.
- Contrato documental de HP de Dark Wizard: revisión técnica aprobada para
  `formula-hp-dark-wizard` `1.0.0`; `EVD-0026` traza la expresión y
  aplicabilidad, mientras `EVD-0021` traza el mínimo canónico de Vitality. La
  estrategia de tres contratos, la definición JSON, los ocho casos y el gate
  semántico están implementados. La revisión de publicación no encontró
  divergencias y la fórmula está `PUBLISHED`; todavía no existe motor.
- Contraste técnico: 2 registros comunitarios nuevos y matriz completa para
  `WZ-CLM-001`–`WZ-CLM-010`; la auditoría concluyó que su independencia y
  procedencia global/inglesa no son demostrables con los artefactos disponibles.
- Preservación técnica: localizado el nombre oficial `MU1_03 full(Eng).exe` y
  un mirror archivado que lo clasifica como cliente inglés Season 4 de 456.77
  MB; no se recuperó el binario ni un checksum del editor.
- MU Online Fanz: no hay capturas indexadas de rutas de segunda/tercera clase
  entre 2012 y 2022. Las primeras capturas de 2023 mezclan clases posteriores;
  EVD-0021 resuelve `DSP-0001` y retira este límite como bloqueo productivo.
- Cadena candidata del mago para Season 4: Dark Wizard → Soul Master → Grand
  Master. La obtención candidata de Grand Master requiere culminar a nivel 400
  la serie de quests de tercera clase. Soul Wizard queda excluido por
  clasificación del propietario y queda aprobada para Season 4 por EVD-0021.
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
  `10.0.301`, SQLite `3.53.3`, 450 archivos y 148.754.557 bytes después de
  publicar la primera fórmula factual. Los diez archivos legales y los 27
  JSON del
  ruleset estuvieron presentes y conservaron sus hashes entre ambas carpetas;
  el modo headless reprodujo 7/7 casos positivos, 3/3 rechazos y una
  configuración `2 × 100 = 200` y una distribución sintética con 201 puntos
  gastados sobre cinco stats en ambas fases.
  Aplicó dos migraciones en la fase inicial, reconoció ambas tras el reemplazo y
  revalidó `publication-smoke-draft` con dataset `2026-07-24.1` y hash
  `sha256:b45eda3083634c43aa4eaead02e02945793075a3c6ee865973c8b4776917a7ad`.
  La fórmula `PUBLISHED` quedó empaquetada como dato canónico, pero WPF no la
  materializa ni la ejecuta.
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
  fuente, 2 × 20/20 fixtures, formatos, SBOM, locks, auditoría y publicación
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
  resuelve `DSP-0002` a favor de Energy 26 para MG.
- Decisión posterior: dejar de exigir comprobación histórica adicional para la
  matriz de `RES-0001`, promover sus seis claims a `VERIFIED` como axiomas del
  ruleset y continuar con la implementación productiva.

## Decisión del propietario — 2026-07-24

- Los resets dependen de cada servidor, no de la versión ni del juego estándar.
- No se requiere investigación factual ni una regla de resets dentro de
  `mu-s4-global-reference`.
- La UI debe recibir cantidad de resets y puntos por reset, ambos con valor
  predeterminado cero, calcular su producto y habilitar esos puntos para la
  distribución de stats.
- Esta decisión se implementa como configuración explícita y conserva los
  puntos de progresión separados de los puntos por resets.
- Las fórmulas de EVD-0026 corresponden a Season 4 global/inglés y se aceptan
  como axiomas del ruleset para las familias completas confirmadas.
- El resultado mostrado por el juego trunca la parte decimal. No se autoriza
  inventar redondeos intermedios; cada futura dependencia deberá especificar su
  precisión y punto de truncamiento.
- Para Summoner se aprueban daño, wizardry, rates, defensa, velocidad, AG,
  Reflect, Berserker, Innovation y Weakness. HP también queda aprobado como
  `70 + (lvl - 1) + (vit - 18) * 2` y Mana como
  `40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`. SD trunca sus tres términos antes
  de sumarlos; `RES-0002` queda completamente verificado.
