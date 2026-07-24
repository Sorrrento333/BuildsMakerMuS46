# Arquitectura de información de UI

## Navegación principal
- Inicio.
- Calculadora.
- Builds.
- Comparador.
- Enciclopedia.
- Perfiles de servidor.
- Investigación/diagnóstico, solo modo avanzado.
- Configuración.

## Calculadora
Panel izquierdo: identidad, nivel, quests, resets y stats.  
Centro: equipo y buffs.  
Panel derecho: resultados agrupados y puntos restantes.  
Detalle expandible: traza, fórmula, versión y fuentes.

## Reglas UX
- Perfil activo siempre visible.
- Advertencias no bloqueantes diferenciadas de errores.
- Inputs con incrementos 1/10/100 y entrada directa.
- Deshacer/rehacer.
- Compartir mediante exportación reproducible.

## Vertical disponible

La primera pantalla funcional limita la Calculadora al presupuesto ganado:
clase, evolución, nivel y Hero Status, con total y traza por regla. Las opciones
y la elegibilidad proceden del snapshot de sólo lectura publicado junto al
binario. Resets, puntos gastados, distribución de stats, equipo y atributos
derivados permanecen fuera de esta vertical.
