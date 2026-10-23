/* =========================================================================
   v3_003 — Maestro-detalle (enmienda 1.4.0 de la constitución)
   =========================================================================
   Dos procedimientos que guardan un MAESTRO con todo su DETALLE en una sola
   transacción: si un hijo falla, no queda nada, ni el maestro ni los hijos
   anteriores.

     sp_registro_calificado_completo_crear   registro_calificado + enfoque_rc + aa_rc
     sp_programa_completo_crear              programa + programa_ac + programa_pe + programa_ci

   El detalle llega como JSON en un parámetro NVARCHAR(MAX) y se lee con
   OPENJSON: sigue siendo un parámetro, nunca SQL concatenado (Artículo 2).

   Dentro de la transacción se vuelve a comprobar que cada padre siga activo:
   el servicio ya lo validó, pero entre esa validación y la escritura alguien
   pudo retirarlo. Si pasó, THROW 50400 (el repositorio → 400) y XACT_ABORT
   deshace todo. Una llave repetida es el error 2627 del motor (→ 409).

   Idempotente: CREATE OR ALTER.
   ========================================================================= */
GO
CREATE OR ALTER PROCEDURE dbo.sp_registro_calificado_completo_crear
    @Codigo INT,
    @CantCreditos VARCHAR(45),
    @HoraAcom VARCHAR(45),
    @HoraInd VARCHAR(45),
    @Metodologia VARCHAR(45),
    @FechaInicio DATE,
    @FechaFin DATE,
    @DuracionAnios VARCHAR(45),
    @DuracionSemestres VARCHAR(45),
    @TipoTitulacion VARCHAR(45),
    @Programa INT,
    @Enfoques NVARCHAR(MAX),      -- [1, 2]
    @Actividades NVARCHAR(MAX)    -- [{"id": 7, "componente": "...", "semestre": "..."}]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;            -- cualquier error deshace TODA la transacción
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    DECLARE @hijosEnfoque TABLE (enfoque INT NOT NULL);
    INSERT INTO @hijosEnfoque (enfoque)
    SELECT CAST(value AS INT) FROM OPENJSON(ISNULL(@Enfoques, N'[]'));

    DECLARE @hijosActividad TABLE (id INT NOT NULL, componente VARCHAR(45) NOT NULL, semestre VARCHAR(45) NOT NULL);
    INSERT INTO @hijosActividad (id, componente, semestre)
    SELECT id, componente, semestre
    FROM OPENJSON(ISNULL(@Actividades, N'[]'))
         WITH (id INT '$.id', componente VARCHAR(45) '$.componente', semestre VARCHAR(45) '$.semestre');

    DECLARE @mensaje NVARCHAR(2048);
    DECLARE @falta INT;

    BEGIN TRANSACTION;

    -- Los padres, otra vez y dentro de la transacción.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @Programa AND activo = 1)
    BEGIN
        SET @mensaje = CONCAT(N'El programa ', @Programa, N' no existe o está retirado. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    SELECT TOP 1 @falta = x.enfoque FROM @hijosEnfoque x
    WHERE NOT EXISTS (SELECT 1 FROM enfoque e WHERE e.id = x.enfoque AND e.activo = 1);
    IF @falta IS NOT NULL
    BEGIN
        SET @mensaje = CONCAT(N'El enfoque ', @falta, N' no existe o está retirado. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    SELECT TOP 1 @falta = x.id FROM @hijosActividad x
    WHERE NOT EXISTS (SELECT 1 FROM activ_academica a WHERE a.id = x.id AND a.activo = 1);
    IF @falta IS NOT NULL
    BEGIN
        SET @mensaje = CONCAT(N'La actividad académica ', @falta, N' no existe o está retirada. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    -- El maestro...
    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia, fecha_inicio,
                                     fecha_fin, duracion_anios, duracion_semestres, tipo_titulacion, programa, activo)
    VALUES (@Codigo, @CantCreditos, @HoraAcom, @HoraInd, @Metodologia, @FechaInicio,
            @FechaFin, @DuracionAnios, @DuracionSemestres, @TipoTitulacion, @Programa, 1);

    -- ...y su detalle.
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo)
    SELECT enfoque, @Codigo, 1 FROM @hijosEnfoque;

    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
    SELECT id, @Codigo, componente, semestre, 1 FROM @hijosActividad;

    COMMIT TRANSACTION;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_programa_completo_crear
    @Id INT,
    @Nombre VARCHAR(150),
    @Tipo VARCHAR(45),
    @Nivel VARCHAR(45),
    @FechaCreacion VARCHAR(45),
    @FechaCierre VARCHAR(45),
    @NumeroCohortes VARCHAR(45),
    @CantGraduados VARCHAR(45),
    @FechaActualizacion VARCHAR(45),
    @Ciudad VARCHAR(45),
    @Facultad INT,
    @Areas NVARCHAR(MAX),            -- ["1A01", "1A02"]
    @Practicas NVARCHAR(MAX),        -- [1, 2]
    @Caracteristicas NVARCHAR(MAX)   -- [1, 2]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    DECLARE @hijosArea TABLE (area VARCHAR(50) NOT NULL);   -- más ancho que la columna: un id largo no se trunca en silencio, falla
    INSERT INTO @hijosArea (area) SELECT CAST(value AS VARCHAR(50)) FROM OPENJSON(ISNULL(@Areas, N'[]'));

    DECLARE @hijosPractica TABLE (practica INT NOT NULL);
    INSERT INTO @hijosPractica (practica) SELECT CAST(value AS INT) FROM OPENJSON(ISNULL(@Practicas, N'[]'));

    DECLARE @hijosCaracteristica TABLE (caracteristica INT NOT NULL);
    INSERT INTO @hijosCaracteristica (caracteristica) SELECT CAST(value AS INT) FROM OPENJSON(ISNULL(@Caracteristicas, N'[]'));

    DECLARE @mensaje NVARCHAR(2048);
    DECLARE @faltaTexto VARCHAR(50);
    DECLARE @falta INT;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM facultad WHERE id = @Facultad AND activo = 1)
    BEGIN
        SET @mensaje = CONCAT(N'La facultad ', @Facultad, N' no existe o está retirada. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    SELECT TOP 1 @faltaTexto = x.area FROM @hijosArea x
    WHERE NOT EXISTS (SELECT 1 FROM area_conocimiento a WHERE a.id = x.area AND a.activo = 1);
    IF @faltaTexto IS NOT NULL
    BEGIN
        SET @mensaje = CONCAT(N'El área de conocimiento ', @faltaTexto, N' no existe o está retirada. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    SELECT TOP 1 @falta = x.practica FROM @hijosPractica x
    WHERE NOT EXISTS (SELECT 1 FROM practica_estrategia p WHERE p.id = x.practica AND p.activo = 1);
    IF @falta IS NOT NULL
    BEGIN
        SET @mensaje = CONCAT(N'La práctica o estrategia ', @falta, N' no existe o está retirada. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    SET @falta = NULL;
    SELECT TOP 1 @falta = x.caracteristica FROM @hijosCaracteristica x
    WHERE NOT EXISTS (SELECT 1 FROM car_innovacion c WHERE c.id = x.caracteristica AND c.activo = 1);
    IF @falta IS NOT NULL
    BEGIN
        SET @mensaje = CONCAT(N'La característica de innovación ', @falta, N' no existe o está retirada. No se guardó nada.');
        THROW 50400, @mensaje, 1;
    END

    INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES (@Id, @Nombre, @Tipo, @Nivel, @FechaCreacion, @FechaCierre, @NumeroCohortes,
            @CantGraduados, @FechaActualizacion, @Ciudad, @Facultad, 1);

    INSERT INTO programa_ac (programa, area_conocimiento, activo) SELECT @Id, area, 1 FROM @hijosArea;
    INSERT INTO programa_pe (programa, practica_estrategia, activo) SELECT @Id, practica, 1 FROM @hijosPractica;
    INSERT INTO programa_ci (programa, car_innovacion, activo) SELECT @Id, caracteristica, 1 FROM @hijosCaracteristica;

    COMMIT TRANSACTION;
END
GO
