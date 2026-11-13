from pathlib import Path
import subprocess,os,json,time,urllib.request,urllib.error,base64,hmac,hashlib,secrets,uuid
import argparse
parser=argparse.ArgumentParser(description='Comprueba health, errores y TLS en un contenedor desechable; requiere laboratorio ya levantado.')
parser.add_argument('--trabajo',required=True,type=Path)
parser.add_argument('--api-url',default='http://localhost:18072')
parser.add_argument('--front-url',default='http://localhost:18073')
args=parser.parse_args()
root=Path(__file__).resolve().parents[1]
work=args.trabajo.resolve()
if work.is_relative_to(root):
    raise SystemExit('Use --trabajo fuera del repositorio para proteger archivos temporales privados.')
work.mkdir(parents=True,exist_ok=True)
envfile=work/('.env.runtime-'+uuid.uuid4().hex[:8])
key=secrets.token_hex(32)
envfile.write_text('PORT=10000\nASPNETCORE_ENVIRONMENT=Production\nBaseDatos__Proveedor=SqlServer\nJwt__Secreto='+key+'\nConnectionStrings__SqlServer=Server=127.0.0.1,1;Database=inexistente;User Id=invalido;Connect Timeout=1;ConnectRetryCount=0;Encrypt=True\n')
def request(url,method='GET',headers=None):
    try:
        with urllib.request.urlopen(urllib.request.Request(url,method=method,headers=headers or {}),timeout=15) as p:
            return p.status,dict(p.headers),p.read().decode()
    except urllib.error.HTTPError as p:return p.code,dict(p.headers),p.read().decode()
def run(args):return subprocess.run(args,capture_output=True,text=True,check=True).stdout.strip()
container=run(['docker','run','-d','--rm','-p','127.0.0.1::10000','--env-file',str(envfile),'astra-innovacion-api:v2'])
try:
    port=run(['docker','port',container,'10000/tcp']).rsplit(':',1)[1]
    url='http://127.0.0.1:'+port
    for i in range(15):
        try:
            if request(url+'/')[0]==200:break
        except (OSError,TimeoutError):pass
        time.sleep(1)
    code,_,body=request(url+'/health');assert code==503,(code,body)
    print('OK: /health devuelve 503 si la base no responde',flush=True)
    def b64(x):return base64.urlsafe_b64encode(json.dumps(x).encode()).rstrip(b'=').decode()
    token=b64({'alg':'HS256','typ':'JWT'})+'.'+b64({'sub':'1','unique_name':'prueba','role':['administrador'],'iss':'innovacion-api','aud':'innovacion-front','nbf':int(time.time())-60,'exp':int(time.time())+120})
    token+='.'+base64.urlsafe_b64encode(hmac.new(key.encode(),token.encode(),hashlib.sha256).digest()).rstrip(b'=').decode()
    code,_,body=request(url+'/api/universidad',headers={'Authorization':'Bearer '+token})
    data=json.loads(body)
    assert code==500 and 'referencia' in data and '127.0.0.1' not in body and 'SqlException' not in body,(code,body)
    print('OK: un 500 de producción oculta detalles SQL y expone referencia',flush=True)
    code,headers,_=request(url+'/api/universidad',method='OPTIONS',headers={'Origin':'https://origen-no-autorizado.example','Access-Control-Request-Method':'GET'})
    assert not any(k.lower()=='access-control-allow-origin' for k in headers)
    print('OK: CORS no permite un origen no configurado',flush=True)
finally:
    run(['docker','stop',container])
invalid=work/('.env.tls-invalido-'+uuid.uuid4().hex[:8])
invalid.write_text('ASPNETCORE_ENVIRONMENT=Production\nBaseDatos__Proveedor=Postgres\nJwt__Secreto='+key+'\nConnectionStrings__Postgres=Host=no-existe.invalid;Database=prueba;Username=prueba;SSL Mode=Disable\n')
attempt=subprocess.run(['docker','run','--rm','--env-file',str(invalid),'astra-innovacion-api:v2'],capture_output=True,text=True)
assert attempt.returncode!=0 and 'requiere SSL Mode=VerifyFull' in (attempt.stdout+attempt.stderr)
print('OK: producción rechaza PostgreSQL sin verificación TLS',flush=True)
for url in [args.api_url+'/health',args.front_url+'/health',args.front_url+'/Error']:
    assert request(url)[0]==200,url
print('OK: ambos procesos finales y página /Error responden 200',flush=True)
(work/'resultados_runtime.json').write_text(json.dumps({'comprobaciones':5,'resultado':'PASS'},indent=2))
