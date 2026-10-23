# Diagramas de secuencia — un diagrama por método

> Cada diagrama muestra **qué hace un método** recorriendo las capas reales del
> sistema (Artículo 3 de la [constitución](1_constitution.md)):
>
> `Front (Blazor)` → `ClienteApi` → **API**: `Controller` → `IServicio` →
> `IRepositorio` → `SQL Server`
>
> Los métodos CRUD tienen el **mismo nombre y el mismo recorrido en las 22
> tablas** del módulo (Artículo 10.1: un controlador, un servicio y un
> repositorio por tabla). Por eso se dibujan una vez, con la tabla como
> ejemplo; lo que cambia de una tabla a otra está en la nota de cada diagrama.
> Los contratos exactos (rutas, cuerpos, códigos) están en el `6_contracts.md`
> de cada versión.

## Índice

| # | Método | Capa de entrada | Versión |
|---|---|---|---|
| 1 | `ObtenerTodos` — listar | `XController.ObtenerTodos` | v1 |
| 2 | `ObtenerPorId` / `ObtenerPorLlave` — obtener uno | `XController.ObtenerPor…` | v1 · v2 |
| 3 | `Crear` | `XController.Crear` | v1 · v2 |
| 4 | `Reemplazar` (PUT) | `XController.Reemplazar` | v1 · v2 |
| 5 | `ActualizarParcial` (PATCH) | `XController.ActualizarParcial` | v1 · v2 |
| 6 | `Eliminar` — borrado lógico | `XController.Eliminar` | v1 · v2 |
| 7 | `ValidarReglas` — claves foráneas activas | `ServicioX` (privado) | v2 |
| 8 | Petición rechazada por el cuerpo (422) | framework | v1 |
| 9 | `PermisoPorVerbo` — autorización | filtro global | v3 |
| 10 | `IniciarSesion` — `POST /api/login` | `LoginController` | v3 |
| 11 | `ServicioUsuario.Crear` | `UsuarioController.Crear` | v3 |
| 12 | `ServicioUsuario.Eliminar` | `UsuarioController.Eliminar` | v3 |
| 13 | `ServicioRolUsuario.Asignar` / `Quitar` | `RolUsuarioController` | v3 |
| 14 | `AsegurarAdministradorInicial` | arranque de la API | v3 |
| 15 | Ingresar en el front (`/ingresar`) | `Ingresar.razor` | v3 |
| 16 | `ClienteApi.Enviar` — token y sesión vencida | front | v3 |
| 17 | Cerrar sesión (`/sesion/salir`) | `EnsambladorSesion` | v3 |

---

## 1. `ObtenerTodos` — listar

```mermaid
sequenceDiagram
    actor U as Usuario
    participant P as Pantalla (Programas.razor)
    participant SF as ServicioPrograma (front)
    participant CA as ClienteApi
    participant C as ProgramaController
    participant S as ServicioPrograma
    participant R as RepositorioProgramaSqlServer
    participant B as SQL Server
    U->>P: abre /programas
    P->>SF: Listar()
    SF->>CA: Listar("/api/programa?limite=1000")
    CA->>C: GET /api/programa?limite=1000 (Bearer token)
    C->>S: ObtenerTodos(1000)
    alt limite <= 0
        S-->>C: ArgumentException
        C-->>CA: 400 {estado, mensaje, detalle}
    else limite válido
        S->>R: ObtenerTodos(1000)
        R->>B: SELECT TOP (@Limite) … WHERE activo = 1 ORDER BY id
        B-->>R: filas
        R-->>S: IEnumerable<Programa>
        S-->>C: lista
        alt lista vacía
            C-->>CA: 204 sin cuerpo
            CA-->>SF: Resultado.Bien([])
        else hay filas
            C-->>CA: 200 {tabla, limite, total, datos}
            CA-->>SF: Resultado.Bien(datos)
        end
    end
    SF-->>P: Resultado
    P-->>U: tabla, o «Todavía no hay registros»
```

**Lo hace igual en las 22 tablas.** Solo cambian la ruta y las columnas del `SELECT`. Los inactivos nunca salen: el filtro `activo = 1` está en el SQL.

