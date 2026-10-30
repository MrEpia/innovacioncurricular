# Cómo se construyeron las entregas 2 y 3 con IA — el prompt usado

> La [GUIA_IA1.md](../v1_aliado/GUIA_IA1.md) explica cómo construir una
> versión con IA. Esta guía registra **lo que realmente se hizo** en las
> entregas 2 y 3: el prompt exacto, cómo se supervisó y qué se corrigió.
> Como dice el plan del módulo, *el chat se cierra, la guía queda*.
>
> La continuación (diagramas de secuencia, verificación total y
> procedimientos almacenados) está en la
> [GUIA_IA3.md](../v3_seguridad/GUIA_IA3.md).

## 0. Ficha

| | |
|---|---|
| Camino | **B — IDE agéntico**: Claude Code (modelo Claude Opus), trabajando sobre la copia local del repositorio |
| Quién lo usó | Sebastián Calvo, encargado del `main` |
| Fecha | 2026-10-22 y 2026-10-23 |
| Entregas | 2 (CRUD de las 15 tablas con clave foránea) y 3 (JWT, sesión y roles), en una sola sesión |
| Material que se le dio | El repositorio completo, `modulo_innovacion_curricular.pdf` (requisitos del curso) y `TutorialEstructurasEmergenciasPython_Resumen.html` (método de ramas y rondas) |
| Lo que leyó por su cuenta | `docs/spec_kit/` completo y `ProyectosDeAula/docs/0_METODOLOGIA.md` |

## 1. El prompt (primer mensaje, tal cual)

El prompt cubre las dos entregas: la 3 depende de la 2 y el reparto en rondas
tenía que verlas juntas.

