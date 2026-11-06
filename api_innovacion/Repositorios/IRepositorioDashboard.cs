using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>v4 — Los datos AGREGADOS del dashboard: conteos y sumas, no filas.</summary>
public interface IRepositorioDashboard
{
    Task<ResumenDashboard> Resumen();
    Task<IEnumerable<DatoGrafico>> ProgramasPorNivel();
    Task<IEnumerable<DatoGrafico>> ProgramasPorSede();
    Task<IEnumerable<DatoGrafico>> AcreditacionesPorCalificacion();
    Task<IEnumerable<DatoGrafico>> RegistrosPorMetodologia();
    Task<IEnumerable<DatoGrafico>> DocentesPorDedicacion();
    Task<IEnumerable<DatoGrafico>> GraduadosPorFacultad();
}