## 2. `ObtenerPorId` / `ObtenerPorLlave` — obtener uno

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant C as AlianzaController
    participant S as ServicioAlianza
    participant R as RepositorioAlianzaSqlServer
    participant B as SQL Server
    CA->>C: GET /api/alianza/{aliado}/{departamento}
    C->>S: ObtenerPorLlave(aliado, departamento)
    S->>R: ObtenerPorLlave(aliado, departamento)
    R->>B: SELECT … WHERE aliado = @Aliado AND departamento = @Departamento AND activo = 1
    B-->>R: fila o nada
    R-->>S: Alianza o null
    alt null (no existe o está retirada)
        S-->>C: NoEncontradoExcepcion
        C-->>CA: 404 {estado, mensaje, detalle}
    else existe
        S-->>C: Alianza
        C-->>CA: 200 la ficha
    end
```

Con llave simple el método se llama `ObtenerPorId` (o `ObtenerPorResolucion`, `ObtenerPorCodigo`, `ObtenerPorNit`); con llave compuesta, `ObtenerPorLlave`, y la ruta lleva un segmento por columna.

## 3. `Crear`

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant C as AcreditacionController
    participant S as ServicioAcreditacion
    participant RR as IRepositorioReferencias
    participant R as RepositorioAcreditacionSqlServer
    participant B as SQL Server
    CA->>C: POST /api/acreditacion {resolucion, tipo, …, programa}
    Note over C: [ApiController] valida AcreditacionCrear.<br/>Si falta un campo → 422 (diagrama 8)
    C->>S: Crear(acreditacion)
    S->>RR: ValidarReglas → ProgramaActivo(programa)
    RR->>B: SELECT COUNT(*) FROM programa WHERE id = @Id AND activo = 1
    alt el padre no existe o está retirado
        S-->>C: ReferenciaInvalidaExcepcion
        C-->>CA: 400
    else padre activo
        S->>R: Crear(acreditacion)
        R->>B: INSERT INTO acreditacion (…, activo) VALUES (…, 1)
        alt llave repetida (error 2627)
            R-->>S: LlaveDuplicadaExcepcion
            S-->>C: (se propaga)
            C-->>CA: 409
        else insertada
            C-->>CA: 201 {estado, mensaje, llave}
        end
    end
```

**v1:** las 7 tablas sin clave foránea no validan referencias y responden **200** (versión cerrada). **v2:** cada servicio valida SUS claves foráneas y sus reglas propias (diagrama 7) y responde **201**.

## 4. `Reemplazar` (PUT)

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant C as FacultadController
    participant S as ServicioFacultad
    participant RR as IRepositorioReferencias
    participant R as RepositorioFacultadSqlServer
    participant B as SQL Server
    CA->>C: PUT /api/facultad/{id} {nombre, tipo, fechaFun, universidad}
    Note over C: FacultadReemplazo exige TODOS los campos:<br/>si falta uno → 422 (aunque el mismo cuerpo<br/>por PATCH respondería 200)
    C->>S: Reemplazar(id, facultad)
    Note over S: la llave sale de la ruta, no del cuerpo
    S->>RR: ValidarReglas(facultad): UniversidadActiva, tipo válido
    alt regla rota
        S-->>C: ArgumentException / ReferenciaInvalidaExcepcion
        C-->>CA: 400
    else reglas ok
        S->>R: Reemplazar(facultad)
        R->>B: UPDATE facultad SET … WHERE id = @Id AND activo = 1
        B-->>R: filas afectadas
        alt 0 filas
            S-->>C: NoEncontradoExcepcion
            C-->>CA: 404
        else 1 fila
            C-->>CA: 200 {filasAfectadas: 1}
        end
    end