```text
PROMPT PARA CLAUDE PRO — AUDITORÍA Y CONTINUACIÓN DEL PROYECTO INNOVACIÓN CURRICULAR (ENTREGAS 2 Y 3)
Actúa como un ingeniero de software senior especializado en desarrollo backend y frontend, APIs REST, bases de datos relacionales, Docker, Git/GitHub y Spec-Driven Development con Spec Kit.
1. Contexto y objetivo
Debes revisar, corregir y continuar el desarrollo del proyecto existente en este repositorio:
https://github.com/MrEpia/innovacioncurricular/tree/main
Los documentos de referencia son:

* `modulo_innovacion_curricular.pdf`: contiene los requisitos funcionales, técnicos, entregables y criterios de evaluación del proyecto.
* `TutorialEstructurasEmergenciasPython_Resumen.html`: establece la metodología de trabajo colaborativo mediante ramas individuales, rondas de desarrollo, commits frecuentes y descriptivos, y posterior integración en `main`.

Primero inspecciona el repositorio completo, incluyendo el código fuente, la estructura de carpetas, la configuración de Docker, los archivos de base de datos, las pruebas, la documentación y todos los artefactos de Spec Kit existentes.
No empieces desde cero. Conserva la arquitectura y las funcionalidades que ya estén bien implementadas. Corrige los errores y realiza los cambios necesarios para que el proyecto cumpla con los requisitos.
El objetivo es completar la segunda y tercera entrega del módulo, entendidas como las entregas académicas definidas en el PDF, sin confundirlas con versiones comerciales del software.
2. Fase inicial: auditoría obligatoria
Antes de modificar archivos:

1. Inspecciona todas las carpetas y archivos relevantes del repositorio.
2. Identifica los lenguajes, frameworks, motores de base de datos y dependencias utilizados.
3. Determina qué funcionalidades están terminadas, incompletas, duplicadas o presentan errores.
4. Revisa los archivos de Spec Kit existentes: constitución, especificaciones, planes, tareas y demás documentación disponible.
5. Comprueba si la implementación coincide con las especificaciones y con la arquitectura definida.
6. Revisa las migraciones, el esquema de la base de datos, las claves primarias, las claves foráneas y las relaciones entre tablas.
7. Identifica errores de compilación, dependencias faltantes, problemas de configuración, rutas incorrectas, endpoints incompletos y fallos de integración.
8. Examina el estado de Git, las ramas existentes, los commits y los posibles cambios locales sin guardar.

Entrega un diagnóstico inicial breve con los hallazgos y el plan de trabajo. Después, continúa con la implementación; no te detengas en el diagnóstico.
No sobrescribas cambios locales del equipo, no elimines archivos sin justificación y no realices operaciones destructivas sobre ramas o datos existentes.
3. Requisitos de la segunda entrega
La segunda entrega debe completar el CRUD de las 22 tablas del módulo Innovación Curricular, incluidas las 7 tablas independientes de la primera entrega y las 15 tablas adicionales con claves foráneas.
Debes verificar y completar, según corresponda:
Backend y API REST

* Implementar los endpoints CRUD de las 22 tablas.
* Mantener respuestas JSON y códigos HTTP apropiados.
* Validar los datos recibidos, los tipos, los campos obligatorios y las restricciones de negocio.
* Validar las claves foráneas al crear y actualizar registros.
* Evitar registros huérfanos y respetar la integridad referencial.
* Gestionar correctamente los errores y los casos en los que no existan registros.
* Implementar borrado lógico, sin eliminar físicamente los registros.
* Excluir de las consultas normales los registros inactivos, respetando las relaciones y los requisitos existentes.
* Mantener la separación entre controladores, servicios, acceso a datos y demás capas, de acuerdo con la arquitectura real del proyecto.

Tablas con relaciones
Verifica especialmente las relaciones de:

* `facultad` y `universidad`.
* `programa` y `facultad`.
* `acreditacion` y `programa`.
* `registro_calificado` y `programa`.
* `activ_academica` y `programa`.
* `pasantia` y `programa`.
* `premio` y `programa`.
* `programa_ac`, `programa_pe`, `programa_ci` y `an_programa`.
* `enfoque_rc` y `aa_rc`.
* `docente_departamento` y `alianza`.

Verifica los nombres reales de las columnas, las claves primarias y las relaciones en el esquema existente. No inventes columnas ni cambies nombres sin comprobar antes las dependencias.
Frontend

* Completar las interfaces CRUD de las 22 tablas.
* Implementar formularios de creación y edición, listados, consultas y confirmación de eliminación lógica.
* En los formularios con claves foráneas, utilizar listas desplegables o controles equivalentes alimentados desde la API.
* Mostrar correctamente los estados de carga, errores de validación, respuestas del servidor y operaciones exitosas.
* Comprobar que el frontend consume la API REST y no se conecta directamente a la base de datos.
* Mantener una interfaz coherente y adaptable a diferentes tamaños de pantalla.

4. Requisitos de la tercera entrega
Implementa o completa la autenticación y autorización conforme al PDF.
Backend

* Crear o corregir `POST /api/login` para autenticar usuarios y devolver un JWT válido.
* Implementar la generación y validación de tokens JWT.
* Configurar el middleware de autenticación para proteger las rutas correspondientes.
* Implementar autorización basada en roles y permisos.
* Completar el CRUD de las tablas `usuario`, `rol` y `rol_usuario`.
* Restringir la administración de usuarios y roles al administrador.
* Aplicar los permisos en el backend, no solamente ocultando opciones en la interfaz.
* Manejar contraseñas de forma segura mediante hash adecuado; nunca almacenarlas en texto plano.
* Gestionar expiración de tokens, credenciales inválidas, usuarios inactivos y acceso no autorizado.

Verifica y aplica los roles previstos en el proyecto. Como mínimo, comprueba el comportamiento esperado para administrador, consulta y editor: el administrador tiene acceso completo; consulta solo puede leer; editor puede consultar y modificar, pero no eliminar. Si la especificación existente define reglas más concretas, respétalas y documenta cualquier discrepancia.
Frontend

* Implementar o corregir la página de inicio de sesión.
* Gestionar la sesión del servidor y almacenar el JWT en dicha sesión, conforme al diseño del proyecto.
* Enviar el token mediante `Authorization: Bearer <token>` en las peticiones protegidas.
* Implementar un menú dinámico según los roles del usuario autenticado.
* Restringir la visualización y el acceso a las opciones que no correspondan a cada rol.
* Crear el panel de administración de usuarios y roles, visible únicamente para administradores autorizados.
* Implementar el cierre de sesión, destruyendo la sesión y eliminando el token almacenado.
* Comprobar el comportamiento cuando la sesión expire o la API rechace el token.

No dupliques funcionalidades que ya estén correctamente implementadas. Integra los cambios con la arquitectura actual.
5. Spec Kit: mantener la coherencia entre especificaciones y código
Spec Kit debe seguir siendo la referencia para el diseño y desarrollo del sistema.

1. Inspecciona los artefactos existentes antes de modificarlos.
2. Compara las especificaciones, los planes y las tareas con la implementación real.
3. Identifica requisitos ausentes, contradicciones y funcionalidades implementadas de forma diferente a lo especificado.
4. Actualiza las especificaciones, los planes y las tareas cuando los cambios lo requieran.
5. Si existe un flujo de comandos de Spec Kit configurado, utiliza sus comandos reales y compatibles con el proyecto.
6. Completa las tareas pendientes de las entregas 2 y 3.
7. Añade o actualiza las pruebas necesarias para verificar el cumplimiento.
8. Comprueba al final que el código, la documentación y las especificaciones estén alineados.

No inventes comandos de Spec Kit ni reemplaces la metodología existente por otra sin justificación. No modifiques la constitución ni los requisitos fundamentales únicamente para que el código actual parezca cumplirlos.
6. Docker y entorno de ejecución
Revisa toda la configuración existente de Docker y realiza los cambios necesarios para que el proyecto pueda ejecutarse de manera reproducible.
Verifica, según la arquitectura real:

* `Dockerfile` de cada aplicación que deba contenerizarse.
* `docker-compose.yml` o `compose.yaml`.
* Configuración del servicio de base de datos.
* Variables de entorno y archivos de configuración.
* Conexiones entre frontend, API y base de datos.
* Puertos, dependencias entre servicios y persistencia de datos.
* Migraciones y mecanismos de inicialización de la base de datos.
* Healthchecks y tiempos de disponibilidad cuando sean necesarios.
* Volúmenes, redes y políticas de reinicio.
* Archivos `.dockerignore` y `.gitignore`.
* Documentación de instalación, ejecución y diagnóstico.

No expongas credenciales ni secretos en el repositorio. Utiliza variables de entorno y un archivo de ejemplo como `.env.example`, sin valores secretos reales.
Si es necesario modificar Docker, hazlo de manera compatible con la arquitectura existente. No añadas servicios innecesarios ni cambies de tecnología sin una razón técnica.
Comprueba que los contenedores se construyan y arranquen, que la API se conecte a la base de datos y que las operaciones principales funcionen. No afirmes que una prueba fue exitosa si no la ejecutaste.
7. Trabajo colaborativo con tres estudiantes
Debes organizar el desarrollo siguiendo el tutorial proporcionado, adaptándolo al proyecto de Innovación Curricular.
Cada estudiante debe tener su propia rama, commits descriptivos y una contribución identificable en GitHub. Nadie debe trabajar directamente sobre `main`.
Primero identifica mi identidad real de Git a partir de la configuración local y de la cuenta autenticada disponible. Si no puedes determinarla con certeza, solicita únicamente el dato que falte. No asumas que el propietario del repositorio es mi cuenta personal.
Organización

* Estudiante 1: yo, responsable de los cambios que me asignes.
* Estudiante 2: compañero A.
* Estudiante 3: compañero B.

Distribuye el trabajo en tres bloques razonablemente equilibrados, evitando que los tres estudiantes modifiquen continuamente los mismos archivos.
Asigna responsabilidades concretas, por ejemplo, módulos CRUD relacionados, autenticación y autorización, frontend, pruebas, documentación o Docker, según las dependencias y el estado real del proyecto.
Ten en cuenta que las tareas de la segunda entrega son una dependencia de la tercera. Organiza las ramas y los puntos de integración para que los cambios puedan combinarse sin dejar el sistema en un estado inconsistente.
Rondas y ramas
Sigue el ciclo de trabajo del tutorial:

1. Sincronizar la rama de referencia.
2. Crear una rama de trabajo por estudiante y por bloque o ronda.
3. Implementar una parte concreta del desarrollo.
4. Ejecutar las verificaciones correspondientes.
5. Realizar varios commits pequeños y descriptivos.
6. Subir la rama correspondiente.
7. Revisar los cambios e integrar las ramas de forma controlada.
8. Resolver conflictos sin descartar trabajo válido.
9. Volver a ejecutar las pruebas tras cada integración.

Utiliza nombres de ramas claros, por ejemplo:

* `feature/<mi-identificador>/entrega-2`
* `feature/<identificador-companero-a>/entrega-2`
* `feature/<identificador-companero-b>/entrega-2`
* `feature/<mi-identificador>/entrega-3`

Adapta estos nombres a las convenciones y ramas ya existentes. No crees ramas redundantes si existe una estructura válida que pueda reutilizarse.
Commits y atribución
Utiliza mensajes descriptivos siguiendo convenciones como:

* `feat: implementar CRUD de facultades`
* `feat: validar relaciones entre programas y facultades`
* `feat: agregar autenticación JWT`
* `fix: corregir validación de claves foráneas`
* `test: agregar pruebas de autorización por roles`
* `docs: actualizar instrucciones de Docker`

Divide el trabajo en commits frecuentes, relacionados con cambios reales y verificables. Evita un único commit gigante con todo el proyecto.
Muy importante:

* Ejecuta y sube únicamente los commits que me correspondan a mí, utilizando mi identidad real y autorizada.
* No suplantes a los otros dos estudiantes ni atribuyas commits a cuentas ajenas.
* Para mis compañeros, prepara por separado las instrucciones, ramas, archivos que deben modificar y comandos de Git que necesitan ejecutar desde sus propias copias del repositorio.
* Entrégame esa información en tu respuesta final, no la incluyas como un archivo adicional dentro del proyecto.
* Los compañeros deben revisar y ejecutar sus propios cambios con sus respectivas identidades de Git configuradas.
* Si no dispones de permisos para subir cambios, indícalo claramente y prepara los comandos para que yo pueda ejecutarlos.

No inventes commits, autores, ramas remotas ni resultados de publicación.
8. Pruebas y criterios de aceptación
No des por terminado el trabajo únicamente porque el código esté escrito.
Ejecuta todas las pruebas viables y comprueba:

1. Compilación y restauración de dependencias.
2. Arranque del backend, frontend y base de datos.
3. Operaciones CRUD de las tablas del módulo.
4. Validación de claves foráneas y restricciones.
5. Borrado lógico y exclusión de registros inactivos.
6. Login correcto e incorrecto.
7. Validez, expiración y rechazo de JWT.
8. Permisos de administrador, consulta y editor.
9. Protección de endpoints de administración.
10. Comportamiento de las sesiones y logout.
11. Integración frontend-API.
12. Construcción y ejecución mediante Docker.
13. Pruebas automatizadas existentes y nuevas.
14. Coherencia entre Spec Kit, documentación e implementación.

Añade pruebas donde falten, especialmente para errores, casos límite y permisos.
No elimines pruebas para conseguir que la compilación pase. No desactives validaciones de seguridad ni ocultes errores. Si una prueba no puede ejecutarse por falta de servicios, credenciales o permisos, documenta la limitación y proporciona el procedimiento para completarla.
9. Reglas para modificar el proyecto

* Trabaja sobre el repositorio existente.
* Respeta las tecnologías y convenciones actuales, salvo que exista una razón documentada para cambiarlas.
* Inspecciona antes de modificar.
* Corrige las causas raíz, no solamente los síntomas.
* Mantén compatibilidad con los datos existentes y prepara migraciones cuando sean necesarias.
* No elimines datos, migraciones ni funcionalidades válidas sin justificación.
* No introduzcas secretos, credenciales reales ni información privada en commits.
* No hagas `push --force`, reseteos destructivos ni fusiones indiscriminadas.
* No marques tareas como terminadas sin evidencia de verificación.
* No dejes errores conocidos sin documentar.
* Si encuentras un conflicto entre el PDF, Spec Kit y el código, registra el conflicto y resuélvelo siguiendo los requisitos académicos y las decisiones arquitectónicas justificadas.

10. Resultado final esperado
Al finalizar, entrégame un informe claro y verificable que contenga:
A. Auditoría

* Estado inicial del repositorio.
* Errores detectados.
* Funcionalidades faltantes.
* Cambios realizados y motivos.

B. Implementación

* Funcionalidades completadas de la segunda entrega.
* Funcionalidades completadas de la tercera entrega.
* Cambios en la API, frontend, base de datos, autenticación y Docker.
* Archivos principales modificados.

C. Spec Kit

* Especificaciones, planes y tareas actualizados.
* Requisitos cumplidos y pendientes.
* Inconsistencias que no se pudieron resolver.

D. Pruebas

* Comandos ejecutados.
* Resultados reales.
* Errores corregidos.
* Pruebas pendientes y cómo ejecutarlas.

E. Git y GitHub

* Mi rama y mis commits reales.
* Ramas y commits que deben crear mis dos compañeros.
* Comandos completos para que cada compañero pueda continuar, hacer sus commits y subir su rama.
* Orden recomendado de integración.
* Estado de la rama principal y posibles conflictos.
* No incluyas estos comandos para mis compañeros dentro de los archivos del proyecto.

F. Docker

* Comandos para construir, levantar, verificar y detener los servicios.
* Variables de entorno necesarias.
* Problemas conocidos y soluciones.

G. Estado de cumplimiento
Presenta una tabla con cada requisito relevante del PDF, su estado (`Cumplido`, `Parcial` o `Pendiente`) y la evidencia correspondiente.
H. Próximos pasos
Indica exactamente qué debo hacer yo y qué deben hacer mis compañeros para cerrar las entregas 2 y 3.
Instrucción final
Comienza inspeccionando el repositorio y sus artefactos de Spec Kit. Después, implementa los cambios reales necesarios siguiendo las fases anteriores.
No te limites a generar un plan, ejemplos de código o instrucciones teóricas: modifica los archivos, ejecuta las pruebas posibles, organiza el trabajo mediante ramas y prepara los commits que me corresponden.
Si una operación requiere permisos, acceso a GitHub o intervención de otra persona, no simules haberla realizado. Completa todo lo que sí esté a tu alcance y explícame exactamente qué falta para finalizar.
```

