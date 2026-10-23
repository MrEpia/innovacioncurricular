#!/bin/bash
# ==============================================================
# Inicializador de SQL Server (contenedor sqlserver-init).
#
# SQL Server NO ejecuta los scripts que se le monten: alguien tiene que
# conectarse al motor y correrlos. Ese alguien es este contenedor, que
# hace su trabajo y termina.
#
# Dos pasos:
#   1. La base: si no existe, la crea con innovacion_curricular.sql
#      (25 tablas y catálogos). Si ya existe, NO la toca.
#   2. Las migraciones de db/migraciones/, en orden alfabético, SIEMPRE.
#      Por eso cada una tiene que ser IDEMPOTENTE: correrla dos veces deja
#      la base igual que correrla una. Es lo que permite que un equipo que
#      ya tenía la base de la v1 reciba los cambios de la v2 y la v3 sin
#      borrar su volumen (docker compose down -v).
#
# La contraseña NO está escrita aquí: llega por MSSQL_SA_PASSWORD.
# ==============================================================

set -e

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
SERVER=sqlserver
DB=innovacion_local

echo "[init] ¿Existe ya la base $DB?"
EXISTE=$($SQLCMD -S $SERVER -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -W \
  -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = '$DB'")

if [ "$EXISTE" = "1" ]; then
    echo "[init] Ya existe: no se recrea."
else
    echo "[init] Creando la base $DB..."
    $SQLCMD -S $SERVER -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "CREATE DATABASE $DB"

    echo "[init] Ejecutando innovacion_curricular.sql (25 tablas y los catálogos)..."
    $SQLCMD -S $SERVER -U sa -P "$MSSQL_SA_PASSWORD" -C -d $DB -i /scripts/innovacion_curricular.sql
fi

# -b: si una migración falla, sqlcmd sale con error y el contenedor también,
# así la API —que espera a que este termine BIEN— no arranca a medias.
for migracion in /scripts/migraciones/*.sql; do
    [ -e "$migracion" ] || continue
    echo "[init] Migración: $(basename "$migracion")"
    $SQLCMD -S $SERVER -U sa -P "$MSSQL_SA_PASSWORD" -C -b -d $DB -i "$migracion"
done

echo "[init] Listo."
