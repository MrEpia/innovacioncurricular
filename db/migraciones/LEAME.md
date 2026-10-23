# Migraciones

`db/init.sh` las ejecuta **en orden alfabético y en cada arranque**, después
de crear la base (si no existía). Por eso cada script debe ser
**idempotente**: correrlo dos veces deja la base igual que correrlo una
(`IF NOT EXISTS`, `MERGE`, etc.).

Convención de nombre: `vN_NNN_descripcion.sql` — la versión que la necesita,
un consecutivo y qué hace. Así el orden alfabético es el orden de ejecución.

Nunca se edita una migración ya integrada a `main`: si hace falta corregirla,
se agrega otra.
