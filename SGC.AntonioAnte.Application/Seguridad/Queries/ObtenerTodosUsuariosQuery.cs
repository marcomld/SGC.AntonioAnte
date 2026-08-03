using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGC.AntonioAnte.Application.Common; // <-- CORRECCIÓN: Apunta al nuevo namespace de tu Result
using SGC.AntonioAnte.Domain.Seguridad.Entities;
using SGC.AntonioAnte.Shared.DTOs.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application.Seguridad.Queries
{
    // CONTRATO UNIFICADO (QUERY)
    public record ObtenerTodosUsuariosQuery : IRequest<Result<List<CreateUsuarioDto>>>;

    // LÓGICA (HANDLER)
    public class ObtenerTodosUsuariosQueryHandler : IRequestHandler<ObtenerTodosUsuariosQuery, Result<List<CreateUsuarioDto>>>
    {
        private readonly UserManager<Usuario> _userManager;

        public ObtenerTodosUsuariosQueryHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<List<CreateUsuarioDto>>> Handle(ObtenerTodosUsuariosQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var usuariosBase = await _userManager.Users
                    .Where(u => u.EstadoActivo)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var listaUsuariosDto = new List<CreateUsuarioDto>();

                foreach (var usuario in usuariosBase)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    var rolPrincipal = roles.FirstOrDefault() ?? "SinRol";

                    listaUsuariosDto.Add(new CreateUsuarioDto
                    {
                        Identificacion = usuario.Identificacion,
                        Nombres = usuario.Nombres,
                        Apellidos = usuario.Apellidos,
                        Email = usuario.Email ?? string.Empty,
                        Departamento = usuario.Departamento,
                        RolAsignado = rolPrincipal,
                        Password = "▲ ENMASCARADO POR POLÍTICA ISO 27001 ▲"
                    });
                }

                return Result<List<CreateUsuarioDto>>.Success(listaUsuariosDto);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QUERY ERROR]: {ex.Message}");
                return Result<List<CreateUsuarioDto>>.Failure("Error interno al compilar la nómina en la base de datos.");
            }
        }
    }
}