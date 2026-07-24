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

La Calculadora cubre presupuesto ganado y distribución de stats. Clase,
evolución, nivel y Hero Status producen un
`ProgressionPointBudgetResult` con total y traza por regla. La pantalla conserva
ese resultado; no recalcula progresión al distribuir.

Los controles de asignación se crean exclusivamente desde los `StatIds` de la
definición seleccionada en el catálogo. No existen listas de nombres, límites o
valores de clase en XAML/C#. La salida muestra puntos gastados, puntos restantes
y las asignaciones por ID. Los fallos de distribución presentan una explicación
en español y conservan visible el código estable original.

Cambiar clase, evolución, nivel o Hero Status invalida el presupuesto anterior
y obliga a calcularlo de nuevo antes de distribuir, evitando combinar entradas
actuales con un origen obsoleto. Persistencia, resets, equipo y atributos
derivados permanecen fuera de esta vertical.
