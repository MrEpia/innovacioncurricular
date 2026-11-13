# ASTRA — estado persistente del trabajo

Actualizado: 2026-11-13 (America/Bogota). **MISIÓN INCOMPLETA**: publicación y verificación externas pendientes; trabajo local y documentación preparados.

## Objetivo y ubicación
Completar Neon + Render + pruebas públicas + Spec Kit V2 + MAPA_COMPLETO_PROYECTO_V2.txt, conservando la arquitectura y los cambios del equipo.

- Repositorio privado: https://github.com/MrEpia/innovacioncurricular
- Copia aislada: `C:/Users/USER/Documents/Codex/2026-11-13/14-continuidad-autom-tica-por-l/work/innovacioncurricular`.
- Rama: `deploy/neon-render-spec-kit-v2`; main no modificada. Base: `24abf0491b5f29aaf0ec346a489570b58140aafa`.
- Último commit al redactar este punto: `c0fca5c` (pruebas runtime). Código de implementación: `1e91a6eea54e690a3faa9637e2c943ef2d2fc5e4`.
- La autoridad para el último commit es `git rev-parse HEAD`; el commit del propio checkpoint necesariamente es posterior al hash que registra.
- PR: aún no creado en este checkpoint; siguiente paso después de confirmar documentación.

## Fase actual y tarea exacta
Fase 10, consolidación documental, mapa final y PR borrador. Se trabaja independientemente de los bloqueos externos de las fases 4–7.

Última tarea ejecutada: validar 163 operaciones de código contra Swagger real local y completar la documentación de pruebas/despliegue. Pendiente inmediato: confirmar esta fase documental, crear PR borrador, regenerar el mapa contra el commit que ya contiene la documentación, verificar enlaces/líneas y subir el checkpoint final.

## Fases y tareas completadas
- 1: Git/acceso WRITE, clon limpio, rama aislada, historial/ramas inspeccionados; sin cambios previos que sobrescribir.
- 2: arquitectura y documentación auditadas (519 archivos y 91 Markdown en la base). ASP.NET Core 10 + Dapper + SQL Server, Blazor Server separado; Spec Kit del curso con versiones históricas hasta V4.
- 3: soporte PostgreSQL paralelo (30 repositorios), 29 funciones, migrador no destructivo con advisory lock/checksum/transacción; LF/CRLF compatibles. SQL Server se mantiene por defecto. Dos Dockerfile.render, PORT, CORS cerrado configurable, health de base/historial, errores 500 saneados y página Error.
- 5 local: 25 tablas funcionales + historial, 24 FK, tres migraciones obligatorias y demo optativa aplicadas; repetición sin duplicados, rechazo de base ajena, fallo de checksum, rollback durante inserción y carrera de último administrador probados.
- 7 local: 194 comprobaciones de capas, 364 de ocho humos, cinco grupos de infraestructura, cinco de runtime y casos numéricos; todo aprobado. APIs/front aislados 18072/18073. Builds finales y blueprint local validado contra esquema oficial.
- 8: Spec Kit en docs/spec_kit/versiones/v2_neon_render; constitución 1.6.0 propuesta en rama de revisión, versiones históricas intactas. No se presenta como nueva instalación del CLI oficial Spec Kit.
- 9: generador y mapa creado; falta regeneración final con todos los documentos ya confirmados. 163 operaciones API exactamente coincidentes con Swagger local; frontend/sesión/Swagger enumerados aparte.

## Neon — estado real
No se ha confirmado ningún proyecto/base/rol/URL. list_organizations, list_projects y search devolvieron Unknown tool; luego el namespace Neon dejó de estar disponible. No se crearon recursos ni se ejecutó SQL remoto. Requiere conectar/restablecer Neon. No afirmar que las pruebas locales verificaron Neon.

## Render — estado real
Conector operativo; workspace `tea-cvbgt23qf0us73d8uuug` (My Workspace). El inventario antes/después solo mostró el servicio ajeno `inventario`, ID `srv-cvbh02rtq21c73dv7330`, repo SebastianBrunal/inventario: no tocar.