```

Las tablas puente sin columnas propias (`programa_ac`, `programa_pe`, `programa_ci`, `an_programa`, `enfoque_rc`, `rol_usuario`) **no tienen** este método.

## 5. `ActualizarParcial` (PATCH)

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant C as RegistroCalificadoController
    participant S as ServicioRegistroCalificado
    participant RR as IRepositorioReferencias
    participant R as RepositorioRegistroCalificadoSqlServer
    participant B as SQL Server
    CA->>C: PATCH /api/registro_calificado/{codigo} {fechaFin}
    C->>S: ActualizarParcial(codigo, cambios)
    alt cambios vacío
        S-->>C: ArgumentException
        C-->>CA: 400 «No se envió ningún campo»
    else
        S->>R: ObtenerPorCodigo(codigo)
        R->>B: SELECT … WHERE codigo = @Codigo AND activo = 1
        alt no existe
            S-->>C: NoEncontradoExcepcion
            C-->>CA: 404
        else existe
            Note over S: aplica los cambios en MEMORIA y valida<br/>cómo quedaría (fechaFin ≥ fechaInicio guardada)
            S->>RR: ValidarReglas(resultado)
            alt regla rota
                C-->>CA: 400
            else ok
                S->>R: ActualizarParcial(codigo, cambios)
                R->>B: UPDATE … SET fecha_fin = @FechaFin WHERE … (solo lo enviado)
                C-->>CA: 200 {filasAfectadas: 1}
            end
        end
    end
```

**v1:** valida solo que llegue algún campo. **v2:** además lee el registro y valida el resultado (D-v2-7).

## 6. `Eliminar` — borrado lógico

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant C as ProgramaController
    participant S as ServicioPrograma
    participant RR as IRepositorioReferencias
    participant R as RepositorioProgramaSqlServer
    participant B as SQL Server
    CA->>C: DELETE /api/programa/{id}
    Note over C: PermisoPorVerbo: solo administrador (diagrama 9)
    C->>S: Eliminar(id)
    S->>RR: DependientesDePrograma(id)
    RR->>B: SELECT 'acreditacion', COUNT(*) … UNION ALL … (11 tablas hijas, activo = 1)
    alt tiene hijos activos
        S-->>C: DependenciasActivasExcepcion
        C-->>CA: 409 «tiene registros activos en acreditacion (1)…»
    else sin hijos
        S->>R: EliminarLogico(id)
        R->>B: UPDATE programa SET activo = 0 WHERE id = @Id AND activo = 1
        alt 0 filas (no existe o ya retirado)
            S-->>C: NoEncontradoExcepcion
            C-->>CA: 404
        else 1 fila
            C-->>CA: 200 {filasAfectadas: 1}
        end
    end
```

La fila **nunca** se borra (Artículo 6). Las tablas que no son padre de nadie se saltan la consulta de dependientes. Las 7 tablas de la v1 también la hacen desde la v2 (RF9).

## 7. `ValidarReglas` — claves foráneas activas (método privado de cada servicio v2)

```mermaid
sequenceDiagram
    participant S as ServicioAlianza
    participant RR as RepositorioReferenciasSqlServer
    participant B as SQL Server
    S->>RR: AliadoActivo(aliado)
    RR->>B: SELECT COUNT(*) FROM aliado WHERE nit = @Nit AND activo = 1
    alt 0
        S-->>S: throw ReferenciaInvalidaExcepcion («El aliado X no existe o está retirado»)
    end
    S->>RR: ProgramaActivo(departamento)
    RR->>B: SELECT COUNT(*) FROM programa WHERE id = @Id AND activo = 1
    alt 0
        S-->>S: throw ReferenciaInvalidaExcepcion
    end
    Note over S: reglas propias: fechaFin ≥ fechaInicio
```

Lo llaman `Crear`, `Reemplazar` y `ActualizarParcial`. Cada tabla pregunta por SUS padres con un método explícito (Artículo 10.1); las claves opcionales (`activ_academica.disenio`) solo se validan si vienen.

## 8. Petición rechazada por el cuerpo (422)

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant M as [ApiController] (validación del modelo)
    participant F as InvalidModelStateResponseFactory (Program.cs)
    participant C as Controller
    CA->>M: POST /api/facultad {"id": "doce"}
    M->>M: valida contra FacultadCrear ([Required], [MaxLength], tipos)
    M->>F: ModelState inválido
    F-->>CA: 422 {estado, mensaje, errores:[…en español…]}
    Note over C: el controlador nunca se ejecuta y la base nunca se toca
```

## 9. `PermisoPorVerbo` — autorización en cada petición

