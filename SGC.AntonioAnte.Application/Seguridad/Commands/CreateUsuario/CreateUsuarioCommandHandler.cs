using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.CreateUsuario
{
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
                Departamento = request.Departamento,
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
                UsuarioId = nuevoUsuario.Id,
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
