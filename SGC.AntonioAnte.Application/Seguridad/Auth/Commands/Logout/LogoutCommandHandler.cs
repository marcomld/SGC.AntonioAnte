using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationDbContext _context;

        public LogoutCommandHandler(UserManager<Usuario> userManager, ICurrentUserService currentUserService, IApplicationDbContext context)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UsuarioId;
            if (string.IsNullOrEmpty(userId)) throw new Exception("Usuario no autenticado.");

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return false;

            // Borramos el token de la tabla Seguridad.UsuarioTokens
            await _userManager.RemoveAuthenticationTokenAsync(usuario, "SGC_System", "RefreshToken");

            // Auditoría
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "LOGOUT",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = "Cierre de sesión manual",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
