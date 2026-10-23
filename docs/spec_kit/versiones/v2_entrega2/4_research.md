# Decisiones — Versión 2

> Las decisiones que hubo que tomar para escribir el [plan](3_plan.md), cada
> una con su alternativa descartada. Las cuatro primeras **enmendaron la
> constitución** (1.0.0 → 1.1.0), por el procedimiento de su Artículo 11.

## D-v2-1 — El ensamblador se reparte por bloque *(enmienda del Artículo 3)*

**Contexto.** La v2 la construyen tres estudiantes en paralelo, cada uno con
un bloque de tablas. En la v1 cada par interfaz→clase se registraba en
`Program.cs`: con tres ramas agregando quince pares, `Program.cs` sería un
conflicto de fusión garantizado en cada ronda.

**Decisión.** Una carpeta `Ensamblador/` en la API y otra en el front, con un
archivo por bloque (`EnsambladorV1`, `EnsambladorAcademico`,
`EnsambladorActividades`, `EnsambladorPuentes`). `Program.cs` solo los llama.
En la ronda 0 se crean los cuatro archivos —tres vacíos— y cada estudiante
llena el suyo.

**Alternativa descartada.** Que cada uno agregue sus líneas en `Program.cs` y
se resuelvan los conflictos al integrar. Funciona, pero cada integración
dependería de que el encargado entienda las líneas de los otros dos, y un
error resolviendo un conflicto de registro de dependencias no se ve hasta que
un endpoint responde 500.

**Lo que NO cambia.** Sigue habiendo un solo lugar que conoce clases
concretas; está repartido en archivos, no en responsabilidades.

## D-v2-2 — Los secretos salen del repositorio *(enmienda del Artículo 7)*

**Contexto.** La v1 heredó de la plantilla del curso la contraseña de `sa`
escrita en `docker-compose.yml` y en `appsettings.json`. El propio Artículo 7
decía «esa parte NO se copia», y la rúbrica anula el criterio de seguridad con
un secreto quemado. En la v3 llega la llave de firma del JWT: si no se
arreglaba ahora, serían dos secretos expuestos.

**Decisión.** Todo secreto en `.env` (fuera de git), con `.env.example`
versionado. El compose usa `${VAR:?mensaje}`.

**Alternativa descartada.** Dejarlo para la v4, cuando se publique. Para
entonces la contraseña llevaría tres versiones en la historia de git.

**Consecuencia.** La contraseña de la v1 sigue en la historia de los commits
anteriores: se cambia el valor (el `.env.example` trae otro) y se considera
expuesta.

## D-v2-3 — 409 para llave duplicada y para dependencias activas *(enmienda del Artículo 10)*

**Contexto.** Dos situaciones nuevas no tenían código: crear con una llave
que ya existe (la v1 respondía 500, C11) y retirar un padre con hijos activos.

**Decisión.** **409 Conflict** para las dos: la petición está bien formada y
apunta a un recurso que existe, pero choca con el estado actual de los datos.
Y **201** para la creación en las tablas de la v2.

**Alternativa descartada.** 500 para la llave duplicada, como la v1. Un 500
dice «falló el servidor» cuando lo que pasó es que el usuario repitió un dato;
el front no podría distinguir «corrija la resolución» de «la base se cayó».

**Lo que no se toca.** La v1 sigue con 200 al crear y 500 en llave
duplicada: es una versión cerrada y sus clientes (la prueba de humo, el front)
dependen de eso.

## D-v2-4 — El front comparte el transporte, no los recursos *(precisión del Artículo 10.1)*

**Contexto.** Cada servicio del front de la v1 traía su propia copia del
transporte HTTP: unas 60 líneas iguales. Con 22 recursos eran 22 copias, y en
la v3 cada petición tiene que llevar el token: habría que cambiarlas todas.

**Decisión.** `ClienteApi` concentra el transporte (enviar, reconocer el 204,
leer el sobre, traducir errores, avisar si la API no responde). Los servicios
de recurso siguen existiendo uno por tabla y son los únicos que las páginas
ven.

