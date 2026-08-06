using FluentValidation;
using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.CreateDepartamento
{
    // COMMAND
    public class CreateDepartamentoCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    // VALIDATOR
    public class CreateDepartamentoCommandValidator : AbstractValidator<CreateDepartamentoCommand>
    {
        public CreateDepartamentoCommandValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre del departamento es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");
        }
    }

    // HANDLER
    public class CreateDepartamentoCommandHandler : IRequestHandler<CreateDepartamentoCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateDepartamentoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var nuevo = new Departamento
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                EstadoActivo = true
            };

            _context.Departamentos.Add(nuevo);

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "CREAR_DEPARTAMENTO",
                Entidad = "Departamento",
                EntidadId = nuevo.Id.ToString(),
                DatosAdicionales = $"Departamento creado: '{nuevo.Nombre}'",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return nuevo.Id;
        }
    }
}
