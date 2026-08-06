using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Commands.DesasignarRolUsuario
{
    // COMMAND
    public class DesasignarRolUsuarioCommand : IRequest<bool>
    {
        public Guid UsuarioId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }

    // HANDLER
    public class DesasignarRolUsuarioCommandHandler : IRequestHandler<DesasignarRolUsuarioCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DesasignarRolUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DesasignarRolUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null) throw new Exception("Funcionario no encontrado.");

            var result = await _userManager.RemoveFromRoleAsync(usuario, request.NombreRol);
            if (!result.Succeeded) throw new Exception("No se pudo remover el rol del funcionario.");

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "DESASIGNAR_ROL_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Rol '{request.NombreRol}' removido del funcionario {usuario.Nombres} {usuario.Apellidos}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}