# Próximas acciones

## Prioridad inmediata

1. Publicar la integración pendiente en una rama y exigir una ejecución limpia
   de GitHub Actions. `build-and-test` debe compilar Json Everything desde fuente,
   pasar restore bloqueado, build, 14/14 pruebas e inspección; el smoke WPF debe
   continuar aprobado.
2. Investigar evidencia histórica autorizada para aplicar a Season 4 la matriz
   candidata de `RES-0001`. `DSP-0002` ya no bloquea esta búsqueda: el propietario
   resolvió el valor de MG a favor de Energy 26.
3. Mantener bloqueados los fixtures productivos de personajes. Los seis claims
   están `PARTIAL`; los casos numéricos añadidos son pruebas de investigación,
   no contratos del ruleset.
4. No iniciar fórmulas derivadas de HP, Mana, AG, SD, daño o defensa hasta que
   sus entradas, redondeos y aplicabilidad a Season 4 tengan evidencia propia.

## Última tarea cerrada

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

Crear/publicar la rama con los cambios acumulados y observar los checks
requeridos del PR. Después registrar en `current-status.md` el ID del run,
imagen, SDK, hashes y resultado de `build-and-test` y
`wpf-publication-smoke`. Si ambos pasan, la siguiente investigación concreta es
localizar una fuente o artefacto histórico que atribuya a Season 4 la matriz de
stats y reglas de progresión; Energy 26 para MG ya es la decisión adoptada.
