# Diseño técnico — consumo acotado del catálogo de skills (aprendizaje)

## Estado y alcance

- Fecha: 2026-09-19.
- Estado: `IMPLEMENTED` (Application + WPF + smoke).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-17.1` (sin cambios respecto al catálogo de skills materializado).
- Registro factual: `docs/05-research/registers/RES-0004-skills-buffs.md`.
- Gate de origen: `docs/04-domain/skills-factual-gate-design.md` (`CLOSED`, `EVD-0045`).

Este documento describe la vertical que **consume** el catálogo acotado de
skills. No añade datos factuales: opera exclusivamente sobre los campos del
axioma (evolución y nivel). El `buffRef` del `kind BUFF` queda **diferido**: aquí
sólo se acota la elegibilidad de aprendizaje.

## Problema

El motor ya cataloga 8 `SkillDefinition` publicados, pero la Calculadora no los
consume: `UC-04` ("el usuario elige...") y el aprendizaje de skills requieren
que, dado el estado validado de una build (evolución y nivel final), se resuelva
si el personaje puede aprender una skill publicada.

La porción verificable y sin invención es la **elegibilidad de aprendizaje**:
evolución en `allowedEvolutionIds` y nivel final `>= requiredLevel`. Todo lo
demás de la skill (buff references, prerrequisitos, costes AG/HP) queda fuera del
axioma y fuera de alcance.

## Contrato de dominio

- `Domain.Skills.SkillDefinition` (nuevo): `Id`, `Version`, `RulesetId`,
  `DisplayName`, `Status` (`SkillDefinitionStatus`), `Kind` (`SkillKind`),
  `RequiredLevel`, `AllowedEvolutionIds`.
- `SkillKind`: `ACTIVE`, `PASSIVE`, `BUFF`, `SUMMON`.
- Sólo se materializan los campos del axioma; el resto del JSON (p. ej.
  `buffRef`, `prerequisiteSkillIds`) se mantiene en el ruleset pero no se
  interpreta en esta vertical.

## Contrato de Application

- `SkillCatalog` + `ISkillCatalogSnapshotReader` +
  `JsonSkillCatalogSnapshotReader`: leen `<snapshot>/skills/*.json`, exigen
  schema `1.0.0`, IDs únicos, un único `rulesetId`, `requiredLevel >= 1`,
  `allowedEvolutionIds` no vacíos y sin duplicados, y `status` `PUBLISHED`
  (fail-closed con `SkillCatalogSnapshotErrorCodes`). Un directorio vacío es
  `SnapshotInvalid`.
- `LearnSkillUseCase`: recibe `LearnSkillRequest(EvolutionId, FinalLevel,
  SkillId)` y devuelve `LearnSkillResult`. Valida, con códigos estables:
  - `skill-learn-skill-not-found`: la skill no resuelve exactamente una
    definición publicada.
  - `skill-learn-evolution-not-allowed`: `allowedEvolutionIds` no contiene la
    evolución.
  - `skill-learn-requirements-not-met`: el nivel final es inferior a
    `requiredLevel`.
- `FinalLevel` es el nivel ya validado por el presupuesto de progresión; la
  vertical no calcula ni aplica reducciones de nivel.

## Invariantes

1. Ningún requisito se compara con una evolución distinta de la seleccionada ni
   con un nivel interpolado: sólo `allowedEvolutionIds` y `requiredLevel`.
2. El `buffRef` de `kind BUFF` no se interpreta: se declara diferido y el
   resultado es informativo de la elegibilidad de aprendizaje.
3. No se gestionan prerrequisitos entre skills, rangos de `kind` ni costes de
   AG/HP/SD.
4. No se emite ninguna bonificación o efecto: el resultado declara su ausencia.

## WPF

- La Calculadora añade una sección "Aprendizaje de skills": selector de skill
  limitado a las publicadas para la evolución seleccionada y resultado de
  elegibilidad. La evaluación se rehace al cambiar evolución, skill o nivel; si
  el nivel no es un entero, pide un nivel válido.
- El smoke WPF verifica que el catálogo acotado materializa las 8 skills
  aprobadas y que una evaluación de aprendizaje acotada resuelve
  `skill-impale` para `evolution-dark-knight` con nivel 28.

## Fuera de alcance

- `buffRef` y efectos de `kind BUFF`, prerrequisitos, reducciones de nivel y
  costes de recursos (AG/HP/SD).
- Cualquier otra skill, evolución o campo no aprobado por `EVD-0045`.
- Cambios en `build.schema.json` o en la persistencia de builds.