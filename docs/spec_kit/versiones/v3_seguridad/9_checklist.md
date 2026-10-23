# Lista de chequeo — Versión 3

> Solo con evidencia: el comando y su resultado.

## A. Especificación

- [ ] Ningún `[NECESITA ACLARACIÓN]` en `2_spec.md`
- [ ] Constitución 1.2.0 con las enmiendas que pidió el plan
- [ ] `6_contracts.md` coincide con Swagger (incluido el esquema Bearer)

## B. Criterios de aceptación

| # | Criterio | Evidencia | ✔ |
|---|---|---|---|
| 1 | v3 sin token en `/`; recursos sin token → 401 | `humo_seguridad.py` | |
| 2 | Login correcto, incorrecto (mismo mensaje), retirado, sin roles | `humo_seguridad.py` | |
| 3 | Token adulterado, de otra llave, vencido → 401 | `humo_seguridad.py` | |
| 4 | Permisos de consulta, editor y administrador | `humo_seguridad.py` + prueba de capas | |
| 5 | Administración solo para administrador | `humo_seguridad.py` | |
| 6 | Contraseñas hasheadas, nunca expuestas | `humo_seguridad.py` (consulta a la base) | |
| 7 | Siempre queda un administrador | `humo_seguridad.py` + prueba de capas | |
| 8 | Prueba de capas sin SQL Server | `docker compose run … PruebaCapas` | |
| 9 | Front: redirección a `/ingresar`, menú por rol, cerrar sesión | recorrido §3 de `7_quickstart.md` | |
| 10 | Front: editor sin «Retirar» ni «Administración» | recorrido §3 | |
| 11 | Front: sesión vencida → `/ingresar` | recorrido §3, paso 5 | |
| 12 | Regresión v1 y v2 | humos de la v2 y `humo_front.py` | |

## C. Seguridad y secretos

- [ ] `git grep -n "JWT_SECRETO=\|ADMIN_CLAVE="` solo encuentra `.env.example` con valores de mentira
- [ ] La API se niega a arrancar con un `JWT_SECRETO` de menos de 32 caracteres
- [ ] El navegador no guarda el JWT (DevTools → Application: solo una cookie `HttpOnly`)

## D. Firmas

| Estudiante | Revisó | Fecha |
|---|---|---|
| Sebastián Calvo (encargado del `main`) | | |
| David Epia | | |
| Sebastián Brunal | | |
