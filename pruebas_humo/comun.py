"""Lo que comparten las pruebas de humo de la API (v2 en adelante).

Cada bloque tiene su propio guion —humo_academico.py, humo_actividades.py,
humo_puentes.py— para que los tres estudiantes no editen el mismo archivo.
Aquí vive solo lo común: cómo llamar a la API y cómo llevar la cuenta de lo
que falló.

Las pruebas de humo hablan con la API REAL, levantada con
`docker compose up -d --build`. Son el complemento de la prueba de capas: esa
prueba los servicios sin base de datos; estas, el sistema completo.
"""
import json
import random
import sys
import urllib.error
import urllib.request

API = "http://localhost:8072"

_fallos = []
_comprobaciones = 0


def api(metodo, ruta, cuerpo=None):
    """Llama a la API y devuelve (código, cuerpo como dict o None)."""
    datos = json.dumps(cuerpo).encode("utf-8") if cuerpo is not None else None
    peticion = urllib.request.Request(API + ruta, data=datos, method=metodo)
    peticion.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(peticion, timeout=30) as r:
            texto = r.read().decode("utf-8")
            return r.status, (json.loads(texto) if texto else None)
    except urllib.error.HTTPError as e:
        texto = e.read().decode("utf-8", "replace")
        try:
            return e.code, (json.loads(texto) if texto else None)
        except ValueError:
            return e.code, {"texto": texto}
    except Exception as e:  # la API no responde
        return 0, {"error": str(e)}


def revisar(condicion, descripcion, detalle=None):
    """La línea que puede fallar: comprueba, narra y deja constancia."""
    global _comprobaciones
    _comprobaciones += 1
    if condicion:
        print(f"  [OK] {descripcion}")
    else:
        print(f"  [ERROR] {descripcion}" + (f"  ->  {detalle}" if detalle is not None else ""))
        _fallos.append(descripcion)


def esperar(codigo_esperado, metodo, ruta, cuerpo=None, descripcion=None):
    """Llama y comprueba el código de estado. Devuelve el cuerpo."""
    codigo, respuesta = api(metodo, ruta, cuerpo)
    revisar(codigo == codigo_esperado,
            descripcion or f"{metodo} {ruta} -> {codigo_esperado}",
            f"llegó {codigo}: {respuesta}")
    return respuesta


def llave_aleatoria(prefijo=7):
    """Una llave distinta en cada corrida: si una se interrumpe a la mitad deja
    registros puestos, y la siguiente fallaría por llave duplicada."""
    return int(f"{prefijo}{random.randint(10000, 99999)}")


def terminar(nombre):
    print()
    if _fallos:
        print(f"=== {nombre}: {len(_fallos)} de {_comprobaciones} comprobaciones FALLARON ===")
        for f in _fallos:
            print(f"   - {f}")
        sys.exit(1)
    print(f"=== {nombre}: las {_comprobaciones} comprobaciones pasaron ===")
    sys.exit(0)
