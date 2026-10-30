# Lista de chequeo — Versión 2

> Se marca **solo con evidencia**: el comando que se corrió y su resultado.
> Una casilla marcada sin evidencia es la forma más rápida de reprobar el
> criterio de Especificación.

## A. Compuertas de la especificación

- [ ] No queda ningún `[NECESITA ACLARACIÓN]` en `2_spec.md`
- [ ] El chequeo de constitución de `3_plan.md` está hecho, y las enmiendas que pidió están en la constitución 1.1.0
- [ ] `5_data_model.md` y `6_contracts.md` coinciden con el esquema y con Swagger

## B. Criterios de aceptación (2_spec.md §5)

| # | Criterio | Evidencia | ✔ |
|---|---|---|---|
| 1 | `GET /` → v2 · Swagger con 22 recursos | | |
| 2 | Cadena completa por la API, cada POST 201 | `humo_academico.py`, `humo_actividades.py`, `humo_puentes.py` | |
| 3 | FK a padre inexistente o retirado → 400, nada creado | humo de cada bloque | |
| 4 | Padre con hijos activos → 409; sin hijos → 200 | humo de cada bloque | |
| 5 | Llave repetida → 409 | humo de cada bloque | |
| 6 | Mismo cuerpo: PUT 422 · PATCH 200 | humo de cada bloque | |
| 7 | Reglas de negocio → 400 | humo de cada bloque | |
| 8 | Borrado lógico: fila con `activo = 0` | consulta directa a SQL Server | |
| 9 | Prueba de capas sin SQL Server | `dotnet run` en `api_innovacion/pruebas` | |
| 10 | Pantallas y listas desplegables desde la API | recorrido de `7_quickstart.md` §4 | |
| 11 | Regresión de la v1 | `humo_front.py` | |

## C. Git

- [ ] Ningún commit directo a `main`: todo entró desde las ramas `feature/…`
- [ ] Cada estudiante con su identidad (`git config user.email` = el de su cuenta de GitHub)
- [ ] Commits pequeños y descriptivos (`feat:`, `fix:`, `docs:`, `test:`)
- [ ] `tag v2` sobre `main`

## D. Firmas

| Estudiante | Revisó | Fecha |
|---|---|---|
| Sebastián Calvo (encargado del `main`) | | |
| David Epia | | |
