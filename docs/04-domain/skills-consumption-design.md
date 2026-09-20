# Diseño técnico — skills como modificador de cálculo (sin catálogo en cliente)

## Estado y alcance

- Fecha: 2026-09-19.
- Estado: `IMPLEMENTED` (modificador materializado; alineado con
  `docs/DECISIONES-PRODUCTO.md`, regla inviolable).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-17.1` (sin cambios).
- Registro factual: `docs/05-research/registers/RES-0004-skills-buffs.md`
  (`SKL-CLM-010`, axioma `EVD-0046`).
- Gate de origen: `docs/04-domain/skills-factual-gate-design.md` (`CLOSED`, `EVD-0045`).

Regla de producto vigente: una skill **sólo** interesa como modificador de
cálculo (dmg / buff) dentro de las fórmulas derivadas. No se expone como entidad:
no hay catálogo ni selector en el cliente, ni pruebas del catálogo.

Este documento reemplaza al diseño previo de "consumo acotado del catálogo de
skills". Se conserva únicamente el backend mínimo que pudiera alimentar el
modificador; se retiró la UI y su verificación por contradecir la regla.

## Qué se conserva (backend mínimo)

- `Domain.Skills.SkillDefinition` + `SkillDefinitionStatus` / `SkillKind`.
- `Application.Skills`: `SkillCatalog`, `ISkillCatalogSnapshotReader`,
  `JsonSkillCatalogSnapshotReader`, `LearnSkillUseCase` y sus excepciones, como
  base reutilizable y sin consumo desde la interfaz.
- Datos del ruleset `v1/skills/*.json` (ocho `SkillDefinition` `PUBLISHED` del
  axioma `EVD-0045`) y su registro en `SchemaContractValidator`.

Estos tipos permanecen compilados pero **no** se referencian desde la app (sin
propiedad `SkillCatalog` en `PublishedProgressionRuleset`, sin smoke, sin UI).

## Qué se retiró (2026-09-19)

- Sección "Aprendizaje de skills" (`ComboBox`/`SkillCatalog`) de `MainWindow`.
- `SkillApplicationIntegrationTests.cs` (pruebas del lector del catálogo).
- La verificación de catálogo de skills en el smoke de publicación
  (`SkillCatalogVerified`, `SkillCatalogSkillCount`, `SkillCatalogSkillReferences`,
  `SyntheticSkillLearnVerified`) y las aserciones del script PS1.
- `reference-cases/skills/{valid,invalid}` (fixtures exclusivos de esas pruebas).

## Dirección objetivo — materializada (2026-09-19)

- La skill entra como **modificador de cálculo** dentro de las fórmulas derivadas
  (`schemaVersion` `2.1.0`), nunca como entidad con UI. El propietario autorizó
  los valores de efecto como axioma acotado (`EVD-0046`, `SKL-CLM-010`).
- Siete fórmulas `PUBLISHED` `VERIFIED` existen en
  `packages/rulesets/mu-s4-global-reference/v1/formulas/`:
  `formula-skill-damage-impale-dark-knight` (`15 + trunc(STR/35)`),
  `formula-skill-damage-twisting-slash-dark-knight` (`15 + trunc(STR/40)`),
  `formula-skill-damage-death-stab-dark-knight` (`70 + trunc(STR/150)`),
  `formula-skill-damage-rageful-blow-dark-knight` (`60 + trunc(STR/150)`),
  `formula-skill-hp-buff-swell-life-dark-knight`
  (`12 + trunc(ENE/20) + trunc(VIT/100)`, un único truncamiento del total),
  `formula-skill-damage-penetration-fairy-elf` (`70 + trunc(AGI/200)`) y
  `formula-skill-damage-multi-shot-fairy-elf` (`40 + trunc(AGI/200)`).
- Cada fórmula tiene cuatro casos válidos reproducibles y controles negativos en
  `reference-cases/formulas/{valid,invalid}`; el resultado se publica en el panel
  "Atributo derivado publicado", sin UI de skills.
- Strike of Destruction queda fuera (Fanz no publica Skill DMG). `buffRef` sigue
  omitido: ningún otro efecto de skill está habilitado.
- Verificación en `docs/10-ai-handoff/current-status.md`.

## Fuera de alcance

- Catálogo/UI de skills, selectores, listados o equipado de skills.
- Pruebas de materialización/lectura de snapshot de skills.
- `buffRef`, y cualquier efecto numérico distinto de los siete materializados por
  `EVD-0046`; prerrequisitos y costes AG/HP/SD.
- Cambios en `build.schema.json` o en la persistencia de builds.
