using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.CambiarEstadoDepartamento
{
    // COMMAND
    public class CambiarEstadoDepartamentoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    // HANDLER
    public class CambiarEstadoDepartamentoCommandHandler : IRequestHandler<CambiarEstadoDepartamentoCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CambiarEstadoDepartamentoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CambiarEstadoDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null) throw new Exception("Departamento no encontrado.");

            dep.EstadoActivo = !dep.EstadoActivo;

            string accionAuditoria = dep.EstadoActivo ? "ACTIVACION_DEPARTAMENTO" : "DESACTIVACION_DEPARTAMENTO";

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = accionAuditoria,
                Entidad = "Departamento",
                EntidadId = dep.Id.ToString(),
                DatosAdicionales = $"EstadoActivo de '{dep.Nombre}' cambiado a {dep.EstadoActivo}",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
