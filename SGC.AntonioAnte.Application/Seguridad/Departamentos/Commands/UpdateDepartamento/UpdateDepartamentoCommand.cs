using MediatR;
using SGC.AntonioAnte.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Departamentos.Commands.UpdateDepartamento
{
    public class UpdateDepartamentoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UpdateDepartamentoCommandHandler : IRequestHandler<UpdateDepartamentoCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateDepartamentoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateDepartamentoCommand request, CancellationToken cancellationToken)
        {
            var dep = await _context.Departamentos.FindAsync(new object[] { request.Id }, cancellationToken);
            if (dep == null) throw new Exception("Departamento no encontrado.");

            dep.Nombre = request.Nombre;
            dep.Descripcion = request.Descripcion;

            // Al guardar cambios, el DbContext intercepta, calcula las diferencias, asigna IP/Navegador y audita.
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}