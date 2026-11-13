-- Funciones atómicas PostgreSQL: errores P0400, P0409, P0410.

CREATE OR REPLACE FUNCTION sp_dependientes_universidad(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'facultad' AS Tabla, COUNT(*) AS Cantidad FROM facultad WHERE universidad = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_universidad(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE facultad, universidad IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_universidad(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE universidad SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_facultad(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'programa' AS Tabla, COUNT(*) AS Cantidad FROM programa WHERE facultad = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_facultad(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE facultad, programa IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_facultad(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE facultad SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_programa(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'acreditacion' AS Tabla, COUNT(*) AS Cantidad FROM acreditacion WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'registro_calificado', COUNT(*) FROM registro_calificado WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'activ_academica', COUNT(*) FROM activ_academica WHERE disenio = p_id AND activo = 1
    UNION ALL SELECT 'pasantia', COUNT(*) FROM pasantia WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'premio', COUNT(*) FROM premio WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'programa_ac', COUNT(*) FROM programa_ac WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'programa_pe', COUNT(*) FROM programa_pe WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'programa_ci', COUNT(*) FROM programa_ci WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'an_programa', COUNT(*) FROM an_programa WHERE programa = p_id AND activo = 1
    UNION ALL SELECT 'docente_departamento', COUNT(*) FROM docente_departamento WHERE departamento = p_id AND activo = 1
    UNION ALL SELECT 'alianza', COUNT(*) FROM alianza WHERE departamento = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_programa(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE acreditacion, activ_academica, alianza, an_programa, docente_departamento, pasantia, premio, programa, programa_ac, programa_ci, programa_pe, registro_calificado IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_programa(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE programa SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_registro_calificado(p_codigo INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'enfoque_rc' AS Tabla, COUNT(*) AS Cantidad FROM enfoque_rc WHERE registro_calificado = p_codigo AND activo = 1
    UNION ALL SELECT 'aa_rc', COUNT(*) FROM aa_rc WHERE registro_calificado_codigo = p_codigo AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_registro_calificado(p_codigo INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE aa_rc, enfoque_rc, registro_calificado IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_registro_calificado(p_codigo) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE registro_calificado SET activo = 0 WHERE codigo = p_codigo AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_activ_academica(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'aa_rc' AS Tabla, COUNT(*) AS Cantidad FROM aa_rc WHERE activ_academicas_idcurso = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_activ_academica(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE aa_rc, activ_academica IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_activ_academica(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE activ_academica SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_area_conocimiento(p_id VARCHAR(6))
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'programa_ac' AS Tabla, COUNT(*) AS Cantidad FROM programa_ac WHERE area_conocimiento = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_area_conocimiento(p_id VARCHAR(6))
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE area_conocimiento, programa_ac IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_area_conocimiento(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE area_conocimiento SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_practica_estrategia(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'programa_pe' AS Tabla, COUNT(*) AS Cantidad FROM programa_pe WHERE practica_estrategia = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_practica_estrategia(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE practica_estrategia, programa_pe IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_practica_estrategia(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE practica_estrategia SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_car_innovacion(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'programa_ci' AS Tabla, COUNT(*) AS Cantidad FROM programa_ci WHERE car_innovacion = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_car_innovacion(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE car_innovacion, programa_ci IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_car_innovacion(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE car_innovacion SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_aspecto_normativo(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'an_programa' AS Tabla, COUNT(*) AS Cantidad FROM an_programa WHERE aspecto_normativo = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_aspecto_normativo(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE an_programa, aspecto_normativo IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_aspecto_normativo(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE aspecto_normativo SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_enfoque(p_id INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'enfoque_rc' AS Tabla, COUNT(*) AS Cantidad FROM enfoque_rc WHERE enfoque = p_id AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_enfoque(p_id INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE enfoque, enfoque_rc IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_enfoque(p_id) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE enfoque SET activo = 0 WHERE id = p_id AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_dependientes_aliado(p_nit INT)
RETURNS TABLE(tabla text, cantidad bigint) LANGUAGE sql AS $$
SELECT 'alianza' AS Tabla, COUNT(*) AS Cantidad FROM alianza WHERE aliado = p_nit AND activo = 1;
$$;

CREATE OR REPLACE FUNCTION sp_retirar_aliado(p_nit INT)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer; mensaje text;
BEGIN
    LOCK TABLE aliado, alianza IN SHARE ROW EXCLUSIVE MODE;
    SELECT string_agg(tabla || ' (' || cantidad || ')', ', ') INTO mensaje
    FROM sp_dependientes_aliado(p_nit) WHERE cantidad > 0;
    IF mensaje IS NOT NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0409', MESSAGE = 'tiene registros activos en ' || mensaje;
    END IF;
    UPDATE aliado SET activo = 0 WHERE nit = p_nit AND activo = 1;
    GET DIAGNOSTICS filas = ROW_COUNT;
    RETURN filas;
END;
$$;


CREATE OR REPLACE FUNCTION sp_contar_administradores_activos()
RETURNS integer LANGUAGE sql AS $$
SELECT COUNT(DISTINCT u.id)::integer FROM usuario u
JOIN rol_usuario ru ON ru.usuario_id=u.id AND ru.activo=1
JOIN rol r ON r.id=ru.rol_id AND r.activo=1
WHERE u.activo=1 AND r.nombre='administrador';
$$;

CREATE OR REPLACE FUNCTION sp_rol_retirar(p_id integer)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer;
BEGIN
    LOCK TABLE rol, rol_usuario, usuario IN SHARE ROW EXCLUSIVE MODE;
    IF EXISTS(SELECT 1 FROM rol_usuario ru JOIN usuario u ON u.id=ru.usuario_id
              WHERE ru.rol_id=p_id AND ru.activo=1 AND u.activo=1) THEN
        RAISE EXCEPTION USING ERRCODE='P0409', MESSAGE='El rol tiene usuarios activos.';
    END IF;
    UPDATE rol SET activo=0 WHERE id=p_id AND activo=1;
    GET DIAGNOSTICS filas=ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_rol_usuario_asignar(p_usuarioid integer, p_rolid integer)
RETURNS integer LANGUAGE plpgsql AS $$
BEGIN
    LOCK TABLE rol, rol_usuario, usuario IN SHARE ROW EXCLUSIVE MODE;
    IF NOT EXISTS(SELECT 1 FROM usuario WHERE id=p_usuarioid AND activo=1)
       OR NOT EXISTS(SELECT 1 FROM rol WHERE id=p_rolid AND activo=1) THEN
        RAISE EXCEPTION USING ERRCODE='P0400', MESSAGE='El usuario o rol no está activo.';
    END IF;
    IF EXISTS(SELECT 1 FROM rol_usuario WHERE usuario_id=p_usuarioid AND rol_id=p_rolid AND activo=1) THEN
        RETURN 0;
    END IF;
    INSERT INTO rol_usuario(usuario_id, rol_id, activo) VALUES(p_usuarioid,p_rolid,1)
    ON CONFLICT(usuario_id,rol_id) DO UPDATE SET activo=1;
    RETURN 1;
END;
$$;

CREATE OR REPLACE FUNCTION sp_rol_usuario_quitar(p_usuarioid integer, p_rolid integer)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer;
BEGIN
    LOCK TABLE rol, rol_usuario, usuario IN SHARE ROW EXCLUSIVE MODE;
    IF EXISTS(SELECT 1 FROM rol WHERE id=p_rolid AND nombre='administrador')
       AND EXISTS(SELECT 1 FROM rol_usuario WHERE usuario_id=p_usuarioid AND rol_id=p_rolid AND activo=1)
       AND sp_contar_administradores_activos()<=1 THEN
        RAISE EXCEPTION USING ERRCODE='P0410', MESSAGE='No se puede quitar el rol administrador al único administrador activo.';
    END IF;
    UPDATE rol_usuario SET activo=0 WHERE usuario_id=p_usuarioid AND rol_id=p_rolid AND activo=1;
    GET DIAGNOSTICS filas=ROW_COUNT;
    RETURN filas;
END;
$$;

CREATE OR REPLACE FUNCTION sp_usuario_retirar(p_id integer)
RETURNS integer LANGUAGE plpgsql AS $$
DECLARE filas integer;
BEGIN
    LOCK TABLE rol, rol_usuario, usuario IN SHARE ROW EXCLUSIVE MODE;
    IF EXISTS(SELECT 1 FROM usuario u JOIN rol_usuario ru ON ru.usuario_id=u.id AND ru.activo=1
              JOIN rol r ON r.id=ru.rol_id AND r.activo=1
              WHERE u.id=p_id AND u.activo=1 AND r.nombre='administrador')
       AND sp_contar_administradores_activos()<=1 THEN
        RAISE EXCEPTION USING ERRCODE='P0410', MESSAGE='No se puede retirar al único administrador activo. Asigne el rol administrador a otro usuario primero.';
    END IF;
    UPDATE usuario SET activo=0, fecha_actualizacion=LOCALTIMESTAMP WHERE id=p_id AND activo=1;
    GET DIAGNOSTICS filas=ROW_COUNT;
    RETURN filas;
END;
$$;