```mermaid
sequenceDiagram
    participant CA as ClienteApi
    participant J as JwtBearer (UseAuthentication)
    participant P as AuthorizeFilter + PermisoPorVerboManejador
    participant C as Controller
    CA->>J: DELETE /api/enfoque/5 + Authorization: Bearer …
    alt sin token / firma inválida / vencido
        J-->>CA: 401 (OnChallenge: «Falta el token» / «no es válido» / «expiró»)
    else token válido
        J->>P: usuario con sus roles
        alt el verbo no está permitido para su rol
            P-->>CA: 403 (OnForbidden: «Su rol no permite esta operación»)
        else permitido
            P->>C: sigue al controlador
        end
    end
```

GET: los tres roles · POST/PUT/PATCH: administrador y editor · DELETE: administrador. `usuario`, `rol` y `rol_usuario` exigen además el rol administrador.

## 10. `IniciarSesion` — `POST /api/login`

```mermaid
sequenceDiagram
    participant F as Front (ServicioSesion)
    participant C as LoginController [AllowAnonymous]
    participant S as ServicioAutenticacion
    participant R as RepositorioUsuarioSqlServer
    participant H as HasheadorClavesPbkdf2
    participant G as GeneradorTokenJwt
    participant B as SQL Server
    F->>C: POST /api/login {usuario, clave}
    C->>S: IniciarSesion(usuario, clave)
    S->>R: ObtenerCredenciales(usuario)
    R->>B: SELECT id, password, activo FROM usuario WHERE username = @Username
    S->>H: Verificar(clave, hashGuardado)
    alt no existe, retirado o clave incorrecta
        S-->>C: CredencialesInvalidasExcepcion
        C-->>F: 401 «Usuario o contraseña incorrectos.» (mismo mensaje en los 3 casos)
    else credenciales correctas
        S->>R: RolesActivos(id)
        R->>B: SELECT r.nombre FROM rol_usuario ru JOIN rol r … (ambos activos)
        alt sin roles
            S-->>C: OperacionNoPermitidaExcepcion
            C-->>F: 403
        else con roles
            S->>G: Generar(id, usuario, nombre, roles)
            G-->>S: token HS256 + vencimiento
            C-->>F: 200 {token, tipo: "Bearer", expira, usuario: {…, roles}}
        end
    end
```

## 11. `ServicioUsuario.Crear`

```mermaid
sequenceDiagram
    participant C as UsuarioController [administrador]
    participant S as ServicioUsuario
    participant H as HasheadorClavesPbkdf2
    participant R as RepositorioUsuarioSqlServer
    participant B as SQL Server
    C->>S: Crear(usuario, clave)
    S->>H: Hashear(clave)
    H-->>S: "pbkdf2-sha256$100000$sal$hash"
    S->>R: Crear(usuario, hash)
    R->>B: INSERT INTO usuario (…, password, activo) OUTPUT INSERTED.id VALUES (…, @Password, 1)
    alt username o email repetido
        R-->>C: LlaveDuplicadaExcepcion → 409
    else
        R-->>S: id
        C-->>C: 201 {llave: {id}}
    end
```

La clave en claro nunca llega al repositorio. Cambiar la clave (`ActualizarParcial` con `clave`) sigue el mismo camino: se hashea en el servicio.

## 12. `ServicioUsuario.Eliminar`

```mermaid
sequenceDiagram
    participant C as UsuarioController
    participant S as ServicioUsuario
    participant R as RepositorioUsuarioSqlServer
    participant B as SQL Server
    C->>S: Eliminar(id)
    S->>R: ObtenerPorId(id)
    alt no existe
        S-->>C: NoEncontradoExcepcion → 404
    else es administrador
        S->>R: ContarAdministradoresActivos()
        R->>B: SELECT COUNT(DISTINCT u.id) … rol = 'administrador'
        alt queda solo él
            S-->>C: OperacionNoPermitidaExcepcion → 409
        end
    end
    S->>R: EliminarLogico(id)
    R->>B: UPDATE usuario SET activo = 0 WHERE id = @Id AND activo = 1
    C-->>C: 200
```

`ServicioRol.Eliminar` sigue la misma forma: 409 si es un rol del sistema o si tiene usuarios activos.

## 13. `ServicioRolUsuario.Asignar` y `Quitar`

