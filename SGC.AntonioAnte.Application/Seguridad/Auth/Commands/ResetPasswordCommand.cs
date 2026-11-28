using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands
{
    // 1. COMMAND
    public record ResetPasswordCommand(
        string Email,
        string Token,
        string NuevaPassword
    ) : IRequest<bool>;

    // 2. VALIDATOR
    public class ResetPasswordCommandValidator
        : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(v => v.Email)
                .NotEmpty()
                .WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido.");

            RuleFor(v => v.Token)
                .NotEmpty()
                .WithMessage("La autorización de recuperación es obligatoria.")
                .MaximumLength(4096);

            RuleFor(v => v.NuevaPassword)
                .NotEmpty()
                .WithMessage("La nueva contraseña es obligatoria.")
                .MinimumLength(8)
                .WithMessage("La contraseña debe tener al menos 8 caracteres.");
        }
    }

    // 3. HANDLER
    public class ResetPasswordCommandHandler
        : IRequestHandler<ResetPasswordCommand, bool>
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

        public async Task<bool> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var usuario = await _userManager
                .FindByEmailAsync(request.Email);

            if (usuario == null || !usuario.EstadoActivo)
                throw TokenInvalido();

            var recuperacion = await _context
                .RecuperacionesPassword
                .SingleOrDefaultAsync(
                    x => x.UsuarioId == usuario.Id,
                    cancellationToken);

            var ahora = DateTime.UtcNow;

            if (recuperacion == null ||
                !recuperacion.FechaValidacionOtp.HasValue ||
                recuperacion.FechaUtilizacion.HasValue ||
                string.IsNullOrEmpty(recuperacion.ResetTokenHash) ||
                !recuperacion.FechaExpiracionResetToken.HasValue ||
                recuperacion.FechaExpiracionResetToken.Value <= ahora)
            {
                throw TokenInvalido();
            }

            var securityStamp =
                await _userManager.GetSecurityStampAsync(usuario);

            if (recuperacion.SecurityStamp != securityStamp)
                throw TokenInvalido();

            var hashRecibido =
                RecuperacionPasswordPolicy.CalcularHashToken(
                    request.Token);

            if (!RecuperacionPasswordPolicy.CompararSeguro(
                    recuperacion.ResetTokenHash,
                    hashRecibido))
            {
                throw TokenInvalido();
            }

            // Comprobar también la validez criptográfica en Identity.
            var tokenValido =
                await _userManager.VerifyUserTokenAsync(
                    usuario,
                    _userManager.Options.Tokens
                        .PasswordResetTokenProvider,
                    UserManager<Usuario>.ResetPasswordTokenPurpose,
                    request.Token);

            if (!tokenValido)
                throw TokenInvalido();

            // Impedir reutilizar la contraseña actual.
            if (!string.IsNullOrEmpty(usuario.PasswordHash))
            {
                var verificacion =
                    _userManager.PasswordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.PasswordHash,
                        request.NuevaPassword);

                if (verificacion != PasswordVerificationResult.Failed)
                {
                    throw new ValidationException(
                        "La nueva contraseña no puede ser igual " +
                        "a la contraseña anterior.");
                }
            }

            // Preparar el consumo del token.
            recuperacion.FechaUtilizacion = ahora;
            recuperacion.ResetTokenHash = null;
            recuperacion.CodigoOtpProtegido = null;

            // Preparar el desbloqueo dentro de la misma actualización.
            usuario.AccessFailedCount = 0;
            usuario.LockoutEnd = null;

            // Preparar auditoría antes de persistir.
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = AuditActions.RestablecerPassword,
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales =
                    "Contraseña restablecida mediante OTP validado " +
                    "y autorización temporal de un solo uso.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = ahora
            });

            // Identity utiliza el mismo ApplicationDbContext.
            // Su actualización persiste también los cambios
            // rastreados en RecuperacionPassword y Auditoria.
            var resultado = await _userManager.ResetPasswordAsync(
                usuario,
                request.Token,
                request.NuevaPassword);

            if (!resultado.Succeeded)
            {
                var error = resultado.Errors.FirstOrDefault();

                var mensaje = error?.Code switch
                {
                    "InvalidToken" =>
                        RecuperacionPasswordPolicy.MensajeTokenInvalido,

                    "ConcurrencyFailure" =>
                        "La recuperación fue utilizada o modificada. " +
                        "Solicite un nuevo código.",

                    "PasswordTooShort" =>
                        "La contraseña debe tener al menos 8 caracteres.",

                    "PasswordRequiresNonAlphanumeric" =>
                        "La contraseña debe contener un carácter especial.",

                    "PasswordRequiresDigit" =>
                        "La contraseña debe contener al menos un número.",

                    "PasswordRequiresUpper" =>
                        "La contraseña debe contener una letra mayúscula.",

                    "PasswordRequiresLower" =>
                        "La contraseña debe contener una letra minúscula.",

                    "PasswordRequiresUniqueChars" =>
                        "La contraseña debe contener más caracteres distintos.",

                    _ => error?.Description ??
                         "No se pudo restablecer la contraseña."
                };

                // No guardar los cambios preparados si Identity falla.
                throw new ValidationException(mensaje);
            }

            return true;
        }

        private static ValidationException TokenInvalido()
        {
            return new ValidationException(
                RecuperacionPasswordPolicy.MensajeTokenInvalido);
        }
    }
}