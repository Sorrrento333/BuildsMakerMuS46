# Contratos de API propuestos

- `GET /rulesets`
- `GET /profiles`
- `POST /profiles/resolve`
- `GET /characters`
- `GET /items`
- `POST /builds/validate`
- `POST /calculations/run`
- `POST /comparisons/run`
- `GET /encyclopedia/search`
- `GET /formulas/{id}`
- `GET /sources/{id}`

## Cálculo
La solicitud incluye IDs y valores, nunca nombres como identidad. La respuesta incluye:
- versión del motor;
- ruleset/profile resuelto;
- resultados;
- advertencias/errores;
- trace ID o traza embebida;
- hashes de datos.
