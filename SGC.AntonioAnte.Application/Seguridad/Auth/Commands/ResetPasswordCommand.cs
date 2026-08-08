using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands
{
    // 1. COMMAND (record posicional)
    public record ResetPasswordCommand(string Email, string Token, string NuevaPassword) : IRequest<bool>;

    // 2. VALIDATOR (FluentValidation)
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(v => v.Token)
                .NotEmpty().WithMessage("El código de verificación (OTP) es obligatorio.");

            RuleFor(v => v.NuevaPassword)
                .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.");
        }
    }

    // 3. HANDLER
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ResetPasswordCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario == null)
            {
                throw new Exception("El usuario no existe o el correo electrónico es incorrecto.");
            }

            if (!usuario.EstadoActivo)
            {
                throw new Exception("El usuario se encuentra inactivo en el sistema.");
            }

            // 🛡️ CONTROL ANTI-REUTILIZACIÓN: Verificar si la nueva clave coincide con la actual en BD
            if (!string.IsNullOrEmpty(usuario.PasswordHash))
            {
                var resultadoVerificacion = _userManager.PasswordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.NuevaPassword);
                if (resultadoVerificacion != PasswordVerificationResult.Failed)
                {
                    throw new Exception("La nueva contraseña no puede ser igual a la contraseña anterior.");
                }
            }

            // Aplicar el cambio de clave mediante el token OTP
            var result = await _userManager.ResetPasswordAsync(usuario, request.Token, request.NuevaPassword);

            if (!result.Succeeded)
            {
                var errorBase = result.Errors.FirstOrDefault();
                string mensajeTraducido = errorBase?.Code switch
                {
                    "InvalidToken" => "El código de verificación (OTP) es inválido o ha expirado.",
                    "PasswordTooShort" => "La contraseña no cumple con la longitud mínima de 8 caracteres.",
                    "PasswordRequiresNonAlphanumeric" => "La contraseña debe contener al menos un carácter especial (!, @, #, etc.).",
                    "PasswordRequiresDigit" => "La contraseña debe contener al menos un número (0-9).",
                    "PasswordRequiresUpper" => "La contraseña debe contener al menos una letra mayúscula (A-Z).",
                    "PasswordRequiresLower" => "La contraseña debe contener al menos una letra minúscula (a-z).",
                    _ => errorBase?.Description ?? "No se pudo restablecer la contraseña."
                };

                throw new Exception(mensajeTraducido);
            }

            // Desbloquear al usuario si estaba penalizado por intentos fallidos
            await _userManager.ResetAccessFailedCountAsync(usuario);
            await _userManager.SetLockoutEndDateAsync(usuario, null);

            // Registro de Auditoría estandarizado
            var auditoria = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = AuditActions.RestablecerPassword,
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = "Contraseña restablecida exitosamente con validación de código OTP y regla anti-reutilización.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Auditorias.Add(auditoria);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}