-- OPCIONAL: datos ficticios de demostración, no transferencia de producción.
/* =========================================================================
   v4_001 — Datos de demostración
   =========================================================================
   Los reportes y el dashboard de la v4 necesitan datos para mostrar algo.
   La base del curso solo trae dos catálogos llenos (universidad y
   area_conocimiento). Esta migración siembra un escenario pequeño pero
   completo: 6 facultades, 12 programas y sus hijos en todas las tablas.

   Reglas:
     · Idempotente: cada fila entra solo si su llave NO existe, así que
       correrla dos veces no duplica nada y no choca con datos ya creados.
     · Respeta las reglas de negocio de los servicios: tipos de facultad,
       dedicaciones, fechaFin ≥ fechaInicio, espejo 0/1.
     · Llaves pequeñas y fijas. Las pruebas de humo usan llaves aleatorias
       de 6 cifras, así que no se cruzan con estas.
   ========================================================================= */


-- ---------------------------------------------------------------- catálogos v1
INSERT INTO enfoque (id, nombre, descripcion, activo)
SELECT v.id, v.nombre, v.descripcion, 1 FROM (VALUES
    (11, 'Por competencias', 'Formación orientada a desempeños'),
    (12, 'Constructivista', 'El estudiante construye el saber'),
    (13, 'Aprendizaje basado en problemas', 'Parte de problemas reales'),
    (14, 'Humanista', 'Formación integral de la persona'),
    (15, 'STEAM', 'Integra ciencia, tecnología y arte')
) v(id, nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM enfoque e WHERE e.id = v.id);

INSERT INTO practica_estrategia (id, tipo, nombre, descripcion, activo)
SELECT v.id, v.tipo, v.nombre, v.descripcion, 1 FROM (VALUES
    (21, 'Estrategia', 'Aula invertida', 'Teoría en casa, práctica en clase'),
    (22, 'Práctica', 'Proyectos integradores', 'Un proyecto por semestre'),
    (23, 'Estrategia', 'Gamificación', 'Mecánicas de juego en el aula'),
    (24, 'Práctica', 'Laboratorio remoto', 'Prácticas en línea'),
    (25, 'Estrategia', 'Mentoría entre pares', 'Estudiantes avanzados guían')
) v(id, tipo, nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM practica_estrategia p WHERE p.id = v.id);

INSERT INTO car_innovacion (id, nombre, descripcion, tipo, activo)
SELECT v.id, v.nombre, v.descripcion, v.tipo, 1 FROM (VALUES
    (31, 'Flexibilidad curricular', 'Electivas y rutas propias', 'Curricular'),
    (32, 'Doble titulación', 'Convenios con otras universidades', 'Internacional'),
    (33, 'Créditos virtuales', 'Cursos 100 % en línea', 'Tecnológica'),
    (34, 'Investigación temprana', 'Semilleros desde primer semestre', 'Investigativa'),
    (35, 'Bilingüismo', 'Cursos dictados en inglés', 'Internacional')
) v(id, nombre, descripcion, tipo)
WHERE NOT EXISTS (SELECT 1 FROM car_innovacion c WHERE c.id = v.id);

INSERT INTO aspecto_normativo (id, tipo, descripcion, fuente, activo)
SELECT v.id, v.tipo, v.descripcion, v.fuente, 1 FROM (VALUES
    (41, 'Decreto', 'Decreto 1330 de 2019: registro calificado', 'MEN'),
    (42, 'Acuerdo', 'Acuerdo 02 de 2020: acreditación', 'CESU'),
    (43, 'Resolución', 'Resolución 21795 de 2020', 'MEN'),
    (44, 'Ley', 'Ley 30 de 1992: educación superior', 'Congreso'),
    (45, 'Acuerdo', 'Reglamento estudiantil institucional', 'Consejo')
) v(id, tipo, descripcion, fuente)
WHERE NOT EXISTS (SELECT 1 FROM aspecto_normativo a WHERE a.id = v.id);

INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
SELECT v.nit, v.razon, v.contacto, v.correo, v.telefono, v.ciudad, 1 FROM (VALUES
    (900100001, 'Andina Software S.A.S.', 'Laura Gómez', 'alianzas@andina.test', '6014001001', 'Bogotá'),
    (900100002, 'Hospital del Valle', 'Carlos Ruiz', 'docencia@hvalle.test', '6024002002', 'Cali'),
    (900100003, 'Puerto Caribe Logística', 'Ana Pérez', 'talento@pcaribe.test', '6054003003', 'Cartagena'),
    (900100004, 'Energía Antioquia', 'Jorge Mesa', 'practicas@energia.test', '6044004004', 'Medellín'),
    (900100005, 'Fundación Monte Verde', 'Marta Ríos', 'contacto@fmv.test', '6084005005', 'Armenia')
) v(nit, razon, contacto, correo, telefono, ciudad)
WHERE NOT EXISTS (SELECT 1 FROM aliado a WHERE a.nit = v.nit);

-- ---------------------------------------------------------------- facultades
INSERT INTO facultad (id, nombre, tipo, fecha_fun, universidad, activo)
SELECT v.id, v.nombre, v.tipo, v.fecha::date, v.universidad, 1 FROM (VALUES
    (101, 'Facultad de Ingeniería', 'Facultad', '1985-02-01', 1),
    (102, 'Facultad de Ciencias de la Salud', 'Facultad', '1990-08-01', 2),
    (103, 'Facultad de Ciencias Empresariales', 'Facultad', '1988-02-01', 4),
    (104, 'Departamento de Humanidades', 'Departamento', '1995-02-01', 3),
    (105, 'Instituto de Investigación', 'Instituto', '2005-08-01', 4),
    (106, 'Facultad de Educación', 'Facultad', '1998-02-01', 5)
) v(id, nombre, tipo, fecha, universidad)
WHERE NOT EXISTS (SELECT 1 FROM facultad f WHERE f.id = v.id);

-- ---------------------------------------------------------------- programas
INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                      cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
SELECT v.id, v.nombre, v.tipo, v.nivel, v.creacion, NULL, v.cohortes, v.graduados, v.actualizacion, v.ciudad, v.facultad, 1 FROM (VALUES
    (201, 'Ingeniería de Sistemas', 'Pregrado', 'Profesional', '1986', '38', '2450', '2024', 'Bogotá', 101),
    (202, 'Ingeniería Electrónica', 'Pregrado', 'Profesional', '1992', '30', '1320', '2023', 'Bogotá', 101),
    (203, 'Especialización en Ciberseguridad', 'Posgrado', 'Especialización', '2015', '9', '210', '2024', 'Bogotá', 101),
    (204, 'Maestría en Ingeniería de Software', 'Posgrado', 'Maestría', '2012', '12', '180', '2025', 'Bogotá', 101),
    (205, 'Enfermería', 'Pregrado', 'Profesional', '1991', '33', '1980', '2024', 'Cali', 102),
    (206, 'Fisioterapia', 'Pregrado', 'Profesional', '1999', '24', '960', '2023', 'Cali', 102),
    (207, 'Administración de Empresas', 'Pregrado', 'Profesional', '1989', '35', '3100', '2024', 'Medellín', 103),
    (208, 'Contaduría Pública', 'Pregrado', 'Profesional', '1993', '31', '2200', '2022', 'Medellín', 103),
    (209, 'Tecnología en Gestión Logística', 'Pregrado', 'Tecnológico', '2010', '14', '520', '2024', 'Medellín', 103),
    (210, 'Licenciatura en Filosofía', 'Pregrado', 'Profesional', '1996', '27', '640', '2023', 'Cartagena', 104),
    (211, 'Doctorado en Ciencias Aplicadas', 'Posgrado', 'Doctorado', '2018', '5', '22', '2025', 'Medellín', 105),
    (212, 'Licenciatura en Educación Infantil', 'Pregrado', 'Profesional', '2001', '22', '890', '2024', 'Armenia', 106)
) v(id, nombre, tipo, nivel, creacion, cohortes, graduados, actualizacion, ciudad, facultad)
WHERE NOT EXISTS (SELECT 1 FROM programa p WHERE p.id = v.id);

