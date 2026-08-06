using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Commands.AsignarRolUsuario
{
    // COMMAND
    public class AsignarRolUsuarioCommand : IRequest<bool>
    {
        public Guid UsuarioId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }

    // HANDLER
    public class AsignarRolUsuarioCommandHandler : IRequestHandler<AsignarRolUsuarioCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AsignarRolUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            RoleManager<Rol> roleManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AsignarRolUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null) throw new Exception("Funcionario no encontrado.");

            var existeRol = await _roleManager.RoleExistsAsync(request.NombreRol);
            if (!existeRol) throw new Exception($"El rol '{request.NombreRol}' no existe en la base de datos.");

            var result = await _userManager.AddToRoleAsync(usuario, request.NombreRol);
            if (!result.Succeeded) throw new Exception("El funcionario ya cuenta con este rol o no se pudo asignar.");

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ASIGNAR_ROL_USUARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Rol '{request.NombreRol}' asignado al funcionario {usuario.Nombres} {usuario.Apellidos}.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}