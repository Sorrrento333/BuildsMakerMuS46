# RES-0002 — Recursos derivados de personajes

## Registro

```yaml
id: RES-0002
question: "¿Cuáles son las fórmulas de HP, Mana, AG y SD para cada familia de clase del ruleset MU Online Season 4 global/inglés?"
scope:
  season: "4"
  class: null
  mode: "general"
  ruleset: "mu-s4-global-reference"
status: VERIFIED
claims:
  - id: DR-HP-DARK-WIZARD
    attribute: hp
    class_id: class-dark-wizard
    statement: "La fórmula de HP para la familia Dark Wizard está identificada."
    status: VERIFIED
    evidence: [EVD-0022, EVD-0023, EVD-0024, EVD-0025, EVD-0026]
  - id: DR-HP-DARK-KNIGHT
    attribute: hp
    class_id: class-dark-knight
    statement: "La fórmula de HP para la familia Dark Knight está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-HP-FAIRY-ELF
    attribute: hp
    class_id: class-fairy-elf
    statement: "La fórmula de HP para la familia Fairy Elf está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-HP-SUMMONER
    attribute: hp
    class_id: class-summoner
    statement: "La fórmula de HP para la familia Summoner está identificada."
    status: VERIFIED
    evidence: [EVD-0027, EVD-0028, EVD-0029, EVD-0030]
  - id: DR-HP-MAGIC-GLADIATOR
    attribute: hp
    class_id: class-magic-gladiator
    statement: "La fórmula de HP para la familia Magic Gladiator está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-HP-DARK-LORD
    attribute: hp
    class_id: class-dark-lord
    statement: "La fórmula de HP para la familia Dark Lord está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-MANA-DARK-WIZARD
    attribute: mana
    class_id: class-dark-wizard
    statement: "La fórmula de Mana para la familia Dark Wizard está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-MANA-DARK-KNIGHT
    attribute: mana
    class_id: class-dark-knight
    statement: "La fórmula de Mana para la familia Dark Knight está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-MANA-FAIRY-ELF
    attribute: mana
    class_id: class-fairy-elf
    statement: "La fórmula de Mana para la familia Fairy Elf está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-MANA-SUMMONER
    attribute: mana
    class_id: class-summoner
    statement: "La fórmula de Mana para la familia Summoner está identificada."
    status: VERIFIED
    evidence: [EVD-0027, EVD-0028, EVD-0029, EVD-0031]
  - id: DR-MANA-MAGIC-GLADIATOR
    attribute: mana
    class_id: class-magic-gladiator
    statement: "La fórmula de Mana para la familia Magic Gladiator está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-MANA-DARK-LORD
    attribute: mana
    class_id: class-dark-lord
    statement: "La fórmula de Mana para la familia Dark Lord está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-AG-DARK-WIZARD
    attribute: ag
    class_id: class-dark-wizard
    statement: "La fórmula de AG para la familia Dark Wizard está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-AG-DARK-KNIGHT
    attribute: ag
    class_id: class-dark-knight
    statement: "La fórmula de AG para la familia Dark Knight está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-AG-FAIRY-ELF
    attribute: ag
    class_id: class-fairy-elf
    statement: "La fórmula de AG para la familia Fairy Elf está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-AG-SUMMONER
    attribute: ag
    class_id: class-summoner
    statement: "La fórmula de AG para la familia Summoner está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-AG-MAGIC-GLADIATOR
    attribute: ag
    class_id: class-magic-gladiator
    statement: "La fórmula de AG para la familia Magic Gladiator está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-AG-DARK-LORD
    attribute: ag
    class_id: class-dark-lord
    statement: "La fórmula de AG para la familia Dark Lord está identificada."
    status: VERIFIED
    evidence: [EVD-0026]
  - id: DR-SD-DARK-WIZARD
    attribute: sd
    class_id: class-dark-wizard
    statement: "La fórmula de SD para la familia Dark Wizard está identificada."
    status: VERIFIED
    evidence: [EVD-0026, EVD-0033]
  - id: DR-SD-DARK-KNIGHT
    attribute: sd
    class_id: class-dark-knight
    statement: "La fórmula de SD para la familia Dark Knight está identificada."
    status: VERIFIED
    evidence: [EVD-0026, EVD-0034]
  - id: DR-SD-FAIRY-ELF
    attribute: sd
    class_id: class-fairy-elf
    statement: "La fórmula de SD para la familia Fairy Elf está identificada."
    status: VERIFIED
    evidence: [EVD-0026, EVD-0034]
  - id: DR-SD-SUMMONER
    attribute: sd
    class_id: class-summoner
    statement: "La fórmula de SD para la familia Summoner está identificada."
    status: VERIFIED
    evidence: [EVD-0027, EVD-0028, EVD-0029, EVD-0032, EVD-0034]
  - id: DR-SD-MAGIC-GLADIATOR
    attribute: sd
    class_id: class-magic-gladiator
    statement: "La fórmula de SD para la familia Magic Gladiator está identificada."
    status: VERIFIED
    evidence: [EVD-0026, EVD-0034]
  - id: DR-SD-DARK-LORD
    attribute: sd
    class_id: class-dark-lord
    statement: "La fórmula de SD para la familia Dark Lord está identificada."
    status: VERIFIED
    evidence: [EVD-0026, EVD-0034]
conflicts:
  - id: DSP-0003
    statement: "MU Online Fanz publica +1 HP por punto de Stamina para Dark Wizard, mientras StrategyWiki e InfinityMU publican +2 HP por punto de Vitality."
    evidence: [EVD-0022, EVD-0024, EVD-0025, EVD-0026]
    scope: "Coeficiente de Stamina/Vitality en el HP de Dark Wizard; ninguna fuente demuestra aplicabilidad a Season 4 global/inglés."
    impact: "La divergencia bloqueó inicialmente DR-HP-DARK-WIZARD; EVD-0026 fija la fórmula del ruleset sin reclasificar las fuentes enfrentadas."
    status: RESOLVED
    resolution: OWNER_DECISION
  - id: DSP-0004
    statement: "La fórmula de SD de Summoner decidida en EVD-0032 produce 103 para nivel 1 y stats base si sólo se trunca al final, mientras EVD-0027–EVD-0029 publican SD inicial 102."
    evidence: [EVD-0027, EVD-0028, EVD-0029, EVD-0032]
    scope: "Orden de evaluación y truncamiento de los términos de SD de Summoner en mu-s4-global-reference."
    impact: "EVD-0032 fija el valor visible 102 y el truncamiento independiente de cada término antes de la suma."
    status: RESOLVED
    resolution: OWNER_DECISION
test_plan: "Crear IDs y contratos de fórmula antes del motor. Para cada uno de los 24 recursos VERIFIED se exigirán casos aprobados por familia, bordes de nivel/stats, dependencias explícitas y sus puntos de truncamiento. SD de Summoner debe incluir el caso nivel 1/stats 21/21/18/23 = 102 y trazas separadas de sus tres términos."
conclusion: "RES-0002 queda VERIFIED con 24/24 claims. EVD-0026 fija las fórmulas Season 4 global/inglés de cinco familias y AG de Summoner; EVD-0030 fija HP de Summoner; EVD-0031 fija Mana; EVD-0032 fija SD como trunc((str + agi + vit + ene) * 1.2) + trunc(defense / 2) + trunc((lvl * lvl) / 30), con defense = agi / 3. El caso inicial produce 99 + 3 + 0 = 102 y resuelve DSP-0004 por decisión del propietario. La verificación factual autoriza diseñar contratos y casos, pero no omitir esos gates ni copiar fórmulas directamente al motor."
reviewed_by: ["project-owner"]
last_reviewed_at: "2026-07-28"
```

