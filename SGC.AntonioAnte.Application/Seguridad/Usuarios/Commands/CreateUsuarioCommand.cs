using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands
{
    // 1. COMMAND (record posicional e inmutable)
    public record CreateUsuarioCommand(
        string Identificacion,
        string Nombres,
        string Apellidos,
        string Email,
        Guid? DepartamentoId,
        string Password,
        string RolAsignado
    ) : IRequest<OperacionResultadoDto>;

    // 2. VALIDATOR (FluentValidation + Módulo 10 Cédula Ecuador)
    public class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
    {
        public CreateUsuarioCommandValidator()
        {
            RuleFor(x => x.Identificacion)
                .NotEmpty().WithMessage("La identificación (Cédula) es totalmente obligatoria.")
                .Length(10).WithMessage("La cédula ecuatoriana debe contener exactamente 10 dígitos.")
                .Must(ValidarCedulaEcuatoriana).WithMessage("La identificación ingresada no es una cédula válida para la República del Ecuador.");

            RuleFor(x => x.Nombres)
                .NotEmpty().WithMessage("Los nombres completos del funcionario son obligatorios.")
                .MaximumLength(150).WithMessage("Los nombres no pueden exceder los 150 caracteres.");

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Los apellidos completos del funcionario son obligatorios.")
                .MaximumLength(150).WithMessage("Los apellidos no pueden exceder los 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico institucional es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");

            RuleFor(x => x.RolAsignado)
                .NotEmpty().WithMessage("Debe asignar un rol base al funcionario.");
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10) return false;
            if (!long.TryParse(cedula, out _)) return false;

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24) return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito < 0 || tercerDigito > 5) return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula.Substring(i, 1)) * coeficientes[i];
                if (valor > 9) valor -= 9;
                suma += valor;
            }

            int digitoVerificador = int.Parse(cedula.Substring(9, 1));
            int decenaSuperior = ((suma + 9) / 10) * 10;
            int resultado = decenaSuperior - suma;

            if (resultado == 10) resultado = 0;

            return resultado == digitoVerificador;
        }
    }

    // 3. HANDLER
    public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, OperacionResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;

        public CreateUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            RoleManager<Rol> roleManager,
            IApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<OperacionResultadoDto> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
        {
            var rolTrim = request.RolAsignado.Trim();

            // 1. Validar que el rol exista en la base de datos
            var roleExists = await _roleManager.RoleExistsAsync(rolTrim);
            if (!roleExists)
            {
                return OperacionResultadoDto.Fallo($"El rol '{rolTrim}' no está registrado en el sistema.");
            }

            // 2. Validar y cargar el objeto Departamento desde EF Core
            Departamento? departamentoObj = null;
            if (request.DepartamentoId.HasValue && request.DepartamentoId != Guid.Empty)
            {
                departamentoObj = await _context.Departamentos
                    .FirstOrDefaultAsync(d => d.Id == request.DepartamentoId.Value, cancellationToken);

                if (departamentoObj == null)
                {
                    return OperacionResultadoDto.Fallo("El departamento municipal seleccionado no existe.");
                }
            }

            // 3. Mapear la entidad de dominio asociando el departamento cargado
            var cedulaTrim = request.Identificacion.Trim();
            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                UserName = cedulaTrim,
                Identificacion = cedulaTrim,
                Nombres = request.Nombres.Trim(),
                Apellidos = request.Apellidos.Trim(),
                Email = request.Email.Trim(),
                DepartamentoId = departamentoObj?.Id,
                Departamento = departamentoObj, // 🔹 Asignamos el objeto para que el interceptor de auditoría pueda leer su Nombre
                EstadoActivo = true,
                FechaCreacion = DateTime.UtcNow
            };

            // 4. Crear el usuario en ASP.NET Core Identity
            var result = await _userManager.CreateAsync(nuevoUsuario, request.Password);

            if (!result.Succeeded)
            {
                var errorBase = result.Errors.FirstOrDefault();
                string mensajeTraducido = errorBase?.Code switch
                {
                    "DuplicateUserName" => "La identificación (Cédula) ingresada ya pertenece a un funcionario registrado.",
                    "DuplicateEmail" => "El correo electrónico institucional ya se encuentra registrado.",
                    "PasswordTooShort" => "La contraseña provista no cumple con la longitud mínima de 8 caracteres.",
                    "PasswordRequiresNonAlphanumeric" => "La contraseña debe contener al menos un carácter especial (!, @, #, etc.).",
                    "PasswordRequiresDigit" => "La contraseña debe contener al menos un número (0-9).",
                    "PasswordRequiresUpper" => "La contraseña debe contener al menos una letra mayúscula (A-Z).",
                    "PasswordRequiresLower" => "La contraseña debe contener al menos una letra minúscula (a-z).",
                    _ => errorBase?.Description ?? "No se pudo procesar el alta del funcionario en la base de datos."
                };

                return OperacionResultadoDto.Fallo(mensajeTraducido);
            }

            // 5. Asignar el rol inicial al usuario
            await _userManager.AddToRoleAsync(nuevoUsuario, rolTrim);

            return OperacionResultadoDto.Exito($"Funcionario '{nuevoUsuario.Nombres} {nuevoUsuario.Apellidos}' registrado exitosamente con el rol '{rolTrim}'.");
        }
    }
}