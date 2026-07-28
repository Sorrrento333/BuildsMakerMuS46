# Changelog

## [Unreleased]

### Added

- Vertical funcional de AG de Magic Gladiator desde el claim `VERIFIED`
  `DR-AG-MAGIC-GLADIATOR` hasta WPF. `formula-ag-magic-gladiator` `1.0.0`
  nace `PUBLISHED` contra schema `2.1.0`, aplica a Magic Gladiator/Duel Master
  y conserva `EVD-0021`, `EVD-0026` y `DSP-0002` sin añadir evidencia ni
  reclasificar la divergencia de Energy.
- Cuatro casos positivos reproducen raw/visible
  `23.40/23`, `23.85/23`, `23.85/23` y `24.30/24`; cinco negativos cubren los
  cuatro stats por debajo de su base y familia. El programa materializa
  exactamente `ene * 0.15 + vit * 0.3 + agi * 0.25 + str * 0.2`.
- `CHECKED_DECIMAL_V1` conserva los cuatro coeficientes exactamente, no
  redondea aportes y trunca una sola vez en `visible-ag`. No se inventa un
  overflow imposible: la suma de coeficientes `0.9` mantiene la salida válida
  dentro de `INT64`.
- Contrato técnico `magic-gladiator-ag-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, cuatro inputs/límites, seis pasos,
  truncamiento, casos y conflicto resuelto.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 611 archivos,
  149.076.719 bytes y 10 avisos legales. El dataset avanza a `2026-07-28.2`,
  contiene 188 JSON y produce
  `sha256:5246861cec04e5e618611091d365e7e0a4c03d8227013f84c13e93354253d901`.
- Vertical funcional de AG de Summoner desde el claim `VERIFIED`
  `DR-AG-SUMMONER` hasta WPF. `formula-ag-summoner` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Summoner/Bloody
  Summoner/Dimension Master y conserva `EVD-0021`/`EVD-0026` sin añadir
  evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `18.30/18`, `18.75/18`, `18.75/18` y `19.20/19`; cinco negativos cubren los
  cuatro stats por debajo de su base y familia. El programa materializa
  exactamente `str * 0.2 + agi * 0.25 + vit * 0.3 + ene * 0.15`.
- `CHECKED_DECIMAL_V1` conserva los cuatro coeficientes exactamente, no
  redondea aportes y trunca una sola vez en `visible-ag`. No se inventa un
  overflow imposible: la suma de coeficientes `0.9` mantiene la salida válida
  dentro de `INT64`.
- Contrato técnico `summoner-ag-formula-contract.md` y bitácora de `RES-0002`
  para identidad, aplicabilidad, cuatro inputs/límites, seis pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 601 archivos,
  149.063.645 bytes y 10 avisos legales. El dataset avanza a `2026-07-28.1`,
  contiene 178 JSON y produce
  `sha256:6380346e97b61e31a6da3329b86f91954f2b120d880e751733e778f4cbb75f43`.
- Vertical funcional de AG de Fairy Elf desde el claim `VERIFIED`
  `DR-AG-FAIRY-ELF` hasta WPF. `formula-ag-fairy-elf` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Fairy Elf/Muse Elf/High Elf y
  conserva `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `20.6/20`, `21.1/21`, `21.1/21` y `21.6/21`; seis negativos cubren los
  cuatro stats por debajo de su base, familia y overflow. El programa
  materializa exactamente
  `ene * 0.2 + vit * 0.3 + agi * 0.2 + str * 0.3`.
- `CHECKED_DECIMAL_V1` conserva `0.2` y `0.3` exactamente, no redondea aportes
  y trunca una sola vez en `visible-ag`. Application materializa quince
  referencias; WPF reutiliza selección, contexto e intérpretes genéricos.
- Contrato técnico `fairy-elf-ag-formula-contract.md` y bitácora de `RES-0002`
  para identidad, aplicabilidad, cuatro inputs/límites, seis pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 591 archivos,
  149.051.441 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.12`,
  contiene 168 JSON y produce
  `sha256:432b6a062fbd5ed996a9d58dbfa32daafec5cf09c447dc7c40bb0ac98e645177`.
- Vertical funcional de AG de Dark Knight desde el claim `VERIFIED`
  `DR-AG-DARK-KNIGHT` hasta WPF. `formula-ag-dark-knight` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Dark Knight/Blade Knight/Blade
  Master y conserva `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `25.70/25`, `27.00/27`, `26.05/26` y `27.35/27`; seis negativos cubren los
  cuatro stats por debajo de su base, familia y overflow. El programa
  materializa exactamente
  `ene + vit * 0.3 + agi * 0.2 + str * 0.15`.
- `CHECKED_DECIMAL_V1` conserva `0.3`, `0.2` y `0.15` exactamente, consume
  Energy sin transformación, no redondea aportes y trunca una sola vez en
  `visible-ag`. Application materializa catorce referencias; WPF reutiliza
  selección, contexto e intérpretes genéricos.
- Contrato técnico `dark-knight-ag-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, cuatro inputs/límites, cinco pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 580 archivos,
  149.038.270 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.11`,
  contiene 157 JSON y produce
  `sha256:67d6fd0e614d3072f214b7dc09f7295e2450d2b4b7c0cd24a91f70a14dad13ef`.
- Vertical funcional de AG de Dark Wizard desde el claim `VERIFIED`
  `DR-AG-DARK-WIZARD` hasta WPF. `formula-ag-dark-wizard` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Dark Wizard/Soul Master/Grand
  Master y conserva `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `21.3/21`, `21.8/21`, `21.9/21` y `22.4/22`; seis negativos cubren los
  cuatro stats por debajo de su base, familia y overflow. El programa
  materializa exactamente
  `ene * 0.2 + vit * 0.3 + agi * 0.4 + str * 0.2`.
- `CHECKED_DECIMAL_V1` conserva `0.2`, `0.3` y `0.4` exactamente, no redondea
  aportes y trunca una sola vez en `visible-ag`. Application materializa trece
  referencias; WPF reutiliza selección, contexto e intérpretes genéricos.
- Contrato técnico `dark-wizard-ag-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, cuatro inputs/límites, seis pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- El smoke y la prueba contextual admiten fórmulas publicadas que no consumen
  nivel: usan nivel 1 sólo para componer un estado de progresión válido y
  verifican exclusivamente los inputs declarados por la fórmula.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 569 archivos,
  149.025.417 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.10`,
  contiene 146 JSON y produce
  `sha256:469f18aec26813dcb75dab054df3df7b2469d730d76d532ed2ef8432711e4651`.
- Vertical funcional de Mana de Dark Lord desde el claim `VERIFIED`
  `DR-MANA-DARK-LORD` hasta WPF. `formula-mana-dark-lord` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Dark Lord/Lord Emperor y conserva
  `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `40/40`, `41/41`, `41.5/41` y `42.5/42`; cuatro negativos cubren nivel,
  Energy, familia y overflow. El programa materializa exactamente
  `40 + (lvl - 1) + (ene - 15) * 1.5`.
- `CHECKED_DECIMAL_V1` conserva `1.5` exactamente, no redondea aportes y trunca
  una sola vez en `visible-mana`. Application materializa doce referencias y
  WPF reutiliza selección, contexto e intérpretes genéricos.
- Contrato técnico `dark-lord-mana-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, seis pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 558 archivos,
  149.012.089 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.9`,
  contiene 135 JSON y produce
  `sha256:440b7fb4a1ef1bd5b57202323bf7cb447c3bf252abe81cf112f8832220e7817a`.
- Vertical funcional de Mana de Magic Gladiator desde el claim `VERIFIED`
  `DR-MANA-MAGIC-GLADIATOR` hasta WPF.
  `formula-mana-magic-gladiator` `1.0.0` nace `PUBLISHED` contra schema `2.0.0`,
  aplica a Magic Gladiator/Duel Master y conserva `EVD-0021`, `EVD-0026` y el
  conflicto resuelto `DSP-0002`.
- Cuatro casos positivos reproducen 60/61/62/63 Mana y cuatro negativos cubren
  nivel, Energy, familia y overflow. El programa materializa exactamente
  `8 + (lvl - 1) + ene * 2`, sin desplazamiento de Energy ni redondeo
  intermedio.
