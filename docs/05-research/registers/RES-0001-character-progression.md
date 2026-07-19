# RES-0001 — Clases y progresión base de personajes

## Registro

```yaml
id: RES-0001
question: "¿Cuáles son, para cada clase del ruleset de referencia de MU Online Season 4 global/inglés, sus estadísticas iniciales, estadísticas disponibles, evoluciones, puntos por nivel y reglas aplicables de Marlon?"
scope:
  season: "4"
  class: null
  mode: "general"
  ruleset: "mu-s4-global-reference"
status: PARTIAL
claims:
  - id: CLM-0001
    statement: "El conjunto de clases jugables del ruleset de referencia está identificado."
    status: PARTIAL
    evidence: [EVD-0001, EVD-0002, EVD-0005, EVD-0006, EVD-0007, EVD-0008, EVD-0009, EVD-0011, EVD-0012]
  - id: CLM-0002
    statement: "Las estadísticas iniciales de cada clase están identificadas."
    evidence: []
  - id: CLM-0003
    statement: "Las estadísticas disponibles para distribuir en cada clase están identificadas."
    evidence: []
  - id: CLM-0004
    statement: "La cadena de evoluciones de cada clase está identificada."
    status: PARTIAL
    evidence: [EVD-0002, EVD-0003, EVD-0004, EVD-0005, EVD-0006, EVD-0007, EVD-0008, EVD-0009, EVD-0011, EVD-0012, EVD-0013]
  - id: CLM-0005
    statement: "Los puntos otorgados por nivel para cada clase y etapa están identificados."
    evidence: []
  - id: CLM-0006
    statement: "La disponibilidad y el efecto de Marlon para cada clase y etapa están identificados."
    evidence: []
conflicts:
  - id: DSP-0001
    statement: "MU Online Fanz nombra Dark Wizard → Soul Wizard en la página de segunda clase, aunque sus páginas de tercera y cuarta clase sitúan Soul Master → Grand Master → Soul Wizard."
    evidence: [EVD-0011, EVD-0012, EVD-0013]
    scope: "Páginas actuales y primeras capturas disponibles de 2023; aplicabilidad a Season 4 no demostrada."
    impact: "Impide usar EVD-0011 como evidencia de la etapa posterior a Dark Wizard; EVD-0012/EVD-0013 aclaran el orden actual, pero no la frontera histórica de Season 4."
    status: OPEN
test_plan: "Contrastar cada celda factual con dos evidencias independientes o con una prueba reproducible aprobada; verificar explícitamente temporada, etapa, límites y condiciones antes de promoverla."
conclusion: "Investigación abierta para Season 4 global/inglés. La cadena candidata del mago para el alcance objetivo es Dark Wizard → Soul Master → Grand Master; el propietario clasifica Grand Master como la evolución incorporada en Season 4, obtenida al completar la serie de quests de tercera clase culminada a nivel 400. Soul Wizard queda clasificado como evolución posterior. MU Online Fanz confirma la estructura actual de tercera/cuarta clase, pero no demuestra la frontera histórica de Season 4. Todos los claims continúan PARTIAL. No hay stats, puntos ni reglas de Marlon autorizados para implementación."
reviewed_by: []
last_reviewed_at: "2026-07-18"
```

## Evidencias capturadas

### EVD-0001 — Guía archivada de actualización Season 4

- URL canónica: https://lostmu.wiki/guide/2008-08-26-season-4-character-update/
- Título/editor: `SEASON4 Character Update 2008.08.26`, LostMu.wiki.
- Consulta: 2026-07-18.
- Versión declarada: `SEASON4`; fecha interna 2008-08-26.
- Dato extraído: enumera habilidades para Blade Knight, Soul Master, Muse Elf,
  Magic Gladiator y Dark Lord, y una sección adicional para Summoner. Respalda
  las seis familias candidatas, pero no enumera todas sus evoluciones.
- Transformación: normalización de nombres; no se extrajeron cifras ni fórmulas.
- Tipo/independencia: espejo comunitario de una guía histórica atribuida a una
  actualización de Webzen; independiente de EVD-0002, pero no es una fuente
  primaria preservada.
- Condición de uso: página pública; sólo se conserva referencia y paráfrasis.
- Snapshot/hash: no capturado; pendiente de confirmar legalidad y estabilidad.

### EVD-0002 — Documentación de un servidor Season 4

- URL canónica: https://www.terra.mu/page.php?al=profession
- Título/editor: `Profesiones`, Terra MU Online.
- Consulta: 2026-07-18.
- Versión declarada: el sitio se identifica como `Servidor MuOnline Season 4`.
- Dato extraído: Dark Knight → Blade Knight → Blade Master; Dark Wizard → Soul
  Master → Grand Master; Fairy Elf → Muse Elf → High Elf; Summoner → Bloody
  Summoner → Dimension Master; Magic Gladiator → Duel Master; Dark Lord → Lord
  Emperor.
- Transformación: transcripción y normalización ortográfica; las familias sin
  segunda etapa no reciben una etapa inventada.
- Tipo/independencia: implementación privada, independiente de EVD-0001; no
  demuestra por sí misma el ruleset oficial de referencia.
- Condición de uso: página pública; sólo se conserva referencia y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0003 — Segunda implementación Season 4

