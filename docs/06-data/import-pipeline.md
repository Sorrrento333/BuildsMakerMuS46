# Pipeline de importación

1. Adquirir datos respetando términos y límites.
2. Guardar raw con metadatos, no directamente en producción.
3. Parsear a staging preservando texto original.
4. Normalizar nombres, unidades y tipos.
5. Resolver identidad sin fusionar por nombre únicamente.
6. Validar schema y reglas de dominio.
7. Comparar con snapshot anterior.
8. Revisión humana/IA asistida.
9. Publicar snapshot firmado/hash.
10. Generar reporte de cambios.

Los scrapers, si se crean, serán herramientas de desarrollo desacopladas y tolerantes a cambios; nunca requisito de ejecución del usuario final.
