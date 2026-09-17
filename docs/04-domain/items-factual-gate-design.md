# Diseño técnico — gate factual de ítems (master buys)

## Estado y alcance

- Fecha: 2026-09-16.
- Estado: `CLOSED` (resuelto por axioma del propietario).
- Implementación productiva: `NOT_STARTED` (el catálogo acotado es la siguiente
  vertical; este diseño no autoriza código).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-07-30.3` (sin cambios).
- Registro factual: `docs/05-research/registers/RES-0003-items-equipment.md`.
- Capas excluidas: Domain, Application, Data, WPF, Calculation Engine, schemas y
  dataset. Este documento no autoriza código ni datos productivos fuera del
  axioma.

Este diseño abre el gate que bloqueaba la vertical de master buys. Su única
salida válida era evidencia que demostrara Season 4 global/inglesa para un
subconjunto acotado de ítems, o una decisión del propietario que aceptara ese
subconjunto como axioma. El 2026-09-16 el propietario resolvió el gate por la
segunda vía (`EVD-0040`).

## Antecedente

`RES-0001` y `RES-0002` cerraron clases, evoluciones, stats base y recursos
derivados. El catálogo canónico de `ItemDefinition`
(`packages/schemas/v1/item.schema.json`, `1.0.0`) existe como contrato, pero el
ruleset no contiene ningún ítem factual: `item` sólo tiene fixtures sintéticos.
Las pantallas restantes y el gasto final de una build maximizada dependen de
datos de ítems que hoy no pueden inventarse.

## Problema que se cierra

Fanz expone la estructura necesaria (`EVD-0035`–`EVD-0039`): ranuras, grados,
opciones, elegibilidad de clase y requisitos STR/AGI en nivel +0. Sin embargo:

- Fanz no declara Season 4 y su catálogo/guía mezclan sistemas y clases
  posteriores (`DSP-0005`).
- Los códigos de clase de las páginas no mapean uno a uno con las seis familias
  del ruleset (`DSP-0006`).
- Los requisitos se publican en +0, sin progresión por nivel de ítem ni por
  opciones (`DSP-0007`).

Sin resolver esos tres puntos, cualquier `ItemDefinition` sería una invención.

## Decisión de diseño

El gate no produjo datos; produjo la determinación requerida para producir
datos. Se resolvió así:

- `RES-0003` pasó de ocho claims `PARTIAL` y tres conflictos abiertos a ocho
  claims `VERIFIED` y tres conflictos `RESOLVED` por `OWNER_DECISION`, sólo
  dentro del axioma acotado de `EVD-0040`.
- El axioma acepta exactamente tres ítems de grado normal (Kris, Dragon Armor,
  Albatross Bow) con los campos `displayName`, `slots`, `allowedClassIds`,
  `requiredStats` en +0, `maxItemLevel` 15, `optionModules` NORMAL y
  `socketSlots` 0, más el mapeo clase-familia/evolución.
- No se añadieron JSON al ruleset, ni fixtures, ni constantes, ni formularios en
  esta vertical: la materialización del catálogo acotado es la siguiente.
- `ItemDefinition` permanece sin datos factuales fuera del axioma.

Los tres conflictos quedaron resueltos exactamente como exige el umbral:

1. `DSP-0005` — el propietario acotó el subconjunto estable aceptado como axioma
   (tres ítems de grado normal), con el límite explícito de los campos cubiertos;
   la divergencia de versión se conserva.
2. `DSP-0006` — mapeo aprobado: `All Classes` → las seis familias, `DK` →
   `class-dark-knight`, `MG` → `class-magic-gladiator`, `ME` →
   `class-fairy-elf`.
3. `DSP-0007` — `requiredStats` se registra sólo en +0; la progresión por nivel
   u opciones queda fuera del alcance.

La segunda línea de evidencia independiente sigue pendiente y no era condición
de este cierre por decisión del propietario; se exigirá para ampliar el catálogo.

## Mapa de campos (resuelto por el axioma)

| Campo `ItemDefinition` | Origen en Fanz | Estado |
|---|---|---|
| `displayName` | Nombre de la página del ítem | Cubierto (axioma) |
| `slots` | Categoría/ranura de la guía de combate | Cubierto (axioma) |
| `allowedClassIds` | `Can be equipped by ...` | Cubierto por mapeo aprobado |
| `requiredStats` | `Requirements` STR/AGI en +0 | Cubierto en +0 |
| `requiredLevel` | No publicado en las páginas de muestra | Fuera del axioma |
| `maxItemLevel` | Selector +0..+15 | Cubierto (axioma) |
| `optionModules` | Grado normal | Cubierto: NORMAL |
| `socketSlots` | Grado normal sin sockets | Cubierto: 0 |
| `evidenceRefs` | `EVD-0037`–`EVD-0040` | Disponible |

## Verificación

- `RES-0003` registra ocho claims atómicos `VERIFIED`, tres conflictos
  `RESOLVED` y seis evidencias con URL, fecha, temporada declarada, dato,
  transformación, uso y licencia (`docs/05-research/source-policy.md`).
- Ninguna afirmación del registro reclasifica a Fanz como fuente Season 4: la
  clasificación proviene del axioma acotado `EVD-0040`.
- El gate no modifica ruleset, dataset, motor, schemas ni migraciones.

## Criterios de cierre

El gate se cierra con la decisión de axioma acotada y los tres conflictos
resueltos (`RESOLVED`, `OWNER_DECISION`). La materialización del catálogo y sus
casos reproducibles por campo pertenecen a la siguiente vertical y no pueden
salir del subconjunto ni de los campos aprobados.

## Cierre — 2026-09-16

- El propietario resolvió el gate mediante axioma acotado (`EVD-0040`).
- `RES-0003` queda `VERIFIED` para el subconjunto; `DSP-0005` a `DSP-0007`
  quedan `RESOLVED`.
- Siguiente vertical documentada: materializar los tres `ItemDefinition`
  `PUBLISHED` contra `item.schema.json`, con el catálogo y su validación.
- Materialización completada: los tres registros viven en
  `packages/rulesets/mu-s4-global-reference/v1/items/`, el validador registra
  `("item","items")` y el dataset avanza a `2026-09-16.1`. Application todavía
  no consume el catálogo.