- URL canónica: https://risemu.net/
- Título/editor: `RiseMU - Servidor de Mu Online Season 4`, RiseMU.
- Consulta: 2026-07-18.
- Versión declarada: `Season 4` en el título y descripción del sitio.
- Dato extraído: rankings y eventos muestran Grand Master, Blade Master, High
  Elf, Duel Master, Lord Emperor y Dimension Master, además de las seis familias
  SM/BM/ELF/MG/DL/SUM.
- Transformación: corroboración de existencia; no se infieren quests ni reglas.
- Tipo/independencia: operador distinto de Terra; puede compartir archivos de
  servidor y no cuenta como prueba primaria del comportamiento oficial.
- Condición de uso: página pública; sólo se conserva referencia y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0004 — Nombres oficiales de terceras clases, versión posterior

- URL canónica: https://muonline.webzen.com/en/events/EX700Guide/pop_master
- Título/editor: `Completion of Master Skill`, Webzen.
- Consulta: 2026-07-18.
- Versión declarada: guía `EX700`; versión posterior, no aplicable directamente a Season 4.
- Dato extraído: enumera Blade Master, Grand Master, High Elf, Dimension Master,
  Duel Master y Lord Emperor, con erratas de traducción en algunos rótulos.
- Transformación: normalización contra los encabezados de la misma página.
- Tipo/independencia: fuente oficial posterior; contrasta nombres, pero no
  atribuye su disponibilidad a Season 4.
- Condición de uso: página pública oficial; sólo referencia y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0005 — Tabla técnica contemporánea de códigos de clase

- URL canónica: https://forum.ragezone.com/threads/how-to-change-the-class-code-on-muweb-0-9.636632/
- Título/editor: `How to change the class code on MuWeb 0.9`; hilo iniciado por
  `darknessgod_565` y respuesta técnica publicada por `xute` en RaGEZONE.
- Fecha publicada: 2010-01-09; consulta: 2026-07-18.
- Versión declarada: no declara Season 4; la fecha cae dentro de la ventana
  lanzamiento global de Season 4 y el anuncio oficial de Season 4.5.
- Dato extraído: tabla de códigos que enumera Dark Wizard → Soul Master → Grand
  Master; Dark Knight → Blade Knight → Blade Master; Elf → Muse Elf → High Elf;
  Magic Gladiator → Duel Master; Dark Lord → Lord Emperor; y Summoner → Bloody
  Summoner → Dimension Master.
- Transformación: se interpretaron únicamente las asociaciones nombre-código;
  no se adoptaron los códigos ni se normalizó `Elf` a `Fairy Elf` como hecho.
- Artefacto/procedencia: fragmento de configuración de `func_muweb.inc.php` de
  MuWeb 0.9 pegado en el foro; no se adjunta una distribución versionada de
  MuWeb, un cliente ni un MuServer del que puedan reproducirse los valores.
- Tipo/independencia: registro técnico comunitario contemporáneo. El foro y el
  usuario son distintos de EVD-0002/EVD-0003, pero no se demostró la autoría
  original del fragmento ni su independencia respecto de distribuciones
  compartidas. No demuestra región ni aplicabilidad a Season 4 global/inglés.
- Condición de uso: foro público; sólo se conserva referencia y paráfrasis.
- Snapshot/hash: no capturado; el buscador devolvió el contenido indexado, pero
  la apertura directa falló durante la consulta.

### EVD-0006 — Guía oficial de quests y cambios de clase

- URL canónica: https://muonline.webzen.com/en/gameinfo/guide/detail/83
- Título/editor: `Quest for Beginners`, Webzen.
- Consulta: 2026-07-18.
- Versión declarada: la sección de cambios de clase no declara temporada; una
  sección posterior, separada, se titula `MU Season 6 New Quest`.
- Dato extraído: la primera quest convierte Dark Knight en Blade Knight, Dark
  Wizard en Soul Master y Elf en Muse Elf. La tabla de tercera clase enumera
  Dark Knight → Blade Knight → Blade Master; Dark Wizard → Soul Master → Grand
  Master; Elf → Muse Elf → High Elf; Magic Gladiator → sin etapa intermedia →
  Duel Master; y Dark Lord → sin etapa intermedia → Lord Emperor.
- Transformación: se dividió la tabla en claims atómicos por familia; el literal
  `- none` se conserva como ausencia declarada de etapa intermedia y `Elf` no se
  normaliza a `Fairy Elf` como hecho de esta fuente.
- Tipo/independencia: documentación oficial prioritaria de Webzen; independiente
  de las implementaciones privadas, pero sin alcance de versión declarado.
- Confianza y uso permitido: `PARTIAL`; confirma nombres y asociaciones de cinco
  familias en una versión oficial no fijada. No demuestra por sí sola Season 4
  global/inglés y omite Summoner de la tabla de tercera clase.
- Condición de uso: página pública oficial; sólo se conserva referencia y
  paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0007 — Guía oficial de alas por clase

- URL canónica: https://muonline.webzen.com/en/gameinfo/guide/detail/313
- Título/editor: `Wings (Lv.1~4)`, Webzen.
- Consulta: 2026-07-18.
- Versión declarada: no declara temporada y contiene clases
  posteriores al corte objetivo.
- Dato extraído: la tabla de alas de nivel 3 asocia Dark Knight con Blade Master,
  Dark Wizard con Grand Master, Elf con High Elf, Summoner con Dimension Master,
  Magic Gladiator con Duel Master y Dark Lord con Lord Emperor. La sección de
  alas de nivel 1 y 2 nombra además Fairy Elf, Blade Knight, Soul Master, Muse
  Elf y Bloody Summoner.
