# Lista de chequeo de requisitos — Entrega 1 (7 tablas)

> La misma compuerta 3 de [v1_aliado/9_checklist.md](../v1_aliado/9_checklist.md),
> aplicada a este documento. Se marca `[x]` solo cuando se cumple **hoy,
> en el documento** — no "cuando lo arregle". Las casillas las marca una
> persona.

## A. Claridad

- [ ] Ningún requisito de [2_spec.md](2_spec.md) usa palabras sin definir.
- [ ] No queda ningún `[NECESITA ACLARACIÓN: …]` sin resolver en la
      sección 6.
- [ ] Cada fila de la tabla de campos (§3) corresponde exactamente a las
      clases `Peticiones/*Crear.cs` del código — sin campos de más ni de
      menos.

## B. Medible

- [ ] Cada tabla tiene su ejemplo concreto en
      [6_contracts.md](6_contracts.md), con el mismo id/nit usado en todo
      el ciclo POST→PUT→PATCH→DELETE.
- [ ] Las dos excepciones al criterio 2 (`area_conocimiento`,
      `universidad` no responden 204 porque vienen con semillas) están
      escritas explícitamente, no implícitas.
- [ ] El contraste `PUT` 422 vs `PATCH` 200 está ejemplificado para **cada
      una** de las seis tablas nuevas, no solo para `aliado`.

## C. Completitud

- [ ] Las siete tablas de la Entrega 1 aparecen en §3 de
      [2_spec.md](2_spec.md), en [5_data_model.md](5_data_model.md) y en
      [6_contracts.md](6_contracts.md).
- [ ] [5_data_model.md](5_data_model.md) documenta las semillas de las
      siete tablas, no solo las que arrancan vacías.
- [ ] La particularidad de `car_innovacion.descripcion`
      (`VARCHAR(MAX)`, sin `[MaxLength]`) queda anotada como decisión
      (E2), no como un olvido.
- [ ] [2_spec.md §2](2_spec.md) tiene su **NO incluye** explícito: v2, v3
      y v4 no se anticipan.

## D. Coherencia

- [ ] Todo campo de la tabla de §3 tiene su tarea correspondiente en
      [8_tasks.md](8_tasks.md) (Fases 1 a 3).
- [ ] Los tipos de [5_data_model.md](5_data_model.md) coinciden con
      `db/innovacion_curricular.sql` para las seis tablas nuevas.
- [ ] Los `[MaxLength]` de cada `Peticiones/*Crear.cs` coinciden con el
      `VARCHAR(N)` de su columna en el script.
- [ ] Este documento no repite lo que ya está en `v1_aliado` (arquitectura,
      catálogo de códigos, convenciones de JSON): remite a él.

## E. Alcance

- [ ] Ningún documento de esta carpeta pide que el código nombre una
      tabla con clave foránea.
- [ ] La entrega se trata como **una sola versión** (`v1`) con siete
      tablas, no como siete versiones independientes (E5).
- [ ] Ningún documento anticipa JWT, roles ni consultas multitabla.

---

## Resultado

| | |
|---|---|
| **Revisada por** | *(nombre de quien firma)* |
| **Fecha** | |
| **Casillas en rojo** | |
| **Veredicto** | ⬜ En verde: puede entregarse · ⬜ En rojo: vuelve a la spec |
