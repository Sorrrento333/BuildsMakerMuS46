# Ruleset MU Online Season 4 global/inglés

Registros canónicos y factuales del ruleset `mu-s4-global-reference`. No son
ejemplos sintéticos. Cada archivo debe validar contra el contrato indicado,
declarar confianza y enlazar la evidencia que autoriza su uso.

## Versión `v1`

- `character-classes/`: seis clases base, estadísticas iniciales/distribuibles
  y evoluciones aprobadas.
- `progression-rules/`: dos reglas de puntos por nivel `PUBLISHED`; sus
  `testCaseRefs` enlazan los siete casos positivos aprobados.
- `formulas/`: `formula-hp-dark-wizard` `1.0.0` está `PUBLISHED` después de
  revisar aplicabilidad, procedencia, bounds, traza y referencias aprobadas.
  La versión `1.1.0` coexiste `PUBLISHED`, usa el programa estructurado
  `CHECKED_INT64_V1` y enlaza sus cuatro casos positivos versionados. Su
  revisión de publicación no encontró divergencias en los nueve JSON.
  `formula-hp-dark-knight` `1.0.0` nace directamente contra el contrato
  ejecutable `2.0.0`, está `PUBLISHED` y enlaza cuatro positivos propios.
  `formula-hp-fairy-elf` `1.0.0` sigue el mismo contrato ejecutable, está
  `PUBLISHED` y enlaza cuatro positivos propios. `formula-hp-summoner`
  `1.0.0` materializa el claim verificado específico de Summoner contra el
  mismo contrato, está `PUBLISHED` y enlaza otros cuatro positivos.
  `formula-hp-magic-gladiator` `1.0.0` materializa la expresión aprobada para
  Magic Gladiator/Duel Master, está `PUBLISHED` y enlaza cuatro positivos.
  `formula-hp-dark-lord` `1.0.0` usa el contrato `2.1.0` y
  `CHECKED_DECIMAL_V1` para conservar exactamente el coeficiente `1.5`, aplica
  a Dark Lord/Lord Emperor y enlaza cuatro positivos.
  `formula-mana-dark-wizard` `1.0.0` usa `CHECKED_INT64_V1`, aplica a las tres
  evoluciones de Dark Wizard y enlaza cuatro positivos sin inferir una base
  fuera de la expresión aprobada.
  `formula-mana-dark-knight` `1.0.0` usa `CHECKED_DECIMAL_V1`, conserva
  exactamente el coeficiente `0.5`, aplica a las tres evoluciones de Dark
  Knight y enlaza cuatro positivos con truncamiento únicamente en la salida.
  `formula-mana-fairy-elf` `1.0.0` usa el mismo modelo decimal para conservar
  exactamente `1.5`, aplica a Fairy Elf/Muse Elf/High Elf y enlaza cuatro
  positivos con truncamiento únicamente en la salida.
  `formula-mana-summoner` `1.0.0` conserva exactamente `1.5` y `1.7`, aplica
  a Summoner/Bloody Summoner/Dimension Master y enlaza cuatro positivos con
  truncamiento únicamente en la salida.
  `formula-mana-magic-gladiator` `1.0.0` usa `CHECKED_INT64_V1`, aplica a
  Magic Gladiator/Duel Master y enlaza cuatro positivos sin desplazar Energy ni
  redondear antes de la salida.
  `formula-mana-dark-lord` `1.0.0` usa `CHECKED_DECIMAL_V1`, conserva
  exactamente `1.5`, aplica a Dark Lord/Lord Emperor y enlaza cuatro positivos
  con truncamiento únicamente en la salida.
  `formula-ag-dark-wizard` `1.0.0` consume exclusivamente los cuatro stats
  resueltos, conserva exactamente `0.2`, `0.3` y `0.4` mediante
  `CHECKED_DECIMAL_V1`, aplica a las tres evoluciones de Dark Wizard y enlaza
  cuatro positivos con truncamiento únicamente en la salida.
  `formula-ag-dark-knight` `1.0.0` consume los mismos cuatro valores
  contextuales, conserva exactamente `0.3`, `0.2` y `0.15`, aplica a Dark
  Knight/Blade Knight/Blade Master y enlaza cuatro positivos con truncamiento
  únicamente en la salida.
  `formula-ag-fairy-elf` `1.0.0` conserva exactamente `0.2` y `0.3`, aplica a
  Fairy Elf/Muse Elf/High Elf y enlaza cuatro positivos con truncamiento
  únicamente en la salida.
  `formula-ag-summoner` `1.0.0` conserva exactamente `0.2`, `0.25`, `0.3` y
  `0.15`, aplica a Summoner/Bloody Summoner/Dimension Master y enlaza cuatro
  positivos con truncamiento únicamente en la salida.
  `formula-ag-magic-gladiator` `1.0.0` conserva exactamente `0.15`, `0.3`,
  `0.25` y `0.2`, aplica a Magic Gladiator/Duel Master, conserva `DSP-0002` y
  enlaza cuatro positivos con truncamiento únicamente en la salida.
