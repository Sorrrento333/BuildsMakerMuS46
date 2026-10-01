# Diseño técnico — comparación bajo escenario, breakpoints y avisos (cierre de UC-06)

## Estado y alcance

- Fecha: 2026-09-30.
- Estado: `IMPLEMENTED` (Application + WPF + smoke, autorización del
  propietario 2026-09-30 para cerrar UC-06).
- Verificación: build Release 0/0, 896/896 pruebas, estructura 17/34, smoke
  WPF `win-x64` PASS.
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-20.1` (sin cambios: ningún JSON factual nuevo).
- Capas incluidas: Application (`CompareBuildsUseCase`, códigos `compare-*`),
  WPF (extensión de la sección «Comparador de builds») y smoke de publicación.
- Capas excluidas: Domain, Calculation Engine, Data, schemas y migraciones
  SQLite (ningún contrato persistido nuevo).

Cierra los tres puntos excluidos en `build-comparison-design.md` (escenario,
breakpoints y advertencias) sin inventar ninguna regla del juego.

## Principio de no-invención

Ninguna fórmula publicada consume entradas de escenario, y no existe ningún
escenario factual publicado (el contrato `scenario` sólo tiene fixtures
sintéticos). Por tanto:

- El **escenario** es contexto definido por el usuario (modalidad del
  vocabulario del contrato: `PVM`/`PVP`/`HYBRID`, más nombre, objetivo y notas
  libres). Se refleja en el informe y **no altera ningún número calculado**.
- La **modalidad** sólo activa un foco de presentación mecánico: con `PVM` o
  `PVP` se destacan las diferencias derivadas cuya `FormulaId` publicada
  contiene `-pvm-`/`-pvp-` (coincidencia ordinal exacta sobre IDs del catálogo).
  `HYBRID` o ausencia de escenario = sin foco. Es ayuda de lectura, no regla.
- Los **breakpoints de usuario** (`stat` por `statId`, `derived` por referencia
  exacta `id@version`) son parámetros de presentación del usuario (una regla,
  no una ley): el informe indica por build si alcanza el objetivo y con qué
  margen. Clave desconocida → fail-closed `compare-unknown-breakpoint`.
  Derivado no aplicable a una build, o stat ausente en un lado → `null`
  (sin inventar ceros, misma semántica nula existente).
- Los **breakpoints de equipo** son factuales: por build y por ítem `PUBLISHED`
  del catálogo se comprueba clase permitida y stats finales contra los
  `requiredStats` publicados en +0 (baseline; la progresión por nivel sigue
  excluida por el gate). Requisito incumplido → aviso.
- Los **buffs externos** (`externalBuffIds` del contrato) quedan
  **excluidos**: referenciarían el catálogo de skills como entidad y violarían
  `docs/DECISIONES-PRODUCTO.md` (skill sólo como modificador de cálculo).

## Contrato de Application

- `ComparisonModality { Pvm, Pvp, Hybrid }`.
- `ComparisonScenario(Modality, DisplayName?, Objective?, Notes?)`:
  `DisplayName` informado debe ser no vacío → si no,
  `compare-invalid-scenario`.
- `BreakpointTarget(Kind, Key, Target)`, `BreakpointKind { Stat, Derived }`;
  `Key` en derivados = `id@version` exacta (el mismo criterio que
  `DerivedDifference`, porque un `OutputId` lo producen varias fórmulas).
- `BuildComparisonOptions(Scenario?, BreakpointTargets[])`, inmutable.
- `CompareBuildsUseCase(progressionCatalog, formulaCatalog, itemCatalog?)`:
  el catálogo de ítems es opcional y retrocompatible (constructor existente
  intacto). Sin catálogo, la sección de requisitos queda vacía.
- `Execute(first, second)` conserva el comportamiento 2026-09-27 byte a byte.
  `Execute(first, second, options)` añade al informe:
  - `Scenario` (eco), `FocusedReferences` (referencias destacadas por modalidad,
    orden ordinal determinista),
  - `BreakpointResults` (por objetivo: `Target`, valores `First`/`Second`
    `nullables`, `MeetsFirst`/`MeetsSecond` `nullables`, márgenes `checked`),
  - `Warnings` (`ComparisonWarning(Code, Side, Detail)` con códigos
    `compare-warning-target-missed` y `compare-warning-requirement-unmet`,
    un aviso por build × objetivo/ítem, orden determinista).
- Con las mismas opciones, intercambiar los lados niega las diferencias e
  intercambia los papeles First/Second de objetivos y avisos.

## WPF

- La sección «Comparador de builds» gana: modalidad (`Sin escenario`/`PVM`/
  `PVP`/`Híbrido`), nombre y objetivo opcionales, y caja de objetivos con
  formato `stat:<statId>=<n>` / `derived:<id>@<version>=<n>` (una por línea).
  Formato inválido, clave desconocida y escenario inválido muestran su código
  con explicación en español; los errores de carga/fórmula reutilizan sus
  traducciones. El informe añade escenario, foco, objetivos y avisos.

## Smoke WPF

- `VerifyBuildComparison` añade una pasada con opciones sobre
  `publication-smoke-build` vs `publication-smoke-equip-build`: escenario `PVM`
  con objetivo `stat:strength` y aviso esperado por requisito (una build no
  cumple el requisito publicado de un ítem del catálogo), foco no vacío bajo
  `PVM` y vacío bajo `HYBRID`, e inversión de papeles al intercambiar el orden.
  Sin campos nuevos de reporte salvo los necesarios para estas aserciones.

## Verificación esperada

- Al menos ocho pruebas de integración con el snapshot canónico: eco de
  escenario y foco PVM/PVP/HYBRID/ausente; objetivo de stat alcanzado/fallado/
  nulo; objetivo derivado y base cero; clave desconocida; escenario inválido;
  requisito cumplido/incumplido contra el catálogo; simetría con opciones;
  sección de requisitos vacía sin catálogo.
- Suite completa en verde; estructura 17/34; smoke WPF `win-x64` PASS.

## Fuera de alcance

- Efectos numéricos de escenario o buffs externos sobre el cálculo.
- Umbrales o diagnósticos propios del juego (p. ej. «esta AGI da X»).
- Borrado/renombrado de builds, exportación y pantallas restantes.
- Cualquier dato, fórmula o regla nuevos de MU Online.
