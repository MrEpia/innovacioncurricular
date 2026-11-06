# Decisiones — Versión 4

> **D-v4-1** y **D-v4-4** enmiendan la constitución (1.4.0 → 1.5.0).

## D-v4-1 — Una ruta por reporte y por gráfico *(enmienda 1.5.0 del Artículo 10)*

**Decisión.** `GET /api/reportes/programas-acreditados`, `…/plan-de-estudios`,
etc., y `GET /api/dashboard/programas-por-sede`, etc. Cada acción tiene su
tipo de fila, y por eso Swagger documenta sus columnas.

**Alternativa descartada.** `/api/reportes/{numero}`. Es la ruta genérica que
prohíbe el Artículo 10.1: no se puede documentar y deja pedir un reporte que
no existe.

## D-v4-2 — El SQL de los reportes, escrito completo en el repositorio

**Decisión.** Las 10 consultas viven en `RepositorioReportesSqlServer`, cada
una visible. Filtran `activo = 1` en **cada** tabla unida: si solo filtraran
la tabla principal, un programa retirado seguiría apareciendo a través de sus
hijos.

**Alternativa descartada.** Vistas de SQL Server. Esconderían la consulta
fuera del código que la usa y obligarían a otra migración por cada cambio.
Los procedimientos se reservan para operaciones atómicas (D-v3-10).

## D-v4-3 — Gráficos en HTML y CSS, barras horizontales de una sola serie

**Decisión.** `GraficoBarras.razor` dibuja barras con `div` y CSS:
- **Una sola serie por gráfico**, en el verde institucional.
- **Valor escrito al final de cada barra**, en tinta.
- Al pasar el mouse sobre una barra aparece su dato.
- «Ver los datos» abre la misma información como tabla.

**Por qué.**
- **Sin dependencias:** no hay biblioteca que descargar, se adapta solo al ancho y funciona con la PWA.
- **Se lee mejor:** la barra horizontal compara cantidades mejor que una dona y le cabe la etiqueta larga («Facultad de Ciencias Empresariales»).
- **No hace falta una paleta de varios colores:** el manual de marca no la define.

**Alternativa descartada.** Chart.js por CDN. Es otra dependencia, necesita
JavaScript e interop con Blazor y no se ve sin red.

## D-v4-4 — Páginas corporativas públicas y estáticas *(enmienda 1.5.0 del Artículo 10)*

**Decisión.**
- `/`, `/servicios`, `/nosotros`, `/soporte` y `/contacto` llevan `[AllowAnonymous]`, sin `@rendermode`, y no llaman a la API.
- La aplicación se mueve de `/` a `/aplicacion`.
- Después de ingresar, el usuario llega a `/aplicacion`.

**Por qué.** El enunciado pide páginas corporativas y que el login se vea
desde la página principal. Si fueran estáticas y sin datos, abrirlas al
público no expone nada.

## D-v4-5 — Los alias de color de la aplicación apuntan a la marca

**Decisión.** En `app.css`, `--acento`, `--oscuro`, `--error` y `--exito` se
redefinen con las variables de `marca.css`, y la barra pasa a verde. Se
cargan Merriweather e Inter desde Google Fonts, que es la fuente que cita el
manual.

**Por qué.** Desde la v1, `app.css` declaraba un acento azul (#1f6feb) que no
existe en el manual: era un incumplimiento del Artículo 12. Si Google Fonts no
carga, la pila de `marca.css` cae a Georgia o a la fuente del sistema.

## D-v4-6 — PWA honesta con Blazor Server

**Decisión.**
- **Lo que se instala:** el manifiesto (con íconos generados del isotipo) y un service worker que guarda solo los estáticos de la marca.
- **Sin conexión:** muestra `offline.html`.
- **Lo que nunca se guarda:** `/_blazor`, `/ingresar`, `/sesion` ni las respuestas de la API, porque son datos de la sesión de una persona.

**Por qué.** Blazor Server pinta en el servidor por una conexión viva: no
puede mostrar datos sin red. Prometer más sería falso. El enunciado dice
«si es posible»: lo posible es instalable y con una página sin conexión.

## D-v4-7 — Publicación en MonsterASP.NET, en dos cuentas

**Contexto.** El enunciado sugiere MonsterASP, SmarterASP.NET, Azure App
Service o Somee para ASP.NET Core. El sistema necesita SQL Server.

**Decisión.** MonsterASP.NET, por estas razones:
- **Plan gratuito sin tarjeta:** incluye una base MSSQL de 1 GB.
- **Corre .NET 10.**
- **Subdominio con HTTPS:** `*.runasp.net`.

El plan gratuito permite **un sitio por cuenta**, así que:
- la **API y la base** van en la cuenta de Sebastián Calvo;
- el **front** va en la de David Epia.

El front llama a la API por su URL pública, con HTTPS y Bearer, desde el
servidor (Blazor Server). Por eso no hace falta CORS.

**Cómo llegan los secretos.** `publicar.py` compila en Release y escribe las
variables de entorno en el `web.config` que genera `dotnet publish`: IIS se
las entrega a la aplicación. El `.env.produccion` y la carpeta `salida/` no
se versionan (Artículo 7).

**Alternativas descartadas.**
- **Azure App Service:** la capa gratuita exige tarjeta de crédito.
- **Somee:** pone publicidad en el sitio gratuito.
- **SmarterASP.NET:** es una prueba de 60 días, no un plan gratuito.

**Precio, documentado.**
- 256 MB de RAM: si la aplicación los supera, IIS la reinicia y las sesiones abiertas se pierden (la sesión vive en memoria, D-v3-7). Hay que volver a ingresar.
- El servidor está en Europa.

## D-v4-8 — Datos de demostración como migración

**Decisión.** `v4_001_datos_demo.sql` siembra 6 facultades, 12 programas y
sus hijos en todas las tablas. Cada fila entra solo si su llave no existe.
Las llaves son pequeñas y fijas; las pruebas de humo usan llaves aleatorias
de 6 cifras, así que no chocan.

**Por qué.** La base del curso solo trae dos catálogos llenos. Sin datos, los
reportes responderían 204 y el dashboard saldría vacío, y no habría nada que
mostrar en la sustentación ni en el sitio publicado.
