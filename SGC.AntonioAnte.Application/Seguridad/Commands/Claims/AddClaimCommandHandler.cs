using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Commands.Claims
{
    public class AddClaimCommandHandler : IRequestHandler<AddClaimCommand, bool>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AddClaimCommandHandler(
            UserManager<Usuario> userManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AddClaimCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _userManager.FindByIdAsync(request.UsuarioId.ToString());
            if (usuario == null)
            {
                throw new Exception("El funcionario especificado no existe.");
            }

            var nuevoClaim = new Claim(request.ClaimType, request.ClaimValue);

            // Persistir directamente en la tabla Seguridad.UsuarioClaims
            var result = await _userManager.AddClaimAsync(usuario, nuevoClaim);

            if (!result.Succeeded)
            {
                throw new Exception("No se pudo asignar el permiso granular al usuario.");
            }

            // Auditoria
            var auditoria = new Auditoria
            {
                UsuarioId = usuario.Id,
                Accion = "PERMISO_GRANULAR_ASIGNADO",
                Entidad = "Usuario",
                EntidadId = usuario.Id.ToString(),
                DatosAdicionales = $"Se agrego el claim {request.ClaimType} con valor {request.ClaimValue}",
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
