using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Departamentos;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IDepartamentoService
    {
        Task<ResultadoPaginadoDto<DepartamentoDto>?> ObtenerPaginadoAsync(string? busqueda, bool? estadoActivo, int pagina, int registrosPorPagina);
        Task<List<DepartamentoDto>?> ObtenerTodosAsync();
        Task<OperacionResultadoDto> CrearAsync(CreateDepartamentoDto dto);
        Task<OperacionResultadoDto> ActualizarAsync(Guid id, CreateDepartamentoDto dto);
        Task<OperacionResultadoDto> CambiarEstadoAsync(Guid id);
        Task<OperacionResultadoDto> EliminarAsync(Guid id);
    }
}