## Alcance y límites

- Las seis familias y sus IDs proceden de los registros canónicos ya aprobados
  por `RES-0001`; este registro no reabre sus nombres, evoluciones ni stats base.
- `HP`, `Mana`, `AG` y `SD` son las cuatro salidas que se investigarán. Su mera
  inclusión como etiquetas de alcance no afirma disponibilidad, fórmula ni
  comportamiento para una clase o evolución.
- Cada combinación atributo/clase es un claim independiente. Una coincidencia
  entre clases o fuentes no permite reutilizar una fórmula por inferencia.
- Antes de promover un claim deben quedar explícitos sus entradas, constantes,
  etapa o evolución aplicable, versión, orden de operaciones y redondeo.
- Buffs, equipo, opciones, perfiles privados, regeneración, daño, defensa,
  rates y comportamiento PvM/PvP quedan fuera de los 24 claims de este registro.
  EVD-0026 preserva las fórmulas adicionales decididas por el propietario para
  transferirlas a futuros registros, sin ampliar silenciosamente estos claims.

## Matriz inicial

| Atributo | Dark Wizard | Dark Knight | Fairy Elf | Summoner | Magic Gladiator | Dark Lord |
|---|---|---|---|---|---|---|
| HP | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` |
| Mana | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` |
| AG | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` |
| SD | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` | `VERIFIED` |

## Campos de verificación por claim

| Campo | Estado inicial | Regla de cierre |
|---|---|---|
| Fórmula y constantes | Desconocido | Transcripción trazada; ninguna constante implícita |
| Entradas | Desconocido | Stats, nivel y demás entradas identificadas por ID |
| Evolución aplicable | Desconocido | No asumir que toda la familia comparte comportamiento |
| Versión | No demostrada | Evidencia aplicable a Season 4 global/inglés |
| Orden de operaciones | Desconocido | Secuencia explícita y reproducible |
| Redondeo | Desconocido | Momento y modo de redondeo explícitos |
| Variantes | No investigadas | Servidores privados separados del ruleset base |
| Casos de prueba | No creados | Bordes y resultados manuales aprobados |

## Plan de investigación

1. Comenzar en MU Online Fanz y registrar la primera evidencia sin promover
   automáticamente ningún claim.
2. Extraer afirmaciones atómicas; si una página agrupa clases o atributos,
   conservar el texto fuente pero evaluar cada celda por separado.
3. Contrastar con fuentes adicionales autorizadas y determinar su independencia,
   región y versión.
4. Registrar como `PARTIAL` toda fórmula útil cuya aplicabilidad a Season 4,
   orden o redondeo permanezca incompleto; abrir un conflicto cuando corresponda.
5. Diseñar casos reproducibles sólo para claims con fórmula suficientemente
   especificada. No materializar datos productivos antes del umbral de
   publicación.

## Evidencias capturadas

### EVD-0022 — MU Online Fanz, stats y aumentos actuales de Dark Wizard

- URL canónica: https://muonlinefanz.com/guide/characters/dw/
- Título/editor: `Dark Wizard - Character Guide`, MU Online Fanz; no se muestra
  autor individual.
- Consulta: 2026-07-24. La página declara actualización del 2025-05-08 y build
  `b2023.09.11.001`.
- Versión declarada: ninguna. La misma página incluye Soul Wizard, cuarta clase,
  nivel máximo 1450 y equipo posterior, por lo que no demuestra Season 4.
- Dato extraído: muestra Stamina inicial 15, HP 60, aumento de 1 HP por nivel de
  personaje y aumento de 1 HP por punto de Stamina.
- Transformación: se conservan como cuatro componentes atómicos de una fórmula
  candidata. No se combinan en una expresión porque la página no aclara si HP 60
  ya incorpora nivel 1 y Stamina inicial, si los incrementos cuentan stats
  totales o aplicados, ni el orden de evaluación.
- Evolución, orden y redondeo: no declarados para la fórmula de HP. La página
  agrupa Dark Wizard, Soul Master, Grand Master y Soul Wizard, pero no prueba que
  el mismo cálculo aplique a cada evolución ni al corte objetivo.
- Condición de uso: `PARTIAL`, sólo investigación y contraste. No autoriza
  fixtures, reglas ni código productivo.
- Licencia/uso conocido: pie `© muonlinefanz.com`; se conserva URL y paráfrasis,
  sin copiar imágenes ni capturar snapshot.
- Snapshot/hash: no capturado.

### EVD-0023 — Webzen, tabla oficial actual de Dark Wizard

- URL canónica: https://muonline.webzen.com/th/gameinfo/guide/detail/6
- Título/editor: `Dark Wizard`, Webzen.
- Consulta: 2026-07-24.
- Versión declarada: ninguna; la ruta actual no atribuye la tabla a Season 4
  global/inglés.
- Dato extraído: HP inicial 60 y `Life Per Level` igual a 1. La tabla no publica
  el incremento por Vitality/Stamina.
- Transformación: confirma únicamente los componentes 60 y 1 por nivel de
  EVD-0022. No completa el coeficiente de stat, la semántica del primer nivel,
  el orden ni el redondeo.
- Condición de uso: `PARTIAL`, contraste oficial actual; no demuestra por sí
  sola continuidad histórica ni una fórmula completa.
- Licencia/uso conocido: documentación del editor oficial; sólo URL,
  transcripción mínima de celdas y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0024 — StrategyWiki, tabla comunitaria de Dark Wizard

- URL canónica:
  https://strategywiki.org/w/index.php?title=Mu_Online/Dark_Wizard&oldid=680744
- Título/editor: `Mu Online/Dark Wizard`, contribuidores de StrategyWiki.
- Consulta: 2026-07-24; revisión permanente editada el 2013-10-13.
- Versión declarada: ninguna. La fecha de edición no prueba que describa Season
  4 ni la región global/inglesa.
- Dato extraído: stats iniciales `18/18/15/30`, Health 60, aumento de Health 1
  por nivel y Life 2 por punto de stat según la tabla.
- Transformación: se interpreta `Life` como el recurso HP sólo para abrir el
  contraste; no se adopta la terminología ni se convierte la tabla en fórmula
  ejecutable. La presentación no especifica base algebraica, orden o redondeo.
- Conflicto: el coeficiente 2 por stat diverge del coeficiente 1 por Stamina de
  EVD-0022; se abre `DSP-0003`.
- Condición de uso: `PARTIAL`, pista comunitaria sin clasificación de temporada.
- Licencia/uso conocido: CC BY-SA 4.0 según el pie de la revisión; sólo URL,
  celdas mínimas y paráfrasis.
- Snapshot/hash: revisión permanente referenciada; no se guardó copia local.

### EVD-0025 — InfinityMU, fórmula explícita de servidor privado

- URL canónica: https://wiki.infinitymu.net/index.php?title=Stats_Formulas
- Título/editor: `Stats Formulas`, InfinityMU.
- Consulta: 2026-07-24; página editada el 2026-02-10.
- Versión declarada: no declara Season 4 global/inglés y pertenece a un servidor
  privado. El sitio enlaza por separado un servidor Season 6, lo que tampoco
  clasifica esta fórmula para el ruleset objetivo.
