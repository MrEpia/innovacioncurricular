# Auditoría inicial — Neon, Render y revisión documental V2

Fecha: 2026-11-13 (America/Bogota). Base: `24abf0491b5f29aaf0ec346a489570b58140aafa`.

## Git y colaboración
Repositorio privado con permiso WRITE, clonado sin cambios locales previos. Rama `deploy/neon-render-spec-kit-v2` creada desde main; no se han cambiado otras ramas ni el sitio MonsterASP. Hay 16 ramas feature remotas además de main. El historial reciente integra la entrega 4, reportes, PWA, publicación y pruebas. No existe AGENTS.md en el árbol clonado.

## Arquitectura real
- API: C# / ASP.NET Core net10.0, controladores por recurso, servicios e interfaces, repositorios Dapper con SQL explícito. No hay Entity Framework ni ORM de entidades.
- Frontend: Blazor Server net10.0, proceso separado; HTTP desde el servidor al backend mediante ClienteApi. Razor Components, cookies con almacén de tickets/JWT en memoria, antifalsificación, PWA con service worker.
- Datos: SQL Server, 25 tablas (22 del dominio y usuario, rol, rol_usuario); 4 migraciones históricas. Catálogos de universidad y área de conocimiento; datos V4 de demostración. Retiros lógicos, relaciones y PK simples/compuestas. SQL y procedimientos en db/; fuentes del curso en ProyectosDeAula/.
- Inyección: api_innovacion/Ensamblador y front_blazor/Ensamblador. Controllers → IServicio → IRepositorio; frontend sin driver de base.
- Seguridad: JwtBearer 10.0.13, PBKDF2 SHA256, 100000 iteraciones; roles consulta/editor/administrador. Filtro global por verbo; usuario/rol/rol_usuario solo administrador. Login anónimo. Cookie HttpOnly/SameSite=Lax, expiración fija, antifalsificación en logout.
- Paquetes iniciales API: Microsoft.Data.SqlClient 5.2.2; Dapper 2.1.35; Swashbuckle.AspNetCore 6.6.2; JwtBearer 10.0.13. Front sin paquetes externos.
- Infraestructura inicial: docker-compose con SQL Server e inicializador, API y frontend con dotnet watch para desarrollo. Los Dockerfile actuales son de desarrollo; Render requiere imágenes publicadas de runtime.
- Configuración: .env.example, appsettings.json de ambos procesos, docker-compose.yml y publicacion/.env.produccion.example. No se copian secretos ni se muestra el contenido de credenciales locales.
- Pruebas: ejecutable de capas api_innovacion/pruebas/PruebaCapas.csproj; ocho scripts de humo HTTP y verificar_todo.py. No ejecutar --limpio ni apagar servicios existentes: son destructivos o afectan el entorno compartido.

## Hallazgos que impiden el despliegue solicitado
1. Neon ofrece PostgreSQL, pero todos los repositorios usan SqlClient/T-SQL: TOP, OUTPUT INSERTED, TRY_CAST, ISNULL, STRING_AGG WITHIN GROUP y procedimientos almacenados. Cambiar solo la cadena no funciona.
2. El script base SQL Server contiene DROP TABLE: nunca ejecutarlo sobre Neon o una base existente. La migración PostgreSQL debe ser independiente, transaccional, con historial y comprobación de checksum.
3. Puerto fijo de API 8072: debe respetar PORT de Render. Frontend debe recibir UrlApi explícita; no usar localhost en producción Render.
4. Faltan Dockerfile de producción, blueprint y health check que compruebe la conexión real. El diagnóstico / no toca la base.
5. Controllers devuelven ex.Message en 500 y el bootstrap registra la excepción completa: ocultar detalles internos en producción.
6. Sesiones del frontend en memoria: reiniciar/desplegar cierra sesiones; escalar a varias instancias requiere un almacén compartido. Plan inicial de una instancia; documentar limitación.
7. No se ha ejecutado transferencia de datos desde MonsterASP. Neon debe ser un entorno nuevo y separado; conservar los datos originales.

## Estado de proveedores observado
- Render: conector operativo, una cuenta/workspace disponible. Solo servicio inventario preexistente (otro repositorio): no modificarlo. No hay servicio innovacioncurricular en el inventario inicial.
- Neon: herramientas anunciadas; llamadas list_organizations y list_projects responden Unknown tool. No se ha confirmado acceso ni creado recurso alguno.
- Browser: Computer Use se detuvo porque no pudo determinar la URL de Chrome con confianza. No continuar automatización de esa ventana en este turno.

