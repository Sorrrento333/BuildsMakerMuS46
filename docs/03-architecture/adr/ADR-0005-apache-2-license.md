# ADR-0005: Apache License 2.0 para el proyecto

- Estado: Aceptado por el propietario.
- Fecha: 2026-07-19.
- Responsables: propietario del proyecto; análisis asistido por IA.
- Decisión aprobada: propietario del proyecto, 2026-07-19.

## Contexto

El repositorio pasó a visibilidad pública sin que esa visibilidad concediera
permiso de reutilización. Una release permanecía bloqueada hasta elegir entre
MIT, Apache-2.0 o una licencia propietaria y separar los derechos sobre material
original de los correspondientes a MU Online y fuentes externas.

## Opciones consideradas

- **MIT:** permisiva y breve, sin una concesión de patentes expresa en su texto.
- **Apache-2.0:** permisiva, con concesión expresa de patentes, terminación
  defensiva y obligaciones de conservación de licencia, cambios y avisos.
- **Propietaria:** conserva restricciones de reutilización, pero limita la
  colaboración y distribución abierta buscadas para el repositorio público.

## Decisión

Licenciar bajo **Apache License, Version 2.0** el código, la documentación y el
demás material original aportado al proyecto. El texto oficial se conserva en
`LICENSE.md` y la atribución general y el límite sobre terceros en `NOTICE`.
La atribución se expresa colectivamente a los contribuidores del proyecto, sin
reservar una mención personal adicional para el propietario inicial.

Las contribuciones enviadas intencionalmente para incorporarse al repositorio se
aceptan bajo Apache-2.0 salvo declaración explícita en contrario, de acuerdo con
la sección 5. No se añaden cabeceras masivas por archivo: la distribución raíz
mantiene `LICENSE.md`, `NOTICE` y la declaración visible del README.

La licencia no concede derechos sobre marcas de Webzen, MU Online, contenido
copiado de fuentes externas ni evidencias sujetas a otras condiciones. Tampoco
relicencia dependencias. `THIRD-PARTY-NOTICES.md` registra la auditoría inicial y
los controles todavía requeridos antes de distribuir binarios.

## Consecuencias

- Se elimina el bloqueo de licencia propia del repositorio.
- El proyecto queda abierto a continuidad y expansión comunitaria; la autoría
  se conserva mediante el historial y la atribución colectiva.
- Reutilizadores deben cumplir Apache-2.0, incluida la conservación de avisos
  aplicables y la identificación de archivos modificados al redistribuir.
- Toda distribución debe incluir `LICENSE.md` y `NOTICE`.
- Una release WPF continúa bloqueada hasta completar los avisos de los binarios
  efectivamente incluidos. Cualquier distribución que incluya el validador
  requiere además resolver la aplicabilidad de `OSMFEULA.txt` presente en la
  familia Json Everything restaurada.
- La política de provenance y los límites de uso de fuentes permanecen sin
  cambios.

## Verificación posterior — 2026-07-19

La publicación WPF copia los avisos propios y de dependencias productivas. Para
el runtime autocontenido, MSBuild obtiene `LICENSE` y `THIRD-PARTY-NOTICES` de
los runtime packs .NET, Windows Desktop y ASP.NET exactos seleccionados por la
restauración.

El smoke local pasó con 417 archivos y 148.506.472 bytes, SQLite `3.53.3` y diez
archivos legales no vacíos. Sus hashes SHA-256 coincidieron en la publicación
inicial y en la copia usada para simular el reemplazo de binarios. El validador
no se incluyó en el artefacto.

## Plan de reversión

El propietario puede relicenciar versiones futuras o material sobre el que
conserve todos los derechos, mediante un nuevo ADR y revisión jurídica. No se
pueden retirar retroactivamente los permisos ya concedidos sobre versiones
publicadas bajo Apache-2.0 ni relicenciar aportes de terceros sin autoridad.

## Fuentes

- [Texto oficial de Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.txt).
- [Guía de aplicación de Apache License 2.0](https://www.apache.org/legal/apply-license).
- [Registro OSI de Apache-2.0](https://opensource.org/license/apache-2-0).
