# Modelo de datos — revisión V2

25 tablas funcionales; historial astra_migraciones separado. No se transfirieron datos de producción. Fuente: db/postgres/001_esquema.sql, derivada del esquema SQL Server sin su limpieza. Conserva IDs y tamaños, incluyendo códigos alfanuméricos de área. activo usa smallint con CHECK 0/1. usuario/rol usan identity; fechas SQL date se leen como timestamp en las consultas PostgreSQL para conservar los modelos DateTime/Dapper.

## area_conocimiento

Implementación: `db/postgres/001_esquema.sql:2`.

```sql
CREATE TABLE area_conocimiento (
    id VARCHAR(6) NOT NULL,
    gran_area VARCHAR(60) NOT NULL,
    area VARCHAR(60) NOT NULL,
    disciplina VARCHAR(150) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id)
);
```

## universidad

Implementación: `db/postgres/001_esquema.sql:12`.

```sql
CREATE TABLE universidad (
    id INT NOT NULL,
    nombre VARCHAR(60) NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    ciudad VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id)
);
```

## aspecto_normativo

Implementación: `db/postgres/001_esquema.sql:22`.

```sql
CREATE TABLE aspecto_normativo (
    id INT NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    fuente VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id)
);
```

## practica_estrategia

Implementación: `db/postgres/001_esquema.sql:32`.

```sql
CREATE TABLE practica_estrategia (
    id INT NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id)
);
```

## enfoque

Implementación: `db/postgres/001_esquema.sql:42`.

```sql
CREATE TABLE enfoque (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id)
);
```

## car_innovacion

Implementación: `db/postgres/001_esquema.sql:51`.

```sql
CREATE TABLE car_innovacion (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    descripcion TEXT NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id)
);
```

## aliado

Implementación: `db/postgres/001_esquema.sql:61`.

```sql
CREATE TABLE aliado (
    nit INT NOT NULL,
    razon_social VARCHAR(60) NOT NULL,
    nombre_contacto VARCHAR(60) NOT NULL,
    correo VARCHAR(70) NOT NULL,
    telefono VARCHAR(45) NOT NULL,
    ciudad VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (nit)
);
```

## facultad

Implementación: `db/postgres/001_esquema.sql:73`.

```sql
CREATE TABLE facultad (
    id INT NOT NULL,
    nombre VARCHAR(60) NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    fecha_fun DATE NOT NULL,
    universidad INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id),
    FOREIGN KEY (universidad) REFERENCES universidad(id)
);
```

## programa

Implementación: `db/postgres/001_esquema.sql:85`.

```sql
CREATE TABLE programa (
    id INT NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    nivel VARCHAR(45) NOT NULL,
    fecha_creacion VARCHAR(45) NOT NULL,
    fecha_cierre VARCHAR(45),
    numero_cohortes VARCHAR(45) NOT NULL,
    cant_graduados VARCHAR(45) NOT NULL,
    fecha_actualizacion VARCHAR(45) NOT NULL,
    ciudad VARCHAR(45) NOT NULL,
    facultad INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id),
    FOREIGN KEY (facultad) REFERENCES facultad(id)
);
```

## acreditacion

Implementación: `db/postgres/001_esquema.sql:103`.

```sql
CREATE TABLE acreditacion (
    resolucion INT NOT NULL,
    tipo VARCHAR(45) NOT NULL,
    calificacion VARCHAR(45) NOT NULL,
    fecha_inicio VARCHAR(45) NOT NULL,
    fecha_fin VARCHAR(45) NOT NULL,
    programa INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (resolucion),
    FOREIGN KEY (programa) REFERENCES programa(id)
);
```

## registro_calificado

Implementación: `db/postgres/001_esquema.sql:116`.

```sql
CREATE TABLE registro_calificado (
    codigo INT NOT NULL,
    cant_creditos VARCHAR(45) NOT NULL,
    hora_acom VARCHAR(45) NOT NULL,
    hora_ind VARCHAR(45) NOT NULL,
    metodologia VARCHAR(45) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    duracion_anios VARCHAR(45) NOT NULL,
    duracion_semestres VARCHAR(45) NOT NULL,
    tipo_titulacion VARCHAR(45) NOT NULL,
    programa INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (codigo),
    FOREIGN KEY (programa) REFERENCES programa(id)
);
```

## activ_academica

Implementación: `db/postgres/001_esquema.sql:134`.

```sql
CREATE TABLE activ_academica (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    num_creditos INT NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    area_formacion VARCHAR(45) NOT NULL,
    h_acom INT NOT NULL,
    h_indep INT NOT NULL,
    idioma VARCHAR(45) NOT NULL,
    espejo SMALLINT NOT NULL,
    entidad_espejo VARCHAR(45) NOT NULL,
    pais_espejo VARCHAR(45) NOT NULL,
    disenio INT,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id),
    FOREIGN KEY (disenio) REFERENCES programa(id)
);
```

## pasantia

Implementación: `db/postgres/001_esquema.sql:153`.

```sql
CREATE TABLE pasantia (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    pais VARCHAR(45) NOT NULL,
    empresa VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    programa INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id),
    FOREIGN KEY (programa) REFERENCES programa(id)
);
```

## premio

Implementación: `db/postgres/001_esquema.sql:166`.

```sql
CREATE TABLE premio (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    descripcion VARCHAR(45) NOT NULL,
    fecha DATE NOT NULL,
    entidad_otorgante VARCHAR(45) NOT NULL,
    pais VARCHAR(45) NOT NULL,
    programa INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (id),
    FOREIGN KEY (programa) REFERENCES programa(id)
);
```

## programa_ac

Implementación: `db/postgres/001_esquema.sql:180`.

```sql
CREATE TABLE programa_ac (
    programa INT NOT NULL,
    area_conocimiento VARCHAR(6) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (programa, area_conocimiento),
    FOREIGN KEY (programa) REFERENCES programa(id),
    FOREIGN KEY (area_conocimiento) REFERENCES area_conocimiento(id)
);
```

## programa_pe

Implementación: `db/postgres/001_esquema.sql:190`.

```sql
CREATE TABLE programa_pe (
    programa INT NOT NULL,
    practica_estrategia INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (programa, practica_estrategia),
    FOREIGN KEY (programa) REFERENCES programa(id),
    FOREIGN KEY (practica_estrategia) REFERENCES practica_estrategia(id)
);
```

## programa_ci

Implementación: `db/postgres/001_esquema.sql:200`.

```sql
CREATE TABLE programa_ci (
    programa INT NOT NULL,
    car_innovacion INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (programa, car_innovacion),
    FOREIGN KEY (programa) REFERENCES programa(id),
    FOREIGN KEY (car_innovacion) REFERENCES car_innovacion(id)
);
```

## an_programa

Implementación: `db/postgres/001_esquema.sql:210`.

```sql
CREATE TABLE an_programa (
    aspecto_normativo INT NOT NULL,
    programa INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (aspecto_normativo, programa),
    FOREIGN KEY (aspecto_normativo) REFERENCES aspecto_normativo(id),
    FOREIGN KEY (programa) REFERENCES programa(id)
);
```

## enfoque_rc

Implementación: `db/postgres/001_esquema.sql:220`.

```sql
CREATE TABLE enfoque_rc (
    enfoque INT NOT NULL,
    registro_calificado INT NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (enfoque, registro_calificado),
    FOREIGN KEY (enfoque) REFERENCES enfoque(id),
    FOREIGN KEY (registro_calificado) REFERENCES registro_calificado(codigo)
);
```

## aa_rc

Implementación: `db/postgres/001_esquema.sql:230`.

```sql
CREATE TABLE aa_rc (
    activ_academicas_idcurso INT NOT NULL,
    registro_calificado_codigo INT NOT NULL,
    componente VARCHAR(45) NOT NULL,
    semestre VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (activ_academicas_idcurso, registro_calificado_codigo),
    FOREIGN KEY (activ_academicas_idcurso) REFERENCES activ_academica(id),
    FOREIGN KEY (registro_calificado_codigo) REFERENCES registro_calificado(codigo)
);
```

## docente_departamento

Implementación: `db/postgres/001_esquema.sql:242`.

```sql
CREATE TABLE docente_departamento (
    docente INT NOT NULL,
    departamento INT NOT NULL,
    dedicacion VARCHAR(15) NOT NULL,
    modalidad VARCHAR(45) NOT NULL,
    fecha_ingreso DATE NOT NULL,
    fecha_salida DATE,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (docente, departamento),
    FOREIGN KEY (departamento) REFERENCES programa(id)
);
```

## alianza

Implementación: `db/postgres/001_esquema.sql:255`.

```sql
CREATE TABLE alianza (
    aliado INT NOT NULL,
    departamento INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    docente INT,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    PRIMARY KEY (aliado, departamento),
    FOREIGN KEY (aliado) REFERENCES aliado(nit),
    FOREIGN KEY (departamento) REFERENCES programa(id)
);
```

## rol

Implementación: `db/postgres/001_esquema.sql:268`.

```sql
CREATE TABLE rol (
    id INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion TEXT,
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    fecha_creacion TIMESTAMP WITHOUT TIME ZONE DEFAULT LOCALTIMESTAMP
);
```

## usuario

Implementación: `db/postgres/001_esquema.sql:277`.

```sql
CREATE TABLE usuario (
    id INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    nombre_completo VARCHAR(200),
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    fecha_creacion TIMESTAMP WITHOUT TIME ZONE DEFAULT LOCALTIMESTAMP,
    fecha_actualizacion TIMESTAMP WITHOUT TIME ZONE DEFAULT LOCALTIMESTAMP
);
```

## rol_usuario

Implementación: `db/postgres/001_esquema.sql:289`.

```sql
CREATE TABLE rol_usuario (
    activo SMALLINT NOT NULL DEFAULT 1 CHECK (activo IN (0, 1)),
    usuario_id INT NOT NULL,
    rol_id INT NOT NULL,
    PRIMARY KEY (usuario_id, rol_id),
    FOREIGN KEY (usuario_id) REFERENCES usuario(id) ON DELETE CASCADE,
    FOREIGN KEY (rol_id) REFERENCES rol(id) ON DELETE CASCADE
);
```

## Operaciones y restricciones

002_operaciones_atomicas.sql contiene las funciones de dependientes, retiro y protección del último administrador; bloqueos de tabla conservan la comprobación y actualización en una operación. 003_maestro_detalle.sql guarda programa o registro y detalles en la misma sentencia/transacción: una excepción revierte todo. No se cambia el contrato JSON ni se borra físicamente desde los endpoints. Las validaciones de referencias activas de CRUD simple siguen en los servicios; no se afirma una garantía nueva frente a toda carrera concurrente del CRUD simple. El historial valida checksums y bloquea inicializar bases ajenas con tablas existentes.

## Referencia de implementación

Código auditado: https://github.com/MrEpia/innovacioncurricular/tree/1e91a6eea54e690a3faa9637e2c943ef2d2fc5e4 . Las declaraciones específicas y sus líneas se vinculan en 6_contracts.md y MAPA_COMPLETO_PROYECTO_V2.txt; los resultados distinguen verificación local y publicación pendiente.
