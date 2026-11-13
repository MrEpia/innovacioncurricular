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

Todo va en **una cuenta gratuita** de MonsterASP, que admite 5 sitios y 5 bases (D-v4-7).

| Pieza | Dirección |
|---|---|
| Sitio (se entrega al profesor) | <http://innovacion-umv.runasp.net> |
| API | <http://innovacion-api.runasp.net> · `/swagger` |
| Base | MSSQL `db79553` (EU) |

### 2.1 En el panel (<https://admin.monsterasp.net>)

1. **Create website** dos veces, con «Free subdomain» y «My own application»:
   - `innovacion-api.runasp.net`
   - `innovacion-umv.runasp.net`
2. **Create database**: SQL Server, collation `SQL_Latin1_General_CP1_CI_AS`, datacenter EU.
3. En la base, **Manage**:
   - pestaña **Local access**: el servidor interno (`SQL_SERVIDOR`), el login y la contraseña;
   - pestaña **Remote access**: activarlo y anotar el servidor público (`SQL_SERVIDOR_REMOTO`).

### 2.2 En el PC (con Docker Desktop abierto)

```powershell
Copy-Item publicacion\.env.produccion.example publicacion\.env.produccion   # y llenarlo
python publicacion/publicar.py bd       # 25 tablas, procedimientos y datos de demostración
python publicacion/publicar.py api      # publicacion/salida/api.zip
python publicacion/publicar.py front    # publicacion/salida/front.zip
```

### 2.3 Subir cada zip

Para cada sitio:
1. **Manage → WebFTP → OPEN**.
2. En `wwwroot`, borrar el archivo de ejemplo.
3. Subir el zip y descomprimirlo **ahí mismo**: el `.dll` y el `web.config` quedan directamente en `wwwroot`.
4. Borrar el zip.

### 2.4 Comprobar

| Qué | Resultado esperado |
|---|---|
| `http://innovacion-api.runasp.net/` | `{"version":"v4", …}` |
| `/swagger` | La documentación |
| El sitio | La página de Inicio |
| «Ingresar» con `ADMIN_USUARIO` / `ADMIN_CLAVE` | Entra |
| Dashboard y reportes | Muestran datos |

### 2.5 HTTPS

El plan gratuito lo activa soporte a pedido:
1. En el panel, **Tickets** → pedir «manual HTTPS activation» para los dos subdominios.
2. Cuando lo activen, poner `URL_API=https://innovacion-api.runasp.net` en el `.env.produccion`.
3. Correr `publicar.py front` y volver a subir `front.zip`.

## 3. Problemas conocidos

| Síntoma | Causa y solución |
|---|---|
| HTTP 500.30 al abrir la API | La aplicación no arrancó. Mire el log del sitio. Lo más común: `JWT_SECRETO` con menos de 32 caracteres o la cadena de conexión mal copiada |
| El front dice «El servicio no está disponible» | `URL_API` mal escrita en el `.env.produccion`. Vuelva a correr `publicar.py front` y suba el zip otra vez |
| Hay que volver a ingresar sin razón | El plan gratuito tiene 256 MB y reinicia la aplicación al pasarlos. La sesión vive en memoria y se pierde (D-v4-7) |
| `publicar.py bd` no se conecta | Falta activar «Remote access» en la base, o `SQL_SERVIDOR_REMOTO` no es el nombre `….public.databaseasp.net`. Ojo: varios intentos fallidos bloquean la IP automáticamente |
| `https://` da `ERR_CONNECTION_RESET` | El plan gratuito no trae HTTPS activado: pídalo por ticket (§2.5) |
