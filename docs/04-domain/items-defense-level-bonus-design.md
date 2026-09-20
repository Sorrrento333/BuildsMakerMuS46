# Diseño técnico — ampliación de UC-04 con defensa de armadura por nivel

## Estado y alcance

- Fecha: 2026-09-20.
- Estado: `IMPLEMENTED` (vertical cerrada con PR a `main`).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-20.1` (materializado en esta vertical).
- Registro factual: `docs/05-research/registers/RES-0005-uc04-bono-required-sockets.md`
  (`VERIFIED`, `DSP-0012` `RESOLVED` por `OWNER_DECISION`, `EVD-0053`).
- Consumo previo: `docs/04-domain/equipped-instance-design.md`
  (instancia `{ itemId, itemVersion, level }`)
  y `docs/04-domain/items-consumption-design.md` (UC-04, elegibilidad).

Este documento es la primera vertical de implementación autorizada por el
axio-axioma parcial de `EVD-0053`: ampliar UC-04 con la progresión defensiva
por nivel de ítem, **sólo** para el subconjunto acotado y **sólo** con la
semántica que el propietario fijó el 2026-09-20.

## Decisión del propietario (2026-09-20)

Tres decisiones cerradas antes de diseñar:

1. **Semántica de la regla de armadura** — aditiva sobre la base del ítem:
   `DEF(n) = DEF+0 × (1 + 0,05·n)`, con un único truncamiento hacia cero en la
   salida (ningún redondeo intermedio), coherente con la convención de las
   demás fórmulas del ruleset.
2. **Materializar `defense` en +0** — el campo `defense` (base, entero, `>= 0`)
   vive en `item.schema.json` y `item-dragon-armor.json` con el valor factual
   ya verificado de Fanz (`EVD-0038`, re-captura `EVD-0048`): `37`.
3. **Diferir JOL** — la regla `+5 STR` por nivel de opción queda documentada en
   el ruleset pero **no** se implementa hasta que `equipmentEntry` modele
   opciones. No amplía `build.schema.json` en esta vertical.

## Alcance materializable

| Campo / regla | Decisión | Se implementa |
|---|---|---|
| `defense` +0 (base) | Adoptado (dato factual verificado) | Sí |
| `DEF(n) = trunc(base × (1 + 0,05·n))` para armadura | Adoptado (axioma parcial) | Sí, como valor derivado en Application |
| JOL `+5 STR` por nivel de opción | Diferido (requiere opciones en equipment) | No |
| `requiredLevel` | Excluido (sin fuente) | No |
| ATK por nivel de ítem (armas) | Excluido (sin fuente) | No |
| Progresión `requiredStats` por nivel | Excluido (sin fuente) | No |
| Sockets | Excluido (grado normal, `socketSlots` 0) | No |

La regla aplica a **armadura**: en el subconjunto sólo `item-dragon-armor`
tiene `defense`. Kris y Albatross Bow (armas) no declaran `defense` en sus
definiciones; su base no es candidata a la progresión defensiva.

## Contrato de dominio

- `Domain.Items.ItemDefinition` gana `Defense` (`long?`, nulo cuando el ítem no
  publica defensa). Es la única adición al record actual
  (`Id`, `Version`, `RulesetId`, `DisplayName`, `Status`, `Slots`,
  `AllowedClassIds`, `RequiredStats`, `MaxItemLevel`, `Defense`).

## Contrato de schemas

- `packages/schemas/v1/item.schema.json` pasa de `1.0.0` a `1.1.0`:
  - `schemaVersion` `const` → `"1.1.0"`.
  - Nueva propiedad **opcional** `defense`: `{ "type": "integer", "minimum": 0 }`.
    Es opcional porque las armas del subconjunto no publican defensa.
  - El resto de `required`/propiedades no cambia.
- Los tres registros canónicos actualizan `schemaVersion` a `"1.1.0"`:
  - `item-dragon-armor.json`: además gana `"defense": 37` y sube
    `version` `1.0.0` → `1.1.0` (cambio de contenido). `evidenceRefs` gana
    `"evd-0048"` y `"evd-0053"`.
  - `item-kris.json` y `item-albatross-bow.json`: sólo cambia `schemaVersion`
    a `"1.1.0"`; `version` se mantiene en `1.0.0` (sin cambio de contenido) y
    **no** declaran `defense`.
- Fixtures sintéticos: `packages/schemas/examples/valid/item.json` sube
  `schemaVersion` a `"1.1.0"` y declara un `defense` como parte del ejemplo
  válido; `packages/schemas/examples/invalid/item.json` sube
  `schemaVersion` a `"1.1.0"` (y conserva un caso inválido). No cambia el
  inventario de contratos ni de fixtures del validador.

## Contrato de Application

- `JsonItemCatalogSnapshotReader.SupportedSchemaVersion` → `"1.1.0"`; el parser
  lee `defense` opcional (ausente → `null`; presente y negativo → fail-closed
  con `ItemCatalogSnapshotErrorCodes.SnapshotInvalid`).
- Nuevo componente derivado **`ItemDefenseBonusCalculator`** (Application,
  paquete `Items/`): aplica únicamente la regla
  `DEF(n) = trunc(base × (1 + 0,05·n))` con aritmética decimal comprobada y un
  único truncamiento hacia cero en la salida. No codifica datos del juego: sólo
  el coeficiente de la regla, trazado a `EVD-0050`/`EVD-0053` en el código y en
  los casos de prueba.
- `EquipItemResult` gana `Defense` (base, `long?`) y `DefenseAtLevel` (`long?`,
  derivado del `Level` solicitado mediante el calculador; `null` cuando el ítem
  no declara `defense`). `EquipItemUseCase` lo rellena desde el catálogo.

## Verificación esperada

- Lectura del snapshot: `defense` `37` en `item-dragon-armor`; `null` en
  Kris y Albatross Bow.
- Calculador sobre base `37` para `n` en `0..15` (todo el rango), con control
  explícito de bordes y de ausencia de base:
  - `n=0 → 37`, `n=1 → 38`, `n=7 → 49`, `n=10 → 55`, `n=15 → 64`
    (truncamiento correcto de los saltos fraccionarios; sin redondeo intermedio).
- `EquipItemResult` expone la base y el valor derivado según el nivel.
- Smoke WPF: el ítem equipado basado en `item-dragon-armor` muestra DEF base y
  DEF derivado en el nivel elegido; sin cambios en la persistencia
  (`equipment` sigue siendo `{ itemId, itemVersion, level }`).

## Evolución del dataset y versiones

- El cambio de contenido en `item-dragon-armor.json` altera el hash del
  snapshot: el dataset avanza a `2026-09-20.1`.
- `ItemDefinition` de dragon armor `1.1.0`; Kris y Albatross Bow permanecen en
  `1.0.0`. Ningún fixture de build referencia `item-dragon-armor` por versión,
  así que la migración de carga no se ve afectada; `item-kris` (usado en el
  smoke equipado) conserva `1.0.0`.
- No cambian `build.schema.json` (`1.2.0`), `build-draft.schema.json` (`1.2.0`),
  el motor `0.2.0`, las trazas, las skills ni la progresión.

## Fuera de alcance

- JOL (`+5 STR` por opción), `requiredLevel`, ATK por nivel de armas,
  progresión de `requiredStats` por nivel y sockets: sin fuente e
  implementación diferida/excluida; cualquier evolución exige nueva evidencia o
  nueva decisión del propietario.
- Cualquier ítem, ranura, grado o campo ajeno al subconjunto de `RES-0003` y al
  alcance de `EVD-0053`.