- `reference-cases/progression/valid/`: siete casos factuales versionados para
  los bordes aprobados de nivel 1/220/221/230 y MG/DL en nivel 220.
- `reference-cases/progression/invalid/`: tres controles técnicos que deben
  rechazarse: Hero Status con evolución base y Hero Status para Magic
  Gladiator/Dark Lord. No agregan datos factuales al ruleset.
- `reference-cases/formulas/valid/`: las dos series históricas de Dark Wizard
  y cuatro casos positivos propios para Dark Knight, Fairy Elf, Summoner,
  Magic Gladiator, Dark Lord, Mana de Dark Wizard, Mana de Dark Knight,
  Mana de Fairy Elf, Mana de Summoner, Mana de Magic Gladiator, Mana de Dark
  Lord, AG de Dark Wizard, AG de Dark Knight, AG de Fairy Elf y AG de
  Summoner, siempre por referencia exacta.
- `reference-cases/formulas/invalid/`: controles de nivel, Vitality, familia y
  overflow por cada referencia de HP, más nivel, Energy, familia y overflow
  para Mana. AG de Dark Wizard, Dark Knight y Fairy Elf cubren los cuatro
  mínimos de stats, familia y overflow. AG de Summoner y Magic Gladiator
  cubren los cuatro mínimos y familia; no se inventa un overflow imposible
  dentro de sus dominios válidos. Los controles nunca se enlazan desde
  `testCaseRefs`.

Los IDs usan prefijos de tipo (`class-`, `evolution-`, `progression-`,
`quest-`) y son referencias estables; los nombres visibles no se usan como
identidad. Los valores proceden del alcance cerrado de `RES-0001` y están
clasificados `VERIFIED` por `EVD-0021`; las fórmulas de Dark Knight y Fairy Elf
proceden de `EVD-0026`; Magic Gladiator y Dark Lord añaden sus mínimos
factuales de Vitality trazados por `EVD-0021`. Mana de Dark Wizard y Dark
Knight conservan Energy mínima 30 y 10 respectivamente desde la misma evidencia.
Mana de Fairy Elf conserva Energy mínima 15 y su expresión procede de
`EVD-0026`. Mana de Summoner conserva Energy mínima 23 desde `EVD-0021`,
truncamiento desde `EVD-0026`, contraste `EVD-0027`–`EVD-0029` y autoridad
final de `EVD-0031`. Mana de Magic Gladiator conserva Energy mínima 26 desde
`EVD-0021`, la expresión y el truncamiento de `EVD-0026`, y el conflicto
resuelto `DSP-0002`. Mana de Dark Lord conserva Energy mínima 15 desde
`EVD-0021` y la expresión y truncamiento de `EVD-0026`. AG de Dark Wizard
conserva los cuatro mínimos canónicos desde `EVD-0021` y la expresión y
truncamiento de `EVD-0026`. AG de Dark Knight, Fairy Elf y Summoner conservan
del mismo modo sus mínimos canónicos, expresión y truncamiento. HP de Summoner
conserva `EVD-0027`–`EVD-0030` y la autoridad final de `EVD-0030`. Las evidencias
externas y los conflictos
conservan su clasificación original en el registro de investigación.

Estos archivos constituyen el snapshot distribuido por la aplicación WPF. El
validador selecciona el contrato de fórmula por `schemaVersion`, exige
identidad compuesta `id` + `version` única y comprueba las relaciones de
progresión y, para fórmulas, identidad,
catálogo, inputs, pasos, outputs, redondeo, provenance y cobertura de positivos.
Los casos con el mismo ID coexisten por la versión de `formulaRef`; una pareja
de caso `id` + versión repetida falla cerrada. Los controles negativos quedan
fuera de las referencias de publicación.

Application materializa productivamente las clases y reglas desde este layout
después del gate de schemas. Su adaptador vuelve a exigir un único ruleset,
referencias clase/regla coherentes y reglas `PUBLISHED`; el caso de uso invoca el
motor sin codificar números del juego. Las pruebas cargan directamente estos
JSON y reproducen los siete casos positivos y tres rechazos. WPF los empaqueta
bajo la misma estructura y calcula la identidad del dataset sobre rutas
relativas y bytes exactos.
