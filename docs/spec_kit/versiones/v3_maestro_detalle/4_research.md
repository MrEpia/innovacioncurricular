# Decisiones — Maestro-detalle

> Cada decisión con su alternativa descartada. La **D-MD-1** enmienda la
> constitución (1.3.0 → 1.4.0), por el procedimiento de su Artículo 11.

## D-MD-1 — El todo-o-nada en un procedimiento almacenado *(enmienda 1.4.0 del Artículo 2)*

**Decisión.** Dos procedimientos en la migración `v3_003_maestro_detalle.sql`:
`sp_registro_calificado_completo_crear` y `sp_programa_completo_crear`. Cada
uno abre una transacción `SERIALIZABLE` con `XACT_ABORT ON`, vuelve a comprobar
que cada padre siga activo, inserta el maestro y después los hijos. Cualquier
error deshace todo.

**Por qué.** La enmienda 1.3.0 permitió procedimientos solo para «comprobar y
escribir» en un paso atómico. Guardar un maestro con su detalle es exactamente
eso, y el riesgo es mayor: son varias tablas. La enmienda 1.4.0 lo agrega a la
lista del Artículo 2.

**Alternativa descartada.** Una `SqlTransaction` abierta desde C# con un
`INSERT` por hijo. Es correcta, pero son N viajes a la base con la transacción
abierta, la regla queda repartida entre la API y la base, y el curso pide
procedimientos.

## D-MD-2 — El detalle viaja como JSON en un parámetro

**Decisión.** Cada lista de hijos va en un parámetro `NVARCHAR(MAX)` con JSON
(`[1, 2]` o `[{"id":7,"componente":"…","semestre":"…"}]`) y el procedimiento la
lee con `OPENJSON`.

**Alternativas descartadas.**
- **Table-valued parameters:** exigen crear un `TYPE` por lista en la base y
  armar un `DataTable` en C#.
- **SQL armado con los valores:** prohibido por el Artículo 2.

El JSON sigue siendo un parámetro y nunca se concatena.

## D-MD-3 — El servicio valida todo antes; el procedimiento revalida

**Decisión.** El servicio comprueba el maestro, cada hijo y los repetidos, y
responde el primer error con un mensaje claro. El procedimiento vuelve a
comprobar los padres dentro de la transacción y lanza `THROW 50400`, que el
repositorio traduce a `ReferenciaInvalidaExcepcion` (400).

**Por qué los dos.** El servicio da el mensaje y se prueba sin base de datos.
El procedimiento cubre la carrera: un padre que se retira entre la validación
y la escritura. Esto cierra, para estas dos operaciones, el «pendiente
conocido» de la D-v3-10.

## D-MD-4 — Controladores aparte, rutas bajo la tabla

**Decisión.** `RegistroCalificadoCompletoController` y
`ProgramaCompletoController`, con `[Route("api/registro_calificado")]` y
`[Route("api/programa")]`, exponen `POST completo` y `GET {llave}/completo`.

**Alternativa descartada.** Meter las rutas en el controlador de cada tabla.
Mezclaría dos recursos en una clase, y el CRUD de la v2, ya entregado, se
tocaría sin necesidad.

## D-MD-5 — El modelo «completo» hereda del maestro

**Decisión.** `RegistroCalificadoCompleto : RegistroCalificado` y
`ProgramaCompleto : Programa`, y lo mismo para las peticiones. Así el JSON es
el maestro de siempre más sus listas, y las validaciones del maestro (los 422)
se reutilizan sin copiarlas.

## D-MD-6 — Sin detalle vacío y sin edición en bloque

**Decisión.** Un maestro-detalle sin hijos es 400. Esta entrega crea y
consulta; editar o retirar sigue por las pantallas de cada tabla (C3 de la
spec).

**Por qué.** Editar en bloque obliga a decidir qué hacer con los hijos que
desaparecen de la lista (¿retirarlos?) y con los que ya tienen sus propios
hijos. Eso es una spec en sí misma, y el enunciado no la pide.
