# Quickstart — Maestro-detalle

## 1. Arranque

```powershell
docker compose up -d --build
```

Al arrancar, `db/init.sh` corre la migración `v3_003_maestro_detalle.sql`, que
crea los dos procedimientos. No hace falta borrar la base.

## 2. Comprobaciones

```powershell
# Criterio 7: prueba de capas (incluye el maestro-detalle), sin SQL Server
docker compose run --rm --no-deps api-innovacion dotnet run --project pruebas/PruebaCapas.csproj

# Criterios 1 a 6: API real y procedimiento real (49 comprobaciones)
python pruebas_humo/humo_maestro_detalle.py

# Todo el sistema, desde cero
python verificar_todo.py --limpio
```

## 3. Recorrido manual (criterio 8)

1. Ingrese en <http://localhost:8073> como administrador o editor.
2. Menú **Maestro-detalle → Registro calificado con su detalle**.
3. Llene el registro, pulse **«+ Agregar enfoque»** dos veces y
   **«+ Agregar actividad»** una vez, y elija los valores de cada fila.
4. Deje la actividad sin semestre y pulse **«Guardar todo»**: aparece
   «El campo actividades[].semestre es obligatorio.», el formulario sigue lleno
   y no se guardó nada.
5. Complete el semestre y pulse **«Guardar todo»**: aparece el mensaje de éxito
   y, arriba, el registro con sus dos enfoques y su actividad, leídos de la API.
6. Repita con **Programa con su detalle**.

Con el rol `consulta` solo se ve la parte de consultar.

## 4. Problemas conocidos

- Las listas desplegables muestran solo registros **activos**. Si un catálogo
  está vacío (enfoques, prácticas…), primero hay que crear sus registros en su
  pantalla.
- Editar el detalle de un maestro ya creado se hace en las pantallas de
  «Relaciones» (C3 de la spec).
