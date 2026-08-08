using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands
{
    // 1. COMMAND
    public class CreateDepartamentoCommand : IRequest<OperacionResultadoDto>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    // 2. VALIDATOR (FluentValidation)
    public class CreateDepartamentoCommandValidator : AbstractValidator<CreateDepartamentoCommand>
    {
        public CreateDepartamentoCommandValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre del departamento es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");

            RuleFor(v => v.Descripcion)
                .MaximumLength(250).WithMessage("La descripción no debe superar los 250 caracteres.");
        }
    }

    // 3. HANDLER
    public class CreateDepartamentoCommandHandler : IRequestHandler<CreateDepartamentoCommand, OperacionResultadoDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateDepartamentoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CreateDepartamentoCommand request, CancellationToken cancellationToken)
        {
            // 🛡️ Validación de Duplicados
            var nombreNorm = request.Nombre.Trim().ToLower();
            var existeDuplicado = await _context.Departamentos
                .AsNoTracking()
                .AnyAsync(d => d.Nombre.ToLower() == nombreNorm, cancellationToken);

            if (existeDuplicado)
            {
                return OperacionResultadoDto.Fallo($"Ya existe un departamento registrado con el nombre '{request.Nombre.Trim()}'.");
            }

            var nuevoDepartamento = new Departamento
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
                EstadoActivo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Departamentos.Add(nuevoDepartamento);

            // 🔹 La auditoría CREAR_DEPARTAMENTO la genera automáticamente nuestro ApplicationDbContext
            await _context.SaveChangesAsync(cancellationToken);

            return OperacionResultadoDto.Exito("Departamento registrado exitosamente.");
        }
    }
}