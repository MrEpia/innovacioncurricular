# Historias de usuario — Versión 2

> Continúan la numeración de
> [historias_usuario_innovacion_curricular.md](../../../../ProyectosDeAula/docs/historias_usuario_innovacion_curricular.md)
> (la v1 llegó hasta la 10). El usuario es la **coordinación curricular** de la
> universidad.

## Historia 11 — Registrar la estructura académica

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Medio | 5 | Académico (Calvo) |

**Yo**, como coordinación curricular, **quiero** registrar las facultades de
cada sede y los programas de cada facultad **para** que el resto de la
información del currículo tenga a qué colgarse.

Observaciones: RF8 (`facultad`, `programa`) · C1 (no se siembran).

**Criterios de aceptación**
1. Al crear una facultad, la sede se elige de una lista con las sedes activas.
2. Una facultad apuntando a una sede inexistente o retirada no se guarda (400).
3. El tipo de facultad solo puede ser Facultad, Departamento o Instituto.
4. Un programa con fecha de cierre vacía se guarda: el campo es opcional.

## Historia 12 — Registrar acreditaciones y registros calificados

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Bajo | 3 | Académico (Calvo) |

**Yo**, como coordinación curricular, **quiero** registrar las acreditaciones y
los registros calificados de cada programa **para** saber qué programas están
autorizados y acreditados, y hasta cuándo.

**Criterios de aceptación**
1. La resolución y el código son la llave: repetirlos responde 409.
2. Un registro calificado con fecha de fin anterior a la de inicio no se guarda.
3. Ambos se asocian a un programa activo elegido de una lista.

## Historia 13 — No dejar información huérfana

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Alto | 5 | Académico (Calvo) |

**Yo**, como coordinación curricular, **quiero** que el sistema no me deje
retirar un programa que todavía tiene acreditaciones, premios o alianzas
activas **para** no perder de vista esa información sin darme cuenta.

Observaciones: RF6, RF9 · D-v2-6 (no hay cascada).

**Criterios de aceptación**
1. Retirar un padre con hijos activos responde 409 y dice cuáles y cuántos.
2. Retirados los hijos, el mismo retiro funciona.
3. Aplica también a las tablas de la v1 (una sede con facultades, un aliado con alianzas).

## Historia 14 — Registrar las actividades académicas

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Medio | 5 | Actividades (Epia) |

**Yo**, como coordinación curricular, **quiero** registrar cada curso con sus
créditos, sus horas y si es espejo con otra institución **para** armar el plan
de estudios de cada registro calificado.

**Criterios de aceptación**
1. Créditos y horas no pueden ser negativos.
2. «¿Es espejo?» solo admite sí o no.
3. El programa de diseño es opcional; si se da, debe estar activo.

## Historia 15 — Registrar pasantías y premios

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Media | Bajo | 3 | Actividades (Epia) |

**Yo**, como coordinación curricular, **quiero** registrar las pasantías
disponibles y los premios recibidos por cada programa **para** mostrarlos como
evidencia de calidad.

**Criterios de aceptación**
1. Ambos exigen un programa activo.
2. Al retirarlos, dejan de aparecer pero la fila queda en la base.

## Historia 16 — Vincular docentes y aliados a los programas

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Medio | 5 | Actividades (Epia) |

**Yo**, como coordinación curricular, **quiero** registrar con qué dedicación
está vinculado cada docente a cada programa y qué aliados tienen alianza con
cada programa **para** sustentar los recursos del programa ante el Ministerio.

**Criterios de aceptación**
1. La dedicación solo puede ser Tiempo Completo, Medio Tiempo o Cátedra.
2. La fecha de salida o de fin, si se da, no puede ser anterior a la de inicio.
3. Docente + programa y aliado + programa no se pueden repetir (409).

## Historia 17 — Relacionar programas con catálogos

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Bajo | 5 | Puentes (Brunal) |

**Yo**, como coordinación curricular, **quiero** indicar qué áreas de
conocimiento, prácticas, características de innovación y aspectos normativos
tiene cada programa **para** poder consultarlo por cualquiera de esos lados.

**Criterios de aceptación**
1. Cada relación se crea eligiendo los dos extremos de listas con los activos.
2. Una relación repetida responde 409.
3. Una relación no se edita: se retira y se crea otra (C5).

## Historia 18 — Relacionar los registros calificados con enfoques y actividades

| Usuario | Prioridad | Riesgo | Puntos | Bloque |
|---|---|---|---|---|
| Coordinación curricular | Alta | Medio | 3 | `enfoque_rc`: Puentes (Brunal) · `aa_rc`: Actividades (Epia) |

**Yo**, como coordinación curricular, **quiero** indicar qué enfoques declara
cada registro calificado y qué actividades incluye, en qué componente y
semestre, **para** reconstruir el plan de estudios aprobado.

**Criterios de aceptación**
1. Una actividad incluida en un registro tiene componente y semestre
   obligatorios, y se pueden editar.
2. Un registro calificado con actividades o enfoques activos no se puede retirar.

## Resumen de la iteración

| Bloque | Historias | Puntos |
|---|---|---|
| Académico (Calvo) | 11, 12, 13 | 13 |
| Actividades (Epia) | 14, 15, 16, 18 (`aa_rc`) | 15 |
| Puentes (Brunal) | 17, 18 (`enfoque_rc`) | 7 |
