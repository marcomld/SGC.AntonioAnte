using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Client.Services.Avaluos.Contracts
{
    public interface IAvaluosService
    {
        Task<(bool Exitoso, string Mensaje, AvaluoSimuladoDto? Datos)> SimularAvaluoAsync(Guid predioId, int anioFiscal);
        Task<(bool Existe, string Mensaje, LiquidacionVigenteDto? Datos)> ObtenerLiquidacionVigenteAsync(Guid predioId, int anioFiscal);
        Task<OperacionResultadoDto> EmitirLiquidacionAsync(EmitirLiquidacionDto dto);
        Task<OperacionResultadoDto> ReliquidarAvaluoAsync(ReliquidarLiquidacionDto dto);
    }
}