- Application materializa once referencias; WPF reutiliza selección, contexto e
  intérprete genéricos. El contrato técnico y la bitácora de `RES-0002`
  conservan identidad, aplicabilidad, límites, cinco pasos, truncamiento y
  conflicto sin reclasificar fuentes.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 549 archivos,
  149.001.252 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.8`,
  contiene 126 JSON y produce
  `sha256:cff8d5726f433448ac7212a7a6e7465475024cc4346e904a649bcc2615e706f5`.
- Vertical funcional de Mana de Summoner desde el claim `VERIFIED`
  `DR-MANA-SUMMONER` hasta WPF. `formula-mana-summoner` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Summoner/Bloody
  Summoner/Dimension Master y conserva
  `EVD-0021`, `EVD-0026`, `EVD-0027`–`EVD-0029` y `EVD-0031`.
- Cuatro casos positivos reproducen raw/visible
  `40/40`, `41.5/41`, `41.7/41` y `43.2/43`; cuatro negativos cubren nivel,
  Energy, familia y overflow. El programa materializa exactamente
  `40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7`.
- `CHECKED_DECIMAL_V1` conserva `1.5` y `1.7` exactamente, no redondea aportes
  y trunca una sola vez en `visible-mana`. Application materializa diez
  referencias y WPF reutiliza selección, contexto e intérpretes genéricos.
- Contrato técnico `summoner-mana-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, siete pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 540 archivos,
  148.989.299 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.7`,
  contiene 117 JSON y produce
  `sha256:7d0e75d9212837a9245a339253b9622ecff2ec4b157cb839606601c3ee73331b`.
- Vertical funcional de Mana de Fairy Elf desde el claim `VERIFIED`
  `DR-MANA-FAIRY-ELF` hasta WPF. `formula-mana-fairy-elf` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Fairy Elf/Muse Elf/High Elf y
  conserva `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `30/30`, `31.5/31`, `31/31` y `32.5/32`; cuatro negativos cubren nivel,
  Energy, familia y overflow. El programa materializa exactamente
  `15 + (lvl - 1) * 1.5 + ene`.
- `CHECKED_DECIMAL_V1` conserva `1.5` exactamente, no redondea aportes y trunca
  una sola vez en `visible-mana`. Application materializa nueve referencias y
  WPF reutiliza selección, contexto e intérpretes genéricos.
- Contrato técnico `fairy-elf-mana-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, seis pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 531 archivos,
  148.977.675 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.6`,
  contiene 108 JSON y produce
  `sha256:78d8688b3b08a02b13ea9971fa45f90ff59612c6c5b1a2981cb102b82c1adc7e`.
- Vertical funcional de Mana de Dark Knight desde el claim `VERIFIED`
  `DR-MANA-DARK-KNIGHT` hasta WPF. `formula-mana-dark-knight` `1.0.0` nace
  `PUBLISHED` contra schema `2.1.0`, aplica a Dark Knight/Blade Knight/Blade
  Master y conserva `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen raw/visible
  `20/20`, `20.5/20`, `21/21` y `21.5/21`; cuatro negativos cubren nivel,
  Energy, familia y overflow. El programa materializa exactamente
  `10 + (lvl - 1) * 0.5 + ene`.
- `CHECKED_DECIMAL_V1` conserva `0.5` exactamente, no redondea aportes y trunca
  una sola vez en `visible-mana`. Application materializa ocho referencias y
  WPF reutiliza la selección genérica entre HP y Mana sin handlers factuales.
- Contrato técnico `dark-knight-mana-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, seis pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 522 archivos,
  148.967.275 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.5`,
  contiene 99 JSON y produce
  `sha256:868ef53f5238066e928f51a56e2f375124c8d2f7b2a5fb6d75078c61c557c120`.
- Vertical funcional de Mana de Dark Wizard desde el claim `VERIFIED`
  `DR-MANA-DARK-WIZARD` hasta WPF. `formula-mana-dark-wizard` `1.0.0` nace
  `PUBLISHED` contra schema `2.0.0`, aplica a Dark Wizard/Soul Master/Grand
  Master y conserva `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Cuatro casos positivos reproducen 60/62/62/64 Mana y cuatro negativos cubren
  nivel, Energy, familia y overflow. El programa materializa exactamente
  `(lvl - 1) * 2 + ene * 2`, sin constante base inferida ni redondeo
  intermedio.
- Application materializa siete fórmulas ejecutables y reproduce 28/28 trazas
  y 28/28 errores desde JSON. WPF incorpora una selección genérica por
  referencia exacta para resolver HP o Mana de Dark Wizard sin handlers ni
  constantes factuales en C#.
- Contrato técnico `dark-wizard-mana-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, cinco pasos,
  truncamiento, casos y ausencia explícita de conflictos.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 513 archivos,
  148.956.244 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.4`,
  contiene 90 JSON y produce
  `sha256:a663630cbf054bef3c71eeb969b0edcb2dc95ec686d6cb2663fee8a2c5af6089`.
- Vertical funcional de HP de Dark Lord desde el claim `VERIFIED`
  `DR-HP-DARK-LORD` hasta WPF. `formula-hp-dark-lord` `1.0.0` nace
  `PUBLISHED`, aplica a Dark Lord/Lord Emperor y conserva
  `EVD-0021`/`EVD-0026` sin añadir evidencia ni conflictos.
- Evolución compatible de `formula.schema.json` a `2.1.0` y modelo
  `CHECKED_DECIMAL_V1`. Conservan exactamente literales base 10 como `1.5`,
  aritmética comprobada y trazas fraccionarias; el redondeo se aplica sólo en
  `APPLY_ROUNDING` y la salida continúa siendo `INT64`.
- Cuatro casos positivos reproducen raw/visible
  `90/90`, `91.5/91`, `92/92` y `93.5/93`; cuatro negativos cubren nivel,
  Vitality, familia y overflow de salida. Application reproduce 24/24 trazas y
  24/24 errores de las seis referencias ejecutables.
- Cuatro pruebas sintéticas del intérprete decimal y un gate de contrato `2.1.0`
  fijan conservación de
  fracciones, ausencia de redondeo intermedio, truncamiento final, rechazo de
  operandos incoherentes y overflow. La solución alcanza 195/195 pruebas.
