using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.CreateUsuario
{
    // COMMAND
    public class CreateUsuarioCommand : IRequest<Guid>
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? DepartamentoId { get; set; }

        // La contraseña inicial que se le asignará al funcionario
        public string Password { get; set; } = string.Empty;

        // El nombre del rol base que queremos asignarle (ej. "TecnicoCatastral")
        public string RolAsignado { get; set; } = string.Empty;
    }

    // VALIDATOR
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
                .NotEmpty().WithMessage("Debe asignar un rol al funcionario para el sistema de catastros.");
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

    // HANDLER
    public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, Guid>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            RoleManager<Rol> roleManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que el rol exista en la base de datos antes de continuar
            var roleExists = await _roleManager.RoleExistsAsync(request.RolAsignado);
            if (!roleExists)
            {
                throw new Exception($"El rol '{request.RolAsignado}' no esta registrado en el sistema.");
            }

            // 2. Mapear el comando a nuestra entidad de dominio
            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                UserName = request.Identificacion, // Su username de acceso será la cédula
                Identificacion = request.Identificacion,
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Email = request.Email,
                DepartamentoId = request.DepartamentoId,
                EstadoActivo = true,
                FechaCreacion = DateTime.UtcNow
            };

            // 3. Crear el usuario usando el motor persistente de Identity
            var result = await _userManager.CreateAsync(nuevoUsuario, request.Password);

            if (!result.Succeeded)
            {
                // TAREA 1: Capturamos el primer error de Identity y lo traducimos al español
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

                throw new Exception(mensajeTraducido);
            }

            // 4. Asignar el rol al usuario (Tabla Seguridad.UsuarioRoles)
            await _userManager.AddToRoleAsync(nuevoUsuario, request.RolAsignado);

            // 5. REGISTRO DE AUDITORÍA: El Toque Senior
            var auditoria = new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "REGISTRO_NUEVO_FUNCIONARIO",
                Entidad = "Usuario",
                EntidadId = nuevoUsuario.Id.ToString(),
                DatosAdicionales = $"Funcionario {request.Nombres} {request.Apellidos} registrado con rol {request.RolAsignado}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Auditorias.Add(auditoria);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoUsuario.Id;
        }
    }
}