- Transformación: se extrajeron únicamente asociaciones de nombres de clase; no
  se infirieron orden de quest, disponibilidad histórica ni propiedades de alas.
- Tipo/independencia: documentación oficial prioritaria de Webzen; es una página
  temática diferente de EVD-0006, pero ambas pertenecen al mismo editor.
- Confianza y uso permitido: `PARTIAL`; confirma que Webzen usa esos nombres y
  asociaciones en una versión posterior/no declarada. No demuestra por sí sola
  su disponibilidad en Season 4 global/inglés.
- Condición de uso: página pública oficial; sólo se conserva referencia y
  paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0008 — Inventario técnico de recursos BMD

- URL canónica: https://forum.muonlinehelp.com/topic83-bmd-files.html
- Título/autoría: `BMD Files`, publicado por el usuario `Hacker` en MU Online
  Help Forum; el propio post acredita el contenido a `BlueWolf`, sin explicar
  la relación entre ambos ni identificar el origen de la extracción.
- Fecha publicada: 2009-10-10; consulta: 2026-07-18.
- Versión declarada: etiqueta recursos de tercera clase como `s3` y recursos de
  Summoner como `s3 episode 2`; no identifica cliente ni región.
- Artefacto/procedencia: inventario comunitario de nombres de archivos bajo
  `Data/Player` y `Data/Item`; no se adjunta el cliente del que fueron extraídos.
- Dato extraído: `BootClass01` a `BootClass05` se asocian con Dark Wizard, Dark
  Knight, Fairy Elf, Magic Gladiator y Dark Lord; `BootClass201` a `203`, con
  Soul Master, Blade Knight y Muse Elf; `BootClass301` a `305`, con Grand Master,
  Blade Master, High Elf, Duel Master y Lord Emperor; `BootClass306`, con
  Summoner. También atribuye alas de tercera clase a BM, GM, HE y DM y alas de
  Summoner a `s3 ep2`.
- Transformación: se usaron sólo asociaciones explícitas nombre-recurso; no se
  interpretó la numeración como orden de evolución y no se infirieron Bloody
  Summoner ni Dimension Master.
- Tipo/independencia: registro técnico comunitario alojado fuera de Webzen y de
  los operadores privados ya registrados. La distinta publicación no prueba
  independencia del contenido: la atribución a `BlueWolf` y la ausencia del
  cliente de origen impiden reconstruir su linaje. No es un artefacto
  reproducible.
- Confianza y uso permitido: `PARTIAL`; confirma presencia técnica de nombres y
  recursos antes de la frontera Season 4.5, pero la fecha del post no demuestra
  que el inventario pertenezca al cliente global Season 4.
- Condición de uso: foro público; sólo referencia y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0009 — Códigos de tercera clase en MuServer 1.03B Season 4

- URL canónica: https://forum.xpzone.net/threads/erro-de-classes-mu-online.34660/
- Título/autor: `Erro de classes Mu Online`, consulta técnica comunitaria en
  XPZONE; reporte de `DaaN123` y respuesta de `fireservers`.
- Fecha publicada: 2017-05-08; consulta: 2026-07-18.
- Versión declarada: el reporte identifica `MuServer 1.03B + Season 4`; no
  declara región global ni procedencia de los archivos.
- Artefacto/procedencia: el rótulo `MuServer 1.03B + Season 4` pertenece al
  reporte de `DaaN123`. La tabla de códigos fue añadida por `fireservers` como
  valores usados "normalmente" y no se demuestra que proceda del servidor del
  reporte. No hay dump, binario ni hash.
- Dato extraído: el reporte nombra Grand Master, Blade Master, High Elf y
  Dimension Master. La respuesta lista además códigos para Grand Master, Blade
  Master, High Elf, Duel Master, Lord Emperor y Dimension Master.
- Transformación: se conservaron sólo asociaciones nombre-clase; los códigos no
  se adoptaron porque no existe artefacto versionado para reproducirlos.
- Tipo/independencia: conversación comunitaria independiente de Webzen a nivel
  de publicación. No se demostró independencia respecto de EVD-0005 ni de
  distribuciones privadas comunes. Sus códigos alternativos de tercera clase
  (`3`, `19`, `35`, `50`, `66`, `83`) difieren de los valores principales del
  fragmento de EVD-0005; como los códigos no fueron adoptados por ningún claim,
  la diferencia se conserva como límite de procedencia y no como conflicto de
  datos publicado.
- Confianza y uso permitido: `PARTIAL`; contrasta las seis terceras clases en
  una implementación rotulada Season 4, pero no prueba las cadenas completas ni
  aplicabilidad al ruleset global/inglés sin revisar procedencia.
- Condición de uso: foro público; sólo referencia y paráfrasis.
- Snapshot/hash: no capturado.

### EVD-0010 — Locator oficial y mirror archivado del cliente inglés Season 4

- URL canónica: https://muonline.webzen.com/en/news/notices/all/992
- Título/editor: `Updated Full Client File Available! (New)`, Webzen MU Support
  Team.
- Fecha publicada: 2009-09-21; consulta: 2026-07-18.
- Alcance declarado: el aviso identifica el instalador como
  `MU1_03 full(Eng).exe` y anuncia mirrors en FilePlanet, AtomicGamer, MMOsite y
  GamersHell dentro de la vigencia oficial de Season 4 global.
