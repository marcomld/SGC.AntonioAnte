using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Client.Services.Catastro.Contracts
{
    public interface IPredioService
    {
        Task<ResultadoPaginadoDto<PredioDto>> ObtenerPaginadoAsync(string? busqueda, bool? estadoActivo, TipoPredio? tipoPredio, int pagina, int registrosPorPagina);
        Task<PredioDto?> ObtenerPorIdAsync(Guid id);
        Task<OperacionResultadoDto> GuardarFichaCompletaAsync(CreateFichaCatastralDto dto);
        Task<OperacionResultadoDto> ActualizarFichaCompletaAsync(Guid id, UpdateFichaCatastralDto dto);
        Task<OperacionResultadoDto> CambiarEstadoAsync(Guid id);
    }
}
