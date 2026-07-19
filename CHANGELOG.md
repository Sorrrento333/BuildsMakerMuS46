# Changelog

## [Unreleased]

### Added

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
