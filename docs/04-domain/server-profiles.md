# Perfiles de servidor

## Objetivo
Representar modificaciones privadas sin contaminar el ruleset de referencia.

## Estructura
- identidad y versión;
- ruleset base compatible;
- progresión por clase;
- nivel y stat máximos;
- quests habilitadas;
- política de reset/master reset;
- multiplicadores PvM/PvP;
- overrides de fórmulas permitidos;
- contenido habilitado/deshabilitado;
- notas y provenance local.

## Herencia
Un perfil puede extender otro, con máximo de profundidad definido. La resolución debe producir un perfil plano y un reporte de overrides.

## Seguridad
Los perfiles son datos declarativos. No pueden incluir scripts ejecutables.
