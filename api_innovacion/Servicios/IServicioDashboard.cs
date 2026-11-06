using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>v4 — Los datos del dashboard, listos para graficar.</summary>
public interface IServicioDashboard
{
    Task<ResumenDashboard> Resumen();
    Task<List<DatoGrafico>> ProgramasPorNivel();
    Task<List<DatoGrafico>> ProgramasPorSede();
    Task<List<DatoGrafico>> AcreditacionesPorCalificacion();
    Task<List<DatoGrafico>> RegistrosPorMetodologia();
    Task<List<DatoGrafico>> DocentesPorDedicacion();

    /// <summary>Las facultades con más graduados; el resto se suma en «Otras».</summary>
    Task<List<DatoGrafico>> GraduadosPorFacultad(int maximo);
}
