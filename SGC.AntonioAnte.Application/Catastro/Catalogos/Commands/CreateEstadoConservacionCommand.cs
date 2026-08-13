using FluentValidation;
using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Catalogos.Commands
{
    public record CreateEstadoConservacionCommand(string Nombre, string Descripcion) : IRequest<Guid>;

    public class CrearEstadoConservacionCommandValidator : AbstractValidator<CreateEstadoConservacionCommand>
    {
        public CrearEstadoConservacionCommandValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre del estado de conservación es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");
        }
    }

    public class CrearEstadoConservacionCommandHandler : IRequestHandler<CreateEstadoConservacionCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CrearEstadoConservacionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateEstadoConservacionCommand request, CancellationToken cancellationToken)
        {
            var nuevo = EstadoConservacion.Crear(request.Nombre, request.Descripcion);
            _context.EstadosConservacion.Add(nuevo);
            await _context.SaveChangesAsync(cancellationToken);
            return nuevo.Id;
        }
    }
}
