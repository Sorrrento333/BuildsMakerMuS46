# Roadmap maestro

## Estrategia
El proyecto avanza por capas verticales verificables. Ninguna fase depende de tener toda la enciclopedia completa; se prioriza un núcleo de cálculo correcto y extensible.

## Fase 0 — Fundación y gobierno
**Entregables:** documentación base, convenciones, ADR inicial, esquemas de evidencia, CI mínimo.  
**Salida:** cualquier colaborador puede instalar, entender y proponer cambios sin conversación previa.

## Fase 1 — Investigación técnica Season 4
**Entregables:** matriz de personajes, stats base, evolución, puntos, Marlon, resets, catálogo preliminar de fórmulas y conflictos.  
**Salida:** cada dato requerido por el MVP tiene al menos dos evidencias o una prueba reproducible y estado explícito.

## Fase 2 — Modelo de dominio y contratos
**Entregables:** entidades, JSON Schemas, repositorios, IDs estables, política de versiones y migraciones.  
**Salida:** fixtures válidos e inválidos pasan validadores.

## Fase 3 — MVP del motor de puntos
**Entregables:** clase, nivel, quest, reset, distribución, validación y traza.  
**Salida:** casos de referencia coinciden con resultados manuales aprobados.

## Fase 4 — Motor de atributos derivados
**Entregables:** HP, Mana, AG, SD, daño, defensa, rates y velocidad por clase según evidencia disponible.  
**Salida:** fórmulas aisladas, pruebas unitarias y trazas completas.

## Fase 5 — Enciclopedia navegable
**Entregables:** búsqueda, fichas, fuentes, filtros por versión y estado.  
**Salida:** consulta offline con provenance visible.

## Fase 6 — Equipamiento
**Entregables:** ranuras, requisitos, mejoras, opciones y agregación de modificadores.  
**Salida:** builds equipadas válidas y explicación de incompatibilidades.

## Fase 7 — Escenarios PvM/PvP
**Entregables:** contexto de objetivo, orden de cálculo, probabilidades y efectos.  
**Salida:** simulaciones deterministas y, donde aplique, distribuciones probabilísticas.

## Fase 8 — Comparador y optimizador asistido
**Entregables:** comparación A/B, breakpoints, diferencias y recomendaciones explicadas.  
**Salida:** ninguna recomendación sin mostrar objetivo, restricciones y evidencia.

## Fase 9 — Perfiles de servidor
**Entregables:** editor, herencia, validación, importación y exportación firmada opcionalmente.  
**Salida:** perfil personalizado sin cambios de código.

## Fase 10 — Estabilización y publicación
**Entregables:** QA, documentación de usuario, instaladores, telemetría opcional con consentimiento, estrategia de actualizaciones.  
**Salida:** release candidata reproducible.
