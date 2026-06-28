using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using SGC.AntonioAnte.Domain.Entities;
using SGC.AntonioAnte.Application.Common.Interfaces;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.ForgotPassword
{
    // Cambiado el tipo de retorno de string a bool por seguridad ISO 27001
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
            _emailService = emailService;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByEmailAsync(request.Email);

            if (usuario == null)
            {
                throw new Exception("El correo electrónico no pertenece a ningún funcionario registrado.");
            }

            if (!usuario.EstadoActivo)
            {
                throw new Exception("El usuario se encuentra inactivo en el sistema.");
            }

            // Generar el código OTP de 6 dígitos
            var codigoOtp = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            // Plantilla HTML unificada con información del tiempo de expiración (3 horas)
            string plantillaHtml = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                <div style='background-color: #0f172a; padding: 15px; text-align: center; border-radius: 6px 6px 0 0;'>
                    <h2 style='color: #ffffff; margin: 0; font-size: 20px;'>SGC - GAD Municipal Antonio Ante</h2>
                </div>
                <div style='padding: 20px; color: #1e293b; background-color: #fafafa;'>
                    <p style='font-size: 15px;'>Estimado/a funcionario/a <strong>{usuario.Nombres} {usuario.Apellidos}</strong>,</p>
                    <p style='font-size: 14px; line-height: 1.5;'>Hemos recibido una solicitud de seguridad para restablecer la contraseña de acceso de su cuenta corporativa en el Sistema de Gestión Catastral.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <p style='font-size: 13px; color: #475569; margin-bottom: 10px;'>Su código de verificación (OTP) es:</p>
                        <span style='background-color: #ffffff; border: 2px dashed #2563eb; color: #2563eb; font-size: 28px; font-weight: bold; padding: 12px 35px; letter-spacing: 6px; border-radius: 6px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);'>
                            {codigoOtp}
                        </span>
                    </div>
                    <p style='font-size: 13px; color: #64748b; line-height: 1.4;'>Este código es de un solo uso y es <strong>válido por las próximas 3 horas</strong> por razones de control interno, seguridad de la información y auditoría gubernamental.</p>
                    <p style='font-size: 13px; color: #dc2626; font-weight: bold;'>Si usted no solicitó este cambio, notifique de inmediato a la Jefatura de Sistemas.</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                    <p style='font-size: 11px; color: #94a3b8; text-align: center; margin: 0;'>© {DateTime.UtcNow.Year} Jefatura de Sistemas - Gobierno Autónomo Descentralizado Municipal de Antonio Ante</p>
                </div>
            </div>";

            await _emailService.EnviarCorreoAsync(usuario.Email!, "SGC Catastro - Código de Verificación Seguro", plantillaHtml);

            var auditoria = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "SOLICITUD_RECUPERACION_PASSWORD",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = "Código de verificación OTP de 6 dígitos generado y enviado por correo electrónico.",
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