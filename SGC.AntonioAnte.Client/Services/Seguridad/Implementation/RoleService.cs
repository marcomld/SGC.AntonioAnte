using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Roles;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<RoleResponseDto>?> ObtenerTodosLosRolesAsync()
        {
            try
            {
                var respuesta = await _httpClient.GetAsync("api/v1/seguridad/roles");
                if (respuesta.IsSuccessStatusCode)
                {
                    var resultadoJson = await respuesta.Content.ReadFromJsonAsync<RespuestaApi<List<RoleResponseDto>>>(_jsonOptions);
                    return resultadoJson?.Data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar roles: {ex.Message}");
            }
            return null;
        }

        public async Task<OperacionResultadoDto> CrearRolAsync(CreateRoleDto nuevoRol)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/roles", nuevoRol);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Rol registrado con éxito.";
                    return resultado;
                }
                resultado.Exitoso = false;
                resultado.Mensaje = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> EliminarRolAsync(Guid id)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.DeleteAsync($"api/v1/seguridad/roles/{id}");
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Rol eliminado correctamente.";
                    return resultado;
                }
                resultado.Exitoso = false;
                resultado.Mensaje = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<List<PermissionDto>?> ObtenerPermisosRolAsync(Guid rolId)
        {
            try
            {
                var respuesta = await _httpClient.GetAsync($"api/v1/seguridad/roles/{rolId}/permisos");
                if (respuesta.IsSuccessStatusCode)
                {
                    var resultadoJson = await respuesta.Content.ReadFromJsonAsync<RespuestaApi<List<PermissionDto>>>(_jsonOptions);
                    return resultadoJson?.Data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar permisos del rol: {ex.Message}");
            }
            return null;
        }

        public async Task<OperacionResultadoDto> AsignarPermisosRolAsync(RolePermissionDto permissionDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/roles/permisos", permissionDto);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Permisos del rol actualizados correctamente.";
                    return resultado;
                }
                resultado.Exitoso = false;
                resultado.Mensaje = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<List<PermissionDto>?> ObtenerPermisosUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var respuesta = await _httpClient.GetAsync($"api/v1/seguridad/roles/usuarios/{usuarioId}/permisos");
                if (respuesta.IsSuccessStatusCode)
                {
                    var resultadoJson = await respuesta.Content.ReadFromJsonAsync<RespuestaApi<List<PermissionDto>>>(_jsonOptions);
                    return resultadoJson?.Data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar permisos del funcionario: {ex.Message}");
            }
            return null;
        }

        public async Task<OperacionResultadoDto> AsignarPermisosUsuarioAsync(RolePermissionDto permissionDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/roles/usuarios/permisos", permissionDto);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Permisos del funcionario actualizados correctamente.";
                    return resultado;
                }
                resultado.Exitoso = false;
                resultado.Mensaje = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> AsignarRolUsuarioAsync(AssignRoleDto assignRoleDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/roles/asignar-usuario", assignRoleDto);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Rol asignado correctamente.";
                    return resultado;
                }
                resultado.Exitoso = false;
                resultado.Mensaje = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> DesasignarRolUsuarioAsync(AssignRoleDto assignRoleDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/roles/desasignar-usuario", assignRoleDto);
                if (respuesta.IsSuccessStatusCode)
                {
                    resultado.Exitoso = true;
                    resultado.Mensaje = "Rol removido correctamente.";
                    return resultado;
                }
                resultado.Exitoso = false;
                resultado.Mensaje = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }
    }
}
