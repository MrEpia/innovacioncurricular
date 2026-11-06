# Quickstart — Versión 4

## 1. En local

```powershell
docker compose up -d --build        # la migración v4_001 siembra los datos de demostración
python verificar_todo.py --limpio   # todo desde cero: capas + 8 pruebas de humo
```

| Qué | Dónde |
|---|---|
| Sitio público | <http://localhost:8073> (Inicio, Servicios, Nosotros, Soporte, Contacto) |
| Aplicación | <http://localhost:8073/aplicacion> (después de ingresar) |
| Reportes | <http://localhost:8073/reportes> |
| Dashboard | <http://localhost:8073/dashboard> |
| API documentada | <http://localhost:8072/swagger> |

Solo la v4: `python pruebas_humo/humo_reportes.py` (criterios 1 a 8).

## 2. Publicar (criterio 10 y entrega)

**Quién:**
- **Sebastián Calvo:** la API y la base, en su cuenta.
- **David Epia:** el front, en la suya.

El plan gratuito permite un sitio por cuenta (D-v4-7).

### 2.1 Crear los sitios (cada uno, en el navegador)

1. Crear la cuenta gratuita en <https://www.monsterasp.net/>.
2. **Calvo:**
   - Crear un sitio ASP.NET Core, por ejemplo `innovacion-api`. Queda en `https://innovacion-api.runasp.net`.
   - En **Databases**, crear una base **MSSQL** y anotar su servidor, nombre, usuario y contraseña.
   - Activar el acceso remoto a la base, si el panel lo pide, para poder prepararla desde el PC.
3. **Epia:** crear un sitio ASP.NET Core, por ejemplo `innovacion-umv`. Es la URL que se entrega al profesor.

### 2.2 Preparar y compilar (en el PC, con Docker Desktop abierto)

```powershell
Copy-Item publicacion\.env.produccion.example publicacion\.env.produccion   # y llenarlo
python publicacion/publicar.py bd       # 25 tablas, catálogos, procedimientos y datos de demostración
python publicacion/publicar.py api      # publicacion/salida/api.zip
python publicacion/publicar.py front    # publicacion/salida/front.zip  (URL_API = https://innovacion-api.runasp.net)
```

### 2.3 Subir

En el panel de cada sitio:
1. Abrir **File Manager** (o conectarse por FTP).
2. Borrar el contenido de ejemplo de `wwwroot`.
3. Subir el `.zip` y descomprimirlo **dentro de `wwwroot`**.
4. Reiniciar el sitio.

### 2.4 Comprobar

| Qué | Resultado esperado |
|---|---|
| `https://innovacion-api.runasp.net/` | `{"version":"v4", …}` |
| `https://innovacion-api.runasp.net/swagger` | La documentación |
| El sitio del front | La página de Inicio |
| «Ingresar», con `ADMIN_USUARIO` / `ADMIN_CLAVE` del `.env.produccion` | Entra |
| `/dashboard` y `/reportes` | Muestran los datos |
| En el celular | Menú plegable y opción «Agregar a la pantalla de inicio» |

Después, anotar las dos URL en el README y poner el tag:

```powershell
git tag v4
git push origin v4
```

## 3. Problemas conocidos

| Síntoma | Causa y solución |
|---|---|
| HTTP 500.30 al abrir la API | La aplicación no arrancó. Mire el log del sitio. Lo más común: `JWT_SECRETO` con menos de 32 caracteres o la cadena de conexión mal copiada |
| El front dice «El servicio no está disponible» | `URL_API` mal escrita en el `.env.produccion`. Vuelva a correr `publicar.py front` y suba el zip otra vez |
| Hay que volver a ingresar sin razón | El plan gratuito tiene 256 MB y reinicia la aplicación al pasarlos. La sesión vive en memoria y se pierde (D-v4-7) |
| `publicar.py bd` no se conecta | El panel no permite conexiones remotas a la base. Actívelas o corra los scripts desde el administrador SQL del panel: primero `db/innovacion_curricular.sql` **sin** la línea `USE innovacion_local;` y después cada archivo de `db/migraciones/` en orden |
