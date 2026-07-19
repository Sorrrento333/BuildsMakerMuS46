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

### CalculationScenario
Modalidad, objetivo, mapa, buffs externos y reglas específicas.

### EvidenceRecord
Fuente, fragmento, fecha, interpretación, alcance, confianza y estado de revisión.

## Value objects
`StatValue`, `Level`, `ResetCount`, `Probability`, `DamageRange`, `Percentage`, `FormulaId`, `RulesetId`, `ItemId`.
