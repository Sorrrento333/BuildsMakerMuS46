# Flujo Git

- Rama principal protegida.
- Ramas cortas: `feat/`, `fix/`, `research/`, `data/`, `docs/`.
- Commits convencionales.
- PR obligatoria para código, schemas y datos publicados.
- Revisión adicional para fórmulas y migraciones.
- Tags para releases de app, ruleset y dataset.

Ejemplo: `feat(engine): add traceable stat budget calculation`.

## Estado de aplicación remota

`main` es la rama predeterminada desde el 2026-07-18 (hora local; 2026-07-19
UTC). El historial del bootstrap se conservó al renombrar
`chore/bootstrap-repository`; no hubo rebase ni reescritura.

El propietario hizo público el repositorio el 2026-07-19. A partir de esa
decisión, branch protection quedó activa y verificada en `main` con:

- PR obligatorio, también para administradores;
- checks estrictos `build-and-test` y `wpf-publication-smoke`;
- historial lineal y resolución de conversaciones;
- descarte de revisiones obsoletas, con cero aprobaciones obligatorias mientras
  el repositorio conserve un único propietario;
- force-push y borrado de `main` deshabilitados.

Los archivos de texto se normalizan a LF mediante `.gitattributes`; esta regla
mantiene reproducibles los hashes de `MANIFEST.sha256` entre plataformas.
