# Principios y reglas permanentes

## Investigación
- Consultar múltiples fuentes antes de afirmar un dato técnico.
- Usar `muonlinefanz.com` como fuente inicial prioritaria de extracción para
  nueva información factual del juego.
- Desde la decisión del propietario del 2026-07-19, se permite consultar,
  extraer y contrastar Webzen y fuentes adicionales. Cada hallazgo debe conservar
  provenance, versión, confianza y conflictos propios; ninguna fuente queda
  autorizada automáticamente como dato publicable de Season 4.
- Ninguna página de MU Online Fanz se atribuye automáticamente a Season 4: se
  debe registrar la versión que demuestra y cualquier límite de aplicación.
- La matriz cerrada en `RES-0001` es una excepción explícita: el propietario la
  aprobó como axioma estable del ruleset el 2026-07-19. Su provenance se conserva,
  pero la búsqueda histórica dejó de ser un gate para implementarla.
- Registrar URL, fecha, extracto, interpretación y conflicto.
- Marcar como `UNVERIFIED`, `PARTIAL`, `VERIFIED` o `DISPUTED`.

## Ingeniería
- Separar dominio, motor, persistencia, API y UI.
- Preferir funciones puras en cálculos.
- No introducir números mágicos.
- Toda fórmula debe tener ID, versión, fuente, redondeo y pruebas.
- Los cambios de arquitectura requieren ADR.

## Producto
- Toda cifra visible debe poder desglosarse.
- La UI debe mostrar el perfil y versión activos.
- El usuario debe distinguir datos estándar de personalizados.
- Ninguna sugerencia del asistente se convierte en decisión sin aprobación del propietario.

## Continuidad
- Actualizar `current-status.md`, `next-actions.md` y `CHANGELOG.md` al cerrar trabajo significativo.
- Nunca dejar una decisión crítica solo en conversaciones.
