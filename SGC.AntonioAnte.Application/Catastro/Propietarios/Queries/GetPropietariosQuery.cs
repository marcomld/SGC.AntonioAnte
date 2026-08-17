using MediatR;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Domain.Catastro.Enums;
using SGC.AntonioAnte.Shared.DTOs.Catastro.Propietarios;
using SGC.AntonioAnte.Shared.DTOs.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Catastro.Propietarios.Queries
{
    public record GetPropietariosQuery(
        string? Busqueda = null,
        bool? EstadoActivo = null,
        TipoPropietario? TipoPropietario = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<PropietarioDto>>;

    public class GetPropietariosQueryHandler : IRequestHandler<GetPropietariosQuery, ResultadoPaginadoDto<PropietarioDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPropietariosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<PropietarioDto>> Handle(GetPropietariosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Propietarios.AsNoTracking().AsQueryable();

            if (request.EstadoActivo.HasValue)
            {
                query = query.Where(p => p.EstadoActivo == request.EstadoActivo.Value);
            }

            if (request.TipoPropietario.HasValue)
            {
                query = query.Where(p => p.TipoPropietario == request.TipoPropietario.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var termino = request.Busqueda.Trim().ToLower();
                query = query.Where(p =>
                    p.Identificacion.Contains(termino) ||
                    (p.Nombres != null && p.Nombres.ToLower().Contains(termino)) ||
                    (p.Apellidos != null && p.Apellidos.ToLower().Contains(termino)) ||
                    (p.RazonSocial != null && p.RazonSocial.ToLower().Contains(termino)));
            }

            int totalRegistros = await query.CountAsync(cancellationToken);
            int pagina = request.Pagina < 1 ? 1 : request.Pagina;
            int registrosPorPagina = request.RegistrosPorPagina < 1 ? 10 : request.RegistrosPorPagina;

            var items = await query
                .OrderBy(p => p.Identificacion)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(p => new PropietarioDto
                {
                    Id = p.Id,
                    TipoPropietario = p.TipoPropietario,
                    Identificacion = p.Identificacion,
                    Nombres = p.Nombres,
                    Apellidos = p.Apellidos,
                    RazonSocial = p.RazonSocial,
                    NombreCompleto = p.TipoPropietario == TipoPropietario.Natural
                        ? $"{p.Nombres} {p.Apellidos}".Trim()
                        : (p.RazonSocial ?? string.Empty),
                    EstadoCivil = p.EstadoCivil,
                    Email = p.Email,
                    Telefono = p.Telefono,
                    EstadoActivo = p.EstadoActivo
                })
                .ToListAsync(cancellationToken);

            return ResultadoPaginadoDto<PropietarioDto>.Crear(items, totalRegistros, pagina, registrosPorPagina);
        }
    }
}