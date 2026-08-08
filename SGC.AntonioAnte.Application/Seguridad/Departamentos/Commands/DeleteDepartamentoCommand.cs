using FluentValidation;
using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands
{
    // 1. COMMAND
    public class DeleteDepartamentoCommand : IRequest<OperacionResultadoDto>
    {
        public Guid Id { get; set; }
    }

    // 2. VALIDATOR (FluentValidation)
    public class DeleteDepartamentoCommandValidator : AbstractValidator<DeleteDepartamentoCommand>
    {
        public DeleteDepartamentoCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El identificador del departamento es obligatorio.");
        }
    }

    // 3. HANDLER
    public class DeleteDepartamentoCommandHandler : IRequestHandler<DeleteDepartamentoCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public DeleteDepartamentoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(DeleteDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null)
            {
                return OperacionResultadoDto.Fallo("Departamento no encontrado.");
            }

            string nombreDep = dep.Nombre;
            _context.Departamentos.Remove(dep);

            // 🔹 El DbContext intercepta el borrado y audita ELIMINAR_DEPARTAMENTO automáticamente
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito($"El departamento '{nombreDep}' fue eliminado permanentemente.");
        }
    }
}