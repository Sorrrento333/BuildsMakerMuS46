# Evaluación de Json Everything para el validador

- Estado: evaluación técnica completada; compilación propia integrada
  localmente, con verificación remota pendiente.
- Fecha de corte: 2026-07-19.
- Alcance: herramienta `MuOnline.SchemaValidator`; la aplicación WPF queda
  expresamente fuera.
- Naturaleza: análisis técnico y operativo, no asesoramiento jurídico.

## Pregunta

Determinar cómo conservar validación JSON Schema Draft 2020-12 sin introducir
condiciones no evaluadas en una futura distribución. Se comparan exactamente
tres caminos: usar los binarios NuGet actuales, compilar Json Everything desde
fuente MIT o sustituir el proveedor.

## Línea base reproducible

El proyecto fija `JsonSchema.Net 9.2.2`. Su lock file resuelve además
`JsonPointer.Net 7.0.1`, `Json.More.Net 3.0.1` y `Humanizer.Core 3.0.10`.
`dotnet list ... package --include-transitive --no-restore` confirmó ese grafo.

Los tres paquetes Json Everything restaurados contienen `OSMFEULA.txt`, marcan
`requireLicenseAcceptance=true` y declaran el archivo como licencia del paquete.
Sus tres copias son byte a byte iguales, con SHA-256
`5c805ac94dfdb4a3be55547f04a2f4b9f1bb87d7e9ee6d6fe54b0e72093900c3`.
Una copia normalizada a LF se conserva en
`legal/tooling/json-everything/OSMFEULA.txt`; el hash anterior corresponde al
archivo original restaurado desde NuGet. La copia LF tiene SHA-256
`af4a3af5dd759dff09ab6f8f34bf9528a9c4b3cb5b97834b8206547aeb12ef33` y su
texto normalizado es idéntico al de los tres paquetes.

El `.nuspec` de `JsonSchema.Net 9.2.2` identifica el commit fuente
`8f112c415735af9519a5b478447ad8239fa18642`; los dos paquetes transitivos
identifican `8b8ab34027de5ad9f4ed50808b8e4889ca69cf4d`. El checkout del primer commit
contiene las versiones exactas `9.2.2`, `7.0.1` y `3.0.1` en los proyectos
relacionados y una licencia fuente MIT.

La suite actual demuestra el alcance funcional mínimo: cinco schemas y diez
fixtures sintéticos, referencias internas mediante `$defs`/`$ref`, formatos
`uri` y `date`, patrones y palabras clave Draft 2020-12. En la línea base:

- restore bloqueado desde NuGet: aprobado;
- build Release: 0 advertencias y 0 errores;
- pruebas .NET: 14/14 aprobadas, incluidas 2/2 del validador;
- control PowerShell: 5/5 schemas y 10/10 fixtures legibles;
- auditoría NuGet directa y transitiva: ningún paquete vulnerable en los cinco
  proyectos según los orígenes consultados el 2026-07-19.

El proyecto WPF referencia únicamente Data. El smoke publicado `win-x64` pasó
con 417 archivos y una inspección nominal confirmó que no contiene ensamblados
Json Everything ni `OSMFEULA.txt`. El validador y sus cuatro dependencias no
forman parte de la publicación WPF actual.

## Lectura acotada del acuerdo

El texto incorporado por los paquetes afirma simultáneamente que:

- el acuerdo alcanza los binarios precompilados publicados por el proyecto;
- el cargo sólo se aplica al uso en actividades generadoras de ingresos con
  ingresos brutos anuales iguales o superiores a USD 10.000, con otra exención
  por soporte o mantenimiento separado;
- el código fuente permanece bajo MIT;
- el propio acuerdo no alcanza binarios compilados independientemente desde
  fuente y, ante conflicto, prevalece la licencia open source;
- permite redistribuir el binario recibido si se cumple MIT.

No se infiere de estas frases que una distribución concreta sea gratuita. El
repositorio no ha clasificado el uso futuro ni los ingresos de cada usuario, y
el archivo incorporado no fija importe, destino ni mecanismo de pago. Además,
el README público del mantenedor resume el alcance con una formulación más
amplia sobre usuarios que generan ingresos que la condición de umbral contenida
en el archivo del paquete. Esta discrepancia se conserva como incertidumbre y
no se resuelve por interpretación del proyecto.

## Comparación

| Alternativa | Compatibilidad observada | Condiciones y provenance | Coste/riesgo | Resultado |
|---|---|---|---|---|
| Binarios NuGet actuales | PASS de los 10 fixtures; API ya integrada | Paquetes y commits identificados; `OSMFEULA.txt` aplica a esos binarios según su texto | Mínimo coste técnico, pero el uso futuro puede requerir clasificar actividad/ingresos y obtener términos externos no incluidos | Permitido sólo para desarrollo interno actual; no aprobado para distribución |
| Compilación propia desde fuente | Mismo código/API; el spike y la integración pasan contratos, formatos y hashes entre dos rutas | Fuente MIT; el acuerdo excluye expresamente binarios autocompilados | Exige conservar pipeline, SBOM/avisos, commits y SDK fijados | Ruta seleccionada e integrada localmente; falta confirmar CI remoto limpio |
| Sustitución por Corvus | `Corvus.Json.Validator 4.6.7` declara .NET 10, validación dinámica y Draft 2020-12 | Paquete y repositorio declaran Apache-2.0 | No es reemplazo directo; incorpora Roslyn y un grafo considerable de generación/validación, con coste de arranque. No se probaron los 10 fixtures ni los formatos requeridos | Alternativa de contingencia; exige spike antes de decidir |

Corvus se usa como candidato concreto porque su documentación y paquete
declaran justamente el dialecto, el modo dinámico y el framework requeridos. No
se equipara esa declaración con compatibilidad del proyecto: faltan pruebas de
paridad, auditoría transitiva, diagnóstico de errores y comportamiento offline.

## Conclusión operativa

La evaluación queda cerrada, pero no autoriza una distribución del validador.
El validador se mantiene fuera de WPF. Su grafo normal ya consume ensamblados
propios desde los commits fuente fijados bajo MIT, sin empaquetarlos en Git, y
conserva `Humanizer.Core 3.0.10` como único paquete runtime. La reproducibilidad,
paridad, avisos y auditoría pasaron localmente; falta la ejecución remota limpia.

Usar el binario NuGet en una actividad potencialmente alcanzada requiere una
clasificación explícita del caso y, cuando corresponda, revisión jurídica o
confirmación del mantenedor. El spike posterior de compilación propia pasó en
dos rutas fuente independientes, con hashes reproducibles, SBOM, auditoría y
paridad sobre los contratos actuales; se documenta en
`json-everything-source-build-spike.md`. Corvus sólo se reconsidera si falla la
integración de esa ruta o su coste de mantenimiento resulta inaceptable.

## Criterios aplicados por el spike posterior

1. Fijar los commits fuente y el SDK, sin depender de ramas o tags móviles.
2. Producir los tres ensamblados necesarios en CI limpio y registrar hashes,
   SBOM, licencias y todas las dependencias de compilación/runtime.
3. Ejecutar los mismos 10 fixtures, la repetición en proceso y validación de
   formatos; exigir 14/14 pruebas de solución sin regresiones.
4. Auditar vulnerabilidades y confirmar que ninguna salida conserva metadatos o
   artefactos del paquete NuGet publicado por Json Everything.
5. Sólo después reemplazar la referencia NuGet actual y decidir si el validador
   puede entrar en una distribución.

Los cinco puntos quedaron demostrados localmente el 2026-07-19. La integración
y sus controles se detallan en `json-everything-source-integration.md`.

## Fuentes

- Copias locales restauradas de los `.nuspec` y `OSMFEULA.txt` de
  `JsonSchema.Net 9.2.2`, `JsonPointer.Net 7.0.1` y `Json.More.Net 3.0.1`.
- [Repositorio oficial de Json Everything](https://github.com/json-everything/json-everything).
- [Licencia MIT fuente de Json Everything](https://github.com/json-everything/json-everything/blob/master/LICENSE).
- [Acuerdo publicado por Json Everything](https://github.com/json-everything/json-everything/blob/master/OSMFEULA.txt).
- [Paquete oficial JsonSchema.Net 9.2.2](https://www.nuget.org/packages/JsonSchema.Net/9.2.2).
- [Repositorio oficial de Corvus.JsonSchema](https://github.com/corvus-dotnet/Corvus.JsonSchema).
- [Paquete oficial Corvus.Json.Validator 4.6.7](https://www.nuget.org/packages/Corvus.Json.Validator/4.6.7).
