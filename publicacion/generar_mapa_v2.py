"""Inventario verificable del código. No lee .env ni guarda cuerpos HTTP de autenticación."""
from pathlib import Path
import argparse, json, re, subprocess, urllib.request
from urllib.parse import quote

ROOT = Path(__file__).resolve().parents[1]
SPEC = 'docs/spec_kit/versiones/v2_neon_render'
REPO = 'https://github.com/MrEpia/innovacioncurricular'

def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT, text=True).strip()

def read(path):
    return (ROOT/path).read_text(encoding='utf-8-sig')

def line_at(text, index):
    return text.count('\n', 0, index)+1

def canonical(route):
    return re.sub(r'\{(\w+):[^}]+\}', r'{\1}', route)

def source_url(ref, path, line=None):
    url = REPO+'/blob/'+ref+'/'+quote(path, safe='/')
    return url+(f'#L{line}' if line else '')

def methods(text):
    return [(m[1], line_at(text,m.start())) for m in re.finditer(r'^\s*(?:public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?[^\n=;{}]+?\s+(\w+)\s*\([^;]*?',text,re.M)]

def method_part(text, name):
    m=re.search(r'^\s*(?:public|private)\s+(?:async\s+)?[^\n]+?\b'+re.escape(name)+r'\s*\(',text,re.M)
    if not m:return text
    n=re.search(r'^\s*(?:public|private)\s+(?:async\s+)?[^\n]+?\w+\s*\(',text[m.end():],re.M)
    return text[m.start():m.end()+n.start()] if n else text[m.start():]