-- ---------------------------------------------------------------- acreditaciones
INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
SELECT v.r, v.tipo, v.calif, v.ini, v.fin, v.programa, 1 FROM (VALUES
    (3001, 'Alta calidad', 'Excelente', '2021', '2029', 201),
    (3002, 'Alta calidad', 'Muy buena', '2020', '2026', 202),
    (3003, 'Alta calidad', 'Excelente', '2022', '2030', 205),
    (3004, 'Alta calidad', 'Buena', '2019', '2025', 207),
    (3005, 'Alta calidad', 'Muy buena', '2023', '2029', 208),
    (3006, 'Internacional', 'Excelente', '2022', '2027', 204),
    (3007, 'Alta calidad', 'Buena', '2021', '2027', 210)
) v(r, tipo, calif, ini, fin, programa)
WHERE NOT EXISTS (SELECT 1 FROM acreditacion a WHERE a.resolucion = v.r);

-- ---------------------------------------------------------------- registros calificados
INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia, fecha_inicio, fecha_fin,
                                 duracion_anios, duracion_semestres, tipo_titulacion, programa, activo)
SELECT v.c, v.cred, v.ha, v.hi, v.met, v.ini::date, v.fin::date, v.anios, v.sem, v.tit, v.programa, 1 FROM (VALUES
    (4001, '160', '48', '96', 'Presencial', '2021-03-01', '2028-03-01', '5', '10', 'Ingeniero', 201),
    (4002, '158', '48', '96', 'Presencial', '2020-06-01', '2027-06-01', '5', '10', 'Ingeniero', 202),
    (4003, '30', '32', '64', 'Virtual', '2022-01-15', '2029-01-15', '1', '2', 'Especialista', 203),
    (4004, '52', '32', '96', 'Híbrida', '2019-08-01', '2026-08-01', '2', '4', 'Magíster', 204),
    (4005, '150', '64', '80', 'Presencial', '2022-02-01', '2029-02-01', '5', '10', 'Enfermero', 205),
    (4006, '150', '64', '80', 'Presencial', '2021-02-01', '2028-02-01', '5', '10', 'Fisioterapeuta', 206),
    (4007, '144', '48', '96', 'Presencial', '2020-02-01', '2027-02-01', '4,5', '9', 'Administrador', 207),
    (4008, '144', '48', '96', 'Virtual', '2023-02-01', '2030-02-01', '4,5', '9', 'Contador', 208),
    (4009, '96', '48', '48', 'Virtual', '2022-08-01', '2029-08-01', '3', '6', 'Tecnólogo', 209),
    (4010, '160', '48', '96', 'Presencial', '2021-08-01', '2028-08-01', '5', '10', 'Licenciado', 210),
    (4011, '90', '32', '160', 'Presencial', '2018-08-01', '2025-08-01', '4', '8', 'Doctor', 211),
    (4012, '160', '48', '96', 'Híbrida', '2022-02-01', '2029-02-01', '5', '10', 'Licenciado', 212)
) v(c, cred, ha, hi, met, ini, fin, anios, sem, tit, programa)
WHERE NOT EXISTS (SELECT 1 FROM registro_calificado r WHERE r.codigo = v.c);

-- ---------------------------------------------------------------- actividades académicas
INSERT INTO activ_academica (id, nombre, num_creditos, tipo, area_formacion, h_acom, h_indep, idioma,
                             espejo, entidad_espejo, pais_espejo, disenio, activo)
