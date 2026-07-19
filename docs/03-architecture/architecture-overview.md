# Arquitectura general

## Estilo recomendado
Monolito modular inicialmente, con núcleo de dominio puro y fronteras claras. Evita complejidad distribuida temprana y permite extraer servicios más adelante.

## Capas
1. **Domain:** entidades, value objects, reglas e interfaces; sin UI ni base de datos.
2. **Calculation Engine:** grafo de fórmulas, modificadores, redondeos y trazas.
3. **Application:** casos de uso, orquestación, permisos y transacciones.
4. **Data:** snapshots, repositorios, migraciones e importadores.
5. **API:** contratos para UI y futuras integraciones.
6. **UI:** calculadora, enciclopedia, comparador y administración.

## Dependencias
Las dependencias apuntan hacia el dominio. La UI y persistencia implementan interfaces; el motor no conoce frameworks visuales.

## Despliegue inicial
Aplicación de escritorio WPF con backend embebido. ADR-0004, aceptado por el
propietario el 2026-07-18, fija una publicación autocontenida para `win-x64` como
primera distribución. Otros RIDs y una PWA futura requieren pruebas y decisiones
explícitas.
