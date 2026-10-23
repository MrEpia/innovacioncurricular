# Historias de usuario — Versión 3

## Historia 19 — Ingresar con mi usuario

| Usuario | Prioridad | Riesgo | Puntos | Responsable |
|---|---|---|---|---|
| Cualquier usuario del sistema | Alta | Alto | 5 | API: Calvo · Front: Epia |

**Yo**, como usuario del sistema, **quiero** ingresar con mi usuario y
contraseña **para** que nadie más pueda ver ni cambiar la información del
currículo en mi nombre.

**Criterios de aceptación**
1. Con usuario y clave correctos entro y veo el menú de mi rol.
2. Con datos equivocados veo «Usuario o contraseña incorrectos», sin pistas de cuál falló.
3. Si mi usuario fue retirado, no puedo entrar.
4. Al cerrar sesión, volver atrás en el navegador no me deja ver nada sin ingresar de nuevo.

## Historia 20 — Que cada rol vea y haga solo lo suyo

| Usuario | Prioridad | Riesgo | Puntos | Responsable |
|---|---|---|---|---|
| Coordinación curricular | Alta | Alto | 5 | API: Calvo · Front: Epia |

**Yo**, como coordinación curricular, **quiero** que el personal de consulta
solo pueda leer y que los editores no puedan retirar información **para** que
nadie borre por error lo que otros registraron.

**Criterios de aceptación**
1. Consulta solo lee; editor lee, crea y modifica; administrador hace todo.
2. Aunque alguien arme la petición a mano, la API la rechaza con 403.
3. En pantalla no aparecen los botones que el rol no puede usar.

## Historia 21 — Administrar usuarios y roles

| Usuario | Prioridad | Riesgo | Puntos | Responsable |
|---|---|---|---|---|
| Administrador | Alta | Medio | 8 | API: Calvo · Front: Brunal |

**Yo**, como administrador, **quiero** crear usuarios, asignarles y quitarles
roles y retirarlos **para** controlar quién tiene acceso y a qué.

**Criterios de aceptación**
1. Solo el administrador ve y usa el panel de administración.
2. Las contraseñas se guardan hasheadas y no se muestran nunca.
3. No puedo dejar el sistema sin administrador.
4. Un usuario nuevo sin rol no puede ingresar hasta que le asigne uno.

## Historia 22 — Sesión que vence

| Usuario | Prioridad | Riesgo | Puntos | Responsable |
|---|---|---|---|---|
| Cualquier usuario | Media | Medio | 3 | API: Calvo · Front: Epia |

**Yo**, como usuario, **quiero** que mi sesión venza sola después de un tiempo
**para** que un computador olvidado abierto no quede con acceso indefinido.

**Criterios de aceptación**
1. Pasado el tiempo configurado, la API rechaza el token (401 «expiró»).
2. La pantalla me lleva a ingresar con el aviso «Su sesión expiró».
