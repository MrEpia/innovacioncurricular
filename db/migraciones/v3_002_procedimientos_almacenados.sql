/* =========================================================================
   v3_002 — Procedimientos almacenados (enmienda 1.3.0 de la constitución)
   =========================================================================
   Solo para las operaciones de VARIOS PASOS que deben ser ATÓMICAS:
   comprobar algo y escribir según la respuesta. Entre la comprobación y la
   escritura no puede colarse otra petición: cada procedimiento lo hace en una
   transacción SERIALIZABLE.

     sp_dependientes_<tabla>   (11)  qué hijos activos tiene un padre
     sp_retirar_<tabla>        (11)  retira el padre SOLO si no tiene hijos activos
     sp_rol_retirar                  retira un rol SOLO si ningún usuario activo lo tiene
     sp_rol_usuario_asignar          crea, reactiva o rechaza la asignación
     sp_rol_usuario_quitar           quita un rol sin dejar el sistema sin administrador
     sp_usuario_retirar              retira un usuario sin dejar el sistema sin administrador
     sp_contar_administradores_activos

   Errores: THROW 50409 = tiene dependientes            (el repositorio → 409)
            THROW 50410 = dejaría el sistema sin administrador (→ 409)

   El CRUD de una sola tabla y una sola sentencia sigue con SQL parametrizado
   en los repositorios (Artículo 2). Idempotente: CREATE OR ALTER.
   ========================================================================= */
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_universidad @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'facultad' AS Tabla, COUNT(*) AS Cantidad FROM facultad WHERE universidad = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_universidad @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_universidad @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE universidad SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_facultad @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'programa' AS Tabla, COUNT(*) AS Cantidad FROM programa WHERE facultad = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_facultad @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_facultad @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE facultad SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_programa @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'acreditacion' AS Tabla, COUNT(*) AS Cantidad FROM acreditacion WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'registro_calificado', COUNT(*) FROM registro_calificado WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'activ_academica', COUNT(*) FROM activ_academica WHERE disenio = @Id AND activo = 1
    UNION ALL SELECT 'pasantia', COUNT(*) FROM pasantia WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'premio', COUNT(*) FROM premio WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'programa_ac', COUNT(*) FROM programa_ac WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'programa_pe', COUNT(*) FROM programa_pe WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'programa_ci', COUNT(*) FROM programa_ci WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'an_programa', COUNT(*) FROM an_programa WHERE programa = @Id AND activo = 1
    UNION ALL SELECT 'docente_departamento', COUNT(*) FROM docente_departamento WHERE departamento = @Id AND activo = 1
    UNION ALL SELECT 'alianza', COUNT(*) FROM alianza WHERE departamento = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_programa @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_programa @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE programa SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_registro_calificado @Codigo INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'enfoque_rc' AS Tabla, COUNT(*) AS Cantidad FROM enfoque_rc WHERE registro_calificado = @Codigo AND activo = 1
    UNION ALL SELECT 'aa_rc', COUNT(*) FROM aa_rc WHERE registro_calificado_codigo = @Codigo AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_registro_calificado @Codigo INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_registro_calificado @Codigo;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE registro_calificado SET activo = 0 WHERE codigo = @Codigo AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_activ_academica @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'aa_rc' AS Tabla, COUNT(*) AS Cantidad FROM aa_rc WHERE activ_academicas_idcurso = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_activ_academica @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_activ_academica @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE activ_academica SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_area_conocimiento @Id VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'programa_ac' AS Tabla, COUNT(*) AS Cantidad FROM programa_ac WHERE area_conocimiento = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_area_conocimiento @Id VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_area_conocimiento @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE area_conocimiento SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_practica_estrategia @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'programa_pe' AS Tabla, COUNT(*) AS Cantidad FROM programa_pe WHERE practica_estrategia = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_practica_estrategia @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_practica_estrategia @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE practica_estrategia SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_car_innovacion @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'programa_ci' AS Tabla, COUNT(*) AS Cantidad FROM programa_ci WHERE car_innovacion = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_car_innovacion @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_car_innovacion @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE car_innovacion SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_aspecto_normativo @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'an_programa' AS Tabla, COUNT(*) AS Cantidad FROM an_programa WHERE aspecto_normativo = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_aspecto_normativo @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_aspecto_normativo @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE aspecto_normativo SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_enfoque @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'enfoque_rc' AS Tabla, COUNT(*) AS Cantidad FROM enfoque_rc WHERE enfoque = @Id AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_enfoque @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_enfoque @Id;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE enfoque SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_dependientes_aliado @Nit INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 'alianza' AS Tabla, COUNT(*) AS Cantidad FROM alianza WHERE aliado = @Nit AND activo = 1;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_retirar_aliado @Nit INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @hijos TABLE (Tabla VARCHAR(40), Cantidad INT);
    INSERT INTO @hijos EXEC dbo.sp_dependientes_aliado @Nit;

    IF EXISTS (SELECT 1 FROM @hijos WHERE Cantidad > 0)
    BEGIN
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'tiene registros activos en ',
            (SELECT STRING_AGG(CONCAT(Tabla, ' (', Cantidad, ')'), ', ') FROM @hijos WHERE Cantidad > 0));
        ROLLBACK TRANSACTION;
        THROW 50409, @mensaje, 1;
    END

    UPDATE aliado SET activo = 0 WHERE nit = @Nit AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_contar_administradores_activos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(DISTINCT u.id)
    FROM usuario u
    JOIN rol_usuario ru ON ru.usuario_id = u.id AND ru.activo = 1
    JOIN rol r ON r.id = ru.rol_id AND r.activo = 1
    WHERE u.activo = 1 AND r.nombre = 'administrador';
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_rol_retirar @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @usuarios INT = (
        SELECT COUNT(*) FROM rol_usuario ru
        JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1
        WHERE ru.rol_id = @Id AND ru.activo = 1);

    IF @usuarios > 0
    BEGIN
        ROLLBACK TRANSACTION;
        DECLARE @mensaje NVARCHAR(2048) = CONCAT(N'lo tienen ', @usuarios, N' usuario(s) activo(s)');
        THROW 50409, @mensaje, 1;
    END

    UPDATE rol SET activo = 0 WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_rol_usuario_asignar @UsuarioId INT, @RolId INT
