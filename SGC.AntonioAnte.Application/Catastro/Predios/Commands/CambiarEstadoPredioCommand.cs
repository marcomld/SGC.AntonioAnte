using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Commands
{
    public record CambiarEstadoPredioCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    public class CambiarEstadoPredioCommandHandler : IRequestHandler<CambiarEstadoPredioCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoPredioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoPredioCommand request, CancellationToken cancellationToken)
        {
            var predio = await _context.Predios.FindAsync(new object[] { request.Id }, cancellationToken);

            if (predio == null)
            {
                return OperacionResultadoDto.Fallo("El predio especificado no existe en el sistema.");
            }

            predio.EstadoActivo = !predio.EstadoActivo;

            await _context.SaveChangesAsync(cancellationToken);

            string accion = predio.EstadoActivo ? "activado" : "desactivado";
            return OperacionResultadoDto.Exito($"El predio ha sido {accion} exitosamente.");
        }
    }
}