- Dato extraído: para Dark Wizard/Soul Master/Grand Master publica
  `30 + (Lvl - 1) + (Vit * 2)`.
- Transformación: se conserva la expresión exactamente como variante de
  contraste. Con nivel 1 y Vitality 15 reproduce HP 60, pero esa coincidencia no
  demuestra que sea la fórmula de Season 4 ni resuelve el conflicto con Fanz.
- Orden y redondeo: el orden algebraico es visible; no se declara política de
  redondeo. Para esta expresión concreta todos los términos son enteros.
- Condición de uso: `PARTIAL` como variante privada; prohibido promoverla al
  ruleset estándar o usarla como segunda evidencia independiente suficiente.
- Licencia/uso conocido: copyright de InfinityMU indicado en el pie; sólo URL,
  fórmula mínima necesaria y paráfrasis.
- Snapshot/hash: revisión `oldid=8372` localizada por la página, sin copia local.

### EVD-0026 — Catálogo de fórmulas Season 4 aprobado por el propietario

- Fuente: decisión explícita del propietario comunicada el 2026-07-24.
- Alcance declarado: `mu-s4-global-reference`, Season 4 global/inglés.
- Familias confirmadas:
  - `class-dark-knight`: Dark Knight, Blade Knight y Blade Master.
  - `class-dark-wizard`: Dark Wizard, Soul Master y Grand Master.
  - `class-fairy-elf`: Fairy Elf, Muse Elf y High Elf.
  - `class-magic-gladiator`: Magic Gladiator y Duel Master.
  - `class-dark-lord`: Dark Lord y Lord Emperor.
  - `class-summoner`: Summoner, Bloody Summoner y Dimension Master.
- Confianza y uso permitido: `VERIFIED` como axioma del ruleset por decisión del
  propietario. Autoriza diseñar contratos, IDs y casos de referencia; no permite
  omitir esos gates ni copiar las expresiones directamente al motor.
- Regla de visualización: el juego muestra el resultado truncando la parte
  decimal. Las expresiones se conservan con sus paréntesis y precedencia
  matemática; no se añade ningún redondeo intermedio que no esté escrito.
- Dependencias nombradas: `mana`, `defense` y `AG` remiten al resultado de la
  fórmula homónima de la misma familia. Antes de hacerlas ejecutables, el
  contrato deberá fijar si consume el valor previo o posterior al truncamiento
  visible; esa precisión interna no fue declarada.
- Variables: `lvl` nivel de personaje; `str`, `agi`, `vit`, `ene` y `cmd`
  valores de stat recibidos por el cálculo; `horseLvl` nivel de Dark Horse;
  `ravenLvl` nivel de Dark Raven. El futuro contrato deberá declarar qué
  modificadores forman cada stat de entrada; esta decisión no lo inventa.
- Summoner: el propietario proporcionó daño, rates, defensa, velocidad,
  wizardry, AG y buffs. No proporcionó HP, Mana, SD ni regeneraciones; esos
  campos permanecen desconocidos y no se derivan de otra familia.

#### Familia Dark Knight — fórmula rotulada Blade Knight

```text
min_damage = str / 6
max_damage = str / 4
combo_base = (str + agi + ene) / 2
speed = agi / 15
defense = agi / 3
pvm_defense_rate = agi / 3
pvm_attack_rate = lvl * 5 + (agi * 3) / 2 + str / 4
pvp_defense_rate = lvl * 2 + agi * 0.5
pvp_attack_rate = lvl * 3 + agi * 4.5
hp = 35 + (lvl - 1) * 2 + vit * 3
mana = 10 + (lvl - 1) * 0.5 + ene
mana_regen = mana / 27.5
sd = (str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30
ag = ene + vit * 0.3 + agi * 0.2 + str * 0.15
ag_regen = 2 + ag / 20
skill_percent = 200 + ene / 10
fortitude_percent = 12 + vit / 100 + ene / 20
fenrir_base_min_damage = 45 + str / 3 + agi / 5 + vit / 5 + ene / 6
fenrir_base_max_damage = 75 + str / 3 + agi / 5 + vit / 5 + ene / 6
```

#### Familia Dark Wizard — fórmula rotulada Soul Master

```text
min_wizardry_damage = ene / 9
max_wizardry_damage = ene / 4
speed = agi / 10
defense = agi / 4
pvm_defense_rate = agi / 3
pvm_attack_rate = lvl * 5 + (agi * 3) / 2 + str / 4
pvp_defense_rate = lvl * 2 + agi * 0.25
pvp_attack_rate = lvl * 3 + agi * 4
hp = 30 + (lvl - 1) + vit * 2
mana = (lvl - 1) * 2 + ene * 2
mana_regen = mana / 27.5
sd = (str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30
ag = ene * 0.2 + vit * 0.3 + agi * 0.4 + str * 0.2
ag_regen = 2 + ag / 20
soul_barrier_percent = 10 + agi / 50 + ene / 200
nova_max_spell_damage = 1320 + str / 2
fenrir_base_min_damage = 60 + str / 5 + agi / 5 + vit / 7 + ene / 3
fenrir_base_max_damage = 90 + str / 5 + agi / 5 + vit / 7 + ene / 3
```

#### Familia Fairy Elf — fórmula rotulada Muse Elf

```text
min_damage = str / 14 + agi / 7
max_damage = str / 8 + agi / 4
speed = agi / 50
defense = agi / 10
pvm_defense_rate = agi / 4
pvm_attack_rate = lvl * 5 + (agi * 3) / 2 + str / 4
pvp_defense_rate = lvl * 2 + agi * 0.1
pvp_attack_rate = lvl * 3 + agi * 0.6
hp = 40 + (lvl - 1) + vit * 2
mana = 15 + (lvl - 1) * 1.5 + ene
mana_regen = mana / 27.5
sd = (str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30
ag = ene * 0.2 + vit * 0.3 + agi * 0.2 + str * 0.3
ag_regen = 2 + ag / 33.33
damage_buff = ene / 7 + 3
defense_buff = ene / 8 + 2
heal = ene / 9 + 2
```

#### Familia Magic Gladiator

```text
min_damage = str / 6 + ene / 12
max_damage = str / 4 + ene / 8
min_wizardry_damage = ene / 9
max_wizardry_damage = ene / 4
speed = agi / 15
defense = agi / 5
pvm_defense_rate = agi / 3
pvm_attack_rate = lvl * 5 + (agi * 3) / 2 + str / 4
pvp_defense_rate = lvl * 2 + agi * 0.25
pvp_attack_rate = lvl * 3 + agi * 3.5
hp = 58 + (lvl - 1) + vit * 2
mana = 8 + (lvl - 1) + ene * 2
mana_regen = mana / 27.5
sd = (str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30
ag = ene * 0.15 + vit * 0.3 + agi * 0.25 + str * 0.2
ag_regen = 1.9 + ag / 33
```

#### Familia Dark Lord