SELECT v.id, v.nombre, v.cred, v.tipo, v.area, v.ha, v.hi, v.idioma, v.espejo, v.entidad, v.pais, v.disenio, 1 FROM (VALUES
    (501, 'Programación I', 3, 'Obligatoria', 'Básica', 48, 96, 'Español', 0, 'N/A', 'N/A', 201),
    (502, 'Bases de datos', 3, 'Obligatoria', 'Profesional', 48, 96, 'Español', 0, 'N/A', 'N/A', 201),
    (503, 'Inteligencia artificial', 3, 'Electiva', 'Profesional', 48, 96, 'Inglés', 1, 'Universidad de Salamanca', 'España', 201),
    (504, 'Circuitos digitales', 4, 'Obligatoria', 'Profesional', 64, 128, 'Español', 0, 'N/A', 'N/A', 202),
    (505, 'Seguridad en redes', 3, 'Obligatoria', 'Profesional', 32, 112, 'Español', 1, 'Tecnológico de Monterrey', 'México', 203),
    (506, 'Arquitectura de software', 4, 'Obligatoria', 'Profesional', 32, 160, 'Español', 1, 'Universidad de Chile', 'Chile', 204),
    (507, 'Cuidado del adulto', 4, 'Obligatoria', 'Profesional', 64, 128, 'Español', 0, 'N/A', 'N/A', 205),
    (508, 'Biomecánica', 3, 'Obligatoria', 'Básica', 48, 96, 'Español', 0, 'N/A', 'N/A', 206),
    (509, 'Mercadeo global', 3, 'Electiva', 'Profesional', 48, 96, 'Inglés', 1, 'Universidad de Lyon', 'Francia', 207),
    (510, 'Contabilidad internacional', 3, 'Obligatoria', 'Profesional', 48, 96, 'Español', 0, 'N/A', 'N/A', 208),
    (511, 'Cadena de suministro', 3, 'Obligatoria', 'Profesional', 48, 96, 'Español', 0, 'N/A', 'N/A', 209),
    (512, 'Ética', 2, 'Obligatoria', 'Humanística', 32, 64, 'Español', 0, 'N/A', 'N/A', 210),
    (513, 'Métodos de investigación', 4, 'Obligatoria', 'Investigativa', 32, 160, 'Español', 1, 'Universidad de Coímbra', 'Portugal', 211),
    (514, 'Desarrollo infantil', 3, 'Obligatoria', 'Profesional', 48, 96, 'Español', 0, 'N/A', 'N/A', 212)
) v(id, nombre, cred, tipo, area, ha, hi, idioma, espejo, entidad, pais, disenio)
WHERE NOT EXISTS (SELECT 1 FROM activ_academica a WHERE a.id = v.id);

-- ---------------------------------------------------------------- pasantías y premios
INSERT INTO pasantia (id, nombre, pais, empresa, descripcion, programa, activo)
SELECT v.id, v.nombre, v.pais, v.empresa, v.descr, v.programa, 1 FROM (VALUES
    (601, 'Desarrollo de software', 'Colombia', 'Andina Software', 'Equipo de producto', 201),
    (602, 'Ciberseguridad ofensiva', 'España', 'SecureLab Madrid', 'Pruebas de penetración', 203),
    (603, 'Atención hospitalaria', 'Colombia', 'Hospital del Valle', 'Rotación clínica', 205),
    (604, 'Rehabilitación deportiva', 'Argentina', 'Club Atlético Sur', 'Fisioterapia deportiva', 206),
    (605, 'Logística portuaria', 'Colombia', 'Puerto Caribe', 'Operación de patio', 209),
    (606, 'Auditoría', 'Estados Unidos', 'Big Four Miami', 'Auditoría financiera', 208),
    (607, 'Investigación en materiales', 'Alemania', 'Instituto Fraunhofer', 'Estancia doctoral', 211)
) v(id, nombre, pais, empresa, descr, programa)
WHERE NOT EXISTS (SELECT 1 FROM pasantia p WHERE p.id = v.id);

