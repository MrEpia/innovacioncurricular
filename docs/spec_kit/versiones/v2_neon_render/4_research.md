# Decisiones — revisión de despliegue V2

## D-NR-1 — Ampliar el proveedor sin reemplazar SQL Server
La misión del propietario pide Neon/PostgreSQL. La constitución permite otro paquete cuando una spec lo exige. Agregar Npgsql 10.0.0 y repositorios concretos PostgreSQL paralelos; servicios, modelos, peticiones y rutas conservan contratos. SqlServer seguirá siendo el valor por defecto. No traducir T-SQL dinámicamente en cada petición: cada consulta PostgreSQL quedará explícita y revisable.

## D-NR-2 — Funciones PostgreSQL para las operaciones atómicas
CommandType.StoredProcedure en Npgsql invoca CALL, que no equivale a los SELECT de resultados del código T-SQL. Usar funciones PL/pgSQL invocadas con SELECT y parámetros Dapper. Mantener transacciones de sentencia y bloqueos para retiro de padres y protección del último administrador. JSON como parámetro text convertido a jsonb; no concatenar valores.

## D-NR-3 — Esquema nuevo y migraciones con historial
Conservar scripts SQL Server. Crear las 25 tablas y semillas de referencia en una base nueva; omitir DROP/DELETE/TRUNCATE. Registrar versión/checksum bajo advisory lock y transacción. Rechazar bases existentes sin historial antes de crear nada. Los datos de demostración son optativos y no representan migración de producción.

## D-NR-4 — Dos Web Services Docker y una instancia
Blazor Server necesita proceso ASP.NET y WebSocket: no es un Static Site. Usar imágenes multietapa .NET 10 runtime, usuario app y PORT de Render. UrlApi explícita. CORS cerrado salvo orígenes configurados; llamadas normales son de servidor a servidor. Sesiones en memoria: reinicios cierran sesiones; escalar es trabajo futuro.

## D-NR-5 — Compatibilidad de tipos y consultas
activo se representará como smallint con CHECK 0/1 para conservar el SQL y el mapeo Dapper; no BIT de PostgreSQL. Los IDs autogenerados usan identity; OUTPUT pasa a RETURNING, TOP a LIMIT, ISNULL a COALESCE. Conversión numérica tolerante para los campos históricos de texto y STRING_AGG con ORDER BY interno.

## Enmienda propuesta y aplicada en esta rama
La instrucción explícita del propietario autoriza Neon y Render. Esta revisión documenta la ampliación de los artículos 2, 5 y 10 sin reabrir versiones cerradas: solo el nuevo proveedor usa funciones PostgreSQL y puertos PORT; SQL Server y compose mantienen su comportamiento histórico. La aceptación de esta rama por el equipo se realiza mediante PR, sin fusión automática.

## Fuentes primarias consultadas
- https://www.npgsql.org/doc/basic-usage.html
- https://www.npgsql.org/doc/security.html
- https://render.com/docs/blueprint-spec
- https://learn.chatgpt.com/docs/long-running-work

## Referencia de implementación

Código auditado: https://github.com/MrEpia/innovacioncurricular/tree/1e91a6eea54e690a3faa9637e2c943ef2d2fc5e4 . Las declaraciones específicas y sus líneas se vinculan en 6_contracts.md y MAPA_COMPLETO_PROYECTO_V2.txt; los resultados distinguen verificación local y publicación pendiente.