```text
min_damage = str / 7 + ene / 14
max_damage = str / 5 + ene / 10
speed = agi / 10
defense = agi / 7
pvm_defense_rate = agi / 7
pvm_attack_rate = lvl * 5 + (agi * 5) / 2 + str / 6 + cmd / 10
pvp_defense_rate = lvl * 2 + agi * 0.5
pvp_attack_rate = lvl * 3 + agi * 4
hp = 50 + (lvl - 1) * 1.5 + vit * 2
mana = 40 + (lvl - 1) + (ene - 15) * 1.5
mana_regen = mana / 27.5
sd = (str + agi + vit + ene + cmd) * 1.2 + defense / 2 + (lvl * lvl) / 30
ag = ene * 0.15 + vit * 0.1 + agi * 0.2 + str * 0.3 + cmd * 0.3
ag_regen = 1.9 + ag / 33
skill_percent = 200 + ene / 20
critical_damage = cmd / 25 + str / 30
fireburst_bonus_min_damage = 100 + str / 25 + ene / 50
fireburst_bonus_max_damage = 150 + str / 25 + ene / 50
horse_bonus_damage = 100 + horseLvl * 10 + lvl * 2.5 + str / 10 + cmd / 5
raven_speed = 20 + (ravenLvl * 4) / 5 + cmd / 50
raven_min_damage = 180 + ravenLvl * 15 + cmd / 8
raven_max_damage = 200 + ravenLvl * 15 + cmd / 4
guild_member_capacity = lvl / 10 + cmd / 10
```

#### Familia Summoner

```text
max_damage = str / 4
min_damage = str / 8
pvm_attack_rate = lvl * 5 + agi * 1.5 + str / 4
pvp_attack_rate = lvl * 3 + agi * 3.5
defense = agi / 3
pvm_defense_rate = agi / 4
pvp_defense_rate = lvl * 2 + agi * 0.5
speed = agi / 20
max_wizardry_damage = ene / 4
min_wizardry_damage = ene / 9
ag = str * 0.2 + agi * 0.25 + vit * 0.3 + ene * 0.15
reflect_percent = 30 + ene / 42
berserker_percent = ene / 30
innovation_percent = ene / 90 + 20
weakness_percent = ene / 65 + 7
```

La ortografía `Beserker` recibida se normaliza únicamente al identificador
`berserker_percent`; la expresión no cambia. No se infiere ninguna fórmula de
recurso ausente a partir de los coeficientes de otras familias.

### EVD-0027 — MU Online Fanz, recursos actuales de Summoner

- URL canónica: https://muonlinefanz.com/guide/characters/sum/
- Título/editor: `Summoner - Character Guide`, MU Online Fanz; no se muestra
  autor individual.
- Consulta: 2026-07-24. La página declara actualización del 2025-05-08 y build
  `b2023.09.11.001`.
- Versión declarada: ninguna. La misma página incluye Dimension Summoner,
  cuarta clase, nivel máximo 1450 y sistemas posteriores, por lo que no
  demuestra Season 4.
- Base publicada: Strength 21, Agility 21, Stamina 18, Energy 23, HP 70,
  Mana 40, AG 18 y SD 102.
- Incrementos publicados: HP aumenta 2 por Stamina y 1 por nivel; Mana aumenta
  1.7 por Energy y 1.5 por nivel; SD aumenta 1.2 por punto de stat, 1.2 por
  nivel y 1 por cada 2 puntos de Defense.
- Transformación: se preservan la base y los incrementos como componentes
  atómicos. No se normalizan a fórmulas como
  `70 + (lvl - 1) + (vit - 18) * 2`, porque la página no declara si los valores
  iniciales ya incorporan stats y nivel, ni dónde aplica truncamiento.
- Contraste interno: aplicar por analogía a Summoner la fórmula de SD aprobada
  para otras familias en EVD-0026 daría, con sus stats base y `defense = agi/3`,
  un valor distinto de 102. Esa fórmula no se traslada ni se abre un conflicto
  formal porque ninguna fuente la atribuye a Summoner.
- Evolución, orden y redondeo: la página agrupa la familia actual; no demuestra
  continuidad hacia Season 4 ni especifica orden o truncamientos.
- Condición de uso: `PARTIAL`, sólo investigación y contraste. No autoriza
  fixtures, reglas ni código productivo.
- Licencia/uso conocido: pie `© muonlinefanz.com`; se conserva URL y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0028 — Webzen, tabla oficial actual de Summoner

- URL canónica: https://muonline.webzen.com/es/gameinfo/guide/detail/90
- Título/editor: `Summoner`, Webzen.
- Consulta: 2026-07-24.
- Versión declarada: ninguna; la navegación actual incluye contenido renovado
  posterior y no atribuye la tabla a Season 4 global/inglés.
- Dato extraído: stats iniciales `21/21/18/23`, HP 70, Mana 40, AG 18 y SD 102;
  publica 1 HP y 1.5 Mana por nivel, pero deja vacío SD por nivel y no publica
  los coeficientes de Stamina/Energy ni una fórmula cerrada.
- Transformación: confirma oficialmente los valores iniciales y los aumentos
  por nivel de HP/Mana de EVD-0027. No completa la semántica de stats, constantes,
  orden, redondeo ni aplicabilidad histórica.
- Condición de uso: `PARTIAL`, contraste oficial actual; no demuestra por sí
  sola continuidad hacia Season 4.
- Licencia/uso conocido: documentación del editor oficial; sólo URL,
  transcripción mínima de celdas y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0029 — MUonline Helper, tabla comunitaria histórica de Summoner

- URL canónica: https://muonlinehelper.blogspot.com/2017/07/summy-stats-build.html
- Título/editor: `SUMMY Stats BUILD`, MUonline Helper; sin autor individual.
- Consulta: 2026-07-24; publicación fechada 2017-07-13.
- Versión declarada: afirma que Summoner fue introducida en Season 3, pero no
  identifica la temporada, región o build de los datos de cálculo publicados.
- Dato extraído: stats iniciales `21/21/18/23`, HP 70, Mana 40, AG 18 y SD 102;
  tabla de aumento por nivel con HP 1, Mana 1.5 y SD sin dato.
- Transformación: sólo contrasta EVD-0028 y la parte de nivel de EVD-0027. No
  aporta fórmula cerrada, coeficientes de stat para estos recursos, orden ni
  redondeo. La coincidencia no demuestra independencia editorial.
- Condición de uso: `PARTIAL`, pista comunitaria histórica sin clasificación
  suficiente para Season 4 global/inglés.
- Licencia/uso conocido: publicación Blogger sin licencia de reutilización
  identificada; sólo URL, celdas mínimas y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0030 — Base y coeficiente de Vitality de HP de Summoner

- Fuente: corrección y decisión explícita del propietario comunicada el
  2026-07-24.
- Alcance: `mu-s4-global-reference`, familia completa Summoner.
- Datos decididos: en nivel 1 con Vitality 18, HP 70; cada punto adicional de
  Vitality aumenta 2 HP y cada nivel aumenta 1 HP.
- Fórmula aprobada: `70 + (lvl - 1) + (vit - 18) * 2`, equivalente a
  `34 + (lvl - 1) + vit * 2`.
- Clasificación: `VERIFIED` como axioma Season 4 por decisión explícita del
  propietario. EVD-0027–EVD-0029 coinciden con base y coeficientes.
- Orden y redondeo: la expresión contiene sólo términos enteros; se conserva la
  regla global de truncamiento visible aunque no altera estos casos.
- Condición de uso: autoriza diseñar contrato y casos de referencia; no permite
  omitir esos gates ni copiar la expresión directamente al motor.

