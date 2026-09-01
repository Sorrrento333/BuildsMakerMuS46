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
- Once contratos JSON Schema 2020-12: siete en `1.0.0`, fórmula,
  distribución de stats/borrador de build en `1.1.0` y fórmula ejecutable
  compatible `2.0.0`/`2.1.0`, con veintidós fixtures sintéticos.
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
  relativas y bytes exactos de los 36 JSON publicados; no se infiere desde
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
- En esa vertical, el validador integral alcanzó 10 contratos/20 fixtures y
  añadió pruebas
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
- Diseño técnico de la primera vertical ejecutable de HP cerrado. Determina que
  `strategy.definition` `1.1.0` no basta para ejecutar sin interpretar texto:
  no enlaza aliases con inputs, pasos con operaciones ni bounds con errores.
  Se selecciona un programa estructurado `CHECKED_INT64_V1`, materializado por
  Application y ejecutado genéricamente por Calculation Engine, sin handlers ni
  constantes por fórmula en C#.
- `formula-hp-dark-wizard` `1.0.0` permanecerá inmutable y válido como artefacto
  histórico publicado. El schema `2.0.0` y la definición ejecutable
  `formula-hp-dark-wizard` `1.1.0` ya existen; sus casos se materializaron
  después y la revisión de publicación ya está cerrada. Antes de ejecutar falta
  implementar el evaluador. Data y WPF quedan expresamente fuera de esa vertical.
- Contrato ejecutable `packages/schemas/v2/formula.schema.json` `2.0.0`
  implementado sin retirar `v1`. `CHECKED_INT64_V1` admite exclusivamente
  inputs `INT32`/`INT64`, literales y salida `INT64`, y las operaciones
  `CONSTANT`, `ADD`, `SUBTRACT`, `MULTIPLY` y `APPLY_ROUNDING` con aridades
  cerradas. Cada input exige bounds y `rangeErrorCode`.
- Dos fixtures `formula-v2` exclusivamente sintéticos recorren las cinco
  operaciones y rechazan formas incompletas. El gate semántico exige IDs
  únicos, inputs declarados, referencias sólo a pasos anteriores, bounds
  coherentes con el tipo y correspondencia exacta entre programa, traza,
  salidas y redondeo. Ruleset, dataset, fórmula factual `1.0.0`, casos
  canónicos, Domain, Calculation Engine, Application, Data y WPF no cambiaron.
- `formula-hp-dark-wizard` `1.1.0` materializada como un segundo artefacto
  canónico en estado `DRAFT` contra el schema `2.0.0`. Conserva aplicabilidad,
  inputs, bounds, salida, redondeo, traza, constraints, evidencia y conflicto
  de la versión publicada; añade `rangeErrorCode` por input y expresa los cinco
  pasos aprobados únicamente mediante `CHECKED_INT64_V1`.
- El artefacto histórico `1.0.0` y sus ocho casos permanecen byte a byte
  intactos. La versión `1.1.0` enlaza exclusivamente cuatro positivos de una
  serie propia de ocho casos. Está `PUBLISHED` y ya se ejecuta por referencia
  exacta desde Application.
- El gate canónico selecciona `v1`/`v2` por `schemaVersion`, usa la identidad
  compuesta `id` + `version`, admite ambas versiones sin resolver “la última” y
  rechaza duplicados exactos. Dos pruebas nuevas fijan la equivalencia de los
  campos compartidos y el fallo cerrado ante identidad compuesta duplicada.
- Ocho casos `1.1.0` materializados en archivos distintos: cuatro positivos y
  cuatro controles negativos. Conservan exactamente IDs, contextos, inputs,
  trazas, resultados, errores, evidencia y conflicto; sólo cambia la versión
  de `formulaRef`.
- El gate de casos usa identidad compuesta `id` + versión de `formulaRef`,
  permite coexistir ambas series y reporta un duplicado exacto como error sin
  abortar la validación. Una prueba estructural compara las ocho parejas y
  exige que no exista ninguna otra divergencia.
- Revisión de publicación de `formula-hp-dark-wizard` `1.1.0` completada sobre
  su definición y ocho casos. No se encontraron divergencias contra el schema
  `2.0.0`, los contratos de traza/caso ni el diseño aprobado; el único cambio
  del artefacto fue `status: PUBLISHED` y una prueba fija ese estado.
- Primera vertical genérica de fórmulas ejecutables implementada en Domain y
  Calculation Engine. Los tipos inmutables y ajenos a JSON representan
  definición/referencia, aplicabilidad, inputs enteros de 32/64 bits,
  bounds/códigos, output, redondeo, programa `CHECKED_INT64_V1`,
  pasos/operandos, solicitud, resultado y traza con evidencia/conflictos.
- `CheckedIntegerFormulaInterpreter` exige estado `PUBLISHED`, aplicabilidad y
  conjunto exacto de inputs; ejecuta únicamente `CONSTANT`, `ADD`, `SUBTRACT`,
  `MULTIPLY` y `APPLY_ROUNDING` con aritmética comprobada. No contiene handlers,
  IDs, constantes ni resultados factuales.
- Veinticinco pruebas exclusivamente sintéticas cubren operaciones, estado,
  clase/evolución, inputs ausentes/extra, bounds inclusivos/exclusivos, tipo de
  32 bits, código de rango materializado, seis modos de redondeo, orden,
  referencia adelantada, programa no soportado, overflow, traza/provenance y
  copia inmutable de colecciones. En ese cierre la fórmula canónica todavía no
  se materializaba desde Application; la vertical posterior lo completa.
- Adaptador de fórmulas ejecutables implementado en Application.
  `JsonExecutableFormulaSnapshotReader` inspecciona `character-classes/` y
  `formulas/`, conserva las definiciones schema `1.1.0` como historia no
  ejecutable y materializa únicamente schema `2.0.0` en estado `PUBLISHED`.
- El adaptador exige un ruleset único, identidad compuesta `id` + `version`,
  aplicabilidad a clase/evoluciones existentes, inputs contextuales, tipos y
  bounds soportados, aridades cerradas, referencias sólo hacia atrás,
  equivalencia programa/traza, salida visible producida por el redondeo y cero
  dependencias en esta primera vertical.
- `ExecutableFormulaCatalog` conserva copias inmutables y resuelve sólo
  referencias exactas. `CalculatePublishedFormulaUseCase` delega directamente
  en `CheckedIntegerFormulaInterpreter`; no selecciona “la última versión” ni
  lee casos de referencia. Una referencia histórica o ausente produce
  `formula-not-executable`.
- Catorce pruebas de integración nuevas leen desde archivos los ocho casos
  canónicos `formula-hp-dark-wizard` `1.1.0`: reproducen 4/4 resultados y
  trazas completos, 4/4 errores esperados y el rechazo de `1.0.0`. Cinco
  controles adicionales dentro del conjunto fijan selección exacta y fallo
  cerrado ante estado no publicado, evolución ajena, referencia adelantada y
  duplicado exacto. No se copiaron datos, constantes o resultados de HP a C#.
- Diseño de resolución productiva de `CONTEXT_VALUE` cerrado antes de modificar
  código o WPF. Application queda como autoridad de composición: el nivel
  procede de una solicitud de progresión validada y cada
  `resolved-{statId}` de la suma `checked` entre `baseValue` canónico y la
  asignación vigente validada por distribución.
- El diseño exige preservar `source.valueId`, materializar base/evidencia desde
  `character-classes/`, conservar estado inmutable con clase, evolución,
  nivel, presupuesto y distribución, y emitir una traza contextual separada de
  la traza aritmética. Fija fallos cerrados, provenance e invalidación; no
  incorpora datos, fórmulas ni código productivo.
