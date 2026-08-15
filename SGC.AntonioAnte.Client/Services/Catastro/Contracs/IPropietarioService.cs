using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Client.Services.Catastro.Contracs
{
    public interface IPropietarioService
    {
        Task<ResultadoPaginadoDto<PropietarioDto>> ObtenerPaginadoAsync(string? busqueda, int pagina, int registrosPorPagina);
        Task<PropietarioDto?> ObtenerPorIdentificacionAsync(string identificacion);
        Task<OperacionResultadoDto> CrearAsync(CreatePropietarioDto dto);
        Task<OperacionResultadoDto> ActualizarAsync(System.Guid id, UpdatePropietarioDto dto);
    }
}