- Contraste archivístico: la captura de Wayback del mirror GamersHell del
  2009-09-28 titula el recurso `Global MU Online English Client (Season 4)` y
  declara un tamaño de 456.77 MB. La página se accedía mediante formularios de
  selección de mirror, no mediante una URL directa estable al binario.
- Uso permitido: locator de procedencia para una búsqueda reproducible; no se
  extrajo ningún dato de clases, mecánicas o fórmulas y no respalda por sí mismo
  `WZ-CLM-001`–`WZ-CLM-010`.
- Resultado de preservación: no se localizó una copia descargable del instalador
  con checksum del editor. Los digests recuperados del índice de Wayback
  corresponden a páginas HTML, no al ejecutable, y no son hashes del cliente.
- Confianza: `PARTIAL`; demuestra que el artefacto existió y fija producto,
  idioma y temporada del locator, pero no permite inspección técnica ni hash.
- Condición de uso: sólo se conservan URLs y metadatos públicos; no se descargó
  software ni se incorporó contenido protegido al repositorio.

### EVD-0011 — MU Online Fanz, página de segunda clase

- URL canónica: https://muonlinefanz.com/guide/quests/2nd-class-upgrade/
- Título/editor: `2nd Class Upgrade Quest`, MU Online Fanz.
- Consulta: 2026-07-18.
- Versión visible actual: `Page updated May 08, 2025` y etiqueta
  `b2023.09.11.001`; no declara Season 4.
- Dato extraído: la página asocia Dark Knight con Blade Knight, Fairy Elf con
  Muse Elf, Dark Wizard con Soul Wizard y Summoner con Bloody Summoner. También
  enumera evoluciones de clases posteriores al corte objetivo.
- Transformación: se conservaron únicamente asociaciones nominales; no se
  extrajeron requisitos, recompensas, cifras ni reglas de quest.
- Historial archivado: la primera captura indexada de la URL es del 2023-03-20
  y muestra `Page updated March 05, 2023 || b2022.12.18.002`. Ya incluía Slayer,
  Gun Crusher, White Wizard y Rune Mage, además de usar `Soul Wizard`.
- Búsqueda negativa: el índice consultado no devolvió capturas de rutas que
  contuvieran segunda clase entre 2012 y 2022. Esto sólo demuestra ausencia en
  el índice consultado, no que nunca hubiera existido otra página.
- Procedencia/versión: fuente obligatoria definida por el propietario, pero la
  página y su primera captura disponible son actuales/posteriores y mezclan
  clases ajenas a Season 4. No demuestra aplicabilidad histórica.
- Confianza y uso permitido: `PARTIAL`; contraste nominal actual. No habilita
  promoción y participa en `DSP-0001` por la denominación `Soul Wizard`.
- Snapshot/hash: no se copió la página al repositorio. Digest de la primera
  respuesta HTML en el índice de Wayback:
  `Z2WMGS7NIZUXA5UAKLXIUQDEBDWLMUWK`; no es hash de un cliente del juego.

### EVD-0012 — MU Online Fanz, página de tercera clase

- URL canónica: https://muonlinefanz.com/guide/quests/3rd-class-upgrade/
- Título/editor: `3rd Class Upgrade Quest`, MU Online Fanz.
- Consulta: 2026-07-18.
- Versión visible actual: `Page updated May 08, 2025` y etiqueta
  `b2023.09.11.001`; no declara Season 4.
- Dato extraído: la página asocia Blade Knight con Blade Master, Muse Elf con
  High Elf, Soul Master con Grand Master, Bloody Summoner con Dimension Master,
  Magic Gladiator con Duel Master y Dark Lord con Lord Emperor. También enumera
  evoluciones de clases posteriores al corte objetivo.
- Contexto de obtención: la página presenta una serie de tres quests de tercera
  clase; permite iniciarla desde nivel 380 y exige nivel 400 para completar la
  parte final que otorga la evolución. No se adopta ninguna recompensa numérica.
- Transformación: se conservaron únicamente asociaciones nominales; no se
  extrajeron requisitos, recompensas, cifras ni reglas de quest.
- Historial archivado: la primera captura indexada de la URL es del 2023-03-20
  y muestra `Page updated February 25, 2023 || b2022.12.18.002`. Ya incluía
  Royal Slayer, Gun Breaker, Rune Spell Master, Grow Lancer y Rage Fighter.
- Búsqueda negativa: el índice consultado no devolvió capturas de rutas que
  contuvieran tercera clase entre 2012 y 2022. Esto sólo demuestra ausencia en
  el índice consultado, no que nunca hubiera existido otra página.
- Procedencia/versión: fuente obligatoria definida por el propietario, pero la
  página y su primera captura disponible son actuales/posteriores y mezclan
  clases ajenas a Season 4. No demuestra aplicabilidad histórica.
- Confianza y uso permitido: `PARTIAL`; contraste nominal actual. No habilita
  promoción y participa en `DSP-0001` porque parte de `Soul Master` mientras
  EVD-0011 denomina la etapa previa `Soul Wizard`.
- Snapshot/hash: no se copió la página al repositorio. Digest de la primera
  respuesta HTML en el índice de Wayback:
  `V5Y42YJVBEFX45KUQQD44O3Z5JG2GOU5`; no es hash de un cliente del juego.

### EVD-0013 — MU Online Fanz, página de cuarta clase

