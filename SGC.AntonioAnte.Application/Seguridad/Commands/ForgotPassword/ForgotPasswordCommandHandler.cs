using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, string>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ForgotPasswordCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByEmailAsync(request.Email);

            if (usuario == null)
            {
                throw new Exception("El correo electronico no pertenece a ningun funcionario registrado.");
            }

            if (!usuario.EstadoActivo)
            {
                throw new Exception("El usuario se encuentra inactivo en el sistema.");
            }

            // Generar el token temporal usando el proveedor de Identity
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            // Registrar en la tabla de Auditorias utilizando el CurrentUserService global
            var auditoria = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "SOLICITUD_RECUPERACION_PASSWORD",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = "Token de recuperacion de contrasena generado exitosamente.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Auditorias.Add(auditoria);
            await _context.SaveChangesAsync(cancellationToken);

            // Retornamos el token para poder visualizarlo y consumirlo desde Swagger o Blazor
            return token;
        }
    }
}
