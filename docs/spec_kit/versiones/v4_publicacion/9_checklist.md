# Lista de chequeo — Versión 4

## A. Especificación

- [x] Cada requisito del enunciado de la entrega 4 tiene su RF (tabla de la sección 2 de la spec).
- [x] Las clarificaciones (C1 a C5) están resueltas dentro de la spec.
- [x] Los cambios de regla pasaron por el Artículo 11: enmienda 1.5.0 (D-v4-1, D-v4-4).

## B. Criterios de aceptación

- [x] 1. 10 reportes con filas y columnas; todos de 4 tablas o más.
- [x] 2. Un registro retirado sale de los reportes.
- [x] 3. Resumen y series cuadran; `maximo=0` → 400.
- [x] 4. 401 sin token; `/reportes` y `/dashboard` piden sesión.
- [x] 5. Swagger documenta los 17 endpoints nuevos con su descripción.
- [x] 6. Las 5 páginas corporativas son públicas y ofrecen «Ingresar».
- [x] 7. Logo, colores y tipografía del manual.
- [x] 8. PWA: manifiesto instalable, service worker, `offline.html`.
- [x] 9. Reportes y dashboard en el navegador; dashboard y menú a 375 px.
- [x] 10. `publicar.py`: base, API y front publicados arrancan en Production (prueba local).
- [x] 11. Regresión: `verificar_todo.py --limpio` en verde.
- [x] Sitio publicado en MonsterASP y URL en el README (por HTTP; HTTPS pedido a soporte).

## C. Arquitectura y secretos

- [x] SQL de las 10 consultas a la vista en el repositorio; ningún paquete nuevo.
- [x] Una ruta por reporte y por gráfico, sin rutas genéricas.
- [x] `.env.produccion` y `publicacion/salida/` en el `.gitignore`.
- [x] Las páginas públicas no llaman a la API.

## D. Firmas

| Estudiante | Revisó | Fecha |
|---|---|---|
| Sebastián Calvo (encargado del `main`) | | |
| David Epia | | |