- URL canónica: https://muonlinefanz.com/guide/quests/4th-class-upgrade/
- Título/editor: `4th Class Upgrade Quest`, MU Online Fanz.
- Consulta: 2026-07-18.
- Versión visible actual: `Page updated May 08, 2025` y etiqueta
  `b2023.09.11.001`; no declara Season 4.
- Dato extraído: la página clasifica Grand Master → Soul Wizard como evolución
  de cuarta clase. En combinación con EVD-0012, el orden actual visible es Soul
  Master → Grand Master → Soul Wizard.
- Transformación: se conservó únicamente la asociación nominal y la categoría
  de cuarta clase indicada por el título/sección; no se extrajeron requisitos,
  recompensas, cifras ni otras cadenas.
- Confianza y uso permitido: `PARTIAL`; aclara que Soul Wizard sucede a Grand
  Master en el contenido actual de MU Online Fanz. No fija en qué temporada se
  introdujo ni demuestra por sí misma que Season 4 terminara en Grand Master.
- Snapshot/hash: no capturado; sólo referencia y paráfrasis.

## Bitácora de investigación

### 2026-07-18 — Aclaración del propietario sobre la cadena del mago

- Aclaración recibida: para Season 4, la cadena es Dark Wizard → Soul Master →
  Grand Master. Grand Master fue la evolución incorporada en esa temporada y
  se obtiene al completar una serie de quests culminada a nivel 400. Soul
  Wizard pertenece a temporadas posteriores.
- Contraste permitido: EVD-0012 enumera Soul Master → Grand Master como tercera
  clase, describe una secuencia de tres quests iniciable a nivel 380 cuya parte
  final exige nivel 400, y EVD-0013 enumera Grand Master → Soul Wizard como
  cuarta clase. Esto confirma la estructura actual y que EVD-0011 omite etapas.
- Límite: EVD-0012/EVD-0013 son páginas actuales sin temporada declarada. La
  introducción de Grand Master específicamente en Season 4 queda como
  clasificación explícita del propietario, no como claim `VERIFIED` por
  evidencia histórica.
- Decisión: mantener la cadena candidata de Season 4 terminada en Grand Master,
  excluir Soul Wizard del ruleset candidato y mantener `CLM-0001`, `CLM-0004`
  y `DSP-0001` en sus estados actuales hasta demostrar la frontera temporal.

### 2026-07-18 — Aplicabilidad histórica de MU Online Fanz

- Pregunta: si las páginas de segunda y tercera clase de MU Online Fanz poseen
  historial, snapshots o marcas que permitan atribuir sus asociaciones a Season
  4 global/inglés.
- Fuentes permitidas: sólo las dos páginas de MU Online Fanz y copias archivadas
  de esas mismas URLs; el índice archivístico se usó únicamente como metadata de
  preservación.
- Resultado: las páginas actuales declaran actualización de 2025 y build 2023.
  Sus primeras capturas disponibles son de marzo de 2023 con build 2022; ya
  mezclaban múltiples clases posteriores al corte objetivo.
- Conflicto: EVD-0011 usa Dark Wizard → Soul Wizard, mientras EVD-0012 usa Soul
  Master → Grand Master. Se abre `DSP-0001`; no se normalizan ambos nombres.
- Resultado negativo: no se hallaron capturas indexadas de rutas de segunda o
  tercera clase entre 2012 y 2022 ni una declaración de Season 4 en las páginas
  auditadas. La ausencia del índice no prueba inexistencia histórica.
- Decisión: añadir EVD-0011/EVD-0012 sólo como contrastes `PARTIAL`, mantener
  todos los claims sin promoción y conservar bloqueados los fixtures de
  personajes, stats, puntos por nivel y Marlon.

### 2026-07-18 — Auditoría de procedencia y búsqueda de artefacto global/inglés

- Pregunta: si EVD-0005, EVD-0008 y EVD-0009 son independientes y atribuibles a
  Season 4 global/inglés, y si sobrevive un artefacto técnico reproducible.
- Auditoría: EVD-0005 es un fragmento de MuWeb sin distribución versionada;
  EVD-0008 fue publicado por `Hacker` pero acredita a `BlueWolf`; EVD-0009
  combina un reporte rotulado Season 4 con una tabla genérica de otro usuario.
- Independencia: sólo se demostró separación de sitios o autores visibles. No
  se demostró independencia del contenido ni procedencia desde el cliente
  global/inglés; las tres evidencias pueden depender de archivos o convenciones
  comunitarias compartidas.
- Artefacto: EVD-0010 localiza el instalador inglés oficial y un mirror
  archivado que lo rotula Season 4, pero no se recuperó el binario ni un
  checksum atribuible al editor. Resultado técnico: negativo para artefacto
  descargable y hasheable.
- Decisión: mantener `CLM-0001`, `CLM-0004` y todos los claims atómicos en
  `PARTIAL`; EVD-0005/EVD-0008/EVD-0009 no cuentan como tres líneas técnicas
  independientes demostradas. No diseñar fixtures ni publicar cadenas.
- Directiva posterior del propietario: toda nueva información factual del juego
  debe extraerse de MU Online Fanz. La consulta preliminar muestra páginas
  actuales con clases posteriores y etiquetas de build 2023/2025; no se les
  atribuye Season 4 sin una revisión de versión específica.

### 2026-07-18 — Contraste técnico independiente por claim

