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
