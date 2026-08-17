using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Propietarios.Commands
{
    public record CambiarEstadoPropietarioCommand(Guid Id) : IRequest<OperacionResultadoDto>;

    public class CambiarEstadoPropietarioCommandHandler : IRequestHandler<CambiarEstadoPropietarioCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoPropietarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CambiarEstadoPropietarioCommand request, CancellationToken cancellationToken)
        {
            var propietario = await _context.Propietarios.FindAsync(new object[] { request.Id }, cancellationToken);

            if (propietario == null)
            {
                return OperacionResultadoDto.Fallo("El sujeto de derecho especificado no existe en el sistema.");
            }

            propietario.EstadoActivo = !propietario.EstadoActivo;

            await _context.SaveChangesAsync(cancellationToken);

            string accion = propietario.EstadoActivo ? "activado" : "desactivado";
            return OperacionResultadoDto.Exito($"El propietario ha sido {accion} exitosamente.");
        }
    }
}