def endpoints():
    rows=[]
    for f in sorted((ROOT/'api_innovacion/Controllers').glob('*.cs')):
        path=f.relative_to(ROOT).as_posix();s=read(path)
        prefix=re.search(r'^\[Route\("([^"]+)"\)\]',s,re.M)[1]
        service=re.search(r'private readonly (IServicio\w+)',s)[1]
        servicefile='api_innovacion/Servicios/'+service[1:]+'.cs'
        svc=read(servicefile)
        repotypes=sorted(set(re.findall(r'\bIRepositorio(\w+)',svc)))
        for a in re.finditer(r'^\s*\[Http(Get|Post|Put|Patch|Delete|Head|Options)(?:\("([^"]*)"\))?\]',s,re.M):
            m=re.search(r'public\s+(?:async\s+)?\S+\s+(\w+)\s*\(',s[a.end():])
            if not m:raise ValueError(path+' no tiene método después del atributo')
            name=m[1];body=method_part(s,name)
            smethods=sorted(set(re.findall(r'_servicio\.(\w+)\(',body)))
            declared='/'+prefix.strip('/')+('/'+a[2].strip('/') if a[2] else '')
            summaries=re.findall(r'<summary>(.*?)</summary>',s[max(0,a.start()-600):a.start()],re.S)
            desc=re.sub(r'\s+',' ',summaries[-1].replace('///','')).strip() if summaries else name+'()'
            auth='Anónimo' if '[AllowAnonymous]' in s else 'JWT y filtro global PermisoPorVerbo'
            if '[Authorize(Roles = Roles.Administrador)]' in s:auth+='; solo administrador'
            reps=[];tables=set()
            for typ in repotypes:
                for engine,folder in [('SqlServer',''),('Postgres','Postgres/')]:
                    rp=f'api_innovacion/Repositorios/{folder}Repositorio{typ}{engine}.cs'
                    if (ROOT/rp).exists():
                        reps.append(rp)
                        if typ != 'Referencias':
                            for sm in smethods:
                                part=method_part(read(rp),sm)
                                tables.update(re.findall(r'\b(?:FROM|JOIN|UPDATE|INTO)\s+(\w+)',part,re.I))
            resource=prefix.split('/')[1]
            if resource not in ['reportes','dashboard','login']:tables.add(resource)
            if resource=='login':tables.update(['usuario','rol','rol_usuario'])
            sql=read('db/postgres/001_esquema.sql')
            known=set(re.findall(r'CREATE TABLE (\w+)',sql))
            tables=sorted(tables & known)
            model='api_innovacion/Modelos/'+f.stem.replace('Controller','')+'.cs'
            if not (ROOT/model).exists():model='api_innovacion/Seguridad/IGeneradorToken.cs' if resource=='login' else 'Ver DTO y peticiones del controlador'
            tests=[]
            for tf in sorted((ROOT/'pruebas_humo').glob('humo_*.py')):
                if '/'+prefix in tf.read_text(encoding='utf-8-sig'):tests.append(tf.relative_to(ROOT).as_posix())
            if resource in ['programa','registro_calificado'] and 'Completo' in f.stem:
                tables= ['programa','programa_ac','programa_pe','programa_ci'] if resource=='programa' else ['registro_calificado','enfoque_rc','aa_rc']
            rows.append(dict(method=a[1].upper(),route=canonical(declared),declared=declared,path=path,line=line_at(s,a.start()+len(a[0])-len(a[0].lstrip())),function=name,service=servicefile,service_methods=smethods,repositories=reps,model=model,tables=tables,auth=auth,tests=tests,description=desc,component='API'))
    s=read('api_innovacion/Program.cs')
    for a in re.finditer(r'app\.MapGet\("([^"]*)"',s):
        rows.append(dict(method='GET',route=a[1],declared=a[1],path='api_innovacion/Program.cs',line=line_at(s,a.start()),function='lambda MapGet',service='Infraestructura inline / configuración',service_methods=[],repositories=[],model='Ninguno (health consulta historial o rol)',tables=['astra_migraciones','rol'] if a[1]=='/health' else [],auth='Anónimo',tests=['docs/VALIDACION_NEON_RENDER_V2.md'],description='Conexión e historial de migraciones' if a[1]=='/health' else 'Diagnóstico de versión sin tocar la base',component='API'))
    return sorted(rows,key=lambda x:(x['route'],x['method']))

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--ref',default=None,help='Commit que contiene todos los archivos enlazados')
    parser.add_argument('--swagger-url',default=None,help='Opcional: contrastar rutas API con OpenAPI real, sin autenticación')
    args=parser.parse_args()
    ref=args.ref or git('rev-parse','HEAD')
    rows=endpoints()
    verified=False
    if args.swagger_url:
        with urllib.request.urlopen(args.swagger_url,timeout=30) as response:swagger=json.load(response)
        actual={(method.upper(),path) for path,ops in swagger['paths'].items() for method in ops if method.lower() in ['get','post','put','patch','delete','head','options']}
        expected={(x['method'],x['route']) for x in rows}
        if actual!=expected:raise ValueError(f'Diferencias Swagger/código: faltantes={actual-expected}; sobrantes={expected-actual}')
        (ROOT/SPEC/'openapi.local.json').write_text(json.dumps(swagger,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        verified=True
    files=sorted(set(git('ls-files').splitlines()+[SPEC+'/6_contracts.md','MAPA_COMPLETO_PROYECTO_V2.txt']))
    files=[f for f in files if (ROOT/f).exists() or f.endswith('6_contracts.md') or f=='MAPA_COMPLETO_PROYECTO_V2.txt']
    contracts=['# Contratos HTTP — revisión V2\n',f'Código fuente auditado: [{ref}]({REPO}/tree/{ref}). {len(rows)} operaciones API explícitas. Comparación con Swagger real local: '+('coincidencia exacta' if verified else 'no ejecutada en esta invocación')+'.\n',
      'Los contratos funcionales históricos se conservan: 204 sin filas; cuerpos inválidos 422; reglas de negocio 400; inexistentes 404; conflicto 409; sin token 401; rol insuficiente 403. Las creaciones V1 conservan 200 y V2+ usan 201. Los esquemas exactos de peticiones/DTO y seguridad están en [openapi.local.json](openapi.local.json), exportado del backend compilado. No se presenta cada operación como verificada públicamente.\n',
      '| Método | Ruta completa | Función y declaración | Autorización |\n|---|---|---|---|']
    for x in rows:
        contracts.append(f"| {x['method']} | `{x['route']}` | [{x['function']}()]({source_url(ref,x['path'],x['line'])}) | {x['auth']} |")
    contracts.append('\nFrontend: las rutas Razor y los endpoints de sesión/health se enumeran por separado en MAPA_COMPLETO_PROYECTO_V2.txt. Swagger y los recursos estáticos son middleware/framework, no controladores de negocio.\n')
    (ROOT/SPEC/'6_contracts.md').write_text('\n'.join(contracts)+'\n',encoding='utf-8')
    out=['MAPA COMPLETO PROYECTO V2',f'Repositorio: {REPO}',f'Referencia inmutable del inventario: {ref}',
         'Estado: preparación local; Neon y Render externos NO VERIFICADOS. Ver ASTRA_ESTADO_TRABAJO.md.',
         f'API: {len(rows)} operaciones explícitas; contraste Swagger local: '+('COINCIDENCIA EXACTA' if verified else 'NO EJECUTADO'),
         'Los números de línea son de esta referencia. El mapa y su propio generador pueden estar en un commit posterior; sus enlaces de autorreferencia usan la rama.',
         '\nENDPOINTS API (contratos declarados y compilados; no equivalen a prueba pública individual)']
    for x in rows:
        out += ['-'*65,f"ENDPOINT: {x['method']} {x['route']}",'-'*65,
          'Descripción: '+x['description'],'Patrón declarado: '+x['declared'],'Archivo: '+x['path'],f"Línea: {x['line']}",'Controlador/función: '+x['function']+'()',
          'Servicio: '+x['service']+'; métodos: '+(', '.join(x['service_methods']) or 'inline'),
          'Repositorios: '+('; '.join(x['repositories']) or 'No aplica'),
          'Modelo/DTO: '+x['model'],'Tablas relacionadas: '+(', '.join(x['tables']) or 'Ninguna identificada; consultar el repositorio/controlador'),
          'Middleware/autorización: '+x['auth']+('; validación ApiController 422 y filtro de errores 500 en MVC' if '/Controllers/' in x['path'] else '; endpoint mínimo fuera del filtro MVC, errores health controlados'),
          'Pruebas relacionadas: '+('; '.join(x['tests']) or 'api_innovacion/pruebas; sin prueba HTTP individual localizada'),
          'Documentación: '+SPEC+'/6_contracts.md; openapi.local.json; docs/VALIDACION_NEON_RENDER_V2.md',
          'GitHub: '+source_url(ref,x['path'],x['line']),
          'Estado: IMPLEMENTADO; CONTRATO COMPARADO CON SWAGGER LOCAL. Cobertura funcional asociada en el informe; verificación pública pendiente.']
    out+=['\nENDPOINTS FRONTEND Y MIDDLEWARE']
    for f in files:
        if f.endswith('.razor'):
            s=read(f)
            for a in re.finditer(r'^@page "([^"]+)"',s,re.M):
                auth='Cookie de sesión y AuthorizeRouteView' if '[Authorize' in s else 'Página pública'
                out += [f'ENDPOINT: GET {a[1]} [FRONTEND]','Archivo: '+f,f'Línea: {line_at(s,a.start())}',f'Función: componente {Path(f).stem}; servicio(s): '+', '.join(re.findall(r'@inject\s+(\w+)',s)),
                        'Autorización: '+auth,'Pruebas: pruebas_humo/humo_front.py, humo_roles_front.py y humo_reportes.py (ver alcance en informe).',
                        'GitHub: '+source_url(ref,f,line_at(s,a.start()))]
    for f in ['front_blazor/Program.cs','front_blazor/Ensamblador/EnsambladorSesion.cs']:
        s=read(f)
        for a in re.finditer(r'app\.Map(Get|Post)\("([^"]+)"',s):
            out += [f'ENDPOINT: {a[1].upper()} {a[2]} [FRONTEND]',f'Archivo: {f}; línea: {line_at(s,a.start())}',
                    'Función: lambda Map'+a[1]+'; sesión cookie/antifalsificación en logout, diagnóstico público en health.',
                    'GitHub: '+source_url(ref,f,line_at(s,a.start()))]
    s=read('api_innovacion/Program.cs')
    for route,method in [('/swagger','UseSwaggerUI'),('/swagger/index.html','UseSwaggerUI'),('/swagger/v1/swagger.json','UseSwagger')]:
        pos=s.index('app.'+method+'(')
        out += [f'ENDPOINT: GET {route} [MIDDLEWARE SWASHBUCKLE]',f'Archivo: api_innovacion/Program.cs; línea: {line_at(s,pos)}; función: {method}()',
                'Autorización: anónimo; modelos/tablas: ninguno; pruebas: humo_reportes.py y exportación OpenAPI local.',
                'GitHub: '+source_url(ref,'api_innovacion/Program.cs',line_at(s,pos))]
    out += ['Infraestructura generada: MapRazorComponents/AddInteractiveServerRenderMode incorpora el transporte de Blazor/SignalR; MapStaticAssets publica las rutas del manifiesto estático. No se inventan números de línea de código del framework. Declaración en front_blazor/Program.cs; las rutas estáticas se relacionan con los archivos wwwroot de la sección B.',
            '\nA. ESTRUCTURA COMPLETA DEL REPOSITORIO']+files
    out+=['\nB. ARCHIVOS FRONTEND Y SUS COMPONENTES']
    for f in files:
        if f.startswith('front_blazor/'):
            if f.endswith(('.cs','.razor')):
                s=read(f); info=', '.join(f'{name}():L{line}' for name,line in methods(s))
                routes=', '.join(re.findall(r'^@page "([^"]+)"',s,re.M))
                out.append(f'{f} | componente/clase: {Path(f).stem} | rutas: {routes or "no @page"} | funciones: {info or "markup/configuración"}')
            else:out.append(f'{f} | recurso estático/configuración; sin controlador HTTP propio')
    out+=['\nC. ARCHIVOS BACKEND Y SUS FUNCIONES']
    for f in files:
        if f.startswith('api_innovacion/') and f.endswith('.cs'):
            s=read(f);out.append(f'{f} | '+(', '.join(f'{name}():L{line}' for name,line in methods(s)) or 'Contrato, modelo o configuración; ver declaraciones en el archivo'))
    out+=['\nD. MODELOS Y TABLAS DE BASE DE DATOS']
    sql=read('db/postgres/001_esquema.sql')
    for m in re.finditer(r'CREATE TABLE (\w+) \((.*?)\n\);',sql,re.S):
        out += [f'TABLA: {m[1]} | db/postgres/001_esquema.sql:L{line_at(sql,m.start())}',m[2].strip(),source_url(ref,'db/postgres/001_esquema.sql',line_at(sql,m.start()))]
    out+=['Historial técnico: astra_migraciones; declaración en api_innovacion/Infraestructura/MigradorPostgres.cs. Modelo completo: '+SPEC+'/5_data_model.md.',
          '\nE. AUTENTICACIÓN Y PERMISOS',
          'API: Seguridad/GeneradorTokenJwt.cs, HasheadorClavesPbkdf2.cs, PermisoPorVerbo.cs, Roles.cs; registro en Ensamblador/EnsambladorSeguridad.cs.',
          'GET/HEAD: consulta, editor, administrador. POST/PUT/PATCH: editor, administrador. DELETE: administrador. Otros verbos no autorizados por la política MVC.',
          'usuario/rol/rol_usuario: administrador adicional. Login anónimo; diagnóstico, health y Swagger públicos. Frontend: cookie HttpOnly y ticket/JWT en memoria; Secure en Render; antifalsificación en POST /sesion/salir.',
          '\nF. VARIABLES DE ENTORNO (SOLO NOMBRES)']
    envs={'BaseDatos__Proveedor','ConnectionStrings__Postgres','ConnectionStrings__SqlServer','Jwt__Secreto','Jwt__Minutos','Jwt__Emisor','Jwt__Audiencia','AdminInicial__Usuario','AdminInicial__Correo','AdminInicial__Clave','UrlApi','Cors__Origenes__0','PORT','RENDER','ASPNETCORE_ENVIRONMENT'}
    for f in files:
        if f.endswith('.example') or f.endswith(('.yml','.yaml')):
            s=read(f)
            envs.update(re.findall(r'^([A-Za-z_][A-Za-z0-9_]*)=',s,re.M))
            envs.update(re.findall(r'\$\{([A-Za-z_][A-Za-z0-9_]*)',s))
        if f.endswith('.py') and f.startswith('pruebas_humo/'):
            envs.update(re.findall(r'os\.environ(?:\.get\(|\[)["\']([^"\']+)',read(f)))
    out+=sorted(envs)
    out+=['\nG. TODOS LOS ARCHIVOS .MD Y SU PROPÓSITO']
    for f in files:
        if f.lower().endswith('.md') and (ROOT/f).exists():
            s=read(f);title=next((x.lstrip('# ') for x in s.splitlines() if x.startswith('#')),Path(f).stem)
            out.append(f'{f} | {len(s.splitlines())} líneas | {title}')
    out+=['\nH. DOCUMENTACIÓN SPEC KIT V2',SPEC+'/','Constitución: docs/spec_kit/1_constitution.md (1.6.0 en revisión); versiones históricas preservadas.',
          '2_spec: requisitos y estados; 3_plan: arquitectura; 4_research: decisiones; 5_data_model: 25 tablas; 6_contracts + openapi.local.json: API real; 7_quickstart: operación; 8_tasks: estado; 9_checklist: aceptación.',
          '\nI. PRUEBAS Y RESULTADOS','Ver docs/VALIDACION_NEON_RENDER_V2.md: distingue capas, HTTP local, migraciones, compilación y pruebas pendientes públicas. No atribuir pruebas locales a Neon/Render.',
          '\nJ. DESPLIEGUE EN NEON Y RENDER','DESPLIEGUE_NEON_RENDER.md; render.yaml; api_innovacion/Dockerfile.render; front_blazor/Dockerfile.render; publicacion/.env.neon.example; db/postgres.',
          'Sin URLs públicas verificadas de esta revisión. No se ha modificado la producción MonsterASP ni el servicio inventario de Render.',
          '\nK. ERRORES CONOCIDOS Y PENDIENTES','Neon: conector sin acceso operativo. Render: intento de blueprint rechazado por acceso al repositorio privado/branch/file (HTTP 400).',
          'Pendientes: vincular Neon y autorizar el repositorio en Render; provisionar recursos nuevos gratuitos, aplicar migraciones, verificar HTTPS público y flujos con base real; interacción Blazor en navegador y comportamiento WebSocket público.',
          'Sesiones en memoria y claves DataProtection efímeras: reinicios cierran sesiones. No hay pruebas de carga ni SLA. No se transfirieron datos reales.',
          'Continuidad: ASTRA_ESTADO_TRABAJO.md y ASTRA_REANUDAR.txt. Verificar Git/Neon/Render antes de continuar; no duplicar operaciones ni sobrescribir cambios.']
    (ROOT/'MAPA_COMPLETO_PROYECTO_V2.txt').write_text('\n'.join(out)+'\n',encoding='utf-8')
    print(json.dumps({'operaciones_api':len(rows),'archivos_inventariados':len(files),'swagger_coincide':verified,'referencia':ref},ensure_ascii=False))

if __name__=='__main__':main()
