using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Propietarios.Commands
{
    public record UpdatePropietarioCommand(
        Guid Id,
        string? Nombres,
        string? Apellidos,
        string? RazonSocial,
        EstadoCivil EstadoCivil,
        string? Email,
        string? Telefono
    ) : IRequest<bool>;

    public class UpdatePropietarioCommandHandler : IRequestHandler<UpdatePropietarioCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdatePropietarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdatePropietarioCommand request, CancellationToken cancellationToken)
        {
            var propietario = await _context.Propietarios.FindAsync(new object[] { request.Id }, cancellationToken);

            if (propietario == null || !propietario.EstadoActivo)
            {
                throw new Exception("El propietario especificado no existe o se encuentra inactivo.");
            }

            // Actualización mediante reflexión / propiedades
            if (propietario.TipoPropietario == TipoPropietario.Natural)
            {
                if (string.IsNullOrWhiteSpace(request.Nombres) || string.IsNullOrWhiteSpace(request.Apellidos))
                    throw new Exception("Los nombres y apellidos son obligatorios para personas naturales.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.RazonSocial))
                    throw new Exception("La razón social es obligatoria para personas jurídicas.");
            }

            typeof(Propietario).GetProperty(nameof(Propietario.Nombres))?.SetValue(propietario, request.Nombres);
            typeof(Propietario).GetProperty(nameof(Propietario.Apellidos))?.SetValue(propietario, request.Apellidos);
            typeof(Propietario).GetProperty(nameof(Propietario.RazonSocial))?.SetValue(propietario, request.RazonSocial);
            typeof(Propietario).GetProperty(nameof(Propietario.EstadoCivil))?.SetValue(propietario, request.EstadoCivil);
            typeof(Propietario).GetProperty(nameof(Propietario.Email))?.SetValue(propietario, request.Email);
            typeof(Propietario).GetProperty(nameof(Propietario.Telefono))?.SetValue(propietario, request.Telefono);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
