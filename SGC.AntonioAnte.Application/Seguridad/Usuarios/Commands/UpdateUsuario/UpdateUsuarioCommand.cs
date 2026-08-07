using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.UpdateUsuario
{
    public class UpdateUsuarioCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? DepartamentoId { get; set; }
    }

    public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;

        public UpdateUsuarioCommandHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.Id.ToString());
            if (usuario == null) throw new Exception("Funcionario no encontrado.");

            usuario.Nombres = request.Nombres;
            usuario.Apellidos = request.Apellidos;
            usuario.Email = request.Email;
            usuario.DepartamentoId = request.DepartamentoId;

            // UserManager.UpdateAsync ejecuta internamente SaveChangesAsync de DbContext
            var result = await _userManager.UpdateAsync(usuario);
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
                    _ => errorBase?.Description ?? "No se pudieron actualizar los datos del funcionario."
                };

                throw new Exception(mensajeTraducido);
            }

            return true;
        }
    }
}