using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SGC.AntonioAnte.Application.Common.Interfaces;

namespace SGC.AntonioAnte.Infrastructure.Services.Seguridad
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreoAsync(string paraEmail, string asunto, string cuerpoHtml)
        {
            try
            {
                var mensaje = new MimeMessage();
                var remitenteNombre = _configuration["SmtpSettings:RemitenteNombre"];
                var remitenteEmail = _configuration["SmtpSettings:RemitenteEmail"]
                    ?? throw new InvalidOperationException("El correo del remitente no está configurado.");

                if (string.IsNullOrWhiteSpace(paraEmail))
                    throw new ArgumentNullException(nameof(paraEmail), "El correo de destino no puede ser nulo o vacío.");

                mensaje.From.Add(new MailboxAddress(remitenteNombre, remitenteEmail));
                mensaje.To.Add(new MailboxAddress(paraEmail, paraEmail));
                mensaje.Subject = asunto;

                var cuerpoBuilder = new BodyBuilder { HtmlBody = cuerpoHtml };
                mensaje.Body = cuerpoBuilder.ToMessageBody();

                using var clienteSmtp = new SmtpClient();

                // Timeout de 10 segundos
                clienteSmtp.Timeout = 10000;

                var servidor = _configuration["SmtpSettings:Servidor"]!;
                var puerto = int.Parse(_configuration["SmtpSettings:Puerto"]!);
                var usuario = _configuration["SmtpSettings:Usuario"]!;
                var clave = _configuration["SmtpSettings:Clave"]!;

                // GMAIL EN PUERTO 587 REQUIERE STARTTLS
                SecureSocketOptions opcionesSocket = puerto == 587
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                // Conexión y autenticación
                await clienteSmtp.ConnectAsync(servidor, puerto, opcionesSocket);
                await clienteSmtp.AuthenticateAsync(usuario, clave);
                await clienteSmtp.SendAsync(mensaje);
                await clienteSmtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SMTP CRITICAL ERROR] {ex.Message}");
                throw;
            }
        }
    }
}