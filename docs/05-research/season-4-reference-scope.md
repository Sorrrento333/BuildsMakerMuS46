# Alcance aprobado del ruleset Season 4

## Decisión

La versión objetivo única del proyecto es **MU Online Season 4 de la familia
global/inglesa**.

Estado: `APPROVED`.

- Aprobado por: propietario del proyecto.
- Fecha: 2026-07-18.
- ID de alcance: `mu-s4-global-reference`.

Esta decisión reemplaza el alcance anterior que exigía Episode 5. El episodio y
el número exacto de `main` dejan de ser dimensiones requeridas o bloqueantes.

## Alcance versionado

| Dimensión | Valor | Estado y confianza |
|---|---|---|
| Producto | MU Online para PC | `VERIFIED` por fuentes oficiales registradas |
| Región/protocolo | Global/inglés | Aprobado como alcance; cada dato debe demostrar aplicabilidad |
| Temporada | Season 4 | `APPROVED` como versión objetivo |
| Episodio/build/main | No requerido | Fuera del criterio de aceptación; registrar sólo si una fuente lo declara |
| ID | `mu-s4-global-reference` | Estable desde esta decisión |

## Reglas de evidencia

1. Ningún dato o fórmula se considera correcto por la sola decisión de alcance.
2. Toda afirmación debe apuntar a evidencia aplicable a Season 4 global/inglesa.
3. Una fuente que declare `Season 4` puede satisfacer la dimensión temporal; su
   región, procedencia, independencia y confianza siguen evaluándose.
4. Datos de otras temporadas, otras regiones o servidores privados no se
   incorporan silenciosamente como reglas base; pueden conservarse como
   contraste o perfiles separados.
5. Cada fórmula necesita ID, versión, entradas, orden, redondeo, evidencia y
   pruebas reproducibles.
6. Las variantes privadas pertenecen a perfiles de servidor.
7. Por decisión posterior del propietario, toda nueva información factual del
   juego se extrae de MU Online Fanz. Webzen se conserva para delimitar versión,
   procedencia y contraste. Ninguna fuente queda exenta de demostrar Season 4;
   no se exige identificar episodio o `main`.

## Evidencia de alcance ya disponible

### S4-EVD-001 — Lanzamiento oficial de Season 4 global

- URL canónica: https://muonline.webzen.com/th/news/notices/all/1014
- Título/editor: `Glabal MU Online new server open!`, Webzen.
- Fecha publicada: 2009-05-14; lanzamiento: 2009-05-21 (PST).
- Consulta: 2026-07-18.
- Dato extraído: Webzen denomina el producto `Global MU Online - Season 4`.
- Uso permitido: demuestra producto, temporada y contexto global.
- Tipo: fuente oficial de Webzen.
- Snapshot/hash: no capturado.

### S4-EVD-002 — Season 4 global tuvo contenido desplegado gradualmente

- URL canónica: https://muonline.webzen.com/en/news/notices/all/365
- Título/editor: `Introducing a New Realm`, Webzen.
- Fecha publicada: 2009-05-20; consulta: 2026-07-18.
- Dato extraído: el lanzamiento usaba un cliente beta de Season 4; Summoner y
  Castle Siege se anunciaban para fechas posteriores.
- Uso permitido: demuestra que `Season 4` puede contener revisiones internas;
  esas revisiones no constituyen una dimensión obligatoria del ruleset.
- Tipo: fuente oficial de Webzen.
- Snapshot/hash: no capturado.

### S4-EVD-003 — Frontera posterior Season 4.5

- URL canónica: https://muonline.webzen.com/news/notices/478
- Título/editor: `MU Online Season 4.5 Update Coming up!`, Webzen.
- Fecha publicada: 2010-02-22; actualización anunciada: 2010-02-24.
- Consulta: 2026-07-18.
- Dato extraído: Webzen identifica una actualización posterior como Season 4.5.
- Uso permitido: contraste de frontera editorial; cada dato debe clasificarse
  por la temporada que demuestre la fuente.
- Tipo: fuente oficial de Webzen.
- Snapshot/hash: no capturado.

## Claims de alcance

| ID | Afirmación | Evidencia | Estado |
|---|---|---|---|
| S4-CLM-001 | Global MU Online lanzó Season 4 el 21 de mayo de 2009. | S4-EVD-001, S4-EVD-002 | `VERIFIED` para Season 4 global; una sola organización |
| S4-CLM-002 | El despliegue global de Season 4 cambió durante su vigencia. | S4-EVD-002 | `VERIFIED` para los cambios anunciados |
| S4-CLM-003 | Cada dato publicado demuestra aplicabilidad a Season 4 global/inglesa. | Evaluación individual | Regla permanente de publicación |

## Consecuencias

- `RES-0001` investiga Season 4 global/inglesa sin exigir episodio.
- Los IDs `mu-s4e5-global-reference` y `S45-*` quedan retirados.
- El episodio y el número de `main` no son prioridades ni bloqueos.
- Las evidencias que declaran Season 4 pueden evaluarse por sus méritos sin una
  penalización automática por omitir episodio.