- Pregunta: si artefactos o registros técnicos independientes permiten fijar
  `WZ-CLM-001` a `WZ-CLM-010` en Season 4 global/inglés.
- Consultas: nombres y códigos de clase, recursos BMD, clientes/MuServer Season
  4, `1.03B`, `1.05D` y clientes Season 4.
- Resultado: EVD-0008 aporta asociaciones de recursos publicadas dentro de la
  ventana histórica de Season 4; EVD-0009 contrasta los seis nombres de tercera
  clase en una implementación rotulada Season 4.
- Límite: no se localizó artefacto técnico descargable y hasheado atribuible al
  cliente global/inglés. Los anuncios de servidores privados recuperados no
  aportaron datos técnicos auditables.
- Decisión: mantener todos los claims y `CLM-0001`/`CLM-0004` en `PARTIAL`; no
  diseñar fixtures ni publicar las cadenas como reglas del producto.

### 2026-07-18 — Matriz atómica desde guías oficiales de Webzen

- Pregunta: qué familias y evoluciones enumeran las guías oficiales actuales o
  históricas de Webzen, y qué parte puede aplicarse al corte objetivo.
- Consultas: biblioteca oficial inglesa de Webzen; guías de quests, personajes,
  alas y cambios de tercera clase; búsquedas por los nombres candidatos.
- Resultado: EVD-0006 enumera cinco cadenas de tercera clase; EVD-0007 confirma
  las seis asociaciones base/tercera clase y los nombres intermedios candidatos.
- Límite: ninguna página declara Season 4. EVD-0007 mezcla contenido
  de clases posteriores, y EVD-0006 no incluye Summoner en su tabla.
- Decisión: añadir los claims oficiales atómicos como `PARTIAL`, mantener
  `CLM-0001` y `CLM-0004` sin promover y no publicar fixtures ni datos.

### 2026-07-18 — Búsqueda inicial de fuente oficial

- Pregunta original: fuente oficial que enumere clases y evoluciones para Season
  4 global/inglés.
- Consultas: dominio oficial de Webzen con `Season 4`, `Episode`, `3rd class`,
  `Blade Master` y `Dimension Master`; contraste abierto con los seis nombres y
  la ventana 2009–2010.
- Resultado: no se localizó una página oficial de Season 4 que enumere el
  conjunto completo. Las páginas recuperadas delimitan Season 4 global o
  pertenecen a versiones posteriores.
- Hallazgo útil: EVD-0005 aporta contraste técnico contemporáneo, pero carece de
  versión declarada y permanece `PARTIAL`.
- Decisión: no promover `CLM-0001` ni `CLM-0004`; no publicar datos.
- Decisión posterior del propietario: se elimina el requisito de
  contemporaneidad; Webzen es la fuente prioritaria aunque la página sea actual,
  posterior o archivada. Se mantienen trazabilidad y clasificación por versión.

## Alcance y reglas de captura

- La nota `docs/05-research/season-4-reference-scope.md` documenta el alcance
  aprobado: Season 4 global/inglés. Episodio y `main` no son requisitos.

- Este registro cubre únicamente el ruleset Season 4 global/inglés. Las
  variantes de servidores privados deben registrarse como perfiles separados.
- Una fuente sin temporada explícita puede servir como pista, pero no basta para
  atribuir un dato a Season 4.
- Dos páginas que se copian entre sí cuentan como una sola línea de evidencia.
- Cada valor debe apuntar a IDs de evidencia; los conflictos deben conservar
  ambas propuestas y no resolverse por preferencia.
- No se implementará la fórmula de puntos mientras la matriz no sea aprobada.

## Matriz de investigación

Las filas son candidatas, no un ruleset publicable. `PARTIAL` indica coincidencia
nominal entre varias publicaciones, no líneas independientes demostradas. La
auditoría de EVD-0005/EVD-0008/EVD-0009 no resolvió su procedencia desde el
ruleset global/inglés.

### Claims atómicos extraídos de Webzen

| ID | Afirmación atómica | Evidencia | Versión y límite de aplicación | Transformación | Confianza |
|---|---|---|---|---|---|
| WZ-CLM-001 | Dark Knight se asocia con Blade Knight. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | Asociación nominal; sin reglas de quest. | PARTIAL |
| WZ-CLM-002 | Blade Knight se asocia con Blade Master como tercera clase. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | Cadena y asociación nominal. | PARTIAL |
| WZ-CLM-003 | Dark Wizard se asocia con Soul Master. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | Asociación nominal; sin reglas de quest. | PARTIAL |
| WZ-CLM-004 | Soul Master se asocia con Grand Master como tercera clase. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | Cadena y asociación nominal. | PARTIAL |
| WZ-CLM-005 | Elf se asocia con Muse Elf. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; `Elf` y `Fairy Elf` no se equiparan como hecho. | Asociación nominal. | PARTIAL |
| WZ-CLM-006 | Muse Elf se asocia con High Elf como tercera clase. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | Cadena y asociación nominal. | PARTIAL |
| WZ-CLM-007 | Magic Gladiator se asocia directamente con Duel Master, sin etapa intermedia en la tabla. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | `- none` conservado como ausencia declarada. | PARTIAL |
| WZ-CLM-008 | Dark Lord se asocia directamente con Lord Emperor, sin etapa intermedia en la tabla. | EVD-0006, EVD-0007 | Webzen sin temporada declarada; no demuestra Season 4. | `- none` conservado como ausencia declarada. | PARTIAL |
| WZ-CLM-009 | Summoner se asocia con Bloody Summoner. | EVD-0007 | Webzen sin temporada declarada y con clases posteriores; no demuestra Season 4. | Asociación nominal desde alas de nivel 1/2. | PARTIAL |
| WZ-CLM-010 | Summoner se asocia con Dimension Master como tercera clase. | EVD-0007 | Webzen sin temporada declarada y con clases posteriores; no demuestra Season 4. | Asociación nominal; no prueba la cadena completa. | PARTIAL |

