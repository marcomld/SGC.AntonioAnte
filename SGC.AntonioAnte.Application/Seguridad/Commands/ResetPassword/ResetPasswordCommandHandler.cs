using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.ResetPassword
{
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
                throw new Exception("Usuario no encontrado.");
            }

            // Validar y aplicar el cambio en el almacén persistente
            var result = await _userManager.ResetPasswordAsync(usuario, request.Token, request.NuevaPassword);

            if (!result.Succeeded)
            {
                var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"No se pudo restablecer la contraseña: {errorMsg}");
            }

            // Desbloquear al usuario si estaba penalizado por intentos fallidos
            await _userManager.ResetAccessFailedCountAsync(usuario);
            await _userManager.SetLockoutEndDateAsync(usuario, null);

            // Registrar en la tabla Auditorias
            var auditoria = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "RESTABLECER_PASSWORD_EXITOSO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = "Contraseña cambiada por token de recuperacion",
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