## 2. Por qué el prompt está escrito así

| Regla del prompt | Qué evitó |
|---|---|
| «No empieces desde cero… Conserva la arquitectura» | Que la IA reescribiera la v1 en otro estilo. Se reutilizaron las capas, el ensamblador y el front de la v1 |
| «Auditoría obligatoria» antes de modificar | Encontró ramas `feature/v2-*` que no compilaban y firmadas por una cuenta ajena. No se integraron |
| «No modifiques la constitución… solo para que el código parezca cumplirlos» | Cada cambio de regla pasó por el Artículo 11: enmiendas 1.1.0 y 1.2.0, con su decisión en el `4_research.md` |
| «No suplantes a los otros dos estudiantes» | La IA solo hizo commits con la identidad del encargado. El trabajo de David se le entregó como parches y comandos, para que lo subiera con su propia identidad. *(El prompt habla de tres estudiantes porque se escribió con el reparto inicial; Sebastián Brunal salió del grupo antes de la entrega 2 y sus bloques los hizo David.)* |
| «Entrégame esa información en tu respuesta final, no… dentro del proyecto» | Las instrucciones de cada compañero quedaron fuera del repositorio |
| «No afirmes que una prueba fue exitosa si no la ejecutaste» | Cada fase se cerró con la salida real de la prueba de capas y de las pruebas de humo |
| «No introduzcas secretos» | Todos los valores van en `.env`; `.env.example` solo tiene valores de mentira (Artículo 7) |

