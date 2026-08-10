using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Utils;
using SGC.AntonioAnte.Shared.DTOs.Common;
using SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SGC.AntonioAnte.Client.Services.Seguridad.Implementation
{
    public class UsuarioService : IUsuarioService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<ResultadoPaginadoDto<UsuarioResponseDto>?> ObtenerUsuariosPaginadosAsync(
            string? busqueda,
            bool? estadoActivo,
            Guid? departamentoId,
            int pagina = 1,
            int registrosPorPagina = 10)
        {
            try
            {
                var url = $"api/v1/seguridad/usuarios?pagina={pagina}&registrosPorPagina={registrosPorPagina}";

                if (!string.IsNullOrWhiteSpace(busqueda))
                    url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

                if (estadoActivo.HasValue)
                    url += $"&estadoActivo={estadoActivo.Value}";

                if (departamentoId.HasValue && departamentoId != Guid.Empty)
                    url += $"&departamentoId={departamentoId.Value}";

                var respuesta = await _httpClient.GetAsync(url);
                if (respuesta.IsSuccessStatusCode)
                {
                    return await respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<UsuarioResponseDto>>(_jsonOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar nómina paginada: {ex.Message}");
            }
            return null;
        }

        // Método sin paginación para modales (compatibilidad)
        public async Task<List<UsuarioResponseDto>?> ObtenerTodosLosUsuariosAsync()
        {
            var resPaginado = await ObtenerUsuariosPaginadosAsync(null, null, null, 1, 1000);
            return resPaginado?.Datos;
        }

        public async Task<OperacionResultadoDto> RegistrarFuncionarioAsync(CreateUsuarioDto nuevoUsuario)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("api/v1/seguridad/usuarios", nuevoUsuario);

                if (respuesta.IsSuccessStatusCode)
                {
                    var respuestaApi = await respuesta.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return respuestaApi ?? OperacionResultadoDto.Exito("Funcionario registrado con éxito en la BD Municipal.");
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de comunicación de red: {ex.Message}";
            }

            return resultado;
        }

        public async Task<OperacionResultadoDto> AsignarPermisoAsync(Guid id, UserPermissionDto permisoDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync($"api/v1/seguridad/usuarios/{id}/permisos", permisoDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    var respuestaApi = await respuesta.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return respuestaApi ?? OperacionResultadoDto.Exito("Permiso asignado correctamente al funcionario.");
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Fallo de red al asignar permiso: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> ActualizarFuncionarioAsync(Guid id, UpdateUsuarioDto usuarioDto)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PutAsJsonAsync($"api/v1/seguridad/usuarios/{id}", usuarioDto);

                if (respuesta.IsSuccessStatusCode)
                {
                    var respuestaApi = await respuesta.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return respuestaApi ?? OperacionResultadoDto.Exito("Datos actualizados correctamente.");
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        public async Task<OperacionResultadoDto> CambiarEstadoFuncionarioAsync(Guid id)
        {
            var resultado = new OperacionResultadoDto();
            try
            {
                var respuesta = await _httpClient.PutAsync($"api/v1/seguridad/usuarios/{id}/cambiar-estado", null);

                if (respuesta.IsSuccessStatusCode)
                {
                    var respuestaApi = await respuesta.Content.ReadFromJsonAsync<OperacionResultadoDto>(_jsonOptions);
                    return respuestaApi ?? OperacionResultadoDto.Exito("Estado del funcionario actualizado correctamente.");
                }

                var contenidoError = await respuesta.Content.ReadAsStringAsync();
                resultado.Exitoso = false;
                resultado.Mensaje = ExtraerMensajeErrorUniversal(contenidoError, respuesta.StatusCode);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error de red: {ex.Message}";
            }
            return resultado;
        }

        private string ExtraerMensajeErrorUniversal(string contenidoRaw, HttpStatusCode statusCode)
        {
            if (string.IsNullOrWhiteSpace(contenidoRaw))
                return $"El servidor rechazó la solicitud (Código HTTP: {(int)statusCode}).";

            try
            {
                using var doc = JsonDocument.Parse(contenidoRaw);
                var root = doc.RootElement;

                if (TryGetProp(root, "mensaje", out var propMensaje) && !string.IsNullOrWhiteSpace(propMensaje.GetString()))
                    return LimpiarTexto(propMensaje.GetString()!);

                if (TryGetProp(root, "detail", out var propDetail) && !string.IsNullOrWhiteSpace(propDetail.GetString()))
                    return LimpiarTexto(propDetail.GetString()!);
            }
            catch { }

            return LimpiarTexto(contenidoRaw);
        }

        private static bool TryGetProp(JsonElement element, string propName, out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, propName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = prop.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "Error de validación.";
            string limpio = texto.Trim('"').Trim();

            if (limpio.Contains("Validation failed") || limpio.Contains("--"))
            {
                if (limpio.Contains("--"))
                {
                    var partesDash = limpio.Split(new[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                    limpio = partesDash.Length > 1 ? partesDash[1] : partesDash[0];
                }
                if (limpio.Contains("Severity:", StringComparison.OrdinalIgnoreCase))
                {
                    limpio = limpio.Split(new[] { "Severity:", "severity:" }, StringSplitOptions.RemoveEmptyEntries)[0];
                }
                if (limpio.Contains(":"))
                {
                    var partesColon = limpio.Split(new[] { ':' }, 2);
                    limpio = partesColon.Length > 1 ? partesColon[1] : partesColon[0];
                }
                return limpio.Trim();
            }
            return limpio;
        }
    }
}