INSERT INTO premio (id, nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
SELECT v.id, v.nombre, v.descr, v.fecha::date, v.entidad, v.pais, v.programa, 1 FROM (VALUES
    (701, 'Maratón nacional de programación', 'Primer puesto', '2024-10-12', 'ACIS', 'Colombia', 201),
    (702, 'Premio a la innovación docente', 'Mejor curso virtual', '2023-05-20', 'MEN', 'Colombia', 204),
    (703, 'Excelencia en enfermería', 'Mención de honor', '2024-03-08', 'ANEC', 'Colombia', 205),
    (704, 'Concurso de emprendimiento', 'Segundo puesto', '2022-11-02', 'Innpulsa', 'Colombia', 207),
    (705, 'Premio iberoamericano de investigación', 'Artículo destacado', '2025-06-15', 'OEI', 'España', 211)
) v(id, nombre, descr, fecha, entidad, pais, programa)
WHERE NOT EXISTS (SELECT 1 FROM premio p WHERE p.id = v.id);

-- ---------------------------------------------------------------- relaciones (puentes)
INSERT INTO programa_ac (programa, area_conocimiento, activo)
SELECT v.p, v.a, 1 FROM (VALUES
    (201, '1A01'), (201, '1A02'), (202, '1A01'), (204, '1A02'), (203, '1A03'), (211, '1A03')
) v(p, a)
WHERE EXISTS (SELECT 1 FROM area_conocimiento x WHERE x.id = v.a)
  AND NOT EXISTS (SELECT 1 FROM programa_ac t WHERE t.programa = v.p AND t.area_conocimiento = v.a);

-- Para las facultades sin área de Matemáticas: una área cualquiera de otra gran área.
INSERT INTO programa_ac (programa, area_conocimiento, activo)
SELECT v.p, a.id, 1 FROM (VALUES (205, 'Ciencias Médicas y de la Salud'), (206, 'Ciencias Médicas y de la Salud'),
             (207, 'Ciencias Sociales'), (208, 'Ciencias Sociales'), (209, 'Ciencias Sociales'),
             (210, 'Humanidades'), (212, 'Ciencias Sociales')) v(p, gran)
CROSS JOIN LATERAL (SELECT id FROM area_conocimiento WHERE gran_area = v.gran AND activo = 1 ORDER BY id LIMIT 1) a
WHERE NOT EXISTS (SELECT 1 FROM programa_ac t WHERE t.programa = v.p AND t.area_conocimiento = a.id);

INSERT INTO programa_pe (programa, practica_estrategia, activo)
SELECT v.p, v.x, 1 FROM (VALUES
    (201, 21), (201, 22), (202, 22), (203, 24), (204, 21), (205, 25), (206, 22), (207, 23), (208, 21), (209, 24), (212, 23)
) v(p, x)
WHERE NOT EXISTS (SELECT 1 FROM programa_pe t WHERE t.programa = v.p AND t.practica_estrategia = v.x);

INSERT INTO programa_ci (programa, car_innovacion, activo)
SELECT v.p, v.x, 1 FROM (VALUES
    (201, 31), (201, 34), (202, 31), (203, 33), (204, 32), (204, 35), (205, 34), (207, 32), (208, 33), (209, 33), (211, 34), (211, 35)
) v(p, x)
WHERE NOT EXISTS (SELECT 1 FROM programa_ci t WHERE t.programa = v.p AND t.car_innovacion = v.x);

INSERT INTO an_programa (aspecto_normativo, programa, activo)
SELECT v.a, v.p, 1 FROM (VALUES
    (41, 201), (42, 201), (41, 202), (41, 203), (43, 204), (41, 205), (42, 205), (44, 207), (41, 208), (45, 210), (43, 211), (41, 212)
) v(a, p)
WHERE NOT EXISTS (SELECT 1 FROM an_programa t WHERE t.aspecto_normativo = v.a AND t.programa = v.p);

INSERT INTO enfoque_rc (enfoque, registro_calificado, activo)
SELECT v.e, v.r, 1 FROM (VALUES
    (11, 4001), (13, 4001), (11, 4002), (15, 4002), (11, 4003), (12, 4004), (14, 4005), (11, 4006),
    (13, 4007), (11, 4008), (13, 4009), (14, 4010), (12, 4011), (12, 4012), (14, 4012)
) v(e, r)
WHERE NOT EXISTS (SELECT 1 FROM enfoque_rc t WHERE t.enfoque = v.e AND t.registro_calificado = v.r);

INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
SELECT v.a, v.r, v.comp, v.sem, 1 FROM (VALUES
    (501, 4001, 'Básico', '1'), (502, 4001, 'Profesional', '4'), (503, 4001, 'Electivo', '8'),
    (504, 4002, 'Profesional', '3'), (505, 4003, 'Profesional', '1'), (506, 4004, 'Profesional', '2'),
    (507, 4005, 'Profesional', '5'), (508, 4006, 'Básico', '2'), (509, 4007, 'Electivo', '7'),
    (510, 4008, 'Profesional', '5'), (511, 4009, 'Profesional', '3'), (512, 4010, 'Humanístico', '1'),
    (513, 4011, 'Investigativo', '1'), (514, 4012, 'Profesional', '4')
) v(a, r, comp, sem)
WHERE NOT EXISTS (SELECT 1 FROM aa_rc t WHERE t.activ_academicas_idcurso = v.a AND t.registro_calificado_codigo = v.r);

-- ---------------------------------------------------------------- docentes y alianzas
INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
SELECT v.d, v.p, v.ded, v.modal, v.ingreso::date, NULL, 1 FROM (VALUES
    (10001, 201, 'Tiempo Completo', 'Presencial', '2010-02-01'), (10002, 201, 'Tiempo Completo', 'Presencial', '2015-08-01'),
    (10003, 201, 'Cátedra', 'Virtual', '2020-02-01'), (10004, 202, 'Tiempo Completo', 'Presencial', '2012-02-01'),
    (10005, 203, 'Cátedra', 'Virtual', '2018-08-01'), (10006, 204, 'Medio Tiempo', 'Híbrida', '2016-02-01'),
    (10007, 205, 'Tiempo Completo', 'Presencial', '2011-02-01'), (10008, 205, 'Medio Tiempo', 'Presencial', '2019-02-01'),
    (10009, 206, 'Cátedra', 'Presencial', '2021-08-01'), (10010, 207, 'Tiempo Completo', 'Presencial', '2009-02-01'),
    (10011, 208, 'Medio Tiempo', 'Virtual', '2017-08-01'), (10012, 209, 'Cátedra', 'Virtual', '2022-02-01'),
    (10013, 210, 'Tiempo Completo', 'Presencial', '2008-02-01'), (10014, 211, 'Tiempo Completo', 'Presencial', '2018-02-01'),
    (10015, 212, 'Medio Tiempo', 'Presencial', '2014-08-01')
) v(d, p, ded, modal, ingreso)
WHERE NOT EXISTS (SELECT 1 FROM docente_departamento t WHERE t.docente = v.d AND t.departamento = v.p);

INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
SELECT v.a, v.p, v.ini::date, v.fin::date, v.doc, 1 FROM (VALUES
    (900100001, 201, '2022-01-01', '2026-12-31', 10001), (900100001, 203, '2023-01-01', NULL, 10005),
    (900100002, 205, '2021-06-01', '2027-06-01', 10007), (900100002, 206, '2022-02-01', NULL, 10009),
    (900100003, 209, '2023-03-01', '2026-03-01', 10012), (900100004, 202, '2020-01-01', '2025-12-31', 10004),
    (900100004, 211, '2024-01-01', NULL, 10014), (900100005, 212, '2022-08-01', NULL, 10015)
) v(a, p, ini, fin, doc)
WHERE NOT EXISTS (SELECT 1 FROM alianza t WHERE t.aliado = v.a AND t.departamento = v.p);
