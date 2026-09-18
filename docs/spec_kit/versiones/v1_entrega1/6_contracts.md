# Contratos HTTP — Entrega 1: los 7 endpoints, siete veces

> Las convenciones globales (sobre de lectura, sobre de error, catálogo de
> códigos, camelCase en el JSON) son las de
> [v1_aliado/6_contracts.md §0](../v1_aliado/6_contracts.md), sin cambios.
> Aquí solo se documenta la ruta y un ejemplo por tabla; el contrato de
> `aliado` ya está completo en ese documento y no se repite.

Patrón, igual para las siete: `GET /api/<tabla>[?limite=N]` ·
`GET /api/<tabla>/{id}` · `POST /api/<tabla>` · `PUT /api/<tabla>/{id}` ·
`PATCH /api/<tabla>/{id}` · `DELETE /api/<tabla>/{id}`.

## `area_conocimiento`

```
GET /api/area_conocimiento
→ 200 { "tabla":"area_conocimiento", "limite":1000, "total":218, "datos":[ … ] }
        ← nunca 204: la tabla viene con semillas (E3)

POST /api/area_conocimiento
body {"id":"9Z99","granArea":"Área de prueba","area":"Área de prueba",
      "disciplina":"Disciplina de prueba"}
→ 200 { "estado":200, "mensaje":"Registro creado exitosamente." }

body {"id":"9Z99","granArea":"X"}              ← faltan area y disciplina
→ 422 { "estado":422, "mensaje":"Datos inválidos.",
        "errores":["El campo area es obligatorio.",
                   "El campo disciplina es obligatorio."] }

PUT /api/area_conocimiento/9Z99
body {"granArea":"Área revisada","area":"Área revisada",
      "disciplina":"Disciplina revisada"}
→ 200 { "estado":200, "mensaje":"Registro reemplazado.", "filasAfectadas":1 }

PATCH /api/area_conocimiento/9Z99
body {"disciplina":"Solo esto cambia"}
→ 200 { "estado":200, "mensaje":"Registro actualizado.", "filasAfectadas":1 }

DELETE /api/area_conocimiento/9Z99
→ 200 { "estado":200, "mensaje":"Registro eliminado.", "filasAfectadas":1 }
DELETE /api/area_conocimiento/9Z99             ← segunda vez
→ 404
```

## `universidad`

```
GET /api/universidad
→ 200 { "tabla":"universidad", "limite":1000, "total":6, "datos":[ … ] }
        ← nunca 204: la tabla viene con semillas (E3)

POST /api/universidad
body {"id":9001,"nombre":"Universidad de Prueba","tipo":"Seccional","ciudad":"Pereira"}
→ 200 { "estado":200, "mensaje":"Registro creado exitosamente." }

PUT /api/universidad/9001
body {"nombre":"Universidad de Prueba S.A.S.","tipo":"Extensión","ciudad":"Manizales"}
→ 200 { "estado":200, "mensaje":"Registro reemplazado.", "filasAfectadas":1 }

body {"nombre":"X","tipo":"Y"}                 ← falta ciudad
→ 422

PATCH /api/universidad/9001
body {"ciudad":"Armenia"}
→ 200 { "estado":200, "mensaje":"Registro actualizado.", "filasAfectadas":1 }

DELETE /api/universidad/9001
→ 200 { "estado":200, "mensaje":"Registro eliminado.", "filasAfectadas":1 }
GET /api/universidad/9001                      ← ya inactivo
→ 404
```

## `aspecto_normativo`