## 3. Cómo se supervisó

1. **Diagnóstico primero.** La IA entregó la auditoría (estado de las ramas,
   qué compilaba y qué faltaba) antes de tocar archivos.
2. **Spec antes que código.** Los spec kits `v2_entrega2/` y
   `v3_seguridad/` se escribieron y se revisaron con su `9_checklist.md`
   antes de implementar.
3. **Una rama por bloque**, con commits pequeños:
   `feature/sebastian-calvo-entrega2-montaje`, `…-entrega2-academico` y
   `…-entrega3-api-seguridad`. Las tareas de cada uno están en el
   [8_tasks.md](8_tasks.md).
4. **Evidencia en cada fase**: la prueba de capas sin base de datos (165
   comprobaciones) y las pruebas de humo contra el sistema levantado.
5. **Orden de integración probado** antes de fusionar: montaje → académico
   → David R2 (Actividades) → David R2 (Puentes) → David R4 (sesión) → David R5 (panel).

## 4. Lo que la IA rompió y a dónde fue cada corrección

Se aplican los tres destinos de la [GUIA_IA1.md](../v1_aliado/GUIA_IA1.md).

| Error | Destino | Corrección |
|---|---|---|
| La clase de la prueba de capas se llamaba `Programa`, igual que el modelo, y chocaban | Supervisor | Se renombró a `PruebaDeCapas` |
| El claim del rol salía con la URI larga de .NET y los permisos no lo reconocían | Supervisor | Se cambió al claim `"role"` |
| El comando documentado para la prueba de capas (`docker compose exec`) reiniciaba la API | **Spec**: `7_quickstart.md` | Se cambió a `docker compose run --rm --no-deps` |
| El 422 por JSON mal formado respondía en inglés | **Spec**: Artículo 8 (todo en español) | Se tradujeron los errores del lector JSON |
| Las tildes de los mensajes de commit se dañaban en PowerShell 5.1 | Supervisor | Los commits se escriben con `git commit -F` desde un archivo UTF-8 |

## 5. Para repetirlo

Abra el IDE agéntico en la raíz del repositorio, adjunte el PDF del módulo y
el tutorial de ramas, y pegue el prompt de la sección 1. No le crea
«terminado» sin la salida real de:

```powershell
python verificar_todo.py --limpio
```
