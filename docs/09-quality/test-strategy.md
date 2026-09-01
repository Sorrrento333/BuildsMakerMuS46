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

El diseño de `DR-HP-DARK-WIZARD` aplica esta regla antes de crear fixtures:
separa el caso base, el incremento de nivel, el incremento de Vitality, una
combinación sobre otra evolución de la familia, mínimos inválidos, familia no
aplicable y overflow técnico. Cada caso válido fija los tres aportes, el valor
crudo y el truncamiento final. Los contratos decididos en
`docs/06-data/formula-schema-contract-decision.md` y los ocho casos factuales ya
existen. Los cuatro positivos son referencias versionadas de
`formula-hp-dark-wizard`; los cuatro negativos permanecen separados. Este gate
validó la promoción de la fórmula a `PUBLISHED` y fija ese estado mediante una
prueba de contrato, pero todavía no ejecuta un motor de HP.

El diseño de la vertical ejecutable concluye que el contrato `1.1.0` no basta
para un motor cerrado. El contrato `formula` `2.0.0` ya separa el programa
sintético de la futura regresión factual y sus pruebas rechazan operaciones o
aridades no soportadas, referencias adelantadas, inputs no declarados, pasos
divergentes, `rangeErrorCode` ausente, bounds incoherentes y desacoples de
salida/redondeo. La futura cobertura del intérprete leerá definición, inputs,
trazas, resultados y errores de los ocho casos factuales versionados, sin
constantes en C#, y rechazará el artefacto textual histórico como ejecutable.
Véase `../04-domain/dark-wizard-hp-execution-vertical-design.md`.

La definición canónica `formula-hp-dark-wizard` `1.1.0` está `PUBLISHED` y
enlaza cuatro positivos de su propia versión. La revisión de sus nueve JSON no
encontró divergencias y cambió únicamente `status`; una prueba fija el estado.
Las pruebas del gate demuestran
que `schemaVersion` selecciona `v1` o `v2`, que `1.0.0`/`1.1.0` coexisten bajo
el mismo ID y que una repetición de la pareja de fórmula `id` + `version` falla
cerrada. Las dos series de ocho casos conservan IDs y contenido y coexisten por
la versión de `formulaRef`; una pareja de caso `id` + versión repetida también
falla cerrada. Una prueba compara cada copia y exige que el único cambio sea
`formulaRef.version`. La versión `1.1.0` ya se ejecuta en la vertical de
Application descrita más abajo.

El intérprete `CHECKED_INT64_V1` ya está cubierto por veinticinco pruebas
exclusivamente sintéticas. Recorren las cinco operaciones cerradas, estado
publicado, aplicabilidad, conjunto exacto de inputs, bounds
inclusivos/exclusivos, rango técnico de 32 bits, códigos de rango
materializados, seis modos de redondeo sobre enteros, orden de traza,
referencias adelantadas, programa no soportado y overflow de suma, resta y
multiplicación. Una prueba adicional dentro de ese conjunto muta las
colecciones del llamador después de construir definición y solicitud para
demostrar que Domain conserva copias inmutables.

Cinco pruebas sintéticas adicionales cubren `CHECKED_DECIMAL_V1`: conservación
exacta de `1.5`, ausencia de redondeo intermedio, truncamiento final, operandos
incoherentes, `DIVIDE` por un divisor declarado y overflow al convertir la
salida publicada a `INT64`.

Esta cobertura no lee fórmulas ni casos factuales y por tanto no constituye
regresión factual. El gate independiente de Application ya materializa
`formula` `2.0.0`/`2.1.0` y ejecuta desde archivos los casos de
Dark Wizard `1.1.0`, Dark Knight `1.0.0`, Fairy Elf `1.0.0` y Summoner
`1.0.0`, Magic Gladiator `1.0.0`, Dark Lord `1.0.0` y Mana de Dark Wizard
`1.0.0`, Dark Knight `1.0.0`, Fairy Elf `1.0.0`, Summoner `1.0.0`, Magic
Gladiator `1.0.0`, Dark Lord `1.0.0`, AG de Dark Wizard `1.0.0`, AG de Dark
Knight `1.0.0`, AG de Fairy Elf `1.0.0`, AG de Summoner `1.0.0`, AG de Magic
  Gladiator `1.0.0`, AG de Dark Lord `1.0.0`, Defense/SD de Dark Wizard `1.0.0`,
  Defense/SD de Dark Knight `1.0.0`, Defense/SD de Fairy Elf `1.0.0`,
  Defense/SD de Magic Gladiator `1.0.0` y Defense/SD de Dark Lord `1.0.0`; también
