using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.Constants;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands
{
    // 1. COMMAND
    public record ForgotPasswordCommand(
        string Email
    ) : IRequest<bool>;

    // 2. VALIDATOR
    public class ForgotPasswordCommandValidator
        : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(v => v.Email)
                .NotEmpty()
                .WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido.");
        }
    }

    // 3. HANDLER
    public class ForgotPasswordCommandHandler
        : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IDataProtector _protector;

        public ForgotPasswordCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IDataProtectionProvider protectionProvider)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
            _emailService = emailService;

            _protector = protectionProvider.CreateProtector(
                RecuperacionPasswordPolicy.PropositoProteccionOtp);
        }

        public async Task<bool> Handle(
            ForgotPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var usuario = await _userManager
                .FindByEmailAsync(request.Email);

            // Respuesta genérica para evitar enumerar usuarios.
            if (usuario == null || !usuario.EstadoActivo)
                return true;

            var ahora = DateTime.UtcNow;

            var recuperacion = await _context
                .RecuperacionesPassword
                .SingleOrDefaultAsync(
                    x => x.UsuarioId == usuario.Id,
                    cancellationToken);

            // Control de reenvío desde el servidor.
            if (recuperacion != null &&
                recuperacion.FechaUltimoEnvio.AddSeconds(
                    RecuperacionPasswordPolicy.SegundosReenvio) > ahora)
            {
                return true;
            }

            var codigoOtp =
                RecuperacionPasswordPolicy.GenerarOtp();

            if (recuperacion == null)
            {
                recuperacion = new RecuperacionPassword
                {
                    UsuarioId = usuario.Id
                };

                _context.RecuperacionesPassword.Add(recuperacion);
            }

            // Vincular el código criptográficamente al usuario.
            recuperacion.CodigoOtpProtegido =
                _protector
                    .CreateProtector(usuario.Id.ToString())
                    .Protect(codigoOtp);

            recuperacion.FechaExpiracionOtp =
                ahora.AddMinutes(
                    RecuperacionPasswordPolicy.MinutosVigenciaOtp);

            recuperacion.FechaUltimoEnvio = ahora;
            recuperacion.IntentosFallidos = 0;
            recuperacion.FechaValidacionOtp = null;
            recuperacion.FechaUtilizacion = null;

            // Invalidar cualquier autorización anterior.
            recuperacion.ResetTokenHash = null;
            recuperacion.FechaExpiracionResetToken = null;

            recuperacion.SecurityStamp =
                await _userManager.GetSecurityStampAsync(usuario);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Otra solicitud modificó el registro.
                return true;
            }

            var nombre = WebUtility.HtmlEncode(
                $"{usuario.Nombres} {usuario.Apellidos}");

            string plantillaHtml = $@"
            <div style='font-family:Arial,sans-serif;
                        max-width:600px;
                        margin:0 auto;
                        padding:20px;
                        border:1px solid #e2e8f0;
                        border-radius:8px;'>

                <div style='background:#0f172a;
                            padding:15px;
                            text-align:center;
                            border-radius:6px 6px 0 0;'>
                    <h2 style='color:#ffffff;margin:0;font-size:20px;'>
                        SGC - GAD Municipal Antonio Ante
                    </h2>
                </div>

                <div style='padding:20px;
                            color:#1e293b;
                            background:#fafafa;'>

                    <p>Estimado/a funcionario/a
                        <strong>{nombre}</strong>,
                    </p>

                    <p>
                        Hemos recibido una solicitud para restablecer
                        la contraseña de su cuenta institucional.
                    </p>

                    <div style='text-align:center;margin:30px 0;'>

                        <p>Su código de verificación es:</p>

                        <span style='background:#ffffff;
                                     border:2px dashed #2563eb;
                                     color:#2563eb;
                                     font-size:28px;
                                     font-weight:bold;
                                     padding:12px 35px;
                                     letter-spacing:6px;
                                     border-radius:6px;
                                     display:inline-block;'>
                            {codigoOtp}
                        </span>
                    </div>

                    <p style='font-size:13px;color:#64748b;'>
                        Este código es de un solo uso y es válido
                        durante los próximos
                        <strong>
                            {RecuperacionPasswordPolicy.MinutosVigenciaOtp}
                            minutos.
                        </strong>
                    </p>

                    <p style='font-size:13px;color:#dc2626;'>
                        No comparta este código con ninguna persona.
                        Si no solicitó la recuperación, comuníquese
                        con la Jefatura de Sistemas.
                    </p>

                    <hr style='border:0;border-top:1px solid #e2e8f0;' />

                    <p style='font-size:11px;
                              color:#94a3b8;
                              text-align:center;'>
                        © {DateTime.UtcNow.Year}
                        Jefatura de Sistemas -
                        GAD Municipal Antonio Ante
                    </p>
                </div>
            </div>";

            await _emailService.EnviarCorreoAsync(
                usuario.Email!,
                "SGC Catastro - Código de Verificación Seguro",
                plantillaHtml);

            var auditoria = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = AuditActions.SolicitarRecuperacionPassword,
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales =
                    "Solicitud de recuperación y envío de OTP.",
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