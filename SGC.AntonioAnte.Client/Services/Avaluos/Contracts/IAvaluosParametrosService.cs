using System;
using System.Threading.Tasks;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.CostosReposicion;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresAfectacion;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.FactoresDepreciacion;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.PreciosBaseSuelo;
using SGC.AntonioAnte.Shared.DTOs.Common;

namespace SGC.AntonioAnte.Client.Services.Avaluos.Contracts
{
    public interface IAvaluosParametrosService
    {
        // 1. Costos de Reposición
        Task<ResultadoPaginadoDto<CostoReposicionDto>> ObtenerCostosReposicionPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina);
        Task<OperacionResultadoDto> CrearCostoReposicionAsync(CreateCostoReposicionDto dto);
        Task<OperacionResultadoDto> ActualizarCostoReposicionAsync(Guid id, UpdateCostoReposicionDto dto);
        Task<OperacionResultadoDto> CambiarEstadoCostoReposicionAsync(Guid id);

        // 2. Precios Base de Suelo
        Task<ResultadoPaginadoDto<PrecioBaseSueloDto>> ObtenerPreciosSueloPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina);
        Task<OperacionResultadoDto> CrearPrecioSueloAsync(CreatePrecioBaseSueloDto dto);
        Task<OperacionResultadoDto> ActualizarPrecioSueloAsync(Guid id, UpdatePrecioBaseSueloDto dto);
        Task<OperacionResultadoDto> CambiarEstadoPrecioSueloAsync(Guid id);

        // 3. Factores de Depreciación
        Task<ResultadoPaginadoDto<FactorDepreciacionDto>> ObtenerFactoresDepreciacionPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina);
        Task<OperacionResultadoDto> CrearFactorDepreciacionAsync(CreateFactorDepreciacionDto dto);
        Task<OperacionResultadoDto> ActualizarFactorDepreciacionAsync(Guid id, UpdateFactorDepreciacionDto dto);
        Task<OperacionResultadoDto> CambiarEstadoFactorDepreciacionAsync(Guid id);

        // 4. Factores de Afectación
        Task<ResultadoPaginadoDto<FactorAfectacionDto>> ObtenerFactoresAfectacionPaginadoAsync(int anioFiscal, int pagina, int registrosPorPagina);
        Task<OperacionResultadoDto> CrearFactorAfectacionAsync(CreateFactorAfectacionDto dto);
        Task<OperacionResultadoDto> ActualizarFactorAfectacionAsync(Guid id, UpdateFactorAfectacionDto dto);
        Task<OperacionResultadoDto> CambiarEstadoFactorAfectacionAsync(Guid id);
    }
}