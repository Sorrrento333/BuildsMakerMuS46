# ADR-0002: C#/.NET para el núcleo y la aplicación inicial

- Estado: Aceptado por el propietario.
- Fecha: 2026-07-18.
- Responsables: propietario del proyecto; implementación asistida por IA.

## Contexto

El proyecto necesita cálculos deterministas, trazas serializables, SQLite,
pruebas rápidas y distribución offline. `technology-evaluation.md` dejó
TypeScript y C# como candidatos y exigió spikes equivalentes antes de decidir.

Los spikes usan el mismo fixture técnico, persistencia y controles. El fixture
no representa ninguna fórmula ni dato de MU Online.

## Opciones consideradas

### TypeScript sobre Node.js 24

- Pasó 3 pruebas: cálculo/traza, entradas inválidas y round-trip SQLite.
- No requirió paquetes externos gracias a `node:test` y `node:sqlite`.
- Conserva la ventaja de un lenguaje común con una futura UI web.
- La aritmética entera exige comprobaciones explícitas del rango seguro.
- `node:sqlite` emitió una advertencia de API experimental.
- La máquina no tenía Node global; ejecutar offline requiere distribuir o
  preinstalar un runtime Node compatible.

### C# sobre .NET 10

- Pasó 4 comprobaciones equivalentes: cálculo/traza, divisor inválido,
  overflow y round-trip SQLite.
- `checked` y los enteros de 64 bits expresan directamente el control de
  overflow; `decimal` queda disponible para fórmulas futuras cuando la
  evidencia exija decimales.
- Publicó y ejecutó correctamente un binario Windows autocontenido offline.
- Medición observada del spike: ~73,4 MB autocontenido; ~194 KB dependiente del
  framework (sin contar el runtime instalado).
- El P/Invoke a `winsqlite3.dll` es deliberadamente una solución sin descargas
  para el spike, específica de Windows; no se adopta como acceso productivo.

### Rust

Se descartó para esta fase conforme a la evaluación previa por complejidad
prematura. No justificaba un tercer spike para resolver la comparación abierta.

## Decisión

Usar **C#/.NET 10** para el dominio, motor, aplicación, persistencia y primera
interfaz. Mantener las fronteras del monolito modular establecidas en ADR-0001.
La primera UI se evaluará dentro del ecosistema .NET (Blazor web/PWA o
escritorio) sin separar el motor en otro lenguaje.

Para producción se seleccionará una biblioteca SQLite mantenida mediante una
decisión de dependencia; el adaptador nativo del spike no se reutilizará como
implementación productiva.

## Consecuencias

- Los schemas y contratos seguirán siendo JSON interoperable.
- La estructura futura deberá adaptarse a una solución .NET conservando las
  capas conceptuales existentes.
- CI deberá fijar el SDK 10 y ejecutar `dotnet test`.
- La distribución inicial objetivo será autocontenida; tamaño, plataforma y
  formato final siguen sujetos al spike específico de UI/distribución.
- La licencia del proyecto continúa siendo una decisión independiente abierta.

## Plan de reversión

El núcleo permanecerá sin dependencias de UI y con contratos JSON. Si un spike
vertical posterior demuestra que .NET impide un requisito crítico, se propondrá
un ADR que reemplace éste y compare costos de migración con evidencia nueva.
