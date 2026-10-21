using SGC.AntonioAnte.Shared.DTOs.Avaluos.Simulacion;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Client.Services.Avaluos.Contracts
{
    public interface IAvaluosService
    {
        Task<(bool Exitoso, string Mensaje, AvaluoSimuladoDto? Datos)> SimularAvaluoAsync(Guid predioId, int anioFiscal);
    }
}