## Spec Kit real
Es una metodología del curso documentada en docs/SDD_SPECKIT.md, sin .specify ni evidencia de instalación del CLI oficial. Constitución 1.5.0; versiones archivadas v1_aliado, v1_entrega1, v2_entrega2, v3_seguridad, v3_maestro_detalle y v4_publicacion. La revisión solicitada «V2» será un directorio nuevo de revisión de despliegue basado en V4: no renombrar ni sobrescribir la V2 histórica. Conservar numeración 2_spec, 3_plan, 4_research, 5_data_model, 6_contracts, 7_quickstart, 8_tasks y 9_checklist. Documentar la ampliación a Npgsql y funciones PostgreSQL como enmienda motivada por esta misión.

## Inventario documental inicial
Todos los Markdown versionados se leyeron para inventariar títulos y longitud; los contratos/constitución/configuración y SQL se contrastaron con el código. Los estados históricos «terminado» no constituyen evidencia de un despliegue nuevo.

| Archivo Markdown | Líneas | Propósito/título existente |
|---|---:|---|
| `MANUAL_DE_MARCA.md` | 239 | Manual de Identidad Visual Corporativa |
| `PLAN_V1.md` | 915 | Plan — construir la v1 del módulo Innovación Curricular con la plantilla del curso |
| `ProyectosDeAula/docs/0_METODOLOGIA.md` | 364 | Proyecto de aula — Metodología de trabajo (SDD, versiones, Git y secretos) |
| `ProyectosDeAula/docs/CONCEPTOS_ELICITACION.md` | 186 | Cómo se le pregunta al cliente para que salgan las historias |
| `ProyectosDeAula/docs/CONCEPTOS_HISTORIAS_DE_USUARIO.md` | 164 | Historias de usuario — qué son y cómo se escriben |
| `ProyectosDeAula/docs/CONCEPTOS_PLAN_DE_DESARROLLO.md` | 142 | El plan de desarrollo de software — qué es y qué lleva |
| `ProyectosDeAula/docs/entrevista_gestion_profesoral.md` | 241 | Entrevista con el cliente — Módulo de Gestión Profesoral |
| `ProyectosDeAula/docs/entrevista_innovacion_curricular.md` | 247 | Entrevista con el cliente — Módulo de Innovación Curricular |
| `ProyectosDeAula/docs/entrevista_investigacion.md` | 274 | Entrevista con el cliente — Módulo de Investigación |
| `ProyectosDeAula/docs/entrevista_mapa_conocimiento.md` | 247 | Entrevista con el cliente — Módulo de Mapa de Conocimiento |
| `ProyectosDeAula/docs/historias_usuario_gestion_profesoral.md` | 267 | Historias de usuario — Módulo de Gestión Profesoral |
| `ProyectosDeAula/docs/historias_usuario_innovacion_curricular.md` | 262 | Historias de usuario — Módulo de Innovación Curricular |
| `ProyectosDeAula/docs/historias_usuario_investigacion.md` | 287 | Historias de usuario — Módulo de Investigación |
| `ProyectosDeAula/docs/historias_usuario_mapa_conocimiento.md` | 282 | Historias de usuario — Módulo de Mapa de Conocimiento |
| `ProyectosDeAula/docs/modulo_gestion_profesoral.md` | 86 | Proyecto de aula — Módulo: Gestión Profesoral |
| `ProyectosDeAula/docs/modulo_innovacion_curricular.md` | 93 | Proyecto de aula — Módulo: Innovación Curricular |
| `ProyectosDeAula/docs/modulo_investigacion.md` | 85 | Proyecto de aula — Módulo: Investigación |
| `ProyectosDeAula/docs/modulo_mapa_conocimiento.md` | 87 | Proyecto de aula — Módulo: Mapa de Conocimiento |
| `ProyectosDeAula/docs/plan_desarrollo_gestion_profesoral.md` | 299 | Plan de Desarrollo de Software |
| `ProyectosDeAula/docs/plan_desarrollo_innovacion_curricular.md` | 299 | Plan de Desarrollo de Software |
| `ProyectosDeAula/docs/plan_desarrollo_investigacion.md` | 299 | Plan de Desarrollo de Software |
| `ProyectosDeAula/docs/plan_desarrollo_mapa_conocimiento.md` | 299 | Plan de Desarrollo de Software |
| `ProyectosDeAula/docs/proyecto_completo.md` | 85 | Proyecto de aula — Proyecto Completo (los 6 módulos integrados) |
| `README.md` | 139 | Módulo Innovación Curricular |
| `db/migraciones/LEAME.md` | 12 | Migraciones |
| `docs/CONCEPTOS_DOCKER.md` | 1118 | Conceptos de Docker — imagen, contenedor, volumen, compose y Kubernetes |
| `docs/CONCEPTOS_ELICITACION.md` | 186 | Cómo se le pregunta al cliente para que salgan las historias |
| `docs/CONCEPTOS_HISTORIAS_DE_USUARIO.md` | 164 | Historias de usuario — qué son y cómo se escriben |
| `docs/CONCEPTOS_IDENTIDAD_VISUAL.md` | 187 | Identidad visual corporativa — qué es y por qué obliga al software |
| `docs/CONCEPTOS_PLAN_DE_DESARROLLO.md` | 142 | El plan de desarrollo de software — qué es y qué lleva |
| `docs/FLUJO_DE_UNA_PETICION.md` | 159 | El flujo de una petición — dónde está el GET, dónde se captura el POST |
| `docs/PARADIGMA_POO.md` | 367 | El paradigma de Programación Orientada a Objetos (P.O.O.) — en C# |
| `docs/PRINCIPIOS_ACID.md` | 116 | Los principios ACID, vistos en este proyecto |
| `docs/PROGRAMACION_ASINCRONICA.md` | 157 | Programación asincrónica en la web — qué resuelve y qué pasa si no se usa |
| `docs/PRUEBAS_Y_CALIDAD_DE_PRUEBAS.md` | 457 | Pruebas y calidad de las pruebas |
| `docs/SDD_SPECKIT.md` | 766 | SDD y Spec Kit — la metodología de este curso |
| `docs/SOLID_CAPAS_PATRONES.md` | 337 | SOLID, programación por capas y patrones de diseño — en este proyecto |
| `docs/spec_kit/1_constitution.md` | 332 | Constitución del proyecto — Módulo Innovación Curricular |
| `docs/spec_kit/DIAGRAMAS_DE_SECUENCIA.md` | 756 | Diagramas de secuencia — un diagrama por método |
| `docs/spec_kit/versiones/0_mapa_versiones.md` | 68 | Mapa de versiones — Módulo Innovación Curricular |
| `docs/spec_kit/versiones/v1_aliado/2_spec.md` | 169 | Especificación — Versión 1: `aliado` + SQL Server |
| `docs/spec_kit/versiones/v1_aliado/3_plan.md` | 365 | Plan técnico — Versión 1: `aliado` (C# / ASP.NET Core) |
| `docs/spec_kit/versiones/v1_aliado/4_research.md` | 183 | Decisiones — Versión 1 |
| `docs/spec_kit/versiones/v1_aliado/5_data_model.md` | 80 | Modelo de datos — Versión 1: la base dada y `aliado` |
| `docs/spec_kit/versiones/v1_aliado/6_contracts.md` | 178 | Contratos HTTP — Versión 1: los 7 endpoints exactos |
| `docs/spec_kit/versiones/v1_aliado/7_quickstart.md` | 179 | Quickstart — Versión 1: arranque y smoke test |
| `docs/spec_kit/versiones/v1_aliado/8_tasks.md` | 137 | Tareas — Versión 1: `aliado` |
| `docs/spec_kit/versiones/v1_aliado/9_checklist.md` | 92 | Lista de chequeo de requisitos — Versión 1 |
| `docs/spec_kit/versiones/v1_aliado/GUIA_IA1.md` | 216 | Cómo construir la v1 con IA — por chat o con un IDE agéntico |
| `docs/spec_kit/versiones/v1_entrega1/2_spec.md` | 145 | Especificación — Entrega 1: las 7 tablas sin clave foránea |
| `docs/spec_kit/versiones/v1_entrega1/5_data_model.md` | 105 | Modelo de datos — Entrega 1: las siete tablas |
| `docs/spec_kit/versiones/v1_entrega1/6_contracts.md` | 186 | Contratos HTTP — Entrega 1: los 7 endpoints, siete veces |
| `docs/spec_kit/versiones/v1_entrega1/8_tasks.md` | 96 | Tareas — Entrega 1: las seis tablas restantes |
| `docs/spec_kit/versiones/v1_entrega1/9_checklist.md` | 69 | Lista de chequeo de requisitos — Entrega 1 (7 tablas) |
| `docs/spec_kit/versiones/v2_entrega2/2_spec.md` | 194 | Especificación — Entrega 2: las 15 tablas con clave foránea |
| `docs/spec_kit/versiones/v2_entrega2/3_plan.md` | 105 | Plan técnico — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/4_research.md` | 153 | Decisiones — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/5_data_model.md` | 259 | Modelo de datos — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/6_contracts.md` | 400 | Contratos HTTP — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/7_quickstart.md` | 80 | Quickstart — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/8_tasks.md` | 114 | Tareas — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/9_checklist.md` | 41 | Lista de chequeo — Versión 2 |
| `docs/spec_kit/versiones/v2_entrega2/GUIA_IA2.md` | 346 | Cómo se construyeron las entregas 2 y 3 con IA — el prompt usado |
| `docs/spec_kit/versiones/v2_entrega2/HISTORIAS_DE_USUARIO.md` | 138 | Historias de usuario — Versión 2 |
| `docs/spec_kit/versiones/v3_maestro_detalle/2_spec.md` | 132 | Especificación — Maestro-detalle: registro calificado y programa |
| `docs/spec_kit/versiones/v3_maestro_detalle/3_plan.md` | 76 | Plan técnico — Maestro-detalle |
| `docs/spec_kit/versiones/v3_maestro_detalle/4_research.md` | 74 | Decisiones — Maestro-detalle |
| `docs/spec_kit/versiones/v3_maestro_detalle/5_data_model.md` | 47 | Modelo de datos — Maestro-detalle |
| `docs/spec_kit/versiones/v3_maestro_detalle/6_contracts.md` | 99 | Contratos HTTP — Maestro-detalle |
| `docs/spec_kit/versiones/v3_maestro_detalle/7_quickstart.md` | 46 | Quickstart — Maestro-detalle |
| `docs/spec_kit/versiones/v3_maestro_detalle/8_tasks.md` | 16 | Tareas — Maestro-detalle |
| `docs/spec_kit/versiones/v3_maestro_detalle/9_checklist.md` | 32 | Lista de chequeo — Maestro-detalle |
| `docs/spec_kit/versiones/v3_seguridad/2_spec.md` | 171 | Especificación — Entrega 3: JWT, sesiones y control de acceso por roles |
| `docs/spec_kit/versiones/v3_seguridad/3_plan.md` | 63 | Plan técnico — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/4_research.md` | 139 | Decisiones — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/5_data_model.md` | 51 | Modelo de datos — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/6_contracts.md` | 90 | Contratos HTTP — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/7_quickstart.md` | 59 | Quickstart — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/8_tasks.md` | 64 | Tareas — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/9_checklist.md` | 39 | Lista de chequeo — Versión 3 |
| `docs/spec_kit/versiones/v3_seguridad/GUIA_IA3.md` | 75 | Cómo se construyó y se cerró la entrega 3 con IA — los prompts usados |
| `docs/spec_kit/versiones/v3_seguridad/HISTORIAS_DE_USUARIO.md` | 60 | Historias de usuario — Versión 3 |
| `docs/spec_kit/versiones/v4_publicacion/2_spec.md` | 160 | Especificación — Entrega 4: reportes, dashboard, sitio corporativo y publicación |
| `docs/spec_kit/versiones/v4_publicacion/3_plan.md` | 87 | Plan técnico — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/4_research.md` | 126 | Decisiones — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/5_data_model.md` | 67 | Modelo de datos — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/6_contracts.md` | 112 | Contratos HTTP — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/7_quickstart.md` | 82 | Quickstart — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/8_tasks.md` | 37 | Tareas — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/9_checklist.md` | 36 | Lista de chequeo — Versión 4 |
| `docs/spec_kit/versiones/v4_publicacion/GUIA_IA4.md` | 82 | Cómo se construyó la v4 con IA — el prompt usado |
