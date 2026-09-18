# Diseño técnico — gate factual de skills y buffs

## Estado y alcance

- Fecha: 2026-09-17.
- Estado: `CLOSED` (resuelto por axioma del propietario).
- Implementación productiva: `NOT_STARTED` (el catálogo acotado de skills es la
  siguiente vertical; este diseño no autoriza código).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-16.1` (sin cambios).
- Registro factual: `docs/05-research/registers/RES-0004-skills-buffs.md`.
- Capas excluidas: Domain, Application, Data, WPF, Calculation Engine, schemas y
  dataset. Este documento no autoriza código ni datos productivos fuera del
  axioma.

Este diseño abre el gate que bloqueaba la vertical de skills, buffs y gasto
final de puntos de una build maximizada. Su única salida válida era evidencia que
demostrara Season 4 global/inglesa para un subconjunto acotado, o una decisión
del propietario que aceptara ese subconjunto como axioma. El 2026-09-17 el
propietario resolvió el gate por la segunda vía (`EVD-0045`), siguiendo el mismo
patrón que cerró el gate de ítems (`RES-0003`/`EVD-0040`).

## Antecedente

`RES-0001` y `RES-0002` cerraron clases, evoluciones, stats base y recursos
derivados. `RES-0003` cerró el gate de ítems y UC-04 consume el catálogo acotado
(`docs/04-domain/items-consumption-design.md`). El contrato `SkillDefinition`
(`packages/schemas/v1/skill.schema.json`, `1.0.0`) existe, pero el ruleset no
contiene ninguna skill factual: `skill` sólo tiene fixtures sintéticos. El gasto
final de puntos y la composición de una build maximizada dependen de datos de
skills y buffs que hoy no pueden inventarse.

## Problema que se cierra

Fanz expone la estructura necesaria (`EVD-0041`–`EVD-0044`): árboles de skills
por clase, categorías, coste de Mana, rango, requisitos por ítem/umbral y efectos
de buff. Sin embargo:

- Fanz no declara Season 4 y mezcla sistemas y clases posteriores (`DSP-0008`).
- Sus categorías (`ATK`, `Non-ATK`, `Buff`, `Debuff`, `Summon`, `WIZ`, `Curse`)
  no mapean una a uno con `kind` (`ACTIVE`, `PASSIVE`, `BUFF`, `SUMMON`)
  (`DSP-0009`).
- Su modelo de desbloqueo (ítem de skill más umbral de personaje/stat) no mapea a
  `requiredLevel`/`prerequisiteSkillIds` (`DSP-0010`).
- Algunos buffs tienen efectos incompletos (`?`) (`DSP-0011`).

Sin resolver esos cuatro puntos, cualquier `SkillDefinition` sería una invención.

## Decisión de diseño

El gate no produjo datos; produjo la determinación requerida para producir
datos. Se resolvió así:

- `RES-0004` pasó de nueve claims `PARTIAL` y cuatro conflictos `OPEN` a nueve
  claims `VERIFIED` y cuatro conflictos `RESOLVED` por `OWNER_DECISION`, sólo
  dentro del axioma acotado de `EVD-0045`.
- El axioma acepta exactamente ocho skills de las guías de Dark Knight y Fairy
  Elf cuyos requisitos publicados incluyen un `Character Level` explícito:
  Impale, Twisting Slash, Swell Life, Death Stab, Rageful Blow, Strike of
  Destruction, Penetration y Multi-Shot.
- No se añadieron JSON al ruleset, ni fixtures, ni constantes, ni formularios en
  esta vertical: la materialización del catálogo acotado es la siguiente.
- `SkillDefinition` permanece sin datos factuales fuera del axioma.

Los cuatro conflictos quedaron resueltos exactamente como exige el umbral:

1. `DSP-0008` — el propietario acotó el subconjunto estable aceptado como axioma
   (ocho skills con `Character Level`), conservando la divergencia de versión.
2. `DSP-0009` — mapeo aprobado: `ATK`/`Non-ATK`/`Debuff` → `ACTIVE`, `Buff` →
   `BUFF`, `Summon` → `SUMMON`; `PASSIVE` no se acepta.
3. `DSP-0010` — `requiredLevel` = `Character Level` publicado; los prerrequisitos
   por stat/quest/equipo no se mapean y `prerequisiteSkillIds` queda vacío.
4. `DSP-0011` — los buffs/skills con valores incompletos (`?`) quedan fuera;
   `buffRef` se omite en el subconjunto.

La segunda línea de evidencia independiente sigue pendiente y no era condición
de este cierre por decisión del propietario; se exigirá para ampliar el catálogo.

## Mapa de campos (resuelto por el axioma)

| Campo `SkillDefinition` | Origen en Fanz | Estado |
|---|---|---|
| `displayName` | Nombre de la skill en el árbol | Cubierto |
| `kind` | Categoría Fanz | Cubierto por mapeo aprobado (`EVD-0045`) |
| `requiredLevel` | `Character Level` publicado | Cubierto (nivel de personaje) |
| `prerequisiteSkillIds` | — | Vacío (decisión del axioma) |
| `allowedEvolutionIds` | Guía de familia | Cubierto: tres evoluciones de la familia |
| `buffRef` | Efectos de buff | Omitido (diferido) |
| `evidenceRefs` | `EVD-0041`–`EVD-0045` | Disponible |

Los campos requeridos por `skill.schema.json` (`id`, `version`, `rulesetId`,
`displayName`, `status`, `confidence`, `kind`, `requiredLevel`,
`allowedEvolutionIds`, `evidenceRefs`) pueden completarse de forma factual para
el subconjunto acotado y no fuera de él.

## Verificación

- `RES-0004` registra nueve claims atómicos `VERIFIED`, cuatro conflictos
  `RESOLVED` y cinco evidencias con URL, fecha, temporada declarada, dato,
  transformación, uso y licencia (`docs/05-research/source-policy.md`).
- Ninguna afirmación del registro reclasifica a Fanz como fuente Season 4: la
  clasificación proviene del axioma acotado `EVD-0045`.
- El gate no modifica ruleset, dataset, motor, schemas ni migraciones.

## Criterios de cierre

El gate se cierra con la decisión de axioma acotada y los cuatro conflictos
resueltos (`RESOLVED`, `OWNER_DECISION`). La materialización del catálogo y sus
casos reproducibles por campo pertenecen a la siguiente vertical y no pueden
salir del subconjunto ni de los campos aprobados.

## Cierre — 2026-09-17

- El propietario resolvió el gate mediante axioma acotado (`EVD-0045`).
- `RES-0004` queda `VERIFIED` para el subconjunto; `DSP-0008` a `DSP-0011` quedan
  `RESOLVED`.
- Siguiente vertical documentada: materializar las ocho `SkillDefinition`
  `PUBLISHED` contra `skill.schema.json`, con el catálogo y su validación.
- Pendiente técnico independiente: contrato de buff (`buffRef`) para modelar
  efectos de buff.
- Materialización completada: las ocho `SkillDefinition` viven en
  `packages/rulesets/mu-s4-global-reference/v1/skills/`, el validador registra
  `("skill","skills")` y el dataset avanza a `2026-09-17.1`.