Los diez claims son extracciones documentales, no datos autorizados del ruleset.
EVD-0006 y EVD-0007 no cuentan como dos editores independientes.

### Matriz de contraste técnico

| Claim | Contraste técnico | Qué confirma | Límite para Season 4 global/inglés | Resultado |
|---|---|---|---|---|
| WZ-CLM-001 | EVD-0008, EVD-0005 | Dark Knight y Blade Knight aparecen como recursos/códigos relacionados por la tabla técnica previa. | Falta demostrar procedencia global/inglesa del artefacto. | PARTIAL |
| WZ-CLM-002 | EVD-0008, EVD-0005, EVD-0009 | Blade Master existe como tercera clase y se asocia a la familia Dark Knight. | EVD-0009 sólo declara Season 4; EVD-0008 no identifica cliente. | PARTIAL |
| WZ-CLM-003 | EVD-0008, EVD-0005 | Dark Wizard y Soul Master aparecen como recursos/códigos relacionados por la tabla técnica previa. | Falta demostrar procedencia global/inglesa del artefacto. | PARTIAL |
| WZ-CLM-004 | EVD-0008, EVD-0005, EVD-0009 | Grand Master existe como tercera clase y se asocia a la familia Dark Wizard. | EVD-0009 sólo declara Season 4; EVD-0008 no identifica cliente. | PARTIAL |
| WZ-CLM-005 | EVD-0008, EVD-0005 | Fairy Elf/Elf y Muse Elf aparecen como recursos/códigos relacionados. | Persiste la diferencia nominal `Elf`/`Fairy Elf` y falta procedencia global. | PARTIAL |
| WZ-CLM-006 | EVD-0008, EVD-0005, EVD-0009 | High Elf existe como tercera clase y se asocia a la familia Elf. | EVD-0009 sólo declara Season 4; EVD-0008 no identifica cliente. | PARTIAL |
| WZ-CLM-007 | EVD-0008, EVD-0005, EVD-0009 | Magic Gladiator y Duel Master aparecen; EVD-0005 los asocia directamente. | La ausencia de etapa intermedia no fue reproducida en un artefacto Season 4 global. | PARTIAL |
| WZ-CLM-008 | EVD-0008, EVD-0005, EVD-0009 | Dark Lord y Lord Emperor aparecen; EVD-0005 los asocia directamente. | La ausencia de etapa intermedia no fue reproducida en un artefacto Season 4 global. | PARTIAL |
| WZ-CLM-009 | EVD-0005 | La tabla técnica previa asocia Summoner con Bloody Summoner. | EVD-0008 sólo nombra Summoner; EVD-0009 sólo Dimension Master. | PARTIAL |
| WZ-CLM-010 | EVD-0005, EVD-0009 | Dimension Master aparece y EVD-0005 lo asocia con Summoner. | No hay artefacto global/inglés ni cadena completa reproducible. | PARTIAL |

### Matriz de aplicabilidad de MU Online Fanz

| Claim | Evidencia Fanz | Resultado nominal | Límite para Season 4 global/inglés | Estado |
|---|---|---|---|---|
| WZ-CLM-001 | EVD-0011 | Coincide Dark Knight → Blade Knight. | Página/build 2022–2025 con clases posteriores; sin Season 4. | PARTIAL |
| WZ-CLM-002 | EVD-0012 | Coincide Blade Knight → Blade Master. | Página/build 2022–2025 con clases posteriores; sin Season 4. | PARTIAL |
| WZ-CLM-003 | EVD-0011, EVD-0012, EVD-0013 | EVD-0012/EVD-0013 ordenan Soul Master → Grand Master → Soul Wizard; EVD-0011 omite etapas. | `DSP-0001` abierto y sin frontera histórica de Season 4. | PARTIAL |
| WZ-CLM-004 | EVD-0012, EVD-0013 | Coincide Soul Master → Grand Master y sitúa Soul Wizard después. | No demuestra que Season 4 terminara en Grand Master. | PARTIAL |
| WZ-CLM-005 | EVD-0011 | Coincide Fairy Elf → Muse Elf. | Página/build 2022–2025 con clases posteriores; sin Season 4. | PARTIAL |
| WZ-CLM-006 | EVD-0012 | Coincide Muse Elf → High Elf. | Página/build 2022–2025 con clases posteriores; sin Season 4. | PARTIAL |
| WZ-CLM-007 | EVD-0012 | Coincide Magic Gladiator → Duel Master. | No demuestra históricamente ausencia de etapa intermedia; sin Season 4. | PARTIAL |
| WZ-CLM-008 | EVD-0012 | Coincide Dark Lord → Lord Emperor. | No demuestra históricamente ausencia de etapa intermedia; sin Season 4. | PARTIAL |
| WZ-CLM-009 | EVD-0011 | Coincide Summoner → Bloody Summoner. | Página/build 2022–2025 con clases posteriores; sin Season 4. | PARTIAL |
| WZ-CLM-010 | EVD-0012 | Coincide Bloody Summoner → Dimension Master. | Página/build 2022–2025 con clases posteriores; sin Season 4. | PARTIAL |

