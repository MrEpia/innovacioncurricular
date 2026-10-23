# Decisiones — Versión 3

## D-v3-1 — Un paquete nuevo: JwtBearer *(enmienda 1.2.0 del Artículo 2)*

**Decisión.** `Microsoft.AspNetCore.Authentication.JwtBearer` valida el token en
cada petición (firma, emisor, audiencia, vencimiento).

**Alternativa descartada.** Validarlo a mano en un middleware propio. Es
exactamente el tipo de código de seguridad que no se escribe a mano en un
proyecto de curso: un error en la comparación de la firma abre la puerta.

## D-v3-2 — PBKDF2 de .NET y no bcrypt

**Decisión.** `Rfc2898DeriveBytes.Pbkdf2` con SHA-256, 100 000 iteraciones y
sal aleatoria de 16 bytes. El número de iteraciones viaja dentro del hash.

**Alternativa descartada.** `BCrypt.Net-Next`. Es igual de válido («bcrypt o
equivalente», dice la metodología), pero sería un paquete más que el Artículo 2
obliga a justificar, cuando .NET ya trae un algoritmo recomendado por NIST.

## D-v3-3 — Mismo 401 para usuario inexistente, retirado y clave equivocada

**Decisión.** Un solo mensaje: «Usuario o contraseña incorrectos.».

**Alternativa descartada.** Mensajes distintos, más útiles para quien se
equivoca. También para quien ataca: le dirían qué nombres de usuario existen.
El administrador sí ve la causa en el panel de usuarios.

## D-v3-4 — Permisos por verbo, en un filtro global

**Decisión.** Un `AuthorizeFilter` global con la política `PermisoPorVerbo`
(GET → los tres roles; POST/PUT/PATCH → administrador y editor; DELETE →
administrador). `usuario`, `rol` y `rol_usuario` suman
`[Authorize(Roles = "administrador")]`; `/api/login` se excluye con
`[AllowAnonymous]`.

**Alternativa descartada.** Un `[Authorize(Roles = …)]` en cada acción de los
22 controladores: 110 atributos, y el día que alguien agregue una tabla sin
ellos, esa tabla queda abierta sin que nada lo avise. Con el filtro global, lo
abierto tiene que pedirse explícitamente.

**Lo que no cambia.** La metodología dice que «los permisos son por recurso»:
siguen pudiendo serlo. Un controlador que necesite una regla propia la agrega
encima, como hacen los tres de administración.

## D-v3-5 — El administrador inicial lo crea la API, con el `.env`

**Decisión.** Al arrancar, si no hay un administrador activo, la API crea (o
restablece) el usuario de `ADMIN_USUARIO`/`ADMIN_CLAVE`, con la clave
hasheada.

**Alternativa descartada.** Un `INSERT` en una migración: obligaría a escribir
la contraseña —o su hash, que también es un secreto: se puede atacar sin
conexión— en un archivo versionado.

## D-v3-6 — Sin refresh token

**Decisión.** El token vive `JWT_MINUTOS` (60). Al vencer, el front cierra la
sesión y pide ingresar.

**Alternativa descartada.** Refresh tokens: exigen guardarlos, rotarlos y
revocarlos — una tabla y un flujo nuevos que el PDF no pide.

## D-v3-7 — El JWT vive en la sesión del SERVIDOR del front

**Decisión.** Autenticación por cookie en el front con un `ITicketStore` en
memoria: el ticket —con el JWT adentro— queda en el servidor y el navegador
recibe una cookie `HttpOnly` que solo lo identifica. Los servicios del front
leen el token de la sesión y lo agregan como `Authorization: Bearer`.

**Alternativas descartadas.** `localStorage` (cualquier script de la página lo
lee: un XSS se lleva el token) y la cookie con el ticket cifrado adentro (el
token sí viajaría en cada petición del navegador, aunque cifrado). El PDF pide
«almacenamiento del JWT en la sesión del servidor», y esto es literalmente eso.

**Precio.** Reiniciar el contenedor del front borra las sesiones: todos vuelven
a ingresar. Para un sistema de un solo servidor es aceptable.

