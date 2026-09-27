# Diseño técnico — opción Jewel of Life en la instancia equipada (vertical 1.3.0)

## Estado y alcance

- Fecha: 2026-09-27.
- Estado: `IMPLEMENTED` (Application + WPF + smoke + schemas 1.3.0).
- Ruleset revisado: `mu-s4-global-reference`.
- Dataset: `2026-09-20.1` (sin cambios: no se añade ningún JSON factual).
- Registro factual: `docs/05-research/registers/RES-0005-uc04-bono-required-sockets.md`
  (`VERIFIED`, `DSP-0012` `RESOLVED` por `OWNER_DECISION`, `EVD-0053`).
- Consumo previo: `docs/04-domain/equipped-instance-design.md`
  (instancia `{ itemId, itemVersion, level }`, schemas `1.2.0`)
  y `docs/04-domain/items-defense-level-bonus-design.md` (primera mitad del
  axio-axioma parcial `EVD-0053`).

Este documento es la segunda vertical de implementación autorizada por el
axio-axioma parcial de `EVD-0053`: materializar la regla de opción Jewel of
Life (`+5 STR` por nivel de opción) sobre la instancia equipada. Era la parte
explícitamente diferida del diseño de defensa ("JOL diferido hasta que
`equipmentEntry` modele opciones").

## Base factual (sin invención)

- `EVD-0036` (MU Online Fanz, guía `Combat Items`): las opciones Jewel of Life
  aportan `STR +5` por nivel de opción.
- `EVD-0047`–`EVD-0049` (páginas de Kris, Dragon Armor y Albatross Bow): cada
  ítem del subconjunto ofrece `+Jewel of Life option*` con la nota
  "the item's STR requirement increases by 5, per option level!".
- `EVD-0053` (decisión del propietario, 2026-09-20): adopta la regla JOL
  `+5 STR` por nivel de opción como axioma parcial del ruleset dentro del
  subconjunto acotado de `RES-0003`.
- La nota de Fanz describe la regla como aumento del **requisito de STR**, no
  como bonificación al personaje: la semántica adoptada es
  `STR_requerido(n) = STR_base + 5·n`, donde `n` es el nivel de opción JOL
  declarado en la instancia equipada.

## Decisiones cerradas antes de implementar

1. **Campo `optionLevel`** — nivel de opción Jewel of Life declarado en la
   instancia equipada (`BuildEquipmentEntry`), entero `>= 0`, opcional con
   valor por defecto `0`. Es un dato declarado por el usuario, igual que el
   nivel del ítem: no se deriva de ninguna definición publicada.
2. **Sin cota superior adoptada** — ninguna fuente del subconjunto publica un
   máximo de nivel de opción JOL aplicable a Season 4. El máximo global `+16`
   de `EVD-0051` (StrategyWiki, contraste `PARTIAL`) no se adopta como regla;
   fijar una cota sería convertir una suposición en regla del sistema. El
   producto usa aritmética comprobada de 64 bits (`5 × optionLevel` cabe
   siempre en `long` para `optionLevel` de 32 bits).
3. **Sólo la clave `strength`** — la regla afecta exclusivamente al requisito
   `strength` cuando el ítem lo declara (los tres ítems del subconjunto lo
   hacen). No se crean claves nuevas ni se tocan otras claves de
   `requiredStats`. Si un ítem futuro no declarase `strength`, la opción JOL
   no tendría efecto sobre requisitos (sin inferencias).
4. **Requisito efectivo, no bonificación** — la elegibilidad (UC-04) y la
   revalidación de la instancia comparan los stats finales contra el requisito
   efectivo `base + 5·n`. El resultado de equipado expone ambos mapas
   (`RequiredStats` base publicada y `EffectiveRequiredStats` con JOL).
5. **Nuevo código fail-closed** — `option-level-out-of-range` (familias
   `item-equip-*`, `build-equipment-*`, `build-draft-equipment-*`) para
   `optionLevel` negativo vía API de C# (el schema JSON lo impide con
   `minimum: 0`).

## Alcance materializable

| Campo / regla | Decisión | Se implementa |
|---|---|---|
| `optionLevel` en `equipmentEntry` | Adoptado (dato declarado del usuario) | Sí, opcional, default `0` |
| `STR_efectivo = base + 5·n` | Adoptado (axioma parcial `EVD-0053`) | Sí, sólo clave `strength` |
| Cota máxima de `optionLevel` | Sin fuente para el subconjunto | No (documentado, sin inventar) |
| `requiredLevel` | Excluido (sin fuente) | No |
| ATK por nivel de ítem (armas) | Excluido (sin fuente) | No |
| Progresión `requiredStats` por nivel de ítem | Excluido (sin fuente) | No |
| Sockets | Excluido (grado normal) | No |

## Contrato de schemas

- `packages/schemas/v1/build-draft.schema.json` y
  `packages/schemas/v1/build.schema.json` avanzan `1.2.0` → `1.3.0`.
- `equipmentEntry` gana la propiedad **opcional** `optionLevel`:
  `{ "type": "integer", "minimum": 0 }`. Sigue sin `maxItemLevel` explícito
  (el rango de `level` lo valida Application contra la definición publicada,
  como hasta ahora).
- Fixtures sintéticos `valid/build-draft.json` y `valid/build.json` suben a
  `1.3.0` y declaran `optionLevel` para probar su aceptación; los fixtures
  inválidos no cambian (siguen rechazados). `Test-SchemaStructure` actualiza
  `1.2.0` → `1.3.0` en ambas entradas; el inventario (17/34) no cambia.

