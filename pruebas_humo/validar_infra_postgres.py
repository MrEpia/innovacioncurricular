from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
import subprocess, uuid, json, os

import argparse
parser=argparse.ArgumentParser(description='Pruebas de migración y concurrencia sobre un laboratorio Docker dedicado. Crea dos bases nuevas, sin borrar ninguna.')
parser.add_argument('--env-file',required=True,type=Path)
parser.add_argument('--trabajo',required=True,type=Path)
parser.add_argument('--contenedor',default='astra-neon-v2-pg-1')
parser.add_argument('--red',default='astra-neon-v2_default')
args=parser.parse_args()
root=Path(__file__).resolve().parents[1]
if args.trabajo.resolve().is_relative_to(root):
    raise SystemExit('Use --trabajo fuera del repositorio para proteger archivos privados.')
work=args.trabajo.resolve()/('infra-'+uuid.uuid4().hex[:8])
work.mkdir(parents=True,exist_ok=False)
container=args.contenedor
network=args.red
if not container.startswith('astra-'):
    raise SystemExit('Se requiere un laboratorio dedicado con prefijo astra-.')
secrets=dict(x.split('=',1) for x in args.env_file.read_text(encoding='utf-8').splitlines() if '=' in x)
def sql(query,db='innovacion_neon_v2'):
    p=subprocess.run(['docker','exec','-i',container,'psql','-U','astra','-d',db,'-X','-t','-A','-v','ON_ERROR_STOP=1','-v','VERBOSITY=verbose'],input=query,capture_output=True,text=True)
    return p.returncode,(p.stdout+p.stderr).strip()
suffix=uuid.uuid4().hex[:8]
guard='astra_guard_'+suffix
infra='astra_infra_'+suffix
for name in [guard,infra]:
    code,out=sql(f'CREATE DATABASE {name}')
    assert code==0,out
code,out=sql('CREATE TABLE testigo(id integer PRIMARY KEY); INSERT INTO testigo VALUES(1);',guard)
assert code==0,out
def migrate(db,extra=None):
    path=work/f'.env.infra-{db}'
    path.write_text('ASPNETCORE_ENVIRONMENT=Development\nConnectionStrings__Postgres=Host=pg;Database='+db+';Username=astra;Password='+secrets['PG_PRUEBA_CLAVE']+';SSL Mode=Disable\n')
    args=['docker','run','--rm','--network',network,'--env-file',str(path)]
    if extra: args+=['-v',str(extra)+':/app/MigracionesPostgres:ro']
    args+=['astra-innovacion-api:v2','--migrar-postgres']
    p=subprocess.run(args,capture_output=True,text=True)
    return p.returncode,p.stdout+p.stderr
code,out=migrate(guard)
assert code!=0 and 'Base existente sin historial' in out
assert sql("SELECT COUNT(*) FROM testigo; SELECT to_regclass('public.astra_migraciones') IS NULL",guard)[1]=='1\nt'
print('OK: base preexistente sin historial rechazada, testigo intacto, sin tabla de historial',flush=True)
code,out=migrate(infra)
assert code==0,out
code,out=migrate(infra)
assert code==0 and out.count('Omitida')==3,out
print('OK: migración repetida conserva tres registros sin duplicar',flush=True)
bad=work/'migraciones-checksum'
bad.mkdir(exist_ok=True)
for source in (root/'db/postgres').glob('*.sql'):
    (bad/source.name).write_text(source.read_text(encoding='utf-8')+'\n-- checksum diferente de prueba\n',encoding='utf-8')
code,out=migrate(infra,bad)
assert code!=0 and 'ya aplicada cambió' in out,out
assert sql('SELECT COUNT(*) FROM astra_migraciones',infra)[1]=='3'
print('OK: cambio de checksum rechazado, historial intacto',flush=True)
code,out=sql("INSERT INTO usuario(username,password,email) VALUES('race1','hash_no_autenticable','race1@example.test'),('race2','hash_no_autenticable','race2@example.test'); INSERT INTO rol_usuario(usuario_id,rol_id,activo) SELECT u.id,r.id,1 FROM usuario u CROSS JOIN rol r WHERE r.nombre='administrador';",infra)
assert code==0,out
with ThreadPoolExecutor(max_workers=2) as pool:
    attempts=list(pool.map(lambda i:sql(f'SELECT sp_usuario_retirar({i})',infra),[1,2]))
assert sorted(x[0]==0 for x in attempts)==[False,True],attempts
assert any('P0410' in x[1] for x in attempts),attempts
assert sql('SELECT sp_contar_administradores_activos()',infra)[1]=='1'
print('OK: retiros concurrentes conservan exactamente un administrador activo',flush=True)
# Forzar una violación DURANTE la escritura del detalle (23505), después de insertar el maestro.
setup="""INSERT INTO facultad(id,nombre,tipo,fecha_fun,universidad) VALUES(1900000001,'Prueba','Facultad','2000-01-01',1);
INSERT INTO programa(id,nombre,tipo,nivel,fecha_creacion,numero_cohortes,cant_graduados,fecha_actualizacion,ciudad,facultad) VALUES(1900000002,'Prueba','Pregrado','Profesional','2000','1','0','2026','Prueba',1900000001);
INSERT INTO enfoque(id,nombre,descripcion) VALUES(1900000003,'Prueba','Prueba');"""
assert sql(setup,infra)[0]==0
code,out=sql("SELECT sp_registro_calificado_completo_crear(1900000004,'1','1','1','Prueba','2024-01-01','2025-01-01','1','2','X',1900000002,'[1900000003,1900000003]','[]')",infra)
assert code!=0 and '23505' in out,out
assert sql('SELECT (SELECT COUNT(*) FROM registro_calificado WHERE codigo=1900000004)+(SELECT COUNT(*) FROM enfoque_rc WHERE registro_calificado=1900000004)',infra)[1]=='0'
print('OK: duplicado durante INSERT revierte maestro y detalle completos',flush=True)
(work/'resultados_infra.json').write_text(json.dumps({'guard_database':guard,'infra_database':infra,'comprobaciones':5,'resultado':'PASS'},indent=2))
