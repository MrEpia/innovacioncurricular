# Tareas — Versión 4

## 0. El reparto

| Estudiante | Qué hace |
|---|---|
| Sebastián Calvo (encargado del `main`) | Spec kit de la v4 y enmienda 1.5.0 · datos de demostración · 10 consultas y dashboard en la API · Swagger documentado · reportes, dashboard, páginas corporativas, imagen y PWA en el front · pruebas · script de publicación · **publicar API, front y base** en su cuenta |
| David Epia | Revisar las páginas en el celular · probar el sitio publicado con los tres roles |

## 1. Construcción (Calvo) — rama `feature/sebastian-calvo-entrega4`

- [x] `v4_001_datos_demo.sql` (idempotente)
- [x] API: `ReportesController` (catálogo + 10 rutas), `DashboardController` (resumen + 6 series), servicios, repositorios y ensamblador
- [x] Swagger con título, versión y los comentarios `///` (XML del compilador)
- [x] Front: 5 páginas corporativas públicas · `/aplicacion` · `/reportes` · `/dashboard` · `GraficoBarras`
- [x] Imagen: alias de `app.css` a `marca.css`, Merriweather e Inter, logo del manual
- [x] Responsive: menú plegable, rejillas que se apilan, sin desplazamiento horizontal a 375 px
- [x] PWA: manifiesto, íconos, service worker, `offline.html`
- [x] `PruebasReportes.cs` (+9) y `humo_reportes.py` (74), incluidos en `verificar_todo.py`
- [x] `publicacion/publicar.py`, probado en local: base nueva → API y front publicados en modo Production
- [x] Constitución 1.5.0, este spec kit, diagramas 22 a 26 y `GUIA_IA4.md`

## 2. Publicación (Calvo, en su cuenta)

- [x] Sitios `innovacion-api.runasp.net` e `innovacion-umv.runasp.net`, base MSSQL `db79553`
- [x] `publicar.py bd` (25 tablas, 29 procedimientos, datos) · `api` · `front`, y los zips subidos
- [x] Comprobado desde fuera: API v4, Swagger, páginas públicas, PWA, y front ↔ API ↔ base (ingreso con usuario inventado → «incorrectos»)
- [ ] Ticket a soporte: activar HTTPS en los dos subdominios, y volver a publicar el front con `https://`
- [x] Recorrido con el administrador sobre el sitio publicado: ingreso, dashboard y reportes (2026-11-12)
- [ ] David: recorrido con los roles editor y consulta sobre el sitio publicado

## 3. Cierre (Calvo)

- [x] Las URL publicadas en el README
- [ ] Todas las ramas integradas en `main` (ninguna con commits fuera de `main`)
- [ ] `9_checklist.md` firmado
- [x] `git tag v4` · `git push origin v4`
