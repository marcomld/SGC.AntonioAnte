using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Avaluos.PreciosBaseSuelo.Commands
{
    public record CambiarEstadoPrecioBaseSueloCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    public class CambiarEstadoPrecioBaseSueloCommandHandler : IRequestHandler<CambiarEstadoPrecioBaseSueloCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoPrecioBaseSueloCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoPrecioBaseSueloCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _context.PreciosBaseSuelo.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entidad == null)
                return OperacionResultadoDto.Fallo("Registro no encontrado.");

            entidad.EstadoActivo = !entidad.EstadoActivo;
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"El precio base fue {(entidad.EstadoActivo ? "activado" : "desactivado")} exitosamente.");
        }
    }
}