- Contrato técnico `dark-lord-hp-formula-contract.md` y bitácora de `RES-0002`
  para identidad, aplicabilidad, inputs, límites, representación exacta,
  truncamiento y casos, sin reclasificaciones nuevas.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 504 archivos,
  148.940.709 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.3`,
  contiene 81 JSON y produce
  `sha256:d7810c927ec692c161d0adcfcfa8cc6374d2213cac97805e9e77ec9d2ecefb32`.
- Vertical funcional de HP de Magic Gladiator desde el claim `VERIFIED`
  `DR-HP-MAGIC-GLADIATOR` hasta WPF. `formula-hp-magic-gladiator` `1.0.0`
  nace `PUBLISHED` contra el schema ejecutable `2.0.0`, aplica a Magic
  Gladiator y Duel Master, y conserva `EVD-0021`/`EVD-0026`.
- Cuatro casos positivos reproducen 110/111/112/113 HP y cuatro controles
  negativos cubren nivel, Vitality, familia y overflow. `DSP-0002` afecta
  Energy, no una entrada de esta fórmula, por lo que `conflictIds` permanece
  vacío.
- Application materializa cinco fórmulas ejecutables por referencia exacta y
  reproduce 20/20 trazas y 20/20 errores desde JSON. La solución alcanza
  190/190 pruebas sin añadir handlers ni constantes de Magic Gladiator a C#.
- WPF reutiliza la selección genérica por clase/evolución y muestra HP de Magic
  Gladiator con trazas contextual y aritmética. El smoke cubre cinco
  referencias ejecutables, veinte casos, 72/72 JSON y ambas fases.
- Contrato técnico `magic-gladiator-hp-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, seis pasos,
  redondeo, casos y alcance exacto del conflicto, sin evidencia ni
  reclasificaciones nuevas.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 495 archivos,
  148.920.099 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.2`,
  contiene 72 JSON y produce
  `sha256:a15ce32bc9610539ca406bdcdb80bbe7049f1879f0222014c9c10e28e30ed7aa`.
- Vertical funcional de HP de Summoner desde el claim `VERIFIED`
  `DR-HP-SUMMONER` hasta WPF. `formula-hp-summoner` `1.0.0` nace
  `PUBLISHED` contra el schema ejecutable `2.0.0`, aplica a Summoner, Bloody
  Summoner y Dimension Master, y conserva
  `EVD-0021`/`EVD-0027`–`EVD-0030` sin reclasificar el contraste.
- Cuatro casos positivos reproducen 70/71/72/73 HP y cuatro controles negativos
  cubren nivel, Vitality, familia y overflow. La definición conserva
  `conflictIds: []` porque no existe un conflicto aplicable documentado.
- Application materializa cuatro fórmulas ejecutables por referencia exacta y
  reproduce 16/16 trazas y 16/16 errores desde JSON. La solución alcanza
  182/182 pruebas sin añadir handlers ni constantes de Summoner a C#.
- WPF reutiliza la selección genérica por clase/evolución y muestra HP de
  Summoner con trazas contextual y aritmética. El smoke cubre cuatro
  referencias ejecutables, dieciséis casos, 63/63 JSON y ambas fases.
- Contrato técnico `summoner-hp-formula-contract.md` y bitácora de `RES-0002`
  para identidad, aplicabilidad, inputs, límites, seis pasos, redondeo, casos y
  ausencia explícita de conflictos, sin evidencia ni reclasificaciones nuevas.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 486 archivos,
  148.908.207 bytes y 10 avisos legales. El dataset avanza a `2026-07-26.1`,
  contiene 63 JSON y produce
  `sha256:afb77b6daa3112da782dbcc68685f0f7e5bc3cbb1ae8f9bf3f6d1f80d0b61dc8`.
- Vertical funcional de HP de Fairy Elf desde el claim `VERIFIED`
  `DR-HP-FAIRY-ELF` hasta WPF. `formula-hp-fairy-elf` `1.0.0` nace
  `PUBLISHED` contra el schema ejecutable `2.0.0`, aplica a Fairy Elf, Muse Elf
  y High Elf, y traza exclusivamente `EVD-0021`/`EVD-0026`.
- Cuatro casos positivos reproducen 80/81/82/83 HP y cuatro controles negativos
  cubren nivel, Vitality, familia y overflow. La definición conserva
  `conflictIds: []` porque no existe un conflicto aplicable documentado.
- Application materializa las tres fórmulas ejecutables por referencia exacta
  y reproduce 12/12 trazas y 12/12 errores desde JSON. La solución alcanza
  174/174 pruebas sin añadir handlers ni constantes de Fairy Elf a C#.
- WPF reutiliza la selección genérica por clase/evolución y muestra HP de Fairy
  Elf con trazas contextual y aritmética. El smoke cubre las tres referencias
  ejecutables, doce casos, 54/54 JSON y ambas fases de publicación.
- Contrato técnico `fairy-elf-hp-formula-contract.md` y bitácora de `RES-0002`
  para identidad, aplicabilidad, inputs, límites, seis pasos, redondeo, casos y
  ausencia explícita de conflictos, sin nueva evidencia ni reclasificaciones.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 477 archivos,
  148.896.977 bytes y 10 avisos legales. El dataset avanza a `2026-07-25.2`,
  contiene 54 JSON y produce
  `sha256:aa3c761e9c3a8a2739c2cf424175c5d5b2ee703793f1489d2b8ebbb823521afa`.
- Vertical funcional de HP de Dark Knight desde el claim `VERIFIED`
  `DR-HP-DARK-KNIGHT` hasta WPF. `formula-hp-dark-knight` `1.0.0` nace
  `PUBLISHED` contra el schema ejecutable `2.0.0`, aplica a Dark Knight, Blade
  Knight y Blade Master, y traza exclusivamente `EVD-0021`/`EVD-0026`.
- Cuatro casos positivos reproducen 110/112/113/115 HP y cuatro controles
  negativos cubren nivel, Vitality, familia y overflow. La definición conserva
  `conflictIds: []` porque no existe un conflicto aplicable documentado; el gate
  ya no exige inventar uno.
- Application y sus pruebas materializan las dos fórmulas ejecutables por
  referencia exacta y reproducen 8/8 trazas y 8/8 errores desde JSON. La
  solución alcanza 166/166 pruebas sin añadir handlers ni constantes de Dark
  Knight a C#.
- WPF reutiliza la selección genérica por clase/evolución y muestra HP de Dark
  Knight con trazas contextual y aritmética. El smoke cubre exactamente
  `formula-hp-dark-knight@1.0.0` y `formula-hp-dark-wizard@1.1.0`, ocho casos,
  45/45 JSON y ambas fases de publicación.
- Contrato técnico `dark-knight-hp-formula-contract.md` y bitácora de
  `RES-0002` para identidad, aplicabilidad, inputs, límites, seis pasos,
  redondeo, casos y ausencia explícita de conflictos, sin nueva evidencia ni
  reclasificaciones.
- Smoke WPF `win-x64` aprobado con SQLite `3.53.3`, 468 archivos,
  148.884.866 bytes y 10 avisos legales. El dataset avanza a `2026-07-25.1`,
  contiene 45 JSON y produce
  `sha256:11a3d88ed670f998ba8ff3d5c149aa2f4017ae9ef1dd4a34994f35644a7024b3`.
- Vertical funcional de HP de Dark Wizard desde snapshot hasta WPF.
  `FormulaInputDefinition` preserva `source.kind/valueId`; las clases
  materializadas conservan `baseValue` y `evidenceRefs`; Application construye
  un estado inmutable desde progresión/distribución y resuelve
  `character-level` y `resolved-{statId}` sin aceptar valores contextuales
  entregados por la UI.
- `FormulaContextValueResolver`, `ResolvedCharacterState` y
  `CalculateCharacterFormulaUseCase`, con traza contextual separada de la
  aritmética y seis códigos estables para mismatch, fuente no soportada, valor
  no resoluble, base/asignación ausentes y overflow comprobado.
- Ocho pruebas de Application para materialización fiel de bases/evidencias y
  `source.valueId`, reproducción 4/4 de los positivos canónicos por la ruta
  completa, errores de contexto, inmutabilidad y rechazo previo de nivel o
  asignación inválidos. La solución alcanza 158/158 pruebas.
- Resultado WPF de atributo derivado con referencia exacta y trazas contextual
  y aritmética. La fórmula aplicable se obtiene del catálogo publicado; cambiar
  progresión, resets o asignaciones invalida el resultado sin persistirlo en el
  borrador.
- Gate de publicación ampliado para reproducir los cuatro casos positivos de
  `formula-hp-dark-wizard` `1.1.0` mediante progresión, distribución y
  resolución contextual en las fases inicial y de reemplazo. El smoke local
  `win-x64` pasó con SQLite `3.53.3`, 459 archivos, 148.871.955 bytes, 10 avisos
  legales y 36/36 JSON idénticos.
- Diseño cerrado de resolución productiva de `source.kind: CONTEXT_VALUE`.
  Application queda como autoridad del estado calculado: `character-level`
  procede de progresión validada y cada `resolved-{statId}` de
  `checked(baseValue + allocation)` usando base/evidencia del snapshot y la
  asignación vigente de distribución. Define preservación de `source.valueId`,
  traza contextual, errores, provenance e invalidación antes de modificar
  código o WPF; no añade datos ni fórmulas de MU Online.
- Adaptador `JsonExecutableFormulaSnapshotReader` en Application para
  materializar definiciones schema `2.0.0` desde `character-classes/` y
  `formulas/`. Conserva `1.0.0` como historia no ejecutable y falla cerrado
  ante rulesets mezclados, referencias duplicadas, estado no publicado,
  aplicabilidad, inputs, aridades, orden, traza/redondeo o dependencias
  incoherentes.
- `ExecutableFormulaCatalog` inmutable y
  `CalculatePublishedFormulaUseCase`. La resolución usa exclusivamente
  `FormulaReference` exacta `id` + `version`, no elige la última versión, no
  acepta definiciones externas y delega en
  `CheckedIntegerFormulaInterpreter`; una referencia histórica o ausente
  produce `formula-not-executable`.
- Catorce pruebas de integración de Application para la vertical de fórmulas.
  Leen desde archivos los 4/4 casos positivos y 4/4 negativos
  `formula-hp-dark-wizard` `1.1.0`, comparan la traza completa, rechazan
  `1.0.0` y fijan selección exacta, estado, aplicabilidad, referencias hacia
  atrás y unicidad compuesta mediante copias temporales. No se duplican
  constantes ni resultados de HP en C#; Data y WPF permanecen sin cambios.
- Representación inmutable y ajena a JSON de fórmulas ejecutables en Domain:
  referencia/definición, aplicabilidad, inputs y bounds con código de rango,
  programa `CHECKED_INT64_V1`, pasos/operandos, redondeo, solicitud, resultado y
  traza con provenance.
- Intérprete genérico en Calculation Engine para `CONSTANT`, `ADD`, `SUBTRACT`,
  `MULTIPLY` y `APPLY_ROUNDING`. Exige estado publicado, aplicabilidad e inputs
  exactos, usa aritmética comprobada de 64 bits y no ramifica por fórmula,
  clase, input o paso.
- Veinticinco pruebas exclusivamente sintéticas para las cinco operaciones,
  estado, aplicabilidad, bounds/códigos, orden, seis modos de redondeo,
  programas inválidos/no soportados, overflow, traza/provenance e inmutabilidad
  de colecciones. Application, Data, WPF y los datos canónicos permanecen fuera
  de esta vertical.
- Smoke WPF publicado aprobado tras incorporar los nuevos binarios: SQLite
  `3.53.3`, 459 archivos, 148.808.995 bytes, 10 avisos legales y 36/36 JSON
  idénticos entre fases. Progresión, distribución, resets, backup/restore y
  borrador persistido permanecen aprobados; WPF aún no ejecuta fórmulas.
- Ocho casos versionados para `formula-hp-dark-wizard` `1.1.0`: cuatro
  positivos y cuatro controles negativos en archivos propios. Conservan IDs,
  contexto, inputs, trazas, resultados, errores, evidencia y conflicto de
  `1.0.0`; el único cambio semántico es `formulaRef.version`. La definición
  `DRAFT` enlaza exclusivamente sus cuatro positivos.
- Identidad compuesta de casos de fórmula mediante `id` + versión de
  `formulaRef`. El gate permite coexistir ambas series, rechaza una pareja
  repetida sin abortar la validación y prueba por comparación estructural que
  las ocho copias no divergen. `1.1.0` permanece `DRAFT`; no se creó evaluador
  ni se modificaron Domain, Calculation Engine, Application, Data o WPF.
- Smoke WPF publicado aprobado con 36/36 JSON idénticos entre publicación
  inicial y reemplazo, SQLite `3.53.3`, 459 archivos y 148.766.267 bytes. El
  hash resultante del snapshot fue
  `sha256:cfe267e51cf07532d5d1828fd524078bf832a122f367cf6381309e0d9010dbf7`.
- Definición canónica `formula-hp-dark-wizard` `1.1.0` en estado `DRAFT`
  contra `formula.schema.json` `2.0.0`. Conserva identidad factual,
  aplicabilidad, inputs, bounds, redondeo, cinco pasos, evidencia y conflicto
  de `1.0.0`; añade los códigos de rango aprobados y expresa la misma
  aritmética únicamente como `CHECKED_INT64_V1`. El artefacto publicado
  `1.0.0` y sus ocho casos permanecen intactos.
- Gate canónico consciente de `schemaVersion` e identidad compuesta
  `id` + `version`. Valida las versiones `1.0.0` y `1.1.0` contra `v1` y `v2`
  respectivamente, permite su coexistencia sin elegir una versión implícita y
  rechaza identidades compuestas duplicadas. No se crearon casos `1.1.0`, un
  evaluador ni cambios en Domain, Calculation Engine, Application, Data o WPF.
- Smoke WPF publicado aprobado con 28/28 JSON idénticos entre publicación
  inicial y reemplazo, SQLite `3.53.3`, persistencia y backup/restore intactos.
  La definición `DRAFT` se empaqueta como parte del snapshot, pero WPF no la
  materializa ni ejecuta.
- Contrato JSON Schema 2020-12 `packages/schemas/v2/formula.schema.json`
  `2.0.0`, coexistente con `v1`, para programas `CHECKED_INT64_V1` sin
  interpretar `strategy.definition`. Cierra inputs `INT32`/`INT64`, output y
  literales `INT64`, `rangeErrorCode`, operandos y las operaciones
  `CONSTANT`, `ADD`, `SUBTRACT`, `MULTIPLY` y `APPLY_ROUNDING`.
- Dos fixtures exclusivamente sintéticos `formula-v2` y gates de contrato para
  aridades, formas incompletas y texto ejecutable no permitido. Los gates
  semánticos rechazan IDs duplicados, inputs inexistentes, referencias a pasos
  actuales o futuros, bounds incompatibles y divergencias entre programa,
  traza, salida cruda y redondeo visible.
- Inventario integral ampliado a once contratos y veintidós fixtures, con
  selección explícita de versión en el validador, control estructural y harness
  de compilación fuente. No se modificaron ruleset, dataset, fórmula factual,
  casos canónicos, registros de investigación ni capas productivas.
- Diseño técnico de la primera vertical ejecutable de
  `formula-hp-dark-wizard`. Determina que `strategy.definition` `1.1.0` no
  permite ejecución cerrada, rechaza handlers factuales y parsing de texto
  implícito, y selecciona un programa estructurado `CHECKED_INT64_V1`.
- Límite explícito Domain/Application/Calculation Engine, gates de
  materialización, errores y estrategia de pruebas sin Data ni WPF. La fórmula
  publicada `1.0.0` permanece inmutable; el contrato `2.0.0` ya se implementó
  y la ruta posterior exige una fórmula `1.1.0` y casos equivalentes antes de
  crear el evaluador. No se añadieron datos ni fórmulas de MU Online.
- Definición factual `formula-hp-dark-wizard` `1.0.0`, incorporada inicialmente
  como `DRAFT` y publicada tras su revisión, con
  aplicabilidad exacta a Dark Wizard, Soul Master y Grand Master, inputs
  tipados, bounds técnicos/factuales, expresión aprobada, truncamiento visible,
  cinco pasos de traza, `EVD-0021`/`EVD-0026` y conflicto `DSP-0003`.
- Cuatro casos positivos y cuatro controles negativos de HP materializados bajo
  `reference-cases/formulas`, sin duplicar constantes o resultados en C#.
  Cubren base, incremento de nivel, incremento de Vitality, Grand Master, nivel
  inválido, Vitality bajo base, familia no aplicable y overflow técnico.
- Gate semántico factual de fórmulas en el validador integral. Comprueba
  identidad exacta, pertenencia al catálogo, inputs, pasos/outputs, redondeo,
  provenance y cobertura completa de positivos; cuatro mutaciones prueban el
  fallo cerrado. No se añadió motor, Application, Data ni WPF para HP.
- Contratos JSON Schema 2020-12 `formula.schema.json` `1.1.0`,
  `calculation-trace.schema.json` `1.0.0` y
  `formula-test-case.schema.json` `1.0.0`. Separan definición, ejecución y
  expectativa, con aplicabilidad, bounds clasificados, fuentes cerradas de
  inputs, pasos ordenados y referencias exactas de ID/versión.
- Seis fixtures exclusivamente sintéticos para los tres contratos. El caso
  positivo de fórmula resuelve mediante `$ref` una traza completa y la unión
  cerrada admite alternativamente un único `expectedErrorCode`; no se añadió
  ninguna fórmula ni caso factual al ruleset.
- Cobertura del validador integral, la prueba estructural y el harness de
  compilación fuente ampliada a 10 schemas y 20 fixtures. Pruebas focalizadas
  rechazan aplicabilidad vacía/duplicada, bounds factuales sin evidencia,
  procedencia incompleta, pasos duplicados, salidas fuera de la declaración y
  expectativas positivas/negativas ambiguas.
- Decisión técnica de contratos de fórmula, ya implementada:
  `formula.schema.json` avanzó a
  `1.1.0` con aplicabilidad, bounds clasificados, procedencia cerrada de inputs
  y pasos ordenados; `calculation-trace.schema.json` y
  `formula-test-case.schema.json` separarán ejecución y expectativas en
  `1.0.0`.
- Comparación de alternativas, responsabilidades de campos, versionado,
  referencias exactas y gates semánticos para fórmulas/trazas/casos. La
  implementación quedó limitada a schemas, fixtures y tooling; no se creó
  ninguna fórmula canónica ni código productivo.
- Diseño previo a implementación de `DR-HP-DARK-WIZARD` con ID candidato
  `formula-hp-dark-wizard` `1.0.0`, alcance sobre las tres evoluciones de la
  familia, entradas, límites técnicos, aritmética comprobada y traza explícita
  `base + nivel + Vitality → raw → truncamiento visible`.
- Ocho casos manuales propuestos para HP de Dark Wizard: base 60, incremento
  aislado de nivel, incremento aislado de Vitality, combinación en Grand Master,
  mínimos inválidos, familia no aplicable y overflow técnico. Permanecen fuera
  del ruleset y del motor hasta aprobar el diseño.
- Análisis de brecha de `formula.schema.json` `1.0.0` para aplicabilidad por
  clase/evolución, límites numéricos, procedencia del input, traza ordenada y
  contrato de casos de fórmula, sin modificar todavía schemas ni fixtures.
- EVD-0032 con la fórmula base de SD de Summoner y `defense = agi / 3`.
  El propietario confirma SD inicial 102 y truncamiento independiente de los
  tres términos antes de sumarlos. `DSP-0004` queda resuelto y `RES-0002`
  alcanza 24/24 claims `VERIFIED`.
- EVD-0031 conserva la corrección final del propietario a +1.7 Mana por punto
  de Energy para Summoner Season 4 y Mana 40 al nacer en nivel 1 con Energy 23,
  coincidente con Fanz; el valor 1.5 por Energy queda retirado. La confirmación
  de +1.5 Mana por nivel cierra
  `40 + (lvl - 1) * 1.5 + (ene - 23) * 1.7` y promueve el claim a `VERIFIED`.
- EVD-0030 con la corrección del propietario para Summoner: nivel 1,
  Vitality 18 = HP 70, +2 HP por cada punto adicional de Vitality y +1 HP por
  nivel. `DR-HP-SUMMONER` pasa a `VERIFIED` con
  `70 + (lvl - 1) + (vit - 18) * 2`; aún no se implementa en el motor.
- EVD-0027–EVD-0029 para los recursos faltantes de Summoner: MU Online Fanz,
  Webzen y MUonline Helper coinciden en HP 70, Mana 40, SD 102 y aumentos por
  nivel de HP/Mana. Los tres claims pasaron inicialmente a `PARTIAL`; EVD-0030
  cerró después HP. No se modifica el catálogo ejecutable.
- EVD-0026 como decisión explícita del propietario para el catálogo de fórmulas
  Season 4 global/inglés de las seis familias. Conserva expresiones de recursos,
  daño, wizardry, defensa, rates, regeneración, buffs, Fenrir, pets y clan, con
  truncamiento de la parte decimal en el valor mostrado.
- Veinticuatro claims `VERIFIED` en `RES-0002`: HP, Mana, AG y SD para las seis
  familias, incluida Summoner.
  HP y Mana de Summoner quedan `VERIFIED`; SD sigue `PARTIAL`.
- Catálogo Summoner aprobado para daño físico y wizardry, attack/defense rates,
  defensa, velocidad, AG y buffs Reflect, Berserker, Innovation y Weakness.
  Queda documentado para futuros contratos sin implementación productiva.
- Primer contraste de `DR-HP-DARK-WIZARD` en `RES-0002`: EVD-0022–EVD-0025
  trazan MU Online Fanz, Webzen, StrategyWiki e InfinityMU. El claim queda
  `PARTIAL` y `DSP-0003` conserva la divergencia de 1 frente a 2 HP por
  Stamina/Vitality; no se publican fórmulas, fixtures ni datos del ruleset.
- Registro de investigación `RES-0002` para HP, Mana, AG y SD: 24 claims
  separados por atributo y familia de clase, todos inicialmente `UNVERIFIED`,
  sin fórmulas ni evidencias. El gate exige versión, evolución aplicable,
  entradas, orden de operaciones, redondeo y casos reproducibles antes de
  modificar schemas, ruleset o motor.
- Configuración de resets aprobada por el propietario: cantidad y puntos por
  reset empiezan en cero, el motor calcula su producto con overflow controlado y
  suma esos puntos al presupuesto disponible para distribución sin modificar el
  ruleset Season 4.
- Controles WPF `Resets`, `Puntos por reset` y `Puntos totales por resets`, más
  desglose visible de progresión, resets, total distribuible, gasto y remanente.
- Cuatro códigos tipados para inputs negativos y overflow de resets; pruebas
  sintéticas cubren defaults, `2 × 100 = 200`, producto fuera de rango y suma
  fuera de rango.
- Persistencia y revalidación de los inputs de reset en borradores. El smoke
  publicado distribuye los 200 puntos configurados y reproduce el mismo
  resultado después de backup/restore y reemplazo del artefacto.
- Composición WPF de `SaveBuildDraftUseCase` y `LoadBuildDraftUseCase` con
  `SqliteBuildDraftRepository`. La base productiva vive en
  `%LOCALAPPDATA%\MuOnline.BuildPlanner\build-planner.sqlite` y aplica
  `SqliteBuildDraftMigrations.All` antes de construir el repositorio.
- Identidad explícita para borradores publicados: ruleset `1.0.0`, dataset
  `2026-07-24.1`, motor `0.1.0` y SHA-256 determinista sobre rutas relativas y
  bytes exactos de los 27 JSON empaquetados.
- Controles WPF mínimos para guardar/cargar por ID. La carga vuelve
  exclusivamente por Application, recalcula antes de repoblar la pantalla y
  muestra los seis códigos `build-draft-*`, incluido
  `build-draft-write-conflict`, junto con su explicación.
- Gate del artefacto publicado para guardar un borrador sintético en la base
  externa, revalidarlo, incluirlo en backup/restore y volver a cargarlo desde
  los binarios de reemplazo. Compara ID, metadata, hash, asignaciones, gasto y
  remanente sin añadir datos ni fórmulas de MU Online.
- Migración SQLite hacia adelante `1/create_build_drafts` y
  `SqliteBuildDraftRepository` como implementación Data de
  `IBuildDraftRepository`. Cada alta/reemplazo confirma payload JSON y metadata
  en una sola fila/transacción inmediata; la carga permanece como `SELECT` sin
  mutaciones.
- Traducción del agotamiento de `SqliteWriteContentionPolicy` al código estable
  `build-draft-write-conflict`, sin filtrar tipos SQLite hacia Application.
- Seis pruebas de integración Data con archivos temporales para payload y
  metadata exactos, reemplazo por ID, rollback ante fallo sintético, reapertura,
  ausencia sin mutaciones y conflicto de escritura tipado. No agregan datos ni
  fórmulas de MU Online.
- Modelo serializable `BuildDraft` en Application alineado campo por campo con
  el schema `1.0.0`, contexto runtime explícito para ruleset, dataset/hash y
  motor, y seis códigos de error estables `build-draft-*`.
- Puerto `IBuildDraftRepository` sin tipos SQLite y casos de uso de guardado y
  carga. La carga recalcula progresión y distribución desde las entradas y
  asignaciones, compara toda la caché y falla cerrada ante identidad,
  dependencia o resultado divergente.
- Siete pruebas sintéticas de Application con repositorio en memoria para
  alta/carga, reemplazo por ID, ausencia, identidad incoherente, dependencia no
  disponible, caché alterada y round-trip JSON exacto. Data, migraciones y WPF
  permanecen fuera de esta vertical.
- Contrato JSON Schema 2020-12 `build-draft.schema.json` `1.0.0` para identidad
  estable, metadata exacto de ruleset/dataset/motor, entradas de progresión y
  un `StatDistribution` compuesto mediante `$ref`.
- Fixtures sintéticos válido/inválido de borrador. El inválido conserva válido
  el envoltorio y falla por la distribución referenciada, demostrando que el
  validador resuelve el contrato compartido.
- Especificación previa a persistencia para autoridad de campos, revalidación al
  cargar, errores estables, límite Application/Data, transacción atómica y
  casos mínimos, sin crear puertos, tablas ni migraciones.
- Flujo WPF de distribución de stats: conserva el
  `ProgressionPointBudgetResult` calculado, genera los inputs desde los
  `StatIds` materializados e invoca `CalculateStatDistributionUseCase` sin
  recalcular progresión ni duplicar nombres o valores del juego.
- Resultado visible con puntos gastados, restantes y asignaciones por ID, más
  traducción en español de los seis errores tipados sin ocultar su código
  estable. Los cambios de identidad, nivel o Hero Status invalidan el
  presupuesto anterior.
- Gate del artefacto publicado para una distribución sintética de un punto
  derivada del snapshot empaquetado. Ambas fases verifican gasto, remanente y
  conjunto de stats sin agregar fixtures factuales ni persistencia.
- Caso de uso `CalculateStatDistributionUseCase` en Application: recibe el
  presupuesto existente y las asignaciones, resuelve una única definición de
  clase del mismo ruleset y delega en el motor sin aceptar clases alternativas
  ni recalcular progresión.
- Cuatro pruebas de integración recorren copia temporal del snapshot → catálogo
  → caso de uso → motor para distribuciones sintéticas parcial/exacta, fallo
  cerrado `budget-source-mismatch` ante origen incoherente y propagación del
  código `allocation-negative` producido por el motor.
- Operación pura `StatDistributionCalculator` en Calculation Engine para
  validar asignaciones no confiables y derivar puntos gastados/remanentes desde
  el presupuesto de progresión, con suma y resta comprobadas de 64 bits.
- Tipos de solicitud/resultado, excepción y seis códigos de error estables en
  Domain para negativos, stats ajenos u omitidos, exceso de presupuesto,
  overflow y divergencias de origen.
- Diez pruebas sintéticas de distribución cubren los seis casos mínimos del
  contrato, las divergencias de ruleset/clase/regla y overflow; una prueba de
  integración adicional confirma que Application materializa los IDs de stats
  directamente desde el snapshot.
- Contrato JSON Schema 2020-12 `stat-distribution.schema.json` `1.0.0` para
  conservar presupuesto ganado, referencia de progresión, asignaciones por
  stat, puntos gastados y remanente, más fixtures sintéticos válido/inválido.
- Especificación previa al código de invariantes, límite entero de 64 bits,
  errores estables y seis casos sintéticos mínimos para la futura distribución;
  `command` se resuelve exclusivamente desde los stats de la clase canónica.
- Cobertura del séptimo contrato en el validador .NET y en la comprobación
  estructural PowerShell: 7 schemas y 14 fixtures.
- Primer flujo funcional WPF para seleccionar clase, evolución, nivel y Hero
  Status desde el catálogo publicado, calcular mediante Application y mostrar
  total, regla/version y traza sin duplicar valores del juego en XAML o C#.
- Empaquetado del snapshot canónico completo en
  `rulesets/mu-s4-global-reference/v1` de la publicación WPF, con resolución
  exclusiva desde la carpeta del artefacto.
- Gate de publicación que carga el snapshot con el adaptador productivo,
  reproduce 7/7 casos positivos y 3/3 rechazos en ambas fases y compara los 18
  JSON por SHA-256 durante el reemplazo simulado.
- Proyecto `MuOnline.BuildPlanner.Application` con adaptador JSON para
  materializar el catálogo de progresión ya validado y caso de uso mínimo que
  invoca `ProgressionPointBudgetCalculator` sin depender de WPF ni SQLite.
- Gate productivo de snapshot con errores tipados para directorios o contenido
  inválidos, IDs duplicados, mezcla de rulesets, reglas no `PUBLISHED` y
  referencias clase/regla incoherentes.
- Doce pruebas de integración de Application que leen entradas y resultados
  desde los diez casos canónicos, reproducen 7/7 resultados y 3/3 rechazos, y
  demuestran fallo cerrado con una regla `REVIEWED` y una referencia inexistente
  en copias temporales.
- Primera vertical productiva del presupuesto de puntos en los proyectos
  `MuOnline.BuildPlanner.Domain` y `MuOnline.BuildPlanner.CalculationEngine`:
  resuelve por clase una única regla `PUBLISHED`, valida evolución, nivel y
  Hero Status, y devuelve puntos ganados con aportes trazables por nivel/quest.
- Errores de dominio tipados para clase/evolución/nivel, resolución de reglas y
  elegibilidad de quest, con los mismos códigos estables de los controles
  canónicos.
- Doce pruebas del motor cargan las definiciones JSON canónicas sin duplicar el
  ruleset: reproducen los siete casos positivos y tres rechazos de progresión,
  fijan la traza 1145+10 del caso de nivel 230 y demuestran que una regla no
  publicada no se ejecuta.
- Gate semántico para `testCaseRefs` de progresión: cada enlace debe resolver a
  un caso positivo del mismo ruleset y regla, y una regla publicada debe cubrir
  todos sus casos positivos. La prueba fija la asignación exacta 5+2 y mantiene
  fuera los tres controles negativos.
- Siete fixtures factuales versionados para los casos aprobados de progresión en
  nivel 1/220/221/230 y MG/DL en nivel 220, con IDs estables y provenance hacia
  `RES-0001`.
- Validación ejecutable de los casos de progresión en el tooling: confirma
  0/1095/1095/1101/1155/1533 puntos y rechaza controles de Hero Status con clase
  base, Magic Gladiator o Dark Lord.
- Ocho registros canónicos `VERIFIED` para
  `mu-s4-global-reference`: seis clases con stats/evoluciones trazados y las
  reglas `progression-five-per-level-hero-status` y
  `progression-seven-per-level`, separadas de los fixtures sintéticos.
- Validación integral de los registros canónicos contra los contratos de clase
  y progresión, más una prueba que fija el conjunto de ocho IDs estables.
- Documentación del paquete de ruleset, su convención de IDs y el gate de casos
  ejecutables mediante `testCaseRefs` resolubles.
- Contrato JSON Schema 2020-12 `progression-rule.schema.json` `1.0.0` para
  puntos por nivel y bonus opcional de quest con elegibilidad y retroactividad
  explícitas, más fixtures sintéticos válido e inválido.
- Cobertura del sexto contrato en el validador .NET, sus pruebas de contrato y
  la comprobación estructural PowerShell: 6 schemas y 12 fixtures.
- `EVD-0021` como decisión del propietario que fija la matriz completa de
  `RES-0001` como axioma estable del ruleset y habilita su implementación.
- `EVD-0019` y `EVD-0020` en `RES-0001`: transcripciones históricas Season 1/3
  con la matriz completa, seis guías fechadas en 2010 y auditoría CDX de sus
  primeras capturas disponibles, sin promover claims ni fixtures.
- `EVD-0014`–`EVD-0018` en `RES-0001` para las seis matrices de stats base,
  stats distribuibles, puntos por nivel, Hero Status/Marlon, contraste oficial
  de Webzen y confirmación del propietario.
- Matriz candidata `STR/AGI/VIT/ENE[/CMD]`, fórmulas de puntos acumulados y seis
  casos numéricos de investigación, sin promover fixtures productivos.
- `DSP-0002` para Energy inicial de Magic Gladiator: 26 en MU Online Fanz y la
  decisión del propietario frente a 16 en la guía actual de Webzen.
- Integración reproducible de Json Everything en el build normal del validador:
  commits, SDK `10.0.301`, locks fuente, SPDX, provenance, hashes y licencia MIT
  son entradas versionadas; los DLL se generan bajo `artifacts/` y no se guardan
  en Git.
- Control CI de publicación del validador que exige hashes revisados, aviso MIT,
  referencias directas y ausencia de `OSMFEULA.txt` y metadatos de los paquetes
  NuGet publicados.
- Spike reproducible de compilación propia de Json Everything desde los commits
  fuente MIT fijados: dos rutas independientes producen hashes idénticos,
  generan SBOM/provenance/locks, pasan dos veces 14/14 fixtures y rechazan una
  prueba aislada de formatos inválidos, sin cambiar todavía el grafo normal.
- Evaluación reproducible de Json Everything para el validador: registra el
  `OSMFEULA.txt` idéntico de los tres paquetes, su hash, commits fuente, alcance
  real y comparación entre binario NuGet, compilación propia MIT y Corvus.
- Copia normalizada a LF de `OSMFEULA.txt` bajo `legal/tooling/json-everything`
  para que las condiciones observadas no dependan de la caché local de NuGet.
- ADR-0005, aceptado por el propietario, que licencia el material original del
  proyecto bajo Apache License 2.0 y separa expresamente marcas, evidencias y
  contenido de terceros.
- Texto oficial Apache-2.0 en `LICENSE.md`, atribución y límites en `NOTICE`, e
  inventario inicial de dependencias en `THIRD-PARTY-NOTICES.md`.
- Reglas operativas para licenciar contribuciones y exigir licencia, avisos e
  inspección del artefacto antes de una distribución.
- Empaquetado legal de la publicación WPF con avisos del proyecto,
  Microsoft.Data.Sqlite, SQLitePCLRaw y runtime packs autocontenidos; el smoke
  verifica diez archivos no vacíos y sus hashes tras el reemplazo simulado.
- Proyecto WPF mínimo `MuOnline.BuildPlanner.App` para .NET 10 y `win-x64`, con
  ventana técnica sin datos del juego y referencia unidireccional hacia Data.
- Modo headless y script de smoke de publicación que verifican SQLite nativo,
  migración, round-trip, backup/restore, integridad y persistencia de una base
  externa a través de una copia de reemplazo de los binarios.
- Job Windows `wpf-publication-smoke` en CI para auditar dependencias, publicar y
  ejecutar el artefacto autocontenido en un runner limpio.
- ADR-0004, aceptado por el propietario, con comparación reproducible entre Blazor
  WebAssembly PWA y WPF .NET 10 para la primera UI/distribución; documenta
  offline, empaquetado, actualización, destinos/RIDs, SQLite nativo, reversión y
  pruebas requeridas sin implementar todavía una UI.
- Publicaciones de plantillas vacías en Release: Blazor PWA con 26,52 MiB/289
  archivos y WPF autocontenido `win-x64` con 139,26 MiB/400 archivos, registradas
  como medidas de empaquetado mínimo y no como estimaciones del producto.
- Spikes equivalentes de TypeScript/Node.js 24 y C#/.NET 10 con cálculo
  sintético trazable, validación, persistencia SQLite y pruebas reproducibles.
- ADR-0002, aceptado por el propietario, que selecciona C#/.NET 10.
- Schemas JSON 2020-12 `1.0.0` para evidencia, fórmula, clase de personaje,
  perfil de servidor y build, con diez fixtures sintéticos.
- Registro `RES-0001` para clases, evoluciones, estadísticas, puntos por nivel y
  Marlon, sin datos factuales publicados.
- Nota de alcance `season-4-reference-scope.md` con ID
  `mu-s4-global-reference`, reglas de evidencia y fronteras oficiales.
- `EVD-0005`, contraste técnico comunitario contemporáneo de códigos y cadenas
  de clase, conservado como `PARTIAL` por no declarar Season 4.
- `EVD-0006` y `EVD-0007`, dos guías oficiales de Webzen sobre quests y alas,
  junto con diez claims atómicos de familias/evoluciones clasificados `PARTIAL`.
- `EVD-0008` y `EVD-0009`, dos registros técnicos comunitarios, y una matriz de
  contraste técnico para `WZ-CLM-001`–`WZ-CLM-010`.
- `EVD-0010`, locator oficial del instalador inglés
  `MU1_03 full(Eng).exe` y un mirror archivado que lo clasifica como cliente
  Season 4, con resultado negativo para binario descargable y hasheable.
- `EVD-0011` y `EVD-0012`, auditoría de las páginas de segunda y tercera clase
  de MU Online Fanz y de sus primeras capturas archivadas disponibles.
- `DSP-0001`, inconsistencia nominal abierta entre `Soul Wizard` en la página de
  segunda clase y `Soul Master` como entrada de la página de tercera clase.
- `EVD-0013`, página de cuarta clase de MU Online Fanz que sitúa Soul Wizard
  después de Grand Master en el contenido actual, sin atribución a Season 4.
- Solución mínima .NET 10 y validador integral JSON Schema Draft 2020-12 para
  los cinco contratos `1.0.0` y sus diez fixtures sintéticos.
- Dos pruebas de contrato, restauración bloqueada y workflow CI para compilar y
  probar en Release mediante Microsoft Testing Platform.
- Evaluación comparativa de `Microsoft.Data.Sqlite`, EF Core SQLite y
  `System.Data.SQLite` para .NET 10, con licencias, soporte nativo, política de
  versiones, estrategia de migraciones y pruebas mínimas.
- ADR-0003, aceptado por el propietario, para usar
  `Microsoft.Data.Sqlite 10.0.10` con
  `SQLitePCLRaw.bundle_e_sqlite3 2.1.12` fijado, sin incorporar aún dependencias
  al producto.
- Smoke test aislado `win-x64` de SQLite: round-trip transaccional con SQLite
  `3.53.3` y auditoría transitiva sin vulnerabilidades para el grafo fijado.
- Proyecto `MuOnline.BuildPlanner.Data` con runner de migraciones SQLite,
  ledger de versión/nombre/SHA-256/fecha UTC, transacciones individuales y
  detección tipada de catálogos alterados o incompletos.
- Proyecto de integración con cinco pruebas sintéticas para base nueva,
  reapertura, reejecución, hash alterado y rollback ante fallo SQL.
- Lock files obligatorios para los cuatro proyectos y referencias productivas
  fijadas a `Microsoft.Data.Sqlite 10.0.10` y SQLitePCLRaw `2.1.12`.
- `SqliteBackupService` con backup online mediante copia candidata, verificación
  `PRAGMA integrity_check`, reemplazo posterior a validación y restauración
  verificada de la base completa.
- `SqliteBackupIntegrityException` y tres pruebas de integración sintéticas para
  consistencia, recuperación de schema/ledger/datos y preservación del último
  backup válido ante una candidata corrupta.
- `SqliteWriteContentionPolicy` con transacción inmediata, timeout positivo por
  intento, reintentos finitos para `SQLITE_BUSY`/`SQLITE_LOCKED`, restauración
  del timeout de conexión y error de agotamiento tipado.
- Cuatro pruebas sintéticas para rechazo de timeout ilimitado, espera acotada
  ante un segundo escritor, commit único después de liberar el bloqueo y
  rollback sin reintento de errores SQLite ajenos a contención.

### Changed

- La planificación de `next-actions.md` adopta verticales funcionales completas:
  diseño restante, implementación, integración, pruebas, documentación y smoke
  se agrupan cuando no existe un gate real. La prioridad inmediata une ahora la
  resolución de `CONTEXT_VALUE` con la entrega trazable de HP de Dark Wizard en
  WPF.
- `formula-hp-dark-wizard` `1.1.0` pasa de `DRAFT` a `PUBLISHED` después de
  revisar su definición y ocho casos contra el schema `2.0.0`, los contratos
  de traza/caso y el diseño aprobado. La auditoría no encontró divergencias:
  el único cambio del artefacto fue `status`; una prueba fija la promoción y el
  evaluador continúa fuera de alcance.
- `formula-hp-dark-wizard` `1.0.0` pasa de `DRAFT` a `PUBLISHED` después de una
  revisión limitada de sus nueve JSON contra el contrato aprobado. Identidad,
  provenance, aplicabilidad, inputs, bounds, expresión, cinco pasos, redondeo,
  cuatro casos positivos y cuatro controles negativos quedaron sin cambios; una
  prueba fija el nuevo estado y el motor continúa fuera de alcance.
- Se cierra la decisión abierta sobre aplicabilidad, límites, procedencia,
  traza y casos. El diseño de `DR-HP-DARK-WIZARD`, el plan de schemas y la
  estrategia de pruebas apuntan ahora al contrato técnico único documentado,
  sin alterar evidencia ni registros de investigación.
- Revisión técnica aprobada de `formula-hp-dark-wizard` `1.0.0` para ID,
  aplicabilidad, tipos, errores, traza y ocho casos manuales. Se corrigió la
  procedencia: `EVD-0026` autoriza la fórmula y `EVD-0021` sustenta Vitality 15
  como mínimo canónico; los extremos enteros quedan clasificados como límites
  técnicos, no como máximos factuales del juego. En ese cierre la estrategia de
  schemas permanecía pendiente; la decisión técnica posterior de este mismo
  bloque la resuelve sin materializar datos ni código.
- `DSP-0003` se resuelve por decisión del propietario a favor de
  `HP = 30 + (lvl - 1) + vit * 2` para la familia Dark Wizard. La evidencia
  contradictoria de MU Online Fanz conserva su clasificación individual.
- `stat-distribution.schema.json` y `build-draft.schema.json` avanzan a `1.1.0`
  para conservar inputs/producto de resets y total distribuible. El motor WPF
  avanza a `0.2.0`; ruleset `1.0.0` y dataset `2026-07-24.1` no cambian.
- La carga normaliza borradores `1.0.0` a resets cero en memoria y los revalida
  como `1.1.0` sin mutar el payload SQLite durante la lectura.
- El validador integral, la prueba de contrato, la comprobación PowerShell y el
  harness de compilación fuente cubren ahora 10 schemas y 20 fixtures; el
  harness autocompilado aprueba dos ejecuciones 20/20 y la prueba de formatos.
- `ProgressionPointBudgetResult` conserva ahora `CharacterClassId`, y
  `CharacterProgressionDefinition` los IDs de stats del snapshot, para validar
  la procedencia y disponibilidad sin duplicar datos factuales.
- `MuOnline.BuildPlanner.App` referencia Application de forma unidireccional y
  su lock registra sólo las nuevas dependencias internas de proyecto.
- Las reglas `progression-five-per-level-hero-status` y
  `progression-seven-per-level` pasan de `REVIEWED` a `PUBLISHED` tras enlazar
  respectivamente cinco y dos casos factuales aprobados.
- La matriz de `RES-0001` reemplaza los seis IDs provisionales por IDs
  definitivos con prefijo `class-` y registra la materialización canónica sin
  cambiar la clasificación individual de ninguna fuente.
- `RES-0001` queda cerrado: `CLM-0001`–`CLM-0006` pasan a `VERIFIED`; la
  búsqueda histórica deja de ser un gate para clases, evoluciones, stats base,
  puntos por nivel y Marlon.
- `DSP-0001` queda resuelto a favor de Dark Wizard → Soul Master → Grand Master,
  con Soul Wizard fuera de Season 4. `DSP-0002` conserva Energy 26 para Magic
  Gladiator y su divergencia documental.
- El modelo de confianza admite axiomas de ruleset aprobados explícitamente por
  el propietario, sin reclasificar la calidad o independencia de las fuentes.
- La política permanente de fuentes queda alineada con la decisión del
  propietario del 2026-07-19: Fanz sigue como fuente inicial prioritaria y se
  autorizan fuentes adicionales con provenance, versión y confianza propias.
- `RES-0001` pasa de 18 a 20 evidencias. `CLM-0002` y `CLM-0003` permanecen
  `PARTIAL` porque no se localizó un original de Webzen ni un snapshot
  contemporáneo que demuestre continuidad de la matriz hasta Season 4.
- `DSP-0002` queda resuelto por decisión explícita del propietario: el proyecto
  adopta `ENE 26` para Magic Gladiator (`26/26/26/26`). El valor 16 de Webzen se
  conserva como divergencia documental de otra versión no demostrada, sin
  bloquear el valor elegido ni promover el conjunto completo a `VERIFIED`.
- `CLM-0002`, `CLM-0003`, `CLM-0005` y `CLM-0006` pasan de no investigados a
  `PARTIAL`; los seis claims de `RES-0001` tienen ahora cobertura candidata y
  ninguno está todavía `VERIFIED`.
- Por decisión del propietario del 2026-07-19, MU Online Fanz continúa como
  fuente inicial prioritaria y se autorizan fuentes adicionales para extracción,
  contraste y resolución de conflictos con clasificación individual de versión.
- El validador dejó de referenciar `JsonSchema.Net` desde NuGet. Su lock normal
  sólo resuelve `Humanizer.Core 3.0.10`; `JsonSchema.Net.dll`,
  `JsonPointer.Net.dll` y `Json.More.dll` proceden de la compilación fuente
  fijada y pasan localmente 14/14 pruebas y la inspección de publicación.
- CI fija el SDK `.NET 10.0.301` para reproducir los hashes del pipeline fuente;
  la verificación del workflow actualizado en runner limpio queda pendiente.
- Los binarios NuGet de Json Everything quedan limitados al desarrollo interno
  histórico y ya no se resuelven en el grafo normal. La compilación propia desde
  los commits MIT fijados es la ruta integrada; Corvus queda como contingencia.
- El proyecto WPF obtiene los avisos de .NET/Windows Desktop/ASP.NET desde los
  runtime packs exactos resueltos por MSBuild, evitando acoplar la distribución
  a la versión instalada localmente.
- La licencia deja de ser una decisión abierta. Una release sigue condicionada
  a empaquetar y probar los avisos de los binarios realmente distribuidos; la
  familia Json Everything usada por el validador declara MIT para el código
  fuente pero incorpora `OSMFEULA.txt` para determinados usos de sus binarios.
- Por aclaración del propietario, `NOTICE` usa atribución colectiva a los
  contribuidores sin atribución personal adicional. La aplicación queda
  definida como herramienta de apoyo sin autenticación, cuentas ni credenciales
  de MU Online.
- Por decisión del propietario, el repositorio pasa a visibilidad pública.
  `main` queda protegido con PR obligatorio para administradores, checks
  estrictos `build-and-test` y `wpf-publication-smoke`, historial lineal,
  resolución de conversaciones y bloqueo de force-push/borrado. En ese momento
  la licencia seguía pendiente; ADR-0005 la resuelve posteriormente sin tratar
  la visibilidad como permiso sobre material de terceros.
- La rama predeterminada remota `chore/bootstrap-repository` se renombró a
  `main` conservando el commit `3935d9b`, y el tracking local quedó normalizado.
  GitHub rechazó activar branch protection con `403` porque el repositorio
  privado requiere GitHub Pro o visibilidad pública; no se cambió ninguna de
  esas opciones y el flujo de ramas/PR/checks queda como control operativo.
- Primera ejecución remota de CI aprobada en GitHub Actions (`run 29666817493`):
  14/14 pruebas en Linux y smoke WPF en Microsoft Windows Server 2025 con SDK
  .NET `10.0.302`, SQLite `3.53.3`, 407 archivos y 148.442.430 bytes. La auditoría
  de los cinco proyectos no encontró paquetes vulnerables y ambas fases del
  smoke pasaron; el alcance permanece limitado a `win-x64`.
- El repositorio remoto quedó inicializado con el commit raíz `2e886c3`.
- La solución incorpora la shell WPF y su lock file. El smoke local `win-x64`
  pasó con SQLite `3.53.3`, 407 archivos y 148.339.336 bytes publicados; la
  primera ejecución del job remoto de CI también quedó aprobada.
- Por decisión del propietario del 2026-07-18, WPF .NET 10 autocontenido y
  `win-x64` quedan seleccionados para la primera UI/distribución mediante
  ADR-0004; SQLite integrado y persistencia ante reemplazo de binarios quedaron
  validados por el smoke local y remoto.
- Por decisión del propietario, Webzen pasa a ser la fuente prioritaria para
  información de MU Online y se elimina el requisito de contemporaneidad; se
  mantienen provenance, clasificación por versión, confianza y contraste.
- Todo el proyecto adopta Season 4 global/inglés como versión objetivo única por
  orden del propietario; episodio y `main` dejan de ser requisitos.
- `RES-0001`, gobierno, producto, roadmap y handoff quedan alineados con Season
  4 global/inglés, sin exigir episodio, y mantienen `PARTIAL` los claims aún no
  demostrados.
- El manifiesto de integridad se actualiza para reflejar la migración documental.
- `RES-0001` mantiene `CLM-0001`/`CLM-0004` en `PARTIAL` y dirige la siguiente
  investigación a la aplicabilidad histórica de MU Online Fanz al corte objetivo.
- La matriz de `RES-0001` separa ahora las asociaciones nominales confirmadas
  por Webzen de su aplicabilidad aún no demostrada a Season 4.
- `CLM-0001` y `CLM-0004` permanecen `PARTIAL` tras el contraste técnico y la
  auditoría negativa de independencia y procedencia global/inglesa.
- La auditoría de `EVD-0005`, `EVD-0008` y `EVD-0009` separa autor visible,
  publicador y origen del contenido; su independencia técnica y procedencia
  global/inglesa quedan no demostradas y todos los claims siguen `PARTIAL`.
- Por decisión del propietario, MU Online Fanz pasa a ser la fuente obligatoria
  para nueva información factual del juego. Webzen y las fuentes históricas se
  conservan para contexto, procedencia y contraste, y ninguna página se aplica
  a Season 4 sin clasificación explícita de versión.
- La auditoría histórica de MU Online Fanz se cierra con resultado negativo para
  Season 4: no hay capturas indexadas anteriores a 2023, las primeras ya mezclan
  clases posteriores y ningún claim se promueve.
- La cadena candidata del mago se clasifica como Dark Wizard → Soul Master →
  Grand Master para Season 4 por aclaración del propietario; Soul Wizard queda
  fuera del corte. Grand Master queda clasificado como incorporación de Season
  4 obtenida al culminar a nivel 400 la serie de quests de tercera clase;
  EVD-0012 confirma la estructura actual, no la frontera histórica.
- La documentación de schemas describe ahora el validador integral, su CLI y
  los comandos reproducibles de prueba.
- La futura persistencia SQLite adopta, mediante ADR-0003, migraciones SQL hacia
  adelante con ledger/hash y backup
  previo a cambios destructivos, manteniendo el proveedor dentro de Data.
- La solución incorpora los proyectos Data e integración conservando la
  separación del dominio; todos los scripts actuales son fixtures sintéticos.

### Fixed

- `global.json` fija exactamente el SDK `10.0.301` con `rollForward: disable` y
  CI declara la versión como cadena. Así un SDK `10.0.302` preinstalado en el
  runner no reemplaza al SDK revisado durante el build reproducible.
- El harness de compilación fuente valida ahora los 20 fixtures de los diez
  contratos actuales en cada ejecución, en lugar de conservar inventarios
  anteriores.
- El pipeline reproducible de Json Everything fuerza checkouts LF y elimina
  símbolos/PDB de los DLL auditados. Así los hashes ya no dependen de
  `core.autocrlf` ni de checksums de fuentes propios del sistema operativo.
- GitHub Actions `run 29697921106` confirma la corrección en los jobs
  `build-and-test` y `wpf-publication-smoke`; el run anterior `29697684666`
  queda trazado como detección del defecto CRLF.
- Se fija `eol=lf` para archivos de texto mediante `.gitattributes`, evitando
  que checkouts con `core.autocrlf` invaliden los hashes de `MANIFEST.sha256`.
- El validador usa un registro de schemas aislado por ejecución y reutiliza cada
  contrato entre sus fixtures válido e inválido, evitando colisiones de `$id`.
- El proyecto de pruebas importa xUnit explícitamente y `dotnet test` selecciona
  Microsoft Testing Platform, por lo que CI descubre y ejecuta las pruebas.
- La evaluación SQLite evita el grafo transitivo predeterminado que resolvió
  `SQLitePCLRaw.lib.e_sqlite3 2.1.11` con `NU1903`; el pin aprobado a `2.1.12`
  restaura y audita sin vulnerabilidades conocidas.

## [0.1.0-docs] - 2026-07-18

### Added

- Constitución, alcance y reglas de gobierno.
- Requisitos funcionales y no funcionales.
- Roadmap maestro y criterios de salida.
- Arquitectura propuesta y modelo de dominio.
- Política de investigación, fuentes y confianza.
- Estrategia de pruebas, seguridad y calidad.
- Sistema de traspaso humano/IA.
