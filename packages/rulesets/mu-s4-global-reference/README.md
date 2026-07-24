# Ruleset MU Online Season 4 global/inglés

Registros canónicos y factuales del ruleset `mu-s4-global-reference`. No son
ejemplos sintéticos. Cada archivo debe validar contra el contrato indicado,
declarar confianza y enlazar la evidencia que autoriza su uso.

## Versión `v1`

- `character-classes/`: seis clases base, estadísticas iniciales/distribuibles
  y evoluciones aprobadas.
- `progression-rules/`: dos reglas de puntos por nivel `PUBLISHED`; sus
  `testCaseRefs` enlazan los siete casos positivos aprobados.
- `reference-cases/progression/valid/`: siete casos factuales versionados para
  los bordes aprobados de nivel 1/220/221/230 y MG/DL en nivel 220.
- `reference-cases/progression/invalid/`: tres controles técnicos que deben
  rechazarse: Hero Status con evolución base y Hero Status para Magic
  Gladiator/Dark Lord. No agregan datos factuales al ruleset.

Los IDs usan prefijos de tipo (`class-`, `evolution-`, `progression-`,
`quest-`) y son referencias estables; los nombres visibles no se usan como
identidad. Los valores proceden del alcance cerrado de `RES-0001` y están
clasificados `VERIFIED` por `EVD-0021`. Las evidencias externas y los conflictos
conservan su clasificación original en el registro de investigación.

Estos archivos todavía no constituyen un snapshot distribuible. El validador de
repositorio ejecuta una transformación limitada, comprueba los resultados,
exige que cada referencia publicada resuelva dentro del mismo ruleset y regla,
y mantiene fuera de publicación los controles negativos.

Application materializa productivamente las clases y reglas desde este layout
después del gate de schemas. Su adaptador vuelve a exigir un único ruleset,
referencias clase/regla coherentes y reglas `PUBLISHED`; el caso de uso invoca el
motor sin codificar números del juego. Las pruebas cargan directamente estos
JSON y reproducen los siete casos positivos y tres rechazos. El empaquetado de
este contenido como snapshot distribuible para WPF sigue pendiente.
