using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Predios.Commands
{
    public record AddDominioCommand(
        Guid PredioId,
        Guid PropietarioId,
        Guid TipoTenenciaId,
        decimal PorcentajePropiedad,
        DateTime? FechaInscripcion,
        string? Notaria
    ) : IRequest<Guid>;

    public class AddDominioCommandValidator : AbstractValidator<AddDominioCommand>
    {
        public AddDominioCommandValidator()
        {
            RuleFor(v => v.PredioId).NotEmpty().WithMessage("El ID del predio es obligatorio.");
            RuleFor(v => v.PropietarioId).NotEmpty().WithMessage("El ID del propietario es obligatorio.");
            RuleFor(v => v.TipoTenenciaId).NotEmpty().WithMessage("El ID del tipo de tenencia es obligatorio.");
            RuleFor(v => v.PorcentajePropiedad)
                .InclusiveBetween(0.01m, 100.00m).WithMessage("El porcentaje de propiedad debe estar entre 0.01% y 100.00%.");
        }
    }

    public class AddDominioCommandHandler : IRequestHandler<AddDominioCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public AddDominioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(AddDominioCommand request, CancellationToken cancellationToken)
        {
            var predio = await _context.Predios.FindAsync(new object[] { request.PredioId }, cancellationToken);
            if (predio == null || !predio.EstadoActivo)
                throw new Exception("El predio especificado no existe o se encuentra inactivo.");

            var propietario = await _context.Propietarios.FindAsync(new object[] { request.PropietarioId }, cancellationToken);
            if (propietario == null || !propietario.EstadoActivo)
                throw new Exception("El propietario especificado no existe o se encuentra inactivo.");

            var tipoTenencia = await _context.TiposTenencia.FindAsync(new object[] { request.TipoTenenciaId }, cancellationToken);
            if (tipoTenencia == null || !tipoTenencia.EstadoActivo)
                throw new Exception("El tipo de tenencia especificado no existe o se encuentra inactivo.");

            // CRITERIO DE ACEPTACIÓN HU-CAT-03: Suma total <= 100.00%
            var sumaActual = await _context.Dominios
                .Where(d => d.PredioId == request.PredioId && d.EstadoActivo)
                .SumAsync(d => d.PorcentajePropiedad, cancellationToken);

            if (sumaActual + request.PorcentajePropiedad > 100.00m)
            {
                throw new Exception($"Operación rechazada. La suma del porcentaje asignado ({request.PorcentajePropiedad}%) más el porcentaje acumulado actual ({sumaActual}%) excede el límite legal del 100.00%.");
            }

            var nuevoDominio = Dominio.Crear(
                request.PredioId,
                request.PropietarioId,
                request.TipoTenenciaId,
                request.PorcentajePropiedad,
                request.FechaInscripcion,
                request.Notaria
            );

            _context.Dominios.Add(nuevoDominio);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoDominio.Id;
        }
    }
}
