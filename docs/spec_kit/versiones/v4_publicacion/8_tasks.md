# Tareas — Versión 4

## 0. El reparto

| Estudiante | Qué hace |
|---|---|
| Sebastián Calvo (encargado del `main`) | Spec kit de la v4 y enmienda 1.5.0 · datos de demostración · 10 consultas y dashboard en la API · Swagger documentado · reportes, dashboard, páginas corporativas, imagen y PWA en el front · pruebas · script de publicación · **publicar la API y la base** |
| David Epia | **Publicar el front** en su cuenta de MonsterASP · revisar las páginas en el celular · probar el sitio publicado con los tres roles |

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

## 2. Publicación

- [ ] **Calvo:** cuenta, sitio de la API, base MSSQL · `publicar.py bd` y `publicar.py api` · subir `api.zip`
- [ ] **Epia:** cuenta y sitio del front · `publicar.py front` con la `URL_API` de Calvo · subir `front.zip`
- [ ] **Los dos:** recorrido de `7_quickstart.md` §2.4 sobre el sitio publicado

## 3. Cierre (Calvo)

- [ ] Las URL publicadas en el README
- [ ] Todas las ramas integradas en `main` (ninguna con commits fuera de `main`)
- [ ] `9_checklist.md` firmado · `git tag v4` · `git push origin v4`
