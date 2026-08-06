using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.DeleteDepartamento
{
    // COMMAND
    public class DeleteDepartamentoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    // HANDLER
    public class DeleteDepartamentoCommandHandler : IRequestHandler<DeleteDepartamentoCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteDepartamentoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null) throw new Exception("Departamento no encontrado.");

            string nombreDep = dep.Nombre;
            _context.Departamentos.Remove(dep);

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = _currentUserService.UsuarioIdGuid,
                Accion = "ELIMINAR_DEPARTAMENTO",
                Entidad = "Departamento",
                EntidadId = request.Id.ToString(),
                DatosAdicionales = $"Departamento '{nombreDep}' eliminado permanentemente.",
                DireccionIp = _currentUserService.IpAddress,
                Navegador = _currentUserService.UserAgent,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
