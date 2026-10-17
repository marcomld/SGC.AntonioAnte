using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Client.Services.Catastro.Contracts
{
    public interface IPropietarioService
    {
        Task<ResultadoPaginadoDto<PropietarioDto>> ObtenerPaginadoAsync(string? busqueda, bool? estadoActivo, TipoPropietario? tipoPropietario, int pagina, int registrosPorPagina);
        Task<PropietarioDto?> ObtenerPorIdentificacionAsync(string identificacion);
        Task<OperacionResultadoDto> CrearAsync(CreatePropietarioDto dto);
        Task<OperacionResultadoDto> ActualizarAsync(Guid id, UpdatePropietarioDto dto);
        Task<OperacionResultadoDto> CambiarEstadoAsync(Guid id);
    }
}
