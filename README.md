# Módulo Innovación Curricular

Proyecto de aula del curso de Diseño de Software: el módulo **Innovación
Curricular** del Sistema de Gestión de Información del Conocimiento
Universitario, para la *Universidad Monte Verde* (institución ficticia del
curso).

**Equipo:** Sebastián Calvo (encargado del `main`) · David Epia

> Sebastián Brunal salió del grupo antes de la entrega 2: desde entonces el
> trabajo se reparte entre los dos. Las ramas `feature/sebastian-brunal-…` de
> GitHub conservan el nombre del reparto inicial, pero su trabajo lo hizo y lo
> subió David.

| Entrega | Qué agrega | Spec kit |
|---|---|---|
| 1 — v1 | CRUD de las 7 tablas sin clave foránea | [v1_entrega1](docs/spec_kit/versiones/v1_entrega1/2_spec.md) |
| 2 — v2 | CRUD de las 15 tablas con clave foránea, listas desplegables desde la API, integridad referencial | [v2_entrega2](docs/spec_kit/versiones/v2_entrega2/2_spec.md) |
| 3 — v3 | JWT, sesión del servidor, permisos por rol, administración de usuarios y roles | [v3_seguridad](docs/spec_kit/versiones/v3_seguridad/2_spec.md) |
| 3 — maestro-detalle | Registro calificado y programa guardados con todo su detalle (todo o nada) | [v3_maestro_detalle](docs/spec_kit/versiones/v3_maestro_detalle/2_spec.md) |

Las reglas que rigen todo están en la [constitución](docs/spec_kit/1_constitution.md)
y la ruta completa en el [mapa de versiones](docs/spec_kit/versiones/0_mapa_versiones.md).
El material del curso (metodología, módulos y scripts originales) está en
[`ProyectosDeAula/`](ProyectosDeAula/).

## 1. Arranque

Solo hace falta **Docker Desktop**.

```powershell
git clone https://github.com/MrEpia/innovacioncurricular.git
cd innovacioncurricular
Copy-Item .env.example .env       # y cambie TODOS los valores
docker compose up -d --build
```

| Qué | Dónde |
|---|---|
| **La aplicación** | <http://localhost:8073> — ingrese con `ADMIN_USUARIO` / `ADMIN_CLAVE` de su `.env` |
| API · diagnóstico | <http://localhost:8072/> |
| Swagger | <http://localhost:8072/swagger> — `POST /api/login`, copie el `token`, botón **Authorize** |
| SQL Server (opcional) | `localhost,11471` · usuario `sa` · clave: `MSSQL_SA_PASSWORD` de su `.env` |

La primera vez tarda unos minutos: descarga imágenes, crea la base con sus 25
tablas y sus catálogos, corre las migraciones de `db/migraciones/`, compila la
API y el front, y crea el administrador inicial.

```powershell
docker compose up -d          # encender
docker compose down           # apagar (los datos se conservan)
docker compose down -v        # borrar la base y empezar de cero
docker compose logs -f api-innovacion
```

### Variables del `.env`

| Variable | Para qué |
|---|---|
| `MSSQL_SA_PASSWORD` | Contraseña de SQL Server (8+ caracteres con mayúscula, número y símbolo) |
| `JWT_SECRETO` | Llave de firma de los tokens. **Mínimo 32 caracteres**: si no, la API no arranca |
| `JWT_MINUTOS` | Duración del token y de la sesión (60 por defecto) |
| `ADMIN_USUARIO` · `ADMIN_CORREO` · `ADMIN_CLAVE` | Administrador que la API crea al arrancar si no existe ninguno |

**Ningún secreto está en el repositorio** (Artículo 7): el `.env` está en el
`.gitignore`; `.env.example` trae solo valores de mentira.

## 2. Roles

| Rol | Puede |
|---|---|
| `consulta` | Ver |
| `editor` | Ver, crear y modificar (no retirar) |
| `administrador` | Todo, incluida la administración de usuarios y roles |

Los permisos se aplican **en la API** (403 si el rol no alcanza); el front además
esconde lo que el rol no puede usar.

## 3. Pruebas

**Todo de una vez** (levanta el sistema y corre todas las pruebas; `--limpio` borra la base y la crea desde cero):

```powershell
python verificar_todo.py --limpio
```

O cada una por separado:

```powershell
# Prueba de capas: los servicios SIN base de datos (contenedor desechable)
docker compose run --rm --no-deps api-innovacion dotnet run --project pruebas/PruebaCapas.csproj

# Pruebas de humo contra el sistema levantado (usan el administrador del .env)
python pruebas_humo/humo_seguridad.py
python pruebas_humo/humo_academico.py
python pruebas_humo/humo_actividades.py
python pruebas_humo/humo_puentes.py
python pruebas_humo/humo_maestro_detalle.py
python pruebas_humo/humo_front.py
python pruebas_humo/humo_roles_front.py
```

## 4. Estructura

```
api_innovacion/          API REST — C# / ASP.NET Core 10, Dapper, SQL a mano
  Controllers/ Modelos/ Peticiones/ Servicios/ Repositorios/ Excepciones/
  Seguridad/             JWT, hash PBKDF2, permisos por verbo
  Ensamblador/           registro de dependencias, un archivo por bloque
  pruebas/               prueba de capas (repositorios de mentiras)
front_blazor/            Blazor Server: solo habla con la API por HTTP
db/                      script de la base + migraciones idempotentes
docs/spec_kit/           constitución y spec kit de cada versión
pruebas_humo/            pruebas contra el sistema levantado
```

## 5. Cómo trabaja el equipo

Nadie hace commits en `main`. Cada estudiante trabaja en su rama
`feature/<nombre>-<descripcion>`, hace commits pequeños con su propia identidad
de git (`git config user.name` / `user.email` **sin** `--global`, dentro de su
carpeta) y sube su rama; el encargado del `main` integra y, al pasar los
criterios, pone el tag de la versión. El reparto y el orden de cada entrega
están en el `8_tasks.md` de su versión.
