# Cómo se construyó la v4 con IA — el prompt usado

> Igual que la [GUIA_IA2](../v2_entrega2/GUIA_IA2.md) y la
> [GUIA_IA3](../v3_seguridad/GUIA_IA3.md): queda escrito qué se le pidió a la
> IA, cómo se supervisó y qué se corrigió. *El chat se cierra, la guía queda.*

## 0. Ficha

| | |
|---|---|
| Camino | **B — IDE agéntico**: Claude Code (modelo Claude Opus) |
| Quién lo usó | Sebastián Calvo, encargado del `main` |
| Fecha | 2026-11-05 |
| Rama | `feature/sebastian-calvo-entrega4` |

## 1. El prompt (tal cual)

El estudiante pegó el enunciado de la entrega 4 del curso, precedido de una línea:

```text
para la cuarta entrega se debe cumplir con estos requisitos;

Aplicativo WEB completo con imagen corporativa, componente de reportes/consultas, dashboard y
publicación en un servidor gratuito. Todas las ramas de GitHub deben estar integradas en la rama
principal.
Entregables del Proyecto API (Backend):
• 10 consultas multitabla (mínimo 4 tablas por consulta) expuestas como endpoints.
• Endpoints para alimentar el dashboard con datos agregados.
• API completamente funcional y documentada.
Entregables del Proyecto Frontend:
• Páginas corporativas completas (Home, Productos/Servicios, Soporte, Contacto, Sobre Nosotros).
• Imagen corporativa de la empresa hipotética (logo, colores, tipografía).
• Módulo de reportes con los resultados de las 10 consultas multitabla.
• Dashboard con gráficos y visualizaciones.
• Todas las páginas deben ser responsive.
• Si es posible, implementar características PWA.
Requisitos de publicación:
El aplicativo completo debe estar publicado y accesible en un servidor gratuito. […]
ASP.NET Core: MonsterASP, SmarterASP.NET, Azure App Service (capa gratuita), Somee.com […]
• Todas las ramas de GitHub integradas en la rama principal.
• Entregar: URL del sitio publicado y enlaces a los repositorios GitHub.
```

Las reglas de trabajo vienen del prompt principal de la GUIA_IA2, que sigue
vigente:
- no suplantar al compañero;
- no subir secretos;
- no afirmar que algo pasó sin ejecutarlo;
- una rama por cambio.

## 2. Qué hizo la IA y cómo se comprobó

| Paso | Evidencia |
|---|---|
| Revisó que todas las ramas remotas estuvieran integradas | `git rev-list --count main..rama` = 0 en las 13 ramas |
| Datos de demostración (`v4_001`) | La migración corre en `db/init.sh` y los conteos aparecen en la base |
| 10 consultas multitabla + dashboard en la API | Cada endpoint respondió con datos antes de seguir con el front |
| Gráficos | Antes de diseñarlos, cargó una guía de visualización. Por eso hay barras horizontales de una serie y no donas, con el valor escrito y una tabla accesible (D-v4-3) |
| Front: páginas corporativas, reportes, dashboard, PWA | Revisado en el navegador, en escritorio y a 375 px |
| Script de publicación | Probado en local: base vacía → `bd`, `api`, `front` → ambos publicados arrancaron en Production y el dashboard cargó a través de ellos |
| Pruebas | `PruebasReportes.cs` (+9), `humo_reportes.py` (74) y `verificar_todo.py --limpio` en verde |

## 3. Lo que salió mal y a dónde fue cada corrección

| Error | Destino | Corrección |
|---|---|---|
| `app.css` tenía un acento **azul** desde la v1, ajeno al manual | **Constitución**, Artículo 12 (ya lo exigía) | Los alias de color apuntan a `marca.css` (D-v4-5) |
| Los miles salían «14,472» (formato inglés) | Supervisor | Cultura `es-CO` en el front: «14.472» |
| El campo «Usuario» se veía angosto en el celular: el componente no pone `type="text"` | Supervisor | El estilo también cubre `input:not([type])` |
| La página `Servicios.razor` se llamaba igual que el espacio de nombres `FrontInnovacion.Servicios` | Supervisor | Renombrada a `PaginaServicios.razor` |
| La API publicada no respondía en el puerto de prueba: `Program.cs` fija `UseUrls(8072)` | Supervisor | En IIS el puerto lo pone el servidor y ese `UseUrls` se ignora. En la prueba local bastó exponer el 8072 |
| El plan gratuito de MonsterASP permite **un** sitio y el sistema tiene dos aplicaciones | **Spec** (C5) y research (D-v4-7) | Dos cuentas: la API y la base con Calvo, el front con Epia |

## 4. Lo que la IA NO puede hacer y queda para el equipo

Crear las cuentas en el hosting y subir los `.zip`: exige registrarse con
datos personales y aceptar términos. Los pasos exactos están en
[7_quickstart.md](7_quickstart.md) §2.
