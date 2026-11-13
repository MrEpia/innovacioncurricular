# Especificación — revisión documental V2: Neon y Render

Revisión de infraestructura del código V4 existente; no sustituye la entrega histórica v2_entrega2. Estado EN CURSO.

## Propósito y alcance
Gestionar innovación curricular de la Universidad Monte Verde (institución ficticia): catálogos, estructura académica, actividades, convenios, relaciones, usuarios/roles, maestro-detalle, reportes y dashboard. Conservar API y frontend separados y SQL Server existente; agregar PostgreSQL seleccionable para Neon y despliegues Docker en Render.

## Requisitos funcionales y trazabilidad
| Requisito | Implementación existente | Estado de código |
|---|---|---|
| CRUD y retiro lógico de 22 entidades | api_innovacion/Controllers, Servicios, Modelos y Repositorios; front_blazor/Components/Pages y Servicios | Implementado; humo PostgreSQL local aprobado |
| Login JWT y autorización consulta/editor/administrador | api_innovacion/Seguridad, Ensamblador/EnsambladorSeguridad.cs y Controllers/LoginController.cs | Implementado |
| Usuarios, roles y protección del último administrador | Controllers/UsuarioController.cs, RolController.cs, RolUsuarioController.cs; Servicios; db/migraciones/v3_002_procedimientos_almacenados.sql | Implementado en ambos proveedores; PostgreSQL probado localmente |
| Programa/registro calificado completos atómicos | Controllers/*CompletoController.cs, Servicios/*Completo.cs, db/migraciones/v3_003_maestro_detalle.sql | Implementado en ambos proveedores; rollback validado localmente |
| Diez reportes y siete endpoints de dashboard | Controllers/ReportesController.cs, DashboardController.cs; front_blazor/Components/Pages/Reportes.razor y Dashboard.razor | Implementado |
| Sitio corporativo y PWA | front_blazor/Components/Corporativo; wwwroot/service-worker.js y manifest.webmanifest | Implementado; no afirmar capacidad offline de operaciones autenticadas |
| Migraciones PostgreSQL verificables | db/postgres y publicacion/migrar_postgres.py | Implementado y validado localmente; Neon pendiente |
| Despliegue HTTPS Neon/Render | render.yaml y Dockerfile.render de ambos procesos | Pendiente de acceso y ejecución real |

## Requisitos no funcionales
Mantener contratos JSON y códigos HTTP; valores SQL parametrizados; secretos solo en entorno; TLS verificado para Neon; health check de dependencia real; logs sin cadenas ni contraseñas; no borrar datos existentes; una instancia Blazor con sesión en memoria. No se fija SLA ni rendimiento sin mediciones. Cuota gratuita y despliegue separado; no contratar servicios pagos.

## Flujos y componentes
Navegador → Blazor Server (cookie/ticket, antifalsificación) → HTTP con JWT → Controller → IServicio → IRepositorio → SQL Server o PostgreSQL. Login fabrica JWT; frontend lo conserva en el servidor. CRUD valida petición y negocio, comprueba referencias, ejecuta SQL y traduce errores. Maestro-detalle valida todos los hijos y guarda todo o revierte todo. Reportes son consultas de lectura sobre tablas activas. El frontend nunca recibe la cadena de base de datos.

## Límites y pendientes
No hay evidencia de SSO, recuperación de contraseña, envío de correo, multitenencia, cola de trabajos ni datos reales transferidos a Neon. Las páginas corporativas de contacto/soporte no prueban una integración de mensajería. No modificar las especificaciones archivadas ni el despliegue MonsterASP. La verificación externa permanece pendiente hasta obtener respuestas HTTP públicas y comprobación de tablas, relaciones y escritura/lectura autenticada.

## Código en GitHub
Repositorio privado: https://github.com/MrEpia/innovacioncurricular/tree/deploy/neon-render-spec-kit-v2 . Los archivos y líneas con enlaces permanentes se generan en MAPA_COMPLETO_PROYECTO_V2.txt y 6_contracts.md; no inventar líneas.

## Referencia de implementación

Código auditado: https://github.com/MrEpia/innovacioncurricular/tree/1e91a6eea54e690a3faa9637e2c943ef2d2fc5e4 . Las declaraciones específicas y sus líneas se vinculan en 6_contracts.md y MAPA_COMPLETO_PROYECTO_V2.txt; los resultados distinguen verificación local y publicación pendiente.
