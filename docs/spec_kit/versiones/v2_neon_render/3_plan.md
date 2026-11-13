# Plan técnico — Neon y Render

Estado: implementación local, publicación externa pendiente. El plan preserva la separación y los contratos de V4.

## Arquitectura e implementación
Controladores MVC → interfaces de servicios → interfaces de repositorios. Los ensambladores históricos registran SQL Server; EnsambladorPostgres reemplaza solo los IRepositorio cuando BaseDatos__Proveedor=Postgres. No hay entidades compartidas entre frontend y API. Repositorios PostgreSQL con consultas explícitas, Npgsql y Dapper; funciones PL/pgSQL para comprobar+retirar, seguridad y maestro-detalle. El SQL Server original no se reescribe.

## Infraestructura
Dockerfile.render multietapa por proceso, .NET 10, usuario app, PORT. render.yaml describe dos Web Services gratuitos separados y despliegue manual. Migrador CLI con advisory lock, transacción e historial/checksum. Conexión Npgsql VerifyFull en producción; no se imprime la cadena. /health de API comprueba conexión e historial, /health de front comprueba el proceso. CORS por lista cerrada; Blazor comunica por HTTP desde el servidor. Errores 500 filtrados antes de responder en producción; página Error en frontend.

## Trazabilidad
- api_innovacion/Program.cs, Infraestructura y Ensamblador/EnsambladorPostgres.cs.
- api_innovacion/Repositorios/Postgres/*.cs, db/postgres/*.sql y demo/004_datos_demo.sql.
- front_blazor/Program.cs, Ensamblador/EnsambladorSesion.cs y Components/Pages/Error.razor.
- DESPLIEGUE_NEON_RENDER.md, render.yaml, docker-compose.postgres.yml y ambos Dockerfile.render.
- Pruebas de capas, pruebas_humo y validar_infra_postgres.py; resultados en docs/VALIDACION_NEON_RENDER_V2.md.

La lista exacta de archivos, funciones y enlaces permanentes se genera en MAPA_COMPLETO_PROYECTO_V2.txt. La configuración de proveedores se comprueba otra vez al reanudar para no duplicar recursos.

## Referencia de implementación

Código auditado: https://github.com/MrEpia/innovacioncurricular/tree/1e91a6eea54e690a3faa9637e2c943ef2d2fc5e4 . Las declaraciones específicas y sus líneas se vinculan en 6_contracts.md y MAPA_COMPLETO_PROYECTO_V2.txt; los resultados distinguen verificación local y publicación pendiente.
