# Estrategia de pruebas

## Pirámide
- Unitarias: fórmulas, redondeos, invariantes.
- Contract: schemas, API, import/export.
- Integración: ruleset + perfil + motor + persistencia.
- End-to-end: flujos críticos de calculadora.
- Regresión: fixtures aprobados por clase y escenario.
- Property-based: límites, monotonicidad cuando corresponda, serialización.
- Golden tests: trazas y resultados de referencia, revisados conscientemente.

## Regla
Una fórmula no entra al ruleset publicado sin casos normales, bordes, valores inválidos y al menos un caso de referencia verificable.

Los casos numéricos asociados a claims `PARTIAL` se etiquetan como pruebas de
investigación. Pueden verificar que una transformación fue transcrita de forma
coherente —por ejemplo los bordes 1, 220 y 221 de Hero Status en `RES-0001`—,
pero no se convierten en golden tests ni fixtures del ruleset hasta que la
evidencia sea promovida y el conflicto aplicable esté cerrado.

Los casos de `RES-0001` autorizados por EVD-0021 ya son fixtures factuales del
ruleset. El tooling debe conservar separados los casos positivos enlazables
desde `testCaseRefs` y los controles negativos que prueban elegibilidad; estos
últimos nunca se publican como referencias de una regla.

Para una regla de progresión `PUBLISHED`, el gate del repositorio exige que
cada referencia resuelva a un caso positivo del mismo `rulesetId` y
`progressionRuleId`, y que todos los casos positivos declarados para esa regla
estén enlazados. La prueba de contrato fija además la asignación exacta de cinco
casos estándar y dos casos de Magic Gladiator/Dark Lord.

La suite productiva del motor vuelve a cargar esos registros canónicos y ejecuta
la API de dominio pública. Debe reproducir 7/7 totales aprobados, rechazar los
3/3 controles con su código exacto, comprobar que la suma de la traza coincide
con el total y demostrar que una regla distinta de `PUBLISHED` no puede
resolverse. Esta cobertura es independiente de la transformación limitada del
validador de schemas.

Las dependencias compiladas desde fuente deben fijar commit, SDK y TFM, generar
SBOM/provenance, comparar hashes en rutas independientes y ejecutar los mismos
contratos que el proveedor sustituido. La repetibilidad en una sola carpeta no
se considera por sí sola evidencia suficiente de reproducibilidad.

La suite de Application prueba el camino archivo → adaptador → catálogo → caso
de uso → motor. Los resultados esperados y entradas se leen de los diez casos
canónicos; no se duplican números del juego en el código de prueba. Dos copias
temporales alteradas verifican además que el adaptador rechace una regla no
`PUBLISHED` y una referencia a regla inexistente antes de ejecutar cálculos.

El smoke WPF publicado añade el camino artefacto → snapshot empaquetado →
Application → motor. Debe localizar las cuatro carpetas requeridas, reproducir
7/7 casos positivos y 3/3 rechazos desde los propios JSON, y repetir el gate
desde una copia de reemplazo. También compara SHA-256 de todos los archivos del
ruleset entre ambas carpetas para detectar pérdida o mutación del contenido.

El contrato de distribución de stats comienza con fixtures estructurales
sintéticos. La siguiente suite de dominio deberá cubrir como mínimo distribución
parcial y exacta, valores negativos, stat no disponible, stat omitido y gasto
superior al presupuesto. La disponibilidad se resolverá contra las claves de
`stats` de la clase cargada; no se duplicará una lista factual de clases o stats
en las pruebas. Los casos sintéticos no se enlazan desde reglas publicadas ni
requieren un registro de investigación.
