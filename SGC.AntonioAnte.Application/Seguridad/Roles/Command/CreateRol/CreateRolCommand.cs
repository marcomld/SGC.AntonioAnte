using MediatR;
using Microsoft.AspNetCore.Identity;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Roles.Command.CreateRol
{
    // COMMAND
    public class CreateRolCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    // HANDLER
    public class CreateRolCommandHandler : IRequestHandler<CreateRolCommand, Guid>
    {
        private readonly RoleManager<Rol> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateRolCommandHandler(
            RoleManager<Rol> roleManager,
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _roleManager = roleManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateRolCommand request, CancellationToken cancellationToken)
        {
            var rolExistente = await _roleManager.RoleExistsAsync(request.Nombre);
            if (rolExistente)
                throw new Exception($"El rol '{request.Nombre}' ya se encuentra registrado.");

            var nuevoRol = new Rol
            {
                Id = Guid.NewGuid(),
                Name = request.Nombre,
                NormalizedName = request.Nombre.ToUpper(),
                Descripcion = request.Descripcion
            };

            var result = await _roleManager.CreateAsync(nuevoRol);
            if (!result.Succeeded)
                throw new Exception("No se pudo registrar el nuevo rol en el sistema.");

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "CREAR_ROL",
                Entidad = "Rol",
                EntidadId = nuevoRol.Id.ToString(),
                DatosAdicionales = $"Rol creado: '{nuevoRol.Name}' - {nuevoRol.Descripcion}",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return nuevoRol.Id;
        }
    }
}
