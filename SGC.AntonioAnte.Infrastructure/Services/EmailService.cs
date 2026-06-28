using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SGC.AntonioAnte.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Infrastructure.Services
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
            var mensaje = new MimeMessage();
            var remitenteNombre = _configuration["SmtpSettings:RemitenteNombre"];
            var remitenteEmail = _configuration["SmtpSettings:RemitenteEmail"];

            mensaje.From.Add(new MailboxAddress(remitenteNombre, remitenteEmail));
            mensaje.To.Add(new MailboxAddress(paraEmail, paraEmail));
            mensaje.Subject = asunto;

            var cuerpoBuilder = new BodyBuilder { HtmlBody = cuerpoHtml };
            mensaje.Body = cuerpoBuilder.ToMessageBody();

            using var clienteSmtp = new SmtpClient();

            var servidor = _configuration["SmtpSettings:Servidor"]!;
            var puerto = int.Parse(_configuration["SmtpSettings:Puerto"]!);
            var habilitarSsl = bool.Parse(_configuration["SmtpSettings:HabilitarSsl"]!);
            var usuario = _configuration["SmtpSettings:Usuario"]!;
            var clave = _configuration["SmtpSettings:Clave"]!;

            // Configuración segura de Sockets según el puerto utilizado
            var opcionesSocket = SecureSocketOptions.Auto;
            if (!habilitarSsl && puerto == 2525)
            {
                opcionesSocket = SecureSocketOptions.StartTlsWhenAvailable;
            }
            else if (habilitarSsl)
            {
                opcionesSocket = SecureSocketOptions.StartTls;
            }

            await clienteSmtp.ConnectAsync(servidor, puerto, opcionesSocket);
            await clienteSmtp.AuthenticateAsync(usuario, clave);
            await clienteSmtp.SendAsync(mensaje);
            await clienteSmtp.DisconnectAsync(true);
        }
    }
}
