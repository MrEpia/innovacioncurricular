using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// v4 — La capa 2 del dashboard. El motor ya agrupó; aquí van las reglas de
/// presentación que no dependen de la base: un gráfico con veinte barras no se
/// lee, así que los graduados muestran las facultades principales y agrupan
/// el resto en «Otras».
/// </summary>
public class ServicioDashboard : IServicioDashboard
{
    private readonly IRepositorioDashboard _repositorio;

    public ServicioDashboard(IRepositorioDashboard repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<ResumenDashboard> Resumen() => _repositorio.Resumen();
    public async Task<List<DatoGrafico>> ProgramasPorNivel() => (await _repositorio.ProgramasPorNivel()).ToList();
    public async Task<List<DatoGrafico>> ProgramasPorSede() => (await _repositorio.ProgramasPorSede()).ToList();
    public async Task<List<DatoGrafico>> AcreditacionesPorCalificacion() => (await _repositorio.AcreditacionesPorCalificacion()).ToList();
    public async Task<List<DatoGrafico>> RegistrosPorMetodologia() => (await _repositorio.RegistrosPorMetodologia()).ToList();
    public async Task<List<DatoGrafico>> DocentesPorDedicacion() => (await _repositorio.DocentesPorDedicacion()).ToList();

    public async Task<List<DatoGrafico>> GraduadosPorFacultad(int maximo)
    {
        if (maximo < 1)
        {
            throw new ArgumentException("El parámetro maximo debe ser un número mayor a 0.");
        }

        var todos = (await _repositorio.GraduadosPorFacultad()).OrderByDescending(d => d.Valor).ToList();
        if (todos.Count <= maximo)
        {
            return todos;
        }

        var principales = todos.Take(maximo).ToList();
        principales.Add(new DatoGrafico { Etiqueta = "Otras", Valor = todos.Skip(maximo).Sum(d => d.Valor) });
        return principales;
    }
}