### EVD-0031 — Corrección final del coeficiente de Energy en Mana de Summoner

- Fuente: corrección final explícita del propietario comunicada el 2026-07-24;
  reemplaza el valor 1.5 indicado inmediatamente antes.
- Alcance: `mu-s4-global-reference`, familia completa Summoner.
- Datos decididos: Summoner nace en nivel 1 con Mana 40 y Energy inicial 23;
  Mana aumenta 1.7 por cada punto adicional de Energy y 1.5 por nivel.
- Fórmula aprobada:
  `40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`.
- Clasificación: `VERIFIED` como axioma Season 4 por decisión explícita del
  propietario.
- Contraste: coincide con el +1.7 por Energy publicado por EVD-0027; no existe
  conflicto vigente sobre este coeficiente.
- Orden y redondeo: evaluar la expresión con sus coeficientes decimales y
  truncar la parte decimal sólo en el valor mostrado, conforme a EVD-0026. No se
  añade truncamiento intermedio.
- Condición de uso: autoriza diseñar contrato y casos de referencia; no permite
  omitir esos gates ni copiar la expresión directamente al motor.

### EVD-0032 — Fórmula base de SD de Summoner

- Fuente: decisión explícita del propietario comunicada el 2026-07-24.
- Alcance: `mu-s4-global-reference`, familia completa Summoner.
- Fórmula aportada:
  `(str + agi + vit + ene) * 1.2 + defense / 2 + (lvl * lvl) / 30`.
- Dependencia aportada: para Summoner, `defense = agi / 3`.
- Semántica: Strength, Agility, Vitality y Energy aportan al primer término;
  Defense aporta la mitad de su valor; Level se eleva al cuadrado y se divide
  por 30.
- Clasificación: `VERIFIED` para expresión, variables, versión y orden de
  truncamiento por decisión del propietario.
- Caso de contraste: con stats iniciales `21/21/18/23`, nivel 1 y
  `defense = 21 / 3`, evaluar con precisión completa y truncar sólo el resultado
  da 103. EVD-0027–EVD-0029 publican SD inicial 102.
- Orden aprobado: truncar por separado el término de stats, `defense / 2` y
  `(lvl * lvl) / 30`, y después sumar los tres enteros:
  `trunc((str + agi + vit + ene) * 1.2) + trunc(defense / 2) + trunc((lvl * lvl) / 30)`.
- Caso aprobado: nivel 1 y stats `21/21/18/23` produce
  `trunc(99.6) + trunc(3.5) + trunc(1/30) = 99 + 3 + 0 = 102`.
- Condición de uso: autoriza diseñar contrato y casos de referencia; no permite
  omitir esos gates ni copiar la expresión directamente al motor.

### EVD-0033 — Etapa de Defense consumida por SD de Dark Wizard

- Fuente: decisión explícita del propietario comunicada el 2026-07-28.
- Pregunta cerrada: si `DR-SD-DARK-WIZARD` consume la salida `RAW` o `VISIBLE`
  de `defense = agi / 4`.
- Decisión: consumir `RAW`, es decir, el cociente decimal previo al truncamiento
  visible de Defense.
- Alcance: exclusivamente `formula-sd-dark-wizard` dentro de
  `mu-s4-global-reference`; no decide la etapa de dependencias de otras
  familias o atributos.
- Clasificación: `VERIFIED` como axioma del ruleset por decisión explícita del
  propietario.
- Consecuencia: SD puede depender por referencia exacta de
  `formula-defense-dark-wizard` `1.0.0` con `outputStage: RAW`. Defense conserva
  a la vez su propia salida visible truncada para presentación.
- Condición de uso: autoriza cerrar contrato, casos y ejecución de Defense/SD
  de Dark Wizard; no altera la clasificación individual de `EVD-0021` o
  `EVD-0026`.

### EVD-0034 — Etapa de Defense para las fórmulas de SD restantes

- Fuente: decisión explícita del propietario comunicada el 2026-07-28.
- Pregunta cerrada: si las fórmulas de SD todavía no materializadas consumen la
  salida `RAW` o `VISIBLE` de su fórmula Defense de la misma familia.
- Decisión: consumir `RAW`, es decir, el valor decimal anterior al truncamiento
  de la salida visible de Defense.
- Alcance: `DR-SD-DARK-KNIGHT`, `DR-SD-FAIRY-ELF`,
  `DR-SD-MAGIC-GLADIATOR`, `DR-SD-DARK-LORD` y `DR-SD-SUMMONER` dentro de
  `mu-s4-global-reference`. `EVD-0033` continúa siendo la decisión específica
  ya aplicada a Dark Wizard.
- Clasificación: `VERIFIED` como axioma del ruleset por decisión explícita del
  propietario.
- Consecuencia: cada SD del alcance debe depender por referencia exacta de la
  versión publicada de Defense de su propia familia con `outputStage: RAW`.
  Esta decisión no cambia las reglas particulares de evaluación o truncamiento
  ya fijadas para cada fórmula, incluida `EVD-0032` para Summoner.
- Evolución: si la observación futura demuestra que una familia requiere
  `VISIBLE`, el cambio deberá registrarse como nueva evidencia y nueva versión
  de fórmula con casos que hagan visible la diferencia; no se reescribe
  silenciosamente esta decisión.
- Condición de uso: autoriza materializar una vertical Defense/SD por vez,
  manteniendo fórmula, versión, etapa, traza productora y casos explícitos.

## Bitácora de investigación

### 2026-07-29 — Materialización productiva de Defense y SD de Dark Lord

- `formula-defense-dark-lord` y `formula-sd-dark-lord` `1.0.0` se
  materializan `PUBLISHED` exclusivamente desde `EVD-0021`, `EVD-0026` y
  `EVD-0034`.
- Defense conserva `agility / 7` y enlaza cuatro positivos y dos controles.
- SD conserva Command en la suma de stats y enlaza cuatro positivos. Su frontera
  fija Agility 25, Defense `RAW=3.571428571…`/`VISIBLE=3` y SD
  `135.019047619…/135`, frente a 134 si consumiera la salida visible.
- Ocho controles cubren nivel, los cinco stats bajo base, familia y overflow.
  No se añadió evidencia, conflicto ni truncamiento intermedio.

### 2026-07-29 — Materialización productiva de Defense y SD de Magic Gladiator

- `formula-defense-magic-gladiator` y `formula-sd-magic-gladiator` `1.0.0` se
  materializan `PUBLISHED` exclusivamente desde `EVD-0021`, `EVD-0026` y
  `EVD-0034`.
- Defense conserva `agility / 5` y enlaza cuatro positivos y dos controles.
- SD enlaza cuatro positivos; su frontera fija Agility 28, Defense
  `RAW=5.6`/`VISIBLE=5` y SD `130.033333…/130`, frente a 129 si consumiera la
  salida visible.
- Siete controles cubren nivel, los cuatro stats, familia y overflow.
  `DSP-0002` permanece resuelto y trazado porque SD consume Energy 26; no se
  añadió evidencia, conflicto ni truncamiento intermedio.

### 2026-07-29 — Materialización productiva de Defense y SD de Fairy Elf

- `formula-defense-fairy-elf` y `formula-sd-fairy-elf` `1.0.0` se
  materializan `PUBLISHED` exclusivamente desde `EVD-0021`, `EVD-0026` y,
  para la etapa de dependencia, `EVD-0034`.