- Vertical funcional de HP de Dark Wizard cerrada desde snapshot hasta WPF.
  Domain preserva `source.kind/valueId`; progresión materializa `baseValue` y
  `evidenceRefs`; Application compone progresión, distribución, estado
  inmutable, resolución contextual y la referencia exacta
  `formula-hp-dark-wizard` `1.1.0` sin constantes factuales en C#.
- `FormulaContextValueResolver` obtiene `character-level` de la solicitud
  validada y cada `resolved-{statId}` de
  `checked(baseValue + allocation)`. Devuelve una traza contextual separada de
  la aritmética e implementa los seis códigos estables `formula-context-*`.
- Ocho pruebas nuevas de Application contrastan bases/evidencias y
  `source.valueId` con los JSON, reproducen 4/4 positivos por la ruta completa,
  y cubren fuentes/valores no resolubles, mismatch, base/asignación ausentes,
  overflow, inmutabilidad y rechazo previo de nivel/asignación inválidos.
- WPF obtiene la única fórmula publicada aplicable desde el catálogo después de
  una distribución válida, muestra HP y ambas trazas e invalida el resultado al
  cambiar progresión, resets o asignaciones. No acepta inputs contextuales
  autoritativos ni persiste el resultado derivado.
- Vertical funcional de HP de Dark Knight cerrada desde `DR-HP-DARK-KNIGHT`
  hasta WPF. `formula-hp-dark-knight` `1.0.0` nace ejecutable y `PUBLISHED`
  contra schema `2.0.0`, aplica a Dark Knight/Blade Knight/Blade Master y
  materializa únicamente `35 + (lvl - 1) * 2 + vit * 3`.
- `EVD-0026` sustenta expresión, alcance y truncamiento; `EVD-0021` sustenta
  Vitality mínima 25. Cuatro positivos reproducen 110/112/113/115 HP y cuatro
  negativos cubren nivel, Vitality, familia y overflow. No se añadió evidencia
  ni un conflicto inexistente: definición y trazas conservan
  `conflictIds: []`.
- El gate canónico admite fórmulas sin conflictos cuando no hay uno aplicable y
  continúa exigiendo herencia exacta entre definición y traza. Application
  materializa dos referencias ejecutables y sus pruebas reproducen 8/8
  positivos y 8/8 errores desde JSON, sin handlers ni constantes factuales en
  C#.
- WPF reutiliza la selección genérica por clase/evolución. El smoke publicado
  exige cobertura exacta de ambas fórmulas y ocho positivos en las fases
  inicial/reemplazo. El dataset avanza a `2026-07-25.1` porque incorpora nueve
  JSON factuales nuevos; ruleset `1.0.0` y motor `0.2.0` no cambian.
- Vertical funcional de HP de Fairy Elf cerrada desde `DR-HP-FAIRY-ELF` hasta
  WPF. `formula-hp-fairy-elf` `1.0.0` nace ejecutable y `PUBLISHED` contra
  schema `2.0.0`, aplica a Fairy Elf/Muse Elf/High Elf y materializa únicamente
  `40 + (lvl - 1) + vit * 2`.
- `EVD-0026` sustenta expresión, alcance y truncamiento; `EVD-0021` sustenta
  Vitality mínima 20. Cuatro positivos reproducen 80/81/82/83 HP y cuatro
  negativos cubren nivel, Vitality, familia y overflow. No se añadió evidencia
  ni un conflicto inexistente: definición y trazas conservan
  `conflictIds: []`.
- Application materializa tres referencias ejecutables y reproduce 12/12
  positivos y 12/12 errores desde JSON. WPF reutiliza selección, resolución
  contextual e intérprete genéricos sin handlers ni constantes de Fairy Elf en
  C#.
- El dataset avanza a `2026-07-25.2` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de HP de Summoner cerrada desde `DR-HP-SUMMONER` hasta
  WPF. `formula-hp-summoner` `1.0.0` nace ejecutable y `PUBLISHED` contra
  schema `2.0.0`, aplica a Summoner/Bloody Summoner/Dimension Master y
  materializa la forma equivalente
  `34 + (lvl - 1) + vit * 2` de la expresión aprobada
  `70 + (lvl - 1) + (vit - 18) * 2`.
- `EVD-0021` sustenta Vitality mínima 18; `EVD-0030` autoriza expresión,
  alcance y redondeo. `EVD-0027`–`EVD-0029` permanecen como contraste con su
  clasificación individual intacta. Cuatro positivos reproducen 70/71/72/73
  HP y cuatro negativos cubren nivel, Vitality, familia y overflow.
- Application materializa cuatro referencias ejecutables y reproduce 16/16
  positivos y 16/16 errores desde JSON. WPF reutiliza selección, resolución
  contextual e intérprete genéricos sin handlers ni constantes de Summoner en
  C#.
- El dataset avanza a `2026-07-26.1` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de HP de Magic Gladiator cerrada desde
  `DR-HP-MAGIC-GLADIATOR` hasta WPF. `formula-hp-magic-gladiator` `1.0.0`
  nace ejecutable y `PUBLISHED` contra schema `2.0.0`, aplica a Magic
  Gladiator/Duel Master y materializa exactamente
  `58 + (lvl - 1) + vit * 2`.
- `EVD-0021` sustenta Vitality mínima 26 y `EVD-0026` autoriza expresión,
  alcance y truncamiento. `DSP-0002` afecta sólo Energy y no se hereda como
  conflicto de esta fórmula. Cuatro positivos reproducen 110/111/112/113 HP y
  cuatro negativos cubren nivel, Vitality, familia y overflow.
- Application materializa cinco referencias ejecutables y reproduce 20/20
  positivos y 20/20 errores desde JSON. WPF reutiliza selección, resolución
  contextual e intérprete genéricos sin handlers ni constantes de Magic
  Gladiator en C#.
- El dataset avanza a `2026-07-26.2` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de HP de Dark Lord cerrada desde `DR-HP-DARK-LORD` hasta
  WPF. `formula-hp-dark-lord` `1.0.0` nace ejecutable y `PUBLISHED` contra la
  evolución compatible del schema `2.1.0`, aplica a Dark Lord/Lord Emperor y
  materializa exactamente `50 + (lvl - 1) * 1.5 + vit * 2`.
- `EVD-0021` sustenta Vitality mínima 20 y `EVD-0026` autoriza expresión,
  alcance y truncamiento. Cuatro positivos conservan raw/visible
  `90/90`, `91.5/91`, `92/92` y `93.5/93`; cuatro negativos cubren nivel,
  Vitality, familia y overflow. No existe conflicto aplicable documentado.
- `CHECKED_DECIMAL_V1` conserva literales e intermedios base 10 exactos,
  ejecuta aritmética comprobada y aplica `TRUNCATE` una sola vez en
  `visible-hp`. `2.0.0`/`CHECKED_INT64_V1` permanecen intactos para las cinco
  fórmulas anteriores.
- Application materializa seis referencias ejecutables y reproduce 24/24
  positivos y 24/24 errores desde JSON. WPF y smoke reutilizan selección,
  resolución contextual e intérpretes genéricos sin handlers ni constantes de
  Dark Lord en C#.
