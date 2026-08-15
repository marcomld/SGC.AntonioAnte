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
    public record CreateTipoEstructuraCommand(string Nombre, string Descripcion) : IRequest<Guid>;

    public class CrearTipoEstructuraCommandValidator : AbstractValidator<CreateTipoEstructuraCommand>
    {
        public CrearTipoEstructuraCommandValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre de la estructura es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");
        }
    }

    public class CrearTipoEstructuraCommandHandler : IRequestHandler<CreateTipoEstructuraCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CrearTipoEstructuraCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateTipoEstructuraCommand request, CancellationToken cancellationToken)
        {
            var nuevo = TipoEstructura.Crear(request.Nombre, request.Descripcion);
            _context.TiposEstructura.Add(nuevo);
            await _context.SaveChangesAsync(cancellationToken);
            return nuevo.Id;
        }
    }
}
