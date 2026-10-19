using MediatR;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;

namespace SGC.AntonioAnte.Application.Avaluos.Simulacion.Queries
{
    // Quitamos los <> y usamos tu DTO estándar
    public record SimularAvaluoQuery(Guid PredioId, int AnioFiscal) : IRequest<OperacionResultadoDto>;
}