rechaza el `1.0.0` histórico de Dark Wizard como no ejecutable. Las trazas se
comparan campo por campo,
incluidos contexto, inputs, pasos, redondeo, outputs, evidencias y conflictos.

Las dependencias usan casos sintéticos para demostrar
referencia/version exactas, selección diferente de `RAW` y `VISIBLE`,
preservación decimal, traza separada y rechazo de ciclos. La vertical factual
añade `sd-dark-wizard-raw-defense-boundary`, que fija `RAW=4.75`,
`VISIBLE=4` y SD visible 101; `EVD-0033` es la autoridad de esa selección.
`sd-dark-knight-raw-defense-boundary` fija `RAW=7.6666…`, `VISIBLE=7` y SD
visible 107; `EVD-0034` es la autoridad de esa selección.
`sd-fairy-elf-raw-defense-boundary` fija `RAW=2.7`, `VISIBLE=2` y SD visible
102, frente a 101 si se consumiera la etapa visible; `EVD-0034` es también la
autoridad de esta selección.

Los casos de fórmula tendrán contrato propio. Un positivo compondrá mediante
`$ref` una traza completa y un negativo conservará sólo el código de error
esperado. Una fórmula publicada referenciará únicamente sus positivos del mismo
ruleset, ID y versión; los controles negativos permanecerán separados. El gate
semántico comprobará además que cada traza contiene exactamente los pasos
declarados por la fórmula, en orden, y que salida cruda, redondeo y salida
visible son coherentes.

La suite de contrato actual usa ocho fixtures exclusivamente sintéticos para
`formula` v1/v2, `calculation-trace` y `formula-test-case`. Pruebas focalizadas
rechazan aplicabilidad vacía o duplicada, bounds factuales sin evidencia,
procedencia incompleta, pasos duplicados y salidas no declaradas; también
demuestran la unión exclusiva positivo/negativo y que una alteración interna de
la traza es rechazada a través del `$ref` real. Para v2 prueban además las cinco
operaciones permitidas, la unión cerrada de operandos, sus aridades, orden de
referencias, compatibilidad de bounds con `INT32` y correspondencia exacta entre
programa y traza.

La suite factual lee las veintinueve definiciones —una histórica y veintiocho
ejecutables— y sus casos desde el ruleset. Las doce referencias HP/Mana
conservan cuatro positivos y cuatro negativos; AG de Dark Wizard, Dark Knight y
Fairy Elf conservan cuatro positivos y seis negativos cada una para cubrir cada
mínimo factual. AG de Summoner y Magic Gladiator conservan cuatro positivos y
cinco negativos cada una porque su dominio válido no puede desbordar la salida.
AG de Dark Lord conserva cuatro positivos y siete negativos para cubrir sus
cinco mínimos, familia y el overflow alcanzable con una suma de coeficientes de
`1.05`.
Defense/SD de Dark Wizard, Dark Knight, Fairy Elf y Magic Gladiator conservan
ocho positivos y nueve negativos por familia. Dark Lord conserva ocho positivos
y diez negativos porque SD consume también Command; cada frontera distingue
explícitamente `RAW` de `VISIBLE`.
No duplica
resultados ni constantes del juego en C#.
El gate exige
identidad exacta de ruleset/fórmula/versión, clase y evolución existentes,
inputs completos, orden de pasos, correspondencia de outputs, redondeo,
evidencias/conflictos heredados y cobertura exacta de los cuatro positivos.
Mutaciones temporales demuestran rechazo de evolución ajena, orden de traza,
provenance y cobertura incompleta.

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

Para fórmulas, la suite usa una copia temporal que contiene únicamente
`character-classes/` y `formulas/`; esto demuestra que el camino normal no lee
casos de referencia. Además de los veinte casos canónicos, mutaciones verifican
fallo cerrado ante estado no publicado, evolución ajena, referencia a un paso
futuro y duplicado exacto `id` + `version`. La solicitud de `1.0.0` falla con
`formula-not-executable` antes de llegar al intérprete.

El gate independiente de `CONTEXT_VALUE` está implementado. Compara bases,
evidencias y `source.valueId` con los JSON, construye el estado mediante
progresión y distribución y deriva las asignaciones requeridas desde casos
canónicos y bases leídas, sin copiar números del juego en C#. Los controles
cubren contexto no resoluble, fuente no soportada, mismatch, base/asignación
ausentes, inmutabilidad y overflow de `baseValue + allocation`. Nivel inválido
y asignación negativa fallan antes en sus casos de uso de origen. La ruta
productiva no acepta un diccionario contextual aportado por WPF.

