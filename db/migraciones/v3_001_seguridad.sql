-- ============================================================
-- v3_001 — Seguridad: los tres roles del sistema y el borrado lógico
-- completo en las tablas de usuarios.
--
-- IDEMPOTENTE (db/migraciones/LEAME.md): correrla dos veces deja la base
-- igual. init.sh la ejecuta en cada arranque.
--
-- No crea usuarios: el administrador inicial lo crea la API al arrancar con
-- los datos del .env, porque su contraseña tiene que guardarse HASHEADA y el
-- hash lo calcula la API (D-v3-5). Un INSERT aquí obligaría a escribir la
-- contraseña, o su hash, en un archivo versionado.
-- ============================================================

-- 1. Los tres roles que nombran los permisos (Seguridad/Roles.cs).
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = 'administrador')
    INSERT INTO rol (nombre, descripcion) VALUES
    ('administrador', 'Acceso completo: consultar, crear, modificar, retirar y administrar usuarios y roles');
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = 'editor')
    INSERT INTO rol (nombre, descripcion) VALUES
    ('editor', 'Consulta, crea y modifica registros; no puede retirarlos');
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = 'consulta')
    INSERT INTO rol (nombre, descripcion) VALUES
    ('consulta', 'Solo consulta');
GO

-- 2. rol y usuario traían 'activo BIT DEFAULT 1' ADMITIENDO NULL. Un NULL no
--    es ni activo ni inactivo: los filtros 'activo = 1' lo esconderían sin
--    que nadie lo haya retirado. Se normaliza y se vuelve obligatorio.
UPDATE rol SET activo = 1 WHERE activo IS NULL;
UPDATE usuario SET activo = 1 WHERE activo IS NULL;
GO
ALTER TABLE rol ALTER COLUMN activo BIT NOT NULL;
ALTER TABLE usuario ALTER COLUMN activo BIT NOT NULL;
GO

-- 3. rol_usuario no traía 'activo': quitar un rol habría sido un DELETE
--    físico (Artículo 6). Se agrega como en las 22 tablas del módulo.
IF COL_LENGTH('rol_usuario', 'activo') IS NULL
    ALTER TABLE rol_usuario ADD activo BIT NOT NULL CONSTRAINT DF_rol_usuario_activo DEFAULT 1;
GO