```
GET /api/aspecto_normativo
→ 204 (sin cuerpo)                             ← arranca vacía

POST /api/aspecto_normativo
body {"id":1,"tipo":"Ley","descripcion":"Ley de prueba","fuente":"Congreso"}
→ 200 { "estado":200, "mensaje":"Registro creado exitosamente." }

GET /api/aspecto_normativo
→ 200 { "tabla":"aspecto_normativo", "limite":1000, "total":1, "datos":[ … ] }

PUT /api/aspecto_normativo/1
body {"tipo":"Decreto","descripcion":"Decreto de prueba","fuente":"Presidencia"}
→ 200 { "estado":200, "mensaje":"Registro reemplazado.", "filasAfectadas":1 }

body {"tipo":"Decreto"}                        ← faltan descripcion y fuente
→ 422

PATCH /api/aspecto_normativo/1
body {"fuente":"Ministerio"}
→ 200 { "estado":200, "mensaje":"Registro actualizado.", "filasAfectadas":1 }

DELETE /api/aspecto_normativo/1
→ 200 { "estado":200, "mensaje":"Registro eliminado.", "filasAfectadas":1 }
GET /api/aspecto_normativo
→ 204 (sin cuerpo)                             ← vuelve al estado inicial
```

## `practica_estrategia`

```
GET /api/practica_estrategia
→ 204 (sin cuerpo)                             ← arranca vacía

POST /api/practica_estrategia
body {"id":1,"tipo":"Metodología","nombre":"Aula invertida",
      "descripcion":"Estrategia de prueba"}
→ 200 { "estado":200, "mensaje":"Registro creado exitosamente." }

PUT /api/practica_estrategia/1
body {"tipo":"Metodología","nombre":"Aprendizaje basado en proyectos",
      "descripcion":"Descripción revisada"}
→ 200 { "estado":200, "mensaje":"Registro reemplazado.", "filasAfectadas":1 }

body {"tipo":"Metodología","nombre":"X"}       ← falta descripcion
→ 422

PATCH /api/practica_estrategia/1
body {"descripcion":"Solo esto cambia"}
→ 200 { "estado":200, "mensaje":"Registro actualizado.", "filasAfectadas":1 }

DELETE /api/practica_estrategia/1
→ 200 { "estado":200, "mensaje":"Registro eliminado.", "filasAfectadas":1 }
DELETE /api/practica_estrategia/1              ← segunda vez
→ 404
```

## `enfoque`

```
GET /api/enfoque
→ 204 (sin cuerpo)                             ← arranca vacía

POST /api/enfoque
body {"id":1,"nombre":"Enfoque de prueba","descripcion":"Descripción de prueba"}
→ 200 { "estado":200, "mensaje":"Registro creado exitosamente." }

PUT /api/enfoque/1
body {"nombre":"Enfoque revisado","descripcion":"Descripción revisada"}
→ 200 { "estado":200, "mensaje":"Registro reemplazado.", "filasAfectadas":1 }

body {"nombre":"X"}                            ← falta descripcion
→ 422

PATCH /api/enfoque/1
body {"descripcion":"Solo esto cambia"}
→ 200 { "estado":200, "mensaje":"Registro actualizado.", "filasAfectadas":1 }

DELETE /api/enfoque/1
→ 200 { "estado":200, "mensaje":"Registro eliminado.", "filasAfectadas":1 }
GET /api/enfoque
→ 204 (sin cuerpo)
```

## `car_innovacion`

```
GET /api/car_innovacion
→ 204 (sin cuerpo)                             ← arranca vacía

POST /api/car_innovacion
body {"id":1,"nombre":"Innovación de prueba",
      "descripcion":"Texto largo sin límite de longitud (VARCHAR(MAX), E2)",
      "tipo":"Curricular"}
→ 200 { "estado":200, "mensaje":"Registro creado exitosamente." }

body {"id":1,"nombre":"X","tipo":"Y"}          ← falta descripcion
→ 422

PUT /api/car_innovacion/1
body {"nombre":"Innovación revisada","descripcion":"Descripción revisada","tipo":"Pedagógica"}
→ 200 { "estado":200, "mensaje":"Registro reemplazado.", "filasAfectadas":1 }

PATCH /api/car_innovacion/1
body {"tipo":"Tecnológica"}
→ 200 { "estado":200, "mensaje":"Registro actualizado.", "filasAfectadas":1 }

DELETE /api/car_innovacion/1
→ 200 { "estado":200, "mensaje":"Registro eliminado.", "filasAfectadas":1 }
DELETE /api/car_innovacion/1                   ← segunda vez
→ 404
```
