# Diseño técnico — instancia equipada sin bonificaciones (vertical 1.2.0)

## Estado y alcance

- Fecha: 2026-09-19.
- Estado: `IMPLEMENTED` (Application + WPF + smoke + schemas).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-17.1`.
- Registro factual: `docs/05-research/registers/RES-0003-items-equipment.md`.
- Axioma del propietario: `EVD-0040` (3 ítems publicados, `maxItemLevel` 15,
  `optionModules` solo `NORMAL`, `socketSlots` 0, `requiredStats` +0).
- Consumo previo: `docs/04-domain/items-consumption-design.md` (UC-04,
  elegibilidad de equipado).

Este documento describe una instancia equipada persistible y revalidable que
reutiliza la elegibilidad ya validada por UC-04 **sin añadir datos factuales**.

## Problema

`build.schema.json`/`build-draft.schema.json` `1.1.0` no representan una
instancia equipada. El usuario necesita guardar el ítem elegido junto a su
build/draft para que sobreviva al cierre de la Calculadora y a la revalidación
contra el snapshot publicado.

El axioma no define mecánica de progresión de ítem (nivel que altera
`requiredStats`, bonificaciones u opciones). La porción verificable es una
instancia acotada: `{ itemId, itemVersion, level }`, donde el nivel es un dato
declarado (0..`maxItemLevel`) y los requisitos se evalúan siempre contra los
valores publicados en nivel +0 (`requiredStats`), igual que en UC-04.

## Instancia equipada

- Se guarda **sin** ranura, opciones ni sockets: la ranura y los atributos se
  derivan de la definición publicada del ítem.
- `BuildEquipmentEntry` = `record { itemId, itemVersion, level }`:
  - `itemId`: identifica exactamente una definición publicada.
  - `itemVersion`: debe coincidir con `ItemDefinition.Version` publicado.
  - `level`: entero en `[0, MaxItemLevel]`.
- Permisos de equipado idénticos a UC-04: la clase debe estar en
  `allowedClassIds` y los `requiredStats` (nivel +0) deben ser alcanzados por
  los **stats finales** de la build/draft.
- No se emite ninguna bonificación ni se usa la progresión de ítem.

## Contrato de Application

- `BuildEquipmentEntry` (nuevo): serializado como `equipment` en
  `CharacterBuild` (`1.2.0`) y `BuildDraft` (`1.2.0`). El documento del draft
  mantiene `statDistribution` en `1.1.0` (no cambia su contrato).
- `BuildEquipmentValidator` (interno): valida contra `ItemCatalog`, con códigos
  estables espejo en `BuildErrorCodes` (`build-equipment-*`) y
  `BuildDraftErrorCodes` (`build-draft-equipment-*`):
  - `item-not-found`, `version-mismatch`, `class-not-allowed`,
    `level-out-of-range`, `duplicate`, `requirements-not-met`.
- `SaveBuildDraftRequest` gana `Equipment` (opcional); `SaveBuildDraftUseCase`
  valida siempre que haya equipo y persiste la lista.
- `SaveBuildUseCase` promueve `draft.Equipment` al `CharacterBuild`.
- `LoadBuildDraftUseCase` y `LoadBuildUseCase` **revalidan** el equipo contra el
  catálogo publicado tras recargar; un cambio de snapshot rechaza el documento
  y protege `Equipment ?? []` tras la deserialización.
- `EquipItemRequest` gana `Level` (0 por defecto) y `EquipItemResult` expone
  `Level` y `MaxItemLevel`; `EquipItemUseCase` rechaza
  `0 <= Level <= item.MaxItemLevel` (`item-equip-level-out-of-range`).

## Evolución de versiones

- `CharacterBuild.current` `1.1.0` → `1.2.0` (`PreviousSchemaVersion` `1.1.0`).
- `BuildDraft.current` `1.1.0` → `1.2.0` (`PreviousSchemaVersion` `1.1.0`);
  `BuildDraftStatDistribution` permanece en `1.1.0`.
- Normalización al cargar (`LoadBuildDraftUseCase`):
  - actual+actual → as-is.
  - draft `1.1.0` + stat `1.1.0` → `1.2.0` con `Equipment = []`
    (preserva `resetInputs`).
  - draft `1.0.0` + stat `1.0.0` → `1.2.0` con defaults de reset y
    `Equipment = []` (migración anterior de `resetInputs`).
  - resta: `build-draft-schema-unsupported`.
- Normalización al cargar (`LoadBuildUseCase`): `1.1.0` → `1.2.0` con
  `Equipment = []`; el resto rechaza con `build-schema-unsupported`.

## Persistencia

- Los repos SQLite guardan `payload_json` completo; no hay migración de tabla.
  `schema_version` simplemente almacena `1.2.0` en los documentos nuevos.
- Tras la deserialización, `Equipment ?? []` protege los documentos previos.

## WPF

- La Calculadora añade la sección central «Equipo» (fila 12 del `Grid`):
  - Nivel del ítem + «Equipar seleccionado» (usa la elección y validación de la
    sección de elegibilidad) y «Desequipar seleccionado».
  - Lista `EquippedItemsListBox` con plantilla
    `ItemId · vItemVersion · nivel Level` (sin `DisplayName`).
  - `_equippedItems` se limpia al cambiar clase y se rellena al final de
    `ApplyLoadedDraft`/`ApplyLoadedBuild` (después de seleccionar clase).
- El guardado del draft pasa `_equippedItems.ToArray()`.
- Si la elegibilidad falla, el botón «Equipar seleccionado» comunica el código
  estable de error.

## Smoke WPF

- Segundo household equipado: clase `class-dark-knight`,
  `evolution-dark-knight`, nivel 7 (EB 30), `agility` 7 (27 final), resets 0.
- Elige `item-kris` a nivel 15 (requiere fuerza 27/agilidad 27; base 28/20 +0).
- `publication-smoke-equip-draft` / `publication-smoke-equip-build` sobreviven a
  la sustitución binaria y a la revalidación; `PersistedBuildCount` pasa a 2.

## Fuera de alcance

- `requiredLevel`, progresión de `requiredStats` por nivel de ítem, sockets,
  opciones, bonificaciones y exclusividades (`conflictIds`).
- Master buy / mecanismos de compra (sin definición de mecánica en el repo).
- Cualquier campo de ítem o clase ajena al axioma `EVD-0040`.