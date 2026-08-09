using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Queries
{
    // 1. QUERY
    public record GetUsuariosQuery(
        string? Busqueda = null,
        bool? EstadoActivo = null,
        Guid? DepartamentoId = null,
        int Pagina = 1,
        int RegistrosPorPagina = 10
    ) : IRequest<ResultadoPaginadoDto<UsuarioResponseDto>>;

    // 2. HANDLER
    public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, ResultadoPaginadoDto<UsuarioResponseDto>>
    {
        private readonly UserManager<Usuario> _userManager;

        public GetUsuariosQueryHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResultadoPaginadoDto<UsuarioResponseDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
        {
            var query = _userManager.Users
                .Include(u => u.Departamento)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                var busquedaNorm = request.Busqueda.Trim().ToLower();
                query = query.Where(u =>
                    u.Identificacion.Contains(busquedaNorm) ||
                    u.Nombres.ToLower().Contains(busquedaNorm) ||
                    u.Apellidos.ToLower().Contains(busquedaNorm) ||
                    (u.Email != null && u.Email.ToLower().Contains(busquedaNorm)));
            }

            if (request.EstadoActivo.HasValue)
            {
                query = query.Where(u => u.EstadoActivo == request.EstadoActivo.Value);
            }

            if (request.DepartamentoId.HasValue && request.DepartamentoId != Guid.Empty)
            {
                query = query.Where(u => u.DepartamentoId == request.DepartamentoId.Value);
            }

            int totalRegistros = await query.CountAsync(cancellationToken);

            int pagina = request.Pagina < 1 ? 1 : request.Pagina;
            int registrosPorPagina = request.RegistrosPorPagina < 1 ? 10 : request.RegistrosPorPagina;

            var usuariosPaginados = await query
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync(cancellationToken);

            var listaDto = new List<UsuarioResponseDto>();

            foreach (var usuario in usuariosPaginados)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                listaDto.Add(new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    Identificacion = usuario.Identificacion,
                    Nombres = usuario.Nombres,
                    Apellidos = usuario.Apellidos,
                    Email = usuario.Email ?? string.Empty,
                    DepartamentoId = usuario.DepartamentoId,
                    NombreDepartamento = usuario.Departamento != null ? usuario.Departamento.Nombre : "Sin Departamento Asignado",
                    EstadoActivo = usuario.EstadoActivo,
                    FechaCreacion = usuario.FechaCreacion,
                    Roles = roles.ToList()
                });
            }

            return ResultadoPaginadoDto<UsuarioResponseDto>.Crear(
                listaDto,
                totalRegistros,
                pagina,
                registrosPorPagina);
        }
    }
}