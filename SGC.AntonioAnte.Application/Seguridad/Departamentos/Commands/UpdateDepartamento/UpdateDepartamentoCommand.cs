using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.UpdateDepartamento
{
    // COMMAND
    public class UpdateDepartamentoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    // HANDLER
    public class UpdateDepartamentoCommandHandler : IRequestHandler<UpdateDepartamentoCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateDepartamentoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(UpdateDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null) throw new Exception("Departamento no encontrado.");

            // Comparador de cambios para la bitácora
            var cambios = new List<string>();
            if (dep.Nombre != request.Nombre)
                cambios.Add($"Nombre: '{dep.Nombre}' -> '{request.Nombre}'");
            if (dep.Descripcion != request.Descripcion)
                cambios.Add($"Descripción: '{dep.Descripcion}' -> '{request.Descripcion}'");

            dep.Nombre = request.Nombre;
            dep.Descripcion = request.Descripcion;

            string detalleAuditoria = cambios.Count > 0
                ? $"Cambios aplicados: {string.Join(" | ", cambios)}"
                : "Se ejecutó actualización sin modificaciones en los datos.";


            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ACTUALIZAR_DEPARTAMENTO",
                Entidad = "Departamento",
                EntidadId = dep.Id.ToString(),
                DatosAdicionales = detalleAuditoria,
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}