# Decisiones de producto — MU Online Build Planner (Season 4, multiclase)

> Regla inviolable. Conservar tal cual. No obligar al usuario a volver a explicarlo.

## Skills: SOLO como modificador de cálculo, NADA MÁS

El usuario NO quiere:

- Catálogo/UI de skills en el cliente (ni selector, ni listado, ni equipar skills).
- Tests propios de "catálogo de skills" (materialización/lectura de snapshot de skills).
- Nada que haga "ruido" visual ni funcional alrededor de skills como entidad.

Lo ÚNICO que le interesa de una skill es **cómo modifica daño (dmg) y buff**
dentro del cálculo de atributos derivados (dmg, dmg rate, def, def rate, speed…).

Cómo se aplica:

- La skill entra solo como **modificador/multiplicador** dentro de las fórmulas
  derivadas (p. ej. un buff de daño o velocidad), nunca como entidad con UI.
- Prohibido: `SkillCatalog` en la interfaz gráfica, `ComboBox` de skills,
  páginas/información de catálogo de skills, tests de integración del lector
  de catálogo de skills (ya se eliminó `SkillApplicationIntegrationTests.cs`).

Estado vigente (sesión 2026): la solución compila con 0 errores y el panel
"Atributo derivado publicado" a todo lo ancho es donde se publica el resultado
(dmg / dmg rate / def / def rate / speed). Ahí, si se quiere, se suma la skill
como modificador de la fórmula.

## Referencia verificada

- Repo: `F:\Proyecto Builds\Programa`
- El "ruleset" real que compila y se lee: `mu-s4-global-reference` bajo
  `packages/rulesets/...`. Antes se intentó `mu-s4-global-reference`
  (variante) y hay que usar el que existe en el snapshot canónico.
- El test unitario de ítems que sirve de patrón sano:
  `tests/MuOnline.BuildPlanner.Application.IntegrationTests/...Item*.cs`

## Convención de sesión (importante)

El transcript de esta herramienta DEGRADA con:
- `read`/`write` grandes (>2000-3000 caracteres de archivos).
- Salidas de `dotnet build`/`test` muy verbosas.

Por eso se trabaja en tramos pequeños, con `dotnet build` como única fuente de
verdad y `git checkout` para restaurar archivos si quedan corruptos tras un
write grande. No reescribir archivos >2000 bytes de una vez en sesiones con
degradación; usar tramos o dejar para sesión limpia.
