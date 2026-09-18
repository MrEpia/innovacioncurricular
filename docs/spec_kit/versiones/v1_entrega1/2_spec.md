# Especificación — Entrega 1: las 7 tablas sin clave foránea

> **Versión 1, completa** ([mapa](../0_mapa_versiones.md)) · Extiende el
> patrón que [v1_aliado](../v1_aliado/2_spec.md) dejó montado y verificado
> sobre una sola tabla, y lo aplica a las **siete** tablas sin clave
> foránea que exige la Entrega 1. Ante conflicto con este documento, manda
> la [constitución](../../1_constitution.md); ante conflicto con
> `v1_aliado`, manda `v1_aliado` para todo lo que no sea específico de una
> tabla (arquitectura, capas, contenedores, convenciones HTTP).

## 1. Propósito de esta entrega

`v1_aliado` construyó y verificó el patrón —controlador, servicio,
repositorio, interfaces, peticiones por verbo, borrado lógico, prueba de
capas sin base de datos— sobre `aliado`, la tabla con más campos de las
siete. Esta entrega **repite ese mismo patrón, sin variarlo**, sobre las
seis tablas restantes:

`area_conocimiento` · `universidad` · `aspecto_normativo` ·
`practica_estrategia` · `enfoque` · `car_innovacion`

Ninguna de las seis introduce una idea nueva de arquitectura: son la misma
rebanada vertical con otro nombre de tabla y otras columnas. Por eso este
documento no repite la justificación de cada decisión —está en
`v1_aliado`— y se concentra en lo que **sí** cambia: los campos, sus
validaciones y las diferencias puntuales entre tablas.

## 2. Alcance

**Incluye**

- El CRUD completo (listar, obtener por llave, crear, reemplazar,
  actualizar parcialmente, eliminar) de las **siete** tablas sin FK:
  `aliado`, `area_conocimiento`, `universidad`, `aspecto_normativo`,
  `practica_estrategia`, `enfoque`, `car_innovacion`.
- **Borrado lógico** en las siete: `DELETE` marca `activo = 0`, los
  listados filtran `WHERE activo = 1` (Artículo 6).
- Documentación interactiva de las siete en `/swagger`.
- Front en Blazor Server con una pantalla propia por tabla, hablando con
  la API solo por HTTP (Artículo 1.1).

**NO incluye** — no se anticipa nada de esto (Artículo 1)

- Las 15 tablas con clave foránea (`facultad`, `programa`, `alianza`,
  etc.): eso es la v2.
- JWT, sesiones, roles ni las tablas `usuario`/`rol`/`rol_usuario`: eso es
  la v3.
- Consultas multitabla ni dashboard: eso es la v4.
- Reactivar un registro inactivo, o validar el formato de un campo de
  texto más allá de que esté presente: nadie lo ha pedido en ninguna de
  las siete tablas.

## 3. Requisitos funcionales (el mismo patrón, siete veces)

Cada tabla expone los mismos siete endpoints que `aliado` (RF1–RF7 de
[v1_aliado/2_spec.md](../v1_aliado/2_spec.md)), cambiando solo el nombre
de la ruta (`/api/<tabla>`) y los campos del cuerpo:

| Tabla | Llave (ruta) | Campos del cuerpo (POST/PUT; PATCH = todos opcionales) |
|---|---|---|
| `aliado` | `nit` (int) | `razonSocial`, `nombreContacto`, `correo`, `telefono`, `ciudad` — *(ya construida en v1_aliado)* |
| `area_conocimiento` | `id` (string, máx. 6) | `granArea` (máx. 60), `area` (máx. 60), `disciplina` (máx. 150) |
| `universidad` | `id` (int, lo asigna quien crea) | `nombre` (máx. 60), `tipo` (máx. 45), `ciudad` (máx. 45) |
| `aspecto_normativo` | `id` (int) | `tipo` (máx. 45), `descripcion` (máx. 45), `fuente` (máx. 45) |
| `practica_estrategia` | `id` (int) | `tipo` (máx. 45), `nombre` (máx. 45), `descripcion` (máx. 45) |
| `enfoque` | `id` (int) | `nombre` (máx. 45), `descripcion` (máx. 45) |
| `car_innovacion` | `id` (int) | `nombre` (máx. 45), `descripcion` (**sin límite** — `VARCHAR(MAX)`), `tipo` (máx. 45) |

Todos los campos listados son **obligatorios en POST y en PUT**; en PATCH
todos son opcionales y solo se escriben los enviados — exactamente la
misma pareja `Reemplazo`/`Actualizar` que `AliadoReemplazo` y
`AliadoActualizar`.

A diferencia de `aliado` (cuyo `nit` es el dato de negocio del propio
registro), en las seis tablas restantes el `id` **no lo genera la base**:
lo asigna quien hace el `POST`, igual que en `aliado` el `nit`. Ninguna
usa `IDENTITY`.

### RF8 — Diagnóstico único

`GET /` sigue siendo uno solo para toda la API (no uno por tabla) y sigue
respondiendo `"version":"v1"`: la entrega es una sola versión con siete
tablas, no siete versiones.

