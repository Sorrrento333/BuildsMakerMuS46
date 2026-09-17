# Diseño técnico — consumo acotado del catálogo de ítems (UC-04)

## Estado y alcance

- Fecha: 2026-09-16.
- Estado: `IMPLEMENTED` (Application + WPF + smoke).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-16.1` (sin cambios respecto al catálogo materializado).
- Registro factual: `docs/05-research/registers/RES-0003-items-equipment.md`.
- Gate de origen: `docs/04-domain/items-factual-gate-design.md` (`CLOSED`).

Este documento describe la primera vertical que **consume** el catálogo acotado
de ítems. No añade datos factuales: opera exclusivamente sobre los campos del
axioma del propietario (`EVD-0040`).

## Problema

`UC-04` ("el usuario elige ranura, ítem, nivel y opciones; el sistema valida
clase, requisitos, exclusiones y bonificaciones") depende de datos que el axioma
no cubre: valores de bonificación (ATK/DEF), progresión de `requiredStats` por
nivel de ítem y opciones/sockets. `build.schema.json` `1.1.0` tampoco representa
una instancia equipada con nivel u opciones.

La porción verificable y sin invención es la **elegibilidad de equipado**: dado
el estado validado de una build (clase y stats finales), resolver si un
`ItemDefinition` publicado puede equiparse.

## Contrato de dominio

- `Domain.Items.ItemDefinition` (nuevo): `Id`, `Version`, `RulesetId`,
  `DisplayName`, `Status` (`ItemDefinitionStatus`), `Slots`,
  `AllowedClassIds`, `RequiredStats`.
- Sólo se materializan los campos del axioma; el resto del JSON se mantiene en
  el ruleset pero no se interpreta en esta vertical.

## Contrato de Application

- `ItemCatalog` + `IItemCatalogSnapshotReader` +
  `JsonItemCatalogSnapshotReader`: leen `<snapshot>/items/*.json`, exigen
  schema `1.0.0`, IDs únicos, un único `rulesetId`, `slots` y
  `allowedClassIds` no vacíos, `requiredStats` no negativos y `status`
  `PUBLISHED` (fail-closed con `ItemCatalogSnapshotErrorCodes`).
- `EquipItemUseCase`: recibe `EquipItemRequest(CharacterClassId, FinalStats,
  ItemId)` y devuelve `EquipItemResult`. Valida, con códigos estables:
  - `item-equip-not-found`: el ítem no resuelve exactamente una definición.
  - `item-equip-class-not-allowed`: `allowedClassIds` no contiene la clase.
  - `item-equip-requirements-not-met`: algún `requiredStats` supera el stat
    final (se listan las claves incumplidas).
- `FinalStats` es el mapa `stat final = base + asignación` ya validado por
  `CalculateStatDistributionUseCase`; la vertical no recalcula stats.

## Invariantes

1. Ningún requisito se compara con `base` ni con `asignación` aislados: sólo con
   el stat final validado.
2. `requiredLevel`, la progresión por nivel de ítem, las opciones y los sockets
   permanecen fuera de alcance; no se infieren.
3. Las ranuras son sólo un criterio de selección en la UI; no se modela una
   instancia equipada ni conflictos de exclusividad (`conflictIds`).
4. No se emite ninguna bonificación (ATK/DEF u otra): el resultado es
   informativo y declara su ausencia.

## WPF

- La Calculadora añade una sección "Elegibilidad de equipado": selector de
  ranura (derivado de los ítems permitidos para la clase), selector de ítem y
  resultado. La evaluación se rehace al cambiar clase, ranura o ítem y tras
  recalcular la distribución; si no hay distribución validada, pide
  distribuir los puntos.
- El smoke WPF verifica que el catálogo acotado materializa los tres ítems
  aprobados y que una evaluación de equipado acotada resuelve `item-kris`.

## Fuera de alcance

- `requiredLevel`, progresión de `requiredStats`, sockets, opciones y
  bonificaciones.
- Cualquier otro ítem, ranura, grado o campo no aprobado por `EVD-0040`.
- Cambios en `build.schema.json` o en la persistencia de builds.