- Defense conserva `agility / 10`, enlaza cuatro positivos `2.5/2`, `2.6/2`,
  `3/3` y `3.5/3`, y dos controles para Agility bajo base y familia.
- SD enlaza cuatro positivos. `sd-fairy-elf-raw-defense-boundary` fija nivel 1,
  Agility 27, Defense `RAW=2.7`/`VISIBLE=2` y SD
  `RAW=102.183333…`/`VISIBLE=102`; consumir Defense visible produciría 101.
- Siete controles de SD cubren nivel, los cuatro stats bajo base, familia y
  overflow. No se añadió evidencia, conflicto ni redondeo intermedio; las
  fórmulas conservan `conflictIds: []`.

### 2026-07-28 — Materialización productiva de Defense y SD de Dark Knight

- `EVD-0034` registra la decisión del propietario para las cinco familias de SD
  pendientes: todas consumen la salida `RAW` de su Defense de la misma familia.
- `formula-defense-dark-knight` y `formula-sd-dark-knight` `1.0.0` se
  materializan `PUBLISHED` exclusivamente desde `EVD-0021`, `EVD-0026` y,
  para la etapa de dependencia, `EVD-0034`.
- Defense enlaza cuatro positivos que conservan `agility / 3`, incluido el
  cociente periódico decimal, y dos controles para Agility bajo base y familia.
- SD enlaza cuatro positivos. El caso
  `sd-dark-knight-raw-defense-boundary` fija nivel 1, Agility 23,
  Defense `RAW=7.6666…`/`VISIBLE=7` y SD `107.0666…/107`; consumir Defense
  visible produciría 106 y queda rechazado.
- Siete controles de SD cubren nivel, los cuatro stats bajo base, familia y
  overflow. No se añadió un conflicto inexistente ni se modificaron las reglas
  particulares todavía pendientes de otras familias.

### 2026-07-28 — Materialización productiva de Defense y SD de Dark Wizard

- `EVD-0033` registra la elección explícita del propietario: SD consume la
  salida `RAW` de `defense = agi / 4`.
- `formula-defense-dark-wizard` y `formula-sd-dark-wizard` `1.0.0` se
  materializan `PUBLISHED` exclusivamente desde `EVD-0021`, `EVD-0026` y,
  para la etapa de dependencia, `EVD-0033`.
- Defense enlaza cuatro positivos `4.5/4`, `4.75/4`, `5/5` y `5.25/5`; dos
  controles cubren Agility bajo base y familia ajena.
- SD enlaza cuatro positivos. El caso
  `sd-dark-wizard-raw-defense-boundary` fija nivel 4, Agility 19,
  Defense `RAW=4.75`/`VISIBLE=4` y SD `101.3083…/101`; consumir Defense visible
  produciría 100 y queda rechazado por el caso.
- Siete controles de SD cubren nivel, los cuatro stats bajo base, familia y
  overflow de salida. No se añadió un conflicto inexistente.
- `DIVIDE` se incorpora sólo a `CHECKED_DECIMAL_V1` para conservar las
  divisiones escritas por 4, 2 y 30 sin sustituir `1/30` por una constante
  aproximada. La dependencia conserva referencia, etapa, valor y traza
  productora.

### 2026-07-28 — Materialización productiva de AG de Dark Lord

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-AG-DARK-LORD` se materializó exclusivamente desde `EVD-0021` y
  `EVD-0026` como `formula-ag-dark-lord` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta los mínimos factuales Energy 15, Vitality 20, Agility 20,
  Strength 26 y Command 25; los máximos de tipos permanecen límites técnicos.
- Cuatro positivos fijan resultados crudos/visibles
  `23.55/23`, `23.80/23`, `24.35/24` y `24.60/24`. Siete controles cubren los
  cinco stats por debajo de su base, familia y overflow.
- `CHECKED_DECIMAL_V1` conserva `0.15`, `0.1`, `0.2` y `0.3` exactamente, no
  redondea aportes y trunca una sola vez en `visible-ag`. El overflow es
  alcanzable: la suma de coeficientes `1.05` aplicada a cinco entradas
  `Int64.MaxValue` supera la salida `INT64`.
- La fórmula no consume nivel ni dependencias. `resolved-command` usa la misma
  ruta contextual genérica que los otros stats. No existe conflicto aplicable
  conocido y la traza conserva `conflictIds: []`.

### 2026-07-28 — Materialización productiva de AG de Magic Gladiator

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-AG-MAGIC-GLADIATOR` se materializó desde `EVD-0021`, `EVD-0026` y el
  alcance aplicable de `DSP-0002` como `formula-ag-magic-gladiator` `1.0.0`,
  ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta los mínimos factuales Strength, Agility, Vitality y
  Energy 26; `DSP-0002` permanece resuelto a favor de Energy 26 y conserva la
  divergencia documental que publica 16.
- Cuatro positivos fijan resultados crudos/visibles
  `23.40/23`, `23.85/23`, `23.85/23` y `24.30/24`. Cinco controles cubren los
  cuatro stats por debajo de su base y familia.
- `CHECKED_DECIMAL_V1` conserva `0.15`, `0.3`, `0.25` y `0.2` exactamente, no
  redondea aportes y trunca una sola vez en `visible-ag`. No existe un caso de
  overflow válido que materializar: la suma de coeficientes `0.9` mantiene la
  salida dentro de `INT64` para cuatro entradas `Int64.MaxValue`.
- La fórmula no consume nivel ni dependencias. La traza conserva
  `conflictIds: ["dsp-0002"]`.

### 2026-07-28 — Materialización productiva de AG de Summoner

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-AG-SUMMONER` se materializó exclusivamente desde `EVD-0026` como
  `formula-ag-summoner` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta los mínimos factuales Strength 21, Agility 21,
  Vitality 18 y Energy 23; los máximos de tipos permanecen límites técnicos.
- Cuatro positivos fijan resultados crudos/visibles
  `18.30/18`, `18.75/18`, `18.75/18` y `19.20/19`. Cinco controles negativos
  cubren los cuatro stats por debajo de su base y una familia ajena.
- Los coeficientes `0.2`, `0.25`, `0.3` y `0.15` se conservan exactamente
  mediante `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento
  se aplica una sola vez en `visible-ag`.
- No se inventó un control de overflow: con inputs válidos no negativos, la
  suma de coeficientes `0.9` mantiene la salida dentro de `INT64` incluso para
  cuatro entradas `Int64.MaxValue`.
- La fórmula no consume nivel ni dependencias. No existe conflicto aplicable
  conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de AG de Dark Knight

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-AG-DARK-KNIGHT` se materializó exclusivamente desde `EVD-0026` como
  `formula-ag-dark-knight` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta los mínimos factuales Energy 10, Vitality 25,
  Agility 20 y Strength 28; los máximos de tipos permanecen límites técnicos.
- Cuatro positivos fijan resultados crudos/visibles
  `25.70/25`, `27.00/27`, `26.05/26` y `27.35/27`. Seis controles negativos
  cubren los cuatro stats por debajo de su base, familia y overflow de salida
  `INT64`.
- Los coeficientes `0.3`, `0.2` y `0.15` se conservan exactamente mediante
  `CHECKED_DECIMAL_V1`; Energy entra sin transformación, no existe redondeo
  intermedio y el truncamiento se aplica una sola vez en `visible-ag`.
- La fórmula no consume nivel ni dependencias. No existe conflicto aplicable
  conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de AG de Dark Wizard

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-AG-DARK-WIZARD` se materializó exclusivamente desde `EVD-0026` como
  `formula-ag-dark-wizard` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta los mínimos factuales Energy 30, Vitality 15,
  Agility 18 y Strength 18; los máximos de tipos permanecen límites técnicos.