## 4. Requisitos no funcionales

Los mismos de [v1_aliado §4](../v1_aliado/2_spec.md): un solo comando
(`docker compose up -d --build`), tres capas con interfaces, SQL a mano y
parametrizado, todo en español, `/swagger`. No hay nada nuevo que agregar
por tener seis tablas más: es el mismo `api_innovacion`, con más clases.

## 5. Criterios de aceptación

Los 7 criterios de `v1_aliado` se cumplen **para cada una de las siete
tablas**, con esta particularidad por el estado inicial de los datos:

| Tabla | Filas al arrancar | Qué prueba el criterio 2 (listado vacío → 204) |
|---|---|---|
| `aliado` | 0 | Directo: `GET /api/aliado` → 204 |
| `area_conocimiento` | **218** (semillas del Excel) | El 204 **no** se prueba aquí: la tabla nunca está vacía en un ambiente con datos. Se prueba en un entorno de pruebas aparte o se documenta como no aplicable |
| `universidad` | **6** (semillas) | Igual que `area_conocimiento`: no aplica en el ambiente con datos |
| `aspecto_normativo` | 0 | Directo |
| `practica_estrategia` | 0 | Directo |
| `enfoque` | 0 | Directo |
| `car_innovacion` | 0 | Directo |

Para las cinco tablas que arrancan vacías, el smoke test recorre el mismo
ciclo que `aliado` (204 → crear → 200 con `total 1` → `PUT` → `PATCH` →
`DELETE` → 404 en el segundo `DELETE`), documentado con un ejemplo
concreto por tabla en [7 — ver `6_contracts.md`](6_contracts.md).

Para `area_conocimiento` y `universidad` el ciclo se prueba igual, salvo
el criterio 2: se verifica en cambio que el listado responde **200 con
`total` igual al número de semillas** (218 y 6 respectivamente) y que el
registro de prueba creado aparece **además** de esas filas.

### Criterios de aceptación de la pantalla

Los mismos P1–P7 de `v1_aliado`, uno por cada una de las **siete**
pantallas (`/aliados`, `/areas-conocimiento`, `/universidades`,
`/aspectos-normativos`, `/practicas-estrategias`, `/enfoques`,
`/caracteristicas-innovacion`).

## 6. Clarificaciones

| # | La pregunta | La respuesta, con su razón | Dónde quedó |
|---|---|---|---|
| E1 | Si el patrón ya está probado en `aliado`, ¿hace falta repetir la justificación de cada decisión (Dapper, capas, prueba sin base de datos…) en cada tabla? | **No.** Repetirla seis veces sería anticipar nada nuevo y solo agregaría volumen; `v1_aliado` es la referencia y este documento remite a ella | §1 |
| E2 | `car_innovacion.descripcion` es la única columna sin límite de longitud (`VARCHAR(MAX)`) de las siete tablas. ¿Se le pone un `[MaxLength]` para ser consistente con las demás? | **No.** El límite lo decide el modelo de datos, no la conveniencia de que todas las peticiones se vean iguales; el código ya refleja el esquema tal como está | `db/innovacion_curricular.sql` · `Peticiones/CarInnovacionCrear.cs` |
| E3 | `area_conocimiento` y `universidad` ya traen semillas. ¿Contradice eso el criterio 2 de `v1_aliado` ("el sistema arranca vacío")? | **No contradice**: ese criterio es de `aliado` específicamente. Cada tabla tiene su propio estado inicial, documentado en [5_data_model.md](5_data_model.md) §3, y el criterio de aceptación de cada una se ajusta a ese estado | §5 de este documento |
| E4 | Los `id` de las seis tablas nuevas son enteros o strings **asignados por quien crea el registro**, no autoincrementales. ¿Es un problema de diseño? | **No para esta versión.** El script no declara `IDENTITY` en ninguna de las 22 tablas del módulo, y la v1 no lo cambia (Artículo 5: el esquema viene dado) | `db/innovacion_curricular.sql` |
| E5 | ¿Cada tabla necesita su propia carpeta de spec kit (`v1_universidad`, `v1_enfoque`…) o basta con este documento único? | **Un documento único**, porque el mapa de versiones ([0_mapa_versiones.md](../0_mapa_versiones.md)) trata las siete tablas como **una sola versión** (v1), no como versiones independientes. `v1_aliado` quedó como el ejemplo de referencia que ya traía el proyecto de GitHub; este spec kit documenta el resto de esa misma v1 | Encabezado de este documento |

## 7. Definición de TERMINADA

Esta entrega está terminada —y solo entonces se escribe la spec de la
v2— cuando:

1. Los criterios de aceptación de la sección 5 pasan **para las siete
   tablas**, verificados con el smoke test de cada una (ver
   [6_contracts.md](6_contracts.md) para los ejemplos concretos).
2. La lista de [9_checklist.md](9_checklist.md) está en verde y firmada.
3. No queda ningún `[NECESITA ACLARACIÓN: …]` en este documento.
4. Se hace commit y **tag `v1`** — uno solo, para las siete tablas: la
   entrega es una versión, no siete.
