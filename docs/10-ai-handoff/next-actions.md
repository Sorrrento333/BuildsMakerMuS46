# Próximas acciones

## Prioridad inmediata

1. Investigar evidencia histórica autorizada para aplicar a Season 4 la matriz
   candidata de `RES-0001`. `DSP-0002` ya no bloquea esta búsqueda: el propietario
   resolvió el valor de MG a favor de Energy 26.
2. Mantener bloqueados los fixtures productivos de personajes. Los seis claims
   están `PARTIAL`; los casos numéricos añadidos son pruebas de investigación,
   no contratos del ruleset.
3. No iniciar fórmulas derivadas de HP, Mana, AG, SD, daño o defensa hasta que
   sus entradas, redondeos y aplicabilidad a Season 4 tengan evidencia propia.

## Última tarea cerrada

La integración acumulada se publicó en
`codex/integrate-source-and-research`. El primer run (`29697684666`) descubrió
una dependencia CRLF en los hashes del build fuente; se corrigió forzando LF y
eliminando PDB/símbolos. El run limpio `29697921106` sobre `d0626d2` aprobó
`build-and-test` y `wpf-publication-smoke` con SDK fijado `10.0.301`.

El workflow fija los selectores `ubuntu-latest`/`windows-latest` y la API pública
confirmó los runners, pero no la versión interna exacta de imagen; se registra
el límite sin inferirlo. Los hashes adoptados son `450646267c…`, `8f7be030e4…` y
`1f2dc6dfad…` para Json.More, JsonPointer y JsonSchema, respectivamente.

Se registraron como candidatos trazables los stats iniciales/distribuibles,
puntos por nivel y reglas de Marlon para las seis clases objetivo. `RES-0001`
contiene ahora 18 evidencias, 6/6 claims `PARTIAL`, un conflicto abierto y uno
resuelto por decisión del propietario.

Matriz confirmada por el propietario en orden `STR/AGI/VIT/ENE[/CMD]`: DW
`18/18/15/30`, DK `28/20/25/10`, ELF `22/25/20/15`, SUM `21/21/18/23`, MG
`26/26/26/26` y DL `26/20/20/15/25`. DW/DK/ELF/SUM ganan 5 puntos por nivel y
6 tras Hero Status desde 220; MG/DL ganan 7 desde el inicio y no realizan
Marlon.

MU Online Fanz coincide con la matriz y las reglas. Webzen coincide en cinco
filas, pero publica Energy 16 para MG en una guía actual con renovaciones
posteriores; `DSP-0002` conserva la divergencia como antecedente, pero quedó
resuelto a favor de Energy 26 por decisión explícita del propietario. Ningún
dato se promovió a `VERIFIED` ni se incorporó al producto.

La política de fuentes quedó ampliada por decisión del propietario: Fanz es la
fuente inicial prioritaria y pueden usarse fuentes adicionales para extracción,
contraste y resolución de conflictos, siempre con provenance y versión.

## Primera acción concreta

Localizar y registrar una fuente o artefacto histórico que atribuya a Season 4
la matriz de stats y reglas de progresión. Empezar por una evidencia primaria o
snapshot contemporáneo que confirme los stats iniciales; conservar Energy 26
para MG como decisión adoptada y no reabrir `DSP-0002` sin evidencia nueva.