- Cuatro positivos fijan resultados crudos/visibles
  `21.3/21`, `21.8/21`, `21.9/21` y `22.4/22`. Seis controles negativos cubren
  los cuatro stats por debajo de su base, familia y overflow de salida `INT64`.
- Los coeficientes `0.2`, `0.3` y `0.4` se conservan exactamente mediante
  `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento se
  aplica una sola vez en `visible-ag`.
- La fórmula no consume nivel ni dependencias. No existe conflicto aplicable
  conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de Mana de Dark Lord

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-MANA-DARK-LORD` se materializó exclusivamente desde `EVD-0026` como
  `formula-mana-dark-lord` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Energy 15; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan resultados crudos/visibles
  `40/40`, `41/41`, `41.5/41` y `42.5/42`. Cuatro controles negativos cubren
  nivel, Energy, familia y overflow de la salida `INT64`.
- El coeficiente `1.5` se conserva exactamente mediante
  `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento se
  aplica una sola vez en `visible-mana`.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de Mana de Magic Gladiator

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-MANA-MAGIC-GLADIATOR` se materializó exclusivamente desde `EVD-0026`
  como `formula-mana-magic-gladiator` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Energy 26; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan 60/61/62/63 Mana para los aportes aislados y
  combinados; cuatro controles negativos cubren nivel, Energy, familia y
  overflow.
- La expresión se conserva exactamente como `8 + (lvl - 1) + ene * 2`: no se
  introduce desplazamiento de Energy ni redondeo intermedio, y el truncamiento
  se aplica una sola vez al resultado crudo.
- `DSP-0002` es aplicable porque la fórmula consume Energy y se conserva
  resuelto por decisión del propietario a favor de 26; la divergencia documental
  que publica 16 no se reescribe.
- La implementación reutiliza el intérprete y la resolución contextual
  genéricos; no incorpora handlers ni constantes de Magic Gladiator en C#.

### 2026-07-26 — Materialización productiva de Mana de Summoner

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-MANA-SUMMONER` se materializó desde `EVD-0021`, `EVD-0026`,
  `EVD-0027`–`EVD-0029` y `EVD-0031` como
  `formula-mana-summoner` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Energy 23; `EVD-0031` conserva la
  expresión y autoridad final. `EVD-0030` se limita a HP y no se hereda.
- Cuatro positivos fijan resultados crudos/visibles
  `40/40`, `41.5/41`, `41.7/41` y `43.2/43`. Cuatro controles negativos cubren
  nivel, Energy, familia y overflow de la salida `INT64`.
- Los coeficientes `1.5` y `1.7` se conservan exactamente mediante
  `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento se
  aplica una sola vez en `visible-mana`.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de Mana de Fairy Elf

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-MANA-FAIRY-ELF` se materializó exclusivamente desde `EVD-0026` como
  `formula-mana-fairy-elf` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Energy 15; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan resultados crudos/visibles
  `30/30`, `31.5/31`, `31/31` y `32.5/32`. Cuatro controles negativos cubren
  nivel, Energy, familia y overflow de la salida `INT64`.
- El coeficiente `1.5` se conserva exactamente mediante
  `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento se
  aplica una sola vez en `visible-mana`.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de Mana de Dark Knight

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-MANA-DARK-KNIGHT` se materializó exclusivamente desde `EVD-0026` como
  `formula-mana-dark-knight` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Energy 10; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan resultados crudos/visibles
  `20/20`, `20.5/20`, `21/21` y `21.5/21`. Cuatro controles negativos cubren
  nivel, Energy, familia y overflow de la salida `INT64`.
- El coeficiente `0.5` se conserva exactamente mediante
  `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento se
  aplica una sola vez en `visible-mana`.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de AG de Fairy Elf

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-AG-FAIRY-ELF` se materializó exclusivamente desde `EVD-0026` como
  `formula-ag-fairy-elf` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta los mínimos factuales Energy 15, Vitality 20, Agility 25
  y Strength 22; los máximos de tipos permanecen límites técnicos.
- Cuatro positivos fijan resultados crudos/visibles
  `20.6/20`, `21.1/21`, `21.1/21` y `21.6/21`. Seis controles cubren los
  cuatro stats por debajo de su base, familia y overflow.
- `CHECKED_DECIMAL_V1` conserva `0.2` y `0.3` exactamente, no redondea aportes
  y trunca una sola vez en `visible-ag`. No existe dependencia ni input de
  nivel.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de Mana de Dark Wizard

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-MANA-DARK-WIZARD` se materializó exclusivamente desde `EVD-0026` como
  `formula-mana-dark-wizard` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Energy 30; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan 60/62/62/64 Mana para los aportes aislados y
  combinados; cuatro controles cubren nivel, Energy, familia y overflow.
- La expresión se conserva sin una constante base inferida y se ejecuta con
  `CHECKED_INT64_V1`; no existe redondeo intermedio y el truncamiento se aplica
  una sola vez al resultado crudo.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de HP de Dark Lord

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-HP-DARK-LORD` se materializó exclusivamente desde `EVD-0026` como
  `formula-hp-dark-lord` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Vitality 20; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan resultados crudos/visibles
  `90/90`, `91.5/91`, `92/92` y `93.5/93`. Cuatro controles negativos cubren
  nivel, Vitality, familia y overflow de la salida `INT64`.
- El coeficiente `1.5` se conserva exactamente mediante
  `CHECKED_DECIMAL_V1`; no existe redondeo intermedio y el truncamiento se aplica
  una sola vez al resultado crudo.
- No existe conflicto aplicable conocido y la traza conserva `conflictIds: []`.

### 2026-07-26 — Materialización productiva de HP de Magic Gladiator

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-HP-MAGIC-GLADIATOR` se materializó exclusivamente desde `EVD-0026` como
  `formula-hp-magic-gladiator` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Vitality 26; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan 110/111/112/113 HP para los aportes aislados y
  combinados aprobados. Cuatro controles negativos cubren nivel, Vitality,
  familia y overflow.
- `DSP-0002` sólo afecta Energy y no es aplicable a esta fórmula de nivel y
  Vitality; la traza conserva `conflictIds: []`.
- La implementación reutiliza el intérprete y la resolución contextual
  genéricos; no incorpora handlers ni constantes de Magic Gladiator en C#.

