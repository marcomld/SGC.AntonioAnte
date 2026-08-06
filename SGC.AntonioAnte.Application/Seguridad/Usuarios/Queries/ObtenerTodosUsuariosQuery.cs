using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common;
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Usuarios.Queries
{
    // CONTRATO UNIFICADO (QUERY)
    public record ObtenerTodosUsuariosQuery : IRequest<Result<List<UsuarioResponseDto>>>;

    // LÓGICA (HANDLER)
    public class ObtenerTodosUsuariosQueryHandler : IRequestHandler<ObtenerTodosUsuariosQuery, Result<List<UsuarioResponseDto>>>
    {
        private readonly UserManager<Usuario> _userManager;

        public ObtenerTodosUsuariosQueryHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<List<UsuarioResponseDto>>> Handle(ObtenerTodosUsuariosQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 🔹 MODIFICADO: Agregamos Include(u => u.Departamento) para realizar el JOIN
                var usuariosBase = await _userManager.Users
                    .Include(u => u.Departamento) // <-- Trae los datos de la tabla relacionada
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var listaUsuariosDto = new List<UsuarioResponseDto>();

                foreach (var usuario in usuariosBase)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);

                    listaUsuariosDto.Add(new UsuarioResponseDto
                    {
                        Id = usuario.Id,
                        Identificacion = usuario.Identificacion,
                        Nombres = usuario.Nombres,
                        Apellidos = usuario.Apellidos,
                        Email = usuario.Email ?? string.Empty,

                        // 🔹 MODIFICADO: Mapeo inteligente con validación de nulos
                        DepartamentoId = usuario.DepartamentoId,
                        NombreDepartamento = usuario.Departamento != null ? usuario.Departamento.Nombre : "Sin Departamento Asignado",

                        EstadoActivo = usuario.EstadoActivo,
                        FechaCreacion = usuario.FechaCreacion,
                        Roles = roles.ToList()
                    });
                }

                return Result<List<UsuarioResponseDto>>.Success(listaUsuariosDto);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QUERY ERROR]: {ex.Message}");
                return Result<List<UsuarioResponseDto>>.Failure("Error interno al compilar la nómina en la base de datos.");
            }
        }
    }
}