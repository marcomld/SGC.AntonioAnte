using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Commands.UpdateUsuario
{
    // COMMAND
    public class UpdateUsuarioCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? DepartamentoId { get; set; }
    }

    // HANDLER
    public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUsuarioCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.Id.ToString());
            if (usuario == null) throw new Exception("Funcionario no encontrado.");

            // DELTA AUDIT: Identificar qué cambió exactamente
            var cambios = new List<string>();

            if (usuario.Nombres != request.Nombres)
                cambios.Add($"Nombres: '{usuario.Nombres}' -> '{request.Nombres}'");

            if (usuario.Apellidos != request.Apellidos)
                cambios.Add($"Apellidos: '{usuario.Apellidos}' -> '{request.Apellidos}'");

            if (usuario.Email != request.Email)
                cambios.Add($"Email: '{usuario.Email}' -> '{request.Email}'");

            if (usuario.DepartamentoId != request.DepartamentoId)
                cambios.Add($"DepartamentoId: '{usuario.DepartamentoId}' -> '{request.DepartamentoId}'");

            usuario.Nombres = request.Nombres;
            usuario.Apellidos = request.Apellidos;
            usuario.Email = request.Email;
            usuario.DepartamentoId = request.DepartamentoId;

            var result = await _userManager.UpdateAsync(usuario);
            if (!result.Succeeded) throw new Exception("No se pudieron actualizar los datos del funcionario.");

            string detalleAudit = cambios.Count > 0
                ? $"Campos modificados: {string.Join(" | ", cambios)}"
                : "Actualización procesada sin cambios en las propiedades.";

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ACTUALIZACION_FUNCIONARIO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = detalleAudit,
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