El smoke WPF publicado añade el camino artefacto → snapshot empaquetado →
Application → motor. Debe localizar las cuatro carpetas requeridas, reproducir
7/7 casos positivos y 3/3 rechazos desde los propios JSON, y repetir el gate
desde una copia de reemplazo. También compara SHA-256 de todos los archivos del
ruleset entre ambas carpetas para detectar pérdida o mutación del contenido.
Sobre un presupuesto positivo ya reproducido, genera todas las asignaciones
desde los `StatIds` materializados, configura dos resets de 100 puntos y
verifica producto 200, presupuesto combinado, gasto, remanente y conjunto exacto
en ambas fases. Este control no publica una regla factual de resets.
El mismo smoke carga las veinticuatro definiciones ejecutables y sus positivos, deriva
el nivel técnico de composición cuando la fórmula no lo consume y las
asignaciones desde cada JSON y la base materializada, y reproduce 24/24
casos de HP más 4/4 casos de Mana de Dark Wizard, 4/4 de Mana de Dark Knight y
4/4 de Mana de Fairy Elf, 4/4 de Mana de Summoner y 4/4 de Mana de Magic
Gladiator, 4/4 de Mana de Dark Lord, 4/4 de AG de Dark Wizard, 4/4 de AG de
Dark Knight, 4/4 de AG de Fairy Elf, 4/4 de AG de Summoner, 4/4 de AG de Magic
Gladiator y 4/4 de AG de Dark Lord, más 8/8 de Defense/SD para Dark Wizard,
Dark Knight, Fairy Elf, Magic Gladiator y Dark Lord,
con trazas contextual y aritmética antes y después del reemplazo.

El contrato de distribución de stats conserva fixtures estructurales
sintéticos. La suite productiva cubre distribución parcial y exacta, valores
negativos, stat no disponible, stat omitido, gasto superior al presupuesto,
overflow y las divergencias de ruleset, clase y regla del presupuesto. La
disponibilidad se resuelve contra los IDs de `stats` de la definición cargada;
no se duplica una lista factual de clases o stats en esas pruebas. Una prueba de
integración adicional compara la materialización de esos IDs con las claves del
propio snapshot. Los casos sintéticos no se enlazan desde reglas publicadas ni
requieren un registro de investigación.

La suite añade el caso `2 × 100 = 200`, valores predeterminados cero, inputs
negativos, overflow del producto y overflow de la suma con progresión. Los
valores son sintéticos y prueban una configuración de servidor, no una mecánica
de MU Online.

La suite de Application añade el camino copia temporal del snapshot → catálogo
→ `CalculateStatDistributionUseCase` → motor. Dos casos sintéticos comprueban
distribución parcial y exacta sin duplicar nombres de stats, y un tercero altera
el ruleset del presupuesto para exigir `budget-source-mismatch`. Un cuarto
provoca `allocation-negative` en el motor y exige que Application conserve el
mismo código tipado. El caso de uso recibe el presupuesto existente y las
asignaciones; las pruebas no recalculan progresión ni aportan valores factuales
nuevos.

El contrato de borrador conserva un gate estructural separado de la
persistencia. Su fixture válido debe resolver el `$ref` real hacia
`stat-distribution.schema.json`; el inválido mantiene válido el envoltorio y
falla sólo por la distribución referenciada. Así el rechazo demuestra la
composición entre contratos. Los dos son sintéticos.

La suite de Application usa un catálogo y un repositorio en memoria sintéticos.
Debe cubrir alta/carga con recálculo, reemplazo por ID, ausencia, identidad
incoherente, metadata exacto no disponible, caché alterada y round-trip JSON con
los nombres de propiedad del schema. Añade alteración específica de la caché de
resets y normalización de un borrador `1.0.0` a resets cero. No usa clases,
fórmulas ni valores de MU Online.

La suite Data aplica la migración productiva sobre archivos SQLite temporales.
Cubre payload y metadata exactos, reemplazo atómico por ID, rollback completo
ante un trigger sintético, reapertura, ausencia sin mutaciones y traducción del
agotamiento de contención a `build-draft-write-conflict`. Ningún caso añade
datos o fórmulas del juego.

El smoke WPF extiende ese límite hasta el artefacto publicado. La composición
aplica la migración de borradores antes de construir el repositorio, guarda un
borrador sintético derivado del mismo caso ya usado para distribución, lo carga
por `LoadBuildDraftUseCase` y exige su revalidación. Después de backup/restore y
reemplazo de binarios vuelve a abrir la base externa y compara ID, ruleset,
dataset/hash, motor, inputs/producto de resets, total distribuible, asignaciones,
gasto y remanente.
