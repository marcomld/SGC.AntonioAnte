using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IDepartamentoService
    {
        Task<List<DepartamentoDto>?> ObtenerTodosAsync();
        Task<OperacionResultadoDto> CrearAsync(CreateDepartamentoDto dto);
        Task<OperacionResultadoDto> ActualizarAsync(Guid id, CreateDepartamentoDto dto);
        Task<OperacionResultadoDto> CambiarEstadoAsync(Guid id);
        Task<OperacionResultadoDto> EliminarAsync(Guid id);
    }
}
