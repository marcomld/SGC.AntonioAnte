using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Contracts
{
    public interface IRoleService
    {
        // GESTIÓN DE ROLES
        Task<List<RoleResponseDto>?> ObtenerTodosLosRolesAsync();
        Task<OperacionResultadoDto> CrearRolAsync(CreateRoleDto nuevoRol);
        Task<OperacionResultadoDto> EliminarRolAsync(Guid id);

        // PERMISOS DE ROL (Roles.razor)
        Task<List<PermissionDto>?> ObtenerPermisosRolAsync(Guid rolId);
        Task<OperacionResultadoDto> AsignarPermisosRolAsync(RolePermissionDto permissionDto);

        // PERMISOS DIRECTOS DE USUARIO (Usuarios.razor)
        Task<List<PermissionDto>?> ObtenerPermisosUsuarioAsync(Guid usuarioId);
        Task<OperacionResultadoDto> AsignarPermisosUsuarioAsync(RolePermissionDto permissionDto);

        // ASIGNACIÓN DE ROLES A USUARIOS
        Task<OperacionResultadoDto> AsignarRolUsuarioAsync(AssignRoleDto assignRoleDto);
        Task<OperacionResultadoDto> DesasignarRolUsuarioAsync(AssignRoleDto assignRoleDto);
    }
}