## D-v3-8 — `rol_usuario` recibe `activo` y quitar un rol es lógico

**Decisión.** Migración idempotente `v3_001_seguridad.sql`. Asignar de nuevo un
rol quitado **reactiva** la fila en vez de responder 409: la llave es la misma.

## D-v3-9 — Reparto de la v3

**Decisión.** La API (todo lo que exige seguridad de verdad) la hace el
encargado del `main`; la sesión del front y el panel de administración, los
otros dos. Ver [8_tasks.md](8_tasks.md).

## D-v3-10 — Procedimientos almacenados para las operaciones atómicas *(enmienda 1.3.0 del Artículo 2)*

**Contexto.** Varias operaciones comprueban algo y luego escriben, en dos
viajes a la base: retirar un padre (¿tiene hijos activos? → `UPDATE`), retirar
un rol (¿algún usuario activo lo tiene?), quitar el rol administrador o retirar
un usuario (¿es el último administrador?) y asignar un rol (¿ya lo tenía? →
reactivar o insertar). Entre la comprobación y la escritura puede colarse otra
petición: dos administradores que se retiran el rol al mismo tiempo dejarían
el sistema sin administrador, y un hijo creado en ese instante quedaría
huérfano. Además el curso pide procedimientos almacenados.

**Decisión.** Migración idempotente `db/migraciones/v3_002_procedimientos_almacenados.sql`
con 27 procedimientos. Cada uno hace la comprobación y la escritura en UNA
transacción `SERIALIZABLE`:

| Procedimiento | Tablas | Lo llama |
|---|---|---|
| `sp_dependientes_<tabla>` (11) | universidad, facultad, programa, registro_calificado, activ_academica, area_conocimiento, practica_estrategia, car_innovacion, aspecto_normativo, enfoque, aliado | `RepositorioReferenciasSqlServer.DependientesDe<Tabla>` |
| `sp_retirar_<tabla>` (11) | las mismas 11: son las únicas que tienen hijos | `Repositorio<Tabla>SqlServer.EliminarLogico` |
| `sp_rol_retirar` | rol | `RepositorioRolSqlServer.EliminarLogico` |
| `sp_rol_usuario_asignar` / `sp_rol_usuario_quitar` | rol_usuario | `RepositorioRolUsuarioSqlServer.Asignar` / `EliminarLogico` |
| `sp_usuario_retirar` / `sp_contar_administradores_activos` | usuario | `RepositorioUsuarioSqlServer.EliminarLogico` / `ContarAdministradoresActivos` |

Los procedimientos avisan con `THROW 50409` (tiene dependientes) o `50410`
(dejaría el sistema sin administrador). El repositorio traduce esos números a
las excepciones de siempre, así que el servicio, el controlador y el contrato
no cambian: siguen respondiendo 409 con el mismo sobre.

Los servicios **conservan** su comprobación previa. Da el mensaje amable y es
lo que verifica la prueba de capas (sin base). El procedimiento es la garantía
final cuando dos peticiones llegan al mismo tiempo.

**Por qué no en todas las tablas.** El CRUD de una sola tabla es una sola
sentencia (`SELECT`, `INSERT`, `UPDATE`): ya es atómico, y un procedimiento solo
escondería el SQL que el Artículo 2 quiere a la vista. Las 11 tablas que no son
padre de nadie, por ejemplo `acreditacion`, `premio` o los puentes, no
necesitan ninguno.

**Alternativa descartada.** Transacciones abiertas desde C# alrededor de dos
consultas. Funcionan, pero reparten la regla entre la API y la base, y el
curso pide procedimientos.

**Pendiente conocido.** Crear un hijo sigue en dos pasos: el servicio comprueba
que el padre esté activo y después inserta. Si el padre se retira justo en
medio, el hijo nace apuntando a un padre inactivo. La ventana es muy corta; si
se quiere cerrar, el siguiente paso es un `sp_crear_<tabla>` por cada tabla
hija. *(Actualización)* Para los hijos creados con el maestro-detalle ya está
cerrado: el procedimiento revalida los padres dentro de su transacción
([D-MD-3](../v3_maestro_detalle/4_research.md)).