El contraste aumenta la cobertura nominal, pero no resuelve el requisito de
versión. EVD-0011/EVD-0012 son páginas del mismo editor, no dos evidencias
independientes, y su historial visible no alcanza Season 4. No habilitan
promoción.

| Clase / ID provisional | Stats iniciales | Stats disponibles | Evolución | Puntos por nivel | Marlon | Evidencias independientes | Conflictos | Confianza |
|---|---|---|---|---|---|---:|---|---|
| Dark Knight (`dark-knight`, provisional) | NOT_RESEARCHED | NOT_RESEARCHED | Dark Knight → Blade Knight → Blade Master | NOT_RESEARCHED | NOT_RESEARCHED | Webzen (`EVD-0006`, `EVD-0007`, mismo editor) + publicaciones comunitarias con independencia no demostrada | Ninguno registrado | PARTIAL |
| Dark Wizard (`dark-wizard`, provisional) | NOT_RESEARCHED | NOT_RESEARCHED | Dark Wizard → Soul Master → Grand Master | NOT_RESEARCHED | NOT_RESEARCHED | Webzen (`EVD-0006`, `EVD-0007`, mismo editor) + publicaciones comunitarias con independencia no demostrada | Ninguno registrado | PARTIAL |
| Fairy Elf (`fairy-elf`, provisional) | NOT_RESEARCHED | NOT_RESEARCHED | Fairy Elf → Muse Elf → High Elf | NOT_RESEARCHED | NOT_RESEARCHED | Webzen alterna `Elf`/`Fairy Elf`; contrastes comunitarios sin procedencia demostrada | Diferencia nominal abierta, sin propuestas incompatibles | PARTIAL |
| Magic Gladiator (`magic-gladiator`, provisional) | NOT_RESEARCHED | NOT_RESEARCHED | Magic Gladiator → Duel Master | NOT_RESEARCHED | NOT_RESEARCHED | Webzen (`EVD-0006`, `EVD-0007`, mismo editor) + publicaciones comunitarias con independencia no demostrada | Ninguno registrado | PARTIAL |
| Dark Lord (`dark-lord`, provisional) | NOT_RESEARCHED | NOT_RESEARCHED | Dark Lord → Lord Emperor | NOT_RESEARCHED | NOT_RESEARCHED | Webzen (`EVD-0006`, `EVD-0007`, mismo editor) + publicaciones comunitarias con independencia no demostrada | Ninguno registrado | PARTIAL |
| Summoner (`summoner`, provisional) | NOT_RESEARCHED | NOT_RESEARCHED | Summoner → Bloody Summoner → Dimension Master | NOT_RESEARCHED | NOT_RESEARCHED | Sólo `EVD-0007` aporta la asociación oficial completa; contrastes comunitarios sin independencia demostrada | EVD-0006 omite Summoner de su tabla | PARTIAL |

EVD-0004 contrasta los nombres de tercera clase desde una fuente oficial
posterior, pero no se suma al conteo por carecer de atribución a Season 4.

## Campos obligatorios por celda factual

1. Valor o afirmación atómica.
2. Clase y etapa/evolución a la que aplica.
3. Temporada o versión declarada por cada fuente.
4. IDs de evidencia y evaluación de independencia.
5. Transformación aplicada al dato original, si existe.
6. Estado de confianza y justificación.
7. Conflicto abierto o prueba reproducible asociada, cuando corresponda.

## Plan de investigación

1. Revisar las guías oficiales de Webzen que enumeren el conjunto completo y las
   evoluciones; registrar por separado qué confirman y qué no fijan para Season
   4 global/inglés.
2. Registrar versión, región, procedencia y confianza de cada hallazgo antes de
   promoverlo al ruleset `mu-s4-global-reference`.
3. Capturar documentación oficial de Webzen cuando sea legalmente accesible,
   sin descartar páginas por su fecha de publicación.
4. Mantener cerrada como negativa la auditoría histórica de MU Online Fanz hasta
   que exista una marca de Season 4 o una captura anterior verificable.
5. Separar stats iniciales, stats distribuibles, puntos por nivel y Marlon en
   claims atómicos por clase y etapa.
6. Registrar cualquier diferencia de versión o implementación como conflicto.
7. Diseñar casos reproducibles para los claims que puedan observarse en un
   entorno controlado.
8. Solicitar revisión antes de promover datos o derivar fórmulas.

## Estado de evidencia

- Evidencias registradas: 13 (tres líneas oficiales de Webzen sobre nombres, un
  locator oficial de cliente, una guía histórica espejada, dos implementaciones
  privadas Season 4, tres registros técnicos comunitarios y dos páginas de MU
  Online Fanz auditadas históricamente, más la página de cuarta clase usada para
  clasificar el orden actual de Soul Wizard; alcance de versión incompleto).
- Claims verificados: 0 de 6.
- Claims parciales: 2 de 6 (`CLM-0001`, `CLM-0004`).
- Conflictos registrados: 1 abierto (`DSP-0001`, inconsistencia nominal interna
  de MU Online Fanz sin aplicabilidad demostrada a Season 4).
- Datos o fórmulas incorporados al producto: ninguno.
