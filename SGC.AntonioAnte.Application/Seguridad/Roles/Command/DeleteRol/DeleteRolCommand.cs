using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Commands.DeleteRol
{
    // COMMAND
    public class DeleteRolCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteRolCommand() { }

        public DeleteRolCommand(Guid id)
        {
            Id = id;
        }
    }

    // HANDLER
    public class DeleteRolCommandHandler : IRequestHandler<DeleteRolCommand, bool>
    {
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteRolCommandHandler(RoleManager<Rol> roleManager, IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _roleManager = roleManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (rol == null) throw new Exception("El rol especificado no existe.");

            if (rol.Name == "AdminSistemas" || rol.Name == "SuperAdmin")
                throw new Exception("No es posible eliminar los roles base del sistema.");

            string nombreRol = rol.Name ?? string.Empty;
            var result = await _roleManager.DeleteAsync(rol);
            if (!result.Succeeded)
                throw new Exception("No se pudo eliminar el rol de la base de datos.");

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ELIMINAR_ROL",
                Entidad = "Rol",
                EntidadId = request.Id.ToString(),
                DatosAdicionales = $"Rol '{nombreRol}' eliminado permanentemente.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}