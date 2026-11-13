CREATE OR REPLACE FUNCTION sp_registro_calificado_completo_crear(
    p_codigo INT,
    p_cantcreditos VARCHAR(45),
    p_horaacom VARCHAR(45),
    p_horaind VARCHAR(45),
    p_metodologia VARCHAR(45),
    p_fechainicio timestamp without time zone,
    p_fechafin timestamp without time zone,
    p_duracionanios VARCHAR(45),
    p_duracionsemestres VARCHAR(45),
    p_tipotitulacion VARCHAR(45),
    p_programa INT,
    p_enfoques text,
    p_actividades text)
RETURNS integer LANGUAGE plpgsql AS $$
BEGIN

    LOCK TABLE aa_rc, activ_academica, enfoque, enfoque_rc, programa, registro_calificado IN SHARE ROW EXCLUSIVE MODE;
    IF NOT EXISTS(SELECT 1 FROM programa WHERE id=p_programa AND activo=1)
       OR EXISTS(SELECT 1 FROM jsonb_array_elements_text(p_enfoques::jsonb) x WHERE NOT EXISTS(SELECT 1 FROM enfoque WHERE id=x.value::int AND activo=1))
       OR EXISTS(SELECT 1 FROM jsonb_to_recordset(p_actividades::jsonb) AS x(id int,componente text,semestre text) WHERE NOT EXISTS(SELECT 1 FROM activ_academica WHERE id=x.id AND activo=1)) THEN
        RAISE EXCEPTION USING ERRCODE='P0400', MESSAGE='Alguna referencia del registro no existe o está retirada. No se guardó nada.';
    END IF;
    INSERT INTO registro_calificado(codigo,cant_creditos,hora_acom,hora_ind,metodologia,fecha_inicio,fecha_fin,duracion_anios,duracion_semestres,tipo_titulacion,programa,activo)
    VALUES(p_codigo,p_cantcreditos,p_horaacom,p_horaind,p_metodologia,p_fechainicio,p_fechafin,p_duracionanios,p_duracionsemestres,p_tipotitulacion,p_programa,1);
    INSERT INTO enfoque_rc(enfoque,registro_calificado,activo) SELECT value::int,p_codigo,1 FROM jsonb_array_elements_text(p_enfoques::jsonb);
    INSERT INTO aa_rc(activ_academicas_idcurso,registro_calificado_codigo,componente,semestre,activo)
    SELECT id,p_codigo,componente,semestre,1 FROM jsonb_to_recordset(p_actividades::jsonb) AS x(id int,componente text,semestre text);

    RETURN 1;
END;
$$;

CREATE OR REPLACE FUNCTION sp_programa_completo_crear(
    p_id INT,
    p_nombre VARCHAR(150),
    p_tipo VARCHAR(45),
    p_nivel VARCHAR(45),
    p_fechacreacion VARCHAR(45),
    p_fechacierre VARCHAR(45),
    p_numerocohortes VARCHAR(45),
    p_cantgraduados VARCHAR(45),
    p_fechaactualizacion VARCHAR(45),
    p_ciudad VARCHAR(45),
    p_facultad INT,
    p_areas text,
    p_practicas text,
    p_caracteristicas text)
RETURNS integer LANGUAGE plpgsql AS $$
BEGIN

    LOCK TABLE area_conocimiento, car_innovacion, facultad, practica_estrategia, programa, programa_ac, programa_ci, programa_pe IN SHARE ROW EXCLUSIVE MODE;
    IF NOT EXISTS(SELECT 1 FROM facultad WHERE id=p_facultad AND activo=1)
       OR EXISTS(SELECT 1 FROM jsonb_array_elements_text(p_areas::jsonb) x WHERE NOT EXISTS(SELECT 1 FROM area_conocimiento WHERE id=x.value AND activo=1))
       OR EXISTS(SELECT 1 FROM jsonb_array_elements_text(p_practicas::jsonb) x WHERE NOT EXISTS(SELECT 1 FROM practica_estrategia WHERE id=x.value::int AND activo=1))
       OR EXISTS(SELECT 1 FROM jsonb_array_elements_text(p_caracteristicas::jsonb) x WHERE NOT EXISTS(SELECT 1 FROM car_innovacion WHERE id=x.value::int AND activo=1)) THEN
        RAISE EXCEPTION USING ERRCODE='P0400', MESSAGE='Alguna referencia del programa no existe o está retirada. No se guardó nada.';
    END IF;
    INSERT INTO programa(id,nombre,tipo,nivel,fecha_creacion,fecha_cierre,numero_cohortes,cant_graduados,fecha_actualizacion,ciudad,facultad,activo)
    VALUES(p_id,p_nombre,p_tipo,p_nivel,p_fechacreacion,p_fechacierre,p_numerocohortes,p_cantgraduados,p_fechaactualizacion,p_ciudad,p_facultad,1);
    INSERT INTO programa_ac(programa,area_conocimiento,activo) SELECT p_id,value,1 FROM jsonb_array_elements_text(p_areas::jsonb);
    INSERT INTO programa_pe(programa,practica_estrategia,activo) SELECT p_id,value::int,1 FROM jsonb_array_elements_text(p_practicas::jsonb);
    INSERT INTO programa_ci(programa,car_innovacion,activo) SELECT p_id,value::int,1 FROM jsonb_array_elements_text(p_caracteristicas::jsonb);

    RETURN 1;
END;
$$;