Se intentó create_blueprint con repo MrEpia/innovacioncurricular, rama de trabajo, render.yaml, create_new, autoSync=false. Respuesta HTTP 400: “the repository, branch, or Blueprint file could not be accessed”. No se obtuvo ID de blueprint ni de servicio nuevo. GitHub API confirma que render.yaml existe en esa rama remota; hay que resolver el acceso de la integración Git de Render. Antes de reintentar, revisar recursos/blueprints existentes para evitar duplicados. No cambiar la privacidad del repo ni incrustar tokens en URLs. No hay URL pública de esta revisión ni deploy exitoso observado.

## Pruebas y fallos resueltos
Ver docs/VALIDACION_NEON_RENDER_V2.md. Fallos iniciales corregidos: GO residual en demo PostgreSQL (rollback), date/DateOnly vs DateTime/Dapper (casts explícitos), aviso Kerberos en runtime mínimo (GSS desactivado, TLS conservado), checksum CRLF vs LF (normalización). Los bloques afectados se repitieron y pasaron. Advertencias frontend heredadas CS0414/BL0008: documentadas, formulario HTTP probado. Interacción completa de eventos Blazor, WebSocket público y cookie Secure del hosting: pendientes. Computer Use se detuvo al no poder determinar la URL de Chrome con confianza; no hubo automatización posterior de esa ventana.

## Comandos y artefactos de evidencia
- gh repo view / git ls-remote / clone / status / branch / log / fetch / switch -c.
- docker build con Dockerfile.render de API/front; target build SDK para PruebaCapas.csproj (194 checks).
- docker compose -p astra-neon-v2 -f docker-compose.postgres.yml --env-file ../pruebas-local.env up/run; --migrar-postgres y --datos-demo, repetidos y desde archivos LF.
- Ocho pruebas_humo con API_URL/FRONT_URL y PRUEBAS_* apuntando solo al laboratorio.
- validar_infra_postgres.py y validar_runtime_produccion.py preservados en el repositorio; pruebas aisladas adicionales de conversión numérica.
- publicacion/generar_mapa_v2.py --swagger-url http://localhost:18072/swagger/v1/swagger.json.
- Validación JSON Schema oficial de render.yaml; gh API verificó el archivo remoto.
- Logs y archivos privados de pruebas: directorio work del chat, fuera del repositorio. Archivo privado de entorno local: `../pruebas-local.env`; no publicar su contenido. No existe cadena Neon real guardada.

## Trabajo externo pendiente (no simulado)
1. Reconectar Neon, listar proyectos/ramas/bases y comprobar plan/capacidad gratuita; no crear duplicados ni generar cargos no autorizados.
2. Resolver acceso del repo privado en Render y consultar servicios/blueprints antes de crear.
3. Preparar base Neon NUEVA y separada; conexión Npgsql VerifyFull, migrar y comprobar tablas/FK/funciones/historial y backend real. No transferir ni borrar datos originales sin un procedimiento seguro.
4. Desplegar dos Web Services gratuitos, registrar IDs y URLs efectivas, configurar UrlApi HTTPS y comprobar health y funcionalidad pública completa.
5. Actualizar resultados/docs/mapa/checkpoint con evidencia pública, confirmar commits, mantener PR sin fusionar.

## Continuidad y límites
No se ha configurado ni confirmado reactivación automática al restaurarse cuota. Se usa la continuidad del mismo chat y archivos persistentes; si la sesión se interrumpe, escribir CONTINUAR. La documentación oficial consultada sobre trabajo prolongado no constituye una garantía de reinicio por cuota. No modificar límites, comprar créditos ni crear bucles o automaciones para este despliegue específico.

Al reanudar: leer este archivo y ASTRA_REANUDAR.txt; status/branch/log/fetch, revisar diferencias preservando cambios, revalidar Neon y Render, y seguir la primera tarea pendiente. No repetir suites ya aprobadas salvo cambios/fallos nuevos. No declarar finalización hasta cumplir todos los criterios externos.
