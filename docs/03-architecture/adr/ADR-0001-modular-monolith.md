# ADR-0001: Monolito modular con núcleo independiente

- Estado: Aceptado provisionalmente.
- Fecha: 2026-07-18.

## Contexto
El dominio es amplio, pero el equipo inicial será pequeño y necesita alta velocidad de cambio.

## Decisión
Construir un monolito modular. El motor de dominio y cálculo será una biblioteca independiente, sin dependencias de UI o persistencia.

## Consecuencias
- Menor complejidad operativa.
- Pruebas rápidas y ejecución offline.
- Posible extracción futura de API o servicios.
- Requiere disciplina para respetar límites internos.