### 2026-07-26 — Materialización productiva de HP de Summoner

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-HP-SUMMONER` se materializó exclusivamente desde
  `EVD-0021`/`EVD-0027`–`EVD-0030` como `formula-hp-summoner` `1.0.0`,
  ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Vitality 18; `EVD-0030` autoriza
  expresión, alcance y redondeo. `EVD-0027`–`EVD-0029` permanecen como
  contraste con su clasificación individual intacta.
- Cuatro positivos fijan 70/71/72/73 HP para los aportes aislados y combinados
  aprobados. Cuatro controles negativos cubren nivel, Vitality, familia y
  overflow. No existe conflicto aplicable conocido y la traza conserva
  `conflictIds: []`.
- La implementación reutiliza el intérprete y la resolución contextual
  genéricos; no incorpora handlers ni constantes de Summoner en C#.

### 2026-07-25 — Materialización productiva de HP de Fairy Elf

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-HP-FAIRY-ELF` se materializó exclusivamente desde `EVD-0026` como
  `formula-hp-fairy-elf` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Vitality 20; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan 80/81/82/83 HP para los aportes aislados y combinados
  aprobados. Cuatro controles negativos cubren nivel, Vitality, familia y
  overflow. No existe conflicto aplicable conocido y la traza conserva
  `conflictIds: []`.
- La implementación reutiliza el intérprete y la resolución contextual
  genéricos; no incorpora handlers ni constantes de Fairy Elf en C#.

### 2026-07-25 — Materialización productiva de HP de Dark Knight

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- `DR-HP-DARK-KNIGHT` se materializó exclusivamente desde `EVD-0026` como
  `formula-hp-dark-knight` `1.0.0`, ejecutable y `PUBLISHED`.
- `EVD-0021` sustenta el mínimo factual de Vitality 25; los máximos de tipos
  permanecen límites técnicos y no máximos factuales del juego.
- Cuatro positivos fijan 110/112/113/115 HP para los aportes aislados y
  combinados aprobados. Cuatro controles negativos cubren nivel, Vitality,
  familia y overflow. No existe conflicto aplicable conocido y la traza
  conserva `conflictIds: []`.
- La implementación reutiliza el intérprete y la resolución contextual
  genéricos; no incorpora handlers ni constantes de Dark Knight en C#.

### 2026-07-25 — Publicación del contrato de HP de Dark Wizard

- No se añadió evidencia, fórmula ni claim, y no se reclasificó ninguna fuente.
- Se revisaron la definición canónica y sus ocho casos contra el contrato
  aprobado, `EVD-0021`, `EVD-0026` y `DSP-0003`, sin encontrar divergencias.
- Los cuatro positivos permanecen enlazados y los cuatro controles negativos
  separados; el gate semántico y la prueba de contrato fijan el estado
  `PUBLISHED`.
- Sólo cambió el estado de publicación. El motor, Application, Data y WPF no
  materializan ni ejecutan todavía la fórmula.

### 2026-07-24 — Fórmula base de SD de Summoner

- El propietario aportó la expresión de SD y confirmó `defense = agi / 3`.
- El propietario confirmó SD visible 102 en nivel 1/stats base. Se aprueba
  truncar independientemente los tres términos antes de sumarlos.
- `DSP-0004` queda resuelto, `DR-SD-SUMMONER` pasa a `VERIFIED` y `RES-0002`
  cierra 24/24 claims. No se modifica el motor.

### 2026-07-24 — Corrección final de Energy para Mana de Summoner

- El propietario retiró el valor 1.5 comunicado inmediatamente antes y fijó
  definitivamente +1.7 Mana por cada punto de Energy para Season 4.
- La decisión coincide con EVD-0027; no se abre un conflicto.
- El propietario confirmó después que Summoner nace en nivel 1 con Mana 40 y
  Energy inicial 23.
- El propietario confirmó finalmente +1.5 Mana por nivel. Queda aprobada
  `mana = 40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`.
- `DR-MANA-SUMMONER` pasa de `PARTIAL` a `VERIFIED`. No se modificaron schemas,
  ruleset, casos ejecutables ni motor.

### 2026-07-24 — Cierre de HP de Summoner

- El propietario fijó nivel 1/Vitality 18 = HP 70, +2 HP por cada punto
  adicional de Vitality y +1 HP por nivel.
- La decisión coincide con EVD-0027–EVD-0029 y fija
  `hp = 70 + (lvl - 1) + (vit - 18) * 2`.
- `DR-HP-SUMMONER` pasa de `PARTIAL` a `VERIFIED`. No se modificaron schemas,
  ruleset, casos ejecutables ni motor.

### 2026-07-24 — Investigación de recursos faltantes de Summoner

- MU Online Fanz, Webzen y MUonline Helper coinciden en stats y recursos
  iniciales de Summoner: `21/21/18/23`, HP 70, Mana 40, AG 18 y SD 102.
- Las tres fuentes coinciden en 1 HP y 1.5 Mana por nivel. Sólo Fanz publica
  además 2 HP por Stamina, 1.7 Mana por Energy y componentes de crecimiento de
  SD; su página actual mezcla evoluciones y sistemas posteriores a Season 4.
- Ninguna evidencia clasifica una fórmula cerrada como Season 4 global/inglés
  ni aclara si la base ya incorpora stats/nivel, orden o truncamientos. Por ello
  `DR-HP-SUMMONER`, `DR-MANA-SUMMONER` y `DR-SD-SUMMONER` pasan de
  `UNVERIFIED` a `PARTIAL`, no a `VERIFIED`.
- No se añadieron expresiones a EVD-0026 ni se modificaron schemas, ruleset,
  casos ejecutables o motor.

### 2026-07-24 — Axioma de fórmulas Season 4 del propietario

- El propietario declaró que el catálogo EVD-0026 corresponde a Season 4
  global/inglés, debe registrarse como decisión y aplica a las familias completas
  enumeradas.
- El resultado visible trunca la parte decimal. No se añadió una política de
  redondeo intermedio ni una fórmula que no estuviera en la decisión.
- Quedan `VERIFIED` 21/24 claims de recursos: cuatro para cinco familias y AG
  para Summoner. En ese cierre, HP, Mana y SD de Summoner permanecían
  `UNVERIFIED`; EVD-0027–EVD-0029 los reclasifican después como `PARTIAL`.
- `DSP-0003` queda resuelto por decisión del propietario a favor del coeficiente
  2 de Vitality en HP de Dark Wizard, sin alterar la clasificación individual
  de EVD-0022–EVD-0025.
- Las fórmulas adicionales se preservan como axiomas de investigación para
  futuros contratos. No se modificaron schemas, ruleset, casos ejecutables ni
  motor en esta tarea.

### 2026-07-24 — Primer contraste de HP para Dark Wizard

- MU Online Fanz aporta HP 60, 1 por nivel y 1 por Stamina, pero su página
  actual mezcla contenido posterior a Season 4 y omite base algebraica,
  evolución aplicable, orden y redondeo.
- Webzen confirma actualmente HP 60 y 1 por nivel, sin publicar el coeficiente
  de Vitality/Stamina ni atribuir la tabla a Season 4.
- StrategyWiki e InfinityMU publican 2 por Vitality; la segunda fuente explicita
  una fórmula de servidor privado que reproduce 60 en nivel 1 con Vitality 15.
- Se registran EVD-0022–EVD-0025, se abre `DSP-0003` y sólo
  `DR-HP-DARK-WIZARD` pasa a `PARTIAL`. No se crean fórmulas canónicas, casos
  ejecutables ni cambios de schemas, ruleset o motor.

### 2026-07-24 — Apertura del registro

- Se crearon 24 claims: una combinación por cada uno de los cuatro atributos y
  las seis familias de clase ya canónicas.
- Todos comienzan `UNVERIFIED`, sin evidencia, cifras, constantes ni fórmulas.
- No se modificaron schemas, ruleset, casos de referencia ni motor de cálculo.
