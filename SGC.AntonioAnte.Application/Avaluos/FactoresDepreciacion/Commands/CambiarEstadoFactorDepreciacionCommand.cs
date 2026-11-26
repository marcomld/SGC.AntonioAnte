using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.FactoresDepreciacion.Commands
{
    public record CambiarEstadoFactorDepreciacionCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    public class CambiarEstadoFactorDepreciacionCommandHandler : IRequestHandler<CambiarEstadoFactorDepreciacionCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoFactorDepreciacionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoFactorDepreciacionCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.FactoresDepreciacion.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("Registro no encontrado.");

            entidad.EstadoActivo = !entidad.EstadoActivo;
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"El factor fue {(entidad.EstadoActivo ? "activado" : "desactivado")} exitosamente.");
        }
    }
}
