# Contrato de persistencia de borradores de build

## Alcance

Esta especificación gobierna la primera persistencia de usuario. Modela
exclusivamente la identidad, las entradas de progresión, la configuración de
resets y la distribución de stats que ya ejecuta la aplicación. No incorpora
equipo, buffs, atributos derivados ni datos nuevos de MU Online.

El contrato serializable es `build-draft.schema.json` `1.1.0`. Sus fixtures son
sintéticos. Application materializa el modelo, el puerto y los casos de uso;
Data implementa el puerto y su migración. WPF compone ambas capas y ofrece el
primer flujo local de guardado/carga por ID.

## Separación respecto de `build.schema.json`

`build.schema.json` representa una build más completa y exige dataset, versión
del motor, resets y un mapa `stats`. El borrador ya conoce los inputs
configurables de resets, pero sus asignaciones continúan sin equivaler a valores
finales de stats y todavía no representa el resto de una build completa.

Por ello el borrador usa un contrato independiente. No modifica ni reinterpreta
`build.schema.json`; una futura promoción de borrador a build completa requerirá
un caso de uso y validaciones propios.

## Campos persistidos

| Campo | Origen | Autoridad al recargar |
|---|---|---|
| `id` | Identidad estable creada por Application | Autoritativo para guardar, reemplazar y cargar |
| `ruleset.id/version` | Paquete de ruleset resuelto | Gate de compatibilidad; no se infiere |
| `dataset.version/hash` | Snapshot exacto resuelto | Gate de integridad; no se infiere |
| `engineVersion` | Versión del motor que calculó el resultado | Gate de compatibilidad |
| `progressionInputs` | Clase, evolución, nivel y quests elegidos por el usuario | Autoritativo |
| `resetInputs` | Cantidad de resets y puntos por reset configurados por el usuario | Autoritativo |
| `statDistribution.allocations` | Asignaciones elegidas por el usuario | Autoritativo |
| Resto de `statDistribution` | Resultado calculado y su referencia de origen | Caché verificable, nunca nueva verdad factual |

`BuildDraftRuntimeContext` expone juntos el catálogo y el metadata exacto de
ruleset, dataset/hash y motor. El llamador debe aportarlos explícitamente; el
caso de uso no usa el nombre de una carpeta, la fecha, la versión de un
ensamblado ni una constante inferida como sustituto.

## Invariantes al recargar

JSON Schema valida forma, rangos, versiones y la composición completa de
`stat-distribution.schema.json`. Application aplica además:

1. `ruleset.id` coincide con `statDistribution.rulesetId`.
2. `progressionInputs.characterClassId` coincide con
   `statDistribution.characterClassId`.
3. Ruleset, dataset y motor exactos están disponibles antes de calcular.
4. Se recalcula `ProgressionPointBudgetResult` desde `progressionInputs`.
5. Se recalcula `StatDistributionResult` usando únicamente `resetInputs` y las
   asignaciones guardadas.
6. Ruleset, clase, regla/version, presupuesto de progresión, resets, total
   distribuible, asignaciones, gasto y remanente recalculados coinciden
   exactamente con la caché persistida.
7. Sólo el resultado recalculado se entrega al llamador. La caché nunca evita la
   validación ni se promueve a dato del ruleset.

Una divergencia falla cerrada. No se corrige silenciosamente el borrador ni se
reemplazan versiones durante una carga.

Application admite borradores `1.0.0` ya persistidos: los normaliza en memoria a
`1.1.0` con `resetCount=0`, `pointsPerReset=0`, `resetPoints=0` y
`totalDistributablePoints=earnedPoints`, y luego ejecuta la misma revalidación.
El payload almacenado no se reescribe durante la lectura.

## Errores estables

| Código | Condición |
|---|---|
| `build-draft-not-found` | No existe el ID solicitado. |
| `build-draft-schema-unsupported` | La versión serializada no está soportada. |
| `build-draft-dependency-unavailable` | No está disponible el ruleset, dataset o motor exacto. |
| `build-draft-source-mismatch` | Las identidades internas no coinciden. |
| `build-draft-revalidation-failed` | El recálculo no reproduce la caché persistida. |
| `build-draft-write-conflict` | La escritura no puede completarse bajo la política transaccional. |

Estos códigos forman parte de Application mediante `BuildDraftException` y WPF
los muestra junto con una traducción en español.

## Límite Application/Data

Application es propietario de los casos de uso y del puerto sin tipos SQLite:

- guardar un borrador completo bajo su `id`;
- cargar por `id` y devolverlo sólo después de revalidarlo;
- distinguir ausencia, incompatibilidad y divergencia;
- aportar a Data una operación corta e idempotente.

Data implementa el puerto mediante `SqliteBuildDraftRepository`. Un guardado es
un `UPSERT` por `id` dentro de una única transacción inmediata gestionada por
`SqliteWriteContentionPolicy`. La fila conserva el payload JSON completo y
columnas explícitas para schema, ruleset, dataset/hash y motor; todos se
confirman o revierten juntos. El agotamiento de contención se traduce al código
estable `build-draft-write-conflict`. Una carga ejecuta sólo un `SELECT`, no
revalida ni modifica datos.

`SqliteBuildDraftMigrations.All` publica la migración hacia adelante
`1/create_build_drafts`. El llamador debe aplicarla con
`SqliteMigrationRunner` antes de construir el repositorio. Data depende de
Application únicamente para implementar el puerto; Application no referencia
Data ni tipos SQLite.

## Composición WPF

`PublishedBuildDraftServices` crea la base
`%LOCALAPPDATA%\MuOnline.BuildPlanner\build-planner.sqlite`, aplica
`SqliteBuildDraftMigrations.All` y sólo después construye
`SqliteBuildDraftRepository`. El smoke inyecta un directorio temporal externo a
los binarios, pero recorre la misma composición.

La composición declara ruleset `1.0.0`, dataset `2026-07-24.1` y motor `0.2.0`.
El hash del dataset se calcula de forma determinista sobre los 27 JSON
publicados: ruta relativa normalizada con `/`, byte nulo, contenido exacto y
byte nulo, ordenados por ruta y acumulados con SHA-256. Así una carga exige el
snapshot byte a byte sin deducir metadata desde la carpeta, la fecha del sistema
o la versión de un ensamblado.

La política interactiva usa timeout de dos segundos por intento, dos reintentos
y 150 ms entre intentos. El agotamiento llega a la pantalla como
`build-draft-write-conflict`, siempre acompañado por una explicación en
español sin ocultar el código estable.

## Casos mínimos

La etapa estructural conserva:

- fixture válido con metadata sintético, entradas de progresión y una
  distribución completa válida;
- fixture con envoltorio de borrador válido y distribución interna inválida,
  rechazado exclusivamente mediante el `$ref` real hacia
  `stat-distribution.schema.json`.

La vertical Application cubre con un repositorio sintético en memoria: alta y
carga, reemplazo por el mismo ID, borrador ausente, incoherencia de identidad,
dependencia exacta no disponible, caché alterada y round-trip serializable con
los nombres del schema.

La vertical Data cubre alta/carga, reemplazo por ID, metadata y payload exactos,
rollback ante fallo, reapertura, lectura ausente sin mutaciones y traducción del
agotamiento de contención. Ningún caso usa datos o fórmulas del juego.

El smoke publicado guarda un borrador sintético derivado de un caso canónico ya
aprobado, lo carga mediante `LoadBuildDraftUseCase`, realiza backup/restore y
repite la carga desde los binarios de reemplazo. Exige identidad, metadata,
hash, asignaciones y resultado recalculado exactos.
