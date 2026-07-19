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

La API de GitHub rechazó branch protection con estado `403`: el repositorio es
privado y el plan actual requiere GitHub Pro o visibilidad pública para activar
esa función. Hasta que el propietario decida una de esas opciones, estas reglas
se aplican operativamente aunque GitHub no pueda imponerlas:

- trabajar en ramas cortas y abrir PR antes de integrar;
- exigir resultados correctos de `build-and-test` y `wpf-publication-smoke`;
- no usar force-push ni borrar `main`;
- no cambiar visibilidad ni plan sin decisión explícita del propietario.

Los archivos de texto se normalizan a LF mediante `.gitattributes`; esta regla
mantiene reproducibles los hashes de `MANIFEST.sha256` entre plataformas.
