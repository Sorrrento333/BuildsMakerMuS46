# Spikes de selección tecnológica

Estos prototipos comparan TypeScript/Node.js y C#/.NET con el mismo alcance:

1. cálculo sintético determinista con redondeo explícito;
2. traza serializable de entradas, operación y resultado;
3. escritura y lectura del resultado en SQLite;
4. prueba automatizada del cálculo, la traza y la persistencia;
5. ejecución y publicación sin conexión después de disponer del SDK/runtime.

El cálculo `(10 * 3) / 4`, redondeado hacia abajo a `7`, es únicamente un
fixture técnico. **No representa una fórmula ni un dato de MU Online.**

## TypeScript

Requiere Node.js 24 o posterior, porque usa ejecución nativa de TypeScript y
`node:sqlite`:

```powershell
node --experimental-strip-types --test spikes/technology-selection/typescript/spike.test.ts
node --experimental-strip-types spikes/technology-selection/typescript/spike.ts
```

La publicación offline consiste en copiar `spike.ts` junto con un runtime de
Node compatible; el spike no usa paquetes de terceros.

## C#

Requiere el SDK de .NET 10 para compilar. En Windows usa `winsqlite3.dll`,
incluida con el sistema operativo:

```powershell
dotnet run --project spikes/technology-selection/csharp/TechnologySpike.csproj -- --test
dotnet run --project spikes/technology-selection/csharp/TechnologySpike.csproj
dotnet publish spikes/technology-selection/csharp/TechnologySpike.csproj -c Release -r win-x64 --self-contained true
```

Los ejecutables escriben la base temporal fuera del repositorio y la eliminan
al terminar.
