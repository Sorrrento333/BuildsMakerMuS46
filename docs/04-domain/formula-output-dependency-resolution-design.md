# Resolución de dependencias entre fórmulas

## Estado

- Fecha: 2026-07-28.
- Estado técnico: `IMPLEMENTED`.
- Alcance factual: no publica fórmulas nuevas de MU Online.
- Gate que motivó el cambio: `DR-SD-DARK-WIZARD`, resuelto después por
  `EVD-0033`; `EVD-0034` extiende la decisión factual `RAW` a los cinco claims
  de SD que permanecían pendientes.

## Problema

`EVD-0026` define SD mediante `defense`, pero declara expresamente desconocido
si la fórmula consumidora usa la salida cruda o la visible truncada. Elegir una
etapa sería inventar una regla.

El contrato ya distinguía `FORMULA_OUTPUT` con referencia exacta y
`outputStage: RAW|VISIBLE`, pero el lector productivo rechazaba dependencias y
el request del motor sólo admitía enteros. Así tampoco podía conservar una
salida cruda fraccionaria después de resolver el gate factual.

## Decisión técnica

Application resuelve el grafo sobre el mismo estado validado:

1. `CONTEXT_VALUE` se resuelve como hasta ahora;
2. `FORMULA_OUTPUT` identifica fórmula y versión exactas;
3. la dependencia se ejecuta recursivamente con el mismo estado;
4. `RAW` entrega `RawOutput` decimal exacto;
5. `VISIBLE` entrega `VisibleOutput` entero;
6. el consumidor recibe la etapa seleccionada;
7. la traza conserva input, referencia, etapa, valor y cálculo productor.

Las referencias declaradas deben coincidir exactamente con los inputs
derivados, resolver en el mismo ruleset y tener aplicabilidad compatible. Los
ciclos fallan cerrados al cargar y al ejecutar. Una dependencia repetida se
calcula una sola vez.

## Precisión

Los mapas de inputs y trazas usan `System.Decimal`. `INT32` e `INT64` continúan
exigiendo valores integrales dentro de rango. El input contractual `DECIMAL`
habilita precisión base 10 para `CHECKED_DECIMAL_V1`; `CHECKED_INT64_V1` lo
rechaza. Los bounds conservan extremos enteros clasificados.

Las pruebas sintéticas seleccionan simultáneamente `RAW=9.5` y `VISIBLE=9`,
producen `18.5/18`, conservan ambas trazas, materializan la referencia JSON y
rechazan un ciclo. No contienen fórmulas de MU Online.

## Cierre factual posterior

`EVD-0033` registra la decisión explícita del propietario de consumir Defense
`RAW` para SD de Dark Wizard. `EVD-0034` registra después `RAW` para Dark
Knight, Fairy Elf, Magic Gladiator, Dark Lord y Summoner. Esas elecciones no
forman parte del mecanismo genérico.

La vertical posterior materializa `formula-defense-dark-wizard` y
`formula-sd-dark-wizard` `1.0.0`, enlazadas por referencia exacta y
`outputStage: RAW`. El caso `sd-dark-wizard-raw-defense-boundary` hace
observable la diferencia entre etapas y evita una regresión silenciosa a
`VISIBLE`.

La vertical siguiente materializa `formula-defense-dark-knight` y
`formula-sd-dark-knight` `1.0.0` con el mismo enlace exacto. El caso
`sd-dark-knight-raw-defense-boundary` fija Defense `RAW=7.6666…`,
`VISIBLE=7` y SD visible 107 frente a 106.

Las verticales posteriores materializan el mismo contrato exacto para Fairy Elf
y Magic Gladiator. `sd-magic-gladiator-raw-defense-boundary` fija Defense
`RAW=5.6`, `VISIBLE=5` y SD visible 130 frente a 129.
