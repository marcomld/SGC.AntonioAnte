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
    public record GetPropietarioByIdentificacionQuery(string Identificacion) : IRequest<PropietarioDto?>;

    public class GetPropietarioByIdentificacionQueryHandler : IRequestHandler<GetPropietarioByIdentificacionQuery, PropietarioDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetPropietarioByIdentificacionQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PropietarioDto?> Handle(GetPropietarioByIdentificacionQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.Propietarios
                .FirstOrDefaultAsync(x => x.Identificacion == request.Identificacion && x.EstadoActivo, cancellationToken);

            if (p == null) return null;

            return new PropietarioDto
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
            };
        }
    }
}
