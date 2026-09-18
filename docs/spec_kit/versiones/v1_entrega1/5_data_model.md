# Modelo de datos — Entrega 1: las siete tablas

> Complementa [v1_aliado/5_data_model.md](../v1_aliado/5_data_model.md) §1
> (la base viene completa, 25 tablas desde el inicio — Artículo 5) y §4
> (invariantes de quién escribe qué), que aplican sin cambios a las siete
> tablas. Aquí solo se documentan las columnas y las semillas específicas
> de cada una.

## 1. Las siete tablas

```mermaid
erDiagram
    aliado {
        INT nit PK
        VARCHAR60 razon_social
        VARCHAR60 nombre_contacto
        VARCHAR70 correo
        VARCHAR45 telefono
        VARCHAR45 ciudad
        BIT activo
    }
    area_conocimiento {
        VARCHAR6 id PK
        VARCHAR60 gran_area
        VARCHAR60 area
        VARCHAR150 disciplina
        BIT activo
    }
    universidad {
        INT id PK
        VARCHAR60 nombre
        VARCHAR45 tipo
        VARCHAR45 ciudad
        BIT activo
    }
    aspecto_normativo {
        INT id PK
        VARCHAR45 tipo
        VARCHAR45 descripcion
        VARCHAR45 fuente
        BIT activo
    }
    practica_estrategia {
        INT id PK
        VARCHAR45 tipo
        VARCHAR45 nombre
        VARCHAR45 descripcion
        BIT activo
    }
    enfoque {
        INT id PK
        VARCHAR45 nombre
        VARCHAR45 descripcion
        BIT activo
    }
    car_innovacion {
        INT id PK
        VARCHAR45 nombre
        VARCHARMAX descripcion "sin limite de longitud (E2)"
        VARCHAR45 tipo
        BIT activo
    }
```

| Tabla | Llave | Columnas (además de `activo`) |
|---|---|---|
| `aliado` | `nit INT` | `razon_social VARCHAR(60)`, `nombre_contacto VARCHAR(60)`, `correo VARCHAR(70)`, `telefono VARCHAR(45)`, `ciudad VARCHAR(45)` |
| `area_conocimiento` | `id VARCHAR(6)` | `gran_area VARCHAR(60)`, `area VARCHAR(60)`, `disciplina VARCHAR(150)` |
| `universidad` | `id INT` | `nombre VARCHAR(60)`, `tipo VARCHAR(45)`, `ciudad VARCHAR(45)` |
| `aspecto_normativo` | `id INT` | `tipo VARCHAR(45)`, `descripcion VARCHAR(45)`, `fuente VARCHAR(45)` |
| `practica_estrategia` | `id INT` | `tipo VARCHAR(45)`, `nombre VARCHAR(45)`, `descripcion VARCHAR(45)` |
| `enfoque` | `id INT` | `nombre VARCHAR(45)`, `descripcion VARCHAR(45)` |
| `car_innovacion` | `id INT` | `nombre VARCHAR(45)`, `descripcion VARCHAR(MAX)`, `tipo VARCHAR(45)` |

Ninguna llave es `IDENTITY`: la asigna quien hace el `POST` (E4), igual
que el `nit` de `aliado`.

## 2. Las semillas: dos tablas llenas, cinco vacías

| Tabla | Filas al arrancar | Fuente |
|---|---|---|
| `area_conocimiento` | **218** | Catálogo del Excel de referencia del módulo |
| `universidad` | **6** | Las seccionales y extensiones de la universidad, cargadas de fábrica |
| `aliado` | 0 | A propósito (C7 de `v1_aliado`) |
| `aspecto_normativo` | 0 | El Excel no trae catálogo para esta tabla |
| `practica_estrategia` | 0 | Ídem |
| `enfoque` | 0 | Ídem |
| `car_innovacion` | 0 | Ídem |

Esto cambia lo que prueba el criterio 2 según la tabla (E3):

- **Las cinco vacías** (`aliado`, `aspecto_normativo`,
  `practica_estrategia`, `enfoque`, `car_innovacion`): el primer `GET`
  responde **204**, igual que en `v1_aliado`.
- **`area_conocimiento` y `universidad`**: el primer `GET` responde
  **200** con `total: 218` y `total: 6` respectivamente. El 204 de
  listado vacío no se puede ejercitar sobre estas dos tablas en el
  ambiente con datos de semilla.

## 3. Reglas de esta versión

Las mismas de [v1_aliado/5_data_model.md §5](../v1_aliado/5_data_model.md):
toda consulta parametrizada, todo listado con `WHERE activo = 1`, y esta
entrega no crea, altera ni borra objetos de la base — el esquema de las
25 tablas viene dado.
