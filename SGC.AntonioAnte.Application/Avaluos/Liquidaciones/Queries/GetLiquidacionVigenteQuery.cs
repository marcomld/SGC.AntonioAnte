using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Avaluos.Liquidaciones;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.Liquidaciones.Queries
{
    public record GetLiquidacionVigenteQuery(Guid PredioId, int AnioFiscal) : IRequest<OperacionResultadoDto>;

    public class GetLiquidacionVigenteQueryHandler : IRequestHandler<GetLiquidacionVigenteQuery, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public GetLiquidacionVigenteQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(GetLiquidacionVigenteQuery request, CancellationToken cancellationToken)
        {
            var liquidacion = await _context.LiquidacionesPrediales
                .AsNoTracking()
                .Where(l => l.PredioId == request.PredioId &&
                            l.AnioFiscal == request.AnioFiscal &&
                            l.EsSimulacion == false &&
                            l.EstadoLiquidacion == "Emitido")
                .Select(l => new LiquidacionVigenteDto
                {
                    Id = l.Id,
                    PredioId = l.PredioId,
                    AnioFiscal = l.AnioFiscal,
                    ValorTerreno = l.ValorTerreno,
                    ValorConstruccion = l.ValorConstruccion,
                    BaseImponible = l.BaseImponible,
                    ValorEmision = l.ValorEmision,
                    EstadoLiquidacion = l.EstadoLiquidacion,
                    FechaEmision = l.FechaCreacion // Heredado de AuditableEntity
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (liquidacion == null)
                return OperacionResultadoDto.Fallo("No existe una liquidación oficial vigente para este año fiscal.");

            return OperacionResultadoDto.Exito("Liquidación vigente recuperada.", null, liquidacion);
        }
    }
}