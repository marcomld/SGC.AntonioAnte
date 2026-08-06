using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.CambiarEstadoUsuario
{
    // COMMAND
    public class CambiarEstadoUsuarioCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool EstadoActivo { get; set; }
    }

    // HANDLER
    public class CambiarEstadoUsuarioCommandHandler : IRequestHandler<CambiarEstadoUsuarioCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CambiarEstadoUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CambiarEstadoUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.Id.ToString());
            if (usuario == null) throw new Exception("Funcionario no encontrado.");

            usuario.EstadoActivo = request.EstadoActivo;
            var result = await _userManager.UpdateAsync(usuario);
            if (!result.Succeeded) throw new Exception("Error al cambiar el estado del funcionario.");

            string accionAuditoria = request.EstadoActivo ? "ACTIVACION_FUNCIONARIO" : "DESACTIVACION_FUNCIONARIO";
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = accionAuditoria,
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"EstadoActivo cambiado a {request.EstadoActivo}",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
