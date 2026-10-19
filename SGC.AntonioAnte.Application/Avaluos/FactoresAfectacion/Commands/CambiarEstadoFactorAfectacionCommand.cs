using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.FactoresAfectacion.Commands
{
    public record CambiarEstadoFactorAfectacionCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    public class CambiarEstadoFactorAfectacionCommandHandler : IRequestHandler<CambiarEstadoFactorAfectacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoFactorAfectacionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoFactorAfectacionCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.FactoresAfectacion.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("Registro no encontrado.");

            entidad.EstadoActivo = !entidad.EstadoActivo;
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"El factor fue {(entidad.EstadoActivo ? "activado" : "desactivado")} exitosamente.");
        }
    }
}