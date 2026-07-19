# Avisos de terceros

Este inventario registra dependencias declaradas por el repositorio; no cambia
sus licencias. Una distribución binaria debe conservar los textos y avisos que
exija cada dependencia incluida.

## Dependencias productivas

| Componente | Versión | Licencia declarada por el paquete | Titular declarado |
|---|---:|---|---|
| `Microsoft.Data.Sqlite` / `.Core` | 10.0.10 | MIT | Microsoft Corporation |
| `SQLitePCLRaw.bundle_e_sqlite3`, `.core`, `.provider.e_sqlite3`, `.lib.e_sqlite3` | 2.1.12 | Apache-2.0 | SourceGear, LLC (2014–2024) |
| SQLite incorporado por `SQLitePCLRaw.lib.e_sqlite3` | 3.53.3 observada | Dominio público según el proyecto SQLite | SQLite authors |

Las versiones proceden de los lock files y la versión nativa observada en el
smoke test. Las licencias y titulares se comprobaron en los metadatos `.nuspec`
de los paquetes restaurados. La condición de SQLite se contrasta con su
[declaración oficial](https://www.sqlite.org/copyright.html).

## Herramientas y pruebas

- Los paquetes Microsoft Testing Platform/Test SDK y sus dependencias
  restauradas declaran MIT.
- Los paquetes xUnit v3 y `xunit.analyzers` restaurados declaran Apache-2.0.
- `Humanizer.Core` declara MIT.
- `JsonSchema.Net 9.2.2`, `JsonPointer.Net 7.0.1` y `Json.More.Net 3.0.1`
  incluyen `OSMFEULA.txt`. El archivo declara que el código fuente continúa
  bajo MIT, pero aplica un acuerdo de mantenimiento a ciertos usos de los
  binarios publicados en actividades que generan ingresos y alcanzan el umbral
  allí definido. Las tres copias restauradas son idénticas, con SHA-256
  `5c805ac94dfdb4a3be55547f04a2f4b9f1bb87d7e9ee6d6fe54b0e72093900c3`;
  el texto normalizado a LF y la evaluación se conservan en
  `legal/tooling/json-everything/OSMFEULA.txt` y
  `docs/03-architecture/json-everything-dependency-evaluation.md`.
- El spike `json-everything-source-build` compila independientemente las mismas
  versiones desde commits fijados y fuente MIT, genera SBOM/provenance y usa
  `Humanizer.Core 3.0.10` MIT en runtime. Sus tres DLL sustituyen localmente a
  los paquetes publicados en el grafo normal del validador; las salidas
  permanecen ignoradas bajo `artifacts/` y no se guardan como binarios en Git.
  La licencia fuente preservada está en
  `legal/tooling/json-everything-source/MIT.txt`.

## Límite previo a una distribución

La publicación WPF incorpora y prueba los avisos del proyecto,
Microsoft.Data.Sqlite, SQLitePCLRaw y los runtime packs autocontenidos resueltos.
El smoke exige diez archivos legales no vacíos y comprueba que sus hashes
sobrevivan al reemplazo de binarios.

Los binarios NuGet publicados de Json Everything ya no se resuelven en el grafo
normal. La compilación propia integrada pasó localmente locks, hashes, contratos,
formatos, auditoría y publicación con aviso MIT, sin `OSMFEULA.txt` ni `.nuspec`
de esos paquetes. Falta confirmar el workflow actualizado en un runner remoto
limpio. Corvus queda como contingencia no validada. Este inventario no es
asesoramiento jurídico ni convierte una licencia de terceros en Apache-2.0.
