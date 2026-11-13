# Estado persistente de la misión

Actualizado: 2026-11-13 (America/Bogota). Misión EN CURSO; no hay despliegue Neon/Render verificado.

## Objetivo
Desplegar la aplicación existente en Neon y Render sin alterar producción ni trabajo colaborativo; probarla, ampliar Spec Kit y entregar MAPA_COMPLETO_PROYECTO_V2.txt.

## Punto de control
- Fase 1 completada: acceso GitHub WRITE comprobado con la sesión autenticada; clon nuevo y limpio.
- Repositorio: https://github.com/MrEpia/innovacioncurricular (privado).
- Copia: `work/innovacioncurricular` dentro de este chat.
- Rama activa: `deploy/neon-render-spec-kit-v2`.
- Último commit: `24abf0491b5f29aaf0ec346a489570b58140aafa` (main al clonar).
- Fase actual: 2, auditoría antes de modificar código.
- Arquitectura detectada: C# ASP.NET Core 10 + Dapper + SQL Server, Blazor Server separado. Publicación MonsterASP existente: preservar.
- Spec Kit existente: constitución y versiones V1, V2, V3, V4; conservar y agregar una revisión de despliegue compatible.
- Cambios: solo estos archivos de continuidad; ningún cambio previo del equipo en esta copia.
- Comandos: gh repo view; git ls-remote; git clone; git status --short; git branch --all; git log -12; git switch -c deploy/neon-render-spec-kit-v2; lectura de README, Program.cs, csproj, compose y documentación inicial.
- Pruebas: todavía sin ejecutar; acceso al repositorio verificado.
- Neon: credenciales de entorno no detectadas; pendiente revisar sesión disponible.
- Render: ventana Chrome titulada Render Dashboard; pendiente inspección, no se ha modificado.
- Error/riesgo conocido: SQL Server no es PostgreSQL; se requiere compatibilidad real de consultas y migraciones antes de Neon.

## Próxima acción exacta
Completar auditoría de repositorios SQL, seguridad, migraciones, controladores y Markdown; comprobar SDK/Docker y sesiones Neon/Render. Preparar soporte PostgreSQL conservando SQL Server.

## Continuidad
Leer primero este archivo y ASTRA_REANUDAR.txt. Verificar Git, Neon y Render antes de ejecutar cambios. No duplicar recursos ni migraciones. Actualizar este registro tras cada fase y antes de interrupciones. No almacenar secretos aquí.

La sesión permite continuar mientras esté disponible. No se ha confirmado un mecanismo de reactivación automática después de agotar cuota; no se alterarán límites ni se crearán bucles externos. Si se detiene, escribir CONTINUAR en el mismo chat.

## Fase 2 — Auditoría completada
Informe: docs/AUDITORIA_NEON_RENDER_V2.md. Inventario inicial: 91 Markdown versionados; arquitectura, SQL, seguridad y pruebas inspeccionados. Docker operativo y SDK 10 disponible en imagen; no hay dotnet local. Próxima acción: preparar repositorios PostgreSQL paralelos y migraciones en una base Docker nueva. Render accesible vía conector (sin servicios de este proyecto). Neon devuelve Unknown tool; revalidar antes de provisionar. Archivos modificados: continuidad e informe de auditoría. Pruebas: no ejecutadas aún.