- El dataset avanza a `2026-07-26.3` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de Mana de Dark Wizard cerrada desde
  `DR-MANA-DARK-WIZARD` hasta WPF. `formula-mana-dark-wizard` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.0.0`, aplica a Dark Wizard/Soul
  Master/Grand Master y materializa exactamente
  `(lvl - 1) * 2 + ene * 2`.
- `EVD-0021` sustenta Energy mínima 30 y `EVD-0026` autoriza expresión,
  alcance y truncamiento. Cuatro positivos reproducen 60/62/62/64 Mana y
  cuatro negativos cubren nivel, Energy, familia y overflow. No existe
  constante base ni conflicto aplicable documentado que incorporar.
- Application materializa siete referencias ejecutables y reproduce 28/28
  positivos y 28/28 errores desde JSON. WPF ofrece una selección genérica por
  referencia exacta cuando Dark Wizard dispone de HP y Mana, sin handlers ni
  constantes factuales en C#.
- El dataset avanza a `2026-07-26.4` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de Mana de Dark Knight cerrada desde
  `DR-MANA-DARK-KNIGHT` hasta WPF. `formula-mana-dark-knight` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a Dark Knight/Blade
  Knight/Blade Master y materializa exactamente
  `10 + (lvl - 1) * 0.5 + ene`.
- `EVD-0021` sustenta Energy mínima 10 y `EVD-0026` autoriza expresión,
  alcance y truncamiento. Cuatro positivos conservan raw/visible
  `20/20`, `20.5/20`, `21/21` y `21.5/21`; cuatro negativos cubren nivel,
  Energy, familia y overflow. No existe conflicto aplicable documentado.
- `CHECKED_DECIMAL_V1` conserva `0.5` exactamente, no redondea aportes y
  trunca una sola vez en `visible-mana`. Application materializa ocho
  referencias; WPF y smoke reutilizan selección, contexto e intérpretes
  genéricos sin handlers ni constantes factuales de Dark Knight en C#.
- El dataset avanza a `2026-07-26.5` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de Mana de Fairy Elf cerrada desde
  `DR-MANA-FAIRY-ELF` hasta WPF. `formula-mana-fairy-elf` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a Fairy Elf/Muse
  Elf/High Elf y materializa exactamente
  `15 + (lvl - 1) * 1.5 + ene`.
- `EVD-0021` sustenta Energy mínima 15 y `EVD-0026` autoriza expresión,
  alcance y truncamiento. Cuatro positivos conservan raw/visible
  `30/30`, `31.5/31`, `31/31` y `32.5/32`; cuatro negativos cubren nivel,
  Energy, familia y overflow. No existe conflicto aplicable documentado.
- `CHECKED_DECIMAL_V1` conserva `1.5` exactamente, no redondea aportes y
  trunca una sola vez en `visible-mana`. Application materializa nueve
  referencias; WPF y smoke reutilizan selección, contexto e intérpretes
  genéricos sin handlers ni constantes factuales de Fairy Elf en C#.
- El dataset avanza a `2026-07-26.6` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de Mana de Summoner cerrada desde
  `DR-MANA-SUMMONER` hasta WPF. `formula-mana-summoner` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a
  Summoner/Bloody Summoner/Dimension Master y materializa exactamente
  `40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`.
- `EVD-0021` sustenta Energy mínima 23; `EVD-0031` autoriza expresión y
  alcance final, y `EVD-0026` el truncamiento visible. `EVD-0027`–`EVD-0029`
  permanecen como contraste y `EVD-0030`, limitado a HP, no se hereda.
- Cuatro positivos conservan raw/visible
  `40/40`, `41.5/41`, `41.7/41` y `43.2/43`; cuatro negativos cubren nivel,
  Energy, familia y overflow. No existe conflicto aplicable documentado.
- `CHECKED_DECIMAL_V1` conserva `1.5` y `1.7` exactamente, no redondea aportes
  y trunca una sola vez en `visible-mana`. Application materializa diez
  referencias; WPF y smoke reutilizan selección, contexto e intérpretes
  genéricos sin handlers ni constantes factuales de Summoner en C#.
- El dataset avanza a `2026-07-26.7` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de Mana de Magic Gladiator cerrada desde
  `DR-MANA-MAGIC-GLADIATOR` hasta WPF.
  `formula-mana-magic-gladiator` `1.0.0` nace ejecutable y `PUBLISHED` contra
  schema `2.0.0`, aplica a Magic Gladiator/Duel Master y materializa exactamente
  `8 + (lvl - 1) + ene * 2`.
- `EVD-0021` sustenta Energy mínima 26 y `EVD-0026` autoriza expresión, alcance
  y truncamiento. `DSP-0002` se conserva como conflicto resuelto aplicable: el
  proyecto adopta 26 y no reescribe la guía actual de Webzen que publica 16.
- Cuatro positivos reproducen 60/61/62/63 Mana y cuatro negativos cubren nivel,
  Energy, familia y overflow. `CHECKED_INT64_V1` no desplaza Energy ni redondea
  aportes; trunca una sola vez en `visible-mana`.
- Application materializa once referencias; WPF y smoke reutilizan selección,
  resolución contextual e intérprete genéricos sin handlers ni constantes
  factuales de Magic Gladiator en C#.
- El dataset avanza a `2026-07-26.8` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de Mana de Dark Lord cerrada desde
  `DR-MANA-DARK-LORD` hasta WPF. `formula-mana-dark-lord` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a Dark Lord/Lord
  Emperor y materializa exactamente
  `40 + (lvl - 1) + (ene - 15) * 1.5`.
- `EVD-0021` sustenta Energy mínima 15 y `EVD-0026` autoriza expresión,
  alcance, orden y truncamiento. Cuatro positivos conservan raw/visible
  `40/40`, `41/41`, `41.5/41` y `42.5/42`; cuatro negativos cubren nivel,
  Energy, familia y overflow. No existe conflicto aplicable documentado.
- `CHECKED_DECIMAL_V1` conserva `1.5` exactamente, no redondea aportes y
  trunca una sola vez en `visible-mana`. Application materializa doce
  referencias; WPF y smoke reutilizan selección, contexto e intérpretes
  genéricos sin handlers ni constantes factuales de Dark Lord en C#.
- El dataset avanza a `2026-07-26.9` porque incorpora nueve JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de AG de Dark Wizard cerrada desde
  `DR-AG-DARK-WIZARD` hasta WPF. `formula-ag-dark-wizard` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a Dark Wizard/Soul
  Master/Grand Master y materializa exactamente
  `ene * 0.2 + vit * 0.3 + agi * 0.4 + str * 0.2`.
- `EVD-0021` sustenta los mínimos Energy 30, Vitality 15, Agility 18 y
  Strength 18; `EVD-0026` autoriza expresión, alcance y truncamiento. Cuatro
  positivos conservan raw/visible `21.3/21`, `21.8/21`, `21.9/21` y
  `22.4/22`; seis negativos cubren cada mínimo, familia y overflow.
- `CHECKED_DECIMAL_V1` conserva `0.2`, `0.3` y `0.4` exactamente, no redondea
  aportes y trunca una sola vez en `visible-ag`. La fórmula no consume nivel
  ni dependencias. Application materializa trece referencias; WPF y smoke
  reutilizan selección, contexto e intérpretes genéricos.
- El dataset avanza a `2026-07-26.10` porque incorpora once JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de AG de Dark Knight cerrada desde
  `DR-AG-DARK-KNIGHT` hasta WPF. `formula-ag-dark-knight` `1.0.0` nace
  ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a Dark Knight/Blade
  Knight/Blade Master y materializa exactamente
  `ene + vit * 0.3 + agi * 0.2 + str * 0.15`.
- `EVD-0021` sustenta los mínimos Energy 10, Vitality 25, Agility 20 y
  Strength 28; `EVD-0026` autoriza expresión, alcance y truncamiento. Cuatro
  positivos conservan raw/visible `25.70/25`, `27.00/27`, `26.05/26` y
  `27.35/27`; seis negativos cubren cada mínimo, familia y overflow.
- `CHECKED_DECIMAL_V1` conserva `0.3`, `0.2` y `0.15` exactamente, consume
  Energy directamente, no redondea aportes y trunca una sola vez en
  `visible-ag`. La fórmula no consume nivel ni dependencias. Application
  materializa catorce referencias; WPF y smoke reutilizan selección, contexto
  e intérpretes genéricos.
- El dataset avanza a `2026-07-26.11` porque incorpora once JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de AG de Fairy Elf cerrada desde
  `DR-AG-FAIRY-ELF` hasta WPF. `formula-ag-fairy-elf` `1.0.0` nace ejecutable
  y `PUBLISHED` contra schema `2.1.0`, aplica a Fairy Elf/Muse Elf/High Elf y
  materializa exactamente
  `ene * 0.2 + vit * 0.3 + agi * 0.2 + str * 0.3`.
- `EVD-0021` sustenta los mínimos Energy 15, Vitality 20, Agility 25 y
  Strength 22; `EVD-0026` autoriza expresión, familia y truncamiento. Cuatro
  positivos fijan raw/visible `20.6/20`, `21.1/21`, `21.1/21` y `21.6/21`;
  seis negativos cubren los cuatro mínimos, familia y overflow.
- `CHECKED_DECIMAL_V1` conserva `0.2` y `0.3` exactamente, no redondea aportes
  y trunca una sola vez en `visible-ag`. La fórmula no consume nivel ni
  dependencias. Application materializa quince referencias; WPF y smoke
  reutilizan selección, contexto e intérpretes genéricos.
- El dataset avanza a `2026-07-26.12` porque incorpora once JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de AG de Summoner cerrada desde `DR-AG-SUMMONER` hasta
  WPF. `formula-ag-summoner` `1.0.0` nace ejecutable y `PUBLISHED` contra
  schema `2.1.0`, aplica a Summoner/Bloody Summoner/Dimension Master y
  materializa exactamente
  `str * 0.2 + agi * 0.25 + vit * 0.3 + ene * 0.15`.
- `EVD-0021` sustenta los mínimos Strength 21, Agility 21, Vitality 18 y
  Energy 23; `EVD-0026` autoriza expresión, familia y truncamiento. Cuatro
  positivos fijan raw/visible `18.30/18`, `18.75/18`, `18.75/18` y
  `19.20/19`; cinco negativos cubren los cuatro mínimos y familia.
- `CHECKED_DECIMAL_V1` conserva los cuatro coeficientes exactamente, no
  redondea aportes y trunca una sola vez en `visible-ag`. No existe un control
  de overflow válido que materializar: la suma de coeficientes `0.9` mantiene
  la salida dentro de `INT64` para todo el dominio de inputs admitido.
- La fórmula no consume nivel ni dependencias. Application materializa
  dieciséis referencias; WPF y smoke reutilizan selección, contexto e
  intérpretes genéricos.
- El dataset avanza a `2026-07-28.1` porque incorpora diez JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de AG de Magic Gladiator cerrada desde
  `DR-AG-MAGIC-GLADIATOR` hasta WPF. `formula-ag-magic-gladiator` `1.0.0`
  nace ejecutable y `PUBLISHED` contra schema `2.1.0`, aplica a Magic
  Gladiator/Duel Master y materializa exactamente
  `ene * 0.15 + vit * 0.3 + agi * 0.25 + str * 0.2`.
- `EVD-0021` sustenta los cuatro mínimos 26 y `EVD-0026` expresión, familia y
  truncamiento. `DSP-0002` permanece resuelto a favor de Energy 26 y la
  divergencia documental que publica 16 conserva su clasificación.
- Cuatro positivos fijan raw/visible `23.40/23`, `23.85/23`, `23.85/23` y
  `24.30/24`; cinco negativos cubren los cuatro mínimos y familia. No se
  inventa overflow: la suma de coeficientes `0.9` mantiene la salida dentro de
  `INT64` para todo el dominio admitido.
- La fórmula no consume nivel ni dependencias. Application materializa
  diecisiete referencias; WPF y smoke reutilizan selección, contexto e
  intérprete decimal genéricos, sin constantes ni handlers factuales en C#.
- El dataset avanza a `2026-07-28.2` porque incorpora diez JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Vertical funcional de AG de Dark Lord cerrada desde
  `DR-AG-DARK-LORD` hasta WPF. `formula-ag-dark-lord` `1.0.0` nace ejecutable
  y `PUBLISHED` contra schema `2.1.0`, aplica a Dark Lord/Lord Emperor y
  materializa exactamente
  `ene * 0.15 + vit * 0.1 + agi * 0.2 + str * 0.3 + cmd * 0.3`.
- `EVD-0021` sustenta los mínimos Energy 15, Vitality 20, Agility 20, Strength
  26 y Command 25; `EVD-0026` autoriza expresión, familia y truncamiento.
  Cuatro positivos fijan raw/visible `23.55/23`, `23.80/23`, `24.35/24` y
  `24.60/24`; siete negativos cubren los cinco mínimos, familia y overflow.
- `CHECKED_DECIMAL_V1` conserva los coeficientes exactamente, no redondea
  aportes y trunca una sola vez en `visible-ag`. La suma `1.05` hace alcanzable
  el overflow para cinco entradas `Int64.MaxValue`, por lo que se conserva como
  control real. No existe conflicto aplicable documentado.
- La fórmula no consume nivel ni dependencias. Application materializa
  dieciocho referencias; `resolved-command` recorre la resolución contextual
  genérica y WPF/smoke reutilizan selección e intérprete decimal sin handlers
  ni constantes factuales en C#.
- El dataset avanza a `2026-07-28.3` porque incorpora doce JSON factuales
  nuevos. Ruleset `1.0.0` y motor `0.2.0` permanecen sin cambios.
- Dependencias ejecutables entre fórmulas implementadas de forma genérica.
  `FORMULA_OUTPUT` conserva referencia ID/version y etapa `RAW`/`VISIBLE`;
  Application ejecuta el grafo con el mismo estado validado, reutiliza
  resultados, rechaza ciclos y entrega una traza separada. Los mapas de inputs
  y trazas conservan `System.Decimal`; `DECIMAL` habilita salidas crudas
  fraccionarias en `CHECKED_DECIMAL_V1` sin relajar `INT32`/`INT64`.
- La primera prioridad de SD de Dark Wizard alcanzó un gate factual real.
  `EVD-0026` no declara si `defense = agi / 4` se consume `RAW` o `VISIBLE`;
  ambas etapas pueden divergir. No se crearon fórmulas, IDs, casos ni datos
  factuales de defensa/SD. `RES-0002` y el diseño técnico registran la decisión
  exacta pendiente.
- `EVD-0033` resuelve el gate por decisión explícita del propietario:
  `DR-SD-DARK-WIZARD` consume la salida `RAW` de
  `formula-defense-dark-wizard` `1.0.0`. La decisión no se extiende a otras
  familias ni reclasifica `EVD-0021`/`EVD-0026`.
- Vertical funcional de Defense/SD de Dark Wizard cerrada.
  `formula-defense-dark-wizard` y `formula-sd-dark-wizard` `1.0.0` nacen
  `PUBLISHED` contra schema `2.1.0`; la segunda declara
  `FORMULA_OUTPUT` por referencia exacta y `outputStage: RAW`.
- Defense conserva `agility / 4` como división decimal y publica cuatro casos
  `4.5/4`, `4.75/4`, `5/5` y `5.25/5`, más dos controles negativos. SD conserva
  `(str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30`, cuatro
  positivos y siete controles.
- El caso `sd-dark-wizard-raw-defense-boundary` fija nivel 4/Agility 19:
  Defense `RAW=4.75`, `VISIBLE=4` y SD `101.3083…/101`; consumir `VISIBLE`
  produciría 100 y queda cubierto como regresión factual.
- `DIVIDE` binario se incorpora exclusivamente a `CHECKED_DECIMAL_V1`, exige
  dos operandos, rechaza divisor cero y permanece inválido para
  `CHECKED_INT64_V1`. Así las divisiones por 4, 2 y 30 quedan declaradas sin
  aproximar `1/30`.
- Application y WPF materializan veinte fórmulas ejecutables y resuelven
  Defense/SD sobre el mismo estado validado, con trazas contextual, de
  dependencia y aritmética. El dataset avanza a `2026-07-28.4`, 219 JSON y
  `sha256:5ed158fe25450520d7a1eddd23edb451b00ab9f19c1d3c46f4e065474dd5082e`.
- `EVD-0034` registra la decisión del propietario para los cinco claims de SD
  que estaban pendientes: todos consumen la salida `RAW` de su Defense de la
  misma familia. Una revisión futura exige nueva evidencia, versión y casos.
- Vertical funcional de Defense/SD de Dark Knight cerrada.
  `formula-defense-dark-knight` y `formula-sd-dark-knight` `1.0.0` nacen
  `PUBLISHED` contra schema `2.1.0`, con dependencia exacta
  `FORMULA_OUTPUT`/`RAW`.
- Defense conserva `agility / 3`; SD conserva
  `(str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30`.
  Cada fórmula enlaza cuatro positivos; sus controles negativos son dos y siete.
  `sd-dark-knight-raw-defense-boundary` fija Defense `RAW=7.6666…`,
  `VISIBLE=7` y SD `107.0666…/107`, frente a 106 con la etapa incorrecta.
- Application y WPF materializan veintidós fórmulas ejecutables. El dataset
  avanza a `2026-07-28.5`, 238 JSON y
  `sha256:ee9d68d056bb2d917b784dd62dd320e1228c62983df37621efddf257dabbfcc7`.
- Vertical funcional de Defense/SD de Fairy Elf cerrada.
  `formula-defense-fairy-elf` y `formula-sd-fairy-elf` `1.0.0` nacen
  `PUBLISHED` contra schema `2.1.0`, con dependencia exacta
  `FORMULA_OUTPUT`/`RAW` autorizada por `EVD-0034`.
- Defense conserva `agility / 10`; SD conserva
  `(str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30`.
  Cada fórmula enlaza cuatro positivos; sus controles negativos son dos y siete.
  `sd-fairy-elf-raw-defense-boundary` fija Defense `RAW=2.7`, `VISIBLE=2` y SD
  `102.183333…/102`, frente a 101 con la etapa incorrecta.
- Application y WPF materializan veinticuatro fórmulas ejecutables. El dataset
  avanza a `2026-07-29.1`, 257 JSON y
  `sha256:a070bd16d1a1a2b60a9b81eeb393d160ac28039c2293e6ff25ebb7c4d2ffa66a`.
- Vertical funcional de Defense/SD de Magic Gladiator cerrada.
  `formula-defense-magic-gladiator` y `formula-sd-magic-gladiator` `1.0.0`
  nacen `PUBLISHED` contra schema `2.1.0` y enlazadas mediante
  `FORMULA_OUTPUT`/`RAW` conforme a `EVD-0034`.
- Defense conserva `agility / 5`; SD conserva
  `(str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30`.
  Cada fórmula enlaza cuatro positivos; sus controles negativos son dos y siete.
  `sd-magic-gladiator-raw-defense-boundary` fija Defense `RAW=5.6`,
  `VISIBLE=5` y SD `130.033333…/130`, frente a 129 con la etapa incorrecta.
- SD conserva `DSP-0002` porque consume Energy; el conflicto permanece resuelto
  a favor del mínimo factual 26. No se añadió evidencia ni se alteró su
  clasificación.
- Application y WPF materializan veintiséis fórmulas ejecutables. El dataset
  avanza a `2026-07-29.2`, 276 JSON y
  `sha256:64cfcd9cefe4056ea8ae7878a0cb71fb56e03178c14bdec78d2df1bb89beae78`.
- Vertical funcional de Defense/SD de Dark Lord cerrada.
  `formula-defense-dark-lord` y `formula-sd-dark-lord` `1.0.0` nacen
  `PUBLISHED` contra schema `2.1.0` y enlazadas mediante
  `FORMULA_OUTPUT`/`RAW` conforme a `EVD-0034`.
- Defense conserva `agility / 7`; SD conserva
  `(str + agi + vit + ene + cmd) * 1.2 + defense / 2 + (lvl * lvl) / 30`.
  Cada fórmula enlaza cuatro positivos; sus controles negativos son dos y ocho.
  `sd-dark-lord-raw-defense-boundary` fija Defense `RAW=3.571428571…`,
  `VISIBLE=3` y SD `135.019047619…/135`, frente a 134 con la etapa incorrecta.
- Application y WPF resuelven Command por la ruta contextual genérica y
  materializan veintiocho fórmulas ejecutables. El dataset avanza a
  `2026-07-29.3`, 296 JSON y
  `sha256:06114f61777a993cba3dacd4ddf45aba95bd7cc92be6b85dd51973c36d266eb4`.
- Vertical funcional de Defense/SD de Summoner cerrada.
  `formula-defense-summoner` y `formula-sd-summoner` `1.0.0` nacen
  `PUBLISHED` contra schema `2.1.0` y enlazadas mediante
  `FORMULA_OUTPUT`/`RAW` conforme a `EVD-0034`. Esta vertical cierra el único
  claim `VERIFIED` de `RES-0002` que quedaba sin materializar; `RES-0002`
  alcanza así 24/24 claims productivos.
- Defense conserva `agility / 3` sobre los mínimos `EVD-0021`: STR 21, AGI 21,
  VIT 18, ENE 23. SD conserva
  `trunc((str + agi + vit + ene) * 1.2) + trunc(defense / 2) + trunc((lvl * lvl) / 30)`
  con los tres truncamientos independientes fijados por `EVD-0032`, antes de
  sumar, consumiendo Defense `RAW`.
- `CHECKED_DECIMAL_V1` admite por primera vez varios pasos intermedios
  `APPLY_ROUNDING`: el gate (`SchemaContractValidator`), el reader de
  Application y el intérprete decimal aceptan truncamientos repetidos, siempre
  que el último paso visible sea el redondeo que consume `rawOutputStepId`.
  El intérprete entero se relaja de forma inocua (intermedios integrales).
- Cada fórmula enlaza cuatro positivos; los controles negativos son dos y siete.
  `sd-summoner-base` fija SD 102 (y discrimina la semántica de truncamientos
  independientes: truncar a plena precisión daría 103). No existe un caso
  frontera RAW/VISIBLE para Summoner porque, con `agi / 3`, siempre se cumple
  `trunc(RAW/2) == trunc(VISIBLE/2)`; no se fabrica ninguno.
- Application y WPF materializan treinta fórmulas ejecutables. El dataset avanza
  a `2026-07-29.4` con hash
  `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`
  (capturado por el smoke de publicación del 2026-09-01).

## No iniciado

- Schemas restantes (`ruleset`, quests, ítems, skills, escenarios y
  trazas); el validador integral/CI ya cubre el contrato de progresión.
- Builds completas, resto del motor de cálculo y flujos de UI
  posteriores al presupuesto ganado y los borradores locales.

## Decisiones abiertas

- El canal público de actualización y firma continúa como decisión posterior de
  distribución.

## Verificación más reciente — 2026-07-29

- Cierre de Defense/SD de Summoner: restauración y build Release aprobados con
  0 advertencias/0 errores; 320/320 pruebas pasan: 40 validator, 58 motor,
  204 Application y 18 Data. CLI del validador: las treinta fórmulas `PUBLISHED`
  pasan sin errores, incluidos `formula-defense-summoner` (4 positivos/2
  controles) y `formula-sd-summoner` (4 positivos/7 controles).
- `CHECKED_DECIMAL_V1` y el gate aceptan múltiples pasos intermedios
  `APPLY_ROUNDING`; `sd-summoner-base` fija SD 102 y discrimina la semántica de
  truncamientos independientes.
- Smoke WPF `win-x64`: PASS con SQLite `3.53.3`, 738 archivos,
  149.256.211 bytes, 315 JSON del ruleset, dataset `2026-07-29.4`, treinta
  fórmulas y 120 casos contextuales. El hash es
  `sha256:3b9cdcb42b7c7f6eb18063b6697f402bda5ae7e16fca5dfafef58143696d03d0`.
- Cierre de Defense/SD de Dark Lord: restauración bloqueada y build Release
  aprobados con 0 advertencias/0 errores; 302/302 pruebas pasan:
  40 validator, 57 motor, 187 Application y 18 Data. Los 18 casos nuevos
  reproducen ocho trazas positivas y diez errores canónicos.
- Comprobación estructural: 11 contratos/22 fixtures, 37 registros canónicos,
  progresión 7/7+3/3 y veintinueve identidades de fórmula aprobados. El gate
  factual cubre 116 positivos y 133 controles negativos.
- Smoke WPF `win-x64`: PASS con SQLite `3.53.3`, 719 archivos,
  149.231.704 bytes, 10 avisos legales, 296 JSON, dataset `2026-07-29.3`,
  veintiocho fórmulas y 112 casos contextuales. El hash es
  `sha256:06114f61777a993cba3dacd4ddf45aba95bd7cc92be6b85dd51973c36d266eb4`.
- Cierre de Defense/SD de Fairy Elf: restauración bloqueada y build Release
  aprobados con 0 advertencias/0 errores; 267/267 pruebas pasan:
  40 validator, 57 motor, 152 Application y 18 Data. La cobertura nueva fija
  los 17 casos directos de Defense/SD y la composición factual con dependencia
  `RAW`.
- Comprobación estructural: 11 contratos/22 fixtures aprobados. El CLI acepta
  11 válidos, rechaza 11 inválidos, valida 33 registros canónicos, progresión
  7/7+3/3 y veinticinco identidades de fórmula sin errores. El gate factual
  cubre 100 positivos y 114 controles negativos.
- Smoke WPF `win-x64`: PASS con SQLite `3.53.3`, 680 archivos,
  149.182.482 bytes, 10 avisos legales y 257 JSON. Conserva dataset
  `2026-07-29.1`, 24 fórmulas ejecutables y 96 casos contextuales, incluido SD
  sobre Defense `RAW`. `dotnet format --verify-no-changes` y
  `git diff --check` pasan.
- Schemas: 11/11 contratos y 22/22 fixtures estructuralmente legibles.
- Validador integral: 11/11 fixtures válidos aceptados, 11/11 inválidos rechazados
  y 33/33 registros canónicos válidos. Ejecuta además 7/7 casos de progresión,
  rechaza 3/3 controles semánticos y valida 100 casos positivos y 114 controles
  negativos de fórmula; las veinticinco identidades se informan
  `PUBLISHED`, sin errores. Sus 40/40 pruebas .NET aprueban también la
  ejecución repetida, los IDs estables, la retroactividad, la elegibilidad, los
  enlaces exactos 5+2 de las reglas publicadas y los contratos sintéticos de
  fórmula/traza/caso, cuatro mutaciones del gate factual y quince rechazos
  estructurales/semánticos específicos del programa v2. Dos controles nuevos
  fijan la equivalencia exacta de los casos y el rechazo de una identidad
  compuesta de caso duplicada.
- Solución .NET 10 de diez proyectos: restauración bloqueada y build Release
  aprobados con 0 advertencias y 0 errores; CLI del validador y formato
  verificados.
- La restauración final dentro del sandbox falló sólo por el bloqueo de red a
  `api.nuget.org` (`NU1301`/`NU1900`). La repetición autorizada del mismo
  comando terminó correctamente; `dotnet format --verify-no-changes` no detectó
  cambios en 114 archivos y la comprobación PowerShell aprobó 11 contratos/22
  fixtures.
- Persistencia SQLite: 18/18 pruebas de integración aprobadas en `win-x64`;
  junto con 40/40 pruebas del validador, 57/57 del motor y 152/152 de Application,
  la solución ejecuta 267/267 pruebas correctamente. Los seis casos de Data
  cubren la migración y el repositorio de borradores con payload/metadata,
  reemplazo, rollback, reapertura, lectura pura y contención tipada.
  Microsoft Testing Platform mantiene los TheoryData dinámicos dentro de esas
  pruebas de Application aunque ahora recorran 60 positivos, 66 negativos y
  60 rutas contextuales de fórmula.
- Motor de puntos: 7/7 casos positivos canónicos, 3/3 rechazos semánticos y
  2/2 controles de traza/publicación aprobados. Domain y Calculation Engine no
  incorporan paquetes externos ni referencias a Data, WPF o serialización.
- Intérprete de fórmulas: 25/25 pruebas sintéticas aprobadas. Ejecuta las cinco
  operaciones de `CHECKED_INT64_V1`, valida estado/aplicabilidad/inputs/bounds,
  aplica el código de rango materializado, controla overflow y conserva orden,
  outputs y provenance en la traza. Domain y Calculation Engine continúan sin
  serialización, Application, Data o WPF en esta vertical.
- Intérprete decimal: 5/5 pruebas sintéticas aprobadas. Conserva `1.5` y los
  intermedios fraccionarios exactamente, no redondea aportes, trunca sólo en la
  etapa declarada, divide por el divisor declarado, rechaza divisor cero y
  operandos del modelo incorrecto, y controla overflow al convertir la salida
  visible a `INT64`.
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
- Fórmulas en Application: 112/112 trazas positivas y 129/129 errores canónicos
  recorren directamente archivo → adaptador → catálogo → caso de uso →
  intérprete para las referencias enteras y la nueva vertical Defense/SD. El
  `1.0.0` histórico de Dark Wizard se rechaza como no ejecutable y las mutaciones
  relacionales fallan cerradas antes del cálculo. La ejecución normal no lee
  `reference-cases/`.
- Contexto de fórmulas en Application: 112/112 positivos canónicos reproducidos por
  snapshot → progresión → distribución → contexto → intérprete. Ocho pruebas
  fijan preservación de bases/evidencias/source, los seis códigos de contexto,
  overflow, inmutabilidad, fallos previos de las entradas de origen y la
  dependencia `RAW` de Defense en SD.
- WPF/Application: el build copia 296 JSON canónicos a una ruta estable tanto en
  salida normal como publicada. El flujo de ventana obtiene clase/evolución y
  Hero Status desde el catálogo/regla, muestra el total junto con la traza y
  conserva ese presupuesto para distribuir sobre controles derivados de
  `StatIds`. Muestra progresión, resets, total, gasto/remanente y errores
  tipados; App referencia
  Application y las capas internas no incorporan referencias inversas. El mismo
  composition root aplica la migración de borradores, configura contención en
  2 s × 3 intentos con 150 ms entre reintentos y conecta guardado/carga por ID
  sin omitir la revalidación de Application.
- Smoke WPF local posterior a la definición `1.1.0`: PASS en `win-x64`, SQLite
  `3.53.3`, 451 archivos y 148.758.166 bytes. Ambas publicaciones conservaron
  28/28 JSON y el hash
  `sha256:369c9e19fcc337a08df86df5e7744a111bfe80f3aaf9b3bcb17ce715f0636279`;
  progresión 7/7+3/3, distribución sintética, resets `2 × 100`, backup/restore
  y borrador persistido siguieron aprobados. WPF empaqueta la fórmula `DRAFT`
  pero no la materializa ni la ejecuta.
- Smoke WPF local posterior a los ocho casos `1.1.0`: PASS en `win-x64`,
  SQLite `3.53.3`, 459 archivos y 148.766.267 bytes. Ambas publicaciones
  conservaron 36/36 JSON y el hash
  `sha256:cfe267e51cf07532d5d1828fd524078bf832a122f367cf6381309e0d9010dbf7`;
  progresión 7/7+3/3, distribución sintética, resets `2 × 100`,
  backup/restore y borrador persistido siguieron aprobados. WPF empaqueta ambas
  series, pero no materializa ni ejecuta fórmulas.
- Smoke WPF local posterior a publicar `1.1.0`: PASS en `win-x64`, SQLite
  `3.53.3`, 459 archivos y 148.766.271 bytes. Ambas publicaciones conservaron
  36/36 JSON y el hash
  `sha256:712afd0b572e68d9025a07f3a8eabc7ee4ac1f8496552d19b4c4172b68332efe`.
  Progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  continuaron aprobados; WPF no materializa ni ejecuta fórmulas.
- Smoke WPF local posterior al intérprete genérico: PASS en `win-x64`, SQLite
  `3.53.3`, 459 archivos y 148.808.995 bytes. Conservó 10 avisos legales,
  36/36 JSON y el hash
  `sha256:712afd0b572e68d9025a07f3a8eabc7ee4ac1f8496552d19b4c4172b68332efe`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. El binario incluye el intérprete, pero WPF todavía no
  materializa ni ejecuta fórmulas.
- Smoke WPF local posterior al cierre funcional de HP: PASS en `win-x64`,
  SQLite `3.53.3`, 459 archivos y 148.871.955 bytes. Conservó 10 avisos legales,
  36/36 JSON y el hash existente del dataset; progresión 7/7+3/3,
  distribución, resets, backup/restore y borrador persistido siguieron
  aprobados. Ambas fases reprodujeron además 4/4 casos positivos de
  `formula-hp-dark-wizard` `1.1.0` con resolución contextual y traza
  aritmética.
- Smoke WPF local posterior al cierre de HP de Dark Knight: PASS en `win-x64`,
  SQLite `3.53.3`, 468 archivos y 148.884.866 bytes. Conservó 10 avisos legales,
  45/45 JSON y el hash
  `sha256:11a3d88ed670f998ba8ff3d5c149aa2f4017ae9ef1dd4a34994f35644a7024b3`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 8/8 positivos de
  `formula-hp-dark-knight` `1.0.0` y `formula-hp-dark-wizard` `1.1.0`.
- Smoke WPF local posterior al cierre de HP de Fairy Elf: PASS en `win-x64`,
  SQLite `3.53.3`, 477 archivos y 148.896.977 bytes. Conservó 10 avisos legales,
  54/54 JSON y el hash
  `sha256:aa3c761e9c3a8a2739c2cf424175c5d5b2ee703793f1489d2b8ebbb823521afa`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 12/12 positivos de las tres
  referencias ejecutables de HP.
- Smoke WPF local posterior al cierre de HP de Summoner: PASS en `win-x64`,
  SQLite `3.53.3`, 486 archivos y 148.908.207 bytes. Conservó 10 avisos legales,
  63/63 JSON y el hash
  `sha256:afb77b6daa3112da782dbcc68685f0f7e5bc3cbb1ae8f9bf3f6d1f80d0b61dc8`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 16/16 positivos de las cuatro
  referencias ejecutables de HP.
- Smoke WPF local posterior al cierre de HP de Magic Gladiator: PASS en
  `win-x64`, SQLite `3.53.3`, 495 archivos y 148.920.099 bytes. Conservó 10
  avisos legales, 72/72 JSON y el hash
  `sha256:a15ce32bc9610539ca406bdcdb80bbe7049f1879f0222014c9c10e28e30ed7aa`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 20/20 positivos de las cinco
  referencias ejecutables de HP.
- Smoke WPF local posterior al cierre de HP de Dark Lord: PASS en `win-x64`,
  SQLite `3.53.3`, 504 archivos y 148.940.709 bytes. Conservó 10 avisos legales,
  81/81 JSON y el hash
  `sha256:d7810c927ec692c161d0adcfcfa8cc6374d2213cac97805e9e77ec9d2ecefb32`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 24/24 positivos de las seis
  referencias ejecutables de HP, incluidos los raw fraccionarios de Dark Lord.
- Smoke WPF local posterior al cierre de Mana de Dark Wizard: PASS en
  `win-x64`, SQLite `3.53.3`, 513 archivos y 148.956.244 bytes. Conservó 10
  avisos legales, 90/90 JSON y el hash
  `sha256:a663630cbf054bef3c71eeb969b0edcb2dc95ec686d6cb2663fee8a2c5af6089`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 28/28 positivos de las siete
  referencias ejecutables de HP/Mana.
- Smoke WPF local posterior al cierre de Mana de Dark Knight: PASS en
  `win-x64`, SQLite `3.53.3`, 522 archivos y 148.967.275 bytes. Conservó 10
  avisos legales, 99/99 JSON y el hash
  `sha256:868ef53f5238066e928f51a56e2f375124c8d2f7b2a5fb6d75078c61c557c120`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 32/32 positivos de las ocho
  referencias ejecutables de HP/Mana.
- Smoke WPF local posterior al cierre de Mana de Fairy Elf: PASS en `win-x64`,
  SQLite `3.53.3`, 531 archivos y 148.977.675 bytes. Conservó 10 avisos legales,
  108/108 JSON y el hash
  `sha256:78d8688b3b08a02b13ea9971fa45f90ff59612c6c5b1a2981cb102b82c1adc7e`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 36/36 positivos de las nueve
  referencias ejecutables de HP/Mana.
- Smoke WPF local posterior al cierre de Mana de Summoner: PASS en `win-x64`,
  SQLite `3.53.3`, 540 archivos y 148.989.299 bytes. Conservó 10 avisos legales,
  117/117 JSON y el hash
  `sha256:7d0e75d9212837a9245a339253b9622ecff2ec4b157cb839606601c3ee73331b`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 40/40 positivos de las diez
  referencias ejecutables de HP/Mana.
- Smoke WPF local posterior al cierre de Mana de Magic Gladiator: PASS en
  `win-x64`, SQLite `3.53.3`, 549 archivos y 149.001.252 bytes. Conservó 10
  avisos legales, 126/126 JSON y el hash
  `sha256:cff8d5726f433448ac7212a7a6e7465475024cc4346e904a649bcc2615e706f5`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 44/44 positivos de las once
  referencias ejecutables de HP/Mana.
- Smoke WPF local posterior al cierre de Mana de Dark Lord: PASS en `win-x64`,
  SQLite `3.53.3`, 558 archivos y 149.012.089 bytes. Conservó 10 avisos legales,
  135/135 JSON y el hash
  `sha256:440b7fb4a1ef1bd5b57202323bf7cb447c3bf252abe81cf112f8832220e7817a`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 48/48 positivos de las doce
  referencias ejecutables de HP/Mana.
- Smoke WPF local posterior al cierre de AG de Dark Wizard: PASS en `win-x64`,
  SQLite `3.53.3`, 569 archivos y 149.025.417 bytes. Conservó 10 avisos legales,
  146/146 JSON y el hash
  `sha256:469f18aec26813dcb75dab054df3df7b2469d730d76d532ed2ef8432711e4651`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 52/52 positivos de las trece
  referencias ejecutables de HP/Mana/AG.
- Smoke WPF local posterior al cierre de AG de Dark Knight: PASS en `win-x64`,
  SQLite `3.53.3`, 580 archivos y 149.038.270 bytes. Conservó 10 avisos legales,
  157/157 JSON y el hash
  `sha256:67d6fd0e614d3072f214b7dc09f7295e2450d2b4b7c0cd24a91f70a14dad13ef`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 56/56 positivos de las catorce
  referencias ejecutables de HP/Mana/AG.
- Smoke WPF local posterior al cierre de AG de Fairy Elf: PASS en `win-x64`,
  SQLite `3.53.3`, 591 archivos y 149.051.441 bytes. Conservó 10 avisos legales,
  168/168 JSON y el hash
  `sha256:432b6a062fbd5ed996a9d58dbfa32daafec5cf09c447dc7c40bb0ac98e645177`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 60/60 positivos de las quince
  referencias ejecutables de HP/Mana/AG.
- Smoke WPF local posterior al cierre de AG de Summoner: PASS en `win-x64`,
  SQLite `3.53.3`, 601 archivos y 149.063.645 bytes. Conservó 10 avisos legales,
  178/178 JSON y el hash
  `sha256:6380346e97b61e31a6da3329b86f91954f2b120d880e751733e778f4cbb75f43`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 64/64 positivos de las
  dieciséis referencias ejecutables de HP/Mana/AG.
- Smoke WPF local posterior al cierre de AG de Magic Gladiator: PASS en
  `win-x64`, SQLite `3.53.3`, 611 archivos y 149.076.719 bytes. Conservó 10
  avisos legales, 188/188 JSON y el hash
  `sha256:5246861cec04e5e618611091d365e7e0a4c03d8227013f84c13e93354253d901`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 68/68 positivos de las
  diecisiete referencias ejecutables de HP/Mana/AG.
- Smoke WPF local posterior al cierre de AG de Dark Lord: PASS en `win-x64`,
  SQLite `3.53.3`, 623 archivos y 149.091.558 bytes. Conservó 10 avisos legales,
  200/200 JSON y el hash
  `sha256:66e83b7bef261a6d310e4c5933522a6387aaab56ce7f50a396df1df2988e47eb`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 72/72 positivos de las
  dieciocho referencias ejecutables de HP/Mana/AG.
- Smoke WPF local posterior al cierre de Defense/SD de Fairy Elf: PASS en
  `win-x64`, SQLite `3.53.3`, 680 archivos y 149.182.482 bytes. Conservó 10
  avisos legales, 257/257 JSON y el hash
  `sha256:a070bd16d1a1a2b60a9b81eeb393d160ac28039c2293e6ff25ebb7c4d2ffa66a`;
  progresión 7/7+3/3, distribución, resets, backup/restore y borrador persistido
  siguieron aprobados. Ambas fases reprodujeron 96/96 positivos de las
  veinticuatro referencias ejecutables, incluida la dependencia Defense `RAW`
  de Fairy Elf.
- Json Everything fuente: 2 compilaciones independientes con SDK `10.0.301`,
  restore bloqueado de los tres proyectos fuente, hashes esperados para 3/3
  DLL y SPDX contrastado. El harness actualizado ejecuta 2 × 22/22 fixtures y
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
- El motor incorpora sólo los datos factuales autorizados por los registros
  cerrados: seis
  clases y dos reglas de progresión canónicas publicadas con treinta referencias
  ejecutables. HP de Dark Wizard, Dark Knight, Fairy Elf, Summoner, Magic
  Gladiator y Dark Lord se
  materializan y
  ejecutan en Application/WPF con nivel y Vitality resueltos desde el estado validado.
  Mana de Dark Wizard, Dark Knight, Fairy Elf, Summoner, Magic Gladiator y Dark
  Lord también se ejecuta con nivel y Energy resueltos. AG de Dark Wizard, Dark
  Knight, Fairy Elf, Summoner y Magic Gladiator se ejecuta con sus cuatro stats
  resueltos; AG de Dark Lord añade Command por la misma ruta contextual.
  Defense y SD de Dark Wizard, Dark Knight, Fairy Elf, Magic Gladiator, Dark
  Lord y Summoner se ejecutan con dependencia `RAW`; daño y el
  resto de Defense todavía no se ejecutan.
- Licencia: texto Apache-2.0 contrastado con la publicación oficial; ADR-0005,
  `NOTICE` e inventario de terceros incorporados. La auditoría leyó metadatos
  `.nuspec` de todas las dependencias restauradas y el acuerdo incluido por la
  familia Json Everything.
- Investigación documental: `RES-0001` conserva 21 evidencias, 6/6 claims
  `VERIFIED` y dos conflictos resueltos. `RES-0002` queda `VERIFIED` con 24/24
  claims, once evidencias y dos conflictos resueltos.
  EVD-0026 autoriza diseñar contratos y casos para su alcance exacto; HP de Dark
  Wizard, Dark Knight, Fairy Elf, Summoner, Magic Gladiator y Dark Lord, más
  Mana de Dark Wizard, Dark Knight, Fairy Elf, Summoner, Magic Gladiator y Dark
  Lord, más AG de las seis familias y SD de todas las familias (Dark
  Wizard/Dark Knight/Fairy Elf/Magic Gladiator/Dark Lord/Summoner), ya son
  productivos; `RES-0002` queda completamente materializado (24/24).
  EVD-0014–EVD-0018 trazan stats, puntos, Marlon, contraste oficial y decisiones
  del propietario; EVD-0019/EVD-0020 trazan la presencia anterior de la matriz
  candidata y la búsqueda negativa de un original o snapshot contemporáneo;
  EVD-0021 fija la matriz como axioma del ruleset y cierra `RES-0001`.
- Contrato documental de HP de Dark Wizard: revisión técnica aprobada para
  `formula-hp-dark-wizard` `1.0.0`; `EVD-0026` traza la expresión y
  aplicabilidad, mientras `EVD-0021` traza el mínimo canónico de Vitality. La
  estrategia de tres contratos, la definición JSON, los ocho casos y el gate
  semántico están implementados. La revisión de publicación no encontró
  divergencias y la fórmula está `PUBLISHED`; su versión ejecutable `1.1.0`
  ya recorre Application y Calculation Engine.
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
  fuente, 2 × 22/22 fixtures, formatos, SBOM, locks, auditoría y publicación
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

## Decisión del propietario — 2026-07-25

- `next-actions.md` debe definir tareas verticales más completas.
- Cada primera prioridad combinará diseño restante, implementación,
  integración, pruebas, documentación y smoke cuando puedan completarse de
  forma coherente.
- No se separará una misma capacidad en iteraciones exclusivamente documentales,
  de código y de UI salvo que exista un gate técnico o factual real.
