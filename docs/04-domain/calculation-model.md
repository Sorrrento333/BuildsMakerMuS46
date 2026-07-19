# Modelo de cálculo

## Pipeline
1. Normalizar entradas.
2. Validar build y escenario.
3. Resolver reglas activas.
4. Calcular progresión y presupuesto.
5. Generar modificadores base/equipo/buffs.
6. Construir grafo de fórmulas.
7. Evaluar en orden topológico.
8. Aplicar redondeos en puntos documentados.
9. Emitir valores, advertencias y traza.

## Tipos de modificador
- `flat_before`: suma previa.
- `percent_additive`: porcentajes agrupados.
- `percent_multiplicative`: multiplicadores secuenciales.
- `flat_after`: suma posterior.
- `override`: reemplazo condicionado.
- `cap/floor`: límite superior o inferior.
- `chance`: efecto probabilístico.

El orden exacto no se supone globalmente: cada atributo o fórmula declara su pipeline.
