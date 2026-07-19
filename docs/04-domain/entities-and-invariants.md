# Entidades e invariantes

## CharacterBuild
- Una clase solo usa stats declarados por su definición.
- Ningún stat puede ser menor al base.
- Puntos gastados no superan presupuesto salvo modo experimental explícito.
- Evolución y quests deben cumplir prerrequisitos configurados.

## ItemInstance
- Debe referenciar una definición del ruleset activo.
- Nivel y opciones dentro de rangos permitidos.
- Opciones mutuamente excluyentes no pueden coexistir.
- Sockets no exceden ranuras.

## ServerProfile
- No puede modificar campos marcados `locked`.
- Todo override declara motivo y autor.
- Debe validar contra versión del schema.

## FormulaDefinition
- No puede publicarse sin redondeo explícito.
- Todas las dependencias deben existir y ser acíclicas, salvo iteraciones documentadas.
- Una versión publicada es inmutable.
