using SGC.AntonioAnte.Shared.DTOs.Catastro.Predios;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Client.Services.Catastro.Contracts
{
    public interface IPredioService
    {
        Task<ResultadoPaginadoDto<PredioDto>> ObtenerPaginadoAsync(string? busqueda, int pagina, int registrosPorPagina);
        Task<PredioDto?> ObtenerPorIdAsync(Guid id);
        Task<OperacionResultadoDto> CrearPredioBaseAsync(CreatePredioDto dto);
        Task<OperacionResultadoDto> AgregarDominiosAsync(Guid predioId, List<AddDominioDto> dominios);
        Task<OperacionResultadoDto> AgregarBloquesAsync(Guid predioId, List<AddBloqueDto> bloques);
    }
}
