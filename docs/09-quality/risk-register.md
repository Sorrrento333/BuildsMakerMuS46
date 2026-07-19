# Registro inicial de riesgos

| Riesgo | Impacto | Probabilidad | Mitigación |
|---|---|---|---|
| Mezclar temporadas | Alto | Alto | Versionado y evidencia por registro |
| Fórmulas contradictorias | Alto | Alto | Disputes y pruebas reproducibles |
| Scraping frágil | Medio | Alto | Importadores desacoplados y snapshots |
| Alcance excesivo | Alto | Alto | Hitos y MVP vertical |
| Opciones con orden desconocido | Alto | Medio | Trazas y estado PARTIAL |
| Datos con derechos inciertos | Alto | Medio | Revisión de licencias y atribución |
| Avisos de terceros incompletos en una distribución | Alto | Medio | Inventario versionado, inspección del artefacto y prueba de `LICENSE`/`NOTICE` antes de release |
| Binarios NuGet de Json Everything no aprobados para distribución | Alto | Medio | Mantener el validador fuera de WPF; el spike fuente MIT ya pasó y debe integrarse para retirar los binarios publicados del grafo normal |
| Arquitectura sobredimensionada | Medio | Medio | Monolito modular |
| Recomendaciones engañosas | Alto | Medio | Explicar objetivo, restricciones y confianza |