```mermaid
sequenceDiagram
    participant C as RolUsuarioController
    participant S as ServicioRolUsuario
    participant RR as IRepositorioReferencias
    participant R as RepositorioRolUsuarioSqlServer
    participant B as SQL Server
    C->>S: Asignar(usuarioId, rolId)
    S->>RR: UsuarioActivo(usuarioId) · RolActivo(rolId)
    alt alguno no activo
        S-->>C: ReferenciaInvalidaExcepcion → 400
    else
        S->>R: Asignar(usuarioId, rolId)
        R->>B: IF activa → 0 · ELSE IF existe → UPDATE activo = 1 · ELSE INSERT
        alt ya la tenía (0)
            S-->>C: LlaveDuplicadaExcepcion → 409
        else
            C-->>C: 201
        end
    end
    C->>S: Quitar(usuarioId, rolId)
    alt es quitar «administrador» al último administrador
        S-->>C: OperacionNoPermitidaExcepcion → 409
    else
        S->>R: EliminarLogico → UPDATE rol_usuario SET activo = 0
        C-->>C: 200
    end
```

## 14. `AsegurarAdministradorInicial` — al arrancar la API

```mermaid
sequenceDiagram
    participant A as Program.cs (UsarSeguridad)
    participant S as ServicioUsuario
    participant R as Repositorios
    participant B as SQL Server
    A->>A: lee ADMIN_USUARIO, ADMIN_CORREO, ADMIN_CLAVE (del .env)
    A->>S: AsegurarAdministradorInicial(…)
    S->>R: ContarAdministradoresActivos()
    alt ya hay uno
        S-->>A: no hace nada
    else no hay ninguno
        S->>R: ObtenerPorNombre("administrador")
        S->>R: crea el usuario (o lo restablece) con la clave HASHEADA
        S->>R: Asignar(id, rol administrador)
        R->>B: INSERT / UPDATE
    end
```

## 15. Ingresar en el front (`/ingresar`)

```mermaid
sequenceDiagram
    actor U as Usuario
    participant I as Ingresar.razor (estática)
    participant SS as ServicioSesion
    participant API as API /api/login
    participant CK as Cookie + AlmacenSesionesMemoria
    U->>I: usuario y contraseña (POST con token antifalsificación)
    I->>SS: IniciarSesion(usuario, clave)
    SS->>API: POST /api/login
    alt 401 / 403
        API-->>I: detalle
        I-->>U: aviso en rojo
    else 200
        API-->>SS: {token, expira, roles}
        I->>CK: SignInAsync(claims + "jwt", ExpiresUtc = expira)
        CK->>CK: guarda el ticket EN EL SERVIDOR; al navegador solo va una cookie HttpOnly
        I-->>U: redirige a ReturnUrl (solo direcciones locales)
    end
```

## 16. `ClienteApi.Enviar` — el token en cada petición y la sesión vencida

```mermaid
sequenceDiagram
    participant P as Pantalla
    participant CA as ClienteApi
    participant PT as ProveedorToken
    participant API as API
    P->>CA: Enviar(POST, "/api/enfoque", cuerpo)
    CA->>PT: Obtener()
    PT-->>CA: claim "jwt" de la sesión
    CA->>API: petición + Authorization: Bearer <jwt>
    alt 401 (el token venció)
        CA->>P: NavigateTo("/sesion/expirada", forceLoad)
        Note over P: el servidor cierra la sesión y muestra<br/>/ingresar con «Su sesión expiró»
    else 403
        CA-->>P: Resultado.Mal(«Su rol no permite esta operación»)
    else 2xx
        CA-->>P: Resultado.Bien
    else 4xx/5xx
        CA-->>P: Resultado.Mal(errores del sobre)
    end
```

Lo usan los servicios del front de las 25 tablas; ninguna pantalla hace HTTP directamente.

## 17. Cerrar sesión (`/sesion/salir`)

```mermaid
sequenceDiagram
    actor U as Usuario
    participant M as NavMenu (formulario POST)
    participant E as /sesion/salir
    participant AS as AlmacenSesionesMemoria
    U->>M: «Cerrar sesión»
    M->>E: POST con token antifalsificación
    E->>E: valida el token antifalsificación
    E->>AS: SignOutAsync → RemoveAsync(llave)
    AS-->>E: ticket y JWT borrados del servidor
    E-->>U: redirige a /ingresar?salio=1
```
