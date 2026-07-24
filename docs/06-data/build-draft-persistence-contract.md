# Contrato de persistencia de borradores de build

## Alcance

Esta especificación gobierna la primera persistencia de usuario. Modela
exclusivamente la identidad, las entradas de progresión y la distribución de
stats que ya ejecuta la aplicación. No incorpora resets, equipo, buffs,
atributos derivados ni datos nuevos de MU Online.

El contrato serializable es `build-draft.schema.json` `1.0.0`. Sus fixtures son
sintéticos. Application materializa el modelo, el puerto y los casos de uso;
Data implementa el puerto y su migración. La integración WPF permanece fuera
de esta vertical.

## Separación respecto de `build.schema.json`

`build.schema.json` representa una build más completa y exige dataset, versión
del motor, resets y un mapa `stats`. El flujo actual no conoce resets y sus
asignaciones no equivalen a valores finales de stats. Completar esos campos con
ceros o inferencias convertiría ausencia de implementación en datos.

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
5. Se recalcula `StatDistributionResult` usando únicamente las asignaciones
   guardadas.
6. Ruleset, clase, regla/version, presupuesto, asignaciones, gasto y remanente
   recalculados coinciden exactamente con la caché persistida.
7. Sólo el resultado recalculado se entrega al llamador. La caché nunca evita la
   validación ni se promueve a dato del ruleset.

Una divergencia falla cerrada. No se corrige silenciosamente el borrador ni se
reemplazan versiones durante una carga.

## Errores estables

| Código | Condición |
|---|---|
| `build-draft-not-found` | No existe el ID solicitado. |
| `build-draft-schema-unsupported` | La versión serializada no está soportada. |
| `build-draft-dependency-unavailable` | No está disponible el ruleset, dataset o motor exacto. |
| `build-draft-source-mismatch` | Las identidades internas no coinciden. |
| `build-draft-revalidation-failed` | El recálculo no reproduce la caché persistida. |
| `build-draft-write-conflict` | La escritura no puede completarse bajo la política transaccional. |

Estos códigos ya forman parte de Application mediante `BuildDraftException`.
Todavía no existen traducciones de UI.

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
