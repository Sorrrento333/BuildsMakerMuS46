# Diseño técnico — skills como modificador de cálculo (sin catálogo en cliente)

## Estado y alcance

- Fecha: 2026-09-19.
- Estado: `ALIGNED` con `docs/DECISIONES-PRODUCTO.md` (regla inviolable).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-17.1` (sin cambios).
- Registro factual: `docs/05-research/registers/RES-0004-skills-buffs.md`.
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

## Dirección objetivo

- La skill entra como **multiplicador/modificador** dentro de las fórmulas
  derivadas (p. ej. un buff de daño o velocidad), nunca como entidad con UI.
- Implementarlo requiere datos de efecto autorizados por el axioma; hoy `buffRef`
  está omitido, por lo que la suma de la skill a la fórmula todavía no está
  habilitada y no se infiere ningún valor.

## Fuera de alcance

- Catálogo/UI de skills, selectores, listados o equipado de skills.
- Pruebas de materialización/lectura de snapshot de skills.
- `buffRef`, efectos de `kind BUFF`, prerrequisitos y costes AG/HP/SD.
- Cambios en `build.schema.json` o en la persistencia de builds.
