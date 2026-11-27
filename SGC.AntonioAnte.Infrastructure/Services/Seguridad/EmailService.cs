using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SGC.AntonioAnte.Application.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Services.Seguridad
{
    public class EmailService : IEmailService
    {
        private const string MensajeErrorCliente =
            "No fue posible enviar el correo electrónico en este momento. Intente nuevamente más tarde.";

        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnviarCorreoAsync(string paraEmail, string asunto, string cuerpoHtml)
        {
            if (string.IsNullOrWhiteSpace(paraEmail))
                throw new ArgumentException("El correo de destino es obligatorio.", nameof(paraEmail));

            var servidor = _configuration["SmtpSettings:Servidor"];
            var puertoTexto = _configuration["SmtpSettings:Puerto"];
            var usuario = _configuration["SmtpSettings:Usuario"];
            var clave = _configuration["SmtpSettings:Clave"];
            var remitenteEmail = _configuration["SmtpSettings:RemitenteEmail"];
            var remitenteNombre = _configuration["SmtpSettings:RemitenteNombre"];

            if (string.IsNullOrWhiteSpace(servidor) ||
                !int.TryParse(puertoTexto, out var puerto) ||
                string.IsNullOrWhiteSpace(remitenteEmail ?? usuario))
            {
                // El detalle queda solo en el log del servidor, nunca llega al cliente.
                _logger.LogError("Configuración SMTP incompleta o inválida. Revise la sección SmtpSettings.");
                throw new InvalidOperationException(MensajeErrorCliente);
            }

            remitenteEmail ??= usuario!;

            try
            {
                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(remitenteNombre ?? string.Empty, remitenteEmail));
                mensaje.To.Add(MailboxAddress.Parse(paraEmail));
                mensaje.Subject = asunto;
                mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

                using var smtp = new SmtpClient { Timeout = 20000 };

                var opciones = puerto switch
                {
                    465 => SecureSocketOptions.SslOnConnect,
                    587 => SecureSocketOptions.StartTls,
                    _ => SecureSocketOptions.Auto
                };

                await smtp.ConnectAsync(servidor, puerto, opciones);

                // Solo autentica si el servidor lo requiere (algunos relays internos no lo piden).
                if (!string.IsNullOrWhiteSpace(usuario) && !string.IsNullOrWhiteSpace(clave))
                    await smtp.AuthenticateAsync(usuario, clave);

                await smtp.SendAsync(mensaje);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Se registra el error técnico (sin credenciales ni datos del destinatario)
                // y se devuelve al cliente un mensaje genérico.
                _logger.LogError(ex, "Fallo en el envío de correo SMTP ({Tipo}).", ex.GetType().Name);
                throw new InvalidOperationException(MensajeErrorCliente);
            }
        }
    }
}