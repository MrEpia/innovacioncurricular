# Quickstart — Versión 3

## 1. Arranque

```powershell
# Si su .env es de la v2, agréguele las variables nuevas de .env.example:
#   JWT_SECRETO (≥ 32 caracteres), JWT_MINUTOS, ADMIN_USUARIO, ADMIN_CORREO, ADMIN_CLAVE
docker compose up -d --build
```

No hace falta borrar el volumen: `init.sh` corre `v3_001_seguridad.sql` sobre
la base que ya existe, y la API crea el administrador al arrancar.

Ingresar: <http://localhost:8073/ingresar> con `ADMIN_USUARIO` / `ADMIN_CLAVE`.

Para probar en Swagger: `POST /api/login`, copiar el `token`, botón
**Authorize** y pegarlo.

## 2. Comprobaciones

```powershell
# Criterio 8 — prueba de capas (incluye la seguridad), sin SQL Server
docker compose run --rm --no-deps api-innovacion dotnet run --project pruebas/PruebaCapas.csproj

# Criterios 1 a 7 — seguridad de la API
python pruebas_humo/humo_seguridad.py

# Criterio 12 — regresión: inician sesión solas con el administrador del .env
python pruebas_humo/humo_academico.py
python pruebas_humo/humo_actividades.py
python pruebas_humo/humo_puentes.py
python pruebas_humo/humo_front.py          # (actualizado por Epia en la ronda 4)
```

## 2.1 Front por rol y logout (criterios 9 a 11, automatizados)

```powershell
# Menú y botones según el rol, panel de administración solo para el
# administrador, y la cookie de antes del logout ya no abre nada (30 comprobaciones)
python pruebas_humo/humo_roles_front.py
```

## 3. Recorrido manual (criterios 9 a 11)

1. Sin sesión, abrir <http://localhost:8073/programas> → lleva a `/ingresar`.
2. Ingresar como administrador → menú completo con «Administración».
3. En «Administración → Usuarios», crear `ana` con rol `editor`; cerrar sesión.
4. Ingresar como `ana`: no hay «Administración» ni botones «Retirar»; escribir
   a mano `/admin/usuarios` → «sin permiso».
5. Bajar `JWT_MINUTOS=1` en el `.env`, `docker compose up -d`, ingresar, esperar
   un minuto y guardar algo → lleva a `/ingresar` con «Su sesión expiró».

## 4. Problemas conocidos

| Síntoma | Causa | Solución |
|---|---|---|
| La API no arranca: «Falta Jwt__Secreto o tiene menos de 32 caracteres» | `.env` de la v2 | Agregar `JWT_SECRETO` largo |
| Nadie puede ingresar | No se creó el administrador inicial | `docker compose logs api-innovacion` → buscar «administrador inicial» |
| Todos deben volver a ingresar tras reiniciar el front | La sesión vive en la memoria del servidor del front (D-v3-7) | Es el comportamiento esperado |
