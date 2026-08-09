using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands
{
    // 1. COMMAND
    public record UpdateDepartamentoCommand(Guid Id, string Nombre, string Descripcion) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation)
    public class UpdateDepartamentoCommandValidator : AbstractValidator<UpdateDepartamentoCommand>
    {
        public UpdateDepartamentoCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El identificador del departamento es obligatorio.");

            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre del departamento es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");

            RuleFor(v => v.Descripcion)
                .MaximumLength(250).WithMessage("La descripción no debe superar los 250 caracteres.");
        }
    }

    // 3. HANDLER
    public class UpdateDepartamentoCommandHandler : IRequestHandler<UpdateDepartamentoCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdateDepartamentoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(UpdateDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null)
            {
                return OperacionResultadoDto.Fallo("Departamento no encontrado.");
            }

            // 🛡️ Control de Nombres Duplicados (excluyendo el departamento actual)
            var nombreNorm = request.Nombre.Trim().ToLower();
            var existeDuplicado = await _context.Departamentos
                .AsNoTracking()
                .AnyAsync(d => d.Id != request.Id && d.Nombre.ToLower() == nombreNorm, cancellationToken);

            if (existeDuplicado)
            {
                return OperacionResultadoDto.Fallo($"Ya existe otro departamento registrado con el nombre '{request.Nombre.Trim()}'.");
            }

            dep.Nombre = request.Nombre.Trim();
            dep.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();

            // 🔹 El DbContext calcula automáticamente las diferencias y audita ACTUALIZAR_DEPARTAMENTO
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Departamento actualizado correctamente.");
        }
    }
}