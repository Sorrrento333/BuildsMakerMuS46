# Releases y versionado

Versiones independientes:
- Aplicación: SemVer.
- Ruleset: SemVer.
- Dataset: fecha + revisión/hash.
- Schema: SemVer.

Una build exportada debe incluir todas estas versiones para poder reproducirse. Las releases publican notas de cambios, migraciones, compatibilidad y hashes.

## Licencias y avisos

Toda distribución incluye `LICENSE.md`, `NOTICE` y los avisos completos exigidos
por las dependencias efectivamente redistribuidas. `THIRD-PARTY-NOTICES.md` es el
inventario de partida, no sustituye los textos legales requeridos por cada
paquete.

Antes de publicar se inspecciona el artefacto final, no sólo el grafo del
repositorio. La evaluación de Json Everything mantiene los binarios NuGet del
validador limitados al desarrollo interno y fuera de toda distribución. Una
compilación propia desde fuente MIT sólo podrá sustituirlos tras demostrar
reproducibilidad, paridad de contratos, SBOM, avisos y auditoría de
vulnerabilidades. La publicación WPF actual no referencia ese proyecto, pero el
smoke debe demostrar qué avisos acompaña realmente al artefacto.

Desde el 2026-07-19, el smoke WPF valida diez archivos legales no vacíos en la
publicación inicial y en su copia de reemplazo, y exige hashes SHA-256 idénticos
entre ambas. Los textos de los runtime packs se copian desde las versiones
exactas resueltas por NuGet/MSBuild.
