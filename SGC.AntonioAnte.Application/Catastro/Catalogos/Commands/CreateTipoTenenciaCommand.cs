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
    public record CreateTipoTenenciaCommand(string Nombre, string Descripcion) : IRequest<Guid>;

    public class CrearTipoTenenciaCommandValidator : AbstractValidator<CreateTipoTenenciaCommand>
    {
        public CrearTipoTenenciaCommandValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre de la tenencia es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");
        }
    }

    public class CrearTipoTenenciaCommandHandler : IRequestHandler<CreateTipoTenenciaCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CrearTipoTenenciaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateTipoTenenciaCommand request, CancellationToken cancellationToken)
        {
            var nuevo = TipoTenencia.Crear(request.Nombre, request.Descripcion);
            _context.TiposTenencia.Add(nuevo);
            await _context.SaveChangesAsync(cancellationToken);
            return nuevo.Id;
        }
    }
}