**Alternativa descartada.** Un `ServicioCrud<T>` genérico con el nombre de la
tabla como parámetro. Es exactamente lo que prohíbe el Artículo 10.1: las
páginas dejarían de decir qué recurso usan y el compilador dejaría de
revisarlo.

## D-v2-5 — La API valida las claves foráneas antes que el motor

**Contexto.** El motor rechaza (error 547) una FK hacia una fila que no existe.
Pero con borrado lógico, un padre retirado **sí existe**: el motor acepta un
hijo que apunte a él.

**Decisión.** Un repositorio dedicado, `IRepositorioReferencias`, con una
pregunta explícita por padre (`UniversidadActiva(id)`, `ProgramaActivo(id)`…),
y los servicios lo consultan antes de escribir. Si el padre no está activo:
`ReferenciaInvalidaExcepcion` → 400 con el nombre del campo.

**Alternativa descartada.** Un `Existe(string tabla, object id)` genérico: más
corto, y con el nombre de tabla viajando como texto, que es lo que el
Artículo 10.1 prohíbe.

**Riesgo aceptado.** Entre la comprobación y el `INSERT` otro usuario podría
retirar el padre. Para el volumen de este sistema es despreciable, y aun así la
FK del motor sigue de última defensa contra un padre inexistente.

## D-v2-6 — Retirar un padre con hijos activos se impide; no se hace en cascada

**Decisión.** El servicio pregunta `DependientesDeX(id)` antes de retirar. Si
hay hijos activos: `DependenciasActivasExcepcion` → 409 con «tiene registros
activos en acreditacion (2), premio (1). Retírelos primero».

**Alternativa descartada.** Retirar en cascada. Es cómodo de usar y peligroso
de tener: retirar un programa por error se llevaría de las listas once tablas
de información.

## D-v2-7 — PATCH valida sobre el registro resultante

**Decisión.** `ActualizarParcial` lee el registro actual, le aplica los
cambios en memoria y valida reglas y FK sobre el resultado. Luego escribe solo
lo que llegó.

**Alternativa descartada.** Validar solo los campos enviados. Una `fecha_fin`
que llega sola no se puede comparar con nada y pasaría siendo anterior a la de
inicio.

## D-v2-8 — Rutas con TODAS las columnas de la llave

**Decisión.** Las tablas de llave compuesta llevan cada columna como un
segmento de ruta: `/api/alianza/{aliado}/{departamento}`. Cada segmento
tiene su tipo (`{aliado:int}`), así que una llave mal formada es 404 de ruta,
no un error de conversión.

**Alternativa descartada.** Una llave «empaquetada» en un solo segmento
(`/api/alianza/900123~5`). Es lo que hacían las ramas `feature/v2-*` que se
encontraron en el repositorio: obliga a inventar un separador, a parsearlo a
mano y a documentar un formato que Swagger no puede describir.

## D-v2-9 — Por qué no se integraron las ramas `feature/v2-*` existentes

**Contexto.** El repositorio tenía cuatro ramas de la v2 sin integrar, firmadas
por «OpenAI Assistant».

**Hallazgos.** Un único repositorio genérico por reflexión que arma el SQL a
partir de un mapa de metadatos (contradice los Artículos 8 y 10.1); sin PATCH
(contradice el Artículo 9); sin front (contradice el Artículo 1.1); sin
pruebas; y `CrudControllerV2.Reemplazar` tiene dos `catch (ArgumentException)`
seguidos, que **no compila** (CS0160).

**Decisión.** No se construye sobre ellas ni se borran: quedan en el remoto
para que el equipo decida. Lo que sí se aprovechó fue su inventario de
relaciones, que coincide con el esquema.

## D-v2-10 — Front: «Guardar solo lo que cambié» compara con lo que se cargó

**Decisión.** En las pantallas de la v2, el PATCH envía solo los campos cuyo
valor **cambió** respecto al registro que se abrió para editar.

**Alternativa descartada.** La de la v1: enviar todo lo que no esté vacío. Con
fechas y listas desplegables, «no vacío» casi siempre es «todo», y el botón
dejaba de diferenciarse del de la ficha completa.