## Contrato de Application

- `BuildEquipmentEntry` gana `OptionLevel` (`int`, default `0`,
  `JsonPropertyName("optionLevel")`). El default conserva la compatibilidad
  de todas las construcciones posicionales existentes.
- Nuevo componente derivado **`ItemOptionBonusCalculator`** (Application,
  paquete `Items/`): constante `StrengthPerOptionLevel = 5` trazada a
  `EVD-0036`/`EVD-0053`; `ApplyToRequiredStats(base, optionLevel)` devuelve el
  mapa efectivo con la clave `strength` incrementada en `checked(5·n)` cuando
  existe; `optionLevel` negativo → `ArgumentOutOfRangeException`. No codifica
  datos del juego: sólo el coeficiente de la regla.
- `BuildEquipmentValidator`: rechaza `OptionLevel < 0` con
  `option-level-out-of-range`; compara requisitos contra el mapa efectivo.
- `EquipItemRequest` gana `OptionLevel` (default `0`);
  `EquipItemResult` gana `OptionLevel` y `EffectiveRequiredStats`;
  `EquipItemUseCase` rechaza negativo con
  `item-equip-option-level-out-of-range` y valida contra el mapa efectivo.
- Nuevos códigos estables: `ItemEquipErrorCodes.OptionLevelOutOfRange`,
  `BuildErrorCodes.EquipmentOptionLevelOutOfRange`,
  `BuildDraftErrorCodes.EquipmentOptionLevelOutOfRange`; los tres
  `MapEquipmentCode` ganan el brazo explícito (antes caería en el `_`
  de requisitos, que ocultaría la causa).
- Versiones: `BuildDraft.CurrentSchemaVersion` y
  `CharacterBuild.CurrentSchemaVersion` → `1.3.0`;
  `PreviousSchemaVersion` → `1.2.0`. Normalización al cargar:
  - `1.3.0` → as-is.
  - `1.2.0` → `1.3.0` mapeando cada entrada con `OptionLevel = 0`
    (el usuario no declaró opciones; no se inventa ninguna).
  - Ramas legacy `1.1.0`/`1.0.0` (drafts) y `1.1.0` (builds) se conservan con
    literales explícitos y `Equipment = []`, como hasta ahora.
- Sin cambios en ruleset (`1.0.0`), dataset (`2026-09-20.1`, hash intacto: no
  hay JSON factuales nuevos ni versionados) ni motor (`0.2.0`): es una regla
  de Application sobre datos declarados, igual que la vertical de defensa.

## WPF

- La sección «Equipo» añade `EquipItemOptionLevelTextBox` ("Nivel de opción
  JOL", default `"0"`); «Equipar seleccionado» valida el entero y lo pasa a
  `EquipItemRequest` y a `BuildEquipmentEntry`.
- La lista muestra `ItemId · vItemVersion · nivel Level · JOL OptionLevel`.
- `FormatItemEquipResult` muestra el requisito efectivo de STR cuando
  `OptionLevel > 0` (trazado a `EVD-0053`); el nuevo código de error tiene
  explicación visible en español.

## Smoke WPF

- Precedente de la vertical de defensa: verificación a nivel de catálogo sin
  alterar el household equipado (`item-kris` +15, que conserva `optionLevel`
  por defecto `0` y sigue pasando con `PersistedBuildCount = 2`).
- `VerifyPublishedItemCatalog` añade la verificación JOL sintética sobre el
  snapshot publicado: kris con `OptionLevel: 1` y STR 32/AGI 27 es elegible
  con `EffectiveRequiredStats["strength"] == 32`; con STR 27 se rechaza con
  `item-equip-requirements-not-met`; con `OptionLevel: -1` se rechaza con
  `item-equip-option-level-out-of-range`. Fallo cerrado vía excepción, sin
  campos nuevos de reporte ni cambios en `Test-WpfPublishedArtifact.ps1`.
- La persistencia de `optionLevel` (round-trip draft→build→carga y migración
  `1.2.0`→`1.3.0`) queda cubierta por pruebas de integración de Application;
  el smoke no crea documentos adicionales para no alterar los contadores.

## Verificación esperada

- Calculador: `base 27, n=0 → 27`; `n=1 → 32`; `n=2 → 37`; claves ajenas a
  `strength` intactas; ausencia de `strength` sin efecto; negativo →
  `ArgumentOutOfRangeException`.
- `EquipItemUseCase` canónico: kris JOL 1 con STR 32 elegible;
  con STR 31 rechazado (`requirements-not-met`); JOL −1 rechazado
  (`option-level-out-of-range`).
- Drafts/builds: round-trip con `OptionLevel = 2`; migración `1.2.0` →
  `1.3.0` con `OptionLevel = 0`; nombre JSON exacto `optionLevel`.
- Estructural: `Test-SchemaStructure` 17/34 con `build-draft`/`build`
  `1.3.0`; suite completa en verde; smoke WPF `win-x64` PASS.

## Fuera de alcance

- Cota máxima de opción JOL, `requiredLevel`, ATK por nivel de armas,
  progresión de `requiredStats` por nivel de ítem y sockets: sin fuente o
  diferidos; cualquier evolución exige nueva evidencia o nueva decisión del
  propietario.
- Cualquier ítem, ranura, grado o campo ajeno al subconjunto de `RES-0003` y
  al alcance de `EVD-0053`.
