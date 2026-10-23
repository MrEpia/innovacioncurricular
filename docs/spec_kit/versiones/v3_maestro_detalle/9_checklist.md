# Lista de chequeo — Maestro-detalle

## A. Especificación

- [x] Cada requisito (MD1 a MD6) tiene su contrato en `6_contracts.md`.
- [x] Cada regla de la sección 4 de la spec tiene su código HTTP.
- [x] Las clarificaciones están resueltas dentro de la spec (C1 a C4).
- [x] El cambio de regla pasó por el Artículo 11: enmienda 1.4.0 con su D-MD-1.

## B. Criterios de aceptación

- [x] 1. Registro con 2 enfoques y 2 actividades → 201, y los hijos existen en sus tablas.
- [x] 2. Tercera actividad inexistente → 400 y el registro no existe.
- [x] 3. Procedimiento con la segunda actividad mala → THROW 50400 y no queda nada en la base.
- [x] 4. Programa con su detalle → 201, y la consulta lo devuelve igual.
- [x] 5. Característica inexistente → 400 y el programa no existe.
- [x] 6. 409 · 400 por detalle vacío · 400 por hijo repetido · 422 por hijo sin campos.
- [x] 7. Prueba de capas: ningún intento fallido llega al repositorio.
- [x] 8. Pantallas: guardar, ver el detalle y ver el error sin perder el formulario.

## C. Arquitectura

- [x] Ningún controlador tiene SQL. Ningún servicio conoce SqlClient.
- [x] Rutas bajo la tabla de su maestro, sin rutas genéricas (Artículo 10.1).
- [x] El detalle viaja como parámetro. Ningún valor se concatena en SQL.
- [x] Ningún secreto nuevo.

## D. Firmas

| Rol | Nombre | Fecha |
|---|---|---|
| Encargado del `main` | Sebastián Calvo | 2026-10-23 |
