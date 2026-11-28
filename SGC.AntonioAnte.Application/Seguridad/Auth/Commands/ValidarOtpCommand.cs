using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Application.Seguridad.Auth.Policies;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands
{
    // 1. COMMAND
    public record ValidarOtpCommand(
        string Email,
        string Codigo
    ) : IRequest<ValidarOtpResultadoDto>;

    // 2. VALIDATOR
    public class ValidarOtpCommandValidator
        : AbstractValidator<ValidarOtpCommand>
    {
        public ValidarOtpCommandValidator()
        {
            RuleFor(v => v.Email)
                .NotEmpty()
                .WithMessage("El correo es obligatorio.")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido.");

            RuleFor(v => v.Codigo)
                .NotEmpty()
                .WithMessage("El código es obligatorio.")
                .Matches(@"^[0-9]{6}$")
                .WithMessage("El código debe contener 6 dígitos.");
        }
    }

    // 3. HANDLER
    public class ValidarOtpCommandHandler
        : IRequestHandler<ValidarOtpCommand, ValidarOtpResultadoDto>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDataProtector _protector;

        public ValidarOtpCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDataProtectionProvider protectionProvider)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;

            _protector = protectionProvider.CreateProtector(
                RecuperacionPasswordPolicy.PropositoProteccionOtp);
        }

        public async Task<ValidarOtpResultadoDto> Handle(
            ValidarOtpCommand request,
            CancellationToken cancellationToken)
        {
            var usuario = await _userManager
                .FindByEmailAsync(request.Email);

            if (usuario == null || !usuario.EstadoActivo)
                return Fallo();

            var recuperacion = await _context
                .RecuperacionesPassword
                .SingleOrDefaultAsync(
                    x => x.UsuarioId == usuario.Id,
                    cancellationToken);

            var ahora = DateTime.UtcNow;

            if (recuperacion == null ||
                string.IsNullOrEmpty(recuperacion.CodigoOtpProtegido) ||
                recuperacion.FechaExpiracionOtp <= ahora ||
                recuperacion.FechaValidacionOtp.HasValue ||
                recuperacion.FechaUtilizacion.HasValue)
            {
                return Fallo();
            }

            if (recuperacion.IntentosFallidos >=
                RecuperacionPasswordPolicy.MaximoIntentos)
            {
                return Fallo(
                    "Se alcanzó el límite de intentos. " +
                    "Solicite un nuevo código.");
            }

            var securityStamp =
                await _userManager.GetSecurityStampAsync(usuario);

            if (recuperacion.SecurityStamp != securityStamp)
                return Fallo();

            string codigoEsperado;

            try
            {
                codigoEsperado = _protector
                    .CreateProtector(usuario.Id.ToString())
                    .Unprotect(recuperacion.CodigoOtpProtegido);
            }
            catch (CryptographicException)
            {
                return Fallo();
            }

            var codigoValido =
                RecuperacionPasswordPolicy.CompararSeguro(
                    codigoEsperado,
                    request.Codigo);

            if (!codigoValido)
            {
                recuperacion.IntentosFallidos++;

                var intentosAgotados =
                    recuperacion.IntentosFallidos >=
                    RecuperacionPasswordPolicy.MaximoIntentos;

                if (intentosAgotados)
                {
                    recuperacion.CodigoOtpProtegido = null;
                }

                try
                {
                    await _context.SaveChangesAsync(
                        cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Fallo(
                        "La solicitud de recuperación cambió. " +
                        "Intente nuevamente.");
                }

                return Fallo(
                    intentosAgotados
                    ? "Se alcanzó el máximo de intentos. " +
                      "Solicite un nuevo código."
                    : "Código incorrecto. Intentos restantes: " +
                      (RecuperacionPasswordPolicy.MaximoIntentos -
                       recuperacion.IntentosFallidos));
            }

            // Generar una autorización independiente con Identity.
            var resetToken = await _userManager
                .GeneratePasswordResetTokenAsync(usuario);

            // Consumir definitivamente el OTP.
            recuperacion.CodigoOtpProtegido = null;
            recuperacion.FechaValidacionOtp = ahora;

            // Guardar solamente el hash del ResetToken.
            recuperacion.ResetTokenHash =
                RecuperacionPasswordPolicy.CalcularHashToken(
                    resetToken);

            recuperacion.FechaExpiracionResetToken =
                ahora.AddMinutes(
                    RecuperacionPasswordPolicy
                        .MinutosVigenciaResetToken);

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "VALIDAR_OTP_RECUPERACION",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales =
                    "OTP verificado y autorización de recuperación emitida.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = ahora
            });

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Fallo(
                    "El código ya fue utilizado o reemplazado. " +
                    "Solicite una nueva recuperación.");
            }

            return new ValidarOtpResultadoDto
            {
                Exitoso = true,
                Mensaje = "Código validado correctamente.",
                ResetToken = resetToken,
                ExpiraEnSegundos =
                    RecuperacionPasswordPolicy
                        .MinutosVigenciaResetToken * 60
            };
        }

        private static ValidarOtpResultadoDto Fallo(
            string? mensaje = null)
        {
            return new ValidarOtpResultadoDto
            {
                Exitoso = false,
                Mensaje = mensaje ??
                    RecuperacionPasswordPolicy.MensajeOtpInvalido
            };
        }
    }
}