AS
BEGIN
    -- 0 = ya la tenía activa (el servicio dice 409) · 1 = creada o reactivada
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @resultado INT = 1;
    IF EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @UsuarioId AND rol_id = @RolId AND activo = 1)
        SET @resultado = 0;
    ELSE IF EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @UsuarioId AND rol_id = @RolId)
        UPDATE rol_usuario SET activo = 1 WHERE usuario_id = @UsuarioId AND rol_id = @RolId;
    ELSE
        INSERT INTO rol_usuario (usuario_id, rol_id, activo) VALUES (@UsuarioId, @RolId, 1);

    COMMIT TRANSACTION;
    SELECT @resultado;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_rol_usuario_quitar @UsuarioId INT, @RolId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM rol WHERE id = @RolId AND nombre = 'administrador')
       AND EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @UsuarioId AND rol_id = @RolId AND activo = 1)
    BEGIN
        DECLARE @administradores TABLE (Cantidad INT);
        INSERT INTO @administradores EXEC dbo.sp_contar_administradores_activos;
        IF (SELECT Cantidad FROM @administradores) <= 1
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50410, N'No se puede quitar el rol administrador al único administrador activo.', 1;
        END
    END

    UPDATE rol_usuario SET activo = 0 WHERE usuario_id = @UsuarioId AND rol_id = @RolId AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_usuario_retirar @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM usuario u
               JOIN rol_usuario ru ON ru.usuario_id = u.id AND ru.activo = 1
               JOIN rol r ON r.id = ru.rol_id AND r.activo = 1
               WHERE u.id = @Id AND u.activo = 1 AND r.nombre = 'administrador')
    BEGIN
        DECLARE @administradores TABLE (Cantidad INT);
        INSERT INTO @administradores EXEC dbo.sp_contar_administradores_activos;
        IF (SELECT Cantidad FROM @administradores) <= 1
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50410, N'No se puede retirar al único administrador activo. Asigne el rol administrador a otro usuario primero.', 1;
        END
    END

    UPDATE usuario SET activo = 0, fecha_actualizacion = GETDATE() WHERE id = @Id AND activo = 1;
    DECLARE @filas INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @filas AS FilasAfectadas;
END
GO
