# Cómo se construyó y se cerró la entrega 3 con IA — los prompts usados

> El prompt principal de las entregas 2 y 3 está completo en la
> [GUIA_IA2.md](../v2_entrega2/GUIA_IA2.md#1-el-prompt-primer-mensaje-tal-cual).
> Sus secciones **4** («Requisitos de la tercera entrega») y **8** (pruebas y
> criterios de aceptación) son las que construyeron esta versión.
>
> Esta guía registra lo que vino **después**: los prompts de seguimiento con
> los que se auditó, se documentó y se cerró la entrega. Cada uno aparece tal
> cual lo escribió el estudiante, con lo que produjo y su evidencia.

## 0. Ficha

| | |
|---|---|
| Camino | **B — IDE agéntico**: Claude Code (modelo Claude Opus) |
| Quién lo usó | Sebastián Calvo, encargado del `main` |
| Fecha | 2026-10-23 |
| Regla de trabajo | Igual que en la v2: una rama `feature/sebastian-calvo-…` por cambio, commits pequeños y fusión en `main` solo con la verificación en verde |

## 1. La parte del prompt principal que construyó la v3

Resumen de la sección 4 del prompt (el texto exacto está en la GUIA_IA2):

- **Backend:** `POST /api/login` con JWT, middleware de autenticación, roles
  y permisos aplicados en la API, CRUD de `usuario`, `rol` y `rol_usuario`
  solo para administrador, contraseñas con hash, y manejo de token vencido,
  credenciales inválidas y usuario inactivo.
- **Roles:** administrador hace todo, consulta solo lee y editor consulta y
  modifica pero no elimina.
- **Front:** página de ingreso, JWT guardado en la sesión del servidor,
  `Authorization: Bearer` en cada petición, menú según el rol, panel de
  administración y cierre de sesión que destruye la sesión.

Las decisiones que tomó la IA para cumplirlo, con la alternativa descartada
en cada caso, están en [4_research.md](4_research.md) (D-v3-1 a D-v3-9).

## 2. Prompts de seguimiento (en orden)

| # | Prompt (tal cual) | Qué produjo | Evidencia |
|---|---|---|---|
| 1 | «verifica rapidamente si se cumple con los métodos solid» | Revisión de los cinco principios SOLID sobre las capas: una clase por responsabilidad, interfaces por tabla e inyección de dependencias | Revisión en el chat; no cambió código |
| 2 | «puedes dejar todo esto listo y que ellos solo tengan que hacer commits?» | Los bloques de David y Brunal como parches con un script por ronda, **fuera del repositorio**, para que cada uno los subiera con su propia identidad | Ramas de los compañeros en GitHub |
| 3 | «ya los otros compañeros subieron sus partes, no hay un archiivo que tenga todo para verificar que todo este funcionando bien?» | `verificar_todo.py`: levanta el sistema y corre la prueba de capas y las cinco pruebas de humo | Commit `7cc225d` |
| 4 | «agrega los tags y abre donde está todo el proyecto» | Tags `v1`, `v2` y `v3` en GitHub | `git tag` |
| 5 | «explicame donde esta la documentacion del contrato y las capas con las que estamos trabajando, recuerda que cada metodo debe tener tambien un diagrama de secuencia que dice lo que hace» | [DIAGRAMAS_DE_SECUENCIA.md](../../DIAGRAMAS_DE_SECUENCIA.md): 17 diagramas Mermaid, uno por método, recorriendo Controller → Servicio → Repositorio → BD | Commits `effbd91` y `50ca186` |
| 6 | «verifica que si se esta cumpliendo lo que dice cada spec y contrató» | Se corrió `verificar_todo.py --limpio`. Dio 4 fallos, que resultaron ser de la codificación de Windows en la salida de las pruebas, no del sistema | Commit `5585fc9`, luego 10/10 en verde |
| 7 | «tenemos los procedimientos de almacenado?» | Diagnóstico: no había ninguno, porque el Artículo 2 pedía el SQL a la vista en los repositorios | Búsqueda en `.sql` y `.cs` |
| 8 | «Si, es necesario agregarlos, modifica esto 1. Enmendar la constitución (Art. 11)… 2. Crear una migración en `db/migraciones/`… 3. Cambiar los repositorios. 4. Volver a correr `verificar_todo.py`. no se cuales tablas lo necesitan verifica en cuales es necesario» | Constitución 1.3.0, decisión D-v3-10 y migración `v3_002` con 27 procedimientos, solo para las operaciones de comprobar y escribir. Se cambiaron 15 repositorios | Commit `f1ee708`, `verificar_todo.py --limpio` 10/10 y prueba directa de los códigos 50409/50410 en SQL Server |
| 9 | «en la documentacion agregaste el prompt usado?» | Esta guía y la GUIA_IA2 | Commit `a72eece` |
| 10 | «donde esta maestro detalle» | Diagnóstico: no existía; solo estaba anunciado en `docs/PRINCIPIOS_ACID.md` | Búsqueda en el código y en el material del curso |
| 11 | «ok si es necesario hacer los maestro detalle, 1. Un endpoint que reciba el padre con su lista de hijos… 2. Un procedimiento almacenado o una transacción que guarde todo junto, o nada si algo falla. 3. Una pantalla en Blazor con el padre arriba y una tabla editable de hijos abajo. 4. Su spec, su contrato y su diagrama de secuencia, más la enmienda si hace falta. hazlo para los dos maestros que estas proponiendo» y «y lo subes» | Spec kit [v3_maestro_detalle](../v3_maestro_detalle/2_spec.md), enmienda 1.4.0, 2 procedimientos, 4 rutas, 2 pantallas y diagramas 18 a 21 | Prueba de capas (+20), `humo_maestro_detalle.py` (49) y recorrido de las dos pantallas en el navegador |

## 3. Lo que la IA rompió en este tramo

| Error | Destino | Corrección |
|---|---|---|
| El diagrama 15 no se dibujaba: un `;` dentro del texto separa instrucciones en Mermaid | Supervisor | Se cambió por una coma. Ahora se validan los 17 diagramas con el parser de Mermaid antes de subir |
| `verificar_todo.py` marcaba en rojo pruebas que sí pasaban: Windows capturaba la salida en cp1252 y las tildes rompían la impresión | Supervisor | Se fuerza `PYTHONIOENCODING=utf-8` en los procesos hijos |
| Primero contó 28 procedimientos y eran 27: había contado una línea de comentario | Supervisor | Se contó contra `sys.procedures` en la base real y se corrigió la documentación |
| Procedimientos almacenados contra el Artículo 2 | **Constitución**, por el Artículo 11 | Enmienda 1.3.0, propuesta en D-v3-10. No se metieron «por excepción» |

## 4. Cómo se decidió en qué tablas iban los procedimientos

El prompt 8 no decía cuáles tablas. Para no improvisar, la IA propuso un
criterio y lo dejó escrito en [4_research.md](4_research.md) (D-v3-10):
**solo donde una operación comprueba algo y después escribe**, porque entre
los dos pasos puede colarse otra petición. Resultado:

- **Necesitan procedimiento:** las 11 tablas que son padre de alguna otra
  (para retirarlas sin dejar huérfanos), y `rol`, `rol_usuario` y `usuario`
  (para no dejar el sistema sin administrador).
- **No lo necesitan:** las otras 11 tablas. Su CRUD es una sola sentencia,
  que ya es atómica.
- **Pendiente anotado:** crear un hijo sigue en dos pasos.
