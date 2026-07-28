# Modelo de dominio

## Agregados principales

### Ruleset
Identifica temporada, revisión, contenido habilitado, fórmulas y fuentes.

### ServerProfile
Hereda de un Ruleset y aplica overrides permitidos: progresión, límites, multiplicadores y contenido.

### CharacterBuild
Clase, evolución, nivel, quests, resets, stats distribuidos, equipo, buffs y metadatos.

### ItemDefinition / ItemInstance
La definición contiene datos canónicos; la instancia contiene nivel, opciones y sockets elegidos.

### FormulaDefinition
ID estable, versión, entradas, expresión/estrategia, redondeo, rango, provenance y tests.

La representación ejecutable usa tipos inmutables y ajenos a JSON para
`CHECKED_INT64_V1` y `CHECKED_DECIMAL_V1`: aplicabilidad por clase/evolución,
inputs enteros con bounds y código de rango materializado, output `INT64`,
programa ordenado, operandos, valores de traza exactos en base 10, redondeo y
metadatos de traza. Calculation Engine interpreta esa estructura sin
conocer IDs ni constantes factuales; Application conserva la responsabilidad de
materializar y cerrar las invariantes relacionales del snapshot.

### CalculationScenario
Modalidad, objetivo, mapa, buffs externos y reglas específicas.

### EvidenceRecord
Fuente, fragmento, fecha, interpretación, alcance, confianza y estado de revisión.

## Value objects
`StatValue`, `Level`, `ResetCount`, `Probability`, `DamageRange`, `Percentage`, `FormulaId`, `RulesetId`, `ItemId`.
