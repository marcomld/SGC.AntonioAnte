using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Propietarios.Queries
{
    public record GetPropietariosQuery(string? Busqueda = null) : IRequest<List<PropietarioDto>>;

    public class GetPropietariosQueryHandler : IRequestHandler<GetPropietariosQuery, List<PropietarioDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPropietariosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PropietarioDto>> Handle(GetPropietariosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Propietarios.Where(p => p.EstadoActivo).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var termino = request.Busqueda.Trim().ToLower();
                query = query.Where(p =>
                    p.Identificacion.Contains(termino) ||
                    (p.Nombres != null && p.Nombres.ToLower().Contains(termino)) ||
                    (p.Apellidos != null && p.Apellidos.ToLower().Contains(termino)) ||
                    (p.RazonSocial != null && p.RazonSocial.ToLower().Contains(termino)));
            }

            var propietarios = await query
                .OrderBy(p => p.Identificacion)
                .Take(100) // Límite de resiliencia
                .ToListAsync(cancellationToken);

            return propietarios.Select(p => new PropietarioDto
            {
                Id = p.Id,
                TipoPropietario = p.TipoPropietario,
                Identificacion = p.Identificacion,
                Nombres = p.Nombres,
                Apellidos = p.Apellidos,
                RazonSocial = p.RazonSocial,
                NombreCompleto = p.ObtenerNombreCompleto(),
                EstadoCivil = p.EstadoCivil,
                Email = p.Email,
                Telefono = p.Telefono,
                EstadoActivo = p.EstadoActivo
            }).ToList();
